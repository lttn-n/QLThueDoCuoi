namespace QLThueDoCuoi.ViewModels;
public sealed record TheSanPhamViewModel(int MaSanPham, string TenSanPham,
    string TenLoai, decimal GiaThue, int SoNgayTrongGoi, string HinhAnh,
    string Nhan, string KichThuoc, decimal? GiaGoc = null, string? HinhAnh2 = null);

public sealed class ChiTietSanPhamViewModel
{
    public int MaSanPham { get; init; }
    public string Ten { get; init; } = "";
    public string TenLoai { get; init; } = "";
    public string? MoTa { get; init; }
    public string? ChatLieu { get; init; }
    public string MauSac { get; init; } = "";
    public string Anh { get; init; } = "";
    public List<string> AnhPhu { get; init; } = new();
    public decimal GiaThue { get; init; }
    public decimal? GiaGoc { get; init; }
    public int SoNgayGoi { get; init; } = 3;
    public decimal GiaNgayThem { get; init; }
    public decimal TienCoc { get; init; }
    public string Nhan { get; init; } = "";
    public List<LuaChonSize> CacSize { get; init; } = new();
    public int SoNgayToiDa => SoNgayGoi + Services.GioThueService.SoNgayThemToiDa;
}

public sealed class DanhSachSanPhamViewModel
{
    public List<TheSanPhamViewModel> SanPham { get; init; } = new();
    public List<string> CacLoai { get; init; } = new();
    public List<string> CacSize { get; init; } = new();
    public string? Loi { get; init; }
}
