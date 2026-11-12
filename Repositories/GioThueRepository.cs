using System.Data;
using Microsoft.Data.SqlClient;
using QLThueDoCuoi.Data;
using QLThueDoCuoi.ViewModels;
namespace QLThueDoCuoi.Repositories;

public sealed class GioThueRepository(KetNoiDatabase ketNoi)
{
    public async Task<int> DemSanPhamAsync(int maTaiKhoan)
    {
        await using var kn = ketNoi.TaoKetNoi(SqlClientFactory.Instance);
        await kn.OpenAsync();
        await using var lenh = kn.CreateCommand();
        lenh.CommandText = "SELECT COALESCE(SUM(c.SoLuong),0) FROM dbo.ChiTietGioThue c JOIN dbo.GioThue g ON g.MaGioThue = c.MaGioThue WHERE g.MaTaiKhoan = @ma";
        var p = lenh.CreateParameter(); p.ParameterName = "@ma"; p.Value = maTaiKhoan; lenh.Parameters.Add(p);
        return Convert.ToInt32(await lenh.ExecuteScalarAsync());
    }

    public async Task<int> ThemNhieuAsync(int maTaiKhoan, IEnumerable<int> dsSanPham)
    {
        await using var kn = ketNoi.TaoKetNoi(SqlClientFactory.Instance);
        await kn.OpenAsync();
        await using var tran = await kn.BeginTransactionAsync();
        try
        {
            int maGio;
            await using (var lenhGio = kn.CreateCommand())
            {
                lenhGio.Transaction = tran;
                lenhGio.CommandText = "IF NOT EXISTS (SELECT 1 FROM dbo.GioThue WHERE MaTaiKhoan = @ma) INSERT dbo.GioThue(MaTaiKhoan) VALUES (@ma); SELECT MaGioThue FROM dbo.GioThue WHERE MaTaiKhoan = @ma;";
                ThamSo(lenhGio, "@ma", maTaiKhoan);
                maGio = Convert.ToInt32(await lenhGio.ExecuteScalarAsync());
            }

            var them = 0;
            foreach (var maSanPham in dsSanPham.Distinct())
            {
                await using var lenh = kn.CreateCommand();
                lenh.Transaction = tran;
                lenh.CommandText = @"IF EXISTS (SELECT 1 FROM dbo.SanPham WHERE MaSanPham = @sp AND TrangThai = 1)
   AND NOT EXISTS (SELECT 1 FROM dbo.ChiTietGioThue WHERE MaGioThue = @gio AND MaSanPham = @sp)
BEGIN
    INSERT dbo.ChiTietGioThue(MaGioThue, MaSanPham, SoLuong) VALUES (@gio, @sp, 1);
    SELECT 1;
END
ELSE SELECT 0;";
                ThamSo(lenh, "@gio", maGio);
                ThamSo(lenh, "@sp", maSanPham);
                them += Convert.ToInt32(await lenh.ExecuteScalarAsync());
            }
            await tran.CommitAsync();
            return them;
        }
        catch
        {
            await tran.RollbackAsync();
            throw;
        }
    }

