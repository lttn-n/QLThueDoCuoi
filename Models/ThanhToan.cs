namespace QLThueDoCuoi.Models;

// Tương ứng bảng ThanhToan trong Database/QuanLyThueDoCuoi.sql.
public class ThanhToan
{
    public int MaThanhToan { get; set; }
    public int MaDonThue { get; set; }
    public string LoaiGiaoDich { get; set; } = string.Empty;
    public decimal SoTien { get; set; }
    public string PhuongThuc { get; set; } = string.Empty;
    public string TrangThai { get; set; } = string.Empty;
    public string? MaThamChieu { get; set; }
    public DateTime NgayTao { get; set; }
    public DateTime? ThoiDiemHoanTat { get; set; }
    public string? GhiChu { get; set; }
}
