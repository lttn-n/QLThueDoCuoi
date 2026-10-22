# Quản lý thuê đồ cưới: kiểm tra còn đồ theo ngày

Bản này chỉ thuê và thử, không mua. Có 13 bảng; đúng một file SQL khởi tạo.

## Các phần đã bổ sung

- Hình ảnh do C# quản lý; không có bảng hình ảnh trong database.
- `BoTrangPhuc`: mã riêng từng chiếc, sản phẩm/size/màu đã gộp, chi nhánh, tình trạng.
- `BoTrangPhucTrongDon`: phân từng bộ vào dòng đơn, không sao chép ngày/giá; hai khóa ngoại ghép chặn phân nhầm size/màu.
- `GioThue`, `ChiTietGioThue`: đã có và giữ nguyên. Giỏ đăng nhập lưu SQL, khách chưa đăng nhập dùng Session.
- Bỏ `SanPham.SoLuong`: đếm từ từng bộ để không có hai nguồn tồn bị lệch.

Ví dụ mẫu Élodie / M / trắng có 2 bộ. Từ X tới Y có 1 bộ bị giữ hoặc đang giặt/sửa thì còn 1. Khách yêu cầu 2: báo thiếu, không tạo đơn. Một bộ phải trống cả khoảng thuê và vệ sinh; không đổi chiếc giữa kỳ.

## Chạy database

Mở `QuanLyThueDoCuoi.sql` trong SSMS, kết nối SQL Server 2016+, F5.
Chỉ khởi tạo khi database `QuanLyThueDoCuoi` chưa có bảng. Không tự xóa hay nâng cấp dữ liệu cũ.
Có 13 dòng sản phẩm(size/màu), 26 bộ ở chi nhánh 1, mã MIRA10. Nhóm phải thay địa chỉ thật. Chi nhánh 2 chưa có bộ, nên kiểm tra tồn riêng chi nhánh 2 trả về 0.

## Gắn service kiểm tra và đặt giữ

`../Services/KhoTrangPhucService.cs` dùng .NET 6+, ADO.NET chuẩn `DbConnection` (không cần EF).
Ứng dụng cài `Microsoft.Data.SqlClient` theo phiên bản dự án. Factory tạo kết nối ví dụ:

```csharp
var kho = new KhoTrangPhucService(() => new Microsoft.Data.SqlClient.SqlConnection(connectionString));
var ton = await kho.KiemTraConDoAsync(maSanPham, ngayNhan, ngayTra, maChiNhanhNhan);
if (!ton.DuSoLuong(soLuong))
{
    // Hiển thị: chỉ còn ton.SoLuongCon bộ cho đúng mẫu/size/màu/kho/khoảng ngày.
}
```

Đây chỉ là xem trước. Luôn kiểm tra lại tại bước tạo đơn:

```csharp
try
{
    var maDongDaTao = await kho.DatThueAsync(async (connection, transaction, ct) =>
    {
        // Dùng CHÍNH connection + transaction được truyền vào.
        // 1. Xác thực chủ giỏ, kiểm tra dữ liệu, chống bấm form lặp.
        // 2. Đọc giá và mã từ SQL, tính tiền/cọc, kiểm tra lượt khuyến mãi.
        // 3. INSERT DonThue, INSERT các ChiTietDonThue; lấy mã qua OUTPUT INSERTED.
        // 4. Trả đủ mã của tất cả dòng trong cùng một đơn mới.
        // Không tự mở kết nối khác, không commit bên trong callback.
        return await donThueRepository.TaoDonVaChiTietAsync(connection, transaction, yeuCau, ct);
    });
    // Đã commit đơn + phân từng bộ. Bây giờ cập nhật/xóa giỏ đã đặt thành công.
}
catch (HetTrangPhucException ex)
{
    // Toàn đơn bị rollback. Giữ giỏ, thông báo ex.SoLuongCon, cho đổi ngày/số lượng.
}
```

`donThueRepository` trong ví dụ là phần tạo đơn của ứng dụng, KHÔNG phải class có sẵn trong gói này. Service cung cấp kiểm tra kho, khóa giao dịch và phân bộ; không phải toàn bộ backend website.

Nếu xóa giỏ ngoài transaction mà lỗi xóa xảy ra, đơn đã tạo vẫn thành công; ứng dụng phải dùng mã yêu cầu chống lặp để không tạo thêm đơn khi khách thử lại. Nếu đã có repository, có thể xóa đúng dòng giỏ trong callback vì lỗi phân bộ sẽ rollback cả thao tác xóa; phải bảo đảm giao diện giữ/khôi phục trạng thái sau lỗi.

## Cơ chế chống hai khách đặt cùng lúc

