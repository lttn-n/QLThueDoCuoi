using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using QLThueDoCuoi.Repositories;
namespace QLThueDoCuoi.ViewComponents;

public class GioThueTomTatViewComponent(GioThueRepository gioThue, ILogger<GioThueTomTatViewComponent> log) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync()
    {
        var so = 0;
        if (UserClaimsPrincipal.Identity?.IsAuthenticated == true
            && int.TryParse(UserClaimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier), out var maTaiKhoan))
        {
            try { so = await gioThue.DemSanPhamAsync(maTaiKhoan); }
            catch (Exception ex) { log.LogWarning(ex, "Không đếm được giỏ thuê"); }
        }
        return View(so);
    }
}