    public async Task<List<DongGioViewModel>> LayGioAsync(int maTaiKhoan)
    {
        var ds = new List<DongGioViewModel>();
        await using var kn = ketNoi.TaoKetNoi(SqlClientFactory.Instance);
        await kn.OpenAsync();
        await using (var lenh = kn.CreateCommand())
        {
            lenh.CommandText = @"SELECT c.MaChiTietGioThue, s.MaSanPham, s.TenSanPham, s.KichThuoc, s.MauSac, s.DuongDanAnh,
       c.SoLuong, c.NgayNhan, c.NgayTraDuKien,
       CASE WHEN s.GiaKhuyenMai IS NOT NULL AND SYSUTCDATETIME() BETWEEN s.NgayBatDauGiamGia AND s.NgayKetThucGiamGia
            THEN s.GiaKhuyenMai ELSE s.GiaThueGoi END,
       s.SoNgayThueTrongGoi, s.GiaThueMoiNgayThem, s.TienCoc, s.TrangThai
FROM dbo.ChiTietGioThue c
JOIN dbo.GioThue g ON g.MaGioThue = c.MaGioThue
JOIN dbo.SanPham s ON s.MaSanPham = c.MaSanPham
WHERE g.MaTaiKhoan = @ma
ORDER BY c.MaChiTietGioThue";
            ThamSo(lenh, "@ma", maTaiKhoan, DbType.Int32);
            await using var doc = await lenh.ExecuteReaderAsync();
            while (await doc.ReadAsync())
                ds.Add(new DongGioViewModel
                {
                    MaChiTiet = doc.GetInt32(0), MaSanPham = doc.GetInt32(1), Ten = doc.GetString(2),
                    KichThuoc = doc.GetString(3), MauSac = doc.GetString(4),
                    Anh = doc.IsDBNull(5) ? null : doc.GetString(5),
                    SoLuong = doc.GetInt32(6),
                    NgayNhan = doc.IsDBNull(7) ? null : DateOnly.FromDateTime(doc.GetDateTime(7)),
                    NgayTra = doc.IsDBNull(8) ? null : DateOnly.FromDateTime(doc.GetDateTime(8)),
                    GiaGoi = doc.GetDecimal(9), SoNgayGoi = doc.GetInt32(10), GiaNgayThem = doc.GetDecimal(11),
                    TienCoc = doc.GetDecimal(12), NgungThue = !doc.GetBoolean(13)
                });
        }
        if (ds.Count == 0) return ds;

        var size = new Dictionary<int, List<LuaChonSize>>();
        await using (var lenh = kn.CreateCommand())
        {
            lenh.CommandText = @"SELECT DISTINCT a.MaSanPham, b.MaSanPham, b.KichThuoc
FROM dbo.SanPham a
JOIN dbo.SanPham b ON b.TenSanPham = a.TenSanPham AND b.MaLoaiSanPham = a.MaLoaiSanPham AND b.MauSac = a.MauSac AND b.TrangThai = 1
WHERE a.MaSanPham IN (SELECT c.MaSanPham FROM dbo.ChiTietGioThue c JOIN dbo.GioThue g ON g.MaGioThue = c.MaGioThue WHERE g.MaTaiKhoan = @ma)
ORDER BY b.MaSanPham";
            ThamSo(lenh, "@ma", maTaiKhoan, DbType.Int32);
            await using var doc = await lenh.ExecuteReaderAsync();
            while (await doc.ReadAsync())
            {
                var goc = doc.GetInt32(0);
                if (!size.TryGetValue(goc, out var l)) size[goc] = l = new List<LuaChonSize>();
                l.Add(new LuaChonSize(doc.GetInt32(1), doc.GetString(2)));
            }
        }
        foreach (var d in ds)
        {
            d.CacSize = size.TryGetValue(d.MaSanPham, out var l) ? l : new List<LuaChonSize>();
            if (d.CacSize.All(x => x.MaSanPham != d.MaSanPham)) d.CacSize.Add(new LuaChonSize(d.MaSanPham, d.KichThuoc));
        }
        return ds;
    }

