using QLKhachSan.DTO;
using QLKhachSan.DAL;
using QLKhachSan.BLL;

namespace QLKhachSan.GUI;

public partial class ucDashboard
{
    private sealed record StayChoice(Stay Stay,string Label) {public override string ToString()=>Label;}
    private ComboBox StayCombo() => Ui.Combo(data.Stays.Where(s=>s.Status==StayStatus.Occupied).Select(s=>new StayChoice(s,$"P.{StayRoom(s)?.Number} — {s.Guest}")));
    private Stay? SelectStay(string title)
    {
        var occupied=data.Stays.Where(s=>s.Status==StayStatus.Occupied)
            .OrderBy(s=>StayRoom(s)?.Number).Select(s=>new StayChoice(s,$"P.{StayRoom(s)?.Number}  •  {s.Guest}  •  {s.Phone}"))
            .ToArray();
        if(occupied.Length==0)throw new BusinessException("Không có phòng đang ở.");
        using var dialog=new InputDialog(title,650,Math.Min(650,Screen.FromControl(this).WorkingArea.Height-40));
        var search=Ui.Text();search.PlaceholderText="Gõ số phòng, tên khách hoặc số điện thoại...";
        var list=new ListBox {Height=390,IntegralHeight=false,Font=new Font("Segoe UI",11)};
        void Filter()
        {
            var query=search.Text.Trim();
            list.BeginUpdate();list.Items.Clear();
            list.Items.AddRange(occupied.Where(x=>query.Length==0 || x.Label.Contains(query,StringComparison.CurrentCultureIgnoreCase)).Cast<object>().ToArray());
            list.EndUpdate();
            if(list.Items.Count>0)list.SelectedIndex=0;
        }
        search.TextChanged+=(_,_)=>Filter();
        search.KeyDown+=(_,e)=>
        {
            if(e.KeyCode==Keys.Down && list.Items.Count>0){list.Focus();list.SelectedIndex=0;e.Handled=true;}
        };
        dialog.Add("Tìm phòng đang ở",search);dialog.Add($"{occupied.Length} phòng đang ở",list);
        var select=dialog.Action(title.Contains("dịch vụ",StringComparison.OrdinalIgnoreCase)?"CHỌN LƯỢT Ở":"THANH TOÁN",()=>Task.CompletedTask);
        list.DoubleClick+=(_,_)=>select.PerformClick();
        Filter();dialog.Shown+=(_,_)=>search.Focus();
        return dialog.ShowDialog(this)==DialogResult.OK && list.SelectedItem is StayChoice item?item.Stay:null;
    }
    private async Task RoomAction(Room room)
    {
        var stay=RoomStay(room);
        switch(room.Status)
        {
            case RoomStatus.Trong: await ShowBooking(false,room);break;
            case RoomStatus.DangO:
                if(stay is null)throw new BusinessException("Thiếu lượt lưu trú của phòng. Hãy kiểm tra dữ liệu.");
                await ShowCheckout(stay);break;
            case RoomStatus.DaDat:
                throw new BusinessException("Trạng thái phòng thuộc phiên bản cũ. Cần nâng cấp dữ liệu và làm mới.");
            case RoomStatus.DangDon:
                if(Ui.Confirm(this,$"Phòng {room.Number} đã dọn xong?"))await Changed(()=>service.SetRoomStatusAsync(room,RoomStatus.Trong));break;
            case RoomStatus.BaoTri:
                if(Ui.Confirm(this,$"Kết thúc bảo trì phòng {room.Number}?"))await Changed(()=>service.SetRoomStatusAsync(room,RoomStatus.Trong));break;
        }
    }
    private async Task ShowBooking(bool reserve,Room? selected=null,DateTime? requestedArrival=null,int requestedDays=1)
    {
        if(selected is null)
        {
            ShowBookingScreen(reserve);
            return;
        }
        await ShowMultipleBooking(new[]{selected},reserve,requestedArrival??ServerNow,requestedDays);
    }
    private async Task ShowCancel(Stay stay)
    {
        var refund=await service.RefundQuoteAsync(stay);
        using var dialog=new InputDialog("Hủy đặt phòng / Hoàn cọc",540,350);
        dialog.Note($"Khách: {stay.Guest}\nPhòng: {StayRoom(stay)?.Number}\nHạn nhận: {stay.HoldUntil:dd/MM/yyyy HH:mm}\nSố tiền được hoàn: {refund:N0} đ\nCọc không hoàn: {stay.Deposit-refund:N0} đ");
        var method=Ui.Combo(new[]{"Tiền mặt","Chuyển khoản","Thẻ POS"});var reference=Ui.Text(100);dialog.Add("Phương thức hoàn",method);dialog.Add("Mã hoàn tiền (nếu có)",reference);
        var confirmed=new CheckBox {Text=refund>0?"Tôi xác nhận đã hoàn đủ tiền cọc":"Xác nhận hủy; không hoàn tiền cọc",AutoSize=true};dialog.Add("Xác nhận",confirmed);
        dialog.Action("HỦY ĐẶT PHÒNG",async()=>
        {
            if(!confirmed.Checked)throw new BusinessException("Cần xác nhận xử lý tiền cọc/giữ chỗ.");
            await Changed(()=>service.CancelAsync(stay,(string)method.SelectedItem!,refund,reference.Text));
        });
        dialog.ShowDialog(this);
    }
    private async void btnDoiPhong_Click(object? sender,EventArgs e)=>await Run(()=>
    {
        using var dialog=new InputDialog("Chuyển phòng",850,Math.Min(790,Screen.FromControl(this).WorkingArea.Height-30));
        var from=Ui.Combo(data.Stays.Where(s=>s.Status==StayStatus.Occupied).OrderBy(s=>StayRoom(s)?.Number)
            .Select(s=>new StayChoice(s,$"P.{StayRoom(s)?.Number} — {s.Guest} — {s.Phone}")));
        var available=data.Rooms.Where(r=>r.Status==RoomStatus.Trong).OrderBy(r=>r.Number).ToArray();
        if(from.Items.Count==0 || available.Length==0)throw new BusinessException("Cần phòng đang ở và phòng trống để chuyển.");
        var current=new Label {Height=106,BackColor=AppTheme.Canvas,ForeColor=AppTheme.Ink,Padding=new Padding(12,8,8,8)};
        var search=Ui.Text();search.PlaceholderText="Tìm theo số hoặc loại phòng...";
        var roomGroups=new TabControl {Height=190,Font=AppTheme.Bold};
        var groupLists=new ListBox[3];
        var groupNames=new[]{"Phòng đơn","Phòng đôi","VIP / loại khác"};
        for(var i=0;i<groupLists.Length;i++)
        {
            var page=new TabPage(groupNames[i]);roomGroups.TabPages.Add(page);
            var roomList=new ListBox {Dock=DockStyle.Fill,IntegralHeight=false,HorizontalScrollbar=true,Font=AppTheme.Body};
            page.Controls.Add(roomList);groupLists[i]=roomList;
        }
        var next=new Label {Height=68,BackColor=Color.FromArgb(232,240,255),ForeColor=AppTheme.Ink,Padding=new Padding(12,8,8,8)};
        dialog.Add("Lượt lưu trú cần chuyển",from);dialog.Add("Phòng hiện tại",current);
        dialog.Add("Tìm phòng trống",search);dialog.Add("Chọn phòng mới theo loại",roomGroups);dialog.Add("Thông tin phòng mới",next);
        dialog.Note("Giá phòng cũ áp dụng cho thời gian đã ở; sau khi chuyển tính theo giá phòng mới. Phòng cũ sẽ chuyển sang trạng thái đang dọn.");
        void Preview()
        {
            if(from.SelectedItem is not StayChoice choice)return;
            var old=StayRoom(choice.Stay);
            current.Text=$"P.{old?.Number} • {old?.Type}  |  {choice.Stay.Guest}\nGiá hiện tại: {old?.Rate:N0} đ/ngày\nHạn trả: {choice.Stay.Departure:dd/MM/yyyy HH:mm}";
            var selectedIndex=roomGroups.SelectedIndex;
            if(selectedIndex>=0 && selectedIndex<groupLists.Length && groupLists[selectedIndex].SelectedItem is Room room)
                next.Text=$"P.{room.Number} • {room.Type}\nGiá mới: {room.Rate:N0} đ/ngày  |  Chênh lệch: {room.Rate-(old?.Rate??0):+#,##0;-#,##0;0} đ/ngày";
            else next.Text="Không có phòng phù hợp.";
        }
        void FilterRooms()
        {
            var query=search.Text.Trim();
            for(var i=0;i<groupLists.Length;i++)
            {
                var list=groupLists[i];var previous=(list.SelectedItem as Room)?.Id;
                var matches=available.Where(r=>(i==0?r.Type=="Đơn":i==1?r.Type=="Đôi":r.Type!="Đơn" && r.Type!="Đôi")
                    && (query.Length==0 || r.Number.Contains(query,StringComparison.CurrentCultureIgnoreCase)
                        || r.Type.Contains(query,StringComparison.CurrentCultureIgnoreCase))).ToArray();
                list.BeginUpdate();list.Items.Clear();list.Items.AddRange(matches.Cast<object>().ToArray());
                list.HorizontalExtent=matches.Length==0?0:matches.Max(r=>TextRenderer.MeasureText(r.ToString(),list.Font).Width)+12;
                list.EndUpdate();var index=Array.FindIndex(matches,r=>r.Id==previous);
                if(matches.Length>0)list.SelectedIndex=index>=0?index:0;
                roomGroups.TabPages[i].Text=$"{groupNames[i]} ({matches.Length})";
            }
            Preview();
        }
        from.SelectedIndexChanged+=(_,_)=>Preview();roomGroups.SelectedIndexChanged+=(_,_)=>Preview();
        foreach(var list in groupLists)list.SelectedIndexChanged+=(_,_)=>Preview();
        search.TextChanged+=(_,_)=>FilterRooms();FilterRooms();
        dialog.Action("XÁC NHẬN CHUYỂN",async()=>
        {
            var selectedIndex=roomGroups.SelectedIndex;
            if(from.SelectedItem is not StayChoice choice || selectedIndex<0 || selectedIndex>=groupLists.Length
                || groupLists[selectedIndex].SelectedItem is not Room room)throw new BusinessException("Hãy chọn phòng mới.");
            if(choice.Stay.Departure<=ServerNow)throw new BusinessException("Lượt ở đã quá hạn trả. Hãy gia hạn trước khi chuyển phòng.");
            await Changed(()=>service.TransferAsync(choice.Stay,room));
        });dialog.ShowDialog(this);return Task.CompletedTask;
    });
    private async void btnGiaHan_Click(object? sender,EventArgs e)=>await Run(()=>
    {
        using var dialog=new Form {Text="Gia hạn lưu trú",Size=new Size(620,550),MinimumSize=new Size(560,530),StartPosition=FormStartPosition.CenterParent,Font=AppTheme.Body,BackColor=AppTheme.Canvas};
        var stay=StayCombo();var days=Ui.Number(30);
        if(stay.Items.Count==0)throw new BusinessException("Không có phòng đang ở.");
        var root=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=1,RowCount=3};
        root.RowStyles.Add(new RowStyle(SizeType.Absolute,82));root.RowStyles.Add(new RowStyle(SizeType.Percent,100));root.RowStyles.Add(new RowStyle(SizeType.Absolute,78));dialog.Controls.Add(root);
        var heading=new Panel {Dock=DockStyle.Fill,BackColor=Color.White,Padding=new Padding(28,14,20,8)};
        heading.Controls.Add(new Label {Text="Gia hạn lưu trú",Dock=DockStyle.Top,Height=34,Font=AppTheme.Title,ForeColor=AppTheme.Ink});
        heading.Controls.Add(new Label {Text="Kiểm tra hạn trả mới trước khi xác nhận",Dock=DockStyle.Bottom,Height=20,ForeColor=AppTheme.Muted});
        root.Controls.Add(heading,0,0);
        var content=new TableLayoutPanel {Dock=DockStyle.Fill,BackColor=Color.White,Margin=new Padding(18,16,18,8),Padding=new Padding(18,14,18,10),ColumnCount=1,RowCount=7};
        content.RowStyles.Add(new RowStyle(SizeType.Absolute,28));content.RowStyles.Add(new RowStyle(SizeType.Absolute,42));
        content.RowStyles.Add(new RowStyle(SizeType.Absolute,18));content.RowStyles.Add(new RowStyle(SizeType.Absolute,28));
        content.RowStyles.Add(new RowStyle(SizeType.Absolute,44));content.RowStyles.Add(new RowStyle(SizeType.Absolute,18));
        content.RowStyles.Add(new RowStyle(SizeType.Percent,100));root.Controls.Add(content,0,1);
        content.Controls.Add(new Label {Text="PHÒNG ĐANG Ở",Dock=DockStyle.Fill,Font=AppTheme.Bold,ForeColor=AppTheme.Muted},0,0);
        stay.Dock=DockStyle.Fill;content.Controls.Add(stay,0,1);
        content.Controls.Add(new Label {Text="SỐ NGÀY GIA HẠN",Dock=DockStyle.Fill,Font=AppTheme.Bold,ForeColor=AppTheme.Muted},0,3);
        var daysRow=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=2,RowCount=1};
        daysRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,150));daysRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
        days.Dock=DockStyle.Fill;days.Font=new Font("Segoe UI Semibold",12);daysRow.Controls.Add(days,0,0);
        daysRow.Controls.Add(new Label {Text="ngày  •  tối đa 30 ngày mỗi lần",Dock=DockStyle.Fill,ForeColor=AppTheme.Muted,TextAlign=ContentAlignment.MiddleLeft,Padding=new Padding(12,0,0,0)},1,0);
        content.Controls.Add(daysRow,0,4);
        var preview=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=2,RowCount=1};
        preview.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,50));preview.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,50));content.Controls.Add(preview,0,6);
        Label DateCard(string title,int column,bool highlighted)
        {
            var card=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=1,RowCount=2,Margin=new Padding(column==0?0:6,0,column==0?6:0,0),Padding=new Padding(14,11,10,8),BackColor=highlighted?Color.FromArgb(232,240,255):AppTheme.Canvas};
            card.RowStyles.Add(new RowStyle(SizeType.Absolute,26));card.RowStyles.Add(new RowStyle(SizeType.Percent,100));
            card.Controls.Add(new Label {Text=title,Dock=DockStyle.Fill,Font=AppTheme.Bold,ForeColor=highlighted?AppTheme.Blue:AppTheme.Muted},0,0);
            var value=new Label {Dock=DockStyle.Fill,Font=new Font("Segoe UI Semibold",13),ForeColor=AppTheme.Ink,TextAlign=ContentAlignment.MiddleLeft};
            card.Controls.Add(value,0,1);preview.Controls.Add(card,column,0);return value;
        }
        var currentDate=DateCard("HẠN TRẢ HIỆN TẠI",0,false);
        var nextDate=DateCard("HẠN TRẢ SAU GIA HẠN",1,true);
        void UpdatePreview()
        {
            if(stay.SelectedItem is not StayChoice choice)return;
            var now=ServerNow;
            currentDate.Text=choice.Stay.Departure.ToString("dd/MM/yyyy HH:mm");
            nextDate.Text=(choice.Stay.Departure>now?choice.Stay.Departure:now).AddDays((int)days.Value).ToString("dd/MM/yyyy HH:mm");
        }
        stay.SelectedIndexChanged+=(_,_)=>UpdatePreview();days.ValueChanged+=(_,_)=>UpdatePreview();UpdatePreview();
        var footer=new Panel {Dock=DockStyle.Fill,Padding=new Padding(22,12,22,16)};
        var save=new Button {Text="XÁC NHẬN GIA HẠN",Dock=DockStyle.Fill};AppTheme.Button(save,true);footer.Controls.Add(save);root.Controls.Add(footer,0,2);
        save.Click+=async (_,_)=>
        {
            if(stay.SelectedItem is not StayChoice choice)return;
            save.Enabled=false;
            try{await Changed(()=>service.ExtendAsync(choice.Stay,(int)days.Value));dialog.Close();}
            catch(Exception ex){Ui.Error(dialog,ex);save.Enabled=true;}
        };
        dialog.ShowDialog(this);return Task.CompletedTask;
    });
    private async void btnBaoTri_Click(object? sender,EventArgs e)=>await Run(()=>
    {
        using var dialog=new InputDialog("Bảo trì phòng",680,Math.Min(650,Screen.FromControl(this).WorkingArea.Height-30));
        var eligible=data.Rooms.Where(r=>r.Status is RoomStatus.Trong or RoomStatus.BaoTri).OrderBy(r=>r.Number).ToArray();
        if(eligible.Length==0)throw new BusinessException("Không có phòng có thể đổi trạng thái bảo trì.");
        var search=Ui.Text();search.PlaceholderText="Tìm theo số hoặc loại phòng...";
        var tabs=new TabControl {Height=260,Font=AppTheme.Bold};
        var lists=new ListBox[2];
        var titles=new[]{"Phòng trống","Đang bảo trì"};
        for(var i=0;i<lists.Length;i++)
        {
            var page=new TabPage(titles[i]);tabs.TabPages.Add(page);
            var list=new ListBox {Dock=DockStyle.Fill,IntegralHeight=false,HorizontalScrollbar=true,Font=AppTheme.Body};
            page.Controls.Add(list);lists[i]=list;
        }
        var details=new Label {Height=98,BackColor=AppTheme.Canvas,ForeColor=AppTheme.Ink,Padding=new Padding(14,10,10,8)};
        dialog.Add("Tìm phòng",search);dialog.Add("Chọn phòng",tabs);dialog.Add("Thông tin và thao tác",details);
        dialog.Note("Chỉ có thể bật bảo trì cho phòng trống không còn lịch đặt. Khi kết thúc bảo trì, phòng sẽ trở lại trạng thái trống.");
        Button? action=null;
        Room? SelectedRoom()
        {
            var index=tabs.SelectedIndex;
            return index>=0 && index<lists.Length?lists[index].SelectedItem as Room:null;
        }
        void Preview()
        {
            var room=SelectedRoom();
            if(room is null){details.Text="Không có phòng phù hợp trong mục này.";if(action is not null)action.Enabled=false;return;}
            var reserved=data.Stays.Any(s=>s.RoomId==room.Id && s.Status==StayStatus.Reserved);
            details.Text=$"P.{room.Number}  •  {room.Type}\nGiá: {room.Rate:N0} đ/ngày  •  Trạng thái: {Ui.Status(room.Status)}\n"+
                (room.Status==RoomStatus.Trong?reserved?"Phòng còn lịch đặt; cần xử lý trước khi bảo trì.":"Có thể đưa phòng vào bảo trì.":"Có thể kết thúc bảo trì để phòng sẵn sàng đón khách.");
            if(action is not null){action.Text=room.Status==RoomStatus.Trong?"BẮT ĐẦU BẢO TRÌ":"KẾT THÚC BẢO TRÌ";action.Enabled=room.Status==RoomStatus.BaoTri || !reserved;}
        }
        void FilterRooms()
        {
            var query=search.Text.Trim();
            for(var i=0;i<lists.Length;i++)
            {
                var list=lists[i];var previous=(list.SelectedItem as Room)?.Id;
                var status=i==0?RoomStatus.Trong:RoomStatus.BaoTri;
                var matches=eligible.Where(r=>r.Status==status && (query.Length==0 || r.Number.Contains(query,StringComparison.CurrentCultureIgnoreCase)
                    || r.Type.Contains(query,StringComparison.CurrentCultureIgnoreCase))).ToArray();
                list.BeginUpdate();list.Items.Clear();list.Items.AddRange(matches.Cast<object>().ToArray());
                list.HorizontalExtent=matches.Length==0?0:matches.Max(r=>TextRenderer.MeasureText(r.ToString(),list.Font).Width)+12;
                list.EndUpdate();var index=Array.FindIndex(matches,r=>r.Id==previous);
                if(matches.Length>0)list.SelectedIndex=index>=0?index:0;
                tabs.TabPages[i].Text=$"{titles[i]} ({matches.Length})";
            }
            Preview();
        }
        tabs.SelectedIndexChanged+=(_,_)=>Preview();foreach(var list in lists)list.SelectedIndexChanged+=(_,_)=>Preview();
        search.TextChanged+=(_,_)=>FilterRooms();
        action=dialog.Action("BẮT ĐẦU BẢO TRÌ",async()=>
        {
            var room=SelectedRoom()??throw new BusinessException("Hãy chọn phòng.");
            await Changed(()=>service.SetRoomStatusAsync(room,room.Status==RoomStatus.Trong?RoomStatus.BaoTri:RoomStatus.Trong));
        });
        FilterRooms();
        dialog.ShowDialog(this);return Task.CompletedTask;
    });
    private async void btnBaoDonXong_Click(object? sender,EventArgs e)=>await Run(async()=>
    {
        var rooms=data.Rooms.Where(r=>r.Status==RoomStatus.DangDon).ToList();
        if(rooms.Count==0)throw new BusinessException("Không có phòng đang dọn.");
        if(Ui.Confirm(this,$"Xác nhận đã dọn xong tất cả {rooms.Count} phòng?"))await Changed(()=>service.CleanAllAsync(rooms));
    });
    private async void btnGoiDichVu_Click(object? sender,EventArgs e)=>await Run(()=>
    {
        using var dialog=new InputDialog("Gọi dịch vụ",900,Math.Min(760,Screen.FromControl(this).WorkingArea.Height-30)) {MaximizeBox=false};
        var stay=StayCombo();if(stay.Items.Count==0)throw new BusinessException("Không có phòng đang ở.");
        var categories=Ui.Combo(new[]{"Tất cả"}.Concat(data.Menu.Select(s=>s.Category).Distinct()));
        var search=Ui.Text();search.PlaceholderText="Tìm tên dịch vụ...";
        var items=new ListBox {Dock=DockStyle.Fill,Font=AppTheme.Body,IntegralHeight=false,HorizontalScrollbar=true};
        var selected=new Label {Dock=DockStyle.Fill,Font=AppTheme.Body,ForeColor=AppTheme.Ink,AutoEllipsis=false,Padding=new Padding(4,5,4,0)};
        var quantity=Ui.Number(100);
        void FilterServices()
        {
            var previous=(items.SelectedItem as ServiceItem)?.Id;
            var query=search.Text.Trim();
            var matches=data.Menu.Where(s=>(categories.SelectedIndex==0 || s.Category==(string?)categories.SelectedItem)
                && (query.Length==0 || s.Name.Contains(query,StringComparison.CurrentCultureIgnoreCase))).ToArray();
            items.BeginUpdate();items.Items.Clear();items.Items.AddRange(matches.Cast<object>().ToArray());
            items.HorizontalExtent=matches.Length==0?0:matches.Max(s=>TextRenderer.MeasureText(s.ToString(),items.Font).Width)+12;
            items.EndUpdate();
            var index=Array.FindIndex(matches,s=>s.Id==previous);
            if(matches.Length>0)items.SelectedIndex=index>=0?index:0;
            selected.Text=items.SelectedItem is ServiceItem item?item.ToString():"Không tìm thấy dịch vụ phù hợp.";
        }
        categories.SelectedIndexChanged+=(_,_)=>FilterServices();search.TextChanged+=(_,_)=>FilterServices();
        items.SelectedIndexChanged+=(_,_)=>selected.Text=items.SelectedItem is ServiceItem item?item.ToString():"Không tìm thấy dịch vụ phù hợp.";
        var cart=new List<(ServiceItem Item,int Quantity)>();var list=new ListBox {Dock=DockStyle.Fill,Font=AppTheme.Body};var total=new Label {Dock=DockStyle.Fill,Font=AppTheme.Bold,ForeColor=AppTheme.Ink,TextAlign=ContentAlignment.MiddleRight};
        void UpdateCart(){list.DataSource=null;list.DataSource=cart.Select(x=>$"{x.Item.Name} × {x.Quantity} = {x.Item.Price*x.Quantity:N0} đ").ToList();total.Text=$"Tạm tính: {cart.Sum(x=>x.Item.Price*x.Quantity):N0} đ";}
        var layout=new TableLayoutPanel {Height=530,ColumnCount=2,RowCount=1,BackColor=Color.White};
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,50));layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,50));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent,100));
        var pick=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=1,RowCount=12,Padding=new Padding(4,0,18,0)};
        pick.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
        foreach(var height in new[]{34,24,36,24,36,24,34,130,63,24,36,48})pick.RowStyles.Add(new RowStyle(SizeType.Absolute,height));
        void Field(string caption,Control control,int labelRow)
        {
            pick.Controls.Add(new Label {Text=caption,Dock=DockStyle.Fill,Font=AppTheme.Bold,ForeColor=AppTheme.Muted},0,labelRow);
            control.Dock=DockStyle.Fill;pick.Controls.Add(control,0,labelRow+1);
        }
        pick.Controls.Add(new Label {Text="CHỌN DỊCH VỤ",Dock=DockStyle.Fill,Font=AppTheme.Title,ForeColor=AppTheme.Ink},0,0);
        Field("Phòng nhận",stay,1);Field("Danh mục",categories,3);Field("Tìm dịch vụ",search,5);
        pick.Controls.Add(items,0,7);pick.Controls.Add(selected,0,8);Field("Số lượng",quantity,9);
        var add=new Button {Text="+ THÊM VÀO YÊU CẦU",Dock=DockStyle.Fill};AppTheme.Button(add,true);
        add.Click+=(_,_)=> {if(items.SelectedItem is ServiceItem item && cart.Count<100){cart.Add((item,(int)quantity.Value));UpdateCart();}};
        pick.Controls.Add(add,0,11);layout.Controls.Add(pick,0,0);
        var order=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=1,RowCount=4,Padding=new Padding(18,0,4,0),BackColor=Color.FromArgb(246,249,255)};
        order.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
        order.RowStyles.Add(new RowStyle(SizeType.Absolute,42));order.RowStyles.Add(new RowStyle(SizeType.Percent,100));order.RowStyles.Add(new RowStyle(SizeType.Absolute,45));order.RowStyles.Add(new RowStyle(SizeType.Absolute,44));
        order.Controls.Add(new Label {Text="DANH SÁCH YÊU CẦU",Dock=DockStyle.Fill,Font=AppTheme.Bold,ForeColor=AppTheme.Ink,TextAlign=ContentAlignment.MiddleLeft},0,0);
        order.Controls.Add(list,0,1);
        var remove=new Button {Text="XÓA DÒNG ĐÃ CHỌN",Dock=DockStyle.Fill};AppTheme.Button(remove);
        remove.Click+=(_,_)=> {if(list.SelectedIndex>=0){cart.RemoveAt(list.SelectedIndex);UpdateCart();}};
        order.Controls.Add(remove,0,2);order.Controls.Add(total,0,3);layout.Controls.Add(order,1,0);
        dialog.Add("",layout);FilterServices();UpdateCart();
        dialog.Action("GỬI YÊU CẦU",async()=> {if(stay.SelectedItem is StayChoice item)await Changed(()=>service.AddServicesAsync(item.Stay,cart.Select(x=>new OrderInput(x.Item.Id,x.Quantity)).ToArray()));});
        dialog.ShowDialog(this);return Task.CompletedTask;
    });
}


