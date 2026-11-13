using QLThueDoCuoi.Repositories;
namespace QLThueDoCuoi.Services;
// Dung chung voi nguoi phu trach GioThue/DatThue: KHONG tu dong tru tien trong phan Diem.
public sealed class KhuyenMaiService(KhuyenMaiRepository repo)
{
    public async Task<(bool HopLe,decimal SoTienGiam,string ThongBao)> KiemTraMa(string? ma,decimal tamTinh)
    {
        if(string.IsNullOrWhiteSpace(ma))return(false,0,"Vui lòng nhập mã ưu đãi.");
        if(tamTinh<0)return(false,0,"Giá trị đơn hàng không hợp lệ.");
        var rows=await repo.TimTheoMa(ma.Trim());
        if(rows.Count==0)return(false,0,"Mã ưu đãi không tồn tại.");
        var r=rows[0];
        if(!Convert.ToBoolean(r["TrangThai"]))return(false,0,"Mã đã tắt.");
        var now=DateTime.UtcNow;
        if(now<Convert.ToDateTime(r["NgayBatDau"])||now>Convert.ToDateTime(r["NgayKetThuc"]))return(false,0,"Mã chưa có hiệu lực hoặc đã hết hạn.");
        var min=Convert.ToDecimal(r["GiaTriDonToiThieu"]);
        if(tamTinh<min)return(false,0,$"Đơn cần đạt tối thiểu {min:N0} đ.");
        if(r["SoLuotSuDungToiDa"] is not null && Convert.ToInt32(r["DaDung"])>=Convert.ToInt32(r["SoLuotSuDungToiDa"]))
            return(false,0,"Mã đã hết lượt sử dụng.");
        var gia=Convert.ToDecimal(r["GiaTriGiam"]);
        var giam=Convert.ToString(r["HinhThucGiam"])=="PHANTRAM"?tamTinh*gia/100m:gia;
        if(r["SoTienGiamToiDa"] is not null)giam=Math.Min(giam,Convert.ToDecimal(r["SoTienGiamToiDa"]));
        giam=Math.Max(0,Math.Min(giam,tamTinh));
        return(true,decimal.Round(giam,2),"Mã hợp lệ.");
    }
}
