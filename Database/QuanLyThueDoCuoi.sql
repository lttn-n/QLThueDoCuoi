
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
GO

IF OBJECT_ID(N'dbo.TaiKhoan', N'U') IS NOT NULL
    THROW 50001, N'Database da co bang. Khong chay lai file khoi tao; hay dung script cap nhat.', 1;
GO

BEGIN TRY
    BEGIN TRANSACTION;

CREATE TABLE dbo.TaiKhoan (
    MaTaiKhoan INT IDENTITY PRIMARY KEY,
    HoTen NVARCHAR(100) NOT NULL,
    Email VARCHAR(254) NULL,
    SoDienThoai VARCHAR(20) NULL,
    MatKhauHash NVARCHAR(500) NOT NULL,
    VaiTro VARCHAR(15) NOT NULL DEFAULT 'KHACHHANG',
    DiaChi NVARCHAR(500) NULL,
    TrangThai BIT NOT NULL DEFAULT 1,
    NgayTao DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT CK_TK_LienLac CHECK (Email IS NOT NULL OR SoDienThoai IS NOT NULL),
    CONSTRAINT CK_TK_Email CHECK (Email IS NULL OR LEN(LTRIM(RTRIM(Email))) > 0),
    CONSTRAINT CK_TK_SDT CHECK (SoDienThoai IS NULL OR LEN(LTRIM(RTRIM(SoDienThoai))) > 0),
    CONSTRAINT CK_TK_VaiTro CHECK (VaiTro IN ('ADMIN','NHANVIEN','KHACHHANG'))
);
CREATE UNIQUE INDEX UX_TK_Email ON dbo.TaiKhoan(Email) WHERE Email IS NOT NULL;
CREATE UNIQUE INDEX UX_TK_SDT ON dbo.TaiKhoan(SoDienThoai) WHERE SoDienThoai IS NOT NULL;

CREATE TABLE dbo.ChiNhanh (
    MaChiNhanh INT IDENTITY PRIMARY KEY,
    TenChiNhanh NVARCHAR(100) NOT NULL UNIQUE,
    DiaChi NVARCHAR(500) NOT NULL,
    SoDienThoai VARCHAR(20) NULL,
    TrangThai BIT NOT NULL DEFAULT 1,
    CONSTRAINT CK_ChiNhanh_ThongTin CHECK (
        LEN(LTRIM(RTRIM(TenChiNhanh))) > 0 AND LEN(LTRIM(RTRIM(DiaChi))) > 0)
);

CREATE TABLE dbo.LoaiSanPham (
    MaLoaiSanPham INT IDENTITY PRIMARY KEY,
    MaLoaiCha INT NULL REFERENCES dbo.LoaiSanPham(MaLoaiSanPham),
    TenLoaiSanPham NVARCHAR(100) NOT NULL,
    HienThi BIT NOT NULL DEFAULT 1,
    CONSTRAINT CK_DM_Cha CHECK (MaLoaiCha IS NULL OR MaLoaiCha <> MaLoaiSanPham),
    CONSTRAINT CK_DM_Ten CHECK (LEN(LTRIM(RTRIM(TenLoaiSanPham))) > 0)
);
CREATE INDEX IX_LoaiSanPham_Cha ON dbo.LoaiSanPham(MaLoaiCha);

CREATE TABLE dbo.SanPham (
    MaSanPham INT IDENTITY PRIMARY KEY,
    MaLoaiSanPham INT NOT NULL REFERENCES dbo.LoaiSanPham(MaLoaiSanPham),
    TenSanPham NVARCHAR(200) NOT NULL,
    KichThuoc NVARCHAR(30) NOT NULL,
    MauSac NVARCHAR(50) NOT NULL,
    MoTa NVARCHAR(MAX) NULL,
    ChatLieu NVARCHAR(200) NULL,
    DuongDanAnh NVARCHAR(500) NULL,
    GiaThueGoi DECIMAL(18,2) NOT NULL,
    SoNgayThueTrongGoi INT NOT NULL DEFAULT 3,
    GiaThueMoiNgayThem DECIMAL(18,2) NOT NULL DEFAULT 0,
    TienCoc DECIMAL(18,2) NOT NULL DEFAULT 0,
    SoNgayVeSinh INT NOT NULL DEFAULT 1,
    GiaKhuyenMai DECIMAL(18,2) NULL,
    NgayBatDauGiamGia DATETIME2(0) NULL,
    NgayKetThucGiamGia DATETIME2(0) NULL,
    TrangThai BIT NOT NULL DEFAULT 1,
    NoiBat BIT NOT NULL DEFAULT 0,
    NhanHienThi NVARCHAR(20) NULL,
    NgayTao DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT UQ_SanPham UNIQUE (MaLoaiSanPham,TenSanPham,KichThuoc,MauSac),
    CONSTRAINT CK_SP_Ten_Size_Mau CHECK (
        LEN(LTRIM(RTRIM(TenSanPham))) > 0 AND
        LEN(LTRIM(RTRIM(KichThuoc))) > 0 AND
        LEN(LTRIM(RTRIM(MauSac))) > 0),
    CONSTRAINT CK_SP_Anh CHECK (DuongDanAnh IS NULL OR LEN(LTRIM(RTRIM(DuongDanAnh))) > 0),
    CONSTRAINT CK_SP_Gia CHECK (GiaThueGoi >= 0 AND GiaThueMoiNgayThem >= 0 AND TienCoc >= 0),
    CONSTRAINT CK_SP_Ngay CHECK (SoNgayThueTrongGoi BETWEEN 1 AND 365 AND SoNgayVeSinh BETWEEN 0 AND 365),
    CONSTRAINT CK_SP_Giam CHECK (
        (GiaKhuyenMai IS NULL AND NgayBatDauGiamGia IS NULL AND NgayKetThucGiamGia IS NULL) OR
        (GiaKhuyenMai IS NOT NULL AND NgayBatDauGiamGia IS NOT NULL AND NgayKetThucGiamGia IS NOT NULL
         AND GiaKhuyenMai >= 0 AND GiaKhuyenMai < GiaThueGoi AND NgayKetThucGiamGia > NgayBatDauGiamGia))
);
CREATE INDEX IX_SanPham_Loai_TrangThai ON dbo.SanPham(MaLoaiSanPham,TrangThai) INCLUDE (TenSanPham,GiaThueGoi);

CREATE TABLE dbo.HinhAnhSanPham (
    MaHinh INT IDENTITY PRIMARY KEY,
    MaSanPham INT NOT NULL REFERENCES dbo.SanPham(MaSanPham) ON DELETE CASCADE,
    DuongDan NVARCHAR(500) NOT NULL,
    ThuTu INT NOT NULL DEFAULT 0,
    CONSTRAINT CK_Hinh_DuongDan CHECK (LEN(LTRIM(RTRIM(DuongDan))) > 0)
);
CREATE INDEX IX_Hinh_SanPham ON dbo.HinhAnhSanPham(MaSanPham,ThuTu);

