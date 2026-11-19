using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using QLThueDoCuoi.Services;

namespace QLThueDoCuoi.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "ADMIN")]
public class HomeController(AdminHomeService service, ILogger<HomeController> logger) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        try
        {
            ViewBag.DonChoXacNhan = await service.DemDonChoXacNhan();
            ViewBag.LichHenSapToi = await service.DemLichHenSapToi();
            ViewBag.TienCocDangGiu = await service.TienCocDangGiu();
            ViewBag.DoanhThuThucThu = await service.DoanhThuThucThu();
            ViewBag.DoanhThuTheoThang = await service.DoanhThuTheoThang();
            ViewBag.DanhSachLichHenSapToi = await service.LichHenSapToi();
            ViewBag.DonThueGanDay = await service.DonThueGanDay();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Không tải được số liệu tổng quan cho trang Admin.");
            ViewBag.DashboardError = "Không tải được một số số liệu tổng quan. Vui lòng thử lại sau.";
        }

        return View();
    }
}
