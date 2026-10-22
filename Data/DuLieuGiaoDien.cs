using QLThueDoCuoi.ViewModels;
namespace QLThueDoCuoi.Data;
// Chỉ minh họa layout. Khi có repository, đọc SQL và ánh xạ sang cùng ViewModel này.
public static class DuLieuGiaoDien
{
    public static IReadOnlyList<TheSanPhamViewModel> SanPham { get; } = new[]
    {
        new TheSanPhamViewModel(1,"Váy cưới Élodie","Váy cưới",2800000,3,"/images/trang-chu/vay-cuoi.jpg","Bán chạy","S M L"),
        new TheSanPhamViewModel(5,"Vest nam Modern Fit","Vest chú rể",1600000,3,"/images/trang-chu/vest.jpg","Mới","M L XL"),
        new TheSanPhamViewModel(9,"Áo dài Minh Châu","Áo dài",1800000,3,"/images/dragneel/aodai.svg","Tuyển chọn","S M L"),
        new TheSanPhamViewModel(13,"Set phụ kiện Pearl","Phụ kiện",650000,3,"/images/dragneel/pearl.svg","Nổi bật","Freesize")
    };
}