CREATE TABLE dbo.BoSuuTap (
    MaBoSuuTap INT IDENTITY PRIMARY KEY,
    TenBoSuuTap NVARCHAR(200) NOT NULL,
    MoTa NVARCHAR(1000) NULL,
    DuongDanAnh NVARCHAR(500) NULL,
    HienThi BIT NOT NULL DEFAULT 1,
    CONSTRAINT CK_BST_Ten CHECK (LEN(LTRIM(RTRIM(TenBoSuuTap))) > 0)
);

CREATE TABLE dbo.BoSuuTapSanPham (
    MaBoSuuTap INT NOT NULL REFERENCES dbo.BoSuuTap(MaBoSuuTap) ON DELETE CASCADE,
    MaSanPham INT NOT NULL REFERENCES dbo.SanPham(MaSanPham) ON DELETE CASCADE,
    PRIMARY KEY (MaBoSuuTap, MaSanPham)
);

CREATE TABLE dbo.BoTrangPhuc (
    MaBoTrangPhuc INT IDENTITY PRIMARY KEY,
    MaSanPham INT NOT NULL REFERENCES dbo.SanPham(MaSanPham),
    MaChiNhanh INT NOT NULL REFERENCES dbo.ChiNhanh(MaChiNhanh),
    MaQuanLy VARCHAR(50) NOT NULL UNIQUE, 
    TinhTrang VARCHAR(20) NOT NULL DEFAULT 'SAN_SANG',
    GhiChu NVARCHAR(1000) NULL,
    CONSTRAINT UQ_Bo_SanPham UNIQUE (MaBoTrangPhuc,MaSanPham),
    CONSTRAINT CK_Bo_TinhTrang CHECK (TinhTrang IN
        ('SAN_SANG','DANG_THUE','DANG_GIAT','DANG_SUA','HU_HONG','MAT','NGUNG_THUE'))
);
CREATE INDEX IX_Bo_SanPham ON dbo.BoTrangPhuc(MaSanPham,MaChiNhanh,TinhTrang);

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
    TrangThai BIT NOT NULL DEFAULT 1,
    CONSTRAINT CK_KM_Ten CHECK (LEN(LTRIM(RTRIM(TenKhuyenMai))) > 0 AND LEN(LTRIM(RTRIM(MaUuDai))) > 0),
    CONSTRAINT CK_KM_Loai CHECK (HinhThucGiam IN ('PHANTRAM','SOTIEN')),
    CONSTRAINT CK_KM_Gia CHECK (GiaTriGiam > 0 AND (HinhThucGiam <> 'PHANTRAM' OR GiaTriGiam <= 100)
        AND GiaTriDonToiThieu >= 0 AND (SoTienGiamToiDa IS NULL OR SoTienGiamToiDa > 0)),
    CONSTRAINT CK_KM_Ngay CHECK (NgayKetThuc > NgayBatDau),
    CONSTRAINT CK_KM_Luot CHECK (SoLuotSuDungToiDa IS NULL OR SoLuotSuDungToiDa > 0)
);
CREATE TABLE dbo.GioThue (
    MaGioThue INT IDENTITY PRIMARY KEY,
    MaTaiKhoan INT NOT NULL UNIQUE REFERENCES dbo.TaiKhoan(MaTaiKhoan),
    MaKhuyenMai INT NULL REFERENCES dbo.KhuyenMai(MaKhuyenMai),
    NgayTao DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME()
);

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
CREATE UNIQUE INDEX UX_Gio_ChuaChonNgay
    ON dbo.ChiTietGioThue(MaGioThue,MaSanPham)
    WHERE NgayNhan IS NULL AND NgayTraDuKien IS NULL;

CREATE TABLE dbo.DonThue (
    MaDonThue INT IDENTITY PRIMARY KEY,
    MaTaiKhoan INT NOT NULL REFERENCES dbo.TaiKhoan(MaTaiKhoan),
    NgayDat DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
    TrangThai VARCHAR(20) NOT NULL DEFAULT 'CHO_XAC_NHAN',
    TenNguoiNhan NVARCHAR(100) NOT NULL,
    SoDienThoaiNhan VARCHAR(20) NOT NULL,
    HinhThucNhan VARCHAR(15) NOT NULL, 
    MaChiNhanhNhan INT NULL REFERENCES dbo.ChiNhanh(MaChiNhanh),
    PhuongThucThanhToan VARCHAR(15) NOT NULL, 
    DiaChiNhan NVARCHAR(500) NULL,
    PhiGiaoNhan DECIMAL(18,2) NOT NULL DEFAULT 0,
    MaKhuyenMai INT NULL REFERENCES dbo.KhuyenMai(MaKhuyenMai),
    SoTienGiam DECIMAL(18,2) NOT NULL DEFAULT 0,
    GhiChu NVARCHAR(1000) NULL,
    CONSTRAINT CK_Don_ThongTinNhan CHECK (
        LEN(LTRIM(RTRIM(TenNguoiNhan))) > 0 AND
        LEN(LTRIM(RTRIM(SoDienThoaiNhan))) > 0),
    CONSTRAINT CK_Don_TT CHECK (TrangThai IN ('CHO_XAC_NHAN','DA_XAC_NHAN','DANG_THUE','HOAN_TAT','DA_HUY')),
    CONSTRAINT CK_Don_Nhan CHECK (HinhThucNhan IN ('CUAHANG','GIAOHANG')),
    CONSTRAINT CK_Don_NoiNhan CHECK (
        (HinhThucNhan='CUAHANG' AND MaChiNhanhNhan IS NOT NULL) OR
        (HinhThucNhan='GIAOHANG' AND MaChiNhanhNhan IS NULL
         AND DiaChiNhan IS NOT NULL AND LEN(LTRIM(RTRIM(DiaChiNhan)))>0)),
    CONSTRAINT CK_Don_PhuongThuc CHECK (PhuongThucThanhToan IN ('TIENMAT','CHUYENKHOAN')),
    CONSTRAINT CK_Don_Tien CHECK (PhiGiaoNhan >= 0 AND SoTienGiam >= 0)
);
CREATE INDEX IX_DonThue_TaiKhoan_NgayDat
    ON dbo.DonThue(MaTaiKhoan,NgayDat);
CREATE INDEX IX_DonThue_TrangThai_NgayDat
    ON dbo.DonThue(TrangThai,NgayDat);

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
    GiaGoiKhiThue DECIMAL(18,2) NOT NULL,
    SoNgayTrongGoiKhiThue INT NOT NULL,
    GiaNgayThemKhiThue DECIMAL(18,2) NOT NULL,
    TienThue DECIMAL(18,2) NOT NULL,
    TienCoc DECIMAL(18,2) NOT NULL,
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
CREATE INDEX IX_ChiTietDon_MaDonThue
    ON dbo.ChiTietDonThue(MaDonThue);
CREATE INDEX IX_PhanBo_Bo ON dbo.BoTrangPhucTrongDon(MaBoTrangPhuc,MaChiTietDonThue);
CREATE INDEX IX_ChiTiet_SanPham_Ngay ON dbo.ChiTietDonThue(MaSanPham,NgayNhan,NgayTraDuKien,TrangThai);

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
CREATE INDEX IX_ThanhToan_Don_TrangThai
    ON dbo.ThanhToan(MaDonThue,TrangThai);
