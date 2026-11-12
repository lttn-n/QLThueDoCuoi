namespace QLThueDoCuoi.Repositories;

internal static class TonKhoSql
{
    public const string ConLai = @"(
        (SELECT COUNT(*) FROM dbo.BoTrangPhuc b WHERE b.MaSanPham = s.MaSanPham AND b.TinhTrang NOT IN ('MAT','NGUNG_THUE','HU_HONG','DANG_SUA'))
      - ISNULL((SELECT SUM(c.SoLuong) FROM dbo.ChiTietDonThue c JOIN dbo.DonThue d ON d.MaDonThue = c.MaDonThue
                WHERE c.MaSanPham = s.MaSanPham AND c.TrangThai IN ('GIU_CHO','DA_GIAO') AND d.TrangThai <> 'DA_HUY'
                  AND c.NgayNhan <= DATEADD(DAY, s.SoNgayVeSinh, @den)
                  AND DATEADD(DAY, c.SoNgayVeSinhKhiThue, c.NgayTraDuKien) >= @tu), 0))";

    public static readonly string TrongGioiHan =
    $"DATEDIFF(DAY, @tu, @den) <= s.SoNgayThueTrongGoi + {Services.GioThueService.SoNgayThemToiDa}";
}
