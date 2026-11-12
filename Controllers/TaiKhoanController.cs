using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLThueDoCuoi.Models;
using QLThueDoCuoi.Services;
using QLThueDoCuoi.ViewModels;

namespace QLThueDoCuoi.Controllers;

public class TaiKhoanController(ITaiKhoanService dichVu, ILogger<TaiKhoanController> log) : Controller
{
    const string LoiHeThong = "Hệ thống tạm thời chưa kết nối được cơ sở dữ liệu. Bạn vui lòng thử lại sau ít phút.";

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true) return RedirectToAction("Index", "Home", new { area = "" });
        return View(new DangNhapViewModel { ReturnUrl = returnUrl });
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> Login(DangNhapViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);
        try
        {
            var kq = await dichVu.DangNhapAsync(vm.TenDangNhap, vm.MatKhau);
            if (!kq.ThanhCong)
            {
                ModelState.AddModelError(kq.Truong ?? string.Empty, kq.Loi!);
                return View(vm);
            }
            await DangNhapCookieAsync(kq.TaiKhoan!, vm.GhiNho);
            TempData["ThongBao"] = $"Chào mừng {kq.TaiKhoan!.HoTen} quay lại Dragneel.";
            return ChuyenHuongSauDangNhap(kq.TaiKhoan!, vm.ReturnUrl);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            log.LogError(ex, "Lỗi khi đăng nhập");
            ModelState.AddModelError(string.Empty, LoiHeThong);
            return View(vm);
        }
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true) return RedirectToAction("Index", "Home", new { area = "" });
        return View(new DangKyViewModel());
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> Register(DangKyViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);
        try
        {
            var kq = await dichVu.DangKyAsync(vm);
            if (!kq.ThanhCong)
            {
                ModelState.AddModelError(kq.Truong ?? string.Empty, kq.Loi!);
                return View(vm);
            }
            TempData["ThongBao"] = "Tạo tài khoản thành công! Hãy đăng nhập để tiếp tục.";
            return RedirectToAction(nameof(Login));
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            log.LogError(ex, "Lỗi khi đăng ký");
            ModelState.AddModelError(string.Empty, LoiHeThong);
            return View(vm);
        }
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult QuenMatKhau()
    {
        if (User.Identity?.IsAuthenticated == true) return RedirectToAction(nameof(Profile));
        return View(new DatLaiMatKhauViewModel());
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> QuenMatKhau(DatLaiMatKhauViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);
        try
        {
            var kq = await dichVu.DatLaiMatKhauAsync(vm);
            if (!kq.ThanhCong)
            {
                ModelState.AddModelError(kq.Truong ?? string.Empty, kq.Loi!);
                return View(vm);
            }
            TempData["ThongBao"] = "Đặt lại mật khẩu thành công! Hãy đăng nhập bằng mật khẩu mới.";
            return RedirectToAction(nameof(Login));
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            log.LogError(ex, "Lỗi khi đặt lại mật khẩu");
            ModelState.AddModelError(string.Empty, LoiHeThong);
            return View(vm);
        }
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
    public async Task<IActionResult> Profile()
    {
        var tk = await LayTaiKhoanHienTaiAsync();
        if (tk is null) return await DangXuatVeDangNhapAsync();
        return View(TaoTrang(tk));
    }

    [Authorize(Roles = "KHACHHANG")]
    [HttpPost]
    public async Task<IActionResult> CapNhatHoSo([Bind(Prefix = "HoSo")] HoSoViewModel hoSo)
    {
        var tk = await LayTaiKhoanHienTaiAsync();
        if (tk is null) return await DangXuatVeDangNhapAsync();
        var trang = TaoTrang(tk);
        trang.HoSo = hoSo;
        if (!ModelState.IsValid) return View(nameof(Profile), trang);

        try
        {
            var kq = await dichVu.CapNhatHoSoAsync(tk.MaTaiKhoan, hoSo);
            if (!kq.ThanhCong)
            {
                ModelState.AddModelError(Khoa("HoSo", kq.Truong), kq.Loi!);
                return View(nameof(Profile), trang);
            }
            await LamMoiCookieAsync(kq.TaiKhoan!);
            TempData["ThongBao"] = "Đã lưu thông tin cá nhân.";
            return RedirectToAction(nameof(Profile));
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            log.LogError(ex, "Lỗi khi cập nhật hồ sơ {Ma}", tk.MaTaiKhoan);
            ModelState.AddModelError(string.Empty, LoiHeThong);
            return View(nameof(Profile), trang);
        }
    }

    [Authorize(Roles = "KHACHHANG")]
    [HttpPost]
    public async Task<IActionResult> DoiMatKhau([Bind(Prefix = "MatKhau")] DoiMatKhauViewModel matKhau)
    {
        var tk = await LayTaiKhoanHienTaiAsync();
        if (tk is null) return await DangXuatVeDangNhapAsync();
        var trang = TaoTrang(tk);
        if (!ModelState.IsValid) return View(nameof(Profile), trang);

        try
        {
            var kq = await dichVu.DoiMatKhauAsync(tk.MaTaiKhoan, matKhau);
            if (!kq.ThanhCong)
            {
                ModelState.AddModelError(Khoa("MatKhau", kq.Truong), kq.Loi!);
                return View(nameof(Profile), trang);
            }
            TempData["ThongBao"] = "Đã đổi mật khẩu thành công.";
            return RedirectToAction(nameof(Profile));
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            log.LogError(ex, "Lỗi khi đổi mật khẩu {Ma}", tk.MaTaiKhoan);
            ModelState.AddModelError(string.Empty, LoiHeThong);
            return View(nameof(Profile), trang);
        }
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        TempData["ThongBao"] = "Bạn đã đăng xuất. Hẹn gặp lại!";
        return RedirectToAction("Index", "Home", new { area = "" });
    }

    static string Khoa(string tienTo, string? truong) => string.IsNullOrEmpty(truong) ? string.Empty : $"{tienTo}.{truong}";

    int MaHienTai() => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var ma) ? ma : 0;

    async Task<TaiKhoan?> LayTaiKhoanHienTaiAsync()
    {
        var ma = MaHienTai();
        if (ma == 0) return null;
        var tk = await dichVu.LayAsync(ma);
        return tk is { TrangThai: true } ? tk : null;
    }

    async Task<IActionResult> DangXuatVeDangNhapAsync()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    static TrangHoSoViewModel TaoTrang(TaiKhoan tk) => new()
    {
        HoSo = new HoSoViewModel { HoTen = tk.HoTen, Email = tk.Email, SoDienThoai = tk.SoDienThoai, DiaChi = tk.DiaChi },
        NgayThamGia = tk.NgayTao.AddHours(7)
    };

    static ClaimsPrincipal TaoPrincipal(TaiKhoan tk) => new(new ClaimsIdentity(new[]
    {
        new Claim(ClaimTypes.NameIdentifier, tk.MaTaiKhoan.ToString()),
        new Claim(ClaimTypes.Name, tk.HoTen),
        new Claim(ClaimTypes.Role, tk.VaiTro)
    }, CookieAuthenticationDefaults.AuthenticationScheme));

    Task DangNhapCookieAsync(TaiKhoan tk, bool ghiNho) =>
        HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, TaoPrincipal(tk),
            new AuthenticationProperties { IsPersistent = ghiNho, ExpiresUtc = ghiNho ? DateTimeOffset.UtcNow.AddDays(14) : null });

    async Task LamMoiCookieAsync(TaiKhoan tk)
    {
        var hienTai = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, TaoPrincipal(tk),
            hienTai.Properties ?? new AuthenticationProperties());
    }

    IActionResult ChuyenHuongSauDangNhap(TaiKhoan tk, string? returnUrl)
    {
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)) return LocalRedirect(returnUrl);
        return tk.VaiTro switch
        {
            "ADMIN" => RedirectToAction("Index", "Home", new { area = "Admin" }),
            "NHANVIEN" => RedirectToAction("Index", "Home", new { area = "NhanVien" }),
            _ => RedirectToAction("Index", "Home", new { area = "" })
        };
    }
}
