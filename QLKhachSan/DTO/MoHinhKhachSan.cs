namespace QLKhachSan.DTO;

public enum RoomStatus { Trong, DaDat, DangO, DangDon, BaoTri }
public enum StayStatus { Reserved, Occupied, Paid, Cancelled }
public sealed record UserSession(int Id, string Username, string Role)
{
    public long SecurityVersion { get; set; } = 1;
    public HashSet<string>? GrantedFunctions { get; set; }
    public string DisplayName { get; set; } = Username;
    public byte[]? AvatarPng { get; set; }
}
public sealed record Room(int Id, string Number, string Type, decimal Rate, decimal Deposit, RoomStatus Status, long Version,string Code="")
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
    decimal Price, DateTime Ordered, DateTime? Delivered, int DeliveredQuantity = 0, DateTime? Cancelled = null,
    decimal? BilledAmount = null)
{
    public decimal Total => Cancelled is null ? BilledAmount ?? Quantity * Price : 0;
}
public sealed record OrderInput(int ServiceId, int Quantity);
public sealed record OrderAssignment(Stay Stay,int ServiceId,int Quantity);
public sealed record TodayScheduleItem(long StayId,string Room,string Guest,string Phone,string Stage,DateTime Time);
public sealed record Invoice(long Id, long StayId, string Room, string Guest, DateTime Issued,
    decimal RoomCharge, decimal ServiceCharge, decimal Deposit, decimal Collected, decimal Refunded, string Method)
{
    public decimal Total => RoomCharge + ServiceCharge;
}
public sealed record CustomerSummary(int Id, string Name, string Phone, string Identity, int Visits, decimal Total,string Code="");
public sealed record RevenueItem(string Category, decimal Total);
public sealed record RevenueEvent(DateTime At, string Category, string Item, decimal Quantity, decimal Amount);
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
    public decimal CashFlow => Method == "Công nợ OTA" || Kind == "Forfeit" ? 0 : Kind == "Refund" ? -Amount : Amount;
}
public sealed record CustomerDepositHistory(List<Stay> Stays,List<PaymentEntry> Payments);
public sealed record DepositReceipt(long PaymentId,long StayId,string Guest,string Phone,string Room,DateTime PaidAt,
    decimal Amount,decimal TotalDeposited,decimal EstimatedRoomCharge,decimal? FinalInvoiceTotal,string Method,string Note)
{
    public decimal Outstanding => Math.Max(0,(FinalInvoiceTotal??EstimatedRoomCharge)-TotalDeposited);
    public bool IsEstimate => FinalInvoiceTotal is null;
}
public sealed record PeriodReport(List<Invoice> Invoices, List<RevenueItem> Revenue, List<PaymentEntry> Payments);
public sealed record UserInfo(int Id, string Username, string Role, bool Active, DateTime? LockedUntil,
    long Version, string DisplayName, byte[]? AvatarPng);
public sealed record ServiceCatalogItem(int Id, string Category, string Name, decimal Price, string Unit, bool Active, long Version);
public sealed record AuditEntry(long Id, DateTime Created, string Username, string Action, string Detail);
public sealed record BillDraftLine(string Name,int Quantity,decimal UnitPrice,string Category,long? OrderId=null,decimal? AmountOverride=null,int? ServiceId=null)
{
    public decimal Total => AmountOverride??Quantity*UnitPrice;
}
public sealed record BillQuote(Stay Stay, Room Room, DateTime At, decimal RoomCharge, decimal Services,
    List<ServiceLine> Lines,List<BillDraftLine>? DraftLines=null,decimal? BaseRoomCharge=null,decimal? BaseServices=null,string? EditReason=null,
    string? DiscountCode=null,decimal DiscountAmount=0)
{
    public decimal Total => RoomCharge + Services - DiscountAmount;
    public decimal ToCollect => Math.Max(0, Total - Stay.Deposit);
    public decimal ToRefund => Math.Max(0, Stay.Deposit - Total);
}
public sealed record DiscountCode(string Code,string Kind,decimal Value,DateTime StartsAt,DateTime EndsAt,
    int? UsageLimit,int UsedCount,bool Active)
{
    public string TypeLabel => Kind=="Percent"?"Theo %":"Theo số tiền";
    public string Description => Kind=="Percent"?$"{Value:0}%":$"{Value:N0} đ";
}
public sealed record InvoiceDiscount(string Code,decimal Amount,decimal GrossRoom);
public sealed record GroupCheckoutResult(long GroupId,List<long> InvoiceIds,string Guest,decimal Total,
    decimal Deposits,decimal Collected,decimal Refunded);
public sealed class BusinessException(string message) : Exception(message);
