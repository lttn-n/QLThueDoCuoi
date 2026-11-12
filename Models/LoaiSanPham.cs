namespace QLThueDoCuoi.Models;

public class LoaiSanPham
{
    public int MaLoaiSanPham { get; set; }
    public int? MaLoaiCha { get; set; }
    public string TenLoaiSanPham { get; set; } = string.Empty;
    public bool HienThi { get; set; }
}
