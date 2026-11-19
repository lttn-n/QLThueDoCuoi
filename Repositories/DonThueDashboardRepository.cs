namespace QLThueDoCuoi.Repositories;

public sealed class DonThueDashboardRepository(DiemDb db)
{
    public async Task<int> DemDonChoXacNhan() //Lấy từ table dbo.DonThue, điều kiện TrangThai = 'CHO_XAC_NHAN'
    {
        var rows = await db.Query(
            "SELECT COUNT(*) AS SoLuong FROM dbo.DonThue WHERE TrangThai=@trangThai",
            DiemDb.P("@trangThai", "CHO_XAC_NHAN"));

        return Convert.ToInt32(rows[0]["SoLuong"]);
    }

    public async Task<int> DemLichHenSapToi() // Lấy từ table dbo.LichHenThuDo, đếm các đơn có điều kiện trong tương lai đang chờ hoặc đã xác nhận.
    {
        var rows = await db.Query(
            @"SELECT COUNT(*) AS SoLuong
              FROM dbo.LichHenThuDo
              WHERE ThoiDiemHen >= SYSUTCDATETIME()
                AND TrangThai IN (@choXacNhan, @daXacNhan)",
            DiemDb.P("@choXacNhan", "CHO_XAC_NHAN"),
            DiemDb.P("@daXacNhan", "DA_XAC_NHAN"));

        return Convert.ToInt32(rows[0]["SoLuong"]);
    }

