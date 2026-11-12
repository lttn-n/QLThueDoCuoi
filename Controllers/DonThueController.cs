using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace QLThueDoCuoi.Controllers;

[Authorize(Roles = "KHACHHANG")]
public class DonThueController : Controller
{
    [HttpGet]
    public IActionResult Index() => View();

    [HttpGet]
    public IActionResult Details(int id = 1) => View();

}
