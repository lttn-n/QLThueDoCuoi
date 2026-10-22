namespace QLThueDoCuoi.Models;

// Tương ứng bảng TaiKhoan trong Database/QuanLyThueDoCuoi.sql.
public class TaiKhoan
{
    public int MaTaiKhoan { get; set; }
    public string HoTen { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? SoDienThoai { get; set; }
    public string MatKhauHash { get; set; } = string.Empty;
    public string VaiTro { get; set; } = string.Empty;
    public string? DiaChi { get; set; }
    public bool TrangThai { get; set; }
    public DateTime NgayTao { get; set; }
}
