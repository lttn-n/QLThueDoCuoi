using Microsoft.Data.SqlClient;
using QLThueDoCuoi.Data;
using QLThueDoCuoi.ViewModels;
namespace QLThueDoCuoi.Repositories;

public sealed class SanPhamRepository(KetNoiDatabase ketNoi)
{
    const string SqlNoiBat = @"
;WITH x AS (
    SELECT s.*, l.TenLoaiSanPham,
           ROW_NUMBER() OVER (PARTITION BY s.TenSanPham ORDER BY s.MaSanPham) AS rn
    FROM dbo.SanPham s JOIN dbo.LoaiSanPham l ON l.MaLoaiSanPham = s.MaLoaiSanPham
    WHERE s.TrangThai = 1 AND s.NoiBat = 1)
SELECT TOP (@soLuong) x.MaSanPham, x.TenSanPham, x.TenLoaiSanPham, x.GiaThueGoi, x.SoNgayThueTrongGoi,
       x.GiaKhuyenMai, x.NgayBatDauGiamGia, x.NgayKetThucGiamGia, x.DuongDanAnh, x.NhanHienThi,
       (SELECT TOP 1 h.DuongDan FROM dbo.HinhAnhSanPham h WHERE h.MaSanPham = x.MaSanPham ORDER BY h.ThuTu) AS Anh2,
       STUFF((SELECT ' ' + s2.KichThuoc FROM dbo.SanPham s2
              WHERE s2.TenSanPham = x.TenSanPham AND s2.TrangThai = 1 ORDER BY s2.MaSanPham FOR XML PATH('')), 1, 1, '') AS KichThuoc
FROM x WHERE x.rn = 1 ORDER BY x.MaSanPham;";

    public async Task<List<TheSanPhamViewModel>> LayNoiBatAsync(int soLuong = 4)
    {
        var ds = new List<TheSanPhamViewModel>();
        await using var kn = ketNoi.TaoKetNoi(SqlClientFactory.Instance);
        await kn.OpenAsync();
        await using var lenh = kn.CreateCommand();
        lenh.CommandText = SqlNoiBat;
        var p = lenh.CreateParameter(); p.ParameterName = "@soLuong"; p.Value = soLuong; lenh.Parameters.Add(p);
        await using var doc = await lenh.ExecuteReaderAsync();
        var bayGio = DateTime.UtcNow;
        while (await doc.ReadAsync())
        {
            var gia = doc.GetDecimal(3);
            var giamGia = !doc.IsDBNull(5) && !doc.IsDBNull(6) && !doc.IsDBNull(7)
                          && doc.GetDateTime(6) <= bayGio && bayGio <= doc.GetDateTime(7);
            var phanTram = giamGia ? (int)Math.Round((1 - doc.GetDecimal(5) / gia) * 100) : 0;
            var nhan = phanTram > 0 ? $"-{phanTram}%" : Chuoi(doc, 9) ?? "";
            ds.Add(new TheSanPhamViewModel(doc.GetInt32(0), doc.GetString(1), doc.GetString(2),
                giamGia ? doc.GetDecimal(5) : gia, doc.GetInt32(4), Chuoi(doc, 8) ?? "", nhan,
                Chuoi(doc, 11) ?? "", giamGia ? gia : null, Chuoi(doc, 10)));
        }
        return ds;
    }

    public async Task<Dictionary<int, string>> LayAnhDaiDienTheoLoaiAsync()
    {
        var kq = new Dictionary<int, string>();
        await using var kn = ketNoi.TaoKetNoi(SqlClientFactory.Instance);
        await kn.OpenAsync();
        await using var lenh = kn.CreateCommand();
        lenh.CommandText = "SELECT MaLoaiSanPham, MIN(DuongDanAnh) FROM dbo.SanPham WHERE TrangThai = 1 AND DuongDanAnh IS NOT NULL GROUP BY MaLoaiSanPham";
        await using var doc = await lenh.ExecuteReaderAsync();
        while (await doc.ReadAsync()) kq[doc.GetInt32(0)] = doc.GetString(1);
        return kq;
    }

    const string SqlDanhSach = @"
;WITH x AS (
    SELECT s.*, l.TenLoaiSanPham,
           ROW_NUMBER() OVER (PARTITION BY s.MaLoaiSanPham, s.TenSanPham ORDER BY s.MaSanPham) AS rn
    FROM dbo.SanPham s JOIN dbo.LoaiSanPham l ON l.MaLoaiSanPham = s.MaLoaiSanPham
    WHERE s.TrangThai = 1 AND l.HienThi = 1)
SELECT x.MaSanPham, x.TenSanPham, x.TenLoaiSanPham, x.GiaThueGoi, x.SoNgayThueTrongGoi,
       x.GiaKhuyenMai, x.NgayBatDauGiamGia, x.NgayKetThucGiamGia, x.DuongDanAnh, x.NhanHienThi,
       (SELECT TOP 1 h.DuongDan FROM dbo.HinhAnhSanPham h WHERE h.MaSanPham = x.MaSanPham ORDER BY h.ThuTu) AS Anh2,
       STUFF((SELECT ' ' + s2.KichThuoc FROM dbo.SanPham s2
              WHERE s2.TenSanPham = x.TenSanPham AND s2.MaLoaiSanPham = x.MaLoaiSanPham AND s2.TrangThai = 1
              ORDER BY s2.MaSanPham FOR XML PATH('')), 1, 1, '') AS KichThuoc
FROM x WHERE x.rn = 1 ORDER BY x.MaLoaiSanPham, x.MaSanPham;";

