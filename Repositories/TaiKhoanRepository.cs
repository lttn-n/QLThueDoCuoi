using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using QLThueDoCuoi.Data;
using QLThueDoCuoi.Models;
namespace QLThueDoCuoi.Repositories;

public sealed class TaiKhoanRepository(KetNoiDatabase ketNoi)
{
    const string Cot = "MaTaiKhoan, HoTen, Email, SoDienThoai, MatKhauHash, VaiTro, DiaChi, TrangThai, NgayTao";

    public Task<TaiKhoan?> LayTheoLienLacAsync(string giaTri) =>
        LayMotAsync($"SELECT TOP 1 {Cot} FROM dbo.TaiKhoan WHERE Email = @v OR SoDienThoai = @v", ("@v", giaTri, DbType.AnsiString));

    public Task<TaiKhoan?> LayTheoMaAsync(int ma) =>
        LayMotAsync($"SELECT {Cot} FROM dbo.TaiKhoan WHERE MaTaiKhoan = @ma", ("@ma", ma, DbType.Int32));

    public async Task<(bool TrungEmail, bool TrungSdt)> KiemTraTrungAsync(string? email, string? sdt, int boQuaMa = 0)
    {
        await using var kn = ketNoi.TaoKetNoi(SqlClientFactory.Instance);
        await kn.OpenAsync();
        await using var lenh = kn.CreateCommand();
        lenh.CommandText = @"SELECT
    CASE WHEN @email IS NOT NULL AND EXISTS (SELECT 1 FROM dbo.TaiKhoan WHERE Email = @email AND MaTaiKhoan <> @ma) THEN 1 ELSE 0 END,
    CASE WHEN @sdt IS NOT NULL AND EXISTS (SELECT 1 FROM dbo.TaiKhoan WHERE SoDienThoai = @sdt AND MaTaiKhoan <> @ma) THEN 1 ELSE 0 END";
        ThamSo(lenh, "@email", email, DbType.AnsiString);
        ThamSo(lenh, "@sdt", sdt, DbType.AnsiString);
        ThamSo(lenh, "@ma", boQuaMa, DbType.Int32);
        await using var doc = await lenh.ExecuteReaderAsync();
        await doc.ReadAsync();
        return (doc.GetInt32(0) == 1, doc.GetInt32(1) == 1);
    }

    public async Task<int> TaoAsync(TaiKhoan tk)
    {
        await using var kn = ketNoi.TaoKetNoi(SqlClientFactory.Instance);
        await kn.OpenAsync();
        await using var lenh = kn.CreateCommand();
        lenh.CommandText = @"INSERT dbo.TaiKhoan(HoTen, Email, SoDienThoai, MatKhauHash, VaiTro)
OUTPUT INSERTED.MaTaiKhoan
VALUES (@ten, @email, @sdt, @hash, @vaiTro)";
        ThamSo(lenh, "@ten", tk.HoTen, DbType.String);
        ThamSo(lenh, "@email", tk.Email, DbType.AnsiString);
        ThamSo(lenh, "@sdt", tk.SoDienThoai, DbType.AnsiString);
        ThamSo(lenh, "@hash", tk.MatKhauHash, DbType.String);
        ThamSo(lenh, "@vaiTro", tk.VaiTro, DbType.AnsiString);
        return Convert.ToInt32(await lenh.ExecuteScalarAsync());
    }

    public async Task CapNhatHoSoAsync(int ma, string hoTen, string? email, string? sdt, string? diaChi)
    {
        await using var kn = ketNoi.TaoKetNoi(SqlClientFactory.Instance);
        await kn.OpenAsync();
        await using var lenh = kn.CreateCommand();
        lenh.CommandText = "UPDATE dbo.TaiKhoan SET HoTen = @ten, Email = @email, SoDienThoai = @sdt, DiaChi = @dc WHERE MaTaiKhoan = @ma";
        ThamSo(lenh, "@ten", hoTen, DbType.String);
        ThamSo(lenh, "@email", email, DbType.AnsiString);
        ThamSo(lenh, "@sdt", sdt, DbType.AnsiString);
        ThamSo(lenh, "@dc", diaChi, DbType.String);
        ThamSo(lenh, "@ma", ma, DbType.Int32);
        await lenh.ExecuteNonQueryAsync();
    }

    public async Task CapNhatMatKhauAsync(int ma, string hash)
    {
        await using var kn = ketNoi.TaoKetNoi(SqlClientFactory.Instance);
        await kn.OpenAsync();
        await using var lenh = kn.CreateCommand();
        lenh.CommandText = "UPDATE dbo.TaiKhoan SET MatKhauHash = @hash WHERE MaTaiKhoan = @ma";
        ThamSo(lenh, "@hash", hash, DbType.String);
        ThamSo(lenh, "@ma", ma, DbType.Int32);
        await lenh.ExecuteNonQueryAsync();
    }

    async Task<TaiKhoan?> LayMotAsync(string sql, (string Ten, object Gia, DbType Kieu) thamSo)
    {
        await using var kn = ketNoi.TaoKetNoi(SqlClientFactory.Instance);
        await kn.OpenAsync();
        await using var lenh = kn.CreateCommand();
        lenh.CommandText = sql;
        ThamSo(lenh, thamSo.Ten, thamSo.Gia, thamSo.Kieu);
        await using var doc = await lenh.ExecuteReaderAsync();
        if (!await doc.ReadAsync()) return null;
        return new TaiKhoan
        {
            MaTaiKhoan = doc.GetInt32(0),
            HoTen = doc.GetString(1),
            Email = doc.IsDBNull(2) ? null : doc.GetString(2),
            SoDienThoai = doc.IsDBNull(3) ? null : doc.GetString(3),
            MatKhauHash = doc.GetString(4),
            VaiTro = doc.GetString(5),
            DiaChi = doc.IsDBNull(6) ? null : doc.GetString(6),
            TrangThai = doc.GetBoolean(7),
            NgayTao = doc.GetDateTime(8)
        };
    }

    static void ThamSo(DbCommand lenh, string ten, object? giaTri, DbType kieu)
    {
        var p = lenh.CreateParameter();
        p.ParameterName = ten;
        p.DbType = kieu;
        p.Value = giaTri ?? DBNull.Value;
        lenh.Parameters.Add(p);
    }
}
