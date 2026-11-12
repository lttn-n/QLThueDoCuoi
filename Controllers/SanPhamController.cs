using Microsoft.AspNetCore.Mvc;
using QLThueDoCuoi.Repositories;
using QLThueDoCuoi.ViewModels;

namespace QLThueDoCuoi.Controllers;

public class SanPhamController(SanPhamRepository repo, ILogger<SanPhamController> log) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        try { return View(await repo.LayDanhSachAsync()); }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            log.LogError(ex, "Không tải được danh sách sản phẩm");
            return View(new DanhSachSanPhamViewModel { Loi = "Không tải được danh sách sản phẩm lúc này. Vui lòng thử lại sau." });
        }
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var sp = await repo.LayChiTietAsync(id);
            return sp is null ? NotFound() : View(sp);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            log.LogError(ex, "Không tải được sản phẩm {Id}", id);
            return StatusCode(500);
        }
    }

    const int SoNgayLocToiDa = 4 + Services.GioThueService.SoNgayThemToiDa;
    static DateOnly HomNay => DateOnly.FromDateTime(DateTime.UtcNow.AddHours(7));

    static string? KiemTraKhoangNgay(DateOnly? tu, DateOnly? den, int toiDa)
    {
        if (tu is null || den is null) return "Vui lòng chọn cả ngày nhận và ngày trả.";
        if (tu < HomNay) return "Ngày nhận không được ở quá khứ.";
        if (den <= tu) return "Ngày trả phải sau ngày nhận.";
        if (den.Value.DayNumber - tu.Value.DayNumber > toiDa) return $"Thuê tối đa {toiDa} ngày.";
        return null;
    }

    [HttpGet]
    public async Task<IActionResult> ConHang(DateOnly? tu, DateOnly? den)
    {
        var loi = KiemTraKhoangNgay(tu, den, SoNgayLocToiDa);
        if (loi is not null) return BadRequest(new { loi });
        try
        {
            var ds = await repo.LayConHangAsync(tu!.Value, den!.Value);
            return Json(ds.GroupBy(x => x.Ten).Select(g => new { ten = g.Key, size = g.Select(x => x.KichThuoc).ToArray() }));
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            log.LogError(ex, "Không kiểm tra được sản phẩm còn trống theo ngày");
            return StatusCode(500, new { loi = "Chưa kiểm tra được ngày thuê lúc này. Vui lòng thử lại." });
        }
    }

    [HttpGet]
    public async Task<IActionResult> KiemTraNgay(int maSanPham, string? kichThuoc, DateOnly? tu, DateOnly? den)
    {
        var loi = KiemTraKhoangNgay(tu, den, 60);
        if (loi is not null) return BadRequest(new { loi });
        try
        {
            var kq = await repo.KiemTraConHangAsync(maSanPham, kichThuoc, tu!.Value, den!.Value);
            return kq is null ? NotFound(new { loi = "Mẫu hoặc kích thước này không còn cho thuê." }) : Json(new { conLai = kq.Value.ConLai, ten = kq.Value.Ten });
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            log.LogError(ex, "Không kiểm tra được ngày thuê của sản phẩm {Id}", maSanPham);
            return StatusCode(500, new { loi = "Chưa kiểm tra được ngày thuê lúc này." });
        }
    }
}
