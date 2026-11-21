using QLThueDoCuoi.Repositories;

namespace QLThueDoCuoi.Services;

// Service kiểm tra nghiệp vụ: controller không truy vấn SQL trực tiếp.
public sealed class QuanTriTaiKhoanService(QuanTriTaiKhoanRepository repo)
{
    static readonly string[] VaiTroHopLe = ["ADMIN", "NHANVIEN", "KHACHHANG"];

    public Task<List<Dictionary<string, object?>>> DanhSach()
        => repo.DanhSach();

    public async Task<int> CapNhatVaiTro(int maTaiKhoan, string vaiTro, int maTaiKhoanDangNhap)
    {
        if (maTaiKhoan <= 0)
            throw new ArgumentException("Tài khoản không hợp lệ.");
        if (!VaiTroHopLe.Contains(vaiTro))
            throw new ArgumentException("Vai trò không hợp lệ.");
        if (maTaiKhoan == maTaiKhoanDangNhap)
            throw new InvalidOperationException("Không thể tự thay đổi vai trò của chính mình.");

        var taiKhoan = await repo.LayVaiTroTrangThai(maTaiKhoan);
        if (taiKhoan is null)
            throw new ArgumentException("Không tìm thấy tài khoản.");

        if (taiKhoan.Value.VaiTro == "ADMIN"
            && vaiTro != "ADMIN"
            && taiKhoan.Value.TrangThai
            && await repo.DemAdminDangHoatDong() <= 1)
        {
            throw new InvalidOperationException("Không thể hạ quyền: hệ thống cần ít nhất một tài khoản ADMIN hoạt động.");
        }

        return await repo.CapNhatVaiTro(maTaiKhoan, vaiTro);
    }

    public async Task<int> CapNhatTrangThai(int maTaiKhoan, bool trangThai, int maTaiKhoanDangNhap)
    {
        if (maTaiKhoan <= 0)
            throw new ArgumentException("Tài khoản không hợp lệ.");
        // Chặn khóa chính tài khoản đang đăng nhập để tránh mất toàn bộ phiên Admin.
        if (!trangThai && maTaiKhoan == maTaiKhoanDangNhap)
            throw new InvalidOperationException("Không thể khóa tài khoản đang đăng nhập.");

        var taiKhoan = await repo.LayVaiTroTrangThai(maTaiKhoan);
        if (taiKhoan is null)
            throw new ArgumentException("Không tìm thấy tài khoản.");

        // Luôn giữ ít nhất một tài khoản ADMIN đang hoạt động.
        if (!trangThai
            && taiKhoan.Value.VaiTro == "ADMIN"
            && taiKhoan.Value.TrangThai
            && await repo.DemAdminDangHoatDong() <= 1)
        {
            throw new InvalidOperationException("Không thể khóa: hệ thống cần ít nhất một tài khoản ADMIN hoạt động.");
        }

        return await repo.CapNhatTrangThai(maTaiKhoan, trangThai);
    }
}
