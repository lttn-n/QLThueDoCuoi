using QLThueDoCuoi.Models;
namespace QLThueDoCuoi.ViewModels;

public sealed record DanhMucTrangChuViewModel(int MaLoaiSanPham, string TenLoai, string MoTa, string? HinhAnh);
public sealed record MenuDanhMucViewModel(LoaiSanPham Cha, IReadOnlyList<LoaiSanPham> Con);

public sealed class TrangChuViewModel
{
    public IReadOnlyList<DanhMucTrangChuViewModel> DanhMuc { get; init; } = Array.Empty<DanhMucTrangChuViewModel>();
    public IReadOnlyList<TheSanPhamViewModel> SanPhamNoiBat { get; init; } = Array.Empty<TheSanPhamViewModel>();
}
