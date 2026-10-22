/*
DATABASE QuanLyThueDoCuoi — THUÊ VÀ THỬ ĐỒ, QUẢN LÝ TỪNG BỘ
SQL Server 2016+, một file SQL, 13 bảng + ràng buộc + dữ liệu mẫu.
Mở SSMS, chạy F5. Chỉ khởi tạo khi database chưa có bảng.
Không xóa dữ liệu, không tự nâng cấp database cũ. Không chạy lẫn các bản trước.

BỔ SUNG THEO GÓP Ý
1. BoTrangPhuc: mã riêng từng chiếc, chi nhánh và tình trạng.
2. BoTrangPhucTrongDon: xác định những chiếc cụ thể được giữ/giao cho đơn nào.
3. GioThue/ChiTietGioThue đã có và được giữ, không chỉ lưu giỏ bằng Session.
4. Bỏ SanPham.SoLuong: tổng kho và số còn trống đếm từ BoTrangPhuc và các lịch phân bộ.

13 BẢNG
TaiKhoan, ChiNhanh, LoaiSanPham, SanPham, BoTrangPhuc,
KhuyenMai, GioThue, ChiTietGioThue, DonThue, ChiTietDonThue,
BoTrangPhucTrongDon, ThanhToan, LichHenThuDo.
SanPham vẫn gộp mẫu + size + màu. Không tạo lại bảng size/màu riêng.

CƠ CHẾ CHỐNG TRÙNG — THỰC HIỆN Ở C#
File KhoTrangPhucService.cs đi kèm có truy vấn ngày X-Y và hàm giữ bộ nguyên tử.
Tạo đơn/chi tiết + chọn bộ + ghi BoTrangPhucTrongDon phải nằm trong CÙNG transaction,
giữ khóa SQL Server sp_getapplock loại Exclusive, owner Transaction cho toàn kho.
Khách khác phải đợi; đọc lại lịch sau khi lấy khóa, thiếu bộ thì rollback toàn đơn.
Khóa ứng dụng này được gọi bằng C#, không phải trigger/procedure nghiệp vụ tự tạo.
Cách này đơn giản cho bài môn Web, đổi lại các thao tác ghi kho xếp hàng tuần tự.
Các luồng đổi ngày/hủy/giao/trả/chuyển chi nhánh/sửa tình trạng cũng phải dùng cùng khóa
và kiểm tra lại lịch trước commit. Thao tác SQL trực tiếp bỏ qua service có thể phá quy tắc.
Chỉ tạo bảng hoặc chạy SELECT trước INSERT KHÔNG tự chống thuê trùng theo khoảng ngày.
Service không thay thế đăng nhập, quyền, tính giá/khuyến mãi và thanh toán của ứng dụng.
Chưa thực thi trên SQL Server; phải kiểm thử tích hợp và đặt đồng thời trước khi triển khai.

QUY ƯỚC
Ngày nhận là X, ngày trả dự kiến là Y, Y>X; ngày trả không tính tiền thuê.
Vẫn giữ bộ thêm SoNgayVeSinhKhiThue ngày. Nếu thời gian vệ sinh=1, trả23 thì từ24 mới sẵn sàng.
Một bộ chỉ được chọn khi phục vụ trọn khoảng khách yêu cầu; không đổi bộ giữa kỳ.
DA_GIAO quá hạn chưa trả khóa bộ cho các lịch tương lai đến khi xử lý; cảnh báo các đơn đã hẹn.
DA_TRA vẫn tính thời gian vệ sinh từ ngày trả thực tế. DANG_GIAT/DANG_SUA/HU_HONG/MAT/
NGUNG_THUE không được chọn mới; nhân viên chỉ chuyển SAN_SANG sau khi kiểm tra thực tế.
BoTrangPhuc.MaChiNhanh là nơi quản lý hiện tại, chỉ chuyển khi không vướng lịch/giao đồ.
Chọn nhận tại cửa hàng thì phân bộ thuộc chi nhánh đã chọn; giao tận nơi có thể lấy kho phù hợp.
Giá gói và giá thêm ngày là đơn giá một chiếc; TienThue/TienCoc/PhuPhi ở chi tiết là tổng dòng.
Giá thuê gói 3 ngày (2.800.000đ/3 ngày); không hiển thị nhầm thành 2.800.000đ/ngày.
DATE là ngày VN; DATETIME2 lưu UTC. Thử đồ không giữ đồ hay tự tạo đơn thuê.
Một dòng chi tiết giao/trả toàn bộ cùng lúc; chưa hỗ trợ trả từng phần trong cùng dòng.
Không có mua, yêu thích, đánh giá; bộ sưu tập cố định và các trang giới thiệu do C# quản lý.
Tên/mô tả/giá vẫn lặp giữa các size: C# cần quản lý sửa chung theo loại + tên mẫu.
*/
USE master;
GO
IF DB_ID(N'QuanLyThueDoCuoi') IS NULL
    EXEC(N'CREATE DATABASE QuanLyThueDoCuoi');