`DatThueAsync` mở transaction, lấy khóa ứng dụng SQL Server `QuanLyThueDoCuoi:Kho`, rồi mới tạo đơn và đọc lại bộ còn trống. Nếu đủ, chọn từng mã và ghi phân bộ; thiếu thì rollback cả đơn. Khóa được giữ đến commit/rollback. Bảng giỏ không giữ đồ.

Dùng `ThucHienTrongKhoaKhoAsync` cho cả hủy, giao/trả, thay đổi tình trạng, chuyển kho, đổi ngày/số lượng. Nó chỉ cấp khóa, không tự triển khai mọi quy tắc của các thao tác này. Mỗi callback phải kiểm tra quyền/trạng thái và lịch liên quan trước commit.

Đổi dòng GIU_CHO: cập nhật ngày/số lượng trong callback, gọi `PhanBoDongAsync` với cùng connection/transaction để phân lại; nếu lỗi phải để exception đi ra wrapper, không nuốt rồi commit. Phải tính lại tiền và điều kiện mã khi đổi số lượng/ngày. Không dùng hàm này cho dòng đã giao.

Giao chỉ khi đến ngày nhận, các bộ sẵn sàng và không có khách trước giữ quá hạn; cập nhật trạng thái bộ + dòng trong cùng khóa. Trả toàn bộ dòng, ghi thời điểm và phí, chuyển bộ đang giặt/sửa; kiểm tra xong mới sẵn sàng. Không xóa lịch sử phân bộ đã trả.

Mọi luồng ghi kho phải dùng cùng resource khóa. SQL trực tiếp, repository tự ghi ngoài wrapper hoặc admin sửa ngày/tình trạng bỏ qua kiểm tra có thể phá cơ chế. Không có trigger/exclusion constraint chặn mọi câu lệnh SQL trực tiếp. Không gọi API/chờ người dùng khi đang giữ transaction. Cách khóa toàn kho hợp bài môn Web; lượng ghi lớn cần cơ chế tinh hơn.

## Khoảng ngày và trạng thái

Khoảng `[Ngày nhận, Ngày trả + thời gian vệ sinh)`; hai khoảng chỉ chạm biên được phép. Ví dụ trả 23, vệ sinh 1 ngày thì từ 24 mới có thể cho thuê lại.

- `GIU_CHO`: giữ theo ngày đã đặt, kể cả đơn chờ xác nhận; đơn hủy giải phóng lịch.
- `DA_GIAO`: giữ đến ngày sẵn sàng dự kiến; quá hạn chưa trả chặn các lịch mới tương lai đến khi xử lý.
- `DA_TRA`: dùng ngày trả thực tế + số ngày vệ sinh. Bộ vẫn đang giặt/sửa không nhận đặt mới tới khi nhân viên chuyển sẵn sàng.
- `SAN_SANG`: có thể chọn khi không vướng lịch. `DANG_THUE`: có thể đặt cho thời gian sau khi dự kiến trả/vệ sinh, nhưng lúc giao phải kiểm tra lại thực tế.
- `DANG_GIAT`, `DANG_SUA`, `HU_HONG`, `MAT`, `NGUNG_THUE`: loại khỏi lựa chọn mới. Lịch đã có bị ảnh hưởng phải được nhân viên xử lý/đổi bộ.

Trả trễ không tự làm biến mất các đơn đã hẹn sau; phải cảnh báo và phân lại chiếc khác/trao đổi với khách. Bản này giao/trả cùng lúc toàn bộ số lượng trên một dòng; chưa hỗ trợ trả từng phần.

## Đã kiểm tra và giới hạn

- C# biên dịch với .NET 8, không cảnh báo/lỗi.
- Kiểm tra 13 bảng, khóa ngoại, cột seed, ràng buộc không trùng.
- 14 tình huống truy vấn tồn: chồng ngày, chạm biên, vệ sinh cả hai phía, hủy, trả thực tế, quá hạn, chi nhánh và trạng thái.
- Các kiểm tra bộ sai size, mã tình trạng sai và giỏ trùng chủ được giữ; bảng ảnh đã bỏ theo yêu cầu.
- Các kiểm tra dữ liệu/truy vấn dùng bản chuyển đổi sang SQLite; KHÔNG phải chạy SQL Server. Chưa chạy kiểm thử hai kết nối SQL Server thật hoặc UI.

Trước triển khai, kiểm thử SQL Server với 2 khách đồng thời tranh bộ cuối: chỉ một đơn commit, đơn còn lại báo hết. Thử tiếp đổi ngày, hủy, trả trễ, khách nhận trước lịch, lỗi giữa các bước, rollback, và chuyển tình trạng.

Tài liệu khóa chính thức: https://learn.microsoft.com/sql/relational-databases/system-stored-procedures/sp-getapplock-transact-sql
