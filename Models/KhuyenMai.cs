namespace QLThueDoCuoi.Models;

// Tương ứng bảng KhuyenMai trong Database/QuanLyThueDoCuoi.sql.
public class KhuyenMai
{
    public int MaKhuyenMai { get; set; }
    public string MaUuDai { get; set; } = string.Empty;
    public string TenKhuyenMai { get; set; } = string.Empty;
    public string HinhThucGiam { get; set; } = string.Empty;
    public decimal GiaTriGiam { get; set; }
    public decimal? SoTienGiamToiDa { get; set; }
    public decimal GiaTriDonToiThieu { get; set; }
    public DateTime NgayBatDau { get; set; }
    public DateTime NgayKetThuc { get; set; }
    public int? SoLuotSuDungToiDa { get; set; }
    public bool TrangThai { get; set; }
}
