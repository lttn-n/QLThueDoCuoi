using QLThueDoCuoi.Repositories;
namespace QLThueDoCuoi.Services;
// Chi doc don cua khach; KHONG can thiep DonThueRepository/DonThueService cua nhan vien.
public sealed class DonThueKhachHangService(DiemDb db)
{
 public Task<List<Dictionary<string,object?>>> DanhSach(int tk)=>db.Query(@"SELECT d.MaDonThue,d.NgayDat,d.TrangThai,d.TenNguoiNhan,d.HinhThucNhan,
    COALESCE(SUM(ct.TienThue),0) AS TienThue,COALESCE(SUM(ct.TienCoc),0) AS TienCoc,
    d.SoTienGiam,d.PhiGiaoNhan
    FROM dbo.DonThue d LEFT JOIN dbo.ChiTietDonThue ct ON ct.MaDonThue=d.MaDonThue
    WHERE d.MaTaiKhoan=@tk GROUP BY d.MaDonThue,d.NgayDat,d.TrangThai,d.TenNguoiNhan,d.HinhThucNhan,
    d.SoTienGiam,d.PhiGiaoNhan ORDER BY d.NgayDat DESC",DiemDb.P("@tk",tk));
 public async Task<(Dictionary<string,object?>? Don,List<Dictionary<string,object?>> ChiTiet)> ChiTiet(int tk,int id){
    var orders=await db.Query(@"SELECT d.*, c.TenChiNhanh FROM dbo.DonThue d
         LEFT JOIN dbo.ChiNhanh c ON c.MaChiNhanh=d.MaChiNhanhNhan
         WHERE d.MaDonThue=@id AND d.MaTaiKhoan=@tk",DiemDb.P("@id",id),DiemDb.P("@tk",tk));
    if(orders.Count==0)return(null,[]);
    var items=await db.Query(@"SELECT ct.TenSanPhamKhiThue,ct.KichThuocKhiThue,ct.MauSacKhiThue,
        ct.NgayNhan,ct.NgayTraDuKien,ct.SoLuong,ct.TienThue,ct.TienCoc,ct.TrangThai,
        s.DuongDanAnh,ct.PhiTraTre,ct.PhiHuHong
        FROM dbo.ChiTietDonThue ct LEFT JOIN dbo.SanPham s ON s.MaSanPham=ct.MaSanPham
        WHERE ct.MaDonThue=@id ORDER BY ct.MaChiTietDonThue",DiemDb.P("@id",id));
    return (orders[0],items);
 }
}
