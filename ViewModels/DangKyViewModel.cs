using System.ComponentModel.DataAnnotations;
namespace QLThueDoCuoi.ViewModels;
public class DangKyViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập họ tên.")]
    [StringLength(100)]
    public string HoTen { get; set; } = string.Empty;
    [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
    public string? Email { get; set; }
    [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
    public string? SoDienThoai { get; set; }
    [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Mật khẩu cần ít nhất 8 ký tự.")]
    [DataType(DataType.Password)]
    public string MatKhau { get; set; } = string.Empty;
    [Required(ErrorMessage = "Vui lòng nhập lại mật khẩu.")]
    [Compare(nameof(MatKhau), ErrorMessage = "Mật khẩu nhập lại không khớp.")]
    [DataType(DataType.Password)]
    public string NhapLaiMatKhau { get; set; } = string.Empty;

    [Range(typeof(bool), "true", "true", ErrorMessage = "Bạn cần đồng ý điều khoản.")]
    public bool DongYDieuKhoan { get; set; }
}