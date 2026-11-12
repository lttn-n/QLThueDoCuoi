using Microsoft.AspNetCore.Mvc;
using QLThueDoCuoi.Services;
namespace QLThueDoCuoi.ViewComponents;

public class MenuChinhViewComponent(ITrangChuService trangChu) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync() => View(await trangChu.LayMenuAsync());
}
