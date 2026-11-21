using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using QLThueDoCuoi.Services;

namespace QLThueDoCuoi.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "ADMIN")]
public class TaiKhoanController(
    QuanTriTaiKhoanService service,
    ILogger<TaiKhoanController> logger) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        try
        {
            ViewBag.MaTaiKhoanDangNhap = MaTaiKhoanDangNhap();
            return View(await service.DanhSach());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Không tải được danh sách tài khoản.");
            TempData["DiemError"] = "Không tải được danh sách tài khoản. Vui lòng thử lại.";
            return View(new List<Dictionary<string, object?>>());
        }
    }

    [HttpPost]
    public async Task<IActionResult> CapNhatVaiTro(int maTaiKhoan, string vaiTro)
    {
        var maDangNhap = MaTaiKhoanDangNhap();
        try
        {
            await service.CapNhatVaiTro(maTaiKhoan, vaiTro, maDangNhap);
            TempData["DiemOk"] = "Đã cập nhật vai trò tài khoản.";
        }
        catch (ArgumentException ex)
        {
            TempData["DiemError"] = ex.Message;
        }
        catch (InvalidOperationException ex)
        {
            TempData["DiemError"] = ex.Message;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Không cập nhật được vai trò tài khoản {Ma}", maTaiKhoan);
            TempData["DiemError"] = "Không cập nhật được vai trò. Vui lòng thử lại.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public Task<IActionResult> KhoaTaiKhoan(int maTaiKhoan)
        => CapNhatTrangThai(maTaiKhoan, false);

    [HttpPost]
    public Task<IActionResult> MoKhoaTaiKhoan(int maTaiKhoan)
        => CapNhatTrangThai(maTaiKhoan, true);

    private async Task<IActionResult> CapNhatTrangThai(int maTaiKhoan, bool trangThai)
    {
        var maDangNhap = MaTaiKhoanDangNhap();
        try
        {
            await service.CapNhatTrangThai(maTaiKhoan, trangThai, maDangNhap);
            TempData["DiemOk"] = trangThai
                ? "Đã mở khóa tài khoản."
                : "Đã khóa tài khoản.";
        }
        catch (ArgumentException ex)
        {
            TempData["DiemError"] = ex.Message;
        }
        catch (InvalidOperationException ex)
        {
            TempData["DiemError"] = ex.Message;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Không cập nhật được trạng thái tài khoản {Ma}", maTaiKhoan);
            TempData["DiemError"] = "Không cập nhật được trạng thái tài khoản.";
        }

        return RedirectToAction(nameof(Index));
    }

    // Lay ma tai khoan tu claim, khong tin ma gui len chi de khoa/mo tai khoan.
    int MaTaiKhoanDangNhap()
        => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var ma)
            ? ma
            : 0;
}
