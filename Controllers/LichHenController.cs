using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace QLThueDoCuoi.Controllers;

[Authorize(Roles = "KHACHHANG")]
public class LichHenController(QLThueDoCuoi.Repositories.SanPhamRepository sanPhamRepo) : Controller
{
    [HttpGet]
    public IActionResult Index() => View();

    [HttpGet]
    public IActionResult Details(int id = 1) => View();

    [HttpGet]
    public async Task<IActionResult> Create(int maSanPham = 1)
    {
        QLThueDoCuoi.ViewModels.TheSanPhamViewModel? sanPham = null;
        try
        {
            var ct = await sanPhamRepo.LayChiTietAsync(maSanPham);
            if (ct is not null)
                sanPham = new QLThueDoCuoi.ViewModels.TheSanPhamViewModel(ct.MaSanPham, ct.Ten, ct.TenLoai, ct.GiaThue, ct.SoNgayGoi, ct.Anh,
                    ct.Nhan, string.Join(' ', ct.CacSize.Select(x => x.KichThuoc)), ct.GiaGoc);
        }
        catch (Exception ex) when (ex is not OperationCanceledException) { }
        sanPham ??= QLThueDoCuoi.Data.DuLieuGiaoDien.SanPham.FirstOrDefault(x => x.MaSanPham == maSanPham);
        return sanPham is null ? NotFound() : View(sanPham);
    }

}