    public async Task<decimal> TienCocDangGiu() // Lấy từ table dbo.ThanhToan, tính tổng tiền cọc đang giữ (đã thu nhưng chưa hoàn trả)
    {
        var rows = await db.Query(
            @"SELECT COALESCE(SUM(
                  CASE
                      WHEN LoaiGiaoDich=@thuCoc THEN SoTien
                      WHEN LoaiGiaoDich=@hoanCoc THEN -SoTien
                      ELSE 0
                  END), 0) AS TongTien
              FROM dbo.ThanhToan
              WHERE TrangThai=@thanhCong
                AND LoaiGiaoDich IN (@thuCoc, @hoanCoc)",
            DiemDb.P("@thuCoc", "THU_COC"),
            DiemDb.P("@hoanCoc", "HOAN_COC"),
            DiemDb.P("@thanhCong", "THANH_CONG"));

        return Convert.ToDecimal(rows[0]["TongTien"]);
    }

    public async Task<decimal> DoanhThuThucThu() // Lấy từ table dbo.ThanhToan, tính tổng doanh thu thực thu (đã thu trừ đi các khoản hoàn trả)
    {
        var rows = await db.Query(
            @"SELECT COALESCE(SUM(
                  CASE
                      WHEN LoaiGiaoDich IN (@thuThue, @thuPhuPhi) THEN SoTien
                      WHEN LoaiGiaoDich IN (@hoanThue, @hoanPhuPhi) THEN -SoTien
                      ELSE 0
                  END), 0) AS TongTien
              FROM dbo.ThanhToan
              WHERE TrangThai=@thanhCong
                AND LoaiGiaoDich IN (@thuThue, @thuPhuPhi, @hoanThue, @hoanPhuPhi)",
            DiemDb.P("@thuThue", "THU_THUE"),
            DiemDb.P("@thuPhuPhi", "THU_PHUPHI"),
            DiemDb.P("@hoanThue", "HOAN_THUE"),
            DiemDb.P("@hoanPhuPhi", "HOAN_PHUPHI"),
            DiemDb.P("@thanhCong", "THANH_CONG"));

        return Convert.ToDecimal(rows[0]["TongTien"]);
    }

    public Task<List<Dictionary<string, object?>>> DoanhThuTheoThang() // Lấy từ table dbo.ThanhToan, tính tổng doanh thu thực thu theo từng tháng trong 6 tháng gần nhất
    {
        return db.Query(
            @"DECLARE @thangHienTai DATE =
                  DATEFROMPARTS(YEAR(SYSUTCDATETIME()), MONTH(SYSUTCDATETIME()), 1);
              WITH CacThang AS
              (
                  SELECT DATEADD(MONTH, n.ThangOffset, @thangHienTai) AS NgayDauThang
                  FROM (VALUES (-5), (-4), (-3), (-2), (-1), (0)) n(ThangOffset)
              )
              SELECT MONTH(m.NgayDauThang) AS Thang,
                     YEAR(m.NgayDauThang) AS Nam,
                     COALESCE(SUM(
                         CASE
                             WHEN tt.LoaiGiaoDich IN (@thuThue, @thuPhuPhi) THEN tt.SoTien
                             WHEN tt.LoaiGiaoDich IN (@hoanThue, @hoanPhuPhi) THEN -tt.SoTien
                             ELSE 0
                         END), 0) AS TongTien
              FROM CacThang m
              LEFT JOIN dbo.ThanhToan tt
                  ON tt.ThoiDiemHoanTat >= m.NgayDauThang
                 AND tt.ThoiDiemHoanTat < DATEADD(MONTH, 1, m.NgayDauThang)
                 AND tt.TrangThai=@thanhCong
                 AND tt.LoaiGiaoDich IN (@thuThue, @thuPhuPhi, @hoanThue, @hoanPhuPhi)
              GROUP BY m.NgayDauThang
              ORDER BY m.NgayDauThang",
            DiemDb.P("@thuThue", "THU_THUE"),
            DiemDb.P("@thuPhuPhi", "THU_PHUPHI"),
            DiemDb.P("@hoanThue", "HOAN_THUE"),
            DiemDb.P("@hoanPhuPhi", "HOAN_PHUPHI"),
            DiemDb.P("@thanhCong", "THANH_CONG"));
    }

     public Task<List<Dictionary<string, object?>>> LichHenSapToi() // Lấy từ table dbo.LichHenThuDo, lấy danh sách các lịch hẹn sắp tới (2 lịch hẹn gần nhất) đang chờ hoặc đã xác nhận
    {
        return db.Query(
            @"SELECT TOP (@soLuong)
                     h.MaLichHen, s.TenSanPham, h.ThoiDiemHen, h.HoTenLienHe
              FROM dbo.LichHenThuDo h
              INNER JOIN dbo.SanPham s ON s.MaSanPham=h.MaSanPham
              WHERE h.ThoiDiemHen >= SYSUTCDATETIME()
                AND h.TrangThai IN (@choXacNhan, @daXacNhan)
              ORDER BY h.ThoiDiemHen",
            DiemDb.P("@soLuong", 2),
            DiemDb.P("@choXacNhan", "CHO_XAC_NHAN"),
            DiemDb.P("@daXacNhan", "DA_XAC_NHAN"));
    }

    public Task<List<Dictionary<string, object?>>> DonThueGanDay()
    {
        return db.Query(
            @"SELECT TOP (@soLuong)
                     d.MaDonThue, d.NgayDat, d.TenNguoiNhan, d.TrangThai,
                     MIN(ct.NgayNhan) AS NgayNhan,
                     MAX(ct.NgayTraDuKien) AS NgayTraDuKien,
                     SUM(ct.TienThue) AS TongTienThue,
                     COUNT(DISTINCT ct.MaChiTietDonThue) AS SoDong,
                     (SELECT TOP 1 ct2.TenSanPhamKhiThue
                      FROM dbo.ChiTietDonThue ct2
                      WHERE ct2.MaDonThue=d.MaDonThue
                      ORDER BY ct2.MaChiTietDonThue) AS TenSanPham
              FROM dbo.DonThue d
              INNER JOIN dbo.ChiTietDonThue ct ON ct.MaDonThue=d.MaDonThue
              GROUP BY d.MaDonThue, d.NgayDat, d.TenNguoiNhan, d.TrangThai
              ORDER BY d.NgayDat DESC, d.MaDonThue DESC",
            DiemDb.P("@soLuong", 3));
    }
}
