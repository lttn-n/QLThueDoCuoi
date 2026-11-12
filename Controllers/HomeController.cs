using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace QLThueDoCuoi.Controllers;

public class HomeController(QLThueDoCuoi.Services.ITrangChuService trangChu, ILogger<HomeController> log) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        log.LogInformation("Mở trang chủ");
        return View(await trangChu.LayTrangChuAsync());
    }

    [HttpGet]
    public IActionResult Guide() => View();

    [HttpGet]
    public IActionResult About() => View();

    [HttpGet]
    public IActionResult Error() => View(new QLThueDoCuoi.Models.ErrorViewModel
    { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });

}
