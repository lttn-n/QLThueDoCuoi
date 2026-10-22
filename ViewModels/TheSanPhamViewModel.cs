namespace QLThueDoCuoi.ViewModels;
// Dữ liệu phục vụ giao diện; không phải bảng SQL mới.
public sealed record TheSanPhamViewModel(int MaSanPham, string TenSanPham,
    string TenLoai, decimal GiaThue, int SoNgayTrongGoi, string HinhAnh,
    string Nhan, string KichThuoc);
