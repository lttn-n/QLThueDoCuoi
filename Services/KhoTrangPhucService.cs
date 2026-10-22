#nullable enable
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace QLThueDoCuoi.Services;

public sealed record BoConTrong(int MaBoTrangPhuc, string MaQuanLy, int MaChiNhanh);
public sealed record KetQuaTon(int TongBoQuanLy, IReadOnlyList<BoConTrong> BoConTrong)
{
    public int SoLuongCon => BoConTrong.Count;
    public bool DuSoLuong(int canThue) => canThue > 0 && SoLuongCon >= canThue;
}
public sealed class HetTrangPhucException : InvalidOperationException
{
    public int MaSanPham { get; }
    public int SoLuongCon { get; }
    public HetTrangPhucException(int maSanPham, int con, int can)
        : base($"Sản phẩm {maSanPham} chỉ còn {con} bộ phù hợp trong khoảng ngày đã chọn; cần {can} bộ.")
        => (MaSanPham, SoLuongCon) = (maSanPham, con);
}

// Dùng ASP.NET Core .NET 6+; không phụ thuộc trực tiếp EF hay SqlClient.
// Truyền factory tạo SqlConnection kết nối QuanLyThueDoCuoi.
// SQL Server docs: https://learn.microsoft.com/sql/relational-databases/system-stored-procedures/sp-getapplock-transact-sql
// Mọi ghi kho/phân bộ/đổi ngày/giao/trả phải dùng cùng khóa; SQL trực tiếp có thể bỏ qua cơ chế.
public sealed class KhoTrangPhucService
{
    private readonly Func<DbConnection> _taoKetNoi;
    public const string TenKhoaKho = "QuanLyThueDoCuoi:Kho";
    public KhoTrangPhucService(Func<DbConnection> taoKetNoi) => _taoKetNoi = taoKetNoi;

