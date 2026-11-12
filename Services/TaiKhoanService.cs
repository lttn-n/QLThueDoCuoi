using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Memory;
using QLThueDoCuoi.Models;
using QLThueDoCuoi.Repositories;
using QLThueDoCuoi.ViewModels;
namespace QLThueDoCuoi.Services;

public sealed record KetQuaTaiKhoan(TaiKhoan? TaiKhoan, string? Truong, string? Loi)
{
    public bool ThanhCong => Loi is null;
    public static KetQuaTaiKhoan Ok(TaiKhoan tk) => new(tk, null, null);
    public static KetQuaTaiKhoan ThatBai(string loi, string? truong = null) => new(null, truong, loi);
}

public interface ITaiKhoanService
{
    Task<TaiKhoan?> LayAsync(int maTaiKhoan);
    Task<KetQuaTaiKhoan> DangKyAsync(DangKyViewModel vm);
    Task<KetQuaTaiKhoan> DangNhapAsync(string tenDangNhap, string matKhau);
    Task<KetQuaTaiKhoan> CapNhatHoSoAsync(int maTaiKhoan, HoSoViewModel vm);
    Task<KetQuaTaiKhoan> DoiMatKhauAsync(int maTaiKhoan, DoiMatKhauViewModel vm);
    Task<KetQuaTaiKhoan> DatLaiMatKhauAsync(DatLaiMatKhauViewModel vm);
}