GO
USE QuanLyThueDoCuoi;
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;

BEGIN TRY
    IF OBJECT_ID(N'dbo.TaiKhoan', N'U') IS NOT NULL
        THROW 50000, N'Database đã có bảng. File chỉ dùng để khởi tạo một lần.', 1;
    BEGIN TRANSACTION;

-- BẢNG: Tài khoản
-- Thông tin đăng nhập và vai trò: khách hàng, nhân viên, quản trị viên
CREATE TABLE dbo.TaiKhoan (
    MaTaiKhoan INT IDENTITY PRIMARY KEY,
    HoTen NVARCHAR(100) NOT NULL,
    Email VARCHAR(254) NULL,
    SoDienThoai VARCHAR(20) NULL,
    MatKhauHash NVARCHAR(500) NOT NULL,
    VaiTro VARCHAR(15) NOT NULL DEFAULT 'KHACHHANG',
    DiaChi NVARCHAR(500) NULL,
    TrangThai BIT NOT NULL DEFAULT 1, -- 1: hoạt động; 0: khóa/ngừng sử dụng
    NgayTao DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT CK_TK_LienLac CHECK (Email IS NOT NULL OR SoDienThoai IS NOT NULL),
    CONSTRAINT CK_TK_Email CHECK (Email IS NULL OR LEN(LTRIM(RTRIM(Email))) > 0),
    CONSTRAINT CK_TK_SDT CHECK (SoDienThoai IS NULL OR LEN(LTRIM(RTRIM(SoDienThoai))) > 0),
    CONSTRAINT CK_TK_VaiTro CHECK (VaiTro IN ('ADMIN','NHANVIEN','KHACHHANG'))
);
CREATE UNIQUE INDEX UX_TK_Email ON dbo.TaiKhoan(Email) WHERE Email IS NOT NULL;
CREATE UNIQUE INDEX UX_TK_SDT ON dbo.TaiKhoan(SoDienThoai) WHERE SoDienThoai IS NOT NULL;

-- BẢNG: Chi nhánh
-- Nơi khách đến thử. C# chỉ cho chọn chi nhánh đang hoạt động.
CREATE TABLE dbo.ChiNhanh (
    MaChiNhanh INT IDENTITY PRIMARY KEY,
    TenChiNhanh NVARCHAR(100) NOT NULL UNIQUE,
    DiaChi NVARCHAR(500) NOT NULL,
    SoDienThoai VARCHAR(20) NULL,
    TrangThai BIT NOT NULL DEFAULT 1, -- 1: hoạt động; 0: ngừng nhận lịch mới
    CONSTRAINT CK_ChiNhanh_ThongTin CHECK (
        LEN(LTRIM(RTRIM(TenChiNhanh))) > 0 AND LEN(LTRIM(RTRIM(DiaChi))) > 0)
);

