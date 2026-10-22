using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace QLThueDoCuoi.Controllers;

// Layout mẫu. Bổ sung service/repository trước khi xử lý dữ liệu thật.
public class SanPhamController : Controller
{
    [HttpGet]
    public IActionResult Index() => View();

    [HttpGet]
    public IActionResult Details(int id = 1)
    {
        var sanPham = QLThueDoCuoi.Data.DuLieuGiaoDien.SanPham.FirstOrDefault(x => x.MaSanPham == id);
        return sanPham is null ? NotFound() : View(sanPham);
    }

}
