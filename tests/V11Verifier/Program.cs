using Microsoft.Data.SqlClient;
using QLKhachSan.BLL;
using QLKhachSan.DAL;
using QLKhachSan.DTO;

if (args.Length != 1 ||
    !new SqlConnectionStringBuilder(args[0]).InitialCatalog.Contains("_Verify_", StringComparison.Ordinal))
    throw new InvalidOperationException("Pass a dedicated _Verify_ database connection string.");

var connectionString = args[0];
var repository = new HotelRepository(connectionString);
await using var connection = new SqlConnection(connectionString);
await connection.OpenAsync();

async Task<long> Scalar(string sql)
{
    await using var command = new SqlCommand(sql, connection);
    return Convert.ToInt64(await command.ExecuteScalarAsync());
}

async Task Execute(string sql)
{
    await using var command = new SqlCommand(sql, connection);
    await command.ExecuteNonQueryAsync();
}

void Check(bool ok, string message)
{
    if (!ok) throw new Exception("FAIL: " + message);
    Console.WriteLine("PASS: " + message);
}

var admin = await repository.RunAsync(false, async db =>
{
    var account = await db.AccountAsync("admin") ?? throw new Exception("Test admin missing");
    return new UserSession(account.Id, account.Username, account.Role)
    {
        SecurityVersion = account.SecurityVersion,
        GrantedFunctions = FunctionPolicy.Defaults(account.Role)
    };
});
var service = new HotelService(repository, admin);
var dashboard = await service.DashboardAsync();
Check(dashboard.Rooms.Count(r => r.Status == RoomStatus.Trong) >= 24, "enough empty rooms for the test");
var rooms = dashboard.Rooms.Where(r => r.Status == RoomStatus.Trong).Take(24).ToList();

async Task<Stay> CreateOccupied(Room room, GuestInput guest, decimal deposit)
{
    var current = (await service.DashboardAsync()).Rooms.Single(r => r.Id == room.Id);
    long id;
    if (deposit > 0)
    {
        var now = await repository.RunAsync(false, db => db.NowAsync());
        id = await service.CreateStayAsync(current, guest, true, now.AddMinutes(2), 1, true,
            "Tiền mặt", deposit, now.AddHours(1));
        var reserved = (await service.DashboardAsync()).Stays.Single(s => s.Id == id);
        await service.CheckInAsync(reserved);
    }
    else
    {
        id = await service.CreateStayAsync(current, guest, false, dashboard.ServerNow, 1, false, "Tiền mặt");
    }
    return (await service.DashboardAsync()).Stays.Single(s => s.Id == id);
}

var guest = new GuestInput("V11 Verification", "0900000011", "V11VERIFY001");
var first = await CreateOccupied(rooms[0], guest, 100_000m);
var second = await CreateOccupied(rooms[1], guest, 100_000m);
var laundry = dashboard.Menu.First(s => s.Category.StartsWith("Giặt là", StringComparison.Ordinal));
await service.AddServicesAsync(first, [new OrderInput(laundry.Id, 1)]);
first = (await service.DashboardAsync()).Stays.Single(s => s.Id == first.Id);
await service.DeliverAsync(first);
first = (await service.DashboardAsync()).Stays.Single(s => s.Id == first.Id);
var quotes2 = await service.GroupQuoteAsync([first, second]);
Check(quotes2.Count == 2 && quotes2.Sum(q => q.Stay.Deposit) == 200_000m,
    "two room quote retains both deposits");

var noRights = new UserSession(admin.Id, admin.Username, admin.Role)
{
    SecurityVersion = admin.SecurityVersion,
    GrantedFunctions = []
};
try
{
    await new HotelService(repository, noRights).GroupCheckoutAsync(quotes2, "Tiền mặt");
    throw new Exception("FAIL: missing permission accepted outside menu");
}
catch (BusinessException)
{
    Console.WriteLine("PASS: business layer denies group checkout without function permission");
}

