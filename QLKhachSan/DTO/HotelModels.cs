namespace QLKhachSan.DTO;

public enum RoomStatus { Trong, DaDat, DangO, DangDon, BaoTri }
public enum StayStatus { Reserved, Occupied, Paid, Cancelled }
public sealed record UserSession(int Id, string Username, string Role)
{
    public long SecurityVersion { get; set; } = 1;
}
public sealed record Room(int Id, string Number, string Type, decimal Rate, decimal Deposit, RoomStatus Status, long Version)
{
    public override string ToString() => $"P.{Number} — {Type} — {Rate:N0} đ/ngày";
}
public sealed record GuestInput(string Name, string Phone, string Identity);
public sealed record Stay(long Id, int RoomId, int CustomerId, string Guest, string Phone, string Identity,
    StayStatus Status, DateTime Created, DateTime Arrival, DateTime Departure, DateTime? CheckIn,
    DateTime? HoldUntil, decimal Deposit, long Version);
public sealed record Segment(long Id, long StayId, int RoomId, DateTime Start, DateTime? End, decimal Rate);
public sealed record ServiceItem(int Id, string Category, string Name, decimal Price, string Unit)
{
    public override string ToString() => $"{Name} — {Price:N0} đ/{Unit}";
}
public sealed record ServiceLine(long Id, long StayId, string Category, string Name, int Quantity,
    decimal Price, DateTime Ordered, DateTime? Delivered, int DeliveredQuantity = 0, DateTime? Cancelled = null)
{
    public decimal Total => Cancelled is null ? Quantity * Price : 0;
}
public sealed record OrderInput(int ServiceId, int Quantity);
public sealed record Invoice(long Id, long StayId, string Room, string Guest, DateTime Issued,
    decimal RoomCharge, decimal ServiceCharge, decimal Deposit, decimal Collected, decimal Refunded, string Method)
{
    public decimal Total => RoomCharge + ServiceCharge;
}
public sealed record CustomerSummary(int Id, string Name, string Phone, string Identity, int Visits, decimal Total);
public sealed record RevenueItem(string Category, decimal Total);
public sealed record RevenueDay(DateTime Day,decimal Rooms,decimal Services,decimal Forfeits)
{
    public decimal Total => Rooms+Services+Forfeits;
}
public sealed record DailyReport(List<Invoice> Invoices, List<RevenueItem> Revenue);
public sealed record DashboardData(List<Room> Rooms, List<Stay> Stays, List<ServiceLine> Pending,
    List<ServiceItem> Menu, List<RevenueItem> Revenue, DateTime ServerNow = default,List<RevenueDay>? Trend=null);
public sealed record PaymentEntry(long Id, long StayId, string Guest, string Kind, decimal Amount, DateTime Created, string Method, string Note, string Username)
{
    // Forfeit recognizes a previously received deposit; it is not another cash receipt.
    public decimal CashFlow => Kind == "Refund" ? -Amount : Kind == "Forfeit" ? 0 : Amount;
}
public sealed record PeriodReport(List<Invoice> Invoices, List<RevenueItem> Revenue, List<PaymentEntry> Payments);
public sealed record UserInfo(int Id, string Username, string Role, bool Active, DateTime? LockedUntil, long Version);
public sealed record ServiceCatalogItem(int Id, string Category, string Name, decimal Price, string Unit, bool Active, long Version);
public sealed record AuditEntry(long Id, DateTime Created, string Username, string Action, string Detail);
public sealed record BillQuote(Stay Stay, Room Room, DateTime At, decimal RoomCharge, decimal Services,
    List<ServiceLine> Lines)
{
    public decimal Total => RoomCharge + Services;
    public decimal ToCollect => Math.Max(0, Total - Stay.Deposit);
    public decimal ToRefund => Math.Max(0, Stay.Deposit - Total);
}
public sealed class BusinessException(string message) : Exception(message);
