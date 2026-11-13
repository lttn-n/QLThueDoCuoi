using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLThueDoCuoi.Services;
using System.Security.Claims;
namespace QLThueDoCuoi.Controllers;
[Authorize(Roles="KHACHHANG")]
public sealed class DonThueController(DonThueKhachHangService svc):Controller
{
 private int Tk()=>int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier),out var id)?id:0;
 public async Task<IActionResult> Index()=>View(await svc.DanhSach(Tk()));
 public async Task<IActionResult> Details(int id){
    var (don,chiTiet)=await svc.ChiTiet(Tk(),id);
    if(don is null)return NotFound();
    ViewBag.Items=chiTiet;return View(don);
 }
}