    public async Task<(bool ThanhCong, string? Loi, string? Ten)> ThemAsync(int maTaiKhoan, int maSanPham, string? kichThuoc,
        string? mauSac, int soLuong, DateOnly? ngayNhan, DateOnly? ngayTra)
    {
        await using var kn = ketNoi.TaoKetNoi(SqlClientFactory.Instance);
        await kn.OpenAsync();
        await using var tran = await kn.BeginTransactionAsync();
        try
        {
            int maThat; string ten;
            await using (var lenh = kn.CreateCommand())
            {
                lenh.Transaction = tran;
                lenh.CommandText = @"SELECT TOP 1 b.MaSanPham, b.TenSanPham
FROM dbo.SanPham a
JOIN dbo.SanPham b ON b.TenSanPham = a.TenSanPham AND b.MaLoaiSanPham = a.MaLoaiSanPham AND b.TrangThai = 1
WHERE a.MaSanPham = @sp AND (@kt IS NULL OR b.KichThuoc = @kt)
ORDER BY CASE WHEN b.MauSac = @mau THEN 0 ELSE 1 END, CASE WHEN b.MaSanPham = a.MaSanPham THEN 0 ELSE 1 END, b.MaSanPham";
                ThamSo(lenh, "@sp", maSanPham, DbType.Int32);
                ThamSo(lenh, "@kt", kichThuoc, DbType.String);
                ThamSo(lenh, "@mau", mauSac, DbType.String);
                await using var doc = await lenh.ExecuteReaderAsync();
                if (!await doc.ReadAsync()) { await doc.CloseAsync(); await tran.RollbackAsync(); return (false, "Mẫu hoặc kích thước này hiện không còn cho thuê.", null); }
                maThat = doc.GetInt32(0); ten = doc.GetString(1);
            }

            if (ngayNhan is not null && ngayTra is not null)
            {
                await using var lenhTon = kn.CreateCommand();
                lenhTon.Transaction = tran;
                lenhTon.CommandText = $"SELECT CASE WHEN {TonKhoSql.TrongGioiHan} THEN {TonKhoSql.ConLai} ELSE -1 END FROM dbo.SanPham s WHERE s.MaSanPham = @sp";
                ThamSo(lenhTon, "@sp", maThat, DbType.Int32);
                ThamSo(lenhTon, "@tu", ngayNhan.Value.ToDateTime(TimeOnly.MinValue), DbType.Date);
                ThamSo(lenhTon, "@den", ngayTra.Value.ToDateTime(TimeOnly.MinValue), DbType.Date);
                var conLai = Convert.ToInt32(await lenhTon.ExecuteScalarAsync());
                if (conLai < soLuong)
                {
                    await tran.RollbackAsync();
                    return (false, conLai < 0 ? "Thời gian thuê vượt mức tối đa của mẫu này."
                        : conLai == 0 ? "Trang phục này đã có người thuê trong khoảng ngày bạn chọn. Vui lòng chọn ngày khác hoặc size khác."
                        : $"Trong khoảng ngày này chỉ còn {conLai} bộ. Vui lòng giảm số lượng hoặc chọn ngày khác.", ten);
                }
            }

            int maGio;
            await using (var lenh = kn.CreateCommand())
            {
                lenh.Transaction = tran;
                lenh.CommandText = "IF NOT EXISTS (SELECT 1 FROM dbo.GioThue WHERE MaTaiKhoan = @ma) INSERT dbo.GioThue(MaTaiKhoan) VALUES (@ma); SELECT MaGioThue FROM dbo.GioThue WHERE MaTaiKhoan = @ma;";
                ThamSo(lenh, "@ma", maTaiKhoan, DbType.Int32);
                maGio = Convert.ToInt32(await lenh.ExecuteScalarAsync());
            }

            int daCong;
            await using (var lenh = kn.CreateCommand())
            {
                lenh.Transaction = tran;
                lenh.CommandText = @"UPDATE dbo.ChiTietGioThue SET SoLuong = CASE WHEN SoLuong + @sl > 10 THEN 10 ELSE SoLuong + @sl END
WHERE MaGioThue = @gio AND MaSanPham = @sp
  AND (NgayNhan = @n OR (NgayNhan IS NULL AND @n IS NULL))
  AND (NgayTraDuKien = @t OR (NgayTraDuKien IS NULL AND @t IS NULL))";
                ThamSo(lenh, "@sl", soLuong, DbType.Int32);
                ThamSo(lenh, "@gio", maGio, DbType.Int32);
                ThamSo(lenh, "@sp", maThat, DbType.Int32);
                ThamSo(lenh, "@n", ngayNhan?.ToDateTime(TimeOnly.MinValue), DbType.Date);
                ThamSo(lenh, "@t", ngayTra?.ToDateTime(TimeOnly.MinValue), DbType.Date);
                daCong = await lenh.ExecuteNonQueryAsync();
            }
            if (daCong == 0)
            {
                await using var lenh = kn.CreateCommand();
                lenh.Transaction = tran;
                lenh.CommandText = "INSERT dbo.ChiTietGioThue(MaGioThue, MaSanPham, SoLuong, NgayNhan, NgayTraDuKien) VALUES (@gio, @sp, @sl, @n, @t)";
                ThamSo(lenh, "@gio", maGio, DbType.Int32);
                ThamSo(lenh, "@sp", maThat, DbType.Int32);
                ThamSo(lenh, "@sl", soLuong, DbType.Int32);
                ThamSo(lenh, "@n", ngayNhan?.ToDateTime(TimeOnly.MinValue), DbType.Date);
                ThamSo(lenh, "@t", ngayTra?.ToDateTime(TimeOnly.MinValue), DbType.Date);
                await lenh.ExecuteNonQueryAsync();
            }
            await tran.CommitAsync();
            return (true, null, ten);
        }
        catch
        {
            await tran.RollbackAsync();
            throw;
        }
    }

