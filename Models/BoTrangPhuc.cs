namespace QLThueDoCuoi.Models;

// Tương ứng bảng BoTrangPhuc trong Database/QuanLyThueDoCuoi.sql.
public class BoTrangPhuc
{
    public int MaBoTrangPhuc { get; set; }
    public int MaSanPham { get; set; }
    public int MaChiNhanh { get; set; }
    public string MaQuanLy { get; set; } = string.Empty;
    public string TinhTrang { get; set; } = string.Empty;
    public string? GhiChu { get; set; }
}
