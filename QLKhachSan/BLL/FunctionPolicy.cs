using QLKhachSan.DTO;

namespace QLKhachSan.BLL;

public sealed record FunctionDefinition(string Code, string Name, string Menu, string Submenu, string Roles);

public static class FunctionPolicy
{
    public static readonly IReadOnlyList<FunctionDefinition> All =
    [
        new("room.map", "Sơ đồ phòng", "rooms", "Phòng", "Admin,Reception,Manager"),
        new("room.search", "Tìm phòng", "rooms", "Phòng", "Admin,Reception,Manager"),
        new("room.walkin", "Nhận phòng trực tiếp", "rooms", "Đặt phòng", "Admin,Reception"),
        new("room.reserve", "Đặt phòng trước", "rooms", "Đặt phòng", "Admin,Reception"),
        new("room.deposit", "Thu cọc bổ sung", "rooms", "Đặt phòng", "Admin,Reception"),
        new("room.checkin", "Nhận phòng đã đặt", "rooms", "Đặt phòng", "Admin,Reception"),
        new("room.booking_edit", "Sửa lượt đặt", "rooms", "Đặt phòng", "Admin,Reception"),
        new("room.booking_cancel", "Hủy lượt đặt", "rooms", "Đặt phòng", "Admin,Reception"),
        new("room.schedule", "Lịch đến / đi", "rooms", "Lưu trú", "Admin,Reception,Manager"),
        new("room.checkout", "Trả phòng", "rooms", "Lưu trú", "Admin,Reception"),
        new("room.transfer", "Chuyển phòng", "rooms", "Lưu trú", "Admin,Reception"),
        new("room.extend", "Gia hạn lưu trú", "rooms", "Lưu trú", "Admin,Reception"),
        new("room.history", "Lịch đặt / lịch sử", "rooms", "Lưu trú", "Admin,Reception,Manager"),
        new("room.clean", "Xong dọn phòng", "rooms", "Buồng phòng", "Admin,Reception"),
        new("room.maintain", "Bảo trì phòng", "rooms", "Buồng phòng", "Admin,Manager"),
        new("room.catalog", "Danh mục phòng", "rooms", "Danh mục phòng", "Admin,Manager"),
        new("room.pricing", "Bảng giá", "rooms", "Danh mục phòng", "Admin,Manager"),
        new("service.order", "Gọi dịch vụ", "services", "Yêu cầu dịch vụ", "Admin,Reception"),
        new("service.manage", "Xử lý dịch vụ", "services", "Yêu cầu dịch vụ", "Admin,Reception"),
        new("service.catalog", "Danh mục dịch vụ", "services", "Danh mục dịch vụ", "Admin,Manager"),
        new("service.stock", "Kho minibar", "services", "Kho dịch vụ", "Admin,Accountant,Manager"),
        new("customer.profile", "Hồ sơ khách hàng", "customers", "Hồ sơ", "Admin,Reception,Manager"),
        new("customer.history", "Lịch sử khách", "customers", "Lịch sử", "Admin,Reception,Manager"),
        new("shift.manage", "Ca trực / bàn giao", "shifts", "Ca trực", "Admin,Reception,Accountant,Manager"),
        new("cash.flow", "Các khoản thu / hoàn", "cash", "Thu chi", "Admin,Accountant,Manager"),
        new("cash.book", "Sổ quỹ", "cash", "Sổ quỹ", "Admin,Accountant,Manager"),
        new("cash.bank", "Đối soát ngân hàng", "cash", "Đối soát", "Admin,Accountant,Manager"),
        new("cash.debt", "Công nợ", "cash", "Công nợ", "Admin,Accountant,Manager"),
        new("invoice.create", "Lập hóa đơn thanh toán", "invoices", "Hóa đơn", "Admin,Reception"),
        new("invoice.list", "Hóa đơn / doanh thu", "invoices", "Hóa đơn", "Admin,Accountant,Manager"),
        new("invoice.control", "Điều chỉnh hóa đơn", "invoices", "Kiểm soát", "Admin,Accountant,Manager"),
        new("invoice.groups", "Nhóm bill", "invoices", "Nhóm bill", "Admin,Accountant,Manager"),
        new("report.revenue", "Biểu đồ doanh thu", "reports", "Doanh thu", "Admin,Accountant,Manager"),
        new("report.profit", "Lãi lỗ", "reports", "Lãi lỗ", "Admin,Accountant,Manager"),
        new("staff.view", "Danh sách nhân viên", "staff", "Nhân viên", "Admin,Manager"),
        new("staff.manage", "Tài khoản & phân quyền", "staff", "Phân quyền", "Admin"),
        new("system.password", "Đổi mật khẩu", "system", "Tài khoản", "Admin,Reception,Accountant,Manager"),
        new("system.audit", "Nhật ký thao tác", "system", "Nhật ký", "Admin,Manager")
    ];

