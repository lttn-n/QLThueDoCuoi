using System.ComponentModel.DataAnnotations;
namespace QLThueDoCuoi.ViewModels;
public class DatLichViewModel
{
    [Required] public int? MaSanPham { get; set; }
    [Required] public int? MaChiNhanh { get; set; }
    [Required, StringLength(100)] public string HoTenLienHe { get; set; } = string.Empty;
    [Required, Phone] public string SoDienThoaiLienHe { get; set; } = string.Empty;
    [Required, DataType(DataType.Date)] public DateTime? NgayHen { get; set; }
    [Required] public TimeSpan? GioHen { get; set; }
    [StringLength(2000)] public string? GhiChu { get; set; }
}
