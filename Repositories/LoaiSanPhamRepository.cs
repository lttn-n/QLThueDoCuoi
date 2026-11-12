using Microsoft.Data.SqlClient;
using QLThueDoCuoi.Data;
using QLThueDoCuoi.Models;
namespace QLThueDoCuoi.Repositories;

public sealed class LoaiSanPhamRepository(KetNoiDatabase ketNoi)
{
    public async Task<List<LoaiSanPham>> LayDanhSachHienThiAsync()
    {
        var ds = new List<LoaiSanPham>();
        await using var kn = ketNoi.TaoKetNoi(SqlClientFactory.Instance);
        await kn.OpenAsync();
        await using var lenh = kn.CreateCommand();
        lenh.CommandText = "SELECT MaLoaiSanPham, MaLoaiCha, TenLoaiSanPham, HienThi FROM dbo.LoaiSanPham WHERE HienThi = 1 ORDER BY MaLoaiSanPham";
        await using var doc = await lenh.ExecuteReaderAsync();
        while (await doc.ReadAsync())
            ds.Add(new LoaiSanPham { MaLoaiSanPham = doc.GetInt32(0), MaLoaiCha = doc.IsDBNull(1) ? null : doc.GetInt32(1), TenLoaiSanPham = doc.GetString(2), HienThi = doc.GetBoolean(3) });
        return ds;
    }
}
