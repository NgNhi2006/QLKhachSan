using QLKhachSan.BLL;
using QLKhachSan.DTO;

namespace QLKhachSan.GUI;

public partial class ucDashboard
{
    private Panel? availableRoomMap;
    private Panel? availableRoomColumns;
    private ToolTip? availableRoomTip;
    private int availableRoomCount;
    private DateTimePicker? availableArrival;
    private NumericUpDown? availableDays;
    private TextBox? availableSearch;
    private ComboBox? availableType;
    private Label? availableCount;
    private Button? availableSelect;
    private Button? availableCheckout;
    private readonly HashSet<int> selectedBookingRooms = [];
    private bool availableReserve;

    private void ShowBookingScreen(bool reserve)
    {
        ClearPage(functionPage);
        availableReserve = reserve;
        selectedBookingRooms.Clear();
        var heading = FunctionHeading(activeFunction ?? "room.reserve",
            "Chọn hình thức, thời gian và một hoặc nhiều phòng trực tiếp trên sơ đồ.");

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
        availableType = Ui.Combo(new[] { "Tất cả", "Đơn", "Đôi", "VIP" });
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

        var bookingTabs=new TabControl {Dock=DockStyle.Fill,Font=AppTheme.Bold};
        var mapTab=new TabPage("Sơ đồ & chọn phòng") {BackColor=Color.White};
        bookingTabs.TabPages.Add(mapTab);
        var mapCard=new Panel {Dock=DockStyle.Fill,BackColor=Color.White};
        availableRoomMap=new Panel {Dock=DockStyle.Fill,AutoScroll=true,BackColor=Color.White};
        availableRoomMap.Resize+=(_,_)=>LayoutAvailableRoomColumns();
        availableRoomTip=new ToolTip();
        var roomTip=availableRoomTip;
        availableRoomMap.Disposed+=(_,_)=>roomTip.Dispose();
        mapCard.Controls.Add(availableRoomMap);
        var mapHeader=new Panel {Dock=DockStyle.Top,Height=70,BackColor=Color.White};
        mapHeader.Controls.Add(new Label {Text="SƠ ĐỒ PHÒNG",Dock=DockStyle.Top,Height=31,
            Padding=new Padding(12,7,0,0),Font=AppTheme.Bold,ForeColor=AppTheme.Navy});
        var legend=new FlowLayoutPanel {Dock=DockStyle.Bottom,Height=34,WrapContents=false,AutoScroll=true,
            Padding=new Padding(8,2,0,0),BackColor=Color.White};
        foreach(var state in new[]{RoomStatus.Trong,RoomStatus.DangO,RoomStatus.DangDon,
            RoomStatus.BaoTri,RoomStatus.DaDat})
            legend.Controls.Add(new Label {Text=state==RoomStatus.DaDat?"Có lịch đặt":Ui.Status(state),AutoSize=true,Height=24,
                BackColor=AppTheme.BookingRoomColor(state),ForeColor=Color.White,
                TextAlign=ContentAlignment.MiddleCenter,Padding=new Padding(8,4,8,4),
                Margin=new Padding(4,0,4,0),Font=AppTheme.Small});
        mapHeader.Controls.Add(legend);
        mapCard.Controls.Add(mapHeader);
        mapTab.Controls.Add(mapCard);
        layout.Controls.Add(bookingTabs, 0, 3);

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
        availableCheckout=new Button {Text="THANH TOÁN",Dock=DockStyle.Left,Width=285,
            Image=UiIcons.Create("receipt",AppTheme.Blue,20),
            ImageAlign=ContentAlignment.MiddleLeft,TextImageRelation=TextImageRelation.ImageBeforeText};
        AppTheme.Button(availableCheckout);
        availableCheckout.Disposed+=(_,_)=>availableCheckout.Image?.Dispose();
        availableCheckout.Click+=async (_,_)=>await Run(async()=>
        {
            var occupied=data.Stays.Where(stay=>stay.Status==StayStatus.Occupied).ToArray();
            var stay=occupied.Length==1?occupied[0]:SelectStay("Chọn phòng thanh toán");
            if(stay is not null)await ShowCheckout(stay);
        });
        footer.Controls.Add(availableCheckout);
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

        functionPage.Controls.Add(layout);
        functionPage.Controls.Add(heading);
        ActivatePage(functionPage);
        UpdateNavigationTitle();
        RefreshAvailableRooms();
        RefreshBookingDetail();
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
        if (availableRoomMap is null || availableRoomMap.IsDisposed ||
            availableArrival is null || availableDays is null ||
            availableSearch is null || availableType is null ||
            availableCount is null || availableSelect is null) return;

        var from = availableReserve ? availableArrival.Value : ServerNow;
        var until = from.AddDays((int)availableDays.Value);
        var search = availableSearch.Text.Trim();
        var type = availableType.SelectedItem as string ?? "Tất cả";
        var rooms = data.Rooms
            .Where(room => availableReserve ? room.Status != RoomStatus.BaoTri : room.Status == RoomStatus.Trong)
            .Where(room => type == "Tất cả" || room.Type == type)
            .Where(room => search.Length == 0 || room.Number.Contains(search, StringComparison.CurrentCultureIgnoreCase)
                || room.Code.Contains(search,StringComparison.CurrentCultureIgnoreCase))
            .Where(room => !data.Stays.Any(stay => stay.RoomId == room.Id &&
                (stay.CheckIn ?? stay.Arrival) < until && stay.Departure > from))
            .OrderBy(room => room.Number).ToArray();
        availableRoomCount=rooms.Length;
        selectedBookingRooms.IntersectWith(rooms.Select(room => room.Id));
        if(availableRoomMap is {IsDisposed:false} map)
        {
            map.SuspendLayout();
            availableRoomTip?.RemoveAll();
            Ui.Clear(map);
            availableRoomColumns=new Panel {BackColor=Color.White};
            map.Controls.Add(availableRoomColumns);
            var groups=new[] {"Đơn","Đôi","VIP"};
            var columns=new Panel[3];
            for(var i=0;i<3;i++)
            {
                var column=new Panel {BackColor=Color.White,Tag=i};
                column.Controls.Add(new Label {Name="groupTitle",Font=AppTheme.Bold,
                    ForeColor=AppTheme.Navy,Location=new Point(8,8),Height=28});
                availableRoomColumns.Controls.Add(column);
                columns[i]=column;
            }
            var eligible=rooms.Select(room=>room.Id).ToHashSet();
            foreach(var room in data.Rooms
                .Where(room=>search.Length==0 || room.Number.Contains(search,StringComparison.CurrentCultureIgnoreCase)
                    || room.Code.Contains(search,StringComparison.CurrentCultureIgnoreCase))
                .Where(room=>type=="Tất cả" || room.Type==type)
                .OrderBy(room=>room.Number))
            {
                    var canBook=eligible.Contains(room.Id);
                    var canPay=room.Status==RoomStatus.DangO &&
                        (FunctionPolicy.Can(user,"room.checkout") || FunctionPolicy.Can(user,"invoice.create"));
                    var chosen=selectedBookingRooms.Contains(room.Id);
                    var blockingStay=data.Stays.FirstOrDefault(stay=>stay.RoomId==room.Id &&
                        (stay.CheckIn??stay.Arrival)<until && stay.Departure>from);
                    var hasBookingConflict=!canBook && blockingStay is {Status:StayStatus.Reserved};
                    var tile=new RoomTile {Tag=room,ShowBookingRates=true,
                        SelectedForBooking=chosen,BookingUnavailable=!canBook,BookingConflict=hasBookingConflict,
                        BookingCheckout=canPay,
                        Reservations=data.Stays.Count(stay=>stay.RoomId==room.Id && stay.Status==StayStatus.Reserved),
                        Size=new Size(172,108),Margin=new Padding(6),
                        AccessibleName=$"Phòng {room.Number}, {(hasBookingConflict?"có lịch đặt":Ui.Status(room.Status))}, {(canPay?"bấm để thanh toán":canBook?"nhấp đúp để nhập thông tin khách, nhấp một lần để chọn nhiều phòng":"bấm để xem lý do không khả dụng")}"};
                    tile.Cursor=Cursors.Hand;
                    var unavailableReason=blockingStay is not null
                        ?$"Phòng {room.Number} đã có lịch từ {blockingStay.Arrival:dd/MM/yyyy HH:mm} đến {blockingStay.Departure:dd/MM/yyyy HH:mm}. Hãy chọn thời gian khác."
                        :$"Phòng {room.Number} hiện {Ui.Status(room.Status).ToLowerInvariant()}, chưa thể nhận phòng ngay. Hãy chọn thời gian hoặc phòng khác.";
                    availableRoomTip?.SetToolTip(tile,$"Phòng {room.Number} · Mã {room.Code}\n{room.Type} · Giá {room.Rate:N0} đ/ngày · Cọc gợi ý {room.Deposit:N0} đ\n{(canPay?"Bấm để xem bill và thanh toán":canBook?"Nhấp đúp để nhập khách ngay; nhấp một lần để chọn nhiều phòng":unavailableReason)}");
                    tile.MouseDoubleClick+=async (_,_)=>
                    {
                        if(!canBook)return;
                        var selectedArrival=availableReserve?availableArrival?.Value??ServerNow:ServerNow;
                        var selectedDays=(int)(availableDays?.Value??1);
                        await Run(async()=>await ShowMultipleBooking([room],availableReserve,selectedArrival,selectedDays));
                        selectedBookingRooms.Clear();
                        RefreshAvailableRooms();
                        RefreshBookingDetail();
                    };
                    tile.Click+=async (_,_)=>
                    {
                        if(canPay)await Run(async()=>
                        {
                            var stay=data.Stays.SingleOrDefault(item=>item.RoomId==room.Id && item.Status==StayStatus.Occupied)
                                ??throw new BusinessException($"Không tìm thấy lượt lưu trú đang ở của phòng {room.Number}. Hãy làm mới dữ liệu.");
                            await ShowCheckout(stay);
                        });
                        else if(canBook)
                        {
                            ToggleAvailableRoom(room.Id);
                        }
                        else MessageBox.Show(this,unavailableReason,"Phòng chưa khả dụng",MessageBoxButtons.OK,MessageBoxIcon.Information);
                    };
                    var group=room.Type=="Đơn"?0:room.Type=="Đôi"?1:2;
                    columns[group].Controls.Add(tile);
            }
            for(var i=0;i<3;i++)
            {
                var count=columns[i].Controls.OfType<RoomTile>().Count();
                if(columns[i].Controls["groupTitle"] is Label title)
                    title.Text=$"PHÒNG {groups[i].ToUpperInvariant()} ({count})";
            }
            LayoutAvailableRoomColumns();
            map.ResumeLayout();
        }
        availableCount.Text = availableReserve
            ? $"{availableRoomCount} phòng còn chỗ từ {from:dd/MM/yyyy HH:mm} · Đã chọn {selectedBookingRooms.Count} phòng"
            : $"{availableRoomCount} phòng sẵn sàng · Đã chọn {selectedBookingRooms.Count} phòng";
        UpdateAvailableSelection();
        if(availableCheckout is {IsDisposed:false} checkout)
        {
            var occupiedCount=data.Stays.Count(stay=>stay.Status==StayStatus.Occupied);
            checkout.Enabled=occupiedCount>0 &&
                (FunctionPolicy.Can(user,"room.checkout") || FunctionPolicy.Can(user,"invoice.create"));
            checkout.Text=$"THANH TOÁN ({occupiedCount} PHÒNG)";
        }
    }

