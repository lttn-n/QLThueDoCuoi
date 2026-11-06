using System.ComponentModel.DataAnnotations;

namespace QLThueDoCuoi.ViewModels;

public class CapNhatThongTinCaNhanViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập họ tên.")]
    [StringLength(100)]
    public string HoTen { get; set; } = string.Empty;
    [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
    [StringLength(254)]
    public string? Email { get; set; }
    [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
    [StringLength(20)]
    public string? SoDienThoai { get; set; }
    [StringLength(500)]
    public string? DiaChi { get; set; }
}