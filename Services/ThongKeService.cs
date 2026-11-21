using QLThueDoCuoi.Repositories;

namespace QLThueDoCuoi.Services;

// Service chuyển ngày theo giờ Việt Nam sang UTC trước khi truy vấn DATETIME2.
public sealed class ThongKeService(ThongKeRepository repo)
{
    public async Task<(Dictionary<string, object?> TongHop,
        List<Dictionary<string, object?>> TheoThang)> LayBaoCao(DateOnly tuNgay, DateOnly denNgay)
    {
        if (denNgay < tuNgay)
            throw new ArgumentException("Ngày kết thúc phải bằng hoặc sau ngày bắt đầu.");

        var tuUtc = tuNgay.ToDateTime(TimeOnly.MinValue).AddHours(-7);
        var denUtc = denNgay.AddDays(1).ToDateTime(TimeOnly.MinValue).AddHours(-7);
        var tongHop = await repo.LayTongHop(tuUtc, denUtc);
        var theoThang = await repo.LayDoanhThuTheoThang(tuUtc, denUtc);
        return (tongHop, theoThang);
    }
}