-- BẢNG: Loại sản phẩm
CREATE TABLE dbo.LoaiSanPham (
    MaLoaiSanPham INT IDENTITY PRIMARY KEY,
    MaLoaiCha INT NULL REFERENCES dbo.LoaiSanPham(MaLoaiSanPham),
    TenLoaiSanPham NVARCHAR(100) NOT NULL,
    HienThi BIT NOT NULL DEFAULT 1,
    CONSTRAINT CK_DM_Cha CHECK (MaLoaiCha IS NULL OR MaLoaiCha <> MaLoaiSanPham)
);
-- BẢNG: Sản phẩm
CREATE TABLE dbo.SanPham (
    MaSanPham INT IDENTITY PRIMARY KEY,
    MaLoaiSanPham INT NOT NULL REFERENCES dbo.LoaiSanPham(MaLoaiSanPham),
    TenSanPham NVARCHAR(200) NOT NULL,
    KichThuoc NVARCHAR(30) NOT NULL,
    MauSac NVARCHAR(50) NOT NULL,
    MoTa NVARCHAR(MAX) NULL,
    ChatLieu NVARCHAR(200) NULL,
    GiaThueGoi DECIMAL(18,2) NOT NULL,
    SoNgayThueTrongGoi INT NOT NULL DEFAULT 3,
    GiaThueMoiNgayThem DECIMAL(18,2) NOT NULL DEFAULT 0,
    TienCoc DECIMAL(18,2) NOT NULL DEFAULT 0,
    SoNgayVeSinh INT NOT NULL DEFAULT 1,
    GiaKhuyenMai DECIMAL(18,2) NULL,
    NgayBatDauGiamGia DATETIME2(0) NULL,
    NgayKetThucGiamGia DATETIME2(0) NULL,
    TrangThai BIT NOT NULL DEFAULT 1, -- 1: hoạt động; 0: ngừng cho thuê
    NgayTao DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT UQ_SanPham UNIQUE (MaLoaiSanPham,TenSanPham,KichThuoc,MauSac),
    CONSTRAINT CK_SP_Gia CHECK (GiaThueGoi >= 0 AND GiaThueMoiNgayThem >= 0 AND TienCoc >= 0),
    CONSTRAINT CK_SP_Ngay CHECK (SoNgayThueTrongGoi BETWEEN 1 AND 365 AND SoNgayVeSinh BETWEEN 0 AND 365),
    CONSTRAINT CK_SP_Giam CHECK (
        (GiaKhuyenMai IS NULL AND NgayBatDauGiamGia IS NULL AND NgayKetThucGiamGia IS NULL) OR
        (GiaKhuyenMai IS NOT NULL AND NgayBatDauGiamGia IS NOT NULL AND NgayKetThucGiamGia IS NOT NULL
         AND GiaKhuyenMai >= 0 AND GiaKhuyenMai < GiaThueGoi AND NgayKetThucGiamGia > NgayBatDauGiamGia))
);
-- BẢNG: Bộ trang phục
-- Từng chiếc thực tế. Hai chiếc cùng mẫu/size/màu vẫn có hai mã khác nhau.
CREATE TABLE dbo.BoTrangPhuc (
    MaBoTrangPhuc INT IDENTITY PRIMARY KEY,
    MaSanPham INT NOT NULL REFERENCES dbo.SanPham(MaSanPham),
    MaChiNhanh INT NOT NULL REFERENCES dbo.ChiNhanh(MaChiNhanh),
    MaQuanLy VARCHAR(50) NOT NULL UNIQUE, -- Ví dụ VAY-M-001
    TinhTrang VARCHAR(20) NOT NULL DEFAULT 'SAN_SANG',
    GhiChu NVARCHAR(1000) NULL,
    CONSTRAINT UQ_Bo_SanPham UNIQUE (MaBoTrangPhuc,MaSanPham),
    CONSTRAINT CK_Bo_TinhTrang CHECK (TinhTrang IN
        ('SAN_SANG','DANG_THUE','DANG_GIAT','DANG_SUA','HU_HONG','MAT','NGUNG_THUE'))
);
CREATE INDEX IX_Bo_SanPham ON dbo.BoTrangPhuc(MaSanPham,MaChiNhanh,TinhTrang);

-- BẢNG: Khuyến mãi
CREATE TABLE dbo.KhuyenMai (
    MaKhuyenMai INT IDENTITY PRIMARY KEY,
    MaUuDai VARCHAR(40) NOT NULL UNIQUE,
    TenKhuyenMai NVARCHAR(200) NOT NULL,
    HinhThucGiam VARCHAR(10) NOT NULL,
    GiaTriGiam DECIMAL(18,2) NOT NULL,
    SoTienGiamToiDa DECIMAL(18,2) NULL,
    GiaTriDonToiThieu DECIMAL(18,2) NOT NULL DEFAULT 0,
    NgayBatDau DATETIME2(0) NOT NULL,
    NgayKetThuc DATETIME2(0) NOT NULL,
    SoLuotSuDungToiDa INT NULL,
    TrangThai BIT NOT NULL DEFAULT 1, -- 1: hoạt động; 0: khóa/ngừng sử dụng
    CONSTRAINT CK_KM_Loai CHECK (HinhThucGiam IN ('PHANTRAM','SOTIEN')),
    CONSTRAINT CK_KM_Gia CHECK (GiaTriGiam > 0 AND (HinhThucGiam <> 'PHANTRAM' OR GiaTriGiam <= 100)
        AND GiaTriDonToiThieu >= 0 AND (SoTienGiamToiDa IS NULL OR SoTienGiamToiDa > 0)),
    CONSTRAINT CK_KM_Ngay CHECK (NgayKetThuc > NgayBatDau),
    CONSTRAINT CK_KM_Luot CHECK (SoLuotSuDungToiDa IS NULL OR SoLuotSuDungToiDa > 0)
);
-- BẢNG: Giỏ thuê
-- Giỏ lâu dài của tài khoản đã đăng nhập; mỗi tài khoản có tối đa một giỏ.
-- Khách chưa đăng nhập dùng Session C#, đăng nhập thì gộp lựa chọn vào giỏ này.
CREATE TABLE dbo.GioThue (
    MaGioThue INT IDENTITY PRIMARY KEY,
    MaTaiKhoan INT NOT NULL UNIQUE REFERENCES dbo.TaiKhoan(MaTaiKhoan),
    MaKhuyenMai INT NULL REFERENCES dbo.KhuyenMai(MaKhuyenMai), -- Mã đang chọn, chưa đảm bảo còn hiệu lực
    NgayTao DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME()
);

