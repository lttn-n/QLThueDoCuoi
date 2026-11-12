using Microsoft.Extensions.Caching.Memory;
using QLThueDoCuoi.Data;
using QLThueDoCuoi.Models;
using QLThueDoCuoi.Repositories;
using QLThueDoCuoi.ViewModels;
namespace QLThueDoCuoi.Services;

public interface ITrangChuService
{
    Task<TrangChuViewModel> LayTrangChuAsync();
    Task<IReadOnlyList<MenuDanhMucViewModel>> LayMenuAsync();
}

public sealed class TrangChuService(LoaiSanPhamRepository loaiRepo, SanPhamRepository sanPhamRepo,
    IMemoryCache boNho, ILogger<TrangChuService> log) : ITrangChuService
{
    static readonly Dictionary<string, string> MoTaLoai = new()
    {
        ["Váy cưới"] = "Dành cho khoảnh khắc duy nhất",
        ["Áo dài"] = "Nét duyên trong ngày chung đôi",
        ["Vest chú rể"] = "Lịch lãm cùng người thương",
        ["Phụ kiện"] = "Hoàn thiện vẻ đẹp của bạn"
    };

    public async Task<IReadOnlyList<MenuDanhMucViewModel>> LayMenuAsync() =>
        (await boNho.GetOrCreateAsync("menu-danh-muc", async e =>
        {
            e.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
            try
            {
                var loai = await loaiRepo.LayDanhSachHienThiAsync();
                return (IReadOnlyList<MenuDanhMucViewModel>)loai.Where(l => l.MaLoaiCha == null)
                    .Select(c => new MenuDanhMucViewModel(c, loai.Where(x => x.MaLoaiCha == c.MaLoaiSanPham).ToList())).ToList();
            }
            catch (Exception ex)
            {
                log.LogWarning(ex, "Không đọc được LoaiSanPham, dùng menu mẫu");
                e.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30);
                var con = DuLieuGiaoDien.SanPham.Select(s => s.TenLoai).Distinct()
                    .Select((t, i) => new LoaiSanPham { MaLoaiSanPham = i + 2, MaLoaiCha = 1, TenLoaiSanPham = t, HienThi = true }).ToList();
                return new[] { new MenuDanhMucViewModel(new LoaiSanPham { MaLoaiSanPham = 1, TenLoaiSanPham = "Trang phục cưới", HienThi = true }, con) };
            }
        }))!;

    public async Task<TrangChuViewModel> LayTrangChuAsync() =>
        (await boNho.GetOrCreateAsync("trang-chu", async e =>
        {
            e.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
            try
            {
                var loai = await loaiRepo.LayDanhSachHienThiAsync();
                var anh = await sanPhamRepo.LayAnhDaiDienTheoLoaiAsync();
                return new TrangChuViewModel
                {
                    DanhMuc = loai.Where(l => l.MaLoaiCha != null).Select(l => new DanhMucTrangChuViewModel(
                        l.MaLoaiSanPham, l.TenLoaiSanPham, MoTaLoai.GetValueOrDefault(l.TenLoaiSanPham, ""), anh.GetValueOrDefault(l.MaLoaiSanPham))).ToList(),
                    SanPhamNoiBat = await sanPhamRepo.LayNoiBatAsync(4)
                };
            }
            catch (Exception ex)
            {
                log.LogWarning(ex, "Không đọc được dữ liệu trang chủ từ SQL Server, dùng dữ liệu mẫu");
                e.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30);
                var mau = DuLieuGiaoDien.SanPham;
                return new TrangChuViewModel
                {
                    DanhMuc = mau.GroupBy(s => s.TenLoai).Select((g, i) => new DanhMucTrangChuViewModel(i + 2, g.Key, MoTaLoai.GetValueOrDefault(g.Key, ""), g.First().HinhAnh)).ToList(),
                    SanPhamNoiBat = mau
                };
            }
        }))!;
}
