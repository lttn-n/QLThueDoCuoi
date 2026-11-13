namespace QLThueDoCuoi.Repositories;
public sealed class KhuyenMaiRepository(DiemDb db)
{
 public Task<List<Dictionary<string,object?>>> DanhSach(bool admin=false)=>db.Query("SELECT * FROM dbo.KhuyenMai "+(admin?"":"WHERE TrangThai=1 AND NgayBatDau<=SYSUTCDATETIME() AND NgayKetThuc>=SYSUTCDATETIME() ")+"ORDER BY NgayBatDau DESC");
 public Task<List<Dictionary<string,object?>>> TimTheoMa(string ma) => db.Query(@"SELECT k.*, (SELECT COUNT(*) FROM dbo.DonThue d WHERE d.MaKhuyenMai=k.MaKhuyenMai AND d.TrangThai<>'DA_HUY') AS DaDung FROM dbo.KhuyenMai k WHERE k.MaUuDai=@ma",DiemDb.P("@ma",ma));
 public Task<List<Dictionary<string,object?>>> Tim(int id)=>db.Query("SELECT * FROM dbo.KhuyenMai WHERE MaKhuyenMai=@id",DiemDb.P("@id",id));
 public Task<int> Luu(int? id,string ma,string ten,string loai,decimal gia,decimal? max,decimal min,DateTime batDau,DateTime ketThuc,int? luot,bool bat)
 {
  string fields="MaUuDai=@ma,TenKhuyenMai=@ten,HinhThucGiam=@loai,GiaTriGiam=@gia,SoTienGiamToiDa=@max,GiaTriDonToiThieu=@min,NgayBatDau=@batdau,NgayKetThuc=@ketthuc,SoLuotSuDungToiDa=@luot,TrangThai=@bat";
  string sql=id.HasValue?$"UPDATE dbo.KhuyenMai SET {fields} WHERE MaKhuyenMai=@id":$"INSERT dbo.KhuyenMai(MaUuDai,TenKhuyenMai,HinhThucGiam,GiaTriGiam,SoTienGiamToiDa,GiaTriDonToiThieu,NgayBatDau,NgayKetThuc,SoLuotSuDungToiDa,TrangThai) VALUES(@ma,@ten,@loai,@gia,@max,@min,@batdau,@ketthuc,@luot,@bat)";
  return db.Execute(sql,DiemDb.P("@id",id),DiemDb.P("@ma",ma),DiemDb.P("@ten",ten),DiemDb.P("@loai",loai),DiemDb.P("@gia",gia),DiemDb.P("@max",max),DiemDb.P("@min",min),DiemDb.P("@batdau",batDau),DiemDb.P("@ketthuc",ketThuc),DiemDb.P("@luot",luot),DiemDb.P("@bat",bat));
 }
 public Task<int> BatTat(int id,bool bat)=>db.Execute("UPDATE dbo.KhuyenMai SET TrangThai=@bat WHERE MaKhuyenMai=@id",DiemDb.P("@bat",bat),DiemDb.P("@id",id));
}
