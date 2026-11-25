using QLKhachSan.BLL;
using QLKhachSan.DAL;
using QLKhachSan.DTO;

var database = args.FirstOrDefault(x => x.StartsWith("--database=", StringComparison.Ordinal))?.Split('=',2)[1]
    ?? "QLKhachSanApp_CompactTest";
var connection = $"Server=.\\SQLEXPRESS;Database={database};Integrated Security=true;Encrypt=true;TrustServerCertificate=true";
await SchemaMigrator.EnsureAsync(connection);
var user = new UserSession(3, "admin", "Admin")
{
    SecurityVersion = 2,
    GrantedFunctions = FunctionPolicy.Defaults("Admin")
};
var service = new HotelService(new HotelRepository(connection), user);
var dashboard = await service.DashboardAsync();
var menus = await service.MenuConfigurationAsync();
var customers = await service.CustomersAsync("");
var invoices = await service.InvoicesAsync(dashboard.ServerNow.Date);
var events = await service.RevenueEventsAsync(dashboard.ServerNow.Date.AddYears(-1), dashboard.ServerNow.Date.AddDays(1));
var shifts = await service.CashShiftsAsync(dashboard.ServerNow.Date.AddYears(-1), dashboard.ServerNow.Date);
var stock = await service.ServiceStockAvailabilityAsync();
Console.WriteLine($"Rooms={dashboard.Rooms.Count}; Stays={dashboard.Stays.Count}; Menus={menus.Menus.Count}; Customers={customers.Count}; Invoices={invoices.Count}; RevenueEvents={events.Count}; Shifts={shifts.Count}; Stock={stock.Count}");
if (args.Contains("--checkout"))
{
    var stay = dashboard.Stays.First(s => s.Status == StayStatus.Occupied &&
        !dashboard.Pending.Any(o => o.StayId == s.Id));
    var bill = await service.QuoteAsync(stay);
    var invoice = await service.CheckoutAsync(bill, "Tiền mặt");
    Console.WriteLine($"Checkout stay={stay.Id}; invoice={invoice}");
}
if (args.Contains("--roundtrip"))
{
    var room = dashboard.Rooms.First(r => r.Status == RoomStatus.Trong &&
        !dashboard.Stays.Any(s => s.RoomId == r.Id && s.Arrival < dashboard.ServerNow.AddDays(1) && s.Departure > dashboard.ServerNow));
    var id = await service.CreateStayAsync(room, new GuestInput("Khach Kiem Thu", "0901234567", "TSTCORE20"),
        false, dashboard.ServerNow, 1, false, "Tiền mặt");
    var current = (await service.DashboardAsync()).Stays.Single(s => s.Id == id);
    var item = stock.First(x => x.Available is null or >= 1);
    await service.AddServicesAsync(current, [new OrderInput(item.ServiceId, 1)]);
    current = (await service.DashboardAsync()).Stays.Single(s => s.Id == id);
    await service.DeliverAsync(current);
    current = (await service.DashboardAsync()).Stays.Single(s => s.Id == id);
    var bill = await service.QuoteAsync(current);
    var invoice = await service.CheckoutAsync(bill, "Tiền mặt");
    Console.WriteLine($"Roundtrip stay={id}; service={item.ServiceId}; invoice={invoice}");
}