CREATE UNIQUE INDEX UX_GD_ThamChieu ON dbo.ThanhToan(PhuongThuc,MaThamChieu) WHERE MaThamChieu IS NOT NULL;
CREATE TABLE dbo.LichHenThuDo (
    MaLichHen INT IDENTITY PRIMARY KEY,
    MaTaiKhoan INT NOT NULL REFERENCES dbo.TaiKhoan(MaTaiKhoan),
    MaSanPham INT NOT NULL REFERENCES dbo.SanPham(MaSanPham),
    MaChiNhanh INT NOT NULL REFERENCES dbo.ChiNhanh(MaChiNhanh),
    HoTenLienHe NVARCHAR(100) NOT NULL,
    SoDienThoaiLienHe VARCHAR(20) NOT NULL,
    ThoiDiemHen DATETIME2(0) NOT NULL,
    TrangThai VARCHAR(15) NOT NULL DEFAULT 'CHO_XAC_NHAN',
    GhiChu NVARCHAR(2000) NULL, 
    NgayTao DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
    ThoiDiemHuy DATETIME2(0) NULL,
    MaTaiKhoanHuy INT NULL REFERENCES dbo.TaiKhoan(MaTaiKhoan),
    CONSTRAINT CK_Hen_TT CHECK (TrangThai IN ('CHO_XAC_NHAN','DA_XAC_NHAN','DA_DEN','HOAN_TAT','DA_HUY')),
    CONSTRAINT CK_Hen_LienHe CHECK (LEN(LTRIM(RTRIM(HoTenLienHe))) > 0
        AND LEN(LTRIM(RTRIM(SoDienThoaiLienHe))) > 0),
    CONSTRAINT CK_Hen_ThongTinHuy CHECK (
        (TrangThai='DA_HUY' AND ThoiDiemHuy IS NOT NULL AND MaTaiKhoanHuy IS NOT NULL
         AND GhiChu IS NOT NULL AND LEN(LTRIM(RTRIM(GhiChu)))>0)
        OR (TrangThai<>'DA_HUY' AND ThoiDiemHuy IS NULL AND MaTaiKhoanHuy IS NULL))
);
CREATE UNIQUE INDEX UX_Hen_KhongTrung ON dbo.LichHenThuDo(MaTaiKhoan,MaSanPham,ThoiDiemHen)
    WHERE TrangThai <> 'DA_HUY';

CREATE INDEX IX_LichHen_ChiNhanh_ThoiDiem
    ON dbo.LichHenThuDo(MaChiNhanh,ThoiDiemHen,TrangThai);

CREATE INDEX IX_LichHen_TaiKhoan
    ON dbo.LichHenThuDo(MaTaiKhoan,ThoiDiemHen);

INSERT dbo.ChiNhanh(TenChiNhanh,DiaChi,SoDienThoai) VALUES
(N'Dragneel Shop - Chi nhánh 1',N'12 Nguyễn Huệ, Quận 1, TP. Hồ Chí Minh','0901234567'),
(N'Dragneel Shop - Chi nhánh 2',N'45 Lê Lợi, Quận 3, TP. Hồ Chí Minh','0907654321');

INSERT dbo.LoaiSanPham(TenLoaiSanPham) VALUES (N'Trang phục cưới');
DECLARE @cha INT=CONVERT(INT,SCOPE_IDENTITY());
INSERT dbo.LoaiSanPham(MaLoaiCha,TenLoaiSanPham) VALUES
(@cha,N'Váy cưới'),(@cha,N'Áo dài'),(@cha,N'Vest chú rể'),(@cha,N'Phụ kiện');

CREATE TABLE #SP (Loai NVARCHAR(100), Ten NVARCHAR(200), Mau NVARCHAR(50), ChatLieu NVARCHAR(200), MoTa NVARCHAR(MAX),
                  Anh NVARCHAR(500), Gia DECIMAL(18,2), Goi INT, VeSinh INT, Km INT, NoiBat BIT, Nhan NVARCHAR(20));
