namespace QLThueDoCuoi.Models;

// Tương ứng bảng BoTrangPhucTrongDon trong Database/QuanLyThueDoCuoi.sql.
public class BoTrangPhucTrongDon
{
    public int MaChiTietDonThue { get; set; }
    public int MaBoTrangPhuc { get; set; }
    public int MaSanPham { get; set; }
}