    public async Task<DanhSachSanPhamViewModel> LayDanhSachAsync()
    {
        var ds = new List<TheSanPhamViewModel>();
        await using var kn = ketNoi.TaoKetNoi(SqlClientFactory.Instance);
        await kn.OpenAsync();
        await using var lenh = kn.CreateCommand();
        lenh.CommandText = SqlDanhSach;
        await using var doc = await lenh.ExecuteReaderAsync();
        var bayGio = DateTime.UtcNow;
        while (await doc.ReadAsync()) ds.Add(DocThe(doc, bayGio));
        var loai = ds.Select(x => x.TenLoai).Distinct().ToList();
        var size = ds.SelectMany(x => x.KichThuoc.Split(' ', StringSplitOptions.RemoveEmptyEntries)).Distinct().ToList();
        return new DanhSachSanPhamViewModel { SanPham = ds, CacLoai = loai, CacSize = size };
    }

    public async Task<ChiTietSanPhamViewModel?> LayChiTietAsync(int maSanPham)
    {
        await using var kn = ketNoi.TaoKetNoi(SqlClientFactory.Instance);
        await kn.OpenAsync();
        int maLoai; string ten, tenLoai, mau;
        string? moTa, chatLieu, anh, nhanHt; decimal gia, goiGia = 0, themNgay, coc; int soNgay;
        decimal? giaKm = null; DateTime? bd = null, kt = null;
        await using (var lenh = kn.CreateCommand())
        {
            lenh.CommandText = @"SELECT s.MaLoaiSanPham, s.TenSanPham, l.TenLoaiSanPham, s.MauSac, s.MoTa, s.ChatLieu, s.DuongDanAnh, s.NhanHienThi,
                s.GiaThueGoi, s.SoNgayThueTrongGoi, s.GiaThueMoiNgayThem, s.TienCoc, s.GiaKhuyenMai, s.NgayBatDauGiamGia, s.NgayKetThucGiamGia
                FROM dbo.SanPham s JOIN dbo.LoaiSanPham l ON l.MaLoaiSanPham = s.MaLoaiSanPham
                WHERE s.MaSanPham = @id AND s.TrangThai = 1";
            var p = lenh.CreateParameter(); p.ParameterName = "@id"; p.Value = maSanPham; lenh.Parameters.Add(p);
            await using var d = await lenh.ExecuteReaderAsync();
            if (!await d.ReadAsync()) return null;
            maLoai = d.GetInt32(0); ten = d.GetString(1); tenLoai = d.GetString(2); mau = d.GetString(3);
            moTa = Chuoi(d, 4); chatLieu = Chuoi(d, 5); anh = Chuoi(d, 6); nhanHt = Chuoi(d, 7);
            gia = d.GetDecimal(8); soNgay = d.GetInt32(9); themNgay = d.GetDecimal(10); coc = d.GetDecimal(11);
            if (!d.IsDBNull(12) && !d.IsDBNull(13) && !d.IsDBNull(14)) { giaKm = d.GetDecimal(12); bd = d.GetDateTime(13); kt = d.GetDateTime(14); }
        }
        var bayGio = DateTime.UtcNow;
        var dangGiam = giaKm is not null && bd <= bayGio && bayGio <= kt;
        var sizes = new List<LuaChonSize>();
        var phu = new List<string>();
        await using (var lenh = kn.CreateCommand())
        {
            lenh.CommandText = "SELECT MaSanPham, KichThuoc FROM dbo.SanPham WHERE MaLoaiSanPham = @loai AND TenSanPham = @ten AND TrangThai = 1 ORDER BY MaSanPham";
            var p1 = lenh.CreateParameter(); p1.ParameterName = "@loai"; p1.Value = maLoai; lenh.Parameters.Add(p1);
            var p2 = lenh.CreateParameter(); p2.ParameterName = "@ten"; p2.Value = ten; lenh.Parameters.Add(p2);
            await using var d = await lenh.ExecuteReaderAsync();
            while (await d.ReadAsync()) sizes.Add(new LuaChonSize(d.GetInt32(0), d.GetString(1)));
        }
        await using (var lenh = kn.CreateCommand())
        {
            lenh.CommandText = @"SELECT h.DuongDan FROM dbo.HinhAnhSanPham h JOIN dbo.SanPham s ON s.MaSanPham = h.MaSanPham
                WHERE s.MaLoaiSanPham = @loai AND s.TenSanPham = @ten ORDER BY h.ThuTu, h.MaHinh";
            var p1 = lenh.CreateParameter(); p1.ParameterName = "@loai"; p1.Value = maLoai; lenh.Parameters.Add(p1);
            var p2 = lenh.CreateParameter(); p2.ParameterName = "@ten"; p2.Value = ten; lenh.Parameters.Add(p2);
            await using var d = await lenh.ExecuteReaderAsync();
            while (await d.ReadAsync()) phu.Add(d.GetString(0));
        }
        var phanTram = dangGiam ? (int)Math.Round((1 - giaKm!.Value / gia) * 100) : 0;
        return new ChiTietSanPhamViewModel
        {
            MaSanPham = sizes.Count > 0 ? sizes[0].MaSanPham : maSanPham, Ten = ten, TenLoai = tenLoai, MoTa = moTa, ChatLieu = chatLieu,
            MauSac = mau, Anh = anh ?? "", AnhPhu = phu, GiaThue = dangGiam ? giaKm!.Value : gia, GiaGoc = dangGiam ? gia : null,
            SoNgayGoi = soNgay, GiaNgayThem = themNgay, TienCoc = coc, Nhan = phanTram > 0 ? $"-{phanTram}%" : nhanHt ?? "", CacSize = sizes
        };
    }

