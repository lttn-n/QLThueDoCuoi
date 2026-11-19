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
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Không tải được số đơn thuê chờ xác nhận cho trang Admin.");
            ViewBag.DashboardError = "Không tải được số đơn chờ xác nhận. Vui lòng thử lại sau.";
        }

        return View();
    }
}