INSERT #SP VALUES
(N'Váy cưới',N'Váy cưới Élodie',N'Trắng',N'Organza',N'Váy cưới dáng chữ A.',N'/images/trang-chu/vay-cuoi.jpg',2800000,3,1,0,1,N'BÁN CHẠY'),
(N'Váy cưới',N'Váy cưới Seraphine',N'Trắng',N'Ren Pháp',N'Cổ điển với thân váy ren thêu tay, tay lỡ nhẹ nhàng.',N'/images/dragneel/elodie.svg',3200000,3,1,0,0,N'MỚI'),
(N'Váy cưới',N'Váy cưới Aurelia',N'Trắng ngà',N'Satin',N'Dáng đuôi cá ôm eo, tôn đường cong cô dâu.',N'/images/dragneel/elodie.svg',3500000,3,1,10,0,N''),
(N'Váy cưới',N'Váy cưới Celeste',N'Trắng',N'Tulle',N'Dáng công chúa xòe rộng, nhiều lớp voan mềm.',N'/images/dragneel/elodie.svg',4200000,4,1,0,0,N'BÁN CHẠY'),
(N'Váy cưới',N'Váy cưới Margaux',N'Champagne',N'Satin lụa',N'Hở lưng quyến rũ, tà váy mềm đổ tự nhiên.',N'/images/dragneel/elodie.svg',3800000,3,1,0,0,N''),
(N'Váy cưới',N'Váy cưới Isabeau',N'Trắng ngà',N'Ren',N'Tay dài kín đáo, cổ cao thanh lịch phong cách hoàng gia.',N'/images/dragneel/elodie.svg',3600000,3,1,0,0,N''),
(N'Váy cưới',N'Váy cưới Odette',N'Trắng',N'Taffeta',N'Cúp ngực chữ A, eo nhấn đính đá nhỏ.',N'/images/dragneel/elodie.svg',3000000,3,1,15,0,N''),
(N'Váy cưới',N'Váy cưới Liora',N'Hồng phấn',N'Organza',N'Váy xòe tùng nhẹ, màu hồng phấn ngọt ngào cho tiệc ngoài trời.',N'/images/dragneel/elodie.svg',2900000,3,1,0,0,N''),
(N'Váy cưới',N'Váy cưới Valentina',N'Trắng',N'Ren Chantilly',N'Ren Chantilly thêu nổi toàn thân, đuôi váy dài 1m.',N'/images/dragneel/elodie.svg',5200000,4,1,0,0,N'NỔI BẬT'),
(N'Váy cưới',N'Váy cưới Giselle',N'Trắng ngà',N'Satin mikado',N'Tối giản, phom suông sang trọng, hợp lễ cưới hiện đại.',N'/images/dragneel/elodie.svg',3400000,3,1,0,0,N''),
(N'Váy cưới',N'Váy cưới Amelie',N'Trắng',N'Crepe',N'Cổ thuyền thanh lịch, tay phồng nhẹ phong cách Pháp.',N'/images/dragneel/elodie.svg',3100000,3,1,0,0,N''),
(N'Váy cưới',N'Váy cưới Rosalind',N'Trắng',N'Chiffon',N'Voan bay nhẹ, lý tưởng cho đám cưới bãi biển.',N'/images/dragneel/elodie.svg',2600000,3,1,0,0,N''),
(N'Váy cưới',N'Váy cưới Beatrix',N'Trắng',N'Satin',N'Hai dây mảnh, dáng slip dress cho cô dâu cá tính.',N'/images/dragneel/elodie.svg',2700000,3,1,10,0,N''),
(N'Váy cưới',N'Váy cưới Clementine',N'Trắng ngà',N'Tulle đính hạt',N'Đính hạt pha lê thủ công, lấp lánh dưới ánh đèn.',N'/images/dragneel/elodie.svg',4800000,4,1,0,0,N''),
(N'Váy cưới',N'Váy cưới Delphine',N'Trắng',N'Organza lụa',N'Dáng chữ A cổ điển, thân váy gọn và đuôi ngắn.',N'/images/dragneel/elodie.svg',2950000,3,1,0,0,N''),
(N'Váy cưới',N'Váy cưới Eleonora',N'Trắng',N'Satin',N'Đuôi dài 2m, trải thảm đỏ hoành tráng.',N'/images/dragneel/elodie.svg',5500000,4,1,0,0,N''),
(N'Váy cưới',N'Váy cưới Florence',N'Trắng',N'Ren hoa',N'Ren hoa 3D nổi, nhẹ nhàng như khu vườn mùa xuân.',N'/images/dragneel/elodie.svg',4100000,3,1,0,0,N''),
(N'Váy cưới',N'Váy cưới Genevieve',N'Trắng ngà',N'Crepe lụa',N'Cổ vuông thanh lịch, thân váy suông thả.',N'/images/dragneel/elodie.svg',3300000,3,1,0,0,N''),
(N'Váy cưới',N'Váy cưới Henriette',N'Champagne',N'Tulle',N'Tùng xòe màu champagne ánh vàng nhẹ, rất ăn ảnh.',N'/images/dragneel/elodie.svg',3900000,3,1,0,0,N''),
(N'Váy cưới',N'Váy cưới Ivanna',N'Trắng',N'Satin',N'Phong cách tối giản Bắc Âu, đường cắt gọn gàng.',N'/images/dragneel/elodie.svg',3500000,3,1,12,0,N''),
(N'Áo dài',N'Áo dài Minh Châu',N'Trắng',N'Lụa',N'Áo dài cưới thêu họa tiết truyền thống.',N'/images/dragneel/aodai.svg',2000000,3,1,10,1,N''),
(N'Áo dài',N'Áo dài Thanh Tâm',N'Đỏ',N'Gấm',N'Gấm đỏ thêu rồng phụng, dành cho lễ rước dâu.',N'/images/dragneel/aodai.svg',2400000,3,1,0,0,N'BÁN CHẠY'),
(N'Áo dài',N'Áo dài Bích Ngọc',N'Xanh ngọc',N'Lụa',N'Xanh ngọc nhẹ nhàng, thêu hoa sen tay áo.',N'/images/dragneel/aodai.svg',1900000,3,1,0,0,N''),
(N'Áo dài',N'Áo dài Diễm Quỳnh',N'Hồng',N'Lụa',N'Hồng đào dịu dàng, cổ thuyền cách tân.',N'/images/dragneel/aodai.svg',1800000,3,1,10,0,N''),
(N'Áo dài',N'Áo dài Hồng Nhung',N'Đỏ',N'Nhung',N'Nhung đỏ đô sang trọng cho mùa cưới thu đông.',N'/images/dragneel/aodai.svg',2600000,3,1,0,0,N'MỚI'),
(N'Áo dài',N'Áo dài Kim Ngân',N'Vàng',N'Gấm',N'Vàng kim thêu chỉ nổi, hợp lễ gia tiên.',N'/images/dragneel/aodai.svg',2500000,3,1,0,0,N''),
(N'Áo dài',N'Áo dài Lệ Quyên',N'Trắng',N'Voan',N'Voan mỏng nhẹ, thêu ren tinh tế.',N'/images/dragneel/aodai.svg',1700000,3,1,0,0,N''),
(N'Áo dài',N'Áo dài Mỹ Duyên',N'Hồng phấn',N'Lụa',N'Hồng phấn nữ tính, tay loe nhẹ.',N'/images/dragneel/aodai.svg',1850000,3,1,0,0,N''),
(N'Áo dài',N'Áo dài Ngọc Lan',N'Trắng ngà',N'Lụa',N'Thêu hoa ngọc lan nhã nhặn ở vạt áo.',N'/images/dragneel/aodai.svg',2100000,3,1,0,0,N''),
(N'Áo dài',N'Áo dài Phương Anh',N'Xanh dương',N'Gấm',N'Xanh hoàng gia họa tiết hoa văn nổi.',N'/images/dragneel/aodai.svg',2300000,3,1,0,0,N''),
(N'Áo dài',N'Áo dài Quỳnh Hoa',N'Trắng',N'Ren',N'Ren nổi toàn thân, kết hợp quần lụa.',N'/images/dragneel/aodai.svg',2700000,4,1,0,0,N'NỔI BẬT'),
(N'Áo dài',N'Áo dài Thúy Vi',N'Xanh lá',N'Lụa',N'Xanh lá mạ trẻ trung, thêu hoa cúc.',N'/images/dragneel/aodai.svg',1750000,3,1,0,0,N''),
(N'Áo dài',N'Áo dài Uyên Nhi',N'Hồng',N'Voan',N'Hồng pastel phối khăn đóng.',N'/images/dragneel/aodai.svg',1950000,3,1,0,0,N''),
(N'Áo dài',N'Áo dài Vân Khánh',N'Đỏ',N'Lụa',N'Đỏ tươi truyền thống, thêu long phụng.',N'/images/dragneel/aodai.svg',2200000,3,1,15,0,N''),
(N'Áo dài',N'Áo dài Xuân Hương',N'Vàng',N'Lụa',N'Vàng nhạt dịu mắt, họa tiết hoa mai.',N'/images/dragneel/aodai.svg',1800000,3,1,0,0,N''),
(N'Áo dài',N'Áo dài Yến Nhi',N'Trắng',N'Gấm',N'Gấm trắng ánh nhũ bạc sang trọng.',N'/images/dragneel/aodai.svg',2450000,3,1,0,0,N''),
(N'Áo dài',N'Áo dài Bảo Trâm',N'Tím',N'Lụa',N'Tím lavender lạ mắt, thêu hoa nhài.',N'/images/dragneel/aodai.svg',1900000,3,1,0,0,N''),
(N'Áo dài',N'Áo dài Cẩm Tú',N'Đỏ',N'Gấm',N'Gấm đỏ thêu hoa mẫu đơn, vạt dài trang trọng.',N'/images/dragneel/aodai.svg',2800000,4,1,0,0,N''),
(N'Áo dài',N'Áo dài Đan Thanh',N'Xanh ngọc',N'Voan',N'Voan xanh ngọc mát mắt, họa tiết trúc.',N'/images/dragneel/aodai.svg',1650000,3,1,10,0,N''),
(N'Áo dài',N'Áo dài Gia Hân',N'Hồng',N'Nhung',N'Nhung hồng cánh sen thêu tay.',N'/images/dragneel/aodai.svg',2650000,3,1,0,0,N''),
(N'Vest chú rể',N'Vest nam Modern Fit',N'Đen',N'Vải wool blend',N'Vest chú rể phom hiện đại.',N'/images/trang-chu/vest.jpg',1600000,3,1,0,1,N'MỚI'),
(N'Vest chú rể',N'Vest nam Classic Tuxedo',N'Đen',N'Wool',N'Tuxedo cổ điển ve lụa, kèm quần và nơ.',N'/images/dragneel/vest.svg',2200000,3,1,0,0,N'BÁN CHẠY'),
(N'Vest chú rể',N'Vest nam Slim Navy',N'Xanh navy',N'Wool blend',N'Navy slim fit tôn dáng, dùng được nhiều dịp.',N'/images/dragneel/vest.svg',1800000,3,1,0,0,N''),
(N'Vest chú rể',N'Vest nam Charcoal 3 Piece',N'Xám than',N'Wool',N'Bộ 3 mảnh gồm áo, gile và quần, sang trọng.',N'/images/dragneel/vest.svg',2500000,3,1,0,0,N'MỚI'),
(N'Vest chú rể',N'Vest nam Midnight Tuxedo',N'Xanh đêm',N'Nhung',N'Tuxedo xanh đêm cho tiệc cưới buổi tối.',N'/images/dragneel/vest.svg',2400000,3,1,0,0,N''),
(N'Vest chú rể',N'Vest nam Burgundy Velvet',N'Đỏ đô',N'Nhung',N'Nhung đỏ đô nổi bật, ve áo bóng.',N'/images/dragneel/vest.svg',2600000,3,1,0,0,N''),
(N'Vest chú rể',N'Vest nam Ivory Dinner',N'Trắng ngà',N'Wool',N'Dinner jacket trắng ngà phong cách cổ điển.',N'/images/dragneel/vest.svg',2300000,3,1,0,0,N''),
(N'Vest chú rể',N'Vest nam Champagne Gold',N'Champagne',N'Satin',N'Màu champagne ánh vàng, hợp váy cô dâu cùng tông.',N'/images/dragneel/vest.svg',2000000,3,1,10,0,N''),
(N'Vest chú rể',N'Vest nam Forest Green',N'Xanh rêu',N'Wool',N'Xanh rêu trầm, hợp tiệc cưới ngoài trời.',N'/images/dragneel/vest.svg',1900000,3,1,0,0,N''),
(N'Vest chú rể',N'Vest nam Light Gray Summer',N'Xám nhạt',N'Cotton',N'Xám nhạt thoáng mát cho mùa hè.',N'/images/dragneel/vest.svg',1500000,3,1,0,0,N''),
(N'Vest chú rể',N'Vest nam Brown Tweed',N'Nâu',N'Tweed',N'Tweed nâu ấm áp cho đám cưới phong cách vintage.',N'/images/dragneel/vest.svg',1900000,3,1,0,0,N''),
(N'Vest chú rể',N'Vest nam Double Breasted',N'Đen',N'Wool',N'Hai hàng khuy mạnh mẽ, phom cắt chuẩn.',N'/images/dragneel/vest.svg',2100000,3,1,0,0,N''),
(N'Vest chú rể',N'Vest nam Mandarin',N'Đen',N'Lụa',N'Cổ trụ phong cách Á Đông, không cần cà vạt.',N'/images/dragneel/vest.svg',1700000,3,1,0,0,N''),
(N'Vest chú rể',N'Vest nam White Tuxedo',N'Trắng',N'Wool',N'Tuxedo trắng cho chú rể nổi bật.',N'/images/dragneel/vest.svg',2800000,3,1,0,0,N'NỔI BẬT'),
(N'Vest chú rể',N'Vest nam Sky Blue Linen',N'Xanh da trời',N'Linen',N'Linen xanh da trời nhẹ nhàng, hợp tiệc bãi biển.',N'/images/dragneel/vest.svg',1400000,3,1,0,0,N''),
(N'Vest chú rể',N'Vest nam Beige Linen',N'Be',N'Linen',N'Be thanh lịch, thoáng khí cho tiệc ban ngày.',N'/images/dragneel/vest.svg',1400000,3,1,10,0,N''),
(N'Vest chú rể',N'Vest nam Pinstripe Navy',N'Xanh navy',N'Wool',N'Kẻ sọc mảnh phong cách doanh nhân thanh lịch.',N'/images/dragneel/vest.svg',2000000,3,1,0,0,N''),
(N'Vest chú rể',N'Vest nam Royal Blue',N'Xanh hoàng gia',N'Wool blend',N'Xanh hoàng gia nổi bật, đường may sắc nét.',N'/images/dragneel/vest.svg',2200000,3,1,0,0,N''),
(N'Vest chú rể',N'Vest nam Dusty Blue',N'Xanh xám',N'Wool blend',N'Xanh xám dịu mắt, hợp tông váy pastel.',N'/images/dragneel/vest.svg',1850000,3,1,0,0,N''),
(N'Vest chú rể',N'Vest nam Emerald Velvet',N'Xanh ngọc lục bảo',N'Nhung',N'Nhung lục bảo sang trọng cho tiệc tối.',N'/images/dragneel/vest.svg',2700000,4,1,0,0,N''),
(N'Phụ kiện',N'Set phụ kiện Pearl',N'Trắng ngọc trai',N'Ngọc trai nhân tạo',N'Bộ phụ kiện ngọc trai cho cô dâu.',N'/images/dragneel/pearl.svg',650000,3,1,0,1,N'NỔI BẬT'),
(N'Phụ kiện',N'Khăn voan ren dài',N'Trắng',N'Ren',N'Voan dài 3m viền ren, phủ đuôi váy.',N'/images/dragneel/pearl.svg',450000,3,1,0,0,N'BÁN CHẠY'),
(N'Phụ kiện',N'Voan cài tóc ngắn',N'Trắng ngà',N'Tulle',N'Voan ngắn tới vai, kèm lược cài.',N'/images/dragneel/pearl.svg',250000,3,1,0,0,N''),
(N'Phụ kiện',N'Vương miện pha lê',N'Bạc',N'Pha lê',N'Vương miện đính pha lê lấp lánh.',N'/images/dragneel/pearl.svg',500000,3,1,10,0,N'NỔI BẬT'),
(N'Phụ kiện',N'Băng đô ngọc trai',N'Trắng ngọc trai',N'Ngọc trai nhân tạo',N'Băng đô thanh lịch cho kiểu tóc búi.',N'/images/dragneel/pearl.svg',220000,3,1,0,0,N''),
(N'Phụ kiện',N'Hoa cài tóc lụa',N'Trắng',N'Lụa',N'Bộ 5 hoa cài tóc lụa thủ công.',N'/images/dragneel/pearl.svg',180000,3,1,0,0,N''),
(N'Phụ kiện',N'Bông tai ngọc trai',N'Trắng ngọc trai',N'Ngọc trai nhân tạo',N'Bông tai hạt ngọc trai thả dài.',N'/images/dragneel/pearl.svg',200000,3,1,0,0,N''),
(N'Phụ kiện',N'Vòng cổ pha lê',N'Bạc',N'Pha lê',N'Dây chuyền pha lê ôm cổ thanh mảnh.',N'/images/dragneel/pearl.svg',350000,3,1,0,0,N''),
(N'Phụ kiện',N'Găng tay ren',N'Trắng',N'Ren',N'Găng tay ren dài qua khuỷu.',N'/images/dragneel/pearl.svg',260000,3,1,0,0,N''),
(N'Phụ kiện',N'Giày cao gót satin',N'Trắng',N'Satin',N'Cao gót 7cm đế êm, đính nơ nhỏ.',N'/images/dragneel/pearl.svg',420000,3,1,0,0,N''),
(N'Phụ kiện',N'Bó hoa cầm tay lụa',N'Trắng hồng',N'Hoa lụa',N'Bó hoa lụa cao cấp, không tàn úa.',N'/images/dragneel/pearl.svg',300000,3,1,0,0,N''),
(N'Phụ kiện',N'Cà vạt lụa chú rể',N'Xanh navy',N'Lụa',N'Cà vạt lụa bản vừa, kèm khăn cài túi.',N'/images/dragneel/pearl.svg',150000,3,1,0,0,N''),
(N'Phụ kiện',N'Nơ cổ chú rể',N'Đen',N'Satin',N'Nơ cổ bản lớn cho tuxedo.',N'/images/dragneel/pearl.svg',120000,3,1,0,0,N''),
(N'Phụ kiện',N'Khuy măng sét',N'Bạc',N'Kim loại mạ bạc',N'Cặp khuy măng sét kèm ghim cà vạt.',N'/images/dragneel/pearl.svg',160000,3,1,0,0,N''),
(N'Phụ kiện',N'Hoa cài ve áo',N'Trắng',N'Hoa lụa',N'Hoa cài ve áo chú rể phối cùng bó hoa.',N'/images/dragneel/pearl.svg',90000,3,1,0,0,N''),
(N'Phụ kiện',N'Thắt lưng ngọc cô dâu',N'Bạc',N'Pha lê',N'Thắt lưng đính ngọc nhấn eo váy.',N'/images/dragneel/pearl.svg',380000,3,1,0,0,N''),
(N'Phụ kiện',N'Túi cầm tay cô dâu',N'Trắng',N'Satin',N'Túi clutch nhỏ đựng đồ dặm.',N'/images/dragneel/pearl.svg',280000,3,1,0,0,N''),
(N'Phụ kiện',N'Áo choàng bolero',N'Trắng',N'Lông nhân tạo',N'Bolero lông mềm cho tiệc cưới mùa lạnh.',N'/images/dragneel/pearl.svg',550000,3,1,0,0,N''),
(N'Phụ kiện',N'Khăn choàng ren',N'Trắng ngà',N'Ren',N'Khăn choàng ren mỏng nhẹ khoác vai.',N'/images/dragneel/pearl.svg',330000,3,1,0,0,N''),
(N'Phụ kiện',N'Đuôi váy rời',N'Trắng',N'Tulle',N'Đuôi váy rời có thể tháo gắn linh hoạt.',N'/images/dragneel/pearl.svg',900000,4,1,0,0,N'');

