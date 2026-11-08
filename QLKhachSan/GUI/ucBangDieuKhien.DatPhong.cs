using QLKhachSan.BLL;
using QLKhachSan.DTO;

namespace QLKhachSan.GUI;

public partial class ucDashboard
{
    private sealed record ReservedBookingRow(long Id, string Phòng, string Khách, string SốĐiệnThoại,
        DateTime NgàyĐến, DateTime? HạnNhận, decimal CọcĐãThu, string TìnhTrạng);

    private DataGridView? reservedBookingGrid;
    private TextBox? reservedBookingSearch;
    private Label? reservedBookingCount;
    private Button? reservedBookingAction;
    private string? reservedBookingFunction;

    private void ShowReservedBookingScreen(string code)
    {
        ClearPage(functionPage);
        reservedBookingFunction = code;
        var name = FunctionPolicy.All.Single(x => x.Code == code).Name;
        var heading = FunctionHeading(code, "Chọn lượt đặt trong bảng để tiếp tục.");
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4,
            Padding = new Padding(14, 12, 14, 14), BackColor = AppTheme.Canvas
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 64));

        var searchPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill, BackColor = Color.White,
            Padding = new Padding(14, 12, 12, 8), WrapContents = false
        };
        reservedBookingSearch = Ui.Text(100);
        reservedBookingSearch.Dock = DockStyle.None;
        reservedBookingSearch.Width = 340;
        reservedBookingSearch.PlaceholderText = "Số phòng, tên khách hoặc điện thoại...";
        AddFilter(searchPanel, "Tìm lượt đặt", reservedBookingSearch);
        layout.Controls.Add(searchPanel, 0, 0);

        reservedBookingCount = new Label
        {
            Dock = DockStyle.Fill, Font = AppTheme.Bold, ForeColor = AppTheme.Ink,
            TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(10, 0, 0, 0)
        };
        layout.Controls.Add(reservedBookingCount, 0, 1);

        reservedBookingGrid = Ui.Grid();
        reservedBookingGrid.MultiSelect = false;
        var calendarImage = UiIcons.Create("calendar", AppTheme.Blue, 23);
        reservedBookingGrid.Disposed += (_, _) => calendarImage.Dispose();
        reservedBookingGrid.Columns.Add(new DataGridViewImageColumn
        {
            Name = "bookingIcon", HeaderText = "", Image = calendarImage,
            Width = 48, AutoSizeMode = DataGridViewAutoSizeColumnMode.None
        });
        layout.Controls.Add(reservedBookingGrid, 0, 2);

        var footer = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(12, 10, 12, 10) };
        reservedBookingAction = new Button
        {
            Text = name.ToUpperInvariant(), Dock = DockStyle.Right, Width = 255,
            Image = UiIcons.Create(FunctionIcon(code), Color.White, 20),
            ImageAlign = ContentAlignment.MiddleLeft, TextImageRelation = TextImageRelation.ImageBeforeText
        };
        AppTheme.Button(reservedBookingAction, true);
        reservedBookingAction.Disposed += (_, _) => reservedBookingAction.Image?.Dispose();
        var depositHistory=new Button {Text="LỊCH SỬ CỌC / CÁC PHÒNG",Dock=DockStyle.Left,Width=245};
        AppTheme.Button(depositHistory);
        depositHistory.Click+=async (_,_)=>
        {
            if(reservedBookingGrid?.CurrentRow?.DataBoundItem is not ReservedBookingRow selected)return;
            try{await ShowCustomerDepositHistory(selected.Id);}catch(Exception ex){Ui.Error(this,ex);}
        };
        footer.Controls.Add(depositHistory);
        footer.Controls.Add(reservedBookingAction);
        layout.Controls.Add(footer, 0, 3);

        reservedBookingSearch.TextChanged += (_, _) => RefreshReservedBookings();
        reservedBookingGrid.SelectionChanged += (_, _) => UpdateReservedSelection();
        reservedBookingAction.Click += async (_, _) => await SelectReservedBooking();
        reservedBookingGrid.CellDoubleClick += async (_, e) =>
        {
            if (e.RowIndex >= 0) await SelectReservedBooking();
        };
        reservedBookingGrid.KeyDown += async (_, e) =>
        {
            if (e.KeyCode != Keys.Enter) return;
            e.Handled = true;
            e.SuppressKeyPress = true;
            await SelectReservedBooking();
        };

        functionPage.Controls.Add(layout);
        functionPage.Controls.Add(heading);
        ActivatePage(functionPage);
        UpdateNavigationTitle();
        RefreshReservedBookings();
    }

    private void RefreshReservedBookings()
    {
        if (reservedBookingGrid is null || reservedBookingGrid.IsDisposed ||
            reservedBookingSearch is null || reservedBookingCount is null) return;
        var selectedId = (reservedBookingGrid.CurrentRow?.DataBoundItem as ReservedBookingRow)?.Id;
        var search = reservedBookingSearch.Text.Trim();
        var rows = data.Stays.Where(stay => stay.Status == StayStatus.Reserved)
            .Select(stay => new ReservedBookingRow(stay.Id, StayRoom(stay)?.Number ?? "?",
                stay.Guest, stay.Phone, stay.Arrival, stay.HoldUntil, stay.Deposit,
                stay.HoldUntil <= ServerNow ? "Quá hạn nhận" : "Chờ nhận"))
            .Where(row => search.Length == 0 ||
                row.Phòng.Contains(search, StringComparison.CurrentCultureIgnoreCase) ||
                row.Khách.Contains(search, StringComparison.CurrentCultureIgnoreCase) ||
                row.SốĐiệnThoại.Contains(search, StringComparison.CurrentCultureIgnoreCase))
            .OrderBy(row => row.NgàyĐến).ThenBy(row => row.Phòng).ToList();
        reservedBookingGrid.DataSource = rows;
        if (reservedBookingGrid.Columns["Id"] is { } id) id.Visible = false;
        if (reservedBookingGrid.Columns["Phòng"] is { } room) room.FillWeight = 70;
        if (reservedBookingGrid.Columns["Khách"] is { } guest) guest.FillWeight = 150;
        if (reservedBookingGrid.Columns["SốĐiệnThoại"] is { } phone)
        {
            phone.HeaderText = "Điện thoại";
            phone.FillWeight = 130;
        }
        if (reservedBookingGrid.Columns["NgàyĐến"] is { } arrival)
        {
            arrival.HeaderText = "Ngày đến";
            arrival.DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
            arrival.FillWeight = 145;
        }
        if (reservedBookingGrid.Columns["HạnNhận"] is { } hold)
        {
            hold.HeaderText = "Hạn nhận";
            hold.DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
            hold.FillWeight = 145;
        }
        if (reservedBookingGrid.Columns["CọcĐãThu"] is { } deposit)
        {
            deposit.HeaderText = "Cọc đã thu";
            deposit.DefaultCellStyle.Format = "N0";
            deposit.FillWeight = 105;
        }
        if (reservedBookingGrid.Columns["TìnhTrạng"] is { } status)
        {
            status.HeaderText = "Tình trạng";
            status.FillWeight = 110;
        }
        reservedBookingGrid.Columns["bookingIcon"]!.DisplayIndex = 0;
        foreach (DataGridViewRow row in reservedBookingGrid.Rows)
            if (row.DataBoundItem is ReservedBookingRow item && item.TìnhTrạng == "Quá hạn nhận")
                row.DefaultCellStyle.BackColor = Color.FromArgb(255, 246, 242);
        if (selectedId is { } previous)
            foreach (DataGridViewRow row in reservedBookingGrid.Rows)
                if (row.DataBoundItem is ReservedBookingRow item && item.Id == previous)
                {
                    reservedBookingGrid.CurrentCell = row.Cells["Phòng"];
                    break;
                }
        reservedBookingCount.Text = $"{rows.Count} lượt đặt trước đang chờ nhận";
        UpdateReservedSelection();
    }

    private void UpdateReservedSelection()
    {
        if (reservedBookingAction is null || reservedBookingAction.IsDisposed ||
            reservedBookingGrid is null || reservedBookingGrid.IsDisposed) return;
        reservedBookingAction.Enabled = reservedBookingGrid.CurrentRow?.DataBoundItem is ReservedBookingRow row &&
            (reservedBookingFunction == "room.booking_cancel" || row.TìnhTrạng != "Quá hạn nhận");
    }

    private async Task SelectReservedBooking()
    {
        if (reservedBookingGrid?.CurrentRow?.DataBoundItem is not ReservedBookingRow row) return;
        if (reservedBookingFunction != "room.booking_cancel" && row.TìnhTrạng == "Quá hạn nhận") return;
        var stay = data.Stays.SingleOrDefault(item => item.Id == row.Id && item.Status == StayStatus.Reserved);
        if (stay is null) { RefreshReservedBookings(); return; }
        await Run(async () =>
        {
            switch (reservedBookingFunction)
            {
                case "room.deposit":
                    await ShowDeposit(stay);
                    break;
                case "room.checkin":
                    if (Ui.Confirm(this, $"Nhận phòng cho {stay.Guest}?"))
                        await Changed(() => service.CheckInAsync(stay));
                    break;
                case "room.booking_edit":
                    await ShowEditBooking(stay);
                    break;
                case "room.booking_cancel":
                    await ShowCancel(stay);
                    break;
            }
        });
        RefreshReservedBookings();
    }
}