var beforeInvoices = await Scalar("SELECT COUNT_BIG(*) FROM dbo.HoaDon");
var beforePayments = await Scalar("SELECT COUNT_BIG(*) FROM dbo.GiaoDichThanhToan");
var beforeGroups = await Scalar("SELECT COUNT_BIG(*) FROM dbo.NhomHoaDon");
var trigger = $"CREATE OR ALTER TRIGGER dbo.TR_V11Verifier_ForceFail ON dbo.HoaDon AFTER INSERT AS BEGIN IF EXISTS(SELECT 1 FROM inserted WHERE MaLuotLuuTru={second.Id}) THROW 51999,N'Injected second invoice failure',1; END";
await Execute(trigger);
try
{
    try
    {
        await service.GroupCheckoutAsync(quotes2, "Tiền mặt");
        throw new Exception("FAIL: injected failure did not occur");
    }
    catch (SqlException ex) when (ex.Number == 51999)
    {
        Console.WriteLine("PASS: injected failure reached the second invoice");
    }
}
finally
{
    await Execute("DROP TRIGGER IF EXISTS dbo.TR_V11Verifier_ForceFail");
}
Check(await Scalar("SELECT COUNT_BIG(*) FROM dbo.HoaDon") == beforeInvoices &&
      await Scalar("SELECT COUNT_BIG(*) FROM dbo.GiaoDichThanhToan") == beforePayments &&
      await Scalar("SELECT COUNT_BIG(*) FROM dbo.NhomHoaDon") == beforeGroups &&
      await Scalar($"SELECT COUNT_BIG(*) FROM dbo.LuotLuuTru WHERE Ma IN ({first.Id},{second.Id}) AND TrangThai='Occupied'") == 2,
    "mid transaction failure rolls back invoices, payment, group and stays");

var result2 = await service.GroupCheckoutAsync(quotes2, "Tiền mặt");
var net2 = result2.Collected - result2.Refunded;
Check(result2.InvoiceIds.Count == 2 &&
      await Scalar($"SELECT COUNT_BIG(*) FROM dbo.PhanChiaHoaDon WHERE MaNhomHoaDon={result2.GroupId}") == 2 &&
      await Scalar("SELECT COUNT_BIG(*) FROM dbo.HoaDon") == beforeInvoices + 2 &&
      await Scalar("SELECT COUNT_BIG(*) FROM dbo.GiaoDichThanhToan") == beforePayments + (net2 == 0 ? 0 : 1),
    "two room checkout creates two invoices and one net payment");
Check(await Scalar($"SELECT COALESCE(SUM(TienCoc),0) FROM dbo.HoaDon WHERE Ma IN ({string.Join(',', result2.InvoiceIds)})") == 200_000,
    "group invoices reconcile the two deposits");
Check(await Scalar($"SELECT COALESCE(SUM(SoTien),0) FROM dbo.GiaoDichThanhToan WHERE LoaiGiaoDich='Deposit' AND MaLuotLuuTru IN ({first.Id},{second.Id})") == 200_000 &&
      result2.Deposits + net2 == result2.Total,
    "deposit receipts plus net collection equal the group invoice total");

var many = new List<Stay>();
for (var i = 2; i < 22; i++) many.Add(await CreateOccupied(rooms[i], guest, 0));
var quotes20 = await service.GroupQuoteAsync(many);
var paymentBefore20 = await Scalar("SELECT COUNT_BIG(*) FROM dbo.GiaoDichThanhToan");
var result20 = await service.GroupCheckoutAsync(quotes20, "Tiền mặt");
Check(result20.InvoiceIds.Count == 20 &&
      await Scalar($"SELECT COUNT_BIG(*) FROM dbo.PhanChiaHoaDon WHERE MaNhomHoaDon={result20.GroupId}") == 20 &&
      await Scalar("SELECT COUNT_BIG(*) FROM dbo.GiaoDichThanhToan") == paymentBefore20 + 1,
    "twenty room checkout creates twenty invoices and one net payment");

var refundStays = new[]
{
    await CreateOccupied(rooms[22], guest, 1_000_000m),
    await CreateOccupied(rooms[23], guest, 1_000_000m)
};
var refundQuotes = await service.GroupQuoteAsync(refundStays);
var refundBefore = await Scalar("SELECT COUNT_BIG(*) FROM dbo.GiaoDichThanhToan WHERE LoaiGiaoDich='Refund'");
var refundResult = await service.GroupCheckoutAsync(refundQuotes, "Tiền mặt");
var refundNet = refundResult.Collected - refundResult.Refunded;
Check(refundNet < 0 && refundResult.InvoiceIds.Count == 2 &&
      await Scalar("SELECT COUNT_BIG(*) FROM dbo.GiaoDichThanhToan WHERE LoaiGiaoDich='Refund'") == refundBefore + 1 &&
      await Scalar($"SELECT COALESCE(SUM(SoTien),0) FROM dbo.GiaoDichThanhToan WHERE LoaiGiaoDich='Refund' AND GhiChu LIKE N'%phiếu tổng #{refundResult.GroupId};%'") == -refundNet &&
      refundResult.Deposits + refundNet == refundResult.Total,
    "two room checkout creates one net refund with reconciled deposits");

Console.WriteLine("PASS: V11 integration verification complete");
