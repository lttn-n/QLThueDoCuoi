namespace QLThueDoCuoi.Repositories;

public sealed class DonThueDashboardRepository(DiemDb db)
{
    public async Task<int> DemDonChoXacNhan()
    {
        var rows = await db.Query(
            "SELECT COUNT(*) AS SoLuong FROM dbo.DonThue WHERE TrangThai=@trangThai",
            DiemDb.P("@trangThai", "CHO_XAC_NHAN"));

        return Convert.ToInt32(rows[0]["SoLuong"]);
    }
}
