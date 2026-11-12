using QLThueDoCuoi.Repositories;
using QLThueDoCuoi.ViewModels;
namespace QLThueDoCuoi.Services;

public sealed record KetQuaGio(bool ThanhCong, string? Loi = null, int Tong = 0, string? Ten = null);

public interface IGioThueService
{
    Task<GioThueViewModel> LayAsync(int maTaiKhoan);
    Task<KetQuaGio> ThemAsync(int maTaiKhoan, ThemGioInput vao);
    Task<KetQuaGio> CapNhatAsync(int maTaiKhoan, IReadOnlyList<DongGioInput> dong);
    Task<bool> XoaAsync(int maTaiKhoan, int maChiTiet);
    Task<int> XoaNhieuAsync(int maTaiKhoan, IReadOnlyCollection<int> dsMaChiTiet);
    Task XoaTatCaAsync(int maTaiKhoan);
}

public sealed class GioThueService(GioThueRepository repo, ILogger<GioThueService> log) : IGioThueService
{
    const int SoLuongToiDa = 10;
    public const int SoNgayThemToiDa = 7;

    public async Task<GioThueViewModel> LayAsync(int maTaiKhoan) => new() { Dong = await repo.LayGioAsync(maTaiKhoan) };

    public async Task<KetQuaGio> ThemAsync(int maTaiKhoan, ThemGioInput vao)
    {
        if (vao.MaSanPham <= 0) return new(false, "Không xác định được sản phẩm.");
        if (string.IsNullOrWhiteSpace(vao.KichThuoc)) return new(false, "Vui lòng chọn kích thước.");
        if (vao.SoLuong < 1 || vao.SoLuong > SoLuongToiDa) return new(false, $"Số lượng từ 1 đến {SoLuongToiDa}.");
        var goi = (await repo.LaySoNgayGoiAsync(new[] { vao.MaSanPham })).GetValueOrDefault(vao.MaSanPham, 3);
        var loiNgay = KiemTraNgay(vao.NgayNhan, vao.NgayTra, batBuoc: true, goi);
        if (loiNgay is not null) return new(false, loiNgay);

        var kq = await repo.ThemAsync(maTaiKhoan, vao.MaSanPham, vao.KichThuoc.Trim(), vao.MauSac?.Trim(), vao.SoLuong, vao.NgayNhan, vao.NgayTra);
        if (!kq.ThanhCong) return new(false, kq.Loi);
        log.LogInformation("Thêm vào giỏ thuê: tài khoản {Ma}, sản phẩm {Sp} x{Sl}", maTaiKhoan, vao.MaSanPham, vao.SoLuong);
        return new(true, null, await repo.DemSanPhamAsync(maTaiKhoan), kq.Ten);
    }

    public async Task<KetQuaGio> CapNhatAsync(int maTaiKhoan, IReadOnlyList<DongGioInput> dong)
    {
        if (dong.Count == 0) return new(false, "Giỏ thuê đang trống.");
        var goiTheoSp = await repo.LaySoNgayGoiAsync(dong.Select(x => x.MaSanPham));
        for (var i = 0; i < dong.Count; i++)
        {
            var d = dong[i];
            if (d.SoLuong < 1 || d.SoLuong > SoLuongToiDa) return new(false, $"Sản phẩm thứ {i + 1}: số lượng từ 1 đến {SoLuongToiDa}.");
            var loi = KiemTraNgay(d.NgayNhan, d.NgayTra, batBuoc: false, goiTheoSp.GetValueOrDefault(d.MaSanPham, 3));
            if (loi is not null) return new(false, $"Sản phẩm thứ {i + 1}: {loi[..1].ToLowerInvariant()}{loi[1..]}");
        }
        var loiDb = await repo.CapNhatAsync(maTaiKhoan, dong);
        if (loiDb is not null) return new(false, loiDb);
        log.LogInformation("Cập nhật giỏ thuê tài khoản {Ma} ({So} dòng)", maTaiKhoan, dong.Count);
        return new(true, null, await repo.DemSanPhamAsync(maTaiKhoan));
    }

    public async Task<bool> XoaAsync(int maTaiKhoan, int maChiTiet)
    {
        var ok = await repo.XoaAsync(maTaiKhoan, maChiTiet);
        if (ok) log.LogInformation("Xóa dòng {Ct} khỏi giỏ thuê tài khoản {Ma}", maChiTiet, maTaiKhoan);
        return ok;
    }

    public async Task<int> XoaNhieuAsync(int maTaiKhoan, IReadOnlyCollection<int> dsMaChiTiet)
    {
        var n = await repo.XoaNhieuAsync(maTaiKhoan, dsMaChiTiet);
        if (n > 0) log.LogInformation("Xóa {N} dòng khỏi giỏ thuê tài khoản {Ma}", n, maTaiKhoan);
        return n;
    }

    public Task XoaTatCaAsync(int maTaiKhoan) => repo.XoaTatCaAsync(maTaiKhoan);

    static string? KiemTraNgay(DateOnly? nhan, DateOnly? tra, bool batBuoc, int soNgayGoi)
    {
        if (nhan is null && tra is null) return batBuoc ? "Vui lòng chọn ngày nhận và ngày trả." : null;
        if (nhan is null || tra is null) return "Vui lòng chọn đủ ngày nhận và ngày trả.";
        var homNay = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(7));
        if (nhan < homNay) return "Ngày nhận không được ở quá khứ.";
        if (tra < homNay) return "Ngày trả không được ở quá khứ.";
        if (tra <= nhan) return "Ngày trả phải sau ngày nhận.";
        var toiDa = soNgayGoi + SoNgayThemToiDa;
        if (tra.Value.DayNumber - nhan.Value.DayNumber > toiDa)
            return $"Thời gian thuê tối đa {toiDa} ngày (gói {soNgayGoi} ngày + {SoNgayThemToiDa} ngày thêm, mỗi ngày thêm tính phí).";
        return null;
    }
}
