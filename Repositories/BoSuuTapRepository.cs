namespace QLThueDoCuoi.Repositories;
public sealed class BoSuuTapRepository(DiemDb db)
{
    public Task<List<Dictionary<string,object?>>> DanhSach() => db.Query(@"SELECT b.MaBoSuuTap, b.TenBoSuuTap, b.MoTa,
      COALESCE(NULLIF(b.DuongDanAnh, ''),
        (SELECT TOP 1 s.DuongDanAnh FROM dbo.BoSuuTapSanPham bs
         JOIN dbo.SanPham s ON s.MaSanPham=bs.MaSanPham
         WHERE bs.MaBoSuuTap=b.MaBoSuuTap AND s.TrangThai=1 AND s.DuongDanAnh IS NOT NULL
         ORDER BY s.MaSanPham)) AS DuongDanAnh,
      (SELECT COUNT(*) FROM dbo.BoSuuTapSanPham bs JOIN dbo.SanPham s ON s.MaSanPham=bs.MaSanPham
       WHERE bs.MaBoSuuTap=b.MaBoSuuTap AND s.TrangThai=1) AS SoSanPham
      FROM dbo.BoSuuTap b WHERE b.HienThi=1 ORDER BY b.MaBoSuuTap DESC");
    public async Task<bool> TonTai(int id) => (await db.Query("SELECT MaBoSuuTap FROM dbo.BoSuuTap WHERE MaBoSuuTap=@id AND HienThi=1",DiemDb.P("@id",id))).Count>0;
    public Task<List<Dictionary<string,object?>>> SanPham(int id) => db.Query(@"SELECT s.MaSanPham,s.TenSanPham,s.DuongDanAnh,s.GiaThueGoi,s.KichThuoc,s.MauSac
      FROM dbo.BoSuuTapSanPham bs JOIN dbo.SanPham s ON s.MaSanPham=bs.MaSanPham
      JOIN dbo.BoSuuTap b ON b.MaBoSuuTap=bs.MaBoSuuTap
      WHERE bs.MaBoSuuTap=@id AND b.HienThi=1 AND s.TrangThai=1 ORDER BY s.MaSanPham",DiemDb.P("@id",id));
}
