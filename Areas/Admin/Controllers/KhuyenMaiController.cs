using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLThueDoCuoi.Services;
namespace QLThueDoCuoi.Areas.Admin.Controllers;
[Area("Admin")][Authorize(Roles="ADMIN")]
public class KhuyenMaiController(KhuyenMaiQuanTriService svc):Controller
{
 public async Task<IActionResult> Index()=>View(await svc.DanhSach());
 [HttpGet] public async Task<IActionResult> Edit(int? id){var r=id.HasValue?await svc.ChiTiet(id.Value):[];return View(r.Count==0?new Dictionary<string,object?>():r[0]);}
 [HttpPost] public async Task<IActionResult> Edit(int? id,string maUuDai,string tenKhuyenMai,string hinhThucGiam,decimal giaTriGiam,decimal? soTienGiamToiDa,decimal giaTriDonToiThieu,DateTime ngayBatDau,DateTime ngayKetThuc,int? soLuotSuDungToiDa,bool trangThai)
 {
  if(string.IsNullOrWhiteSpace(maUuDai)||string.IsNullOrWhiteSpace(tenKhuyenMai)||maUuDai.Length>40||tenKhuyenMai.Length>200||!new[]{"PHANTRAM","SOTIEN"}.Contains(hinhThucGiam)||giaTriGiam<=0||(hinhThucGiam=="PHANTRAM"&&giaTriGiam>100)||giaTriDonToiThieu<0||(soTienGiamToiDa.HasValue&&soTienGiamToiDa.Value<=0)||(soLuotSuDungToiDa.HasValue&&soLuotSuDungToiDa.Value<=0)||ngayKetThuc<=ngayBatDau){TempData["DiemError"]="Dữ liệu khuyến mãi không hợp lệ.";return RedirectToAction(nameof(Edit),new{id});}
  try{await svc.Luu(id,maUuDai.Trim(),tenKhuyenMai.Trim(),hinhThucGiam,giaTriGiam,soTienGiamToiDa,giaTriDonToiThieu,ngayBatDau,ngayKetThuc,soLuotSuDungToiDa,trangThai);TempData["DiemOk"]="Đã lưu.";return RedirectToAction(nameof(Index));}
  catch(Exception){TempData["DiemError"]="Không lưu được. Hãy kiểm tra mã ưu đãi trùng hoặc dữ liệu không hợp lệ.";return RedirectToAction(nameof(Edit),new{id});}
 }
 [HttpPost] public async Task<IActionResult> Toggle(int id,bool bat){try{await svc.BatTat(id,bat);}catch(Exception){TempData["DiemError"]="Không thay đổi được trạng thái mã ưu đãi.";}return RedirectToAction(nameof(Index));}
}
