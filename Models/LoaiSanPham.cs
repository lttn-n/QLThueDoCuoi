namespace QLThueDoCuoi.Models;

// Tương ứng bảng LoaiSanPham trong Database/QuanLyThueDoCuoi.sql.
public class LoaiSanPham
{
    public int MaLoaiSanPham { get; set; }
    public int? MaLoaiCha { get; set; }
    public string TenLoaiSanPham { get; set; } = string.Empty;
    public bool HienThi { get; set; }
}
