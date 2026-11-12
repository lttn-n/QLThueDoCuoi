using System.Data.Common;
namespace QLThueDoCuoi.Data;
public sealed class KetNoiDatabase(IConfiguration cauHinh)
{
    public DbConnection TaoKetNoi(DbProviderFactory provider)
    {
        var c = provider.CreateConnection() ?? throw new InvalidOperationException("Không tạo được kết nối.");
        c.ConnectionString = cauHinh.GetConnectionString("QuanLyThueDoCuoi")
            ?? cauHinh.GetConnectionString("QLThueDoCuoi")
            ?? throw new InvalidOperationException("Thiếu chuỗi kết nối.");
        return c;
    }
}
