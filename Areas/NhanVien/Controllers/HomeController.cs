using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace QLThueDoCuoi.Areas.NhanVien.Controllers;

[Area("NhanVien")]
[Authorize(Roles = "NHANVIEN,ADMIN")]
public class HomeController : Controller
{
    [HttpGet]
    public IActionResult Index() => View();

}
