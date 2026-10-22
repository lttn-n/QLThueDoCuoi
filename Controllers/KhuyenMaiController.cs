using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace QLThueDoCuoi.Controllers;

// Layout mẫu. Bổ sung service/repository trước khi xử lý dữ liệu thật.
public class KhuyenMaiController : Controller
{
    [HttpGet]
    public IActionResult Index() => View();

}
