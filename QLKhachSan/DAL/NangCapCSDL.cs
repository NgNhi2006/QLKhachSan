using Microsoft.Data.SqlClient;
using System.Text.RegularExpressions;

namespace QLKhachSan.DAL;

public sealed class SchemaMigrationException(int version, SqlException cause)
    : Exception($"Nâng cấp dữ liệu lên phiên bản {version} thất bại (SQL {cause.Number}, dòng {cause.LineNumber}): {cause.Message}", cause);

public static class SchemaMigrator
{
    public static async Task EnsureAsync(string? connectionString = null)
    {
        var rawConnStr = connectionString ?? AppSettings.Load().ConnectionString;
        var builder = new SqlConnectionStringBuilder(rawConnStr);
        var dbName = string.IsNullOrWhiteSpace(builder.InitialCatalog) ? "QLKhachSanApp" : builder.InitialCatalog;

        // 1. Đảm bảo Database QLKhachSanApp đã được tạo trên SQL Server
        var masterBuilder = new SqlConnectionStringBuilder(rawConnStr) { InitialCatalog = "master", ConnectTimeout = 10 };
        await using var masterConn = new SqlConnection(masterBuilder.ConnectionString);
        await masterConn.OpenAsync();
        var safeName = dbName.Replace("]", "]]", StringComparison.Ordinal);
        await using var createCmd = new SqlCommand($"IF DB_ID(@name) IS NULL EXEC(N'CREATE DATABASE [{safeName}]')", masterConn);
        createCmd.Parameters.AddWithValue("@name", dbName);
        await createCmd.ExecuteNonQueryAsync();

        // 2. Kết nối vào CSDL QLKhachSanApp
        builder.InitialCatalog = dbName;
        await using var connection = new SqlConnection(builder.ConnectionString);
        await connection.OpenAsync();

        var coreTables = "'Phong','KhachHang','TaiKhoanNhanVien','LuotLuuTru','HoaDon','CaTruc','GiaoDichThanhToan'";
        await using (var schemaCheck = new SqlCommand($"SELECT COUNT(*), SUM(CASE WHEN name IN ({coreTables}) THEN 1 ELSE 0 END) FROM sys.tables WHERE schema_id=SCHEMA_ID('dbo') AND name<>N'sysdiagrams'", connection))
        {
            await using var rows = await schemaCheck.ExecuteReaderAsync();
            await rows.ReadAsync();
            var total = rows.GetInt32(0);
            var core = rows.IsDBNull(1) ? 0 : rows.GetInt32(1);
            if (total == 7 && core == 7)
            {
                await rows.DisposeAsync();
                await using var permissionColumn = new SqlCommand("IF COL_LENGTH(N'dbo.TaiKhoanNhanVien',N'QuyenChucNang') IS NULL ALTER TABLE dbo.TaiKhoanNhanVien ADD QuyenChucNang nvarchar(max) NULL", connection);
                await permissionColumn.ExecuteNonQueryAsync();
                return;
            }
            if (total != 0) throw new InvalidOperationException($"CSDL hiện có {total} bảng. Hãy sao lưu và chạy quy trình chuyển đổi 7 bảng trước khi mở ứng dụng.");
        }
        using var stream = typeof(SchemaMigrator).Assembly.GetManifestResourceStream("QLKhachSan.Database.sql")
            ?? throw new InvalidOperationException("Không tìm thấy script khởi tạo CSDL.");
        using var reader = new StreamReader(stream);
        var script = await reader.ReadToEndAsync();
        var start = script.IndexOf("SET NOCOUNT ON;", StringComparison.OrdinalIgnoreCase);
        if (start < 0) throw new InvalidOperationException("Script CSDL không hợp lệ.");
        foreach (var batch in Regex.Split(script[start..], @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(batch)) continue;
            await using var command = new SqlCommand(batch, connection) { CommandTimeout = 120 };
            await command.ExecuteNonQueryAsync();
        }
    }
}
