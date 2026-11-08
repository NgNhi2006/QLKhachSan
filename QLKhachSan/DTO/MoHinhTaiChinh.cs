namespace QLKhachSan.DTO;

public sealed record CashShift(long Id, string Cashier, DateTime OpenedAt, DateTime? ClosedAt,
    decimal OpeningCash, decimal? CountedCash, decimal? ExpectedCash, string? Explanation, string Status,
    decimal CashIn, decimal CashOut, decimal Pos, decimal Bank, decimal Ota)
{
    public decimal Discrepancy => (CountedCash ?? 0) - (ExpectedCash ?? 0);
}
public sealed record Voucher(long Id, DateTime PostedAt, string Type, string Channel, string Category,
    decimal Amount, string Counterparty, string? Reference, string Note, string Username, bool Reversed);
public sealed record Debt(long Id, string Type, string Counterparty, long? InvoiceId, DateTime IssuedAt,
    DateTime DueAt, decimal Amount, decimal Paid, string Note)
{
    public decimal Outstanding => Amount - Paid;
    public int AgeDays(DateTime at) => Math.Max(0,(at.Date-DueAt.Date).Days);
    public string AgeBucket(DateTime at) => at.Date<DueAt.Date ? "Trong hạn" : AgeDays(at) switch
    { <30=>"Quá hạn <30", <60=>"30–60", <90=>"60–90", _=>">90" };
}
public sealed record StockItem(int Id, int? ServiceId, string Name, string Unit, decimal Quantity,
    decimal AverageCost, decimal ReorderLevel, bool Active);
public sealed record StockMovement(long Id, DateTime HappenedAt, string Item, string Kind,
    decimal Quantity, decimal UnitCost, string Reason);
public sealed record MinibarReconciliation(string Item, decimal Billed, decimal Housekeeping,
    decimal BookSold, decimal BookBalance, decimal Variance);
public sealed record InvoiceControl(long Id,bool Voided,decimal VatRate,decimal ServiceRate,
    string? EInvoiceNumber,decimal Adjustments);
public sealed record BankStatementLine(long Id,DateTime OccurredAt,string Channel,string Reference,
    decimal Amount,long? PaymentId,long? VoucherId)
{
    public string Status => PaymentId is not null || VoucherId is not null ? "Đã khớp" : "Chưa khớp";
}
public sealed record BillShareView(long GroupId,string GroupName,long InvoiceId,string Payer,
    decimal Amount,DateTime CreatedAt);
public sealed record FinanceSummary(decimal RoomRevenue, decimal MinibarRevenue, decimal OtherRevenue,
    decimal Discounts, decimal CostOfGoods, decimal OperatingExpenses, int RoomNightsSold,
    int RoomNightsAvailable)
{
    public decimal NetRevenue => RoomRevenue + MinibarRevenue + OtherRevenue - Discounts;
    public decimal GrossProfit => NetRevenue - CostOfGoods - OperatingExpenses;
    public decimal Adr => RoomNightsSold==0 ? 0 : RoomRevenue / RoomNightsSold;
    public decimal RevPar => RoomNightsAvailable==0 ? 0 : RoomRevenue / RoomNightsAvailable;
    public decimal Occupancy => RoomNightsAvailable==0 ? 0 : 100m*RoomNightsSold/RoomNightsAvailable;
}
