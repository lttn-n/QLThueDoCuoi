using Microsoft.AspNetCore.Mvc;
using QLThueDoCuoi.Repositories;
namespace QLThueDoCuoi.Controllers;
public sealed class BoSuuTapController(BoSuuTapRepository repo):Controller
{
 public async Task<IActionResult> Index()=>View(await repo.DanhSach());
 public async Task<IActionResult> Details(int id){
    if(!await repo.TonTai(id))return NotFound();
    ViewBag.Products=await repo.SanPham(id);return View(id);
 }
}
