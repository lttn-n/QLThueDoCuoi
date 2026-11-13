using Microsoft.AspNetCore.Mvc;
using QLThueDoCuoi.Repositories;
namespace QLThueDoCuoi.Controllers;
public class KhuyenMaiController(KhuyenMaiRepository repo):Controller
{
 public async Task<IActionResult> Index()=>View(await repo.DanhSach());
}