    public async Task<string?> CapNhatAsync(int maTaiKhoan, IReadOnlyList<DongGioInput> dong)
    {
        await using var kn = ketNoi.TaoKetNoi(SqlClientFactory.Instance);
        await kn.OpenAsync();
        await using var tran = await kn.BeginTransactionAsync();
        try
        {
            foreach (var d in dong)
            {
                await using var lenh = kn.CreateCommand();
                lenh.Transaction = tran;
                lenh.CommandText = @"UPDATE c SET c.MaSanPham = @sp, c.SoLuong = @sl, c.NgayNhan = @n, c.NgayTraDuKien = @t
FROM dbo.ChiTietGioThue c
JOIN dbo.GioThue g ON g.MaGioThue = c.MaGioThue
JOIN dbo.SanPham cu ON cu.MaSanPham = c.MaSanPham
JOIN dbo.SanPham moi ON moi.MaSanPham = @sp AND moi.TenSanPham = cu.TenSanPham AND moi.MaLoaiSanPham = cu.MaLoaiSanPham
                    AND moi.MauSac = cu.MauSac AND moi.TrangThai = 1
WHERE c.MaChiTietGioThue = @ct AND g.MaTaiKhoan = @ma";
                ThamSo(lenh, "@sp", d.MaSanPham, DbType.Int32);
                ThamSo(lenh, "@sl", d.SoLuong, DbType.Int32);
                ThamSo(lenh, "@n", d.NgayNhan?.ToDateTime(TimeOnly.MinValue), DbType.Date);
                ThamSo(lenh, "@t", d.NgayTra?.ToDateTime(TimeOnly.MinValue), DbType.Date);
                ThamSo(lenh, "@ct", d.MaChiTiet, DbType.Int32);
                ThamSo(lenh, "@ma", maTaiKhoan, DbType.Int32);
                if (await lenh.ExecuteNonQueryAsync() == 0)
                {
                    await tran.RollbackAsync();
                    return "Có sản phẩm không còn hợp lệ (đã ngừng cho thuê hoặc không thuộc giỏ của bạn). Vui lòng tải lại trang.";
                }
            }
            await tran.CommitAsync();
            return null;
        }
        catch (SqlException ex) when (ex.Number is 2601 or 2627)
        {
            await tran.RollbackAsync();
            return "Có hai dòng trùng sản phẩm, size và ngày thuê. Hãy đổi một trong hai dòng hoặc xóa bớt rồi lưu lại.";
        }
        catch
        {
            await tran.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> XoaAsync(int maTaiKhoan, int maChiTiet)
    {
        await using var kn = ketNoi.TaoKetNoi(SqlClientFactory.Instance);
        await kn.OpenAsync();
        await using var lenh = kn.CreateCommand();
        lenh.CommandText = "DELETE c FROM dbo.ChiTietGioThue c JOIN dbo.GioThue g ON g.MaGioThue = c.MaGioThue WHERE c.MaChiTietGioThue = @ct AND g.MaTaiKhoan = @ma";
        ThamSo(lenh, "@ct", maChiTiet, DbType.Int32);
        ThamSo(lenh, "@ma", maTaiKhoan, DbType.Int32);
        return await lenh.ExecuteNonQueryAsync() > 0;
    }

    public async Task XoaTatCaAsync(int maTaiKhoan)
    {
        await using var kn = ketNoi.TaoKetNoi(SqlClientFactory.Instance);
        await kn.OpenAsync();
        await using var lenh = kn.CreateCommand();
        lenh.CommandText = "DELETE c FROM dbo.ChiTietGioThue c JOIN dbo.GioThue g ON g.MaGioThue = c.MaGioThue WHERE g.MaTaiKhoan = @ma";
        ThamSo(lenh, "@ma", maTaiKhoan, DbType.Int32);
        await lenh.ExecuteNonQueryAsync();
    }

    public async Task<int> XoaNhieuAsync(int maTaiKhoan, IReadOnlyCollection<int> dsMaChiTiet)
    {
        if (dsMaChiTiet.Count == 0) return 0;
        await using var kn = ketNoi.TaoKetNoi(SqlClientFactory.Instance);
        await kn.OpenAsync();
        await using var lenh = kn.CreateCommand();
        var tenThamSo = new List<string>();
        var i = 0;
        foreach (var ma in dsMaChiTiet.Distinct())
        {
            var ten = "@ct" + i++;
            tenThamSo.Add(ten);
            ThamSo(lenh, ten, ma, DbType.Int32);
        }
        lenh.CommandText = "DELETE c FROM dbo.ChiTietGioThue c JOIN dbo.GioThue g ON g.MaGioThue = c.MaGioThue WHERE g.MaTaiKhoan = @ma AND c.MaChiTietGioThue IN (" + string.Join(",", tenThamSo) + ")";
        ThamSo(lenh, "@ma", maTaiKhoan, DbType.Int32);
        return await lenh.ExecuteNonQueryAsync();
    }

    public async Task<Dictionary<int, int>> LaySoNgayGoiAsync(IEnumerable<int> dsMaSanPham)
    {
        var kq = new Dictionary<int, int>();
        var ds = dsMaSanPham.Distinct().ToList();
        if (ds.Count == 0) return kq;
        await using var kn = ketNoi.TaoKetNoi(SqlClientFactory.Instance);
        await kn.OpenAsync();
        await using var lenh = kn.CreateCommand();
        var tenThamSo = new List<string>();
        for (var i = 0; i < ds.Count; i++) { tenThamSo.Add("@sp" + i); ThamSo(lenh, "@sp" + i, ds[i], DbType.Int32); }
        lenh.CommandText = "SELECT MaSanPham, SoNgayThueTrongGoi FROM dbo.SanPham WHERE MaSanPham IN (" + string.Join(",", tenThamSo) + ")";
        await using var doc = await lenh.ExecuteReaderAsync();
        while (await doc.ReadAsync()) kq[doc.GetInt32(0)] = doc.GetInt32(1);
        return kq;
    }

    static void ThamSo(System.Data.Common.DbCommand lenh, string ten, object giaTri)
    {
        var p = lenh.CreateParameter(); p.ParameterName = ten; p.Value = giaTri; lenh.Parameters.Add(p);
    }

    static void ThamSo(System.Data.Common.DbCommand lenh, string ten, object? giaTri, DbType kieu)
    {
        var p = lenh.CreateParameter(); p.ParameterName = ten; p.DbType = kieu; p.Value = giaTri ?? DBNull.Value; lenh.Parameters.Add(p);
    }
}
