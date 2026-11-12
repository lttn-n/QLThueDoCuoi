Viết SELECT/INSERT/UPDATE/DELETE có SqlParameter tại đây. Dùng Data/KetNoiDatabase; nghiệp vụ và giao dịch ở Services.

Đã có (ADO.NET, đăng ký trong Program.cs):
- LoaiSanPhamRepository: đọc LoaiSanPham (menu 2 cấp, danh mục trang chủ).
- SanPhamRepository: sản phẩm nổi bật (gom theo TenSanPham), ảnh đại diện theo loại.
- TaiKhoanRepository: đăng ký / đăng nhập / hồ sơ / đổi mật khẩu (TaiKhoan). Nghiệp vụ ở Services/TaiKhoanService.cs.
- GioThueRepository: đếm trang phục trong giỏ (GioThue + ChiTietGioThue).
Nhóm bổ sung tiếp: DonThueRepository, LichHenThuDoRepository, BoSuuTapRepository, YeuThichRepository...
