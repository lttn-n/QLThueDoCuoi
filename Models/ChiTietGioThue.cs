namespace QLThueDoCuoi.Models;

// Tương ứng bảng ChiTietGioThue trong Database/QuanLyThueDoCuoi.sql.
public class ChiTietGioThue
{
    public int MaChiTietGioThue { get; set; }
    public int MaGioThue { get; set; }
    public int MaSanPham { get; set; }
    public int SoLuong { get; set; }
    public DateTime? NgayNhan { get; set; }
    public DateTime? NgayTraDuKien { get; set; }
}