-- BẢNG: Chi tiết giỏ thuê
-- Không lưu giá trong giỏ: C# đọc giá hiện tại và tính lại khi hiển thị/đặt thuê.
CREATE TABLE dbo.ChiTietGioThue (
    MaChiTietGioThue INT IDENTITY PRIMARY KEY,
    MaGioThue INT NOT NULL REFERENCES dbo.GioThue(MaGioThue),
    MaSanPham INT NOT NULL REFERENCES dbo.SanPham(MaSanPham),
    SoLuong INT NOT NULL DEFAULT 1 CHECK (SoLuong > 0),
    NgayNhan DATE NULL,
    NgayTraDuKien DATE NULL,
    CONSTRAINT CK_Gio_Ngay CHECK (
        (NgayNhan IS NULL AND NgayTraDuKien IS NULL) OR
        (NgayNhan IS NOT NULL AND NgayTraDuKien IS NOT NULL AND NgayTraDuKien > NgayNhan)),
    CONSTRAINT UQ_Gio_Dong UNIQUE (MaGioThue,MaSanPham,NgayNhan,NgayTraDuKien)
);

-- BẢNG: Đơn thuê
CREATE TABLE dbo.DonThue (
    MaDonThue INT IDENTITY PRIMARY KEY,
    MaTaiKhoan INT NOT NULL REFERENCES dbo.TaiKhoan(MaTaiKhoan),
    NgayDat DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
    TrangThai VARCHAR(20) NOT NULL DEFAULT 'CHO_XAC_NHAN',
    TenNguoiNhan NVARCHAR(100) NOT NULL,
    SoDienThoaiNhan VARCHAR(20) NOT NULL,
    HinhThucNhan VARCHAR(15) NOT NULL, -- CUAHANG hoặc GIAOHANG
    MaChiNhanhNhan INT NULL REFERENCES dbo.ChiNhanh(MaChiNhanh),
    PhuongThucThanhToan VARCHAR(15) NOT NULL, -- Cách khách dự định thanh toán, không phải đã thu tiền
    DiaChiNhan NVARCHAR(500) NULL,
    PhiGiaoNhan DECIMAL(18,2) NOT NULL DEFAULT 0,
    MaKhuyenMai INT NULL REFERENCES dbo.KhuyenMai(MaKhuyenMai),
    SoTienGiam DECIMAL(18,2) NOT NULL DEFAULT 0,
    GhiChu NVARCHAR(1000) NULL,
    CONSTRAINT CK_Don_TT CHECK (TrangThai IN ('CHO_XAC_NHAN','DA_XAC_NHAN','DANG_THUE','HOAN_TAT','DA_HUY')),
    CONSTRAINT CK_Don_Nhan CHECK (HinhThucNhan IN ('CUAHANG','GIAOHANG')),
    CONSTRAINT CK_Don_NoiNhan CHECK (
        (HinhThucNhan='CUAHANG' AND MaChiNhanhNhan IS NOT NULL) OR
        (HinhThucNhan='GIAOHANG' AND MaChiNhanhNhan IS NULL
         AND DiaChiNhan IS NOT NULL AND LEN(LTRIM(RTRIM(DiaChiNhan)))>0)),
    CONSTRAINT CK_Don_PhuongThuc CHECK (PhuongThucThanhToan IN ('TIENMAT','CHUYENKHOAN')),
    CONSTRAINT CK_Don_Tien CHECK (PhiGiaoNhan >= 0 AND SoTienGiam >= 0)
);
-- BẢNG: Chi tiết đơn thuê
CREATE TABLE dbo.ChiTietDonThue (
    MaChiTietDonThue INT IDENTITY PRIMARY KEY,
    MaDonThue INT NOT NULL REFERENCES dbo.DonThue(MaDonThue),
    MaSanPham INT NOT NULL REFERENCES dbo.SanPham(MaSanPham),
    SoLuong INT NOT NULL CHECK (SoLuong > 0),
    TenSanPhamKhiThue NVARCHAR(200) NOT NULL,
    KichThuocKhiThue NVARCHAR(30) NOT NULL,
    MauSacKhiThue NVARCHAR(50) NOT NULL,
    NgayNhan DATE NOT NULL,
    NgayTraDuKien DATE NOT NULL,
    SoNgayVeSinhKhiThue INT NOT NULL,
    GiaGoiKhiThue DECIMAL(18,2) NOT NULL, -- Giá gói cho MỘT chiếc
    SoNgayTrongGoiKhiThue INT NOT NULL,
    GiaNgayThemKhiThue DECIMAL(18,2) NOT NULL, -- Giá ngày thêm cho MỘT chiếc
    TienThue DECIMAL(18,2) NOT NULL, -- Tổng tiền thuê của cả dòng, đã nhân SoLuong, chưa trừ mã giảm
    TienCoc DECIMAL(18,2) NOT NULL, -- Tổng cọc của cả dòng, đã nhân SoLuong
    TrangThai VARCHAR(15) NOT NULL DEFAULT 'GIU_CHO',
    ThoiDiemGiao DATETIME2(0) NULL,
    ThoiDiemTra DATETIME2(0) NULL,
    TinhTrangKhiTra NVARCHAR(1000) NULL,
    PhiTraTre DECIMAL(18,2) NOT NULL DEFAULT 0,
    PhiHuHong DECIMAL(18,2) NOT NULL DEFAULT 0,
    CONSTRAINT UQ_ChiTietDon_SanPham UNIQUE (MaChiTietDonThue,MaSanPham),
    CONSTRAINT CK_CT_Ngay CHECK (NgayTraDuKien > NgayNhan
        AND SoNgayVeSinhKhiThue BETWEEN 0 AND 365 AND SoNgayTrongGoiKhiThue BETWEEN 1 AND 365
        AND NgayTraDuKien <= CONVERT(date,'99981231')),
    CONSTRAINT CK_CT_Tien CHECK (GiaGoiKhiThue >= 0 AND GiaNgayThemKhiThue >= 0
        AND TienThue >= 0 AND TienCoc >= 0 AND PhiTraTre >= 0 AND PhiHuHong >= 0),
    CONSTRAINT CK_CT_TT CHECK (TrangThai IN ('GIU_CHO','DA_GIAO','DA_TRA','DA_HUY')),
    CONSTRAINT CK_CT_GiaoTra CHECK (
        (TrangThai IN ('GIU_CHO','DA_HUY') AND ThoiDiemGiao IS NULL AND ThoiDiemTra IS NULL) OR
        (TrangThai = 'DA_GIAO' AND ThoiDiemGiao IS NOT NULL AND ThoiDiemTra IS NULL) OR
        (TrangThai = 'DA_TRA' AND ThoiDiemGiao IS NOT NULL AND ThoiDiemTra IS NOT NULL AND ThoiDiemTra >= ThoiDiemGiao))
);
-- BẢNG: Bộ trang phục trong đơn thuê
-- Một dòng chi tiết thuê N chiếc có N dòng tại đây.
-- MaSanPham lặp chỉ để hai FK ghép bảo đảm bộ được phân đúng sản phẩm/size/màu.
-- Ngày và giá vẫn lưu duy nhất ở ChiTietDonThue, không sao chép sang bảng này.
CREATE TABLE dbo.BoTrangPhucTrongDon (
    MaChiTietDonThue INT NOT NULL,
    MaBoTrangPhuc INT NOT NULL,
    MaSanPham INT NOT NULL,
    PRIMARY KEY (MaChiTietDonThue,MaBoTrangPhuc),
    CONSTRAINT FK_PhanBo_ChiTiet FOREIGN KEY (MaChiTietDonThue,MaSanPham)
        REFERENCES dbo.ChiTietDonThue(MaChiTietDonThue,MaSanPham),
    CONSTRAINT FK_PhanBo_Bo FOREIGN KEY (MaBoTrangPhuc,MaSanPham)
        REFERENCES dbo.BoTrangPhuc(MaBoTrangPhuc,MaSanPham)
);
CREATE INDEX IX_PhanBo_Bo ON dbo.BoTrangPhucTrongDon(MaBoTrangPhuc,MaChiTietDonThue);
CREATE INDEX IX_ChiTiet_SanPham_Ngay ON dbo.ChiTietDonThue(MaSanPham,NgayNhan,NgayTraDuKien,TrangThai);

