using System.Text.Json;
using Microsoft.Data.SqlClient;

namespace QLKhachSan.DAL;

public sealed class AppSettings
{
    public string ConnectionString { get; set; } = "";
    public string BankCode { get; set; } = "";
    public string BankAccount { get; set; } = "";
    public string BankAccountName { get; set; } = "";
    public string HotelName { get; set; } = "KHÁCH SẠN";
    public string HotelAddress { get; set; } = "";
    public string HotelPhone { get; set; } = "";
    public string HotelEmail { get; set; } = "";
    public string HotelWebsite { get; set; } = "";
    public string InternalIssuerCode { get; set; } = "KS-2026-001";
    public static AppSettings Load()
    {
        var settings = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "appsettings.json")))
            ?? throw new InvalidOperationException("Không đọc được appsettings.json.");
        settings.ConnectionString = Environment.GetEnvironmentVariable("QLKHACHSAN_CONNECTION_STRING") ?? settings.ConnectionString;
        if (string.IsNullOrWhiteSpace(settings.ConnectionString)) throw new InvalidOperationException("Chưa cấu hình kết nối SQL Server.");
        return settings;
    }
}
public static class DatabaseHelper
{
    public static SqlConnection GetConnection() => new(AppSettings.Load().ConnectionString);
}
