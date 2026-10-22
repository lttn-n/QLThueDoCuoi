using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace QLThueDoCuoi.Controllers;

[Authorize(Roles = "KHACHHANG")]
// Layout mẫu. Bổ sung service/repository trước khi xử lý dữ liệu thật.
public class DonThueController : Controller
{
    [HttpGet]
    public IActionResult Index() => View();

    [HttpGet]
    public IActionResult Details(int id = 1) => View();

}
