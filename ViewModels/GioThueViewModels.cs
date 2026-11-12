using System.ComponentModel.DataAnnotations;
namespace QLThueDoCuoi.ViewModels;

public static class TinhTien
{
    public static decimal Thue(int soLuong, decimal giaGoi, int soNgayGoi, decimal giaNgayThem, int? soNgay)
    {
        var them = soNgay is int n ? Math.Max(0, n - soNgayGoi) : 0;
        return soLuong * (giaGoi + them * giaNgayThem);
    }
}

public sealed record LuaChonSize(int MaSanPham, string KichThuoc);

public sealed class DongGioViewModel
{
    public int MaChiTiet { get; init; }
    public int MaSanPham { get; init; }
    public string Ten { get; init; } = "";
    public string KichThuoc { get; init; } = "";
    public string MauSac { get; init; } = "";
    public string? Anh { get; init; }
    public int SoLuong { get; init; }
    public DateOnly? NgayNhan { get; init; }
    public DateOnly? NgayTra { get; init; }
    public decimal GiaGoi { get; init; }
    public int SoNgayGoi { get; init; }
    public decimal GiaNgayThem { get; init; }
    public decimal TienCoc { get; init; }
    public bool NgungThue { get; init; }
    public int SoNgayToiDa => SoNgayGoi + Services.GioThueService.SoNgayThemToiDa;
    public List<LuaChonSize> CacSize { get; set; } = new();

    public int? SoNgay => NgayNhan is { } a && NgayTra is { } b ? b.DayNumber - a.DayNumber : null;
    public decimal TienThue => TinhTien.Thue(SoLuong, GiaGoi, SoNgayGoi, GiaNgayThem, SoNgay);
    public decimal TongCoc => TienCoc * SoLuong;
}

public sealed class GioThueViewModel
{
    public List<DongGioViewModel> Dong { get; init; } = new();
    public bool Rong => Dong.Count == 0;
    public int TongSoLuong => Dong.Sum(d => d.SoLuong);
    public decimal TongThue => Dong.Sum(d => d.TienThue);
    public decimal TongCoc => Dong.Sum(d => d.TongCoc);
    public decimal TongBanDau => TongThue + TongCoc;
    public bool CoDongKhongHopLe => Dong.Any(d => d.NgungThue || d.SoNgay is null);
}

public sealed class ThemGioInput
{
    public int MaSanPham { get; set; }
    public string? KichThuoc { get; set; }
    public string? MauSac { get; set; }
    [Range(1, 10)] public int SoLuong { get; set; } = 1;
    public DateOnly? NgayNhan { get; set; }
    public DateOnly? NgayTra { get; set; }
}

public sealed class DongGioInput
{
    public int MaChiTiet { get; set; }
    public int MaSanPham { get; set; }
    public int SoLuong { get; set; }
    public DateOnly? NgayNhan { get; set; }
    public DateOnly? NgayTra { get; set; }
}