    static TheSanPhamViewModel DocThe(System.Data.Common.DbDataReader doc, DateTime bayGio)
    {
        var gia = doc.GetDecimal(3);
        var giamGia = !doc.IsDBNull(5) && !doc.IsDBNull(6) && !doc.IsDBNull(7)
                      && doc.GetDateTime(6) <= bayGio && bayGio <= doc.GetDateTime(7);
        var phanTram = giamGia ? (int)Math.Round((1 - doc.GetDecimal(5) / gia) * 100) : 0;
        var nhan = phanTram > 0 ? $"-{phanTram}%" : Chuoi(doc, 9) ?? "";
        return new TheSanPhamViewModel(doc.GetInt32(0), doc.GetString(1), doc.GetString(2),
            giamGia ? doc.GetDecimal(5) : gia, doc.GetInt32(4), Chuoi(doc, 8) ?? "", nhan,
            Chuoi(doc, 11) ?? "", giamGia ? gia : null, Chuoi(doc, 10));
    }

    static string? Chuoi(System.Data.Common.DbDataReader d, int i) => d.IsDBNull(i) ? null : d.GetString(i);

    public async Task<List<(string Ten, string KichThuoc)>> LayConHangAsync(DateOnly tu, DateOnly den)
    {
        var ds = new List<(string, string)>();
        await using var kn = ketNoi.TaoKetNoi(SqlClientFactory.Instance);
        await kn.OpenAsync();
        await using var lenh = kn.CreateCommand();
        lenh.CommandText = $@"SELECT s.TenSanPham, s.KichThuoc FROM dbo.SanPham s
WHERE s.TrangThai = 1 AND {TonKhoSql.TrongGioiHan} AND {TonKhoSql.ConLai} > 0
ORDER BY s.TenSanPham, s.MaSanPham";
        ThamNgay(lenh, tu, den);
        await using var doc = await lenh.ExecuteReaderAsync();
        while (await doc.ReadAsync()) ds.Add((doc.GetString(0), doc.GetString(1)));
        return ds;
    }

    public async Task<(int ConLai, string Ten)?> KiemTraConHangAsync(int maSanPham, string? kichThuoc, DateOnly tu, DateOnly den)
    {
        await using var kn = ketNoi.TaoKetNoi(SqlClientFactory.Instance);
        await kn.OpenAsync();
        await using var lenh = kn.CreateCommand();
        lenh.CommandText = $@"SELECT TOP 1 s.TenSanPham, CASE WHEN {TonKhoSql.TrongGioiHan} THEN {TonKhoSql.ConLai} ELSE -1 END
FROM dbo.SanPham a
JOIN dbo.SanPham s ON s.TenSanPham = a.TenSanPham AND s.MaLoaiSanPham = a.MaLoaiSanPham AND s.TrangThai = 1
WHERE a.MaSanPham = @sp AND (@kt IS NULL OR s.KichThuoc = @kt)
ORDER BY CASE WHEN s.MaSanPham = a.MaSanPham THEN 0 ELSE 1 END, s.MaSanPham";
        var p = lenh.CreateParameter(); p.ParameterName = "@sp"; p.DbType = System.Data.DbType.Int32; p.Value = maSanPham; lenh.Parameters.Add(p);
        var k = lenh.CreateParameter(); k.ParameterName = "@kt"; k.DbType = System.Data.DbType.String; k.Value = (object?)kichThuoc ?? DBNull.Value; lenh.Parameters.Add(k);
        ThamNgay(lenh, tu, den);
        await using var doc = await lenh.ExecuteReaderAsync();
        if (!await doc.ReadAsync()) return null;
        return (doc.GetInt32(1), doc.GetString(0));
    }

    static void ThamNgay(System.Data.Common.DbCommand lenh, DateOnly tu, DateOnly den)
    {
        foreach (var (ten, v) in new[] { ("@tu", tu), ("@den", den) })
        {
            var p = lenh.CreateParameter(); p.ParameterName = ten; p.DbType = System.Data.DbType.Date;
            p.Value = v.ToDateTime(TimeOnly.MinValue); lenh.Parameters.Add(p);
        }
    }
}
