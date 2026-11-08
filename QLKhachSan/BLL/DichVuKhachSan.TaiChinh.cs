using QLKhachSan.DAL;
using QLKhachSan.DTO;
using System.Runtime.CompilerServices;

namespace QLKhachSan.BLL;

public sealed partial class HotelService
{
    private Task<T> FinanceWrite<T>(Func<HotelTransaction,Task<T>> action,[CallerMemberName]string caller="") => repository.RunAsync(true,async db=>
    {
        await db.RequireUserAsync(user);
        if(user.Role is not ("Admin" or "Accountant" or "Manager")) throw new BusinessException("Chỉ Kế toán hoặc Quản lý được ghi sổ.");
        FunctionPolicy.RequireMethod(user,caller);
        return await action(db);
    });
    private static string Required(string value,int max,string label)
    {
        value=value.Trim();
        if(value.Length==0 || value.Length>max || value.Any(char.IsControl))throw new BusinessException($"{label} không hợp lệ.");
        return value;
    }
    private static void Positive(decimal value) {if(value<=0 || value>1_000_000_000_000m)throw new BusinessException("Số tiền/số lượng phải lớn hơn 0 và trong giới hạn.");}
    public Task<List<CashShift>> CashShiftsAsync(DateTime from,DateTime through) => Read(async db=>
    {
        if(!RolePolicy.CanViewFinance(user.Role) && user.Role!="Reception")throw new BusinessException("Không có quyền xem ca.");
        return await db.ShiftsAsync(from.Date,through.Date.AddDays(1),user.Role=="Reception"?user.Id:0);
    });
    public Task<long> OpenCashShiftAsync(decimal opening) => repository.RunAsync(true,async db=>
    {
        await db.RequireUserAsync(user);
        FunctionPolicy.Require(user,"shift.manage");
        if(user.Role is not ("Admin" or "Reception" or "Accountant" or "Manager"))throw new BusinessException("Không có quyền mở ca.");
        if(opening<0 || opening>1_000_000_000_000m)throw new BusinessException("Tiền đầu ca không hợp lệ.");
        await db.RequireOpenPeriodAsync(await db.NowAsync());
        var id=await db.OpenShiftAsync(user.Id,opening);
        await db.AuditAsync(user,"OpenShift",$"Ca {id}; đầu ca {opening:N0}");
        return id;
    });
    public Task SubmitCashShiftAsync(long id,decimal counted,string explanation) => repository.RunAsync(true,async db=>
    {
        await db.RequireUserAsync(user);
        FunctionPolicy.Require(user,"shift.manage");
        if(user.Role is not ("Admin" or "Reception" or "Accountant" or "Manager"))throw new BusinessException("Không có quyền chốt ca.");
        if(counted<0 || counted>1_000_000_000_000m)throw new BusinessException("Tiền đếm thực tế không hợp lệ.");
        var shift=await db.ShiftIdentityAsync(id);
        if(shift.CashierId!=user.Id || shift.Status!="Open")throw new BusinessException("Chỉ người mở ca được nộp ca đang mở.");
        var expected=await db.ExpectedCashAsync(id);
        if(counted!=expected)explanation=Required(explanation,350,"Giải trình chênh lệch");
        else if(explanation.Trim().Length>350)throw new BusinessException("Giải trình tối đa 350 ký tự.");
        if(await db.SubmitShiftAsync(id,counted,expected,explanation.Trim())!=1)throw new BusinessException("Ca đã thay đổi.");
        return await db.AuditAsync(user,"SubmitShift",$"Ca {id}; lý thuyết {expected:N0}; đếm {counted:N0}; chênh {counted-expected:N0}; {explanation}");
    });
    public Task LockCashShiftAsync(long id) => FinanceWrite(async db=>
    {
        if(user.Role is not ("Admin" or "Accountant"))throw new BusinessException("Chỉ Kế toán được khóa ca.");
        if(await db.PendingOrdersInShiftAsync(id)>0)throw new BusinessException("Ca còn dịch vụ chưa giao hoặc chưa hủy; xử lý xong trước khi khóa.");
        if(await db.LockShiftAsync(id,user.Id)!=1)throw new BusinessException("Chỉ khóa được ca đã bàn giao.");
        return await db.AuditAsync(user,"LockShift",$"Ca {id}");
    });
    public Task LockAccountingPeriodAsync(DateTime month,string note) => FinanceWrite(async db=>
    {
        if(user.Role is not ("Admin" or "Accountant"))throw new BusinessException("Chỉ Kế toán được khóa kỳ.");
        if(month.Year<2020 || month.Year>2100)throw new BusinessException("Kỳ kế toán không hợp lệ.");
        var now=await db.NowAsync();
        if(new DateTime(month.Year,month.Month,1)>=new DateTime(now.Year,now.Month,1))throw new BusinessException("Chỉ được khóa tháng đã kết thúc.");
        note=Required(note,300,"Ghi chú khóa kỳ");
        if(await db.UnlockedShiftsInMonthAsync(month)>0)throw new BusinessException("Còn ca trong tháng chưa được Kế toán khóa.");
        await db.LockPeriodAsync(month,user.Id,note);
        return await db.AuditAsync(user,"LockPeriod",$"{month:MM/yyyy}; {note}");
    });
    public Task<List<Voucher>> VouchersAsync(DateTime from,DateTime through) => FinanceRead(db=>db.VouchersAsync(from.Date,through.Date.AddDays(1)));
    public Task<List<BankStatementLine>> BankLinesAsync(DateTime from,DateTime through) => FinanceRead(db=>db.BankLinesAsync(from.Date,through.Date.AddDays(1)));
    public Task<int> ReconcilePendingBankAsync() => FinanceWrite(async db=>
    {
        var count=0;
        foreach(var line in await db.UnmatchedBankLinesAsync())
        {
            var match=await db.FindBankMatchAsync(line.Channel,line.Reference,line.Amount);
            if(match.Payment is null && match.Voucher is null)continue;
            count+=await db.SetBankMatchAsync(line.Id,match.Payment,match.Voucher);
        }
        await db.AuditAsync(user,"BankReconcileRetry",$"Khớp bổ sung {count} dòng");return count;
    });
    public Task<int> ImportBankStatementsAsync(IReadOnlyList<(DateTime At,string Channel,string Reference,decimal Amount)> lines) => FinanceWrite(async db=>
    {
        if(lines.Count is <1 or >1000)throw new BusinessException("Nhập 1–1000 dòng sao kê mỗi lần.");
        var count=0;
        foreach(var line in lines)
        {
            if(line.Channel is not ("Bank" or "POS") || line.Amount==0 || line.Amount is >1_000_000_000_000m or < -1_000_000_000_000m)
                throw new BusinessException("Kênh hoặc số tiền sao kê không hợp lệ.");
            var reference=Required(line.Reference,100,"Mã giao dịch sao kê");
            var id=await db.ImportBankLineAsync(line.At,line.Channel,reference,line.Amount,user.Id);
            var matched=await db.FindBankMatchAsync(line.Channel,reference,line.Amount);
            if(matched.Payment is not null || matched.Voucher is not null){await db.SetBankMatchAsync(id,matched.Payment,matched.Voucher);count++;}
        }
        await db.AuditAsync(user,"BankReconcile",$"Nhập {lines.Count} dòng; khớp {count} dòng");
        return count;
    });
    public Task<List<InvoiceControl>> InvoiceControlsAsync(DateTime from,DateTime through) => FinanceRead(db=>db.InvoiceControlsAsync(from.Date,through.Date.AddDays(1)));
    public Task<List<Invoice>> AllFinanceInvoicesAsync(DateTime from,DateTime through) => FinanceRead(db=>db.AllFinanceInvoicesAsync(from.Date,through.Date.AddDays(1)));
    public Task<(decimal Opening,decimal Receipts,decimal Payments,decimal Closing)> BookAsync(string channel,DateTime from,DateTime through) => FinanceRead(db=>db.BookAsync(channel,from.Date,through.Date.AddDays(1)));
    public Task SetOpeningBalanceAsync(string channel,DateTime asOf,decimal amount,string note) => FinanceWrite(async db=>
    {
        if(channel is not ("Cash" or "Bank" or "POS" or "OTA"))throw new BusinessException("Kênh mở sổ không hợp lệ.");
        if(amount<0 || amount>1_000_000_000_000m)throw new BusinessException("Số dư mở sổ không hợp lệ.");
        note=Required(note,300,"Ghi chú số dư đầu");
        if(asOf.Date>(await db.NowAsync()).Date)throw new BusinessException("Ngày mở sổ không thể trong tương lai.");
        await db.RequireOpenPeriodAsync(asOf);
        if(await db.OpeningBalanceAsync(channel) is not null)throw new BusinessException("Kênh này đã có số dư mở sổ; không thể ghi đè.");
        await db.SetOpeningBalanceAsync(channel,asOf.Date,amount,note,user.Id);
        return await db.AuditAsync(user,"OpeningBalance",$"{channel}; {asOf:dd/MM/yyyy}; {amount:N0}; {note}");
    });
    public Task<long> PostVoucherAsync(string type,string channel,string category,decimal amount,string party,string? reference,string note) => FinanceWrite(async db=>
    {
        if(type is not ("Receipt" or "Payment") || channel is not ("Cash" or "Bank" or "POS" or "OTA"))throw new BusinessException("Loại phiếu hoặc kênh không hợp lệ.");
        Positive(amount);category=Required(category,80,"Hạng mục");party=Required(party,150,"Đối tượng");note=Required(note,500,"Diễn giải");
        reference=string.IsNullOrWhiteSpace(reference)?null:Required(reference,100,"Mã giao dịch");
        if(channel is "Bank" or "POS" && reference is null)throw new BusinessException("Giao dịch ngân hàng/POS cần mã tham chiếu.");
        var now=await db.NowAsync();await db.RequireOpenPeriodAsync(now);
        var shift=channel=="Cash"?await db.RequireOpenShiftAsync(user.Id):(long?)null;
        var id=await db.AddVoucherAsync(type,channel,category,amount,party,reference,note,user.Id,shift);
        await db.AuditAsync(user,"Voucher",$"Phiếu {id}; {type}; {amount:N0}; {channel}; {reference}");
        return id;
    });
    public Task<List<Debt>> DebtsAsync() => FinanceRead(db=>db.DebtsAsync());
    public Task<long> CreateDebtAsync(string type,string party,long? invoice,DateTime due,decimal amount,string note) => FinanceWrite(async db=>
    {
        if(type is not ("AR" or "AP"))throw new BusinessException("Loại công nợ không hợp lệ.");
        Positive(amount);party=Required(party,150,"Đối tượng");note=Required(note,500,"Diễn giải");
        var now=await db.NowAsync();await db.RequireOpenPeriodAsync(now);
        if(invoice is { } id)
        {
            if(type!="AR")throw new BusinessException("Mã hóa đơn khách sạn chỉ gắn với khoản phải thu.");
            var bill=(await db.InvoiceAmountsAsync()).SingleOrDefault(x=>x.Id==id);
            if(bill.Id==0 || bill.Void)throw new BusinessException("Hóa đơn không hợp lệ hoặc đã hủy.");
            var allocated=(await db.DebtsAsync()).Where(x=>x.Type=="AR" && x.InvoiceId==id).Sum(x=>x.Amount);
            if(allocated+amount>bill.Total)throw new BusinessException("Tổng công nợ vượt số tiền hóa đơn.");
        }
        var debt=await db.AddDebtAsync(type,party,invoice,due,amount,note,user.Id);
        await db.AuditAsync(user,"Debt",$"{type} #{debt}; {party}; {amount:N0}; hạn {due:dd/MM/yyyy}");
        return debt;
    });
    public Task AllocateDebtAsync(long debtId,long voucherId,decimal amount) => FinanceWrite(async db=>
    {
        Positive(amount);var debt=(await db.DebtsAsync()).SingleOrDefault(x=>x.Id==debtId)??throw new BusinessException("Công nợ không tồn tại.");
        if(amount>debt.Outstanding)throw new BusinessException("Số gạch nợ vượt số còn phải thu/trả.");
        var voucher=await db.VoucherAllocationInfoAsync(voucherId);
        if(voucher.Type is null || voucher.Reversed || voucher.Type!=(debt.Type=="AR"?"Receipt":"Payment"))throw new BusinessException("Phiếu không phù hợp với chiều công nợ.");
        if(amount>voucher.Amount-voucher.Allocated)throw new BusinessException("Số gạch nợ vượt phần chưa phân bổ của phiếu.");
        await db.RequireOpenPeriodAsync(await db.NowAsync());
        await db.AllocateDebtAsync(debtId,voucherId,amount,user.Id);
        return await db.AuditAsync(user,"DebtAllocation",$"Nợ {debtId}; phiếu {voucherId}; {amount:N0}");
    });
    public Task<List<StockItem>> StockAsync() => FinanceRead(db=>db.StockAsync());
    public Task<List<ServiceItem>> StockLinkableServicesAsync() => FinanceRead(db=>db.MenuAsync());
    public Task<List<StockMovement>> StockMovementsAsync(DateTime from,DateTime through) => FinanceRead(db=>db.StockMovementsAsync(from.Date,through.Date.AddDays(1)));
    public Task<List<MinibarReconciliation>> ReconcileMinibarAsync(DateTime from,DateTime through) => FinanceRead(db=>db.ReconcileMinibarAsync(from.Date,through.Date.AddDays(1)));
    public Task AddStockItemAsync(string name,string unit,int? serviceId,decimal reorder) => FinanceWrite(async db=>
    {
        name=Required(name,150,"Mặt hàng");unit=Required(unit,30,"Đơn vị");
        if(reorder<0)throw new BusinessException("Ngưỡng nhập hàng không hợp lệ.");
        await db.AddStockItemAsync(name,unit,serviceId,reorder);
        return await db.AuditAsync(user,"StockItem",name);
    });
    public Task MoveStockAsync(int itemId,string kind,decimal quantity,decimal unitCost,string reason) => FinanceWrite(async db=>
    {
        if(kind is not ("Purchase" or "Spoilage" or "Internal" or "Adjustment"))throw new BusinessException("Loại xuất nhập không hợp lệ.");
        Positive(quantity);if(unitCost<0)throw new BusinessException("Giá vốn không hợp lệ.");
        reason=Required(reason,400,"Lý do");var item=(await db.StockAsync()).SingleOrDefault(x=>x.Id==itemId)??throw new BusinessException("Mặt hàng không tồn tại.");
        var now=await db.NowAsync();await db.RequireOpenPeriodAsync(now);
        var delta=kind=="Purchase"?quantity:kind=="Adjustment"?quantity:-quantity;
        if(item.Quantity+delta<0)throw new BusinessException("Không đủ tồn kho.");
        var average=kind=="Purchase"?(item.Quantity*item.AverageCost+quantity*unitCost)/(item.Quantity+quantity):item.AverageCost;
        await db.UpdateStockAsync(itemId,item.Quantity+delta,decimal.Round(average,2));
        await db.MoveStockAsync(itemId,kind,delta,kind=="Purchase"?unitCost:item.AverageCost,reason,user.Id);
        return await db.AuditAsync(user,"StockMovement",$"{item.Name}; {kind}; {delta}; {reason}");
    });
    public Task ReportHousekeepingAsync(int itemId,long? stayId,decimal quantity,string note) => FinanceWrite(async db=>
    {
        Positive(quantity);note=Required(note,300,"Ghi chú buồng phòng");
        await db.ReportHousekeepingAsync(itemId,stayId,quantity,note,user.Id);
        return await db.AuditAsync(user,"HousekeepingConsumption",$"Hàng {itemId}; lượt {stayId}; SL {quantity}");
    });
    public Task<FinanceSummary> FinanceSummaryAsync(DateTime from,DateTime through) => FinanceRead(db=>db.FinanceSummaryAsync(from.Date,through.Date.AddDays(1)));
    public Task<List<BillShareView>> BillSharesAsync() => FinanceRead(db=>db.BillSharesAsync());
    public Task<List<Invoice>> GroupInvoicesAsync(long groupId) => FinanceRead(db=>db.GroupInvoicesAsync(groupId));
    public Task VoidInvoiceAsync(long invoice,string code,string explanation) => FinanceWrite(async db=>
    {
        if(code is not ("GuestCancelled" or "WrongRoomType" or "RoomChange"))throw new BusinessException("Lý do hủy không hợp lệ.");
        explanation=Required(explanation,400,"Giải trình hủy");
        var item=(await db.InvoiceAmountsAsync()).SingleOrDefault(x=>x.Id==invoice);
        if(item.Id==0 || item.Void)throw new BusinessException("Hóa đơn không tồn tại hoặc đã hủy.");
        var now=await db.NowAsync();await db.RequireOpenPeriodAsync(now);
        var debts=(await db.DebtsAsync()).Where(x=>x.InvoiceId==invoice).ToArray();
        if(debts.Any(x=>x.Paid>0))throw new BusinessException("Hóa đơn đã gạch công nợ; cần xử lý chứng từ hoàn/điều chỉnh trước khi hủy.");
        foreach(var debt in debts)await db.CancelDebtAsync(debt.Id,user.Id,$"Hủy hóa đơn {invoice}: {explanation}");
        await db.VoidInvoiceAsync(invoice,code,explanation,user.Id);
        return await db.AuditAsync(user,"VoidInvoice",$"Hóa đơn {invoice}; {code}; {explanation}");
    });
    public Task AddInvoiceAdjustmentAsync(long invoice,string category,decimal amount,string reason) => FinanceWrite(async db=>
    {
        if(category is not ("ServiceFailure" or "VIP" or "Voucher"))throw new BusinessException("Loại giảm trừ không hợp lệ.");
        Positive(amount);reason=Required(reason,400,"Lý do giảm trừ");
        var item=(await db.InvoiceAmountsAsync()).SingleOrDefault(x=>x.Id==invoice);
        if(item.Id==0 || item.Void || amount>item.Total)throw new BusinessException("Hóa đơn/số giảm trừ không hợp lệ.");
        await db.RequireOpenPeriodAsync(await db.NowAsync());
        await db.AddAdjustmentAsync(invoice,category,amount,reason,user.Id);
        return await db.AuditAsync(user,"InvoiceAdjustment",$"Hóa đơn {invoice}; {category}; {amount:N0}; {reason}");
    });
    public Task MarkEInvoiceAsync(long invoice,string number,decimal vat,decimal serviceRate,int rounding) => FinanceWrite(async db=>
    {
        number=Required(number,100,"Số hóa đơn điện tử");
        if(vat is not (8 or 10) || serviceRate is not (0 or 5) || rounding is not (1 or 100 or 500 or 1000))throw new BusinessException("Thuế/phí hoặc đơn vị làm tròn không hợp lệ.");
        if(!(await db.InvoiceAmountsAsync()).Any(x=>x.Id==invoice && !x.Void))throw new BusinessException("Hóa đơn không hợp lệ.");
        if(await db.HasEInvoiceAsync(invoice))throw new BusinessException("Hóa đơn đã được đánh dấu phát hành; không thể xuất trùng hoặc ghi đè số.");
        await db.RequireOpenPeriodAsync(await db.NowAsync());
        await db.SaveInvoiceFinanceAsync(invoice,vat,serviceRate,rounding,number);
        return await db.AuditAsync(user,"EInvoice",$"Hóa đơn {invoice}; số điện tử {number}; VAT {vat}%; phí {serviceRate}%");
    });
    public Task<long> GroupBillsAsync(string name,IReadOnlyList<(long Invoice,string Payer,decimal Amount)> shares) => FinanceWrite(async db=>
    {
        name=Required(name,150,"Tên hóa đơn tổng");if(shares.Count==0)throw new BusinessException("Cần ít nhất một phần hóa đơn.");
        var invoices=await db.InvoiceAmountsAsync();
        var grouped=(await db.BillSharesAsync()).Select(x=>x.InvoiceId).ToHashSet();
        foreach(var group in shares.GroupBy(x=>x.Invoice))
        {
            var invoice=invoices.SingleOrDefault(x=>x.Id==group.Key);
            if(grouped.Contains(group.Key) || invoice.Id==0 || invoice.Void || group.Sum(x=>x.Amount)!=invoice.Total)throw new BusinessException($"Hóa đơn {group.Key} đã được nhóm hoặc các phần không bằng tổng hóa đơn.");
        }
        await db.RequireOpenPeriodAsync(await db.NowAsync());
        var id=await db.NewBillGroupAsync(name,user.Id);
        foreach(var share in shares){Positive(share.Amount);await db.AddBillShareAsync(id,share.Invoice,Required(share.Payer,150,"Người trả"),share.Amount);}
        await db.AuditAsync(user,"BillGroup",$"Nhóm {id}; {shares.Count} phần");return id;
    });
}
