using QLKhachSan.DAL;
using QLKhachSan.DTO;

namespace QLKhachSan.GUI;

public partial class ucDashboard
{
    private async Task ShowMultipleRoomPicker(bool reserve,DateTime arrival,int days,int? initialRoomId)
    {
        var from=reserve?arrival:ServerNow;
        var until=from.AddDays(days);
        var available=data.Rooms
            .Where(room=>reserve?room.Status!=RoomStatus.BaoTri:room.Status==RoomStatus.Trong)
            .Where(room=>!data.Stays.Any(stay=>stay.RoomId==room.Id &&
                (stay.CheckIn??stay.Arrival)<until && stay.Departure>from))
            .OrderBy(room=>room.Number).ToList();
        if(available.Count==0)throw new BusinessException("Không còn phòng trong thời gian đã chọn.");
        using var form=new Form {Text=reserve?"Chọn nhiều phòng để đặt":"Chọn nhiều phòng để nhận",
            Size=new Size(1150,740),MinimumSize=new Size(900,600),
            StartPosition=FormStartPosition.CenterParent,Font=AppTheme.Body,BackColor=AppTheme.Canvas};
        var root=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=1,RowCount=3};
        root.RowStyles.Add(new RowStyle(SizeType.Absolute,96));
        root.RowStyles.Add(new RowStyle(SizeType.Percent,100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute,76));form.Controls.Add(root);
        var heading=new Panel {Dock=DockStyle.Fill,BackColor=Color.White,Padding=new Padding(22,12,22,8)};
        heading.Controls.Add(new Label {Text=reserve?"Chọn danh sách phòng đặt trước":"Chọn danh sách phòng nhận ngay",
            Dock=DockStyle.Top,Height=40,Font=AppTheme.Title,ForeColor=AppTheme.Ink});
        heading.Controls.Add(new Label {Text=$"Từ {from:dd/MM/yyyy HH:mm} · {days} ngày · Chọn 1–20 phòng cho cùng một khách",
            Dock=DockStyle.Bottom,Height=24,ForeColor=AppTheme.Muted});root.Controls.Add(heading,0,0);
        var body=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=3,Padding=new Padding(18,16,18,12)};
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,45));
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,175));
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,55));root.Controls.Add(body,0,1);
        var availablePanel=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=1,RowCount=3,
            BackColor=Color.White,Padding=new Padding(15),Margin=new Padding(0,0,8,0)};
        availablePanel.RowStyles.Add(new RowStyle(SizeType.Absolute,34));
        availablePanel.RowStyles.Add(new RowStyle(SizeType.Absolute,52));
        availablePanel.RowStyles.Add(new RowStyle(SizeType.Percent,100));body.Controls.Add(availablePanel,0,0);
        var availableTitle=new Label {Text="PHÒNG CÒN CHỖ",Dock=DockStyle.Fill,Font=AppTheme.Bold,ForeColor=AppTheme.Blue};
        availablePanel.Controls.Add(availableTitle,0,0);
        var filters=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=2};
        filters.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,63));
        filters.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,37));
        var search=Ui.Text(40);search.PlaceholderText="Tìm số phòng, loại phòng...";
        var floors=new[]{"Tất cả tầng"}.Concat(available.Select(r=>r.Number.Length>=3?$"Tầng {r.Number[0]}":"Khác")
            .Distinct().OrderBy(x=>x)).ToArray();
        var floor=Ui.Combo(floors);
        filters.Controls.Add(search,0,0);filters.Controls.Add(floor,1,0);availablePanel.Controls.Add(filters,0,1);
        var availableList=new ListBox {Dock=DockStyle.Fill,IntegralHeight=false,Font=AppTheme.Body,
            HorizontalScrollbar=true};availablePanel.Controls.Add(availableList,0,2);
        var moves=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=1,RowCount=5,
            Padding=new Padding(9,60,9,40)};
        for(var i=0;i<5;i++)moves.RowStyles.Add(new RowStyle(SizeType.Percent,20));
        body.Controls.Add(moves,1,0);
        Button MoveButton(string label,int row)
        {
            var button=new Button {Text=label,Dock=DockStyle.Fill,Margin=new Padding(0,5,0,5)};
            AppTheme.Button(button);moves.Controls.Add(button,0,row);return button;
        }
        var add=MoveButton("THÊM  →",0);
        var addVisible=MoveButton("THÊM KẾT QUẢ  →",1);
        var remove=MoveButton("←  BỎ PHÒNG",3);
        var clear=MoveButton("BỎ TẤT CẢ",4);
        var selectedPanel=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=1,RowCount=3,
            BackColor=Color.White,Padding=new Padding(15),Margin=new Padding(8,0,0,0)};
        selectedPanel.RowStyles.Add(new RowStyle(SizeType.Absolute,34));
        selectedPanel.RowStyles.Add(new RowStyle(SizeType.Percent,100));
        selectedPanel.RowStyles.Add(new RowStyle(SizeType.Absolute,74));body.Controls.Add(selectedPanel,2,0);
        var selectedTitle=new Label {Dock=DockStyle.Fill,Font=AppTheme.Bold,ForeColor=AppTheme.Teal};
        selectedPanel.Controls.Add(selectedTitle,0,0);
        var selectedList=new ListBox {Dock=DockStyle.Fill,IntegralHeight=false,Font=AppTheme.Body,
            HorizontalScrollbar=true};selectedPanel.Controls.Add(selectedList,0,1);
        var summary=new Label {Dock=DockStyle.Fill,BackColor=Color.FromArgb(239,245,251),
            Font=AppTheme.Bold,ForeColor=AppTheme.Ink,Padding=new Padding(12,10,8,7)};
        selectedPanel.Controls.Add(summary,0,2);
        var chosen=new List<Room>();
        if(initialRoomId is { } initial && available.FirstOrDefault(x=>x.Id==initial) is { } first)chosen.Add(first);
        void RefreshChosen()
        {
            var previous=(selectedList.SelectedItem as Room)?.Id;
            selectedList.Items.Clear();
            selectedList.Items.AddRange(chosen.OrderBy(x=>x.Number).Cast<object>().ToArray());
            if(previous is { } id && selectedList.Items.Cast<Room>().FirstOrDefault(x=>x.Id==id) is { } old)
                selectedList.SelectedItem=old;
            selectedTitle.Text=$"ĐÃ CHỌN  ({chosen.Count}/20)";
            summary.Text=$"Tổng giá dự kiến: {chosen.Sum(x=>x.Rate)*days:N0} đ / {days} ngày\n"+
                $"Phòng: {(chosen.Count==0?"Chưa chọn":string.Join(", ",chosen.OrderBy(x=>x.Number).Select(x=>x.Number)))}";
        }
        void RefreshAvailable()
        {
            var query=search.Text.Trim();
            var selectedFloor=(string?)floor.SelectedItem??"Tất cả tầng";
            var rows=available.Where(x=>!chosen.Any(c=>c.Id==x.Id))
                .Where(x=>query.Length==0 || x.Number.Contains(query,StringComparison.CurrentCultureIgnoreCase) ||
                    x.Type.Contains(query,StringComparison.CurrentCultureIgnoreCase))
                .Where(x=>selectedFloor=="Tất cả tầng" ||
                    (x.Number.Length>=3?$"Tầng {x.Number[0]}":"Khác")==selectedFloor).ToArray();
            availableList.Items.Clear();availableList.Items.AddRange(rows.Cast<object>().ToArray());
            availableTitle.Text=$"PHÒNG CÒN CHỖ  ({rows.Length})";
        }
        void AddRoom(Room? room)
        {
            if(room is null)return;
            if(chosen.Count>=20){Ui.Error(form,new BusinessException("Chỉ chọn tối đa 20 phòng mỗi lần."));return;}
            if(chosen.All(x=>x.Id!=room.Id))chosen.Add(room);
            RefreshChosen();RefreshAvailable();
        }
        void RemoveRoom(Room? room)
        {
            if(room is null)return;
            chosen.RemoveAll(x=>x.Id==room.Id);RefreshChosen();RefreshAvailable();
        }
        add.Click+=(_,_)=>AddRoom(availableList.SelectedItem as Room);
        availableList.DoubleClick+=(_,_)=>AddRoom(availableList.SelectedItem as Room);
        addVisible.Click+=(_,_)=>
        {
            foreach(var room in availableList.Items.Cast<Room>().Take(20-chosen.Count).ToArray())
                if(chosen.All(x=>x.Id!=room.Id))chosen.Add(room);
            RefreshChosen();RefreshAvailable();
        };
        remove.Click+=(_,_)=>RemoveRoom(selectedList.SelectedItem as Room);
        selectedList.DoubleClick+=(_,_)=>RemoveRoom(selectedList.SelectedItem as Room);
        clear.Click+=(_,_)=>{chosen.Clear();RefreshChosen();RefreshAvailable();};
        search.TextChanged+=(_,_)=>RefreshAvailable();floor.SelectedIndexChanged+=(_,_)=>RefreshAvailable();
        RefreshChosen();RefreshAvailable();
        var footer=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=2,Padding=new Padding(18,12,18,12),
            BackColor=Color.White};
        footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,36));
        footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,64));root.Controls.Add(footer,0,2);
        var cancel=new Button {Text="QUAY LẠI",Dock=DockStyle.Fill,Margin=new Padding(0,0,7,0)};
        var next=new Button {Text="TIẾP TỤC VỚI CÁC PHÒNG ĐÃ CHỌN",Dock=DockStyle.Fill,Margin=new Padding(7,0,0,0)};
        AppTheme.Button(cancel);AppTheme.Button(next,true);footer.Controls.Add(cancel,0,0);footer.Controls.Add(next,1,0);
        cancel.Click+=(_,_)=>form.Close();
        next.Click+=async (_,_)=>
        {
            if(chosen.Count<1){Ui.Error(form,new BusinessException("Chọn ít nhất một phòng trong danh sách bên phải."));return;}
            next.Enabled=false;
            try
            {
                if(await ShowMultipleBooking(chosen.ToArray(),reserve,arrival,days))form.Close();
            }
            catch(Exception ex){Ui.Error(form,ex);}
            finally{if(!form.IsDisposed)next.Enabled=true;}
        };
        form.ShowDialog(this);
    }
}
