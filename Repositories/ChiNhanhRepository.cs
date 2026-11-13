namespace QLThueDoCuoi.Repositories;
public sealed class ChiNhanhRepository(DiemDb db)
{
 public Task<List<Dictionary<string,object?>>> DanhSach(bool admin=false)=>db.Query("SELECT * FROM dbo.ChiNhanh "+(admin?"":"WHERE TrangThai=1 ")+"ORDER BY MaChiNhanh");
 public Task<int> Luu(int? id,string ten,string diaChi,string? sdt,bool bat)
 =>db.Execute(id.HasValue?"UPDATE dbo.ChiNhanh SET TenChiNhanh=@ten,DiaChi=@dc,SoDienThoai=@sdt,TrangThai=@bat WHERE MaChiNhanh=@id":"INSERT dbo.ChiNhanh(TenChiNhanh,DiaChi,SoDienThoai,TrangThai) VALUES(@ten,@dc,@sdt,@bat)",DiemDb.P("@id",id),DiemDb.P("@ten",ten),DiemDb.P("@dc",diaChi),DiemDb.P("@sdt",sdt),DiemDb.P("@bat",bat));
 public Task<int> BatTat(int id,bool bat)=>db.Execute("UPDATE dbo.ChiNhanh SET TrangThai=@bat WHERE MaChiNhanh=@id",DiemDb.P("@bat",bat),DiemDb.P("@id",id));
}