INSERT dbo.SanPham(MaLoaiSanPham,TenSanPham,KichThuoc,MauSac,MoTa,ChatLieu,DuongDanAnh,GiaThueGoi,SoNgayThueTrongGoi,
                   GiaThueMoiNgayThem,TienCoc,SoNgayVeSinh,GiaKhuyenMai,NgayBatDauGiamGia,NgayKetThucGiamGia,NoiBat,NhanHienThi)
SELECT l.MaLoaiSanPham, t.Ten, z.KichThuoc, t.Mau, t.MoTa, t.ChatLieu, t.Anh, t.Gia, t.Goi,
       ROUND(t.Gia * 0.14, -4), ROUND(t.Gia * 0.7, -5), t.VeSinh,
       CASE WHEN t.Km > 0 THEN ROUND(t.Gia * (100 - t.Km) / 100.0, -3) END,
       CASE WHEN t.Km > 0 THEN DATEADD(DAY, -1, SYSUTCDATETIME()) END,
       CASE WHEN t.Km > 0 THEN DATEADD(DAY, 90, SYSUTCDATETIME()) END,
       t.NoiBat, NULLIF(t.Nhan, N'')
FROM #SP t
JOIN dbo.LoaiSanPham l ON l.TenLoaiSanPham = t.Loai
CROSS JOIN (VALUES (N'S'),(N'M'),(N'L'),(N'XL'),(N'Freesize')) z(KichThuoc)
WHERE (t.Loai = N'Phụ kiện' AND z.KichThuoc = N'Freesize') OR (t.Loai <> N'Phụ kiện' AND z.KichThuoc <> N'Freesize')
ORDER BY l.MaLoaiSanPham, t.Ten, z.KichThuoc;
DROP TABLE #SP;

