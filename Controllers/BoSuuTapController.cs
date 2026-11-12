using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace QLThueDoCuoi.Controllers;

public class BoSuuTapController : Controller
{
    [HttpGet]
    public IActionResult Index() => View();

}
