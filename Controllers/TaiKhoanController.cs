using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using QLThueDoCuoi.Models;
using QLThueDoCuoi.Repositories;
using QLThueDoCuoi.ViewModels;

namespace QLThueDoCuoi.Controllers;

// NHÓM 2: bổ sung đăng ký/đăng nhập thật bằng dữ liệu TaiKhoan.
// Không tạo cookie từ tên/role do trình duyệt tự truyền.
public class TaiKhoanController : Controller
{
    private readonly TaiKhoanRepository _taiKhoanRepository;

    public TaiKhoanController(TaiKhoanRepository taiKhoanRepository)
    {
        _taiKhoanRepository = taiKhoanRepository;
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View(new DangNhapViewModel());
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> Login(DangNhapViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View(model);
        }

        var taiKhoan = _taiKhoanRepository.GetTaiKhoanByTenDangNhap(model.TenDangNhap);
        var hasher = new PasswordHasher<TaiKhoan>();
        if (taiKhoan is null ||
            !taiKhoan.TrangThai ||
            hasher.VerifyHashedPassword(taiKhoan, taiKhoan.MatKhauHash, model.MatKhau)
                == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError(string.Empty, "Thông tin đăng nhập không hợp lệ.");
            ViewData["ReturnUrl"] = returnUrl;
            return View(model);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, taiKhoan.MaTaiKhoan.ToString()),
            new(ClaimTypes.Name, taiKhoan.HoTen),
            new(ClaimTypes.Role, taiKhoan.VaiTro)
        };
        var identity = new ClaimsIdentity(
            claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme, principal);

        if (Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl!);

        return RedirectToAction("Index", "Home", new { area = "" });
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Register() => View(new DangKyViewModel());

    [AllowAnonymous]
    [HttpPost]
    public IActionResult Register(DangKyViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Email) &&
            string.IsNullOrWhiteSpace(model.SoDienThoai))
        {
            ModelState.AddModelError(nameof(model.Email),
                "Vui lòng nhập email hoặc số điện thoại.");
        }

        if (!ModelState.IsValid)
            return View(model);

        var taiKhoan = new TaiKhoan
        {
            HoTen = model.HoTen.Trim(),
            Email = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim(),
            SoDienThoai = string.IsNullOrWhiteSpace(model.SoDienThoai)
                ? null
                : model.SoDienThoai.Trim()
        };

        var hasher = new PasswordHasher<TaiKhoan>();
        taiKhoan.MatKhauHash = hasher.HashPassword(taiKhoan, model.MatKhau);

        bool daThem = _taiKhoanRepository.ThemKhachHang(
            taiKhoan.HoTen,
            taiKhoan.Email,
            taiKhoan.SoDienThoai,
            taiKhoan.MatKhauHash);

        if (!daThem)
        {
            ModelState.AddModelError(nameof(model.Email),
                "Email hoặc số điện thoại đã được sử dụng.");
            return View(model);
        }

        TempData["ThongBao"] = "Đăng ký thành công. Vui lòng đăng nhập.";
        return RedirectToAction(nameof(Login));
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult AccessDenied()
    {
        Response.StatusCode = StatusCodes.Status403Forbidden;
        return View();
    }

    [Authorize(Roles = "KHACHHANG")]
    [HttpGet]
    public IActionResult Profile() => View();

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        // Anti-forgery đã được kiểm tra bởi filter toàn cục trong Program.cs.
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home", new { area = "" });
    }
}