    private void LayoutAvailableRoomColumns()
    {
        if(availableRoomMap is not {IsDisposed:false} map ||
            availableRoomColumns is not {IsDisposed:false} surface)return;
        const int gap=12;
        const int tileGap=8;
        const int tileHeight=108;
        const int rowHeight=116;
        var width=Math.Max(660,map.ClientSize.Width-18);
        var columnWidth=(width-gap*2)/3;
        var maxHeight=0;
        foreach(var column in surface.Controls.OfType<Panel>())
        {
            var index=(int)(column.Tag??0);
            var tiles=column.Controls.OfType<RoomTile>()
                .OrderBy(tile=>(tile.Tag as Room)?.Number).ToArray();
            var perRow=columnWidth>=360?2:1;
            var tileWidth=(columnWidth-16-(perRow-1)*tileGap)/perRow;
            for(var i=0;i<tiles.Length;i++)
                tiles[i].Bounds=new Rectangle(8+(i%perRow)*(tileWidth+tileGap),
                    44+(i/perRow)*rowHeight,tileWidth,tileHeight);
            var rows=(tiles.Length+perRow-1)/perRow;
            var height=44+Math.Max(1,rows)*rowHeight;
            column.Bounds=new Rectangle(index*(columnWidth+gap),0,columnWidth,height);
            if(column.Controls["groupTitle"] is Label title)title.Width=columnWidth-16;
            maxHeight=Math.Max(maxHeight,height);
        }
        surface.Bounds=new Rectangle(8,8,width,maxHeight);
    }

