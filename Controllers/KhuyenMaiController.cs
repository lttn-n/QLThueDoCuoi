using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace QLThueDoCuoi.Controllers;

public class KhuyenMaiController : Controller
{
    [HttpGet]
    public IActionResult Index() => View();

}
