using QLThueDoCuoi.Repositories;

namespace QLThueDoCuoi.Services;

public sealed class AdminHomeService(DonThueDashboardRepository repository)
{
    public Task<int> DemDonChoXacNhan() => repository.DemDonChoXacNhan();
}
