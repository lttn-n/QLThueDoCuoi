namespace QLThueDoCuoi.Models;

// Tương ứng bảng LichHenThuDo trong Database/QuanLyThueDoCuoi.sql.
public class LichHenThuDo
{
    public int MaLichHen { get; set; }
    public int MaTaiKhoan { get; set; }
    public int MaSanPham { get; set; }
    public int MaChiNhanh { get; set; }
    public string HoTenLienHe { get; set; } = string.Empty;
    public string SoDienThoaiLienHe { get; set; } = string.Empty;
    public DateTime ThoiDiemHen { get; set; }
    public string TrangThai { get; set; } = string.Empty;
    public string? GhiChu { get; set; }
    public DateTime NgayTao { get; set; }
    public DateTime? ThoiDiemHuy { get; set; }
    public int? MaTaiKhoanHuy { get; set; }
}
