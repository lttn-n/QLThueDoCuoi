using QLThueDoCuoi.Repositories;
namespace QLThueDoCuoi.Services;
// Admin service: keeps controller independent of SQL repository.
public sealed class ChiNhanhQuanTriService(ChiNhanhRepository repo)
{
    public Task<List<Dictionary<string,object?>>> DanhSach()=>repo.DanhSach(true);
    public Task<int> Luu(int? id,string ten,string diaChi,string? soDienThoai,bool hoatDong)
    {
        if(id is <=0 || string.IsNullOrWhiteSpace(ten) || string.IsNullOrWhiteSpace(diaChi)
          ||ten.Length>100||diaChi.Length>500||(soDienThoai?.Length??0)>20)
            throw new ArgumentException("Thông tin chi nhánh không hợp lệ.");
        return repo.Luu(id,ten.Trim(),diaChi.Trim(),soDienThoai?.Trim(),hoatDong);
    }
    public Task<int> BatTat(int id,bool active)
    {
        if(id<=0)throw new ArgumentException("Chi nhánh không hợp lệ.");
        return repo.BatTat(id,active);
    }
}