    private void UpdateAvailableSelection()
    {
        if (availableSelect is null || availableSelect.IsDisposed || availableRoomMap is null ||
            availableRoomMap.IsDisposed) return;
        availableSelect.Enabled = selectedBookingRooms.Count > 0;
        availableSelect.Text = selectedBookingRooms.Count > 0
            ? $"TIẾP TỤC · {selectedBookingRooms.Count} PHÒNG" : "CHỌN PHÒNG ĐỂ TIẾP TỤC";
        if (availableCount is not null && !availableCount.IsDisposed)
        {
            availableCount.Text = availableReserve
                ? $"{availableRoomCount} phòng còn chỗ · Đã chọn {selectedBookingRooms.Count} phòng"
                : $"{availableRoomCount} phòng sẵn sàng · Đã chọn {selectedBookingRooms.Count} phòng";
        }
    }

    private async Task SelectAvailableRoom()
    {
        if (availableRoomMap is null ||
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
        RefreshBookingDetail();
    }

    private void ToggleAvailableRoom(int roomId)
    {
        if(selectedBookingRooms.Contains(roomId))selectedBookingRooms.Remove(roomId);
        else if(selectedBookingRooms.Count<20)selectedBookingRooms.Add(roomId);
        else {Ui.Error(this,new BusinessException("Chọn tối đa 20 phòng mỗi lần."));return;}
        if(availableRoomMap is {IsDisposed:false} map)
            foreach(var tile in (availableRoomColumns?.Controls.OfType<Panel>() ?? [])
                .SelectMany(column=>column.Controls.OfType<RoomTile>())
                .Where(tile=>tile.Tag is Room room && room.Id==roomId))
            {
                tile.SelectedForBooking=selectedBookingRooms.Contains(roomId);
                tile.AccessibleName=$"Phòng {((Room)tile.Tag!).Number}, {(tile.SelectedForBooking?"đã chọn":"có thể chọn")}";
                tile.Invalidate();
            }
        UpdateAvailableSelection();
    }

}
