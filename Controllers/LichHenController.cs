using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace QLThueDoCuoi.Controllers;

[Authorize(Roles = "KHACHHANG")]
// Layout mẫu. Bổ sung service/repository trước khi xử lý dữ liệu thật.
public class LichHenController : Controller
{
    [HttpGet]
    public IActionResult Index() => View();

    [HttpGet]
    public IActionResult Details(int id = 1) => View();

    [HttpGet]
    public IActionResult Create(int maSanPham = 1)
    {
        var sanPham = QLThueDoCuoi.Data.DuLieuGiaoDien.SanPham.FirstOrDefault(x => x.MaSanPham == maSanPham);
        return sanPham is null ? NotFound() : View(sanPham);
    }

}