    private static readonly Dictionary<string, FunctionDefinition> ByCode = All.ToDictionary(x => x.Code);
    private static readonly Dictionary<string, string[]> MethodAccess = new(StringComparer.Ordinal)
    {
        ["CustomersAsync"]=["customer.profile"], ["CustomerInvoicesAsync"]=["customer.profile"],
        ["CustomerOrdersAsync"]=["customer.profile"], ["InvoicesAsync"]=["invoice.list"],
        ["RevenueAsync"]=["report.revenue","invoice.list"], ["ReportAsync"]=["report.revenue","invoice.list"],
        ["CreateStayAsync"]=["room.walkin","room.reserve"], ["CheckInAsync"]=["room.checkin"],
        ["CancelAsync"]=["room.booking_cancel"], ["TransferAsync"]=["room.transfer"],
        ["ExtendAsync"]=["room.extend"], ["AddServicesAsync"]=["service.order"],
        ["DeliverAsync"]=["service.manage"], ["SetRoomStatusAsync"]=["room.clean","room.maintain"],
        ["CleanAllAsync"]=["room.clean"], ["QuoteAsync"]=["room.checkout","invoice.create"],
        ["CheckoutAsync"]=["room.checkout","invoice.create"], ["RefundQuoteAsync"]=["room.booking_cancel"],
        ["GroupQuoteAsync"]=["room.checkout","invoice.create"], ["GroupCheckoutAsync"]=["room.checkout","invoice.create"],
        ["StayHistoryAsync"]=["room.history","customer.history"], ["TodayScheduleAsync"]=["room.schedule"],
        ["DepositHistoryAsync"]=["room.deposit","room.history","customer.history"],
        ["DepositReceiptsAsync"]=["invoice.list"],
        ["StayOrdersAsync"]=["service.manage"], ["UpdateBookingAsync"]=["room.booking_edit"],
        ["PeriodReportAsync"]=["invoice.list","cash.flow"], ["InvoiceStayAsync"]=["invoice.list"],
        ["InvoiceOrdersAsync"]=["invoice.list"], ["AddDepositAsync"]=["room.deposit"],
        ["ChangeOrderAsync"]=["service.manage"], ["DeliverOrderAsync"]=["service.manage"],
        ["CancelPendingOrdersAsync"]=["service.manage"],
        ["CatalogAsync"]=["service.catalog"],
        ["SaveServiceAsync"]=["service.catalog"],
        ["SaveRoomAsync"]=["room.catalog","room.pricing"], ["UpdateRoomsAsync"]=["room.catalog"],
        ["DeleteRoomAsync"]=["room.catalog"],
        ["AuditsAsync"]=["system.audit"],
        ["CashShiftsAsync"]=["shift.manage"], ["LockCashShiftAsync"]=["shift.manage"],
        ["LockAccountingPeriodAsync"]=["shift.manage"],
        ["VouchersAsync"]=["cash.book"], ["BankLinesAsync"]=["cash.bank"],
        ["ReconcilePendingBankAsync"]=["cash.bank"], ["ImportBankStatementsAsync"]=["cash.bank"],
        ["InvoiceControlsAsync"]=["invoice.control"], ["AllFinanceInvoicesAsync"]=["invoice.control"],
        ["BookAsync"]=["cash.book"], ["SetOpeningBalanceAsync"]=["cash.book"],
        ["PostVoucherAsync"]=["cash.book"], ["DebtsAsync"]=["cash.debt"],
        ["CreateDebtAsync"]=["cash.debt"], ["AllocateDebtAsync"]=["cash.debt"],
        ["StockAsync"]=["service.stock"], ["StockMovementsAsync"]=["service.stock"],
        ["StockLinkableServicesAsync"]=["service.stock"],
        ["ReconcileMinibarAsync"]=["service.stock"], ["AddStockItemAsync"]=["service.stock"],
        ["MoveStockAsync"]=["service.stock"], ["ReportHousekeepingAsync"]=["service.stock"],
        ["FinanceSummaryAsync"]=["report.profit"], ["BillSharesAsync"]=["invoice.groups"],
        ["GroupInvoicesAsync"]=["invoice.groups"],
        ["VoidInvoiceAsync"]=["invoice.control"], ["AddInvoiceAdjustmentAsync"]=["invoice.control"],
        ["MarkEInvoiceAsync"]=["invoice.control"], ["GroupBillsAsync"]=["invoice.groups"]
    };

    public static bool RoleAllows(string role, string code) =>
        ByCode.TryGetValue(code, out var function) && function.Roles.Split(',').Contains(role);

    public static bool Can(UserSession user, string code) => RoleAllows(user.Role, code)
        && (user.GrantedFunctions is null || user.GrantedFunctions.Contains(code));

    public static bool CanAny(UserSession user, string menu) => All.Any(x => x.Menu == menu && Can(user, x.Code));

    public static HashSet<string> Defaults(string role) => All.Where(x => RoleAllows(role, x.Code))
        .Select(x => x.Code).ToHashSet(StringComparer.Ordinal);

    public static void Require(UserSession user, string code)
    {
        if (!Can(user, code)) throw new BusinessException("Bạn không được cấp quyền sử dụng chức năng này.");
    }

    public static void RequireMethod(UserSession user, string method)
    {
        if (method is "DashboardAsync" or "ServerNowAsync" or "ExpireReservationsAsync") return;
        if (!MethodAccess.TryGetValue(method, out var codes) || !codes.Any(code => Can(user, code)))
            throw new BusinessException("Bạn không được cấp quyền sử dụng chức năng này.");
    }
}
