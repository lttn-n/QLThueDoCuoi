namespace QLThueDoCuoi.Models;

// Tương ứng bảng DonThue trong Database/QuanLyThueDoCuoi.sql.
public class DonThue
{
    public int MaDonThue { get; set; }
    public int MaTaiKhoan { get; set; }
    public DateTime NgayDat { get; set; }
    public string TrangThai { get; set; } = string.Empty;
    public string TenNguoiNhan { get; set; } = string.Empty;
    public string SoDienThoaiNhan { get; set; } = string.Empty;
    public string HinhThucNhan { get; set; } = string.Empty;
    public int? MaChiNhanhNhan { get; set; }
    public string PhuongThucThanhToan { get; set; } = string.Empty;
    public string? DiaChiNhan { get; set; }
    public decimal PhiGiaoNhan { get; set; }
    public int? MaKhuyenMai { get; set; }
    public decimal SoTienGiam { get; set; }
    public string? GhiChu { get; set; }
}