public sealed class TaiKhoanService(TaiKhoanRepository repo, IPasswordHasher<TaiKhoan> hasher,
    IMemoryCache boNho, ILogger<TaiKhoanService> log) : ITaiKhoanService
{
    const int SoLanSaiToiDa = 5;
    static readonly TimeSpan ThoiGianKhoa = TimeSpan.FromMinutes(10);
    const string LoiLienLac = "Nhập email hợp lệ hoặc số điện thoại Việt Nam (10 số, bắt đầu bằng 0).";

    public Task<TaiKhoan?> LayAsync(int maTaiKhoan) => repo.LayTheoMaAsync(maTaiKhoan);

    public async Task<KetQuaTaiKhoan> DangKyAsync(DangKyViewModel vm)
    {
        var (email, sdt) = LienLac.PhanTich(vm.TenDangNhap);
        if (email is null && sdt is null) return KetQuaTaiKhoan.ThatBai(LoiLienLac, nameof(vm.TenDangNhap));

        var (trungEmail, trungSdt) = await repo.KiemTraTrungAsync(email, sdt);
        if (trungEmail || trungSdt) return KetQuaTaiKhoan.ThatBai(
            trungEmail ? "Email này đã được đăng ký. Bạn hãy đăng nhập nhé." : "Số điện thoại này đã được đăng ký. Bạn hãy đăng nhập nhé.",
            nameof(vm.TenDangNhap));

        var tk = new TaiKhoan { HoTen = vm.HoTen.Trim(), Email = email, SoDienThoai = sdt, VaiTro = "KHACHHANG", TrangThai = true };
        tk.MatKhauHash = hasher.HashPassword(tk, vm.MatKhau);
        try
        {
            tk.MaTaiKhoan = await repo.TaoAsync(tk);
        }
        catch (SqlException ex) when (ex.Number is 2601 or 2627)
        {
            return KetQuaTaiKhoan.ThatBai("Email hoặc số điện thoại này đã được đăng ký.", nameof(vm.TenDangNhap));
        }
        log.LogInformation("Đăng ký tài khoản khách hàng mới: {Ma}", tk.MaTaiKhoan);
        tk.NgayTao = DateTime.UtcNow;
        return KetQuaTaiKhoan.Ok(tk);
    }

    public async Task<KetQuaTaiKhoan> DangNhapAsync(string tenDangNhap, string matKhau)
    {
        var (email, sdt) = LienLac.PhanTich(tenDangNhap);
        var khoaLienLac = email ?? sdt;
        if (khoaLienLac is null) return KetQuaTaiKhoan.ThatBai(LoiLienLac, "TenDangNhap");
        var khoaDem = "dn-sai:" + khoaLienLac;

        if (boNho.TryGetValue(khoaDem, out int soLanSai) && soLanSai >= SoLanSaiToiDa)
        {
            log.LogWarning("Khóa tạm đăng nhập vì sai nhiều lần: {Khoa}", khoaDem);
            return KetQuaTaiKhoan.ThatBai("Bạn đã nhập sai quá nhiều lần. Vui lòng thử lại sau 10 phút.", "MatKhau");
        }

        var tk = await repo.LayTheoLienLacAsync(khoaLienLac);
        if (tk is null)
        {
            GhiNhanSai(khoaDem);
            return KetQuaTaiKhoan.ThatBai(email is not null
                ? "Email này chưa được đăng ký tài khoản. Bạn hãy kiểm tra lại hoặc đăng ký mới."
                : "Số điện thoại này chưa được đăng ký tài khoản. Bạn hãy kiểm tra lại hoặc đăng ký mới.", "TenDangNhap");
        }

        PasswordVerificationResult kq;
        try { kq = hasher.VerifyHashedPassword(tk, tk.MatKhauHash, matKhau); }
        catch (FormatException) { kq = PasswordVerificationResult.Failed; }
        if (kq == PasswordVerificationResult.Failed)
        {
            GhiNhanSai(khoaDem);
            return KetQuaTaiKhoan.ThatBai("Mật khẩu bạn nhập không đúng. Vui lòng thử lại hoặc chọn \"Quên mật khẩu?\".", "MatKhau");
        }

        if (kq == PasswordVerificationResult.SuccessRehashNeeded)
            await repo.CapNhatMatKhauAsync(tk.MaTaiKhoan, hasher.HashPassword(tk, matKhau));

        if (!tk.TrangThai)
        {
            log.LogWarning("Tài khoản bị khóa cố đăng nhập: {Ma}", tk.MaTaiKhoan);
            return KetQuaTaiKhoan.ThatBai("Tài khoản này đang bị khóa. Vui lòng liên hệ cửa hàng để được hỗ trợ.", "TenDangNhap");
        }

        boNho.Remove(khoaDem);
        log.LogInformation("Đăng nhập thành công: {Ma} ({VaiTro})", tk.MaTaiKhoan, tk.VaiTro);
        return KetQuaTaiKhoan.Ok(tk);
    }

    public async Task<KetQuaTaiKhoan> CapNhatHoSoAsync(int maTaiKhoan, HoSoViewModel vm)
    {
        string? email = null, sdt = null;
        if (!string.IsNullOrWhiteSpace(vm.Email))
        {
            email = LienLac.ChuanHoaEmail(vm.Email);
            if (email is null) return KetQuaTaiKhoan.ThatBai("Email chưa đúng định dạng.", nameof(vm.Email));
        }
        if (!string.IsNullOrWhiteSpace(vm.SoDienThoai))
        {
            sdt = LienLac.ChuanHoaSdt(vm.SoDienThoai);
            if (sdt is null) return KetQuaTaiKhoan.ThatBai("Số điện thoại không hợp lệ (10 số, bắt đầu bằng 0).", nameof(vm.SoDienThoai));
        }
        if (email is null && sdt is null)
            return KetQuaTaiKhoan.ThatBai("Cần có ít nhất email hoặc số điện thoại để cửa hàng liên lạc.", nameof(vm.Email));

        var (trungEmail, trungSdt) = await repo.KiemTraTrungAsync(email, sdt, maTaiKhoan);
        if (trungEmail) return KetQuaTaiKhoan.ThatBai("Email này đang được tài khoản khác sử dụng.", nameof(vm.Email));
        if (trungSdt) return KetQuaTaiKhoan.ThatBai("Số điện thoại này đang được tài khoản khác sử dụng.", nameof(vm.SoDienThoai));

        var diaChi = string.IsNullOrWhiteSpace(vm.DiaChi) ? null : vm.DiaChi.Trim();
        try { await repo.CapNhatHoSoAsync(maTaiKhoan, vm.HoTen.Trim(), email, sdt, diaChi); }
        catch (SqlException ex) when (ex.Number is 2601 or 2627)
        {
            return KetQuaTaiKhoan.ThatBai("Email hoặc số điện thoại này đang được tài khoản khác sử dụng.", nameof(vm.Email));
        }
        log.LogInformation("Cập nhật hồ sơ tài khoản {Ma}", maTaiKhoan);
        var tk = await repo.LayTheoMaAsync(maTaiKhoan);
        return tk is null ? KetQuaTaiKhoan.ThatBai("Không tìm thấy tài khoản.") : KetQuaTaiKhoan.Ok(tk);
    }

    public async Task<KetQuaTaiKhoan> DoiMatKhauAsync(int maTaiKhoan, DoiMatKhauViewModel vm)
    {
        var tk = await repo.LayTheoMaAsync(maTaiKhoan);
        if (tk is null) return KetQuaTaiKhoan.ThatBai("Không tìm thấy tài khoản.");

        var khoaDem = "dmk-sai:" + maTaiKhoan;
        if (boNho.TryGetValue(khoaDem, out int soLanSai) && soLanSai >= SoLanSaiToiDa)
            return KetQuaTaiKhoan.ThatBai("Bạn đã nhập sai mật khẩu hiện tại quá nhiều lần. Vui lòng thử lại sau 10 phút.", nameof(vm.MatKhauCu));

        PasswordVerificationResult kq;
        try { kq = hasher.VerifyHashedPassword(tk, tk.MatKhauHash, vm.MatKhauCu); }
        catch (FormatException) { kq = PasswordVerificationResult.Failed; }
        if (kq == PasswordVerificationResult.Failed)
        {
            GhiNhanSai(khoaDem);
            return KetQuaTaiKhoan.ThatBai("Mật khẩu hiện tại không đúng.", nameof(vm.MatKhauCu));
        }
        if (vm.MatKhauMoi == vm.MatKhauCu)
            return KetQuaTaiKhoan.ThatBai("Mật khẩu mới cần khác mật khẩu hiện tại.", nameof(vm.MatKhauMoi));

        boNho.Remove(khoaDem);
        await repo.CapNhatMatKhauAsync(maTaiKhoan, hasher.HashPassword(tk, vm.MatKhauMoi));
        log.LogInformation("Đổi mật khẩu tài khoản {Ma}", maTaiKhoan);
        return KetQuaTaiKhoan.Ok(tk);
    }

    public async Task<KetQuaTaiKhoan> DatLaiMatKhauAsync(DatLaiMatKhauViewModel vm)
    {
        var (email, sdt) = LienLac.PhanTich(vm.TenDangNhap);
        var khoaLienLac = email ?? sdt;
        if (khoaLienLac is null) return KetQuaTaiKhoan.ThatBai(LoiLienLac, nameof(vm.TenDangNhap));

        var khoaDem = "dlmk:" + khoaLienLac;
        if (boNho.TryGetValue(khoaDem, out int soLan) && soLan >= SoLanSaiToiDa)
            return KetQuaTaiKhoan.ThatBai("Bạn đã thử quá nhiều lần. Vui lòng quay lại sau 10 phút.", nameof(vm.TenDangNhap));
        GhiNhanSai(khoaDem);

        var tk = await repo.LayTheoLienLacAsync(khoaLienLac);
        if (tk is null || tk.VaiTro != "KHACHHANG")
            return KetQuaTaiKhoan.ThatBai("Không tìm thấy tài khoản khách hàng với email/số điện thoại này.", nameof(vm.TenDangNhap));
        if (!tk.TrangThai)
            return KetQuaTaiKhoan.ThatBai("Tài khoản này đang bị khóa. Vui lòng liên hệ cửa hàng để được hỗ trợ.", nameof(vm.TenDangNhap));

        await repo.CapNhatMatKhauAsync(tk.MaTaiKhoan, hasher.HashPassword(tk, vm.MatKhauMoi));
        boNho.Remove(khoaDem);
        boNho.Remove("dn-sai:" + khoaLienLac);
        log.LogInformation("Đặt lại mật khẩu tài khoản {Ma}", tk.MaTaiKhoan);
        return KetQuaTaiKhoan.Ok(tk);
    }

    void GhiNhanSai(string khoa)
    {
        boNho.TryGetValue(khoa, out int n);
        boNho.Set(khoa, n + 1, ThoiGianKhoa);
    }
}

public static class LienLac
{
    static readonly Regex EmailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);
    static readonly Regex SdtRegex = new(@"^0\d{9}$", RegexOptions.Compiled);

    public static string? ChuanHoaEmail(string? s)
    {
        s = s?.Trim();
        if (string.IsNullOrEmpty(s) || s.Length > 254 || !EmailRegex.IsMatch(s)) return null;
        return s.ToLowerInvariant();
    }

    public static string? ChuanHoaSdt(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return null;
        var d = new string(s.Where(c => char.IsDigit(c) || c == '+').ToArray());
        if (d.StartsWith("+84")) d = "0" + d[3..];
        else if (d.StartsWith("84") && d.Length == 11) d = "0" + d[2..];
        return SdtRegex.IsMatch(d) ? d : null;
    }

    public static (string? Email, string? Sdt) PhanTich(string? s)
    {
        s = s?.Trim() ?? "";
        return s.Contains('@') ? (ChuanHoaEmail(s), null) : (null, ChuanHoaSdt(s));
    }
}
