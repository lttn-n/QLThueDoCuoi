using Microsoft.Data.SqlClient;
using System.Data;
using QLThueDoCuoi.Models;

namespace QLThueDoCuoi.Repositories;

public class TaiKhoanRepository
{
    private readonly string _connectionString; // Chỉ class TaiKhoanRepository được sử dụng biến này

    public TaiKhoanRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("QLThueDoCuoi") ?? throw new InvalidOperationException("Không tìm thấy chuỗi kết nối Database!");
    }

    //Lấy danh sách tài khoản 
    public List<TaiKhoan> GetAllTaiKhoan()
    {
        List<TaiKhoan> taiKhoans = new List<TaiKhoan>();
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            string query = @"
                SELECT MaTaiKhoan, HoTen, VaiTro
                FROM TaiKhoan";
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        TaiKhoan taiKhoan = new TaiKhoan
                        {
                            MaTaiKhoan = reader.GetInt32(reader.GetOrdinal("MaTaiKhoan")),
                            HoTen = reader.GetString(reader.GetOrdinal("HoTen")),
                            VaiTro = reader.GetString( reader.GetOrdinal("VaiTro")),
                            MatKhauHash = string.Empty
                        };
                        taiKhoans.Add(taiKhoan);
                    }
                }
            }
        }
        return taiKhoans;
    }
    // Tìm kiếm tài khoản theo mã tài khoản
    public TaiKhoan? GetTaiKhoanById(int maTaiKhoan)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            string query = @"
                SELECT MaTaiKhoan, HoTen, MatKhauHash, VaiTro
                FROM TaiKhoan
                WHERE MaTaiKhoan = @MaTaiKhoan";
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.Add("@MaTaiKhoan", SqlDbType.Int).Value= maTaiKhoan;
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new TaiKhoan
                        {
                            MaTaiKhoan = reader.GetInt32(reader.GetOrdinal("MaTaiKhoan")),
                            HoTen = reader.GetString(reader.GetOrdinal("HoTen")),
                            MatKhauHash = reader.GetString(reader.GetOrdinal("MatKhauHash")),
                            VaiTro = reader.GetString(reader.GetOrdinal("VaiTro"))
                        };
                    }
                }
            }
        }
        return null;
    }
    // Tìm kiếm tài khoản theo tên đăng nhập (email hoặc số điện thoại)
    public TaiKhoan? GetTaiKhoanByTenDangNhap(string tenDangNhap)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            string query = @"
                SELECT MaTaiKhoan, HoTen, Email, SoDienThoai,
                    MatKhauHash, VaiTro, TrangThai, NgayTao
                FROM dbo.TaiKhoan
                WHERE Email = @TenDangNhap
                OR SoDienThoai = @TenDangNhap";

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.Add("@TenDangNhap", SqlDbType.VarChar, 254).Value =
                    tenDangNhap.Trim();

                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                        return null;

                    int emailOrdinal = reader.GetOrdinal("Email");
                    int phoneOrdinal = reader.GetOrdinal("SoDienThoai");

                    return new TaiKhoan
                    {
                        MaTaiKhoan = reader.GetInt32(reader.GetOrdinal("MaTaiKhoan")),
                        HoTen = reader.GetString(reader.GetOrdinal("HoTen")),
                        Email = reader.IsDBNull(emailOrdinal) ? null : reader.GetString(emailOrdinal),
                        SoDienThoai = reader.IsDBNull(phoneOrdinal) ? null : reader.GetString(phoneOrdinal),
                        MatKhauHash = reader.GetString(reader.GetOrdinal("MatKhauHash")),
                        VaiTro = reader.GetString(reader.GetOrdinal("VaiTro")),
                        TrangThai = reader.GetBoolean(reader.GetOrdinal("TrangThai")),
                        NgayTao = reader.GetDateTime(reader.GetOrdinal("NgayTao"))
                    };
                }
            }
        }
    }

    public TaiKhoan? GetThongTinCaNhan(int maTaiKhoan)
    {
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(@"
            SELECT MaTaiKhoan, HoTen, Email, SoDienThoai, DiaChi
            FROM dbo.TaiKhoan
            WHERE MaTaiKhoan = @MaTaiKhoan;", connection);
        command.Parameters.Add("@MaTaiKhoan", SqlDbType.Int).Value = maTaiKhoan;

        connection.Open();
        using var reader = command.ExecuteReader();
        if (!reader.Read())
            return null;

        int emailOrdinal = reader.GetOrdinal("Email");
        int phoneOrdinal = reader.GetOrdinal("SoDienThoai");
        int addressOrdinal = reader.GetOrdinal("DiaChi");

        return new TaiKhoan
        {
            MaTaiKhoan = reader.GetInt32(reader.GetOrdinal("MaTaiKhoan")),
            HoTen = reader.GetString(reader.GetOrdinal("HoTen")),
            Email = reader.IsDBNull(emailOrdinal) ? null : reader.GetString(emailOrdinal),
            SoDienThoai = reader.IsDBNull(phoneOrdinal) ? null : reader.GetString(phoneOrdinal),
            DiaChi = reader.IsDBNull(addressOrdinal) ? null : reader.GetString(addressOrdinal)
        };
    }

    public bool CapNhatThongTinCaNhan(
        int maTaiKhoan,
        string hoTen,
        string? email,
        string? soDienThoai,
        string? diaChi)
    {
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(@"
            UPDATE dbo.TaiKhoan
            SET HoTen = @HoTen,
                Email = @Email,
                SoDienThoai = @SoDienThoai,
                DiaChi = @DiaChi
            WHERE MaTaiKhoan = @MaTaiKhoan;", connection);
        command.Parameters.Add("@MaTaiKhoan", SqlDbType.Int).Value = maTaiKhoan;
        command.Parameters.Add("@HoTen", SqlDbType.NVarChar, 100).Value = hoTen;
        command.Parameters.Add("@Email", SqlDbType.VarChar, 254).Value =
            (object?)email ?? DBNull.Value;
        command.Parameters.Add("@SoDienThoai", SqlDbType.VarChar, 20).Value =
            (object?)soDienThoai ?? DBNull.Value;
        command.Parameters.Add("@DiaChi", SqlDbType.NVarChar, 500).Value =
            (object?)diaChi ?? DBNull.Value;

        try
        {
            connection.Open();
            return command.ExecuteNonQuery() > 0;
        }
        catch (SqlException ex) when (ex.Number is 2601 or 2627)
        {
            return false;
        }
    }

    public bool ThemKhachHang(string hoTen, string? email, string? soDienThoai, string matKhauHash)
    {
        //kiểm tra email và số điện thoại có trống hay không, nếu trống thì ném ra ngoại lệ
        if (string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(soDienThoai))
        {
            throw new ArgumentException("Cần có email hoặc số điện thoại.");
        }
        using var connection = new SqlConnection(_connectionString);
        // Sử dụng truy vấn SQL để thêm tài khoản mới vào DB mặt định là khách hàng(Nhân viên admin thêm)
        using var command = new SqlCommand(@"
            INSERT INTO dbo.TaiKhoan
                (HoTen, Email, SoDienThoai, MatKhauHash, VaiTro)
            VALUES
                (@HoTen, @Email, @SoDienThoai, @MatKhauHash, 'KHACHHANG');",connection);
        //Lưu ý quan trọng: 
        command.Parameters.Add("@HoTen", SqlDbType.NVarChar, 100).Value = hoTen.Trim();
        command.Parameters.Add("@Email", SqlDbType.VarChar, 254).Value = string.IsNullOrWhiteSpace(email) ? DBNull.Value : email.Trim();
        command.Parameters.Add("@SoDienThoai", SqlDbType.VarChar, 20).Value = string.IsNullOrWhiteSpace(soDienThoai) ? DBNull.Value : soDienThoai.Trim();
        command.Parameters.Add("@MatKhauHash", SqlDbType.NVarChar, 500).Value = matKhauHash;
        try
        {
            connection.Open();
            command.ExecuteNonQuery();
            return true;
        }
        catch (SqlException ex) when (ex.Number is 2601 or 2627)
        {
            return false; //trả lỗi khi trùng email hoăc số điện thoại
        }
    }
}