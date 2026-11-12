using System.ComponentModel.DataAnnotations;
namespace QLThueDoCuoi.ViewModels;

public static class QuyTacMatKhau
{
    public const string Mau = @"^(?=.*\p{L})(?=.*\d)\S{8,100}$";
    public const string Loi = "Mật khẩu 8-100 ký tự, gồm cả chữ và số, không chứa khoảng trắng.";
}

public class DangNhapViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập email hoặc số điện thoại.")]
    [StringLength(254, ErrorMessage = "Tối đa 254 ký tự.")]
    [Display(Name = "Email hoặc số điện thoại")]
    public string TenDangNhap { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
    [StringLength(100, ErrorMessage = "Mật khẩu tối đa 100 ký tự.")]
    [DataType(DataType.Password)]
    [Display(Name = "Mật khẩu")]
    public string MatKhau { get; set; } = string.Empty;

    [Display(Name = "Ghi nhớ đăng nhập")]
    public bool GhiNho { get; set; }

    public string? ReturnUrl { get; set; }
}

public class DangKyViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập họ và tên.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Họ và tên từ 2 đến 100 ký tự.")]
    [Display(Name = "Họ và tên")]
    public string HoTen { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập email hoặc số điện thoại.")]
    [StringLength(254, ErrorMessage = "Tối đa 254 ký tự.")]
    [Display(Name = "Email hoặc số điện thoại")]
    public string TenDangNhap { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
    [RegularExpression(QuyTacMatKhau.Mau, ErrorMessage = QuyTacMatKhau.Loi)]
    [DataType(DataType.Password)]
    [Display(Name = "Mật khẩu")]
    public string MatKhau { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập lại mật khẩu.")]
    [Compare(nameof(MatKhau), ErrorMessage = "Mật khẩu nhập lại không khớp.")]
    [DataType(DataType.Password)]
    [Display(Name = "Nhập lại mật khẩu")]
    public string NhapLaiMatKhau { get; set; } = string.Empty;
}

public class HoSoViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập họ và tên.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Họ và tên từ 2 đến 100 ký tự.")]
    [Display(Name = "Họ và tên")]
    public string HoTen { get; set; } = string.Empty;

    [StringLength(254, ErrorMessage = "Tối đa 254 ký tự.")]
    [EmailAddress(ErrorMessage = "Email chưa đúng định dạng.")]
    [Display(Name = "Email")]
    public string? Email { get; set; }

    [StringLength(20, ErrorMessage = "Tối đa 20 ký tự.")]
    [Display(Name = "Số điện thoại")]
    public string? SoDienThoai { get; set; }

    [StringLength(500, ErrorMessage = "Tối đa 500 ký tự.")]
    [Display(Name = "Địa chỉ")]
    public string? DiaChi { get; set; }
}

public class DoiMatKhauViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập mật khẩu hiện tại.")]
    [DataType(DataType.Password)]
    [Display(Name = "Mật khẩu hiện tại")]
    public string MatKhauCu { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới.")]
    [RegularExpression(QuyTacMatKhau.Mau, ErrorMessage = QuyTacMatKhau.Loi)]
    [DataType(DataType.Password)]
    [Display(Name = "Mật khẩu mới")]
    public string MatKhauMoi { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập lại mật khẩu mới.")]
    [Compare(nameof(MatKhauMoi), ErrorMessage = "Mật khẩu nhập lại không khớp.")]
    [DataType(DataType.Password)]
    [Display(Name = "Nhập lại mật khẩu mới")]
    public string NhapLaiMatKhauMoi { get; set; } = string.Empty;
}

public class TrangHoSoViewModel
{
    public HoSoViewModel HoSo { get; set; } = new();
    public DoiMatKhauViewModel MatKhau { get; set; } = new();
    public DateTime NgayThamGia { get; set; }
}

public class DatLaiMatKhauViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập email hoặc số điện thoại.")]
    [StringLength(254, ErrorMessage = "Tối đa 254 ký tự.")]
    [Display(Name = "Email hoặc số điện thoại")]
    public string TenDangNhap { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới.")]
    [RegularExpression(QuyTacMatKhau.Mau, ErrorMessage = QuyTacMatKhau.Loi)]
    [DataType(DataType.Password)]
    [Display(Name = "Mật khẩu mới")]
    public string MatKhauMoi { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập xác nhận mật khẩu mới.")]
    [Compare(nameof(MatKhauMoi), ErrorMessage = "Mật khẩu xác nhận chưa khớp, vui lòng nhập lại cho đúng.")]
    [DataType(DataType.Password)]
    [Display(Name = "Xác nhận mật khẩu mới")]
    public string NhapLaiMatKhauMoi { get; set; } = string.Empty;
}