    // Xem trước: không giữ đồ. Kết quả có thể thay đổi trước lúc khách bấm xác nhận.
    // maChiNhanh=null: toàn kho. Khi nhận tại cửa hàng phải truyền đúng chi nhánh.
    public async Task<KetQuaTon> KiemTraConDoAsync(int maSanPham, DateTime ngayNhan,
        DateTime ngayTra, int? maChiNhanh = null, CancellationToken ct = default)
    {
        KiemTraNgay(ngayNhan, ngayTra);
        await using var c = _taoKetNoi();
        await c.OpenAsync(ct);
        int veSinh = await DocSoNgayVeSinhAsync(c, null, maSanPham, ct);
        var bo = await TimBoAsync(c, null, maSanPham, ngayNhan.Date, ngayTra.Date,
            veSinh, maChiNhanh, null, ct);
        using var cmd = Lenh(c, null, @"
SELECT COUNT(*) FROM dbo.BoTrangPhuc b
WHERE b.MaSanPham=@sp AND b.TinhTrang NOT IN ('MAT','NGUNG_THUE')
AND (@cn IS NULL OR b.MaChiNhanh=@cn);", ("@sp", maSanPham), ("@cn", maChiNhanh));
        int tong = Convert.ToInt32(await cmd.ExecuteScalarAsync(ct));
        return new KetQuaTon(tong, bo);
    }

    // Callback phải TẠO DonThue + toàn bộ ChiTietDonThue trong c,t truyền vào,
    // không tự commit, không mở connection riêng. Trả đủ mã của mọi dòng vừa tạo.
    // Quyền tài khoản, giá, voucher, chống bấm lặp được ứng dụng kiểm tra trong callback.
    // Exception (gồm thiếu bộ) rollback TOÀN BỘ đơn, không chỉ rollback phần phân bộ.
    public Task<IReadOnlyList<int>> DatThueAsync(
        Func<DbConnection, DbTransaction, CancellationToken, Task<IReadOnlyList<int>>> taoDon,
        CancellationToken ct = default)
        => ThucHienTrongKhoaKhoAsync<IReadOnlyList<int>>(async (c, t, token) =>
        {
            var ids = await taoDon(c, t, token);
            if (ids.Count == 0 || ids.Distinct().Count() != ids.Count)
                throw new InvalidOperationException("Phải trả danh sách mã chi tiết đơn không rỗng, không trùng.");
            // Không được bỏ sót dòng hoặc ghép mã dòng của nhiều đơn vào một lần đặt.
            var toanBo = new List<int>();
            using (var cmd = Lenh(c, t, @"
SELECT d.MaChiTietDonThue FROM dbo.ChiTietDonThue d
JOIN dbo.DonThue o ON o.MaDonThue=d.MaDonThue
WHERE d.MaDonThue=(SELECT MaDonThue FROM dbo.ChiTietDonThue WHERE MaChiTietDonThue=@id)
AND o.TrangThai='CHO_XAC_NHAN';", ("@id", ids[0])))
            using (var r = await cmd.ExecuteReaderAsync(token))
                while (await r.ReadAsync(token)) toanBo.Add(r.GetInt32(0));
            if (toanBo.Count != ids.Count || !toanBo.ToHashSet().SetEquals(ids))
                throw new InvalidOperationException("Phải phân bộ cho toàn bộ dòng của cùng một đơn mới.");
            foreach (int id in ids.OrderBy(x => x))
                await PhanBoDongAsync(c, t, id, token);
            return ids;
        }, ct);

    // Dùng chung wrapper cho sửa ngày/hủy/giao/trả/sửa tình trạng/chuyển kho.
    // Wrapper chỉ cấp khóa + commit/rollback; callback vẫn phải kiểm tra quyền/trạng thái/lịch.
    // Hạn chế phạm vi callback, không chờ người dùng/HTTP/upload trong transaction.
    public async Task<T> ThucHienTrongKhoaKhoAsync<T>(
        Func<DbConnection, DbTransaction, CancellationToken, Task<T>> xuLy,
        CancellationToken ct = default)
    {
        await using var c = _taoKetNoi();
        await c.OpenAsync(ct);
        await using var t = await c.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);
        try
        {
            using (var cmd = Lenh(c, t, @"
SET XACT_ABORT ON;
DECLARE @kq int;
EXEC @kq=sys.sp_getapplock @Resource=@khoa,@LockMode='Exclusive',
    @LockOwner='Transaction',@DbPrincipal='public',@LockTimeout=10000;
IF @kq<0 THROW 51000,N'Kho đang bận. Chưa đặt được đồ, vui lòng thử lại.',1;",
                ("@khoa", TenKhoaKho)))
                await cmd.ExecuteNonQueryAsync(ct);
            T ketQua = await xuLy(c, t, ct);
            await t.CommitAsync(ct); // khóa Transaction tự được giải phóng
            return ketQua;
        }
        catch
        {
            try { await t.RollbackAsync(CancellationToken.None); } catch { /* giữ lỗi gốc */ }
            throw;
        }
    }

    // Chỉ gọi trong wrapper có khóa trên, cho dòng GIU_CHO chưa giao.
    // Sau đổi ngày/số lượng của dòng GIU_CHO: gọi lại hàm này TRƯỚC commit để phân lại bộ.
    // Nếu thất bại phải để exception đi ra wrapper, KHÔNG nuốt lỗi rồi commit.
    public async Task PhanBoDongAsync(DbConnection c, DbTransaction t,
        int maChiTiet, CancellationToken ct = default)
    {
        using (var guard = Lenh(c, t,
            "SELECT APPLOCK_MODE('public',@khoa,'Transaction');", ("@khoa", TenKhoaKho)))
            if (Convert.ToString(await guard.ExecuteScalarAsync(ct)) != "Exclusive")
                throw new InvalidOperationException("Chưa lấy khóa kho trong transaction.");
        int sp, soLuong, veSinh; int? cn;
        DateTime nhan, tra;
        using (var cmd = Lenh(c, t, @"
SELECT d.MaSanPham,d.SoLuong,d.NgayNhan,d.NgayTraDuKien,d.SoNgayVeSinhKhiThue,
       CASE WHEN o.HinhThucNhan='CUAHANG' THEN o.MaChiNhanhNhan END
FROM dbo.ChiTietDonThue d JOIN dbo.DonThue o ON o.MaDonThue=d.MaDonThue
WHERE d.MaChiTietDonThue=@id AND d.TrangThai='GIU_CHO'
AND o.TrangThai IN ('CHO_XAC_NHAN','DA_XAC_NHAN');", ("@id", maChiTiet)))
        using (var r = await cmd.ExecuteReaderAsync(ct))
        {
            if (!await r.ReadAsync(ct)) throw new InvalidOperationException("Dòng thuê không còn được phép phân bộ.");
            sp = r.GetInt32(0); soLuong = r.GetInt32(1); nhan = r.GetDateTime(2);
            tra = r.GetDateTime(3); veSinh = r.GetInt32(4); cn = r.IsDBNull(5) ? null : r.GetInt32(5);
        }
        KiemTraNgay(nhan, tra);
        // Thời điểm hôm nay lấy từ SQL Server (cùng múi giờ VN với truy vấn kho).
        using (var cmd = Lenh(c, t, "SELECT CONVERT(date,DATEADD(hour,7,SYSUTCDATETIME()));"))
            if (nhan.Date < Convert.ToDateTime(await cmd.ExecuteScalarAsync(ct)).Date)
                throw new InvalidOperationException("Không được đặt ngày nhận trong quá khứ.");
        if (soLuong <= 0) throw new InvalidOperationException("Số lượng phải lớn hơn 0.");
        var con = await TimBoAsync(c, t, sp, nhan, tra, veSinh, cn, maChiTiet, ct);
        if (con.Count < soLuong) throw new HetTrangPhucException(sp, con.Count, soLuong);
        // Xóa phân bộ cũ chỉ SAU kiểm tra đủ; toàn bộ vẫn được rollback nếu có lỗi.
        using (var cmd = Lenh(c, t, "DELETE dbo.BoTrangPhucTrongDon WHERE MaChiTietDonThue=@id;", ("@id", maChiTiet)))
            await cmd.ExecuteNonQueryAsync(ct);
        foreach (var bo in con.Take(soLuong))
        {
            using var cmd = Lenh(c, t, @"
INSERT dbo.BoTrangPhucTrongDon(MaChiTietDonThue,MaBoTrangPhuc,MaSanPham)
VALUES(@id,@bo,@sp);", ("@id", maChiTiet), ("@bo", bo.MaBoTrangPhuc), ("@sp", sp));
            await cmd.ExecuteNonQueryAsync(ct);
        }
    }

    private static async Task<int> DocSoNgayVeSinhAsync(DbConnection c, DbTransaction? t,
        int sp, CancellationToken ct)
    {
        using var cmd = Lenh(c, t, "SELECT SoNgayVeSinh FROM dbo.SanPham WHERE MaSanPham=@sp AND TrangThai=1;", ("@sp", sp));
        object? v = await cmd.ExecuteScalarAsync(ct);
        if (v is null || v is DBNull) throw new InvalidOperationException("Sản phẩm không tồn tại hoặc đã ngừng cho thuê.");
        return Convert.ToInt32(v);
    }

    // Bộ hợp lệ phải TRỐNG TRỌN khoảng [X,Y+vệ sinh). Biên kết thúc bằng ngày nhận mới được phép.
    // DA_GIAO chưa quá hạn: dùng ngày dự kiến. Quá hạn chưa trả: chặn tới khi xử lý thực tế.
    // DANG_THUE chỉ cho đặt tương lai khi có bản ghi DA_GIAO chứng minh lịch đang giữ bộ.
    private static async Task<IReadOnlyList<BoConTrong>> TimBoAsync(DbConnection c, DbTransaction? t,
        int sp, DateTime nhan, DateTime tra, int veSinh, int? cn, int? boQuaDong, CancellationToken ct)
    {
        if (veSinh < 0 || veSinh > 365 || tra.Date > new DateTime(9998,12,31))
            throw new ArgumentOutOfRangeException(nameof(veSinh), "Ngày hoặc thời gian vệ sinh vượt phạm vi hỗ trợ.");
        using var cmd = Lenh(c, t, SqlTimBo,
            ("@sp", sp), ("@x", nhan.Date), ("@y", tra.Date), ("@vs", veSinh),
            ("@cn", cn), ("@boQua", boQuaDong));
        var result = new List<BoConTrong>();
        using var r = await cmd.ExecuteReaderAsync(ct);
        while (await r.ReadAsync(ct)) result.Add(new BoConTrong(r.GetInt32(0), r.GetString(1), r.GetInt32(2)));
        return result;
    }

    internal const string SqlTimBo = @"
DECLARE @homNay date=CONVERT(date,DATEADD(hour,7,SYSUTCDATETIME()));
SELECT b.MaBoTrangPhuc,b.MaQuanLy,b.MaChiNhanh
FROM dbo.BoTrangPhuc b
JOIN dbo.SanPham s ON s.MaSanPham=b.MaSanPham AND s.TrangThai=1
JOIN dbo.ChiNhanh cn ON cn.MaChiNhanh=b.MaChiNhanh AND cn.TrangThai=1
WHERE b.MaSanPham=@sp AND (@cn IS NULL OR b.MaChiNhanh=@cn)
AND (b.TinhTrang='SAN_SANG' OR (b.TinhTrang='DANG_THUE' AND EXISTS (
    SELECT 1 FROM dbo.BoTrangPhucTrongDon a JOIN dbo.ChiTietDonThue d
    ON d.MaChiTietDonThue=a.MaChiTietDonThue
    WHERE a.MaBoTrangPhuc=b.MaBoTrangPhuc AND d.TrangThai='DA_GIAO')))
AND NOT EXISTS (
    SELECT 1 FROM dbo.BoTrangPhucTrongDon a
    JOIN dbo.ChiTietDonThue d ON d.MaChiTietDonThue=a.MaChiTietDonThue
    JOIN dbo.DonThue o ON o.MaDonThue=d.MaDonThue
    CROSS APPLY (SELECT CASE
        WHEN d.TrangThai='DA_TRA' THEN DATEADD(day,d.SoNgayVeSinhKhiThue,
             CONVERT(date,DATEADD(hour,7,d.ThoiDiemTra)))
        WHEN d.TrangThai='DA_GIAO' AND d.NgayTraDuKien<=@homNay THEN CONVERT(date,'99991231')
        ELSE DATEADD(day,d.SoNgayVeSinhKhiThue,d.NgayTraDuKien) END AS NgaySanSang) k
    WHERE a.MaBoTrangPhuc=b.MaBoTrangPhuc
      AND (@boQua IS NULL OR d.MaChiTietDonThue<>@boQua)
      AND d.TrangThai IN ('GIU_CHO','DA_GIAO','DA_TRA')
      AND (d.TrangThai IN ('DA_GIAO','DA_TRA') OR o.TrangThai<>'DA_HUY')
      AND d.NgayNhan<DATEADD(day,@vs,@y) AND @x<k.NgaySanSang
)
ORDER BY b.MaBoTrangPhuc;";

    private static void KiemTraNgay(DateTime x, DateTime y)
    {
        if (x.Date < new DateTime(2000,1,1) || y.Date <= x.Date || y.Date > new DateTime(9998,12,31))
            throw new ArgumentException("Ngày nhận/trả không hợp lệ: ngày trả phải sau ngày nhận.");
    }
    private static DbCommand Lenh(DbConnection c, DbTransaction? t, string sql,
        params (string Ten, object? GiaTri)[] args)
    {
        var cmd = c.CreateCommand(); cmd.Transaction = t; cmd.CommandText = sql; cmd.CommandTimeout = 30;
        foreach (var (ten, value) in args)
        {
            var p = cmd.CreateParameter(); p.ParameterName = ten; p.Value = value ?? DBNull.Value;
            // Null phải có kiểu để SqlClient không đoán thành NVARCHAR.
            if (value is null || value is int) p.DbType = DbType.Int32;
            else if (value is DateTime) p.DbType = DbType.Date;
            else if (value is string) p.DbType = DbType.String;
            cmd.Parameters.Add(p);
        }
        return cmd;
    }
}