-- BẢNG: Thanh toán
CREATE TABLE dbo.ThanhToan (
    MaThanhToan INT IDENTITY PRIMARY KEY,
    MaDonThue INT NOT NULL REFERENCES dbo.DonThue(MaDonThue),
    LoaiGiaoDich VARCHAR(20) NOT NULL,
    SoTien DECIMAL(18,2) NOT NULL,
    PhuongThuc VARCHAR(15) NOT NULL,
    TrangThai VARCHAR(15) NOT NULL DEFAULT 'CHO_XU_LY',
    MaThamChieu VARCHAR(150) NULL,
    NgayTao DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
    ThoiDiemHoanTat DATETIME2(0) NULL,
    GhiChu NVARCHAR(1000) NULL,
    CONSTRAINT CK_GD_Loai CHECK (LoaiGiaoDich IN ('THU_COC','HOAN_COC','THU_THUE','HOAN_THUE','THU_PHUPHI','HOAN_PHUPHI')),
    CONSTRAINT CK_GD_Tien CHECK (SoTien > 0),
    CONSTRAINT CK_GD_PT CHECK (PhuongThuc IN ('TIENMAT','CHUYENKHOAN')),
    CONSTRAINT CK_GD_TT CHECK (TrangThai IN ('CHO_XU_LY','THANH_CONG','THAT_BAI')),
    CONSTRAINT CK_GD_Ngay CHECK ((TrangThai = 'THANH_CONG' AND ThoiDiemHoanTat IS NOT NULL)
        OR (TrangThai <> 'THANH_CONG' AND ThoiDiemHoanTat IS NULL))
);
CREATE UNIQUE INDEX UX_GD_ThamChieu ON dbo.ThanhToan(PhuongThuc,MaThamChieu) WHERE MaThamChieu IS NOT NULL;
-- BẢNG: Lịch hẹn thử đồ
CREATE TABLE dbo.LichHenThuDo (
    MaLichHen INT IDENTITY PRIMARY KEY,
    MaTaiKhoan INT NOT NULL REFERENCES dbo.TaiKhoan(MaTaiKhoan),
    MaSanPham INT NOT NULL REFERENCES dbo.SanPham(MaSanPham),
    MaChiNhanh INT NOT NULL REFERENCES dbo.ChiNhanh(MaChiNhanh),
    HoTenLienHe NVARCHAR(100) NOT NULL,
    SoDienThoaiLienHe VARCHAR(20) NOT NULL,
    ThoiDiemHen DATETIME2(0) NOT NULL, -- C# ghép ngày + giờ hẹn theo giờ VN rồi đổi sang UTC
    TrangThai VARCHAR(15) NOT NULL DEFAULT 'CHO_XAC_NHAN',
    GhiChu NVARCHAR(2000) NULL, -- Số đo, yêu cầu thử đồ; khi hủy nối thêm lý do hủy
    NgayTao DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
    ThoiDiemHuy DATETIME2(0) NULL,
    MaTaiKhoanHuy INT NULL REFERENCES dbo.TaiKhoan(MaTaiKhoan),
    CONSTRAINT CK_Hen_TT CHECK (TrangThai IN ('CHO_XAC_NHAN','DA_XAC_NHAN','DA_DEN','HOAN_TAT','DA_HUY')),
    CONSTRAINT CK_Hen_LienHe CHECK (LEN(LTRIM(RTRIM(HoTenLienHe))) > 0
        AND LEN(LTRIM(RTRIM(SoDienThoaiLienHe))) > 0),
    -- Lý do hủy ghi chung trong GhiChu; C# kiểm tra và nối thêm, không ghi đè nội dung cũ.
    CONSTRAINT CK_Hen_ThongTinHuy CHECK (
        (TrangThai='DA_HUY' AND ThoiDiemHuy IS NOT NULL AND MaTaiKhoanHuy IS NOT NULL
         AND GhiChu IS NOT NULL AND LEN(LTRIM(RTRIM(GhiChu)))>0)
        OR (TrangThai<>'DA_HUY' AND ThoiDiemHuy IS NULL AND MaTaiKhoanHuy IS NULL))
);
CREATE UNIQUE INDEX UX_Hen_KhongTrung ON dbo.LichHenThuDo(MaTaiKhoan,MaSanPham,ThoiDiemHen)
    WHERE TrangThai <> 'DA_HUY';

