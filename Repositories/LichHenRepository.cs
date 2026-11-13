using QLThueDoCuoi.Repositories;
namespace QLThueDoCuoi.Repositories;
public sealed class LichHenRepository(DiemDb db)
{
    public Task<List<Dictionary<string, object?>>> CuaToi(int tk)
    => db.Query(@"
        SELECT
            h.*,
            s.TenSanPham,
            s.DuongDanAnh AS AnhSanPham,
            c.TenChiNhanh
        FROM dbo.LichHenThuDo h
        JOIN dbo.SanPham s
            ON s.MaSanPham = h.MaSanPham
        JOIN dbo.ChiNhanh c
            ON c.MaChiNhanh = h.MaChiNhanh
        WHERE h.MaTaiKhoan = @tk
        ORDER BY h.ThoiDiemHen DESC",
        DiemDb.P("@tk", tk));
    public Task<List<Dictionary<string,object?>>> ChiTiet(int tk,int id) => db.Query(@"SELECT h.*,s.TenSanPham,c.TenChiNhanh,c.DiaChi
        FROM dbo.LichHenThuDo h JOIN dbo.SanPham s ON s.MaSanPham=h.MaSanPham
        JOIN dbo.ChiNhanh c ON c.MaChiNhanh=h.MaChiNhanh
        WHERE h.MaTaiKhoan=@tk AND h.MaLichHen=@id",DiemDb.P("@tk",tk),DiemDb.P("@id",id));
    public async Task<bool> HopLeSanPhamVaChiNhanh(int sp,int cn) {
        var r=await db.Query(@"SELECT TOP 1 s.MaSanPham FROM dbo.SanPham s CROSS JOIN dbo.ChiNhanh c
            WHERE s.MaSanPham=@sp AND c.MaChiNhanh=@cn AND s.TrangThai=1 AND c.TrangThai=1",
            DiemDb.P("@sp",sp),DiemDb.P("@cn",cn));return r.Count!=0;
    }
    public async Task<bool> TrungLich(int tk,int sp,DateTime utc) {
        var r=await db.Query(@"SELECT TOP 1 MaLichHen FROM dbo.LichHenThuDo
          WHERE MaTaiKhoan=@tk AND MaSanPham=@sp AND ThoiDiemHen=@utc AND TrangThai<>'DA_HUY'",
          DiemDb.P("@tk",tk),DiemDb.P("@sp",sp),DiemDb.P("@utc",utc));return r.Count!=0;
    }
    public Task<int> Tao(int tk,int sp,int cn,string ten,string sdt,DateTime utc,string? ghiChu) => db.Execute(@"INSERT dbo.LichHenThuDo
        (MaTaiKhoan,MaSanPham,MaChiNhanh,HoTenLienHe,SoDienThoaiLienHe,ThoiDiemHen,GhiChu)
        VALUES(@tk,@sp,@cn,@ten,@sdt,@utc,@gc)",DiemDb.P("@tk",tk),DiemDb.P("@sp",sp),DiemDb.P("@cn",cn),
        DiemDb.P("@ten",ten),DiemDb.P("@sdt",sdt),DiemDb.P("@utc",utc),DiemDb.P("@gc",ghiChu));
    public Task<int> Huy(int tk,int id,string lyDo)=>db.Execute(@"UPDATE dbo.LichHenThuDo
        SET TrangThai='DA_HUY',ThoiDiemHuy=SYSUTCDATETIME(),MaTaiKhoanHuy=@tk,
            GhiChu=CASE WHEN NULLIF(LTRIM(RTRIM(GhiChu)), '') IS NULL THEN @lydo ELSE CONCAT(GhiChu, N' | Lý do hủy: ', @lydo) END WHERE MaTaiKhoan=@tk AND MaLichHen=@id
            AND TrangThai IN ('CHO_XAC_NHAN','DA_XAC_NHAN')",
        DiemDb.P("@tk",tk),DiemDb.P("@id",id),DiemDb.P("@lydo",lyDo));
}