INSERT dbo.BoTrangPhuc(MaSanPham,MaChiNhanh,MaQuanLy)
SELECT s.MaSanPham,c.MaChiNhanh,CONCAT('DS-',s.MaSanPham,'-',n.STT)
FROM dbo.SanPham s CROSS JOIN (VALUES(1),(2)) n(STT)
CROSS JOIN dbo.ChiNhanh c WHERE c.TenChiNhanh=N'Dragneel Shop - Chi nhánh 1';
INSERT dbo.BoTrangPhuc(MaSanPham,MaChiNhanh,MaQuanLy)
SELECT s.MaSanPham,c.MaChiNhanh,CONCAT('DS-',s.MaSanPham,'-B1')
FROM dbo.SanPham s CROSS JOIN dbo.ChiNhanh c WHERE c.TenChiNhanh=N'Dragneel Shop - Chi nhánh 2';

INSERT dbo.BoSuuTap(TenBoSuuTap,MoTa) VALUES
(N'Bộ sưu tập ngày chung đôi',N'Váy, áo dài, vest và phụ kiện hài hòa cho cặp đôi.'),
(N'Phong cách Việt',N'Áo dài cưới truyền thống thêu tay cho lễ rước dâu và gia tiên.'),
(N'Chú rể lịch lãm',N'Vest và tuxedo từ cổ điển đến hiện đại dành cho chú rể.');
INSERT dbo.BoSuuTapSanPham(MaBoSuuTap,MaSanPham)
SELECT b.MaBoSuuTap,s.MaSanPham FROM dbo.BoSuuTap b CROSS JOIN dbo.SanPham s
WHERE b.TenBoSuuTap=N'Bộ sưu tập ngày chung đôi';
INSERT dbo.BoSuuTapSanPham(MaBoSuuTap,MaSanPham)
SELECT b.MaBoSuuTap,s.MaSanPham FROM dbo.BoSuuTap b
JOIN dbo.SanPham s ON s.MaLoaiSanPham=(SELECT MaLoaiSanPham FROM dbo.LoaiSanPham WHERE TenLoaiSanPham=N'Áo dài')
WHERE b.TenBoSuuTap=N'Phong cách Việt';
INSERT dbo.BoSuuTapSanPham(MaBoSuuTap,MaSanPham)
SELECT b.MaBoSuuTap,s.MaSanPham FROM dbo.BoSuuTap b
JOIN dbo.SanPham s ON s.MaLoaiSanPham=(SELECT MaLoaiSanPham FROM dbo.LoaiSanPham WHERE TenLoaiSanPham=N'Vest chú rể')
WHERE b.TenBoSuuTap=N'Chú rể lịch lãm';

