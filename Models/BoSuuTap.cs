namespace QLThueDoCuoi.Models;

public class BoSuuTap
{
    public int MaBoSuuTap { get; set; }
    public string TenBoSuuTap { get; set; } = string.Empty;
    public string? MoTa { get; set; }
    public string? DuongDanAnh { get; set; }
    public bool HienThi { get; set; }
}