-- Hỗ trợ đọc lịch tại chi nhánh theo khoảng ngày/giờ. Không tự tạo khung giờ.
CREATE INDEX IX_LichHen_ChiNhanh_ThoiDiem
    ON dbo.LichHenThuDo(MaChiNhanh,ThoiDiemHen,TrangThai);

-- PHẦN B: DỮ LIỆU MẪU
-- Tên/địa chỉ dưới đây chỉ để minh họa. Nhóm cập nhật địa chỉ và điện thoại thật.
INSERT dbo.ChiNhanh(TenChiNhanh,DiaChi) VALUES
(N'DRAGNEEL - Chi nhánh 1',N'Địa chỉ minh họa chi nhánh 1 - nhóm cập nhật trước khi sử dụng'),
(N'DRAGNEEL - Chi nhánh 2',N'Địa chỉ minh họa chi nhánh 2 - nhóm cập nhật trước khi sử dụng');

INSERT dbo.LoaiSanPham(TenLoaiSanPham) VALUES (N'Trang phục cưới');
DECLARE @cha INT=CONVERT(INT,SCOPE_IDENTITY());
INSERT dbo.LoaiSanPham(MaLoaiCha,TenLoaiSanPham) VALUES
(@cha,N'Váy cưới'),(@cha,N'Áo dài'),(@cha,N'Vest chú rể'),(@cha,N'Phụ kiện');