INSERT dbo.KhuyenMai(MaUuDai,TenKhuyenMai,HinhThucGiam,GiaTriGiam,SoTienGiamToiDa,GiaTriDonToiThieu,NgayBatDau,NgayKetThuc,SoLuotSuDungToiDa)
VALUES
('DRAGNEEL10',N'Ưu đãi khai trương 10%','PHANTRAM',10,500000,1000000,DATEADD(DAY,-1,SYSUTCDATETIME()),DATEADD(DAY,90,SYSUTCDATETIME()),100),
('CUOI2026',N'Mùa cưới 2026 giảm 15%','PHANTRAM',15,800000,3000000,DATEADD(DAY,-1,SYSUTCDATETIME()),DATEADD(DAY,120,SYSUTCDATETIME()),200),
('COMBO300K',N'Giảm 300.000đ cho đơn từ 4 triệu','SOTIEN',300000,NULL,4000000,DATEADD(DAY,-1,SYSUTCDATETIME()),DATEADD(DAY,120,SYSUTCDATETIME()),150),
('KHACHMOI5',N'Khách mới giảm 5%','PHANTRAM',5,300000,0,DATEADD(DAY,-1,SYSUTCDATETIME()),DATEADD(DAY,120,SYSUTCDATETIME()),500);

INSERT dbo.TaiKhoan(HoTen,Email,SoDienThoai,MatKhauHash,VaiTro,DiaChi) VALUES
(N'Quản trị Dragneel','admin@dragneel.vn','0900000001',N'AQAAAAIAAYagAAAAEDJciuXP3RJ6eOiw09rV7t3G2vPYATyfssnNayLjGriJJ0qwG0ypGlx8OdYLCM/pTA==','ADMIN',NULL),
(N'Nhân viên Dragneel','nhanvien@dragneel.vn','0900000002',N'AQAAAAIAAYagAAAAEFt/RvjKAnKxX+njgSzMhejyfNsQqhJwI4aFsqP/kTaV0cpKfFTVXGBS+POGwCCgWQ==','NHANVIEN',NULL),
(N'Nguyễn Thị Lan','lan.nguyen@example.com','0912000001',N'AQAAAAIAAYagAAAAEAXlAC4wR26Zf+GCnI68qOGV1xYvkwsk+tV7hAnB8DqGMTeNWIUbGI5qBTN8DlDcLA==','KHACHHANG',N'25 Hai Bà Trưng, Quận 1, TP. Hồ Chí Minh'),
(N'Trần Quốc Minh','minh.tran@example.com','0912000002',N'AQAAAAIAAYagAAAAELx5autQsMU3Uuh8r5OGbSrF2rukpHsw9+VcoheEXjJlTz182vxTX8CuzSh9K1kSQQ==','KHACHHANG',N'88 Cách Mạng Tháng 8, Quận 3, TP. Hồ Chí Minh'),
(N'Lê Phương Thảo','thao.le@example.com','0912000003',N'AQAAAAIAAYagAAAAEP8Z+Hnv/cqh+Wec9jUCyw13FlgwfVbFE0ChiSMYmoeRKC+W9XS+Xz4QaZJe22KbKA==','KHACHHANG',NULL),
(N'Phạm Gia Huy','huy.pham@example.com','0912000004',N'AQAAAAIAAYagAAAAEAt9F6HeZiXcTyY1UV7qwA3eO/iliCbs4mjoh/XpKaiJ6ocfMENaLghkDoeKaS3FpA==','KHACHHANG',NULL);

DECLARE @hom DATE = CAST(DATEADD(HOUR,7,SYSUTCDATETIME()) AS DATE);
DECLARE @cn1 INT = (SELECT MaChiNhanh FROM dbo.ChiNhanh WHERE TenChiNhanh=N'Dragneel Shop - Chi nhánh 1');
DECLARE @cn2 INT = (SELECT MaChiNhanh FROM dbo.ChiNhanh WHERE TenChiNhanh=N'Dragneel Shop - Chi nhánh 2');
DECLARE @k1 INT = (SELECT MaTaiKhoan FROM dbo.TaiKhoan WHERE Email='lan.nguyen@example.com');
DECLARE @k2 INT = (SELECT MaTaiKhoan FROM dbo.TaiKhoan WHERE Email='minh.tran@example.com');
DECLARE @k3 INT = (SELECT MaTaiKhoan FROM dbo.TaiKhoan WHERE Email='thao.le@example.com');
DECLARE @k4 INT = (SELECT MaTaiKhoan FROM dbo.TaiKhoan WHERE Email='huy.pham@example.com');
DECLARE @dA INT,@dB INT,@dC INT,@dD INT,@dE INT,@dF INT;
INSERT dbo.DonThue(MaTaiKhoan,TrangThai,TenNguoiNhan,SoDienThoaiNhan,HinhThucNhan,MaChiNhanhNhan,PhuongThucThanhToan,GhiChu)
VALUES (@k1,'DA_XAC_NHAN',N'Nguyễn Thị Lan','0912000001','CUAHANG',@cn1,'CHUYENKHOAN',N'Đơn mẫu A');
SET @dA=CONVERT(INT,SCOPE_IDENTITY());
INSERT dbo.DonThue(MaTaiKhoan,TrangThai,TenNguoiNhan,SoDienThoaiNhan,HinhThucNhan,MaChiNhanhNhan,PhuongThucThanhToan,GhiChu)
VALUES (@k2,'DA_XAC_NHAN',N'Trần Quốc Minh','0912000002','CUAHANG',@cn1,'TIENMAT',N'Đơn mẫu B');
SET @dB=CONVERT(INT,SCOPE_IDENTITY());
INSERT dbo.DonThue(MaTaiKhoan,TrangThai,TenNguoiNhan,SoDienThoaiNhan,HinhThucNhan,MaChiNhanhNhan,PhuongThucThanhToan,GhiChu)
VALUES (@k3,'DANG_THUE',N'Lê Phương Thảo','0912000003','CUAHANG',@cn1,'TIENMAT',N'Đơn mẫu C: đã giao');
SET @dC=CONVERT(INT,SCOPE_IDENTITY());
INSERT dbo.DonThue(MaTaiKhoan,TrangThai,TenNguoiNhan,SoDienThoaiNhan,HinhThucNhan,MaChiNhanhNhan,PhuongThucThanhToan,GhiChu)
VALUES (@k4,'CHO_XAC_NHAN',N'Phạm Gia Huy','0912000004','CUAHANG',@cn2,'CHUYENKHOAN',N'Đơn mẫu D: chờ xác nhận');
SET @dD=CONVERT(INT,SCOPE_IDENTITY());
INSERT dbo.DonThue(MaTaiKhoan,TrangThai,TenNguoiNhan,SoDienThoaiNhan,HinhThucNhan,MaChiNhanhNhan,PhuongThucThanhToan,GhiChu)
VALUES (@k1,'HOAN_TAT',N'Nguyễn Thị Lan','0912000001','CUAHANG',@cn1,'TIENMAT',N'Đơn mẫu E: đã trả');
SET @dE=CONVERT(INT,SCOPE_IDENTITY());
INSERT dbo.DonThue(MaTaiKhoan,TrangThai,TenNguoiNhan,SoDienThoaiNhan,HinhThucNhan,MaChiNhanhNhan,PhuongThucThanhToan,GhiChu)
VALUES (@k2,'DA_HUY',N'Trần Quốc Minh','0912000002','CUAHANG',@cn1,'TIENMAT',N'Đơn mẫu F: khách hủy');
SET @dF=CONVERT(INT,SCOPE_IDENTITY());

