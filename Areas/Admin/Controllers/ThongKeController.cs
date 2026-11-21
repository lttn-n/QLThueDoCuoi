using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using QLThueDoCuoi.Services;

namespace QLThueDoCuoi.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "ADMIN")]
public class ThongKeController(ThongKeService service, ILogger<ThongKeController> logger) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(DateOnly? tuNgay, DateOnly? denNgay)
    {
        var homNay = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(7));
        var tu = tuNgay ?? new DateOnly(homNay.Year, homNay.Month, 1);
        var den = denNgay ?? homNay;

        ViewBag.TuNgay = tu.ToString("yyyy-MM-dd");
        ViewBag.DenNgay = den.ToString("yyyy-MM-dd");

        try
        {
            var baoCao = await service.LayBaoCao(tu, den);
            ViewBag.TongHop = baoCao.TongHop;
            ViewBag.DoanhThuTheoThang = baoCao.TheoThang;
        }
        catch (ArgumentException ex)
        {
            ViewBag.BaoCaoError = ex.Message;
            ViewBag.TongHop = new Dictionary<string, object?>
            {
                ["DoanhThu"] = 0m,
                ["ThuThue"] = 0m,
                ["ThuPhuPhi"] = 0m,
                ["HoanTien"] = 0m
            };
            ViewBag.DoanhThuTheoThang = new List<Dictionary<string, object?>>();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Không tải được báo cáo doanh thu.");
            ViewBag.BaoCaoError = "Không tải được báo cáo. Vui lòng thử lại.";
            ViewBag.TongHop = new Dictionary<string, object?>
            {
                ["DoanhThu"] = 0m,
                ["ThuThue"] = 0m,
                ["ThuPhuPhi"] = 0m,
                ["HoanTien"] = 0m
            };
            ViewBag.DoanhThuTheoThang = new List<Dictionary<string, object?>>();
        }

        return View();
    }

}
