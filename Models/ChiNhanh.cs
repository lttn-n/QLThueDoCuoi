namespace QLThueDoCuoi.Models;

// Tương ứng bảng ChiNhanh trong Database/QuanLyThueDoCuoi.sql.
public class ChiNhanh
{
    public int MaChiNhanh { get; set; }
    public string TenChiNhanh { get; set; } = string.Empty;
    public string DiaChi { get; set; } = string.Empty;
    public string? SoDienThoai { get; set; }
    public bool TrangThai { get; set; }
}
