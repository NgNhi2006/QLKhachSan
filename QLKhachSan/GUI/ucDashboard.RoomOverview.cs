using QLKhachSan.BLL;
using QLKhachSan.DTO;

namespace QLKhachSan.GUI;

public partial class ucDashboard
{
    private Action? roomOverviewRefresh;

    private Control BuildRoomOverview()
    {
        var root=new TableLayoutPanel {Dock=DockStyle.Fill,BackColor=AppTheme.Canvas,ColumnCount=1,RowCount=3,
            Padding=new Padding(20,14,20,18)};
        root.RowStyles.Add(new RowStyle(SizeType.Absolute,66));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute,112));
        root.RowStyles.Add(new RowStyle(SizeType.Percent,100));

        var filters=new FlowLayoutPanel {Dock=DockStyle.Fill,BackColor=Color.White,WrapContents=false,
            AutoScroll=true,Padding=new Padding(14,12,10,8),Margin=new Padding(0,0,0,10)};
        var search=new TextBox {Width=260,Height=34,Font=AppTheme.Body,PlaceholderText="Tìm số phòng...",
            Margin=new Padding(0,3,16,0),BorderStyle=BorderStyle.FixedSingle};
        var type=new ComboBox {Width=180,DropDownStyle=ComboBoxStyle.DropDownList,Font=AppTheme.Body,
            Margin=new Padding(0,3,16,0)};
        var status=new ComboBox {Width=190,DropDownStyle=ComboBoxStyle.DropDownList,Font=AppTheme.Body,
            Margin=new Padding(0,3,16,0)};
        status.Items.AddRange(["Tất cả trạng thái","Trống","Đã đặt","Đang ở","Đang dọn","Bảo trì"]);
        status.SelectedIndex=0;
        filters.Controls.Add(new Label {Text="Tìm phòng",AutoSize=true,Font=AppTheme.Bold,ForeColor=AppTheme.Navy,
            Margin=new Padding(0,8,10,0)});
        filters.Controls.Add(search);
        filters.Controls.Add(new Label {Text="Loại phòng",AutoSize=true,Font=AppTheme.Bold,ForeColor=AppTheme.Navy,
            Margin=new Padding(0,8,10,0)});
        filters.Controls.Add(type);
        filters.Controls.Add(new Label {Text="Trạng thái",AutoSize=true,Font=AppTheme.Bold,ForeColor=AppTheme.Navy,
            Margin=new Padding(0,8,10,0)});
        filters.Controls.Add(status);
        root.Controls.Add(filters,0,0);

        var stats=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=4,RowCount=1,BackColor=AppTheme.Canvas,
            Margin=new Padding(0,0,0,12)};
        for(var i=0;i<4;i++)stats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,25));
        var statLabels=new List<Label>();
        var statDefinitions=new[]
        {
            ("Trống",AppTheme.Teal),("Đang ở",AppTheme.Blue),
            ("Đang dọn",AppTheme.Amber),("Bảo trì",AppTheme.Danger)
        };
        for(var i=0;i<statDefinitions.Length;i++)
        {
            var (caption,color)=statDefinitions[i];
            var card=new DashboardMetricCard {Dock=DockStyle.Fill,BackColor=Color.White,Accent=color,
                Margin=new Padding(i==0?0:5,0,i==3?0:5,0)};
            card.Controls.Add(new Label {Text=caption,Font=AppTheme.Bold,ForeColor=AppTheme.Navy,
                Location=new Point(16,13),AutoSize=true});
            var value=new Label {Text="0",Font=AppTheme.TextFont(24F,FontStyle.Bold),ForeColor=color,
                Location=new Point(14,38),AutoSize=true};
            card.Controls.Add(value);statLabels.Add(value);stats.Controls.Add(card,i,0);
        }
        root.Controls.Add(stats,0,1);

        var body=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=2,RowCount=1,BackColor=AppTheme.Canvas};
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,76));
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,24));
        var roomsCard=new Panel {Dock=DockStyle.Fill,BackColor=Color.White,Margin=new Padding(0,0,12,0)};
        var roomsTitle=new Label {Text="Danh sách phòng",Dock=DockStyle.Top,Height=52,
            Font=AppTheme.TextFont(17F,FontStyle.Bold),ForeColor=AppTheme.Navy,
            Padding=new Padding(16,11,0,0)};
        var roomRows=new Panel {Dock=DockStyle.Fill,AutoScroll=true,BackColor=Color.White};
        roomsCard.Controls.Add(roomRows);roomsCard.Controls.Add(roomsTitle);
        body.Controls.Add(roomsCard,0,0);

        var arrivalCard=new Panel {Dock=DockStyle.Fill,BackColor=Color.White,Margin=Padding.Empty};
        var arrivalTitle=new Label {Text="Khách sắp đến hôm nay",Dock=DockStyle.Top,Height=52,
            Font=AppTheme.TextFont(13F,FontStyle.Bold),ForeColor=AppTheme.Navy,
            Padding=new Padding(12,14,0,0)};
        var arrivalRows=new FlowLayoutPanel {Dock=DockStyle.Fill,FlowDirection=FlowDirection.TopDown,
            WrapContents=false,AutoScroll=true,BackColor=Color.White,Padding=new Padding(8)};
        arrivalCard.Controls.Add(arrivalRows);arrivalCard.Controls.Add(arrivalTitle);
        body.Controls.Add(arrivalCard,1,0);
        root.Controls.Add(body,0,2);

        var rendering=false;
        void RefreshOverview()
        {
            if(root.IsDisposed || rendering)return;
            rendering=true;
            try
            {
                var selectedType=type.SelectedItem?.ToString()??"Tất cả loại phòng";
                var roomTypes=data.Rooms.Select(r=>r.Type).Distinct().OrderBy(x=>x).ToArray();
                var options=new[]{"Tất cả loại phòng"}.Concat(roomTypes).ToArray();
                if(type.Items.Count!=options.Length || !type.Items.Cast<string>().SequenceEqual(options))
                {
                    type.Items.Clear();type.Items.AddRange(options);
                    type.SelectedItem=options.Contains(selectedType)?selectedType:options[0];
                }
                var statuses=new[]{RoomStatus.Trong,RoomStatus.DangO,RoomStatus.DangDon,RoomStatus.BaoTri};
                for(var i=0;i<statuses.Length;i++)
                    statLabels[i].Text=data.Rooms.Count(r=>r.Status==statuses[i]).ToString();

                var query=data.Rooms.AsEnumerable();
                if(!string.IsNullOrWhiteSpace(search.Text))
                    query=query.Where(r=>r.Number.Contains(search.Text.Trim(),StringComparison.OrdinalIgnoreCase));
                if(type.SelectedIndex>0)query=query.Where(r=>r.Type==type.SelectedItem?.ToString());
                if(status.SelectedIndex>0)
                {
                    var selected=new[]{RoomStatus.Trong,RoomStatus.DaDat,RoomStatus.DangO,
                        RoomStatus.DangDon,RoomStatus.BaoTri}[status.SelectedIndex-1];
                    query=query.Where(r=>r.Status==selected);
                }
                Ui.Clear(roomRows);
                var top=4;
                foreach(var floor in query.OrderBy(r=>r.Number).GroupBy(r=>
                    r.Number.Length>0 && char.IsDigit(r.Number[0])?$"Tầng {r.Number[0]}":"Khác"))
                {
                    var row=new Panel {Location=new Point(8,top),Width=Math.Max(360,roomRows.ClientSize.Width-22),
                        BackColor=Color.White};
                    var floorLabel=new Label {Text=floor.Key,Location=new Point(10,16),Size=new Size(86,50),
                        Font=AppTheme.Bold,ForeColor=AppTheme.Navy};
                    row.Controls.Add(floorLabel);
                    var tileWidth=132;
                    var columns=Math.Max(1,(row.Width-110)/tileWidth);
                    var index=0;
                    foreach(var room in floor)
                    {
                        var reservations=data.Stays.Count(s=>s.RoomId==room.Id && s.Status==StayStatus.Reserved);
                        var tile=new RoomTile {Tag=room,Reservations=reservations,
                            AccessibleName=$"Phòng {room.Number}, {Ui.Status(room.Status)}, {reservations} lịch đặt",
                            Size=new Size(124,86),Location=new Point(105+(index%columns)*tileWidth,5+(index/columns)*94)};
                        tile.Click+=async (_,_)=>
                        {
                            if(tile.Tag is not Room selected)return;
                            var code=selected.Status switch
                            {
                                RoomStatus.Trong=>"room.walkin",RoomStatus.DangO=>"room.checkout",
                                RoomStatus.DangDon=>"room.clean",RoomStatus.BaoTri=>"room.maintain",_=>"room.map"
                            };
                            if(FunctionPolicy.Can(user,code))await Run(()=>RoomAction(selected));
                        };
                        row.Controls.Add(tile);index++;
                    }
                    row.Height=Math.Max(96,((index+columns-1)/columns)*94+8);
                    row.Paint+=(_,e)=>
                    {
                        using var border=new Pen(AppTheme.Border);
                        e.Graphics.DrawLine(border,0,row.Height-1,row.Width,row.Height-1);
                    };
                    roomRows.Controls.Add(row);top+=row.Height;
                }
                roomRows.AutoScrollMinSize=new Size(0,top+8);

                Ui.Clear(arrivalRows);
                var arrivals=data.Stays.Where(s=>s.Status==StayStatus.Reserved && s.Arrival.Date==ServerNow.Date)
                    .OrderBy(s=>s.Arrival).Take(8).ToArray();
                if(arrivals.Length==0)
                    arrivalRows.Controls.Add(new Label {Text="Chưa có khách đến hôm nay.",AutoSize=true,
                        Font=AppTheme.Body,ForeColor=AppTheme.Muted,Margin=new Padding(8,12,0,0)});
                foreach(var stay in arrivals)
                {
                    var room=data.Rooms.FirstOrDefault(r=>r.Id==stay.RoomId)?.Number??"?";
                    var row=new Panel {Width=Math.Max(165,arrivalRows.ClientSize.Width-24),Height=76,
                        BackColor=Color.White,Margin=new Padding(0,0,0,5)};
                    row.Controls.Add(new Label {Text=stay.Arrival.ToString("HH:mm"),Location=new Point(7,7),
                        AutoSize=true,Font=AppTheme.Bold,ForeColor=AppTheme.Amber});
                    row.Controls.Add(new Label {Text=stay.Guest,Location=new Point(58,6),
                        Width=Math.Max(80,row.Width-65),Height=24,AutoEllipsis=true,
                        Font=AppTheme.Bold,ForeColor=AppTheme.Navy});
                    row.Controls.Add(new Label {Text=$"Phòng {room}",Location=new Point(58,33),
                        AutoSize=true,Font=AppTheme.Small,ForeColor=AppTheme.Muted});
                    row.Paint+=(_,e)=>
                    {
                        using var line=new Pen(AppTheme.Border);
                        e.Graphics.DrawLine(line,0,row.Height-1,row.Width,row.Height-1);
                    };
                    arrivalRows.Controls.Add(row);
                }
            }
            finally{rendering=false;}
        }
        search.TextChanged+=(_,_)=>RefreshOverview();
        type.SelectedIndexChanged+=(_,_)=>RefreshOverview();
        status.SelectedIndexChanged+=(_,_)=>RefreshOverview();
        roomRows.Resize+=(_,_)=>RefreshOverview();
        arrivalRows.Resize+=(_,_)=>RefreshOverview();
        roomOverviewRefresh=RefreshOverview;
        root.Disposed+=(_,_)=>{if(roomOverviewRefresh==RefreshOverview)roomOverviewRefresh=null;};
        RefreshOverview();
        return root;
    }
}
