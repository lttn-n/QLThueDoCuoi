namespace QLThueDoCuoi.Models;

// Tương ứng bảng ChiTietDonThue trong Database/QuanLyThueDoCuoi.sql.
public class ChiTietDonThue
{
    public int MaChiTietDonThue { get; set; }
    public int MaDonThue { get; set; }
    public int MaSanPham { get; set; }
    public int SoLuong { get; set; }
    public string TenSanPhamKhiThue { get; set; } = string.Empty;
    public string KichThuocKhiThue { get; set; } = string.Empty;
    public string MauSacKhiThue { get; set; } = string.Empty;
    public DateTime NgayNhan { get; set; }
    public DateTime NgayTraDuKien { get; set; }
    public int SoNgayVeSinhKhiThue { get; set; }
    public decimal GiaGoiKhiThue { get; set; }
    public int SoNgayTrongGoiKhiThue { get; set; }
    public decimal GiaNgayThemKhiThue { get; set; }
    public decimal TienThue { get; set; }
    public decimal TienCoc { get; set; }
    public string TrangThai { get; set; } = string.Empty;
    public DateTime? ThoiDiemGiao { get; set; }
    public DateTime? ThoiDiemTra { get; set; }
    public string? TinhTrangKhiTra { get; set; }
    public decimal PhiTraTre { get; set; }
    public decimal PhiHuHong { get; set; }
}
