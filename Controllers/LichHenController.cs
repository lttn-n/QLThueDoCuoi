using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLThueDoCuoi.Repositories;
using QLThueDoCuoi.Services;
using System.Security.Claims;
namespace QLThueDoCuoi.Controllers;
[Authorize(Roles="KHACHHANG")]
public sealed class LichHenController(LichHenService svc,ChiNhanhRepository cn,DiemDb db):Controller
{
    private int Tk()=>int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier),out var id)?id:0;
    public async Task<IActionResult> Index()=>View(await svc.DanhSach(Tk()));
    public async Task<IActionResult> Details(int id){var r=await svc.ChiTiet(Tk(),id);return r.Count==0?NotFound():View(r[0]);}
    [HttpGet] public async Task<IActionResult> Create(){await LoadOptions();return View();}
    private async Task LoadOptions(){
        ViewBag.ChiNhanh=await cn.DanhSach();
        ViewBag.SanPham=await db.Query("SELECT MaSanPham,TenSanPham,KichThuoc FROM dbo.SanPham WHERE TrangThai=1 ORDER BY TenSanPham,KichThuoc");
    }
    [HttpPost] public async Task<IActionResult> Create(int maSanPham,int maChiNhanh,string? hoTenLienHe,string? soDienThoaiLienHe,DateTime thoiDiemHen,string? ghiChu){
        var error=await svc.Tao(Tk(),maSanPham,maChiNhanh,hoTenLienHe,soDienThoaiLienHe,thoiDiemHen,ghiChu);
        if(error is null){TempData["DiemOk"]="Đặt lịch thành công.";return RedirectToAction(nameof(Index));}
        ModelState.AddModelError("",error);await LoadOptions();return View();
    }
    [HttpPost] public async Task<IActionResult> Cancel(int id,string? lyDo){
        if(string.IsNullOrWhiteSpace(lyDo)){TempData["DiemError"]="Cần nhập lý do hủy lịch.";return RedirectToAction(nameof(Details),new{id});}
        var ok=await svc.Huy(Tk(),id,lyDo);
        TempData[ok?"DiemOk":"DiemError"]=ok?"Đã hủy lịch.":"Không thể hủy lịch này.";
        return RedirectToAction(nameof(Index));
    }
}
