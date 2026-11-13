using QLThueDoCuoi.Repositories;
namespace QLThueDoCuoi.Services;
public sealed class KhuyenMaiQuanTriService(KhuyenMaiRepository repo)
{
    public Task<List<Dictionary<string,object?>>> DanhSach()=>repo.DanhSach(true);
    public Task<List<Dictionary<string,object?>>> ChiTiet(int id)=>repo.Tim(id);
    public Task<int> Luu(int? id,string ma,string ten,string loai,decimal giaTri,decimal? tran,
        decimal toiThieu,DateTime batDauUtc,DateTime ketThucUtc,int? soLuot,bool bat)
    {
        if(id is <=0 || string.IsNullOrWhiteSpace(ma)||string.IsNullOrWhiteSpace(ten)||ma.Length>40||ten.Length>200
           ||(loai!="PHANTRAM"&&loai!="SOTIEN")||giaTri<=0||(loai=="PHANTRAM"&&giaTri>100)
           ||toiThieu<0||(tran.HasValue&&tran.Value<=0)||(soLuot.HasValue&&soLuot.Value<=0)
           ||batDauUtc==default||ketThucUtc<=batDauUtc)
            throw new ArgumentException("Thông tin khuyến mãi không hợp lệ.");
        return repo.Luu(id,ma.Trim(),ten.Trim(),loai,giaTri,tran,toiThieu,batDauUtc,ketThucUtc,soLuot,bat);
    }
    public Task<int> BatTat(int id,bool bat)
    {
        if(id<=0)throw new ArgumentException("Khuyến mãi không hợp lệ.");
        return repo.BatTat(id,bat);
    }
}
