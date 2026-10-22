using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace QLThueDoCuoi.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "ADMIN")]
// Layout mẫu. Bổ sung service/repository trước khi xử lý dữ liệu thật.
public class HomeController : Controller
{
    [HttpGet]
    public IActionResult Index() => View();

}
