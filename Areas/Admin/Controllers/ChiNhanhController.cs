using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLThueDoCuoi.Services;
namespace QLThueDoCuoi.Areas.Admin.Controllers;
[Area("Admin")][Authorize(Roles="ADMIN")]
public class ChiNhanhController(ChiNhanhQuanTriService svc):Controller
{
 public async Task<IActionResult> Index()=>View(await svc.DanhSach());
 [HttpPost] public async Task<IActionResult> Save(int? id,string tenChiNhanh,string diaChi,string? soDienThoai,bool trangThai)
 {
  if(string.IsNullOrWhiteSpace(tenChiNhanh)||string.IsNullOrWhiteSpace(diaChi)||tenChiNhanh.Length>100||diaChi.Length>500||(soDienThoai?.Length??0)>20){TempData["DiemError"]="Cần nhập tên và địa chỉ.";return RedirectToAction(nameof(Index));}
  try{await svc.Luu(id,tenChiNhanh.Trim(),diaChi.Trim(),soDienThoai?.Trim(),trangThai);TempData["DiemOk"]="Đã lưu chi nhánh.";}
  catch(Exception){TempData["DiemError"]="Không lưu được. Tên chi nhánh có thể đã trùng.";}
  return RedirectToAction(nameof(Index));
 }
 [HttpPost] public async Task<IActionResult> Toggle(int id,bool bat){try{await svc.BatTat(id,bat);}catch(Exception){TempData["DiemError"]="Không thay đổi được trạng thái chi nhánh.";}return RedirectToAction(nameof(Index));}
}
