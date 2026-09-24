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
        using var dialog=new InputDialog(title,520,260);
        var combo=StayCombo();if(combo.Items.Count==0)throw new BusinessException("Không có phòng đang ở.");
        dialog.Add("Phòng đang ở",combo);dialog.Action("CHỌN",()=>Task.CompletedTask);
        return dialog.ShowDialog(this)==DialogResult.OK && combo.SelectedItem is StayChoice item?item.Stay:null;
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
    private Task ShowBooking(bool reserve,Room? selected=null)
    {
        var rooms=data.Rooms.Where(r=>reserve?r.Status!=RoomStatus.BaoTri:r.Status==RoomStatus.Trong).ToList();
        if(rooms.Count==0)throw new BusinessException("Không có phòng phù hợp.");
        using var dialog=new InputDialog(reserve?"Đặt phòng trước":"Nhận phòng trực tiếp",760,reserve?780:540,true);
        var room=Ui.Combo(rooms);if(selected!=null)room.SelectedItem=rooms.Single(r=>r.Id==selected.Id);
        var name=Ui.Text();var phone=Ui.Text(20);var identity=Ui.Text(20);
        var arrival=Ui.DatePicker(ServerNow.AddHours(2));var days=Ui.Number(60);
        var receiveBy=Ui.DatePicker(ServerNow.AddDays(1));
        var deposit=new CheckBox {Text="Đã thu tiền cọc",AutoSize=true};
        var amount=Ui.Money();
        deposit.CheckedChanged+=(_,_)=>UpdateHold();
        void UpdateHold()
        {
            if(!reserve)return;
            receiveBy.Value=HotelService.ReservationHoldLimit(ServerNow,deposit.Checked);
        }
        var depositInfo=new Label {AutoSize=true};
        void UpdateDeposit(){if(room.SelectedItem is Room r){depositInfo.Text=$"Cọc gợi ý: {r.Deposit:N0} đ";amount.Value=Math.Min(amount.Maximum,r.Deposit);}}
        room.SelectedIndexChanged+=(_,_)=>UpdateDeposit();UpdateDeposit();
        void FilterRooms()
        {
            var oldId=(room.SelectedItem as Room)?.Id;
            var from=reserve?arrival.Value:ServerNow;var until=from.AddDays((int)days.Value);
            room.DataSource=rooms.Where(r=>!data.Stays.Any(s=>s.RoomId==r.Id && (s.CheckIn??s.Arrival)<until && s.Departure>from)).ToList();
            if(oldId is { } id && room.Items.Cast<Room>().FirstOrDefault(r=>r.Id==id) is { } previous)room.SelectedItem=previous;
        }
        arrival.ValueChanged+=(_,_)=>FilterRooms();
        days.ValueChanged+=(_,_)=>FilterRooms();FilterRooms();UpdateHold();
        var method=Ui.Combo(new[]{"Tiền mặt","Chuyển khoản"});
        dialog.Add("Chọn phòng",room);dialog.Add("Họ và tên",name);dialog.Add("Số điện thoại",phone);dialog.Add("CCCD (12 số) / Hộ chiếu",identity);
        if(reserve){dialog.Add("Ngày giờ dự kiến đến",arrival);dialog.Add("Hạn cuối nhận phòng (tối đa 1 hoặc 15 ngày từ lúc đặt)",receiveBy);}
        dialog.Add("Số ngày thuê dự kiến",days);
        if(reserve){dialog.Add("Tiền cọc",deposit);dialog.Add("",depositInfo);dialog.Add("Số tiền thực thu (đồng)",amount);dialog.Add("Hình thức thu cọc",method);}
        if(reserve)
        {
            PaymentQr.Add(dialog,method,()=>amount.Value,()=>"COC PHONG "+phone.Text.Trim(),amount,phone);
        }
        if(reserve)dialog.Note("Chưa cọc: giữ tối đa 24 giờ. Đã cọc: giữ tối đa 15 ngày từ lúc đặt. Ngày đến phải trước hạn giữ. Quá hạn chưa nhận: tự hủy, không hoàn cọc; hủy trước hạn: hoàn cọc.");
        dialog.Action(reserve?"LƯU ĐẶT PHÒNG":"XÁC NHẬN NHẬN PHÒNG",async()=>
        {
            if(room.SelectedItem is not Room chosen)throw new BusinessException("Chưa chọn phòng.");
            await Changed(async()=> {await service.CreateStayAsync(chosen,new GuestInput(name.Text,phone.Text,identity.Text),reserve,arrival.Value,(int)days.Value,reserve && deposit.Checked,(string)method.SelectedItem!,reserve?amount.Value:0,reserve?receiveBy.Value:null);});
        });
        dialog.ShowDialog(this);return Task.CompletedTask;
    }
    private async Task ShowCancel(Stay stay)
    {
        var refund=await service.RefundQuoteAsync(stay);
        using var dialog=new InputDialog("Hủy đặt phòng / Hoàn cọc",540,350);
        dialog.Note($"Khách: {stay.Guest}\nPhòng: {StayRoom(stay)?.Number}\nHạn nhận: {stay.HoldUntil:dd/MM/yyyy HH:mm}\nSố tiền được hoàn: {refund:N0} đ\nCọc không hoàn: {stay.Deposit-refund:N0} đ");
        var method=Ui.Combo(new[]{"Tiền mặt","Chuyển khoản"});dialog.Add("Phương thức hoàn",method);
        var confirmed=new CheckBox {Text=refund>0?"Tôi xác nhận đã hoàn đủ tiền cọc":"Xác nhận hủy; không hoàn tiền cọc",AutoSize=true};dialog.Add("Xác nhận",confirmed);
        dialog.Action("HỦY ĐẶT PHÒNG",async()=>
        {
            if(!confirmed.Checked)throw new BusinessException("Cần xác nhận xử lý tiền cọc/giữ chỗ.");
            await Changed(()=>service.CancelAsync(stay,(string)method.SelectedItem!,refund));
        });
        dialog.ShowDialog(this);
    }
    private async void btnDoiPhong_Click(object? sender,EventArgs e)=>await Run(()=>
    {
        using var dialog=new InputDialog("Chuyển phòng",560,390);var from=StayCombo();var to=Ui.Combo(data.Rooms.Where(r=>r.Status==RoomStatus.Trong));
        if(from.Items.Count==0 || to.Items.Count==0)throw new BusinessException("Cần phòng đang ở và phòng trống để chuyển.");
        dialog.Add("Lượt lưu trú",from);dialog.Add("Phòng mới",to);
        dialog.Note("Giá phòng cũ được giữ cho thời gian đã ở. Phần thời gian sau chuyển tính theo giá phòng mới; lượt ở chỉ làm tròn ngày một lần khi trả phòng.");
        dialog.Action("XÁC NHẬN CHUYỂN",async()=>
        {
            if(from.SelectedItem is StayChoice a && to.SelectedItem is Room b)await Changed(()=>service.TransferAsync(a.Stay,b));
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
        using var dialog=new InputDialog("Bảo trì phòng",530,270);var rooms=Ui.Combo(data.Rooms.Where(r=>r.Status is RoomStatus.Trong or RoomStatus.BaoTri));
        if(rooms.Items.Count==0)throw new BusinessException("Không có phòng có thể đổi trạng thái bảo trì.");
        dialog.Add("Chọn phòng trống hoặc đang bảo trì",rooms);
        dialog.Action("BẬT / KẾT THÚC BẢO TRÌ",async()=> {if(rooms.SelectedItem is Room r)await Changed(()=>service.SetRoomStatusAsync(r,r.Status==RoomStatus.Trong?RoomStatus.BaoTri:RoomStatus.Trong));});
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
        using var dialog=new InputDialog("Gọi dịch vụ",760,760);
        var stay=StayCombo();if(stay.Items.Count==0)throw new BusinessException("Không có phòng đang ở.");
        var categories=Ui.Combo(new[]{"Tất cả"}.Concat(data.Menu.Select(s=>s.Category).Distinct()));var items=Ui.Combo(data.Menu);var quantity=Ui.Number(100);
        categories.SelectedIndexChanged+=(_,_)=>items.DataSource=data.Menu.Where(s=>categories.SelectedIndex==0 || s.Category==(string?)categories.SelectedItem).ToList();
        var cart=new List<(ServiceItem Item,int Quantity)>();var list=new ListBox {Height=170};var total=new Label {AutoSize=true};
        void UpdateCart(){list.DataSource=null;list.DataSource=cart.Select(x=>$"{x.Item.Name} × {x.Quantity} = {x.Item.Price*x.Quantity:N0} đ").ToList();total.Text=$"Tạm tính: {cart.Sum(x=>x.Item.Price*x.Quantity):N0} đ";}
        dialog.Add("Phòng nhận",stay);dialog.Add("Danh mục",categories);dialog.Add("Dịch vụ",items);dialog.Add("Số lượng",quantity);
        var add=new Button {Text="+ Thêm vào danh sách",Height=34};
        add.Click+=(_,_)=> {if(items.SelectedItem is ServiceItem item && cart.Count<100){cart.Add((item,(int)quantity.Value));UpdateCart();}};
        dialog.Add("",add);dialog.Add("Danh sách yêu cầu",list);
        var remove=new Button {Text="Xóa dòng đã chọn",Height=32};remove.Click+=(_,_)=> {if(list.SelectedIndex>=0){cart.RemoveAt(list.SelectedIndex);UpdateCart();}};
        dialog.Add("",remove);dialog.Add("",total);UpdateCart();
        dialog.Action("GỬI YÊU CẦU",async()=> {if(stay.SelectedItem is StayChoice item)await Changed(()=>service.AddServicesAsync(item.Stay,cart.Select(x=>new OrderInput(x.Item.Id,x.Quantity)).ToArray()));});
        dialog.ShowDialog(this);return Task.CompletedTask;
    });
}


