using Microsoft.Data.SqlClient;
using QLThueDoCuoi.Repositories;
using System.Text.RegularExpressions;
namespace QLThueDoCuoi.Services;
public sealed class LichHenService(LichHenRepository repo)
{
    // Gio mo cua 08:00-20:00 la cau hinh demo; xac nhan voi nhom truoc khi nop.
    private static readonly TimeSpan MoCua=new(8,0,0), DongCua=new(20,0,0);
    public Task<List<Dictionary<string,object?>>> DanhSach(int tk)=>repo.CuaToi(tk);
    public Task<List<Dictionary<string,object?>>> ChiTiet(int tk,int id)=>repo.ChiTiet(tk,id);
    public async Task<string?> Tao(int tk,int sp,int cn,string? ten,string? sdt,DateTime gioVietNam,string? ghiChu)
    {
        if(tk<=0||sp<=0||cn<=0||string.IsNullOrWhiteSpace(ten)||string.IsNullOrWhiteSpace(sdt))
            return "Vui lòng nhập đầy đủ thông tin.";
        ten=ten.Trim();sdt=sdt.Trim();
        if(ten.Length>100||sdt.Length>20||!Regex.IsMatch(sdt,@"^\+?[0-9][0-9 .-]{7,18}$"))
            return "Họ tên hoặc số điện thoại không hợp lệ.";
        if((ghiChu?.Length??0)>2000)return "Ghi chú quá dài.";
        if(gioVietNam==default || gioVietNam.TimeOfDay<MoCua || gioVietNam.TimeOfDay>=DongCua)
            return "Chỉ nhận lịch hẹn từ 08:00 đến 20:00 (giờ Việt Nam).";
        var utc=DateTime.SpecifyKind(gioVietNam.AddHours(-7),DateTimeKind.Utc);
        if(utc<=DateTime.UtcNow) return "Không thể đặt lịch trong quá khứ.";
        try {
            if(!await repo.HopLeSanPhamVaChiNhanh(sp,cn))return "Sản phẩm hoặc chi nhánh không còn hoạt động.";
            if(await repo.TrungLich(tk,sp,utc))return "Bạn đã đặt sản phẩm này vào thời điểm đã chọn.";
            await repo.Tao(tk,sp,cn,ten,sdt,utc,ghiChu?.Trim());return null;
        } catch(SqlException ex) when(ex.Number is 2601 or 2627) {
            return "Bạn đã đặt sản phẩm này vào thời điểm đã chọn.";
        } catch(SqlException) {
            return "Không thể lưu lịch hẹn lúc này. Vui lòng thử lại.";
        }
    }
    public async Task<bool> Huy(int tk,int id,string lyDo) {
        if(tk<=0||id<=0||string.IsNullOrWhiteSpace(lyDo)||lyDo.Length>2000)return false;
        try { return await repo.Huy(tk,id,lyDo.Trim())>0; }
        catch(SqlException){return false;}
    }
}