INSERT dbo.ChiTietDonThue(MaDonThue,MaSanPham,SoLuong,TenSanPhamKhiThue,KichThuocKhiThue,MauSacKhiThue,NgayNhan,NgayTraDuKien,
    SoNgayVeSinhKhiThue,GiaGoiKhiThue,SoNgayTrongGoiKhiThue,GiaNgayThemKhiThue,TienThue,TienCoc,TrangThai,ThoiDiemGiao,ThoiDiemTra)
SELECT v.Don, s.MaSanPham, v.SL, s.TenSanPham, s.KichThuoc, s.MauSac, DATEADD(DAY,v.N,@hom), DATEADD(DAY,v.T,@hom),
       s.SoNgayVeSinh, s.GiaThueGoi, s.SoNgayThueTrongGoi, s.GiaThueMoiNgayThem,
       (s.GiaThueGoi + CASE WHEN v.T-v.N > s.SoNgayThueTrongGoi THEN (v.T-v.N-s.SoNgayThueTrongGoi)*s.GiaThueMoiNgayThem ELSE 0 END) * v.SL,
       s.TienCoc * v.SL, v.TT,
       CASE WHEN v.TT IN ('DA_GIAO','DA_TRA') THEN DATEADD(HOUR,9,CAST(DATEADD(DAY,v.N,@hom) AS DATETIME2(0))) END,
       CASE WHEN v.TT = 'DA_TRA' THEN DATEADD(HOUR,17,CAST(DATEADD(DAY,v.T,@hom) AS DATETIME2(0))) END
FROM (VALUES
    (@dA,N'Váy cưới Élodie',N'M',2,4,6,'GIU_CHO'),
    (@dA,N'Áo dài Minh Châu',N'S',1,4,6,'GIU_CHO'),
    (@dB,N'Váy cưới Élodie',N'M',1,5,7,'GIU_CHO'),
    (@dC,N'Vest nam Modern Fit',N'L',3,-1,2,'DA_GIAO'),
    (@dD,N'Set phụ kiện Pearl',N'Freesize',3,10,12,'GIU_CHO'),
    (@dE,N'Áo dài Thanh Tâm',N'S',1,-20,-17,'DA_TRA'),
    (@dF,N'Váy cưới Seraphine',N'L',3,5,8,'DA_HUY')
) v(Don,Ten,Size,SL,N,T,TT)
JOIN dbo.SanPham s ON s.TenSanPham = v.Ten AND s.KichThuoc = v.Size;

INSERT dbo.BoTrangPhucTrongDon(MaChiTietDonThue,MaBoTrangPhuc,MaSanPham)
SELECT c.MaChiTietDonThue, b.MaBoTrangPhuc, b.MaSanPham
FROM dbo.ChiTietDonThue c JOIN dbo.BoTrangPhuc b ON b.MaSanPham = c.MaSanPham WHERE c.TrangThai = 'DA_GIAO';
INSERT dbo.BoTrangPhucTrongDon(MaChiTietDonThue,MaBoTrangPhuc,MaSanPham)
SELECT c.MaChiTietDonThue, x.MaBoTrangPhuc, c.MaSanPham
FROM dbo.ChiTietDonThue c
CROSS APPLY (SELECT TOP 1 b.MaBoTrangPhuc FROM dbo.BoTrangPhuc b WHERE b.MaSanPham = c.MaSanPham ORDER BY b.MaBoTrangPhuc) x
WHERE c.TrangThai = 'DA_TRA';
UPDATE b SET TinhTrang = 'DANG_THUE'
FROM dbo.BoTrangPhuc b
JOIN dbo.BoTrangPhucTrongDon p ON p.MaBoTrangPhuc = b.MaBoTrangPhuc
JOIN dbo.ChiTietDonThue c ON c.MaChiTietDonThue = p.MaChiTietDonThue
WHERE c.TrangThai = 'DA_GIAO';

INSERT dbo.ThanhToan(MaDonThue,LoaiGiaoDich,SoTien,PhuongThuc,TrangThai,MaThamChieu,ThoiDiemHoanTat,GhiChu)
SELECT d.MaDonThue,'THU_COC',SUM(c.TienCoc),d.PhuongThucThanhToan,'THANH_CONG',CONCAT('DEMO-COC-',d.MaDonThue),SYSUTCDATETIME(),N'Thu cọc (dữ liệu mẫu)'
FROM dbo.DonThue d JOIN dbo.ChiTietDonThue c ON c.MaDonThue = d.MaDonThue
WHERE d.TrangThai IN ('DA_XAC_NHAN','DANG_THUE','HOAN_TAT')
GROUP BY d.MaDonThue, d.PhuongThucThanhToan;

INSERT dbo.LichHenThuDo(MaTaiKhoan,MaSanPham,MaChiNhanh,HoTenLienHe,SoDienThoaiLienHe,ThoiDiemHen,TrangThai)
SELECT @k1,s.MaSanPham,@cn1,N'Nguyễn Thị Lan','0912000001',DATEADD(HOUR,10,CAST(DATEADD(DAY,2,@hom) AS DATETIME2(0))),'CHO_XAC_NHAN'
FROM dbo.SanPham s WHERE s.TenSanPham=N'Váy cưới Élodie' AND s.KichThuoc=N'M';
INSERT dbo.LichHenThuDo(MaTaiKhoan,MaSanPham,MaChiNhanh,HoTenLienHe,SoDienThoaiLienHe,ThoiDiemHen,TrangThai)
SELECT @k2,s.MaSanPham,@cn2,N'Trần Quốc Minh','0912000002',DATEADD(HOUR,14,CAST(DATEADD(DAY,3,@hom) AS DATETIME2(0))),'DA_XAC_NHAN'
FROM dbo.SanPham s WHERE s.TenSanPham=N'Áo dài Minh Châu' AND s.KichThuoc=N'S';
INSERT dbo.LichHenThuDo(MaTaiKhoan,MaSanPham,MaChiNhanh,HoTenLienHe,SoDienThoaiLienHe,ThoiDiemHen,TrangThai,GhiChu,ThoiDiemHuy,MaTaiKhoanHuy)
SELECT @k3,s.MaSanPham,@cn1,N'Lê Phương Thảo','0912000003',DATEADD(HOUR,9,CAST(DATEADD(DAY,1,@hom) AS DATETIME2(0))),'DA_HUY',N'Khách bận việc, hẹn lại sau',SYSUTCDATETIME(),@k3
FROM dbo.SanPham s WHERE s.TenSanPham=N'Vest nam Modern Fit' AND s.KichThuoc=N'L';

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
