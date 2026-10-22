using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace QLThueDoCuoi.Controllers;

// NHÓM 2: bổ sung đăng ký/đăng nhập thật bằng dữ liệu TaiKhoan.
// Không tạo cookie từ tên/role do trình duyệt tự truyền.
public class TaiKhoanController : Controller
{
    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login() => View();

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Register() => View();

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
