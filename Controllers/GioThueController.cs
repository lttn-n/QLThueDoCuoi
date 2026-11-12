using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using QLThueDoCuoi.Repositories;
using QLThueDoCuoi.Services;
using QLThueDoCuoi.ViewModels;

namespace QLThueDoCuoi.Controllers;

public class GioThueController(GioThueRepository gioThue, IGioThueService dichVu, ILogger<GioThueController> log) : Controller
{
    const string LoiHeThong = "Hệ thống tạm thời chưa xử lý được giỏ thuê. Bạn vui lòng thử lại sau ít phút.";

    int MaHienTai() => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var ma) ? ma : 0;

    [HttpGet]
    [Authorize(Roles = "KHACHHANG")]
    public async Task<IActionResult> Index() => View(await dichVu.LayAsync(MaHienTai()));

    [HttpGet]
    [Authorize(Roles = "KHACHHANG")]
    public IActionResult Checkout() => View();

    [HttpPost]
    [Authorize(Roles = "KHACHHANG")]
    public async Task<IActionResult> Them(ThemGioInput vao)
    {
        try
        {
            var kq = await dichVu.ThemAsync(MaHienTai(), vao);
            if (!kq.ThanhCong) return BadRequest(new { loi = kq.Loi });
            return Json(new { total = kq.Tong, ten = kq.Ten });
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            log.LogError(ex, "Không thêm được vào giỏ thuê");
            return StatusCode(500, new { loi = LoiHeThong });
        }
    }

    [HttpPost]
    [Authorize(Roles = "KHACHHANG")]
    public async Task<IActionResult> CapNhat(List<DongGioInput> dong)
    {
        try
        {
            var kq = await dichVu.CapNhatAsync(MaHienTai(), dong ?? new());
            TempData[kq.ThanhCong ? "ThongBao" : "ThongBaoLoi"] = kq.ThanhCong ? "Đã lưu thay đổi giỏ thuê." : kq.Loi;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            log.LogError(ex, "Không cập nhật được giỏ thuê");
            TempData["ThongBaoLoi"] = LoiHeThong;
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = "KHACHHANG")]
    public async Task<IActionResult> Xoa(int id)
    {
        try
        {
            if (await dichVu.XoaAsync(MaHienTai(), id)) TempData["ThongBao"] = "Đã xóa sản phẩm khỏi giỏ thuê.";
            else TempData["ThongBaoLoi"] = "Không tìm thấy sản phẩm trong giỏ thuê.";
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            log.LogError(ex, "Không xóa được dòng giỏ thuê");
            TempData["ThongBaoLoi"] = LoiHeThong;
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = "KHACHHANG")]
    public async Task<IActionResult> XoaTatCa()
    {
        try { await dichVu.XoaTatCaAsync(MaHienTai()); TempData["ThongBao"] = "Đã xóa toàn bộ giỏ thuê."; }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            log.LogError(ex, "Không xóa được giỏ thuê");
            TempData["ThongBaoLoi"] = LoiHeThong;
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = "KHACHHANG")]
    public async Task<IActionResult> XoaNhieu(int[] chon)
    {
        try
        {
            if (chon is null || chon.Length == 0) TempData["ThongBaoLoi"] = "Bạn chưa chọn sản phẩm nào để xóa.";
            else
            {
                var n = await dichVu.XoaNhieuAsync(MaHienTai(), chon);
                if (n > 0) TempData["ThongBao"] = $"Đã xóa {n} sản phẩm khỏi giỏ thuê.";
                else TempData["ThongBaoLoi"] = "Không tìm thấy sản phẩm đã chọn trong giỏ thuê.";
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            log.LogError(ex, "Không xóa được các dòng giỏ thuê");
            TempData["ThongBaoLoi"] = LoiHeThong;
        }
        return RedirectToAction(nameof(Index));
    }
}
