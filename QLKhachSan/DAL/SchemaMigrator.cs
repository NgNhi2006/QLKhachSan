using Microsoft.Data.SqlClient;

namespace QLKhachSan.DAL;

public static class SchemaMigrator
{
    public static async Task EnsureAsync(string? connectionString = null)
    {
        await using var connection = new SqlConnection(connectionString ?? AppSettings.Load().ConnectionString);
        await connection.OpenAsync();
        // Up-to-date installations do not require schema modification permission.
        await using var check = new SqlCommand("IF OBJECT_ID(N'dbo.SchemaVersion') IS NULL SELECT 0 ELSE SELECT MAX(Version) FROM dbo.SchemaVersion", connection);
        var version = Convert.ToInt32(await check.ExecuteScalarAsync());
        if (version is <1 or >5) throw new InvalidOperationException("Schema không tương thích. Hãy chạy Setup.sql hoặc dùng đúng phiên bản ứng dụng.");
        foreach(var next in Enumerable.Range(version+1,5-version))
        {
            using var stream = typeof(SchemaMigrator).Assembly.GetManifestResourceStream($"QLKhachSan.MigrateV{next}.sql")
                ?? throw new InvalidOperationException("Thiếu script nâng cấp.");
            using var reader = new StreamReader(stream);
            await using var command = new SqlCommand(await reader.ReadToEndAsync(), connection) { CommandTimeout = 60 };
            await command.ExecuteNonQueryAsync();
        }
    }
}
