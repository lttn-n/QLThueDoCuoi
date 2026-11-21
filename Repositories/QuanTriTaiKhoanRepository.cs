namespace QLThueDoCuoi.Repositories;

// Repository chỉ truy cập bảng TaiKhoan cho khu quản trị tài khoản.
public sealed class QuanTriTaiKhoanRepository(DiemDb db)
{
    public Task<List<Dictionary<string, object?>>> DanhSach()
        => db.Query(
            @"SELECT MaTaiKhoan, HoTen, Email, SoDienThoai, VaiTro, TrangThai, NgayTao
              FROM dbo.TaiKhoan
              ORDER BY NgayTao DESC, MaTaiKhoan DESC");

    public Task<int> CapNhatVaiTro(int maTaiKhoan, string vaiTro)
        => db.Execute(
            "UPDATE dbo.TaiKhoan SET VaiTro=@vaiTro WHERE MaTaiKhoan=@ma",
            DiemDb.P("@vaiTro", vaiTro),
            DiemDb.P("@ma", maTaiKhoan));

    public Task<int> CapNhatTrangThai(int maTaiKhoan, bool trangThai)
        => db.Execute(
            "UPDATE dbo.TaiKhoan SET TrangThai=@trangThai WHERE MaTaiKhoan=@ma",
            DiemDb.P("@trangThai", trangThai),
            DiemDb.P("@ma", maTaiKhoan));

    public async Task<(string VaiTro, bool TrangThai)?> LayVaiTroTrangThai(int maTaiKhoan)
    {
        var rows = await db.Query(
            "SELECT VaiTro, TrangThai FROM dbo.TaiKhoan WHERE MaTaiKhoan=@ma",
            DiemDb.P("@ma", maTaiKhoan));
        if (rows.Count == 0)
            return null;

        return (
            Convert.ToString(rows[0]["VaiTro"]) ?? "",
            Convert.ToBoolean(rows[0]["TrangThai"]));
    }

    public async Task<int> DemAdminDangHoatDong()
    {
        var rows = await db.Query(
            "SELECT COUNT(*) AS SoLuong FROM dbo.TaiKhoan WHERE VaiTro=@vaiTro AND TrangThai=1",
            DiemDb.P("@vaiTro", "ADMIN"));
        return Convert.ToInt32(rows[0]["SoLuong"]);
    }
}
