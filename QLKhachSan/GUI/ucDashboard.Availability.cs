using QLKhachSan.BLL;
using QLKhachSan.DTO;

namespace QLKhachSan.GUI;

public partial class ucDashboard
{
    private sealed record AvailableRoomRow(int Id, string Phòng, string Loại, decimal GiáMỗiNgày,
        decimal CọcGợiÝ, string TrạngTháiHiệnTại);

    private DataGridView? availableRoomGrid;
    private DateTimePicker? availableArrival;
    private NumericUpDown? availableDays;
    private TextBox? availableSearch;
    private ComboBox? availableType;
    private Label? availableCount;
    private Button? availableSelect;
    private bool availableReserve;

    private void ShowBookingScreen(bool reserve)
    {
        ClearPage(functionPage);
        availableReserve = reserve;
        var heading = FunctionHeading(reserve ? "room.reserve" : "room.walkin",
            reserve ? "Chọn ngày đến và phòng còn chỗ, sau đó nhập thông tin khách."
                    : "Chọn phòng trống để nhận khách.");

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4,
            Padding = new Padding(14, 12, 14, 14), BackColor = AppTheme.Canvas
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 74));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 64));

        var filters = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(12, 15, 8, 6),
            WrapContents = true, AutoScroll = true
        };
        availableSearch = Ui.Text(40);
        availableSearch.Dock = DockStyle.None;
        availableSearch.Width = 180;
        availableSearch.PlaceholderText = "Số phòng...";
        availableType = Ui.Combo(new[] { "Tất cả", "Đơn", "Đôi", "VIP", "Tình nhân" });
        availableType.Dock = DockStyle.None;
        availableType.Width = 120;
        availableArrival = Ui.DatePicker(ServerNow.AddHours(2));
        availableArrival.Dock = DockStyle.None;
        availableArrival.Width = 190;
        availableDays = Ui.Number(60);
        availableDays.Dock = DockStyle.None;
        availableDays.Width = 75;
        AddFilter(filters, "Tìm phòng", availableSearch);
        AddFilter(filters, "Loại", availableType);
        if (reserve) AddFilter(filters, "Ngày đến", availableArrival);
        AddFilter(filters, "Số ngày", availableDays);
        layout.Controls.Add(filters, 0, 0);

        availableCount = new Label
        {
            Dock = DockStyle.Fill, Font = AppTheme.Bold, ForeColor = AppTheme.Ink,
            TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(10, 0, 0, 0)
        };
        layout.Controls.Add(availableCount, 0, 1);

        availableRoomGrid = Ui.Grid();
        availableRoomGrid.MultiSelect = false;
        availableRoomGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        var bedImage = UiIcons.Create("bed", AppTheme.Blue, 24);
        availableRoomGrid.Disposed += (_, _) => bedImage.Dispose();
        availableRoomGrid.Columns.Add(new DataGridViewImageColumn
        {
            Name = "roomIcon", HeaderText = "", Image = bedImage,
            Width = 48, AutoSizeMode = DataGridViewAutoSizeColumnMode.None
        });
        availableRoomGrid.Columns.Add(new DataGridViewButtonColumn
        {
            Name = "chooseRoom", HeaderText = "", Text = "Chọn phòng",
            UseColumnTextForButtonValue = true, Width = 130,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.None
        });
        layout.Controls.Add(availableRoomGrid, 0, 2);

        var footer = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(12, 10, 12, 10) };
        availableSelect = new Button
        {
            Text = "CHỌN PHÒNG VÀ TIẾP TỤC", Dock = DockStyle.Right, Width = 280,
            Image = UiIcons.Create("bed", Color.White, 20),
            ImageAlign = ContentAlignment.MiddleLeft, TextImageRelation = TextImageRelation.ImageBeforeText
        };
        AppTheme.Button(availableSelect, true);
        availableSelect.Disposed += (_, _) => availableSelect.Image?.Dispose();
        footer.Controls.Add(availableSelect);
        layout.Controls.Add(footer, 0, 3);

        availableSearch.TextChanged += (_, _) => RefreshAvailableRooms();
        availableType.SelectedIndexChanged += (_, _) => RefreshAvailableRooms();
        availableArrival.ValueChanged += (_, _) => RefreshAvailableRooms();
        availableDays.ValueChanged += (_, _) => RefreshAvailableRooms();
        availableRoomGrid.SelectionChanged += (_, _) => UpdateAvailableSelection();
        availableSelect.Click += async (_, _) => await SelectAvailableRoom();
        availableRoomGrid.CellContentClick += async (_, e) =>
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0 &&
                availableRoomGrid.Columns[e.ColumnIndex].Name == "chooseRoom")
            {
                availableRoomGrid.CurrentCell = availableRoomGrid.Rows[e.RowIndex].Cells["Phòng"];
                await SelectAvailableRoom();
            }
        };
        availableRoomGrid.CellDoubleClick += async (_, e) =>
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0 &&
                availableRoomGrid.Columns[e.ColumnIndex].Name != "chooseRoom")
                await SelectAvailableRoom();
        };
        availableRoomGrid.KeyDown += async (_, e) =>
        {
            if (e.KeyCode != Keys.Enter) return;
            e.Handled = true;
            e.SuppressKeyPress = true;
            await SelectAvailableRoom();
        };

        functionPage.Controls.Add(layout);
        functionPage.Controls.Add(heading);
        ActivatePage(functionPage);
        UpdateNavigationTitle();
        RefreshAvailableRooms();
    }

    private static void AddFilter(FlowLayoutPanel panel, string label, Control field)
    {
        panel.Controls.Add(new Label
        {
            Text = label, AutoSize = true, Font = AppTheme.Bold,
            ForeColor = AppTheme.Muted, Margin = new Padding(7, 8, 7, 0)
        });
        field.Margin = new Padding(0, 0, 13, 0);
        panel.Controls.Add(field);
    }

    private void RefreshAvailableRooms()
    {
        if (availableRoomGrid is null || availableRoomGrid.IsDisposed ||
            availableArrival is null || availableDays is null ||
            availableSearch is null || availableType is null ||
            availableCount is null || availableSelect is null) return;

        var previouslySelected = (availableRoomGrid.CurrentRow?.DataBoundItem as AvailableRoomRow)?.Id;
        var from = availableReserve ? availableArrival.Value : ServerNow;
        var until = from.AddDays((int)availableDays.Value);
        var search = availableSearch.Text.Trim();
        var type = availableType.SelectedItem as string ?? "Tất cả";
        var rows = data.Rooms
            .Where(room => availableReserve ? room.Status != RoomStatus.BaoTri : room.Status == RoomStatus.Trong)
            .Where(room => type == "Tất cả" || room.Type == type)
            .Where(room => search.Length == 0 || room.Number.Contains(search, StringComparison.CurrentCultureIgnoreCase))
            .Where(room => !data.Stays.Any(stay => stay.RoomId == room.Id &&
                (stay.CheckIn ?? stay.Arrival) < until && stay.Departure > from))
            .OrderBy(room => room.Number)
            .Select(room => new AvailableRoomRow(room.Id, room.Number, room.Type, room.Rate,
                room.Deposit, Ui.Status(room.Status))).ToList();
        availableRoomGrid.DataSource = rows;
        if (availableRoomGrid.Columns["Id"] is { } id) id.Visible = false;
        if (availableRoomGrid.Columns["GiáMỗiNgày"] is { } price)
        {
            price.HeaderText = "Giá mỗi ngày";
            price.DefaultCellStyle.Format = "N0";
        }
        if (availableRoomGrid.Columns["CọcGợiÝ"] is { } deposit)
        {
            deposit.HeaderText = "Cọc gợi ý";
            deposit.DefaultCellStyle.Format = "N0";
        }
        if (availableRoomGrid.Columns["TrạngTháiHiệnTại"] is { } status)
            status.HeaderText = "Trạng thái hiện tại";
        availableRoomGrid.Columns["roomIcon"]!.DisplayIndex = 0;
        availableRoomGrid.Columns["chooseRoom"]!.DisplayIndex = availableRoomGrid.Columns.Count - 1;
        if (previouslySelected is { } selectedId)
            foreach (DataGridViewRow row in availableRoomGrid.Rows)
                if (row.DataBoundItem is AvailableRoomRow item && item.Id == selectedId)
                {
                    availableRoomGrid.CurrentCell = row.Cells["Phòng"];
                    break;
                }
        availableCount.Text = availableReserve
            ? $"{rows.Count} phòng có thể đặt từ {from:dd/MM/yyyy HH:mm} trong {availableDays.Value:0} ngày"
            : $"{rows.Count} phòng trống có thể nhận khách";
        UpdateAvailableSelection();
    }

    private void UpdateAvailableSelection()
    {
        if (availableSelect is null || availableSelect.IsDisposed || availableRoomGrid is null ||
            availableRoomGrid.IsDisposed) return;
        availableSelect.Enabled = availableRoomGrid.CurrentRow?.DataBoundItem is AvailableRoomRow;
    }

    private async Task SelectAvailableRoom()
    {
        if (availableRoomGrid?.CurrentRow?.DataBoundItem is not AvailableRoomRow row ||
            availableDays is null || availableArrival is null) return;
        var room = data.Rooms.SingleOrDefault(item => item.Id == row.Id);
        if (room is null) { RefreshAvailableRooms(); return; }
        var arrival = availableReserve ? availableArrival.Value : (DateTime?)null;
        var days = (int)availableDays.Value;
        await Run(() => ShowBooking(availableReserve, room, arrival, days));
        RefreshAvailableRooms();
    }
}