INSERT dbo.SanPham(MaLoaiSanPham,TenSanPham,KichThuoc,MauSac,MoTa,ChatLieu,GiaThueGoi,GiaThueMoiNgayThem,TienCoc)
SELECT l.MaLoaiSanPham,N'Váy cưới Élodie',z.KichThuoc,N'Trắng',N'Váy cưới dáng chữ A.',N'Organza',2800000,400000,2000000
FROM dbo.LoaiSanPham l CROSS JOIN (VALUES(N'S'),(N'M'),(N'L'),(N'XL')) z(KichThuoc)
WHERE l.TenLoaiSanPham=N'Váy cưới';

INSERT dbo.SanPham(MaLoaiSanPham,TenSanPham,KichThuoc,MauSac,MoTa,ChatLieu,GiaThueGoi,GiaThueMoiNgayThem,TienCoc)
SELECT l.MaLoaiSanPham,N'Vest nam Modern Fit',z.KichThuoc,N'Đen',N'Vest chú rể phom hiện đại.',N'Vải wool blend',1600000,200000,1000000
FROM dbo.LoaiSanPham l CROSS JOIN (VALUES(N'S'),(N'M'),(N'L'),(N'XL')) z(KichThuoc)
WHERE l.TenLoaiSanPham=N'Vest chú rể';

INSERT dbo.SanPham(MaLoaiSanPham,TenSanPham,KichThuoc,MauSac,MoTa,ChatLieu,GiaThueGoi,GiaThueMoiNgayThem,TienCoc,GiaKhuyenMai,NgayBatDauGiamGia,NgayKetThucGiamGia)
SELECT l.MaLoaiSanPham,N'Áo dài Minh Châu',z.KichThuoc,N'Trắng',N'Áo dài cưới thêu họa tiết truyền thống.',N'Lụa',2000000,250000,1200000,1800000,DATEADD(DAY,-1,SYSUTCDATETIME()),DATEADD(DAY,90,SYSUTCDATETIME())
FROM dbo.LoaiSanPham l CROSS JOIN (VALUES(N'S'),(N'M'),(N'L'),(N'XL')) z(KichThuoc)
WHERE l.TenLoaiSanPham=N'Áo dài';

INSERT dbo.SanPham(MaLoaiSanPham,TenSanPham,KichThuoc,MauSac,MoTa,ChatLieu,GiaThueGoi,GiaThueMoiNgayThem,TienCoc)
SELECT l.MaLoaiSanPham,N'Set phụ kiện Pearl',z.KichThuoc,N'Trắng ngọc trai',N'Bộ phụ kiện ngọc trai cho cô dâu.',N'Ngọc trai nhân tạo',650000,100000,500000
FROM dbo.LoaiSanPham l CROSS JOIN (VALUES(N'Freesize')) z(KichThuoc)
WHERE l.TenLoaiSanPham=N'Phụ kiện';

-- Hai bộ cho mỗi mẫu/size/màu tại chi nhánh 1, không lưu SoLuong trùng trong SanPham.
INSERT dbo.BoTrangPhuc(MaSanPham,MaChiNhanh,MaQuanLy)
SELECT s.MaSanPham,c.MaChiNhanh,CONCAT('DRAGNEEL-',s.MaSanPham,'-',n.STT)
FROM dbo.SanPham s CROSS JOIN (VALUES(1),(2)) n(STT)
CROSS JOIN dbo.ChiNhanh c WHERE c.TenChiNhanh=N'DRAGNEEL - Chi nhánh 1';
INSERT dbo.KhuyenMai(MaUuDai,TenKhuyenMai,HinhThucGiam,GiaTriGiam,SoTienGiamToiDa, GiaTriDonToiThieu,NgayBatDau,NgayKetThuc,SoLuotSuDungToiDa)
VALUES ('MIRA10',N'Ưu đãi khai trương 10%','PHANTRAM',10,500000,1000000,
DATEADD(DAY,-1,SYSUTCDATETIME()),DATEADD(DAY,90,SYSUTCDATETIME()),100);

    COMMIT TRANSACTION;
    PRINT N'Đã tạo 13 bảng và dữ liệu mẫu cho QuanLyThueDoCuoi.';
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO


/* HƯỚNG DẪN NỐI VỚI C#
- Khách chưa đăng nhập dùng Session; đăng nhập gộp vào GioThue của đúng tài khoản.
  Giỏ không giữ đồ/chốt giá. Đăng ký luôn KHACHHANG, chỉ lưu hash mật khẩu.
- Khi xác nhận thuê: kiểm tra chủ giỏ, giá/mã, ngày, chi nhánh, số lượng; dùng
  KhoTrangPhucService.DatThueAsync để tạo đơn, chi tiết và phân bộ cùng transaction.
  Nếu SoLuong=2, BoTrangPhucTrongDon phải có đúng2 bộ khác nhau cho dòng đó.
  Không có đủ bộ sẽ ném HetTrangPhucException, rollback, giữ giỏ và báo số còn lại.
- Đếm tổng kho bằng COUNT bộ không MAT/NGUNG_THUE; đếm còn thuê được theo hàm kiểm tra.
  Không lấy tổng kho trừ tất cả số lượng đơn: phải so ngày chồng của từng bộ.
- Chốt tên/size/màu/giá vào KhiThue, không lấy tiền từ trình duyệt.
  TienThue = SoLuong * (GiaGoiKhiThue + MAX(0,số ngày - SoNgayTrongGoiKhiThue)*GiaNgayThemKhiThue).
  TienCoc dòng = SoLuong * cọc đơn giá. Mã chỉ giảm tiền thuê; tính giới hạn lượt với khóa phù hợp.
- Giao: GIU_CHO->DA_GIAO; kiểm tra đúng các bộ, SAN_SANG và chưa bị khách khác giữ quá hạn;
  ghi ThoiDiemGiao, chuyển bộ DANG_THUE. Không giao trước NgayNhan.
- Trả toàn dòng: DA_GIAO->DA_TRA, ghi ThoiDiemTra, phí, chuyển bộ DANG_GIAT/DANG_SUA.
  Hoàn tất vệ sinh/kiểm tra rồi mới chuyển SAN_SANG. Không xóa phân bộ đã trả để giữ lịch sử.
  Tình trạng bộ hiện tại và lịch phân bộ là hai thông tin khác nhau: giữ lịch tương lai
  không làm bộ thành DANG_THUE ngay hôm nay.
- Hủy trước giao: đổi đơn/dòng sang DA_HUY trong cùng khóa kho. Lịch hủy không chặn bộ.
  Không tự hoàn tiền; ghi ThanhToan khoản hoàn thực tế. Không sửa/mở lại lịch cũ tùy tiện.
- Đổi ngày/số lượng: dùng cùng khóa, kiểm tra lại từng bộ, phân lại và tính lại tiền/mã.
  Không UPDATE ngày trực tiếp rồi commit mà bỏ qua kiểm tra. Service có hàm giao dịch
  và PhanBoDongAsync để ứng dụng tái phân bộ cho dòng GIU_CHO sau khi sửa trong transaction.
- Tạo đơn chống bấm lặp ở C#, xóa giỏ sau khi ghi đơn thành công, không ghi tiền thành công giả.
  PhuongThucThanhToan là dự định, ThanhToan.PhươngThuc là cách thực thu/hoàn.
  Theo dõi cọc riêng, không cộng cọc vào doanh thu; không hoàn vượt tiền thực thu còn lại.
- Lịch thử: đăng nhập, chọn sản phẩm/chi nhánh/ngày giờ tương lai. C# kiểm tra giờ mở cửa,
  sức chứa và lịch chồng, lưu CHO_XAC_NHAN; muốn đổi giờ phải kiểm tra lại và xác nhận lại.
  Hủy CHO_XAC_NHAN trước giờ; DA_XAC_NHAN trước >=24giờ. Ghi thời điểm/người hủy,
  nối lý do mới vào GhiChu, giữ ghi chú cũ; kiểm tra quyền tại server.
- Hình ảnh do C# quản lý, bản database này không có bảng hình ảnh sản phẩm.
  Ẩn sản phẩm/khóa tài khoản/chi nhánh thay vì xóa lịch sử thuê/thanh toán.
Dữ liệu mẫu:2 chi nhánh,13 dòng sản phẩm(size/màu),26 bộ tại chi nhánh1,MIRA10.
Không có tài khoản/đơn/giao dịch giả. Các mã/địa chỉ minh họa cần nhóm thay thông tin thật.
*/
