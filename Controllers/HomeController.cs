using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace QLThueDoCuoi.Controllers;

// Layout mẫu. Bổ sung service/repository trước khi xử lý dữ liệu thật.
public class HomeController : Controller
{
    [HttpGet]
    public IActionResult Index() => View();

    [HttpGet]
    public IActionResult Guide() => View();

    [HttpGet]
    public IActionResult About() => View();

    [HttpGet]
    public IActionResult Error() => View(new QLThueDoCuoi.Models.ErrorViewModel
    { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });

}
