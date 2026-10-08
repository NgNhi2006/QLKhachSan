using Microsoft.Data.SqlClient;
using System.Text.RegularExpressions;

namespace QLKhachSan.DAL;

public sealed class SchemaMigrationException(int version, SqlException cause)
    : Exception($"Nâng cấp dữ liệu lên phiên bản {version} thất bại (SQL {cause.Number}, dòng {cause.LineNumber}): {cause.Message}", cause);

public static class SchemaMigrator
{
    public static async Task EnsureAsync(string? connectionString = null)
    {
        await using var connection = new SqlConnection(connectionString ?? AppSettings.Load().ConnectionString);
        await connection.OpenAsync();
        // Determine the known table/column combination before constructing the version query.
        await using var shape = new SqlCommand("""
            SELECT CASE
                WHEN OBJECT_ID(N'dbo.PhienBanCSDL', N'U') IS NOT NULL
                     AND COL_LENGTH(N'dbo.PhienBanCSDL', N'PhienBan') IS NOT NULL THEN 2
                WHEN OBJECT_ID(N'dbo.PhienBanCSDL', N'U') IS NOT NULL
                     AND COL_LENGTH(N'dbo.PhienBanCSDL', N'Version') IS NOT NULL THEN 1
                WHEN OBJECT_ID(N'dbo.SchemaVersion', N'U') IS NOT NULL THEN 0
                ELSE -1 END;
            """, connection);
        var schemaShape = Convert.ToInt32(await shape.ExecuteScalarAsync());
        if (schemaShape < 0)
            throw new InvalidOperationException("Schema không tương thích. Hãy chạy Database.sql hoặc dùng đúng phiên bản ứng dụng.");
        var versionSql = schemaShape switch
        {
            2 => "SELECT MAX(PhienBan) FROM dbo.PhienBanCSDL",
            1 => "SELECT MAX(Version) FROM dbo.PhienBanCSDL",
            _ => "SELECT MAX(Version) FROM dbo.SchemaVersion"
        };
        await using var check = new SqlCommand(versionSql, connection);
        var version = Convert.ToInt32(await check.ExecuteScalarAsync());
        if (version is <1 or >16) throw new InvalidOperationException("Schema không tương thích. Hãy chạy Database.sql hoặc dùng đúng phiên bản ứng dụng.");
        if (version == 16) return;

        using var stream = typeof(SchemaMigrator).Assembly.GetManifestResourceStream("QLKhachSan.Database.sql")
            ?? throw new InvalidOperationException("Thiếu Database.sql trong ứng dụng.");
        using var reader = new StreamReader(stream);
        var source = await reader.ReadToEndAsync();
        await using var legacyCheck = new SqlCommand(
            "SELECT CASE WHEN OBJECT_ID(N'dbo.SchemaVersion', N'U') IS NULL THEN 0 ELSE 1 END", connection);
        if (Convert.ToInt32(await legacyCheck.ExecuteScalarAsync()) == 1)
        {
            var bridge = Regex.Match(source,
                @"(?ms)^-- BEGIN LEGACY TABLE RENAME[ \t]*\r?\n(?<sql>.*?)^-- END LEGACY TABLE RENAME[ \t]*\r?$");
            if (!bridge.Success)
                throw new InvalidOperationException("Database.sql thiếu bước đổi tên bảng cũ.");
            await using var rename = new SqlCommand(bridge.Groups["sql"].Value.Trim(), connection)
                { CommandTimeout = 60 };
            try { await rename.ExecuteNonQueryAsync(); }
            catch (SqlException ex) { throw new SchemaMigrationException(9, ex); }
        }
        if (schemaShape != 2)
        {
            var bridge = Regex.Match(source,
                @"(?ms)^-- BEGIN LEGACY COLUMN RENAME[ \t]*\r?\n(?<sql>.*?)^-- END LEGACY COLUMN RENAME[ \t]*\r?$");
            if (!bridge.Success)
                throw new InvalidOperationException("Database.sql thiếu bước đổi tên cột cũ.");
            await using var rename = new SqlCommand(bridge.Groups["sql"].Value.Trim(), connection)
                { CommandTimeout = 120 };
            try { await rename.ExecuteNonQueryAsync(); }
            catch (SqlException ex) { throw new SchemaMigrationException(10, ex); }
        }
        var sections = Regex.Matches(source,
            @"(?ms)^-- BEGIN MIGRATION V(?<version>[2-9]|10|11|12|13|14|15|16)[ \t]*\r?\n(?<sql>.*?)^-- END MIGRATION V\k<version>[ \t]*\r?$");
        var migrations = sections.Cast<Match>().ToDictionary(
            match => int.Parse(match.Groups["version"].Value),
            match => match.Groups["sql"].Value.Trim());
        if (sections.Count != 15 || Enumerable.Range(2,15).Any(next => !migrations.ContainsKey(next)))
            throw new InvalidOperationException("Database.sql thiếu một bước nâng cấp từ V2 đến V16.");

        foreach(var next in Enumerable.Range(version+1,16-version))
        {
            await using var command = new SqlCommand(migrations[next], connection) { CommandTimeout = 60 };
            try { await command.ExecuteNonQueryAsync(); }
            catch (SqlException ex) { throw new SchemaMigrationException(next, ex); }
        }
    }
}
