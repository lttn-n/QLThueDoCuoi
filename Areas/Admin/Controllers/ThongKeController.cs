using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace QLThueDoCuoi.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "ADMIN")]
public class ThongKeController : Controller
{
    [HttpGet]
    public IActionResult Index() => View();

}
