namespace QLThueDoCuoi.Models;

// Tương ứng bảng GioThue trong Database/QuanLyThueDoCuoi.sql.
public class GioThue
{
    public int MaGioThue { get; set; }
    public int MaTaiKhoan { get; set; }
    public int? MaKhuyenMai { get; set; }
    public DateTime NgayTao { get; set; }
}
