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
    private readonly HashSet<int> selectedBookingRooms = [];
    private bool availableReserve;

    private void ShowBookingScreen(bool reserve)
    {
        ClearPage(functionPage);
        availableReserve = reserve;
        selectedBookingRooms.Clear();
        var heading = FunctionHeading(activeFunction ?? "room.reserve",
            "Chọn hình thức, thời gian và một hoặc nhiều phòng cho khách.");

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 5,
            Padding = new Padding(14, 12, 14, 14), BackColor = AppTheme.Canvas
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 78));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 74));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 64));

        var modePanel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2,
            BackColor = AppTheme.Canvas, Padding = new Padding(0, 0, 0, 8) };
        modePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        modePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        var nowButton = new Button { Text = "NHẬN PHÒNG NGAY\nKhách đến và nhận phòng hôm nay",
            Dock = DockStyle.Fill, Margin = new Padding(0, 0, 6, 0) };
        var reserveButton = new Button { Text = "ĐẶT PHÒNG TRƯỚC\nGiữ phòng cho ngày đến dự kiến",
            Dock = DockStyle.Fill, Margin = new Padding(6, 0, 0, 0) };
        modePanel.Controls.Add(nowButton, 0, 0);
        modePanel.Controls.Add(reserveButton, 1, 0);
        layout.Controls.Add(modePanel, 0, 0);

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
        AddFilter(filters, "Ngày đến", availableArrival);
        AddFilter(filters, "Số ngày", availableDays);
        layout.Controls.Add(filters, 0, 1);

        availableCount = new Label
        {
            Dock = DockStyle.Fill, Font = AppTheme.Bold, ForeColor = AppTheme.Ink,
            TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(10, 0, 0, 0)
        };
        layout.Controls.Add(availableCount, 0, 2);

        availableRoomGrid = Ui.Grid();
        availableRoomGrid.MultiSelect = false;
        availableRoomGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        availableRoomGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        var bedImage = UiIcons.Create("bed", AppTheme.Blue, 24);
        availableRoomGrid.Disposed += (_, _) => bedImage.Dispose();
        availableRoomGrid.Columns.Add(new DataGridViewImageColumn
        {
            Name = "roomIcon", HeaderText = "", Image = bedImage,
            Width = 48, AutoSizeMode = DataGridViewAutoSizeColumnMode.None
        });
        availableRoomGrid.Columns.Add(new DataGridViewCheckBoxColumn
        {
            Name = "selectedRoom", HeaderText = "Chọn", Width = 66,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.None, ReadOnly = true
        });
        layout.Controls.Add(availableRoomGrid, 0, 3);

        var footer = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(12, 10, 12, 10) };
        availableSelect = new Button
        {
            Text = "TIẾP TỤC VỚI PHÒNG ĐÃ CHỌN", Dock = DockStyle.Right, Width = 310,
            Image = UiIcons.Create("bed", Color.White, 20),
            ImageAlign = ContentAlignment.MiddleLeft, TextImageRelation = TextImageRelation.ImageBeforeText
        };
        AppTheme.Button(availableSelect, true);
        availableSelect.Disposed += (_, _) => availableSelect.Image?.Dispose();
        footer.Controls.Add(availableSelect);
        var booked=new Button {Text="PHÒNG ĐANG Ở / THANH TOÁN",Dock=DockStyle.Left,Width=275};
        AppTheme.Button(booked);
        booked.Click+=(_,_)=>ShowOccupiedRoomsOverview();
        footer.Controls.Add(booked);
        layout.Controls.Add(footer, 0, 4);

        availableSearch.TextChanged += (_, _) => { selectedBookingRooms.Clear(); RefreshAvailableRooms(); };
        void SetMode(bool requested)
        {
            if (!FunctionPolicy.Can(user, requested ? "room.reserve" : "room.walkin")) return;
            availableReserve = requested;
            selectedBookingRooms.Clear();
            availableArrival.Enabled = requested;
            AppTheme.Button(nowButton, !requested);
            AppTheme.Button(reserveButton, requested);
            RefreshAvailableRooms();
        }
        nowButton.Enabled = FunctionPolicy.Can(user, "room.walkin");
        reserveButton.Enabled = FunctionPolicy.Can(user, "room.reserve");
        nowButton.Click += (_, _) => SetMode(false);
        reserveButton.Click += (_, _) => SetMode(true);
        SetMode(reserve);
        availableType.SelectedIndexChanged += (_, _) => { selectedBookingRooms.Clear(); RefreshAvailableRooms(); };
        availableArrival.ValueChanged += (_, _) => { selectedBookingRooms.Clear(); RefreshAvailableRooms(); };
        availableDays.ValueChanged += (_, _) => { selectedBookingRooms.Clear(); RefreshAvailableRooms(); };
        availableSelect.Click += async (_, _) => await SelectAvailableRoom();
        availableRoomGrid.CellClick += (_, e) =>
        {
            if (e.RowIndex < 0 || availableRoomGrid.Rows[e.RowIndex].DataBoundItem is not AvailableRoomRow row) return;
            if (selectedBookingRooms.Contains(row.Id)) selectedBookingRooms.Remove(row.Id);
            else if (selectedBookingRooms.Count < 20) selectedBookingRooms.Add(row.Id);
            else { Ui.Error(this,new BusinessException("Chọn tối đa 20 phòng mỗi lần.")); return; }
            availableRoomGrid.Rows[e.RowIndex].Cells["selectedRoom"].Value = selectedBookingRooms.Contains(row.Id);
            UpdateAvailableSelection();
        };
        availableRoomGrid.KeyDown += (_, e) =>
        {
            if (e.KeyCode != Keys.Enter && e.KeyCode != Keys.Space) return;
            e.Handled = true;
            e.SuppressKeyPress = true;
            if (availableRoomGrid.CurrentRow?.DataBoundItem is not AvailableRoomRow row) return;
            if (selectedBookingRooms.Contains(row.Id)) selectedBookingRooms.Remove(row.Id);
            else if (selectedBookingRooms.Count < 20) selectedBookingRooms.Add(row.Id);
            else { Ui.Error(this,new BusinessException("Chọn tối đa 20 phòng mỗi lần.")); return; }
            availableRoomGrid.CurrentRow.Cells["selectedRoom"].Value = selectedBookingRooms.Contains(row.Id);
            UpdateAvailableSelection();
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
        selectedBookingRooms.IntersectWith(rows.Select(row => row.Id));
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
        availableRoomGrid.Columns["selectedRoom"]!.DisplayIndex = 1;
        foreach (DataGridViewRow row in availableRoomGrid.Rows)
            if (row.DataBoundItem is AvailableRoomRow item)
                row.Cells["selectedRoom"].Value = selectedBookingRooms.Contains(item.Id);
        if (previouslySelected is { } selectedId)
            foreach (DataGridViewRow row in availableRoomGrid.Rows)
                if (row.DataBoundItem is AvailableRoomRow item && item.Id == selectedId)
                {
                    availableRoomGrid.CurrentCell = row.Cells["Phòng"];
                    break;
                }
        availableCount.Text = availableReserve
            ? $"{rows.Count} phòng còn chỗ từ {from:dd/MM/yyyy HH:mm} · Đã chọn {selectedBookingRooms.Count} phòng"
            : $"{rows.Count} phòng sẵn sàng · Đã chọn {selectedBookingRooms.Count} phòng";
        UpdateAvailableSelection();
    }

    private void UpdateAvailableSelection()
    {
        if (availableSelect is null || availableSelect.IsDisposed || availableRoomGrid is null ||
            availableRoomGrid.IsDisposed) return;
        availableSelect.Enabled = selectedBookingRooms.Count > 0;
        availableSelect.Text = selectedBookingRooms.Count > 0
            ? $"TIẾP TỤC · {selectedBookingRooms.Count} PHÒNG" : "CHỌN PHÒNG ĐỂ TIẾP TỤC";
        if (availableCount is not null && !availableCount.IsDisposed)
        {
            var total = availableRoomGrid.Rows.Count;
            availableCount.Text = availableReserve
                ? $"{total} phòng còn chỗ · Đã chọn {selectedBookingRooms.Count} phòng"
                : $"{total} phòng sẵn sàng · Đã chọn {selectedBookingRooms.Count} phòng";
        }
    }

    private async Task SelectAvailableRoom()
    {
        if (availableRoomGrid is null ||
            availableDays is null || availableArrival is null) return;
        var rooms = data.Rooms.Where(item => selectedBookingRooms.Contains(item.Id)).ToArray();
        if (rooms.Length == 0) { RefreshAvailableRooms(); return; }
        var arrival = availableReserve ? availableArrival.Value : ServerNow;
        var days = (int)availableDays.Value;
        await Run(async () =>
        {
            if(await ShowMultipleBooking(rooms, availableReserve, arrival, days))
                selectedBookingRooms.Clear();
        });
        RefreshAvailableRooms();
    }

    private sealed record OccupiedRoomRow(long Id,string Phòng,string Khách,string SốĐiệnThoại,
        DateTime NgàyNhận,DateTime NgàyTrảDựKiến,decimal CọcĐãThu);

    private void ShowOccupiedRoomsOverview()
    {
        using var form=new Form {Text="Các phòng đang có khách",Size=new Size(1050,650),
            MinimumSize=new Size(760,480),StartPosition=FormStartPosition.CenterParent,
            Font=AppTheme.Body,BackColor=AppTheme.Canvas};
        var root=new TableLayoutPanel {Dock=DockStyle.Fill,RowCount=3,ColumnCount=1,Padding=new Padding(16)};
        root.RowStyles.Add(new RowStyle(SizeType.Absolute,76));
        root.RowStyles.Add(new RowStyle(SizeType.Percent,100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute,58));
        form.Controls.Add(root);
        var search=Ui.Text(100);search.PlaceholderText="Tìm số phòng, tên khách hoặc số điện thoại";
        search.Dock=DockStyle.Top;
        var heading=new Panel {Dock=DockStyle.Fill,BackColor=Color.White,Padding=new Padding(12,10,12,5)};
        heading.Controls.Add(search);
        heading.Controls.Add(new Label {Text="DANH SÁCH PHÒNG ĐANG Ở",Dock=DockStyle.Top,
            Height=32,Font=AppTheme.Bold,ForeColor=AppTheme.Ink});
        root.Controls.Add(heading,0,0);
        var grid=Ui.Grid();grid.Dock=DockStyle.Fill;grid.ReadOnly=true;
        root.Controls.Add(grid,0,1);
        void Refresh()
        {
            var query=search.Text.Trim();
            grid.DataSource=data.Stays.Where(stay=>stay.Status==StayStatus.Occupied)
                .Select(stay=>new OccupiedRoomRow(stay.Id,StayRoom(stay)?.Number??"?",
                    stay.Guest,stay.Phone,stay.CheckIn??stay.Arrival,stay.Departure,stay.Deposit))
                .Where(row=>query.Length==0 ||
                    row.Phòng.Contains(query,StringComparison.CurrentCultureIgnoreCase) ||
                    row.Khách.Contains(query,StringComparison.CurrentCultureIgnoreCase) ||
                    row.SốĐiệnThoại.Contains(query,StringComparison.CurrentCultureIgnoreCase))
                .OrderBy(row=>row.Phòng).ToList();
            if(grid.Columns["Id"] is { } id)id.Visible=false;
            if(grid.Columns["NgàyNhận"] is { } arrival)arrival.DefaultCellStyle.Format="dd/MM/yyyy HH:mm";
            if(grid.Columns["NgàyTrảDựKiến"] is { } departure)departure.DefaultCellStyle.Format="dd/MM/yyyy HH:mm";
            if(grid.Columns["CọcĐãThu"] is { } deposit)deposit.DefaultCellStyle.Format="N0";
        }
        search.TextChanged+=(_,_)=>Refresh();
        var close=new Button {Text="ĐÓNG",Dock=DockStyle.Right,Width=170};
        AppTheme.Button(close);close.Click+=(_,_)=>form.Close();
        var pay=new Button {Text="XEM BILL / THANH TOÁN",Dock=DockStyle.Right,Width=275,
            Margin=new Padding(0,0,8,0)};
        AppTheme.Button(pay,true);
        pay.Enabled=FunctionPolicy.Can(user,"room.checkout") || FunctionPolicy.Can(user,"invoice.create");
        var paying=false;
        async Task PaySelected()
        {
            if(paying || grid.CurrentRow?.DataBoundItem is not OccupiedRoomRow selected)return;
            var stay=data.Stays.SingleOrDefault(x=>x.Id==selected.Id && x.Status==StayStatus.Occupied);
            if(stay is null){Refresh();return;}
            paying=true;pay.Enabled=false;
            try
            {
                await ShowCheckout(stay);
                Refresh();
            }
            catch(Exception ex){Ui.Error(form,ex);}
            finally
            {
                paying=false;
                if(!form.IsDisposed)
                    pay.Enabled=(FunctionPolicy.Can(user,"room.checkout") ||
                        FunctionPolicy.Can(user,"invoice.create")) && grid.CurrentRow is not null;
            }
        }
        pay.Click+=async (_,_)=>await PaySelected();
        grid.CellDoubleClick+=async (_,e)=>{if(e.RowIndex>=0 && pay.Enabled)await PaySelected();};
        grid.SelectionChanged+=(_,_)=>pay.Enabled=!paying && grid.CurrentRow is not null &&
            (FunctionPolicy.Can(user,"room.checkout") || FunctionPolicy.Can(user,"invoice.create"));
        var footer=new Panel {Dock=DockStyle.Fill,BackColor=Color.White,Padding=new Padding(8)};
        footer.Controls.Add(close);footer.Controls.Add(pay);root.Controls.Add(footer,0,2);
        Refresh();form.ShowDialog(this);
    }
}
