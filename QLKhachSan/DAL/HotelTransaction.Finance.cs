using Microsoft.Data.SqlClient;
using QLKhachSan.DTO;

namespace QLKhachSan.DAL;

public sealed partial class HotelTransaction
{
    public async Task<bool> PeriodLockedAsync(DateTime at) => (await Query(
        "SELECT TOP(1) 1 FROM dbo.AccountingPeriods WHERE PeriodStart=DATEFROMPARTS(YEAR(@p0),MONTH(@p0),1)",
        r=>true,at)).Count>0;
    public async Task RequireOpenPeriodAsync(DateTime at)
    {
        if(await PeriodLockedAsync(at)) throw new BusinessException("Kỳ kế toán đã khóa.");
    }
    public Task<List<CashShift>> ShiftsAsync(DateTime from, DateTime until,int cashierId=0) => Query(@"
SELECT s.Id,u.Username,s.OpenedAt,s.ClosedAt,s.OpeningCash,s.CountedCash,s.ExpectedCash,s.Explanation,s.Status,
 COALESCE(SUM(CASE WHEN p.Method=N'Tiền mặt' AND p.Kind IN('Deposit','Checkout') THEN p.Amount ELSE 0 END),0)
 + COALESCE((SELECT SUM(v.Amount) FROM dbo.FinanceVouchers v WHERE v.ShiftId=s.Id AND v.Channel='Cash' AND v.VoucherType='Receipt' AND v.ReversedAt IS NULL),0),
 COALESCE(SUM(CASE WHEN p.Method=N'Tiền mặt' AND p.Kind='Refund' THEN p.Amount ELSE 0 END),0)
 + COALESCE((SELECT SUM(v.Amount) FROM dbo.FinanceVouchers v WHERE v.ShiftId=s.Id AND v.Channel='Cash' AND v.VoucherType='Payment' AND v.ReversedAt IS NULL),0),
 COALESCE(SUM(CASE WHEN p.Method=N'Thẻ POS' THEN CASE WHEN p.Kind='Refund' THEN -p.Amount WHEN p.Kind='Forfeit' THEN 0 ELSE p.Amount END ELSE 0 END),0),
 COALESCE(SUM(CASE WHEN p.Method=N'Chuyển khoản' THEN CASE WHEN p.Kind='Refund' THEN -p.Amount WHEN p.Kind='Forfeit' THEN 0 ELSE p.Amount END ELSE 0 END),0),
 COALESCE(SUM(CASE WHEN p.Method=N'Công nợ OTA' THEN CASE WHEN p.Kind='Refund' THEN -p.Amount WHEN p.Kind='Forfeit' THEN 0 ELSE p.Amount END ELSE 0 END),0)
FROM dbo.CashShifts s JOIN dbo.Users u ON u.Id=s.CashierId
LEFT JOIN dbo.Payments p ON p.ShiftId=s.Id
WHERE s.OpenedAt>=@p0 AND s.OpenedAt<@p1 AND (@p2=0 OR s.CashierId=@p2)
GROUP BY s.Id,u.Username,s.OpenedAt,s.ClosedAt,s.OpeningCash,s.CountedCash,s.ExpectedCash,s.Explanation,s.Status
ORDER BY s.Id DESC",r=>new CashShift(r.GetInt64(0),r.GetString(1),r.GetDateTime(2),Date(r,3),r.GetDecimal(4),r.IsDBNull(5)?null:r.GetDecimal(5),r.IsDBNull(6)?null:r.GetDecimal(6),r.IsDBNull(7)?null:r.GetString(7),r.GetString(8),r.GetDecimal(9),r.GetDecimal(10),r.GetDecimal(11),r.GetDecimal(12),r.GetDecimal(13)),from,until,cashierId);
    public async Task<(int CashierId,string Status)> ShiftIdentityAsync(long id) => (await Query(
        "SELECT CashierId,Status FROM dbo.CashShifts WHERE Id=@p0",r=>(r.GetInt32(0),r.GetString(1)),id)).SingleOrDefault();
    public Task<long> OpenShiftAsync(int cashier,decimal opening) => Scalar(
        "INSERT dbo.CashShifts(CashierId,OpeningCash) OUTPUT INSERTED.Id VALUES(@p0,@p1)",cashier,opening);
    public async Task<long> RequireOpenShiftAsync(int cashier) => (await Query(
        "SELECT Id FROM dbo.CashShifts WHERE CashierId=@p0 AND Status='Open'",r=>r.GetInt64(0),cashier)).SingleOrDefault() is var id && id>0
        ? id : throw new BusinessException("Hãy mở ca và nhập tiền mặt đầu ca trước khi thu/chi.");
    public async Task<decimal> ExpectedCashAsync(long shiftId)
    {
        var rows=await Query(@"
SELECT s.OpeningCash
 + COALESCE((SELECT SUM(CASE WHEN Kind='Refund' THEN -Amount WHEN Kind='Forfeit' THEN 0 ELSE Amount END) FROM dbo.Payments WHERE ShiftId=s.Id AND Method=N'Tiền mặt'),0)
 + COALESCE((SELECT SUM(CASE WHEN VoucherType='Receipt' THEN Amount ELSE -Amount END) FROM dbo.FinanceVouchers WHERE ShiftId=s.Id AND Channel='Cash' AND ReversedAt IS NULL),0)
FROM dbo.CashShifts s WHERE s.Id=@p0 AND s.Status='Open'",r=>r.GetDecimal(0),shiftId);
        return rows.SingleOrDefault();
    }
    public Task<int> SubmitShiftAsync(long id,decimal counted,decimal expected,string explanation) => Execute(
        "UPDATE dbo.CashShifts SET Status='Submitted',ClosedAt=SYSDATETIME(),CountedCash=@p1,ExpectedCash=@p2,Explanation=@p3 WHERE Id=@p0 AND Status='Open'",id,counted,expected,explanation);
    public Task<int> LockShiftAsync(long id,int approver) => Execute(
        "UPDATE dbo.CashShifts SET Status='Locked',LockedAt=SYSDATETIME(),LockedBy=@p1 WHERE Id=@p0 AND Status='Submitted'",id,approver);
    public Task<long> PendingOrdersInShiftAsync(long id) => Scalar(
        "SELECT COUNT_BIG(*) FROM dbo.ServiceOrders WHERE ShiftId=@p0 AND Cancelled IS NULL AND DeliveredQuantity<Quantity",id);
    public Task<int> LockPeriodAsync(DateTime month,int approver,string note) => Execute(
        "INSERT dbo.AccountingPeriods(PeriodStart,LockedBy,Note) VALUES(@p0,@p1,@p2)",new DateTime(month.Year,month.Month,1),approver,note);
    public Task<long> UnlockedShiftsInMonthAsync(DateTime month) => Scalar(@"
SELECT COUNT_BIG(*) FROM dbo.CashShifts WHERE OpenedAt>=@p0 AND OpenedAt<@p1 AND Status<>'Locked'",
        new DateTime(month.Year,month.Month,1),new DateTime(month.Year,month.Month,1).AddMonths(1));
    public Task<long> AddVoucherAsync(string type,string channel,string category,decimal amount,string party,string? reference,string note,int actor,long? shift) => Scalar(@"
INSERT dbo.FinanceVouchers(VoucherType,Channel,Category,Amount,Counterparty,Reference,Note,CreatedBy,ShiftId)
OUTPUT INSERTED.Id VALUES(@p0,@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8)",type,channel,category,amount,party,reference,note,actor,shift);
    public async Task<(DateTime AsOf,decimal Amount)?> OpeningBalanceAsync(string channel)
    {
        var rows=await Query("SELECT AsOf,Amount FROM dbo.FinanceOpeningBalances WHERE Channel=@p0",
            r=>(r.GetDateTime(0),r.GetDecimal(1)),channel);
        return rows.Count==0?null:rows[0];
    }
    public Task<int> SetOpeningBalanceAsync(string channel,DateTime asOf,decimal amount,string note,int actor) => Execute(
        "INSERT dbo.FinanceOpeningBalances(Channel,AsOf,Amount,Note,SetBy) VALUES(@p0,@p1,@p2,@p3,@p4)",channel,asOf,amount,note,actor);
    public Task<List<Voucher>> VouchersAsync(DateTime from,DateTime until) => Query(@"
SELECT v.Id,v.PostedAt,v.VoucherType,v.Channel,v.Category,v.Amount,v.Counterparty,v.Reference,v.Note,u.Username,v.ReversedAt
FROM dbo.FinanceVouchers v JOIN dbo.Users u ON u.Id=v.CreatedBy
WHERE v.PostedAt>=@p0 AND v.PostedAt<@p1 ORDER BY v.Id DESC",
        r=>new Voucher(r.GetInt64(0),r.GetDateTime(1),r.GetString(2),r.GetString(3),r.GetString(4),r.GetDecimal(5),r.GetString(6),r.IsDBNull(7)?null:r.GetString(7),r.GetString(8),r.GetString(9),!r.IsDBNull(10)),from,until);
    public Task<List<BankStatementLine>> BankLinesAsync(DateTime from,DateTime until) => Query(@"
SELECT Id,OccurredAt,Channel,Reference,Amount,MatchedPaymentId,MatchedVoucherId
FROM dbo.BankStatementLines WHERE OccurredAt>=@p0 AND OccurredAt<@p1 ORDER BY Id DESC",
        r=>new BankStatementLine(r.GetInt64(0),r.GetDateTime(1),r.GetString(2),r.GetString(3),r.GetDecimal(4),r.IsDBNull(5)?null:r.GetInt64(5),r.IsDBNull(6)?null:r.GetInt64(6)),from,until);
    public Task<List<BankStatementLine>> UnmatchedBankLinesAsync() => Query(@"
SELECT TOP(1000) Id,OccurredAt,Channel,Reference,Amount,MatchedPaymentId,MatchedVoucherId
FROM dbo.BankStatementLines WHERE MatchedPaymentId IS NULL AND MatchedVoucherId IS NULL ORDER BY Id",
        r=>new BankStatementLine(r.GetInt64(0),r.GetDateTime(1),r.GetString(2),r.GetString(3),r.GetDecimal(4),null,null));
    public Task<long> ImportBankLineAsync(DateTime at,string channel,string reference,decimal amount,int actor) => Scalar(
        "INSERT dbo.BankStatementLines(OccurredAt,Channel,Reference,Amount,ImportedBy) OUTPUT INSERTED.Id VALUES(@p0,@p1,@p2,@p3,@p4)",at,channel,reference,amount,actor);
    public async Task<(long? Payment,long? Voucher)> FindBankMatchAsync(string channel,string reference,decimal amount)
    {
        var payments=await Query(@"
SELECT p.Id FROM dbo.Payments p WHERE p.ExternalReference=@p1 AND
 ((@p0='Bank' AND p.Method=N'Chuyển khoản') OR (@p0='POS' AND p.Method=N'Thẻ POS'))
 AND CASE WHEN p.Kind='Refund' THEN -p.Amount WHEN p.Kind='Forfeit' THEN 0 ELSE p.Amount END=@p2",
            r=>r.GetInt64(0),channel,reference,amount);
        var vouchers=await Query(@"
SELECT v.Id FROM dbo.FinanceVouchers v WHERE v.Channel=@p0 AND v.Reference=@p1
 AND v.ReversedAt IS NULL AND CASE WHEN v.VoucherType='Receipt' THEN v.Amount ELSE -v.Amount END=@p2",
            r=>r.GetInt64(0),channel,reference,amount);
        if(payments.Count+vouchers.Count!=1)return (null,null);
        return (payments.SingleOrDefault() is var p && p>0?p:null,vouchers.SingleOrDefault() is var v && v>0?v:null);
    }
    public Task<int> SetBankMatchAsync(long line,long? payment,long? voucher) => Execute(
        "UPDATE dbo.BankStatementLines SET MatchedPaymentId=@p1,MatchedVoucherId=@p2 WHERE Id=@p0 AND MatchedPaymentId IS NULL AND MatchedVoucherId IS NULL",line,payment,voucher);
    public async Task<(string Type,decimal Amount,bool Reversed,decimal Allocated)> VoucherAllocationInfoAsync(long id) => (await Query(@"
SELECT v.VoucherType,v.Amount,CAST(CASE WHEN v.ReversedAt IS NULL THEN 0 ELSE 1 END AS bit),
 COALESCE((SELECT SUM(a.Amount) FROM dbo.DebtAllocations a WHERE a.VoucherId=v.Id),0)
FROM dbo.FinanceVouchers v WHERE v.Id=@p0",
        r=>(r.GetString(0),r.GetDecimal(1),r.GetBoolean(2),r.GetDecimal(3)),id)).SingleOrDefault();
    public async Task<(decimal Opening,decimal Receipts,decimal Payments,decimal Closing)> BookAsync(string channel,DateTime from,DateTime until)
    {
        var initial=await OpeningBalanceAsync(channel);
        var lower=initial?.AsOf??new DateTime(1900,1,1);
        if(from<lower)throw new BusinessException("Khoảng báo cáo trước ngày mở sổ của kênh này.");
        var rows=await Query(@"
SELECT
 COALESCE(SUM(CASE WHEN HappenedAt<@p1 THEN Amount ELSE 0 END),0),
 COALESCE(SUM(CASE WHEN HappenedAt>=@p1 AND Amount>0 THEN Amount ELSE 0 END),0),
 COALESCE(SUM(CASE WHEN HappenedAt>=@p1 AND Amount<0 THEN -Amount ELSE 0 END),0)
FROM (
 SELECT PostedAt HappenedAt,CASE WHEN VoucherType='Receipt' THEN Amount ELSE -Amount END Amount
 FROM dbo.FinanceVouchers WHERE Channel=@p0 AND PostedAt>=@p3 AND PostedAt<@p2 AND ReversedAt IS NULL
 UNION ALL
 SELECT Created,CASE WHEN Kind='Refund' THEN -Amount WHEN Kind='Forfeit' THEN 0 ELSE Amount END
 FROM dbo.Payments WHERE Created>=@p3 AND Created<@p2 AND
  ((@p0='Cash' AND Method=N'Tiền mặt') OR (@p0='Bank' AND Method=N'Chuyển khoản') OR
   (@p0='POS' AND Method=N'Thẻ POS') OR (@p0='OTA' AND Method=N'Công nợ OTA'))
) ledger",
            r=>(r.GetDecimal(0),r.GetDecimal(1),r.GetDecimal(2)),channel,from,until,lower);
        var x=rows[0];var opening=(initial?.Amount??0)+x.Item1;
        return (opening,x.Item2,x.Item3,opening+x.Item2-x.Item3);
    }
    public Task<long> AddDebtAsync(string type,string party,long? invoice,DateTime due,decimal amount,string note,int actor) => Scalar(@"
INSERT dbo.FinanceDebts(DebtType,Counterparty,InvoiceId,DueAt,Amount,Note,CreatedBy)
OUTPUT INSERTED.Id VALUES(@p0,@p1,@p2,@p3,@p4,@p5,@p6)",type,party,invoice,due.Date,amount,note,actor);
    public Task<List<Debt>> DebtsAsync() => Query(@"
SELECT d.Id,d.DebtType,d.Counterparty,d.InvoiceId,d.IssuedAt,d.DueAt,d.Amount,
 COALESCE(SUM(CASE WHEN v.ReversedAt IS NULL THEN a.Amount ELSE 0 END),0),d.Note
FROM dbo.FinanceDebts d LEFT JOIN dbo.DebtAllocations a ON a.DebtId=d.Id
LEFT JOIN dbo.FinanceVouchers v ON v.Id=a.VoucherId
WHERE d.CancelledAt IS NULL
GROUP BY d.Id,d.DebtType,d.Counterparty,d.InvoiceId,d.IssuedAt,d.DueAt,d.Amount,d.Note ORDER BY d.DueAt,d.Id",
        r=>new Debt(r.GetInt64(0),r.GetString(1),r.GetString(2),r.IsDBNull(3)?null:r.GetInt64(3),r.GetDateTime(4),r.GetDateTime(5),r.GetDecimal(6),r.GetDecimal(7),r.GetString(8)));
    public Task<int> CancelDebtAsync(long debt,int actor,string reason) => Execute(
        "UPDATE dbo.FinanceDebts SET CancelledAt=SYSDATETIME(),CancelledBy=@p1,CancelReason=@p2 WHERE Id=@p0 AND CancelledAt IS NULL",debt,actor,reason);
    public Task<int> AllocateDebtAsync(long debtId,long voucherId,decimal amount,int actor) => Execute(
        "INSERT dbo.DebtAllocations(DebtId,VoucherId,Amount,CreatedBy) VALUES(@p0,@p1,@p2,@p3)",debtId,voucherId,amount,actor);
    public Task<List<StockItem>> StockAsync() => Query(
        "SELECT Id,ServiceId,Name,Unit,Quantity,AverageCost,ReorderLevel,Active FROM dbo.StockItems ORDER BY Name",
        r=>new StockItem(r.GetInt32(0),r.IsDBNull(1)?null:r.GetInt32(1),r.GetString(2),r.GetString(3),r.GetDecimal(4),r.GetDecimal(5),r.GetDecimal(6),r.GetBoolean(7)));
    public Task<int> AddStockItemAsync(string name,string unit,int? service,decimal reorder) => Execute(
        "INSERT dbo.StockItems(Name,Unit,ServiceId,ReorderLevel) VALUES(@p0,@p1,@p2,@p3)",name,unit,service,reorder);
    public Task<int> UpdateStockAsync(int item,decimal quantity,decimal average) => Execute(
        "UPDATE dbo.StockItems SET Quantity=@p1,AverageCost=@p2 WHERE Id=@p0",item,quantity,average);
    public Task<int> MoveStockAsync(int item,string kind,decimal quantity,decimal cost,string reason,int actor,long? order=null) => Execute(@"
INSERT dbo.StockMovements(ItemId,Kind,Quantity,UnitCost,Reason,CreatedBy,ServiceOrderId)
VALUES(@p0,@p1,@p2,@p3,@p4,@p5,@p6)",item,kind,quantity,cost,reason,actor,order);
    public async Task ConsumeOrderStockAsync(long orderId,int quantity,int actor)
    {
        var rows=await Query(@"SELECT i.Id,i.Quantity,i.AverageCost FROM dbo.ServiceOrders o
JOIN dbo.StockItems i ON i.ServiceId=o.ServiceId WHERE o.Id=@p0 AND i.Active=1",
            r=>(Id:r.GetInt32(0),Quantity:r.GetDecimal(1),Cost:r.GetDecimal(2)),orderId);
        if(rows.Count==0)return;
        var stock=rows[0];
        if(stock.Quantity<quantity)throw new BusinessException("Tồn minibar không đủ để giao dịch vụ.");
        await UpdateStockAsync(stock.Id,stock.Quantity-quantity,stock.Cost);
        await MoveStockAsync(stock.Id,"Sale",-quantity,stock.Cost,$"Giao dịch vụ #{orderId}",actor,orderId);
    }
    public Task<int> ReportHousekeepingAsync(int item,long? stay,decimal qty,string note,int actor) => Execute(
        "INSERT dbo.HousekeepingConsumption(ItemId,StayId,Quantity,Note,CreatedBy) VALUES(@p0,@p1,@p2,@p3,@p4)",item,stay,qty,note,actor);
    public Task<List<StockMovement>> StockMovementsAsync(DateTime from,DateTime until) => Query(@"
SELECT m.Id,m.HappenedAt,i.Name,m.Kind,m.Quantity,m.UnitCost,m.Reason
FROM dbo.StockMovements m JOIN dbo.StockItems i ON i.Id=m.ItemId
WHERE m.HappenedAt>=@p0 AND m.HappenedAt<@p1 ORDER BY m.Id DESC",
        r=>new StockMovement(r.GetInt64(0),r.GetDateTime(1),r.GetString(2),r.GetString(3),r.GetDecimal(4),r.GetDecimal(5),r.GetString(6)),from,until);
    public Task<List<MinibarReconciliation>> ReconcileMinibarAsync(DateTime from,DateTime until) => Query(@"
SELECT i.Name,
 COALESCE((SELECT SUM(o.Quantity) FROM dbo.ServiceOrders o WHERE o.ServiceId=i.ServiceId AND o.Ordered>=@p0 AND o.Ordered<@p1 AND o.Cancelled IS NULL),0),
 COALESCE((SELECT SUM(h.Quantity) FROM dbo.HousekeepingConsumption h WHERE h.ItemId=i.Id AND h.ReportedAt>=@p0 AND h.ReportedAt<@p1),0),
 COALESCE((SELECT -SUM(m.Quantity) FROM dbo.StockMovements m WHERE m.ItemId=i.Id AND m.Kind='Sale' AND m.HappenedAt>=@p0 AND m.HappenedAt<@p1),0),
 i.Quantity
FROM dbo.StockItems i WHERE i.Active=1 ORDER BY i.Name",
        r=>new MinibarReconciliation(r.GetString(0),r.GetDecimal(1),r.GetDecimal(2),r.GetDecimal(3),r.GetDecimal(4),r.GetDecimal(1)-r.GetDecimal(2)),from,until);
    public Task<int> SaveInvoiceFinanceAsync(long invoice,decimal vat,decimal service,int rounding,string? eNumber) => Execute(@"
MERGE dbo.InvoiceFinance AS target USING (SELECT @p0 AS InvoiceId) AS source ON target.InvoiceId=source.InvoiceId
WHEN MATCHED THEN UPDATE SET VatRate=@p1,ServiceRate=@p2,RoundingUnit=@p3,EInvoiceNumber=@p4,EInvoiceIssuedAt=CASE WHEN @p4 IS NULL THEN NULL ELSE SYSDATETIME() END
WHEN NOT MATCHED THEN INSERT(InvoiceId,VatRate,ServiceRate,RoundingUnit,EInvoiceNumber,EInvoiceIssuedAt)
VALUES(@p0,@p1,@p2,@p3,@p4,CASE WHEN @p4 IS NULL THEN NULL ELSE SYSDATETIME() END);",invoice,vat,service,rounding,eNumber);
    public async Task<bool> HasEInvoiceAsync(long invoice) => (await Query(
        "SELECT 1 FROM dbo.InvoiceFinance WHERE InvoiceId=@p0 AND EInvoiceNumber IS NOT NULL",r=>true,invoice)).Count>0;
    public Task<int> AddAdjustmentAsync(long invoice,string category,decimal amount,string reason,int approver) => Execute(
        "INSERT dbo.InvoiceAdjustments(InvoiceId,Category,Amount,Reason,ApprovedBy) VALUES(@p0,@p1,@p2,@p3,@p4)",invoice,category,amount,reason,approver);
    public Task<int> VoidInvoiceAsync(long invoice,string reason,string explanation,int actor) => Execute(
        "INSERT dbo.InvoiceVoids(InvoiceId,ReasonCode,Explanation,VoidedBy) VALUES(@p0,@p1,@p2,@p3)",invoice,reason,explanation,actor);
    public Task<int> AddBillGroupAsync(string name,int actor) => Execute(
        "INSERT dbo.BillGroups(Name,CreatedBy) VALUES(@p0,@p1)",name,actor);
    public Task<long> NewBillGroupAsync(string name,int actor) => Scalar(
        "INSERT dbo.BillGroups(Name,CreatedBy) OUTPUT INSERTED.Id VALUES(@p0,@p1)",name,actor);
    public Task<int> AddBillShareAsync(long group,long invoice,string payer,decimal amount) => Execute(
        "INSERT dbo.BillShares(GroupId,InvoiceId,Payer,Amount) VALUES(@p0,@p1,@p2,@p3)",group,invoice,payer,amount);
    public Task<List<BillShareView>> BillSharesAsync() => Query(@"
SELECT g.Id,g.Name,s.InvoiceId,s.Payer,s.Amount,g.CreatedAt
FROM dbo.BillShares s JOIN dbo.BillGroups g ON g.Id=s.GroupId ORDER BY g.Id DESC,s.Id",
        r=>new BillShareView(r.GetInt64(0),r.GetString(1),r.GetInt64(2),r.GetString(3),r.GetDecimal(4),r.GetDateTime(5)));
    public Task<List<(long Id,decimal Total,bool Void)>> InvoiceAmountsAsync() => Query(
        "SELECT i.Id,i.RoomCharge+i.ServiceCharge-COALESCE((SELECT SUM(a.Amount) FROM dbo.InvoiceAdjustments a WHERE a.InvoiceId=i.Id),0),CASE WHEN v.InvoiceId IS NULL THEN CAST(0 AS bit) ELSE CAST(1 AS bit) END FROM dbo.Invoices i LEFT JOIN dbo.InvoiceVoids v ON v.InvoiceId=i.Id",
        r=>(r.GetInt64(0),r.GetDecimal(1),r.GetBoolean(2)));
    public Task<List<InvoiceControl>> InvoiceControlsAsync(DateTime from,DateTime until) => Query(@"
SELECT i.Id,CAST(CASE WHEN v.InvoiceId IS NULL THEN 0 ELSE 1 END AS bit),
 COALESCE(f.VatRate,0),COALESCE(f.ServiceRate,0),f.EInvoiceNumber,
 COALESCE((SELECT SUM(a.Amount) FROM dbo.InvoiceAdjustments a WHERE a.InvoiceId=i.Id),0)
FROM dbo.Invoices i LEFT JOIN dbo.InvoiceVoids v ON v.InvoiceId=i.Id
LEFT JOIN dbo.InvoiceFinance f ON f.InvoiceId=i.Id
WHERE i.Issued>=@p0 AND i.Issued<@p1",
        r=>new InvoiceControl(r.GetInt64(0),r.GetBoolean(1),r.GetDecimal(2),r.GetDecimal(3),r.IsDBNull(4)?null:r.GetString(4),r.GetDecimal(5)),from,until);
    public Task<List<Invoice>> AllFinanceInvoicesAsync(DateTime from,DateTime until) => Query(@"
SELECT Id,StayId,RoomNumber,GuestName,Issued,RoomCharge,ServiceCharge,Deposit,Collected,Refunded,Method
FROM dbo.Invoices WHERE Issued>=@p0 AND Issued<@p1 ORDER BY Id DESC",
        r=>new Invoice(r.GetInt64(0),r.GetInt64(1),r.GetString(2),r.GetString(3),r.GetDateTime(4),r.GetDecimal(5),r.GetDecimal(6),r.GetDecimal(7),r.GetDecimal(8),r.GetDecimal(9),r.GetString(10)),from,until);
    public async Task<FinanceSummary> FinanceSummaryAsync(DateTime from,DateTime until)
    {
        var row=await Query(@"
SELECT
 COALESCE((SELECT SUM(i.RoomCharge) FROM dbo.Invoices i LEFT JOIN dbo.InvoiceVoids v ON v.InvoiceId=i.Id WHERE v.InvoiceId IS NULL AND i.Issued>=@p0 AND i.Issued<@p1),0),
 COALESCE((SELECT SUM(o.Quantity*o.Price) FROM dbo.ServiceOrders o JOIN dbo.Invoices i ON i.StayId=o.StayId LEFT JOIN dbo.InvoiceVoids v ON v.InvoiceId=i.Id WHERE v.InvoiceId IS NULL AND o.Cancelled IS NULL AND o.Category=N'Minibar' AND i.Issued>=@p0 AND i.Issued<@p1),0),
 COALESCE((SELECT SUM(i.ServiceCharge) FROM dbo.Invoices i LEFT JOIN dbo.InvoiceVoids v ON v.InvoiceId=i.Id WHERE v.InvoiceId IS NULL AND i.Issued>=@p0 AND i.Issued<@p1),0),
 COALESCE((SELECT SUM(a.Amount) FROM dbo.InvoiceAdjustments a JOIN dbo.Invoices i ON i.Id=a.InvoiceId LEFT JOIN dbo.InvoiceVoids v ON v.InvoiceId=i.Id WHERE v.InvoiceId IS NULL AND a.CreatedAt>=@p0 AND a.CreatedAt<@p1),0),
 COALESCE((SELECT SUM(-m.Quantity*m.UnitCost) FROM dbo.StockMovements m WHERE m.Kind='Sale' AND m.HappenedAt>=@p0 AND m.HappenedAt<@p1),0),
 COALESCE((SELECT SUM(v.Amount) FROM dbo.FinanceVouchers v WHERE v.VoucherType='Payment' AND v.Category NOT IN(N'Mua hàng tồn kho',N'Trả công nợ',N'Tạm ứng') AND v.ReversedAt IS NULL AND v.PostedAt>=@p0 AND v.PostedAt<@p1),0)
 + COALESCE((SELECT SUM(-m.Quantity*m.UnitCost) FROM dbo.StockMovements m WHERE m.Kind IN('Spoilage','Internal') AND m.HappenedAt>=@p0 AND m.HappenedAt<@p1),0),
 COALESCE((SELECT SUM(CASE WHEN DATEDIFF(day,CONVERT(date,s.CheckIn),CONVERT(date,i.Issued))<1 THEN 1 ELSE DATEDIFF(day,CONVERT(date,s.CheckIn),CONVERT(date,i.Issued)) END) FROM dbo.Invoices i JOIN dbo.Stays s ON s.Id=i.StayId LEFT JOIN dbo.InvoiceVoids v ON v.InvoiceId=i.Id WHERE v.InvoiceId IS NULL AND i.Issued>=@p0 AND i.Issued<@p1),0),
 (SELECT COUNT(*) FROM dbo.Rooms)*DATEDIFF(day,@p0,@p1)
",r=>new FinanceSummary(r.GetDecimal(0),r.GetDecimal(1),r.GetDecimal(2)-r.GetDecimal(1),r.GetDecimal(3),r.GetDecimal(4),r.GetDecimal(5),r.GetInt32(6),r.GetInt32(7)),from,until);
        return row[0];
    }
}
