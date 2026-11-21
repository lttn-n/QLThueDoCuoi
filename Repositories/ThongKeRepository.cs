namespace QLThueDoCuoi.Repositories;

// Repository đọc dữ liệu báo cáo từ dbo.ThanhToan.
// Chỉ tính giao dịch THANH_CONG; tiền cọc không tính là doanh thu.
public sealed class ThongKeRepository(DiemDb db)
{
    public async Task<Dictionary<string, object?>> LayTongHop(DateTime tuUtc, DateTime denUtc)
    {
        var rows = await db.Query(
            @"SELECT
                  COALESCE(SUM(CASE WHEN LoaiGiaoDich=@thuThue THEN SoTien ELSE 0 END),0) AS ThuThue,
                  COALESCE(SUM(CASE WHEN LoaiGiaoDich=@thuPhuPhi THEN SoTien ELSE 0 END),0) AS ThuPhuPhi,
                  COALESCE(SUM(CASE WHEN LoaiGiaoDich IN (@hoanThue,@hoanPhuPhi) THEN SoTien ELSE 0 END),0) AS HoanTien,
                  COALESCE(SUM(CASE
                      WHEN LoaiGiaoDich IN (@thuThue,@thuPhuPhi) THEN SoTien
                      WHEN LoaiGiaoDich IN (@hoanThue,@hoanPhuPhi) THEN -SoTien
                      ELSE 0
                  END),0) AS DoanhThu
              FROM dbo.ThanhToan
              WHERE TrangThai=@thanhCong
                AND ThoiDiemHoanTat >= @tu
                AND ThoiDiemHoanTat < @den",
            DiemDb.P("@thuThue", "THU_THUE"),
            DiemDb.P("@thuPhuPhi", "THU_PHUPHI"),
            DiemDb.P("@hoanThue", "HOAN_THUE"),
            DiemDb.P("@hoanPhuPhi", "HOAN_PHUPHI"),
            DiemDb.P("@thanhCong", "THANH_CONG"),
            DiemDb.P("@tu", tuUtc),
            DiemDb.P("@den", denUtc));

        return rows.Count == 0
            ? new Dictionary<string, object?>()
            : rows[0];
    }

    public Task<List<Dictionary<string, object?>>> LayDoanhThuTheoThang(DateTime tuUtc, DateTime denUtc)
        => db.Query(
            @"SELECT
                  YEAR(ThoiDiemHoanTat) AS Nam,
                  MONTH(ThoiDiemHoanTat) AS Thang,
                  COALESCE(SUM(CASE
                      WHEN LoaiGiaoDich IN (@thuThue,@thuPhuPhi) THEN SoTien
                      WHEN LoaiGiaoDich IN (@hoanThue,@hoanPhuPhi) THEN -SoTien
                      ELSE 0
                  END),0) AS DoanhThu
              FROM dbo.ThanhToan
              WHERE TrangThai=@thanhCong
                AND ThoiDiemHoanTat >= @tu
                AND ThoiDiemHoanTat < @den
                AND LoaiGiaoDich IN (@thuThue,@thuPhuPhi,@hoanThue,@hoanPhuPhi)
              GROUP BY YEAR(ThoiDiemHoanTat), MONTH(ThoiDiemHoanTat)
              ORDER BY YEAR(ThoiDiemHoanTat), MONTH(ThoiDiemHoanTat)",
            DiemDb.P("@thuThue", "THU_THUE"),
            DiemDb.P("@thuPhuPhi", "THU_PHUPHI"),
            DiemDb.P("@hoanThue", "HOAN_THUE"),
            DiemDb.P("@hoanPhuPhi", "HOAN_PHUPHI"),
            DiemDb.P("@thanhCong", "THANH_CONG"),
            DiemDb.P("@tu", tuUtc),
            DiemDb.P("@den", denUtc));
}
