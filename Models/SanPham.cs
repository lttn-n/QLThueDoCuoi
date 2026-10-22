namespace QLThueDoCuoi.Models;

// Tương ứng bảng SanPham trong Database/QuanLyThueDoCuoi.sql.
public class SanPham
{
    public int MaSanPham { get; set; }
    public int MaLoaiSanPham { get; set; }
    public string TenSanPham { get; set; } = string.Empty;
    public string KichThuoc { get; set; } = string.Empty;
    public string MauSac { get; set; } = string.Empty;
    public string? MoTa { get; set; }
    public string? ChatLieu { get; set; }
    public decimal GiaThueGoi { get; set; }
    public int SoNgayThueTrongGoi { get; set; }
    public decimal GiaThueMoiNgayThem { get; set; }
    public decimal TienCoc { get; set; }
    public int SoNgayVeSinh { get; set; }
    public decimal? GiaKhuyenMai { get; set; }
    public DateTime? NgayBatDauGiamGia { get; set; }
    public DateTime? NgayKetThucGiamGia { get; set; }
    public bool TrangThai { get; set; }
    public DateTime NgayTao { get; set; }
}
