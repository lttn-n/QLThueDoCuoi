using QLThueDoCuoi.Repositories;

namespace QLThueDoCuoi.Services;

public sealed class AdminHomeService(DonThueDashboardRepository repository)
{
    public Task<int> DemDonChoXacNhan() => repository.DemDonChoXacNhan();
    public Task<int> DemLichHenSapToi() => repository.DemLichHenSapToi();
    public Task<decimal> TienCocDangGiu() => repository.TienCocDangGiu();
    public Task<decimal> DoanhThuThucThu() => repository.DoanhThuThucThu();
    public Task<List<Dictionary<string, object?>>> DoanhThuTheoThang() => repository.DoanhThuTheoThang();
    public Task<List<Dictionary<string, object?>>> LichHenSapToi() => repository.LichHenSapToi();
    public Task<List<Dictionary<string, object?>>> DonThueGanDay() => repository.DonThueGanDay();
}
