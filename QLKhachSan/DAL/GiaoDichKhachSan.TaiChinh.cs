using Microsoft.Data.SqlClient;
using QLKhachSan.DTO;

namespace QLKhachSan.DAL;

public sealed partial class HotelTransaction
{
    public Task<bool> PeriodLockedAsync(DateTime at) => Task.FromResult(false);
    public Task RequireOpenPeriodAsync(DateTime at) => Task.CompletedTask;

    public Task<List<CashShift>> ShiftsAsync(DateTime from, DateTime until, int cashierId = 0) => Query(@"
SELECT s.Ma,u.TenDangNhap,s.ThoiDiemMoCa,s.ThoiDiemDongCa,s.TienMatDauCa,s.TienMatKiemDem,s.TienMatDuKien,s.GiaiTrinh,s.TrangThai,
 COALESCE((SELECT SUM(CASE WHEN p.LoaiGiaoDich IN('Deposit','Checkout') THEN p.SoTien ELSE 0 END)
           FROM dbo.GiaoDichThanhToan p WHERE p.MaCaTruc=s.Ma AND p.PhuongThucThanhToan=N'Tiền mặt'), 0),
 COALESCE((SELECT SUM(CASE WHEN p.LoaiGiaoDich='Refund' THEN p.SoTien ELSE 0 END)
           FROM dbo.GiaoDichThanhToan p WHERE p.MaCaTruc=s.Ma AND p.PhuongThucThanhToan=N'Tiền mặt'), 0),
 COALESCE((SELECT SUM(p.SoTien) FROM dbo.GiaoDichThanhToan p WHERE p.MaCaTruc=s.Ma AND (p.PhuongThucThanhToan=N'Thẻ' OR p.PhuongThucThanhToan=N'Thẻ POS')), 0),
 COALESCE((SELECT SUM(p.SoTien) FROM dbo.GiaoDichThanhToan p WHERE p.MaCaTruc=s.Ma AND p.PhuongThucThanhToan=N'Chuyển khoản'), 0),
 CAST(0 AS decimal(18,2))
FROM dbo.CaTruc s JOIN dbo.TaiKhoanNhanVien u ON u.Ma=s.MaNhanVien
WHERE ((s.ThoiDiemMoCa>=@p0 AND s.ThoiDiemMoCa<@p1) OR s.TrangThai='Open') AND (@p2=0 OR s.MaNhanVien=@p2)
ORDER BY s.Ma DESC", r => new CashShift(r.GetInt64(0), r.GetString(1), r.GetDateTime(2), Date(r, 3), Convert.ToDecimal(r[4]),
        r.IsDBNull(5) ? null : Convert.ToDecimal(r[5]), r.IsDBNull(6) ? null : Convert.ToDecimal(r[6]), r.IsDBNull(7) ? null : r.GetString(7),
        r.GetString(8), Convert.ToDecimal(r[9]), Convert.ToDecimal(r[10]), Convert.ToDecimal(r[11]), Convert.ToDecimal(r[12]), Convert.ToDecimal(r[13])),
        from, until, cashierId);

    public async Task<(int CashierId, string Status)> ShiftIdentityAsync(long id) => (await Query(
        "SELECT MaNhanVien,TrangThai FROM dbo.CaTruc WHERE Ma=@p0", r => (r.GetInt32(0), r.GetString(1)), id)).SingleOrDefault();

    public async Task<long> OpenShiftAsync(int cashier, decimal opening)
    {
        var existing = (await Query("SELECT Ma FROM dbo.CaTruc WHERE MaNhanVien=@p0 AND TrangThai='Open'", r => r.GetInt64(0), cashier)).SingleOrDefault();
        if (existing > 0) throw new BusinessException($"Tài khoản này đang có ca trực #{existing} đang mở. Hãy bàn giao ca cũ trước khi mở ca mới.");
        return await Scalar("INSERT dbo.CaTruc(MaNhanVien,TienMatDauCa) VALUES(@p0,@p1); SELECT CONVERT(bigint,SCOPE_IDENTITY());", cashier, opening);
    }

    public async Task<long> RequireOpenShiftAsync(int cashier) => (await Query(
        "SELECT Ma FROM dbo.CaTruc WHERE MaNhanVien=@p0 AND TrangThai='Open'", r => r.GetInt64(0), cashier)).SingleOrDefault() is var id && id > 0
        ? id : throw new BusinessException("Hãy mở ca và nhập tiền mặt đầu ca trước khi thu/chi.");

    public async Task<decimal> ExpectedCashAsync(long shiftId)
    {
        var rows = await Query(@"
SELECT s.TienMatDauCa
 + COALESCE((SELECT SUM(CASE WHEN p.LoaiGiaoDich='Refund' THEN -p.SoTien WHEN p.LoaiGiaoDich='Forfeit' THEN 0 ELSE p.SoTien END)
             FROM dbo.GiaoDichThanhToan p WHERE p.MaCaTruc=s.Ma AND p.PhuongThucThanhToan=N'Tiền mặt'), 0)
FROM dbo.CaTruc s WHERE s.Ma=@p0 AND s.TrangThai='Open'", r => Convert.ToDecimal(r[0]), shiftId);
        return rows.Count > 0 ? rows[0] : 0m;
    }

    public Task<int> SubmitShiftAsync(long id, decimal counted, decimal expected, string explanation) => Execute(
        "UPDATE dbo.CaTruc SET TrangThai='Submitted',ThoiDiemDongCa=SYSDATETIME(),TienMatKiemDem=@p1,TienMatDuKien=@p2,GiaiTrinh=@p3 WHERE Ma=@p0 AND TrangThai='Open'", id, counted, expected, explanation);

    public Task<int> LockShiftAsync(long id, int approver) => Execute(
        "UPDATE dbo.CaTruc SET TrangThai='Locked',ThoiDiemKhoa=SYSDATETIME(),MaNguoiKhoa=@p1 WHERE Ma=@p0 AND TrangThai='Submitted'", id, approver);

    public Task<long> PendingOrdersInShiftAsync(long id) => Task.FromResult(0L);
    public Task<int> LockPeriodAsync(DateTime month, int approver, string note) => Task.FromResult(0);
    public Task<long> UnlockedShiftsInMonthAsync(DateTime month) => Task.FromResult(0L);
    public Task<long> AddVoucherAsync(string type, string channel, string category, decimal amount, string party, string? reference, string note, int actor, long? shift) => Task.FromResult(0L);
    public Task<(DateTime AsOf, decimal Amount)?> OpeningBalanceAsync(string channel) => Task.FromResult<ValueTuple<DateTime, decimal>?>(null);
    public Task<int> SetOpeningBalanceAsync(string channel, DateTime asOf, decimal amount, string note, int actor) => Task.FromResult(0);
    public Task<List<Voucher>> VouchersAsync(DateTime from, DateTime until) => Task.FromResult(new List<Voucher>());
    public Task<List<BankStatementLine>> BankLinesAsync(DateTime from, DateTime until) => Task.FromResult(new List<BankStatementLine>());
    public Task<List<BankStatementLine>> UnmatchedBankLinesAsync() => Task.FromResult(new List<BankStatementLine>());
    public Task<long> ImportBankLineAsync(DateTime at, string channel, string reference, decimal amount, int actor) => Task.FromResult(0L);
    public Task<(long? Payment, long? Voucher)> FindBankMatchAsync(string channel, string reference, decimal amount) => Task.FromResult<(long?, long?)>((null, null));
    public Task<int> SetBankMatchAsync(long line, long? payment, long? voucher) => Task.FromResult(0);
    public Task<(string Type, decimal Amount, bool Reversed, decimal Allocated)> VoucherAllocationInfoAsync(long id) => Task.FromResult(("", 0m, false, 0m));
    public Task<(decimal Opening, decimal Receipts, decimal Payments, decimal Closing)> BookAsync(string channel, DateTime from, DateTime until) => Task.FromResult((0m, 0m, 0m, 0m));

    public Task<long> AddDebtAsync(string type, string party, long? invoice, DateTime due, decimal amount, string note, int actor) => Task.FromResult(0L);
    public Task<List<Debt>> DebtsAsync() => Task.FromResult(new List<Debt>());
    public Task<int> CancelDebtAsync(long debt, int actor, string reason) => Task.FromResult(0);
    public Task<int> AllocateDebtAsync(long debtId, long voucherId, decimal amount, int actor) => Task.FromResult(0);

    public Task<List<StockItem>> StockAsync() => Task.FromResult(new List<StockItem>());
    public Task<List<(int ServiceId, decimal Quantity)>> ReservedServiceStockAsync() => Task.FromResult(new List<(int, decimal)>());
    public Task<List<RentalOut>> RentalOutAsync() => Task.FromResult(new List<RentalOut>());
    public Task<int> ReturnRentalAsync(long orderId, int quantity, int actor, string note) => Task.FromResult(0);
    public Task<int> OrderServiceIdAsync(long orderId) => Task.FromResult(0);
    public Task<int> AddStockItemAsync(string name, string unit, int? service, decimal reorder) => Task.FromResult(0);
    public Task EnsureServiceStockItemsAsync() => Task.CompletedTask;
    public Task<int> UpdateStockAsync(int item, decimal quantity, decimal average) => Task.FromResult(0);
    public Task<int> MoveStockAsync(int item, string kind, decimal quantity, decimal cost, string reason, int actor, long? order = null) => Task.FromResult(0);
    public Task ConsumeOrderStockAsync(long orderId, int quantity, int actor) => Task.CompletedTask;
    public Task<int> ReportHousekeepingAsync(int item, long? stay, decimal qty, string note, int actor) => Task.FromResult(0);
    public Task<List<StockMovement>> StockMovementsAsync(DateTime from, DateTime until) => Task.FromResult(new List<StockMovement>());
    public Task<List<MinibarReconciliation>> ReconcileMinibarAsync(DateTime from, DateTime until) => Task.FromResult(new List<MinibarReconciliation>());

    public Task<int> SaveInvoiceFinanceAsync(long invoice, decimal vat, decimal service, int rounding, string? eNumber) => Task.FromResult(0);
    public Task<bool> HasEInvoiceAsync(long invoice) => Task.FromResult(false);
    public Task<int> AddAdjustmentAsync(long invoice, string category, decimal amount, string reason, int approver) => Task.FromResult(0);
    public Task<int> VoidInvoiceAsync(long invoice, string reason, string explanation, int actor) => Task.FromResult(0);
    public Task<int> AddBillGroupAsync(string name, int actor) => Task.FromResult(0);
    public Task<long> NewBillGroupAsync(string name, int actor) => Task.FromResult(0L);
    public Task<int> AddBillShareAsync(long group, long invoice, string payer, decimal amount) => Task.FromResult(0);
    public Task<List<BillShareView>> BillSharesAsync() => Task.FromResult(new List<BillShareView>());
    public Task<List<Invoice>> GroupInvoicesAsync(long groupId) => Task.FromResult(new List<Invoice>());
    public Task<List<(long Id, decimal Total, bool Void)>> InvoiceAmountsAsync() => Task.FromResult(new List<(long, decimal, bool)>());

    public Task<List<InvoiceControl>> InvoiceControlsAsync(DateTime from, DateTime until) => Task.FromResult(new List<InvoiceControl>());

    public Task<List<Invoice>> AllFinanceInvoicesAsync(DateTime from, DateTime until) => Query(@"
SELECT Ma,MaLuotLuuTru,SoPhongHoaDon,TenKhach,ThoiDiemLapHoaDon,TienPhong,TienDichVu,TienCoc,TienDaThu,TienDaHoan,PhuongThucThanhToan
FROM dbo.HoaDon WHERE ThoiDiemLapHoaDon>=@p0 AND ThoiDiemLapHoaDon<@p1 ORDER BY Ma DESC",
        r => new Invoice(r.GetInt64(0), r.GetInt64(1), r.GetString(2), r.GetString(3), r.GetDateTime(4), r.GetDecimal(5), r.GetDecimal(6), r.GetDecimal(7), r.GetDecimal(8), r.GetDecimal(9), r.GetString(10)), from, until);

    public async Task<FinanceSummary> FinanceSummaryAsync(DateTime from, DateTime until)
    {
        var row = await Query(@"
SELECT
 COALESCE((SELECT SUM(i.TienPhong) FROM dbo.HoaDon i WHERE i.ThoiDiemLapHoaDon>=@p0 AND i.ThoiDiemLapHoaDon<@p1),0),
 0,
 0,
 0,
 0,
 0,
 COALESCE((SELECT SUM(CASE WHEN DATEDIFF(day,CONVERT(date,s.ThoiDiemNhanPhong),CONVERT(date,i.ThoiDiemLapHoaDon))<1 THEN 1 ELSE DATEDIFF(day,CONVERT(date,s.ThoiDiemNhanPhong),CONVERT(date,i.ThoiDiemLapHoaDon)) END) FROM dbo.HoaDon i JOIN dbo.LuotLuuTru s ON s.Ma=i.MaLuotLuuTru WHERE i.ThoiDiemLapHoaDon>=@p0 AND i.ThoiDiemLapHoaDon<@p1),0),
 (SELECT COUNT(*) FROM dbo.Phong)*DATEDIFF(day,@p0,@p1)
", r => new FinanceSummary(r.GetDecimal(0), 0, 0, 0, 0, 0, r.GetInt32(6), r.GetInt32(7)), from, until);
        return row[0];
    }
}
