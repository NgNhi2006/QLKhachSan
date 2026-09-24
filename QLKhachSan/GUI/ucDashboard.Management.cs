using QLKhachSan.DTO;

namespace QLKhachSan.GUI;

public partial class ucDashboard
{
    private void AddTool(string title,int y,Func<Task> action)
    {
        var button=new Button {Text=title,Width=190,Height=38,Location=new Point(10,y)};
        button.Click+=async (_,_)=>await Run(action);pnlLeftTools.Controls.Add(button);
    }
    private Task ShowDeposit()
    {
        using var dialog=new InputDialog("Thu cọc bổ sung",600,460);
        var choice=Ui.Combo(data.Stays.Where(s=>s.Status==StayStatus.Reserved).Select(s=>new StayChoice(s,$"#{s.Id} • P.{StayRoom(s)?.Number} • {s.Guest} • Đã cọc {s.Deposit:N0} đ")));
        if(choice.Items.Count==0)throw new BusinessException("Không có lượt đặt trước chờ nhận phòng để thu cọc.");
        var amount=Ui.Money();var method=Ui.Combo(new[]{"Tiền mặt","Chuyển khoản"});
        var confirm=new CheckBox {Text="Đã nhận đủ số tiền bổ sung",AutoSize=true};
        dialog.Add("Lượt đặt trước chưa nhận phòng",choice);dialog.Add("Thu thêm (đồng)",amount);dialog.Add("Hình thức",method);
        PaymentQr.Add(dialog,method,()=>amount.Value,()=>choice.SelectedItem is StayChoice item?$"COC-{item.Stay.Id}":"COC",amount,null,choice);
        dialog.Add("Xác nhận",confirm);
        dialog.Note("Cọc đầu tiên chuyển hạn giữ tối đa sang 15 ngày tính từ lúc đặt phòng. Quá hạn chưa nhận: tự hủy và không hoàn cọc.");
        dialog.Action("GHI NHẬN THU",async()=>
        {
            if(!confirm.Checked)throw new BusinessException("Cần xác nhận đã nhận tiền.");
            if(choice.SelectedItem is StayChoice selected)await Changed(()=>service.AddDepositAsync(selected.Stay,amount.Value,(string)method.SelectedItem!));
        });
        dialog.ShowDialog(this);return Task.CompletedTask;
    }
    private Task ShowEditBooking(Stay stay)
    {
        using var dialog=new InputDialog("Sửa lịch đặt / thông tin khách",760,720,true);
        var room=Ui.Combo(data.Rooms.Where(r=>r.Status!=RoomStatus.BaoTri));
        room.SelectedItem=data.Rooms.FirstOrDefault(r=>r.Id==stay.RoomId);
        var name=Ui.Text();name.Text=stay.Guest;var phone=Ui.Text(20);phone.Text=stay.Phone;var identity=Ui.Text(20);identity.Text=stay.Identity;
        var arrival=Ui.DatePicker(stay.Arrival);var days=Ui.Number(60,Math.Clamp((int)(stay.Departure-stay.Arrival).TotalDays,1,60));
        var receiveBy=Ui.DatePicker(stay.HoldUntil??stay.Arrival.AddHours(2));var reason=Ui.Text(300);
        dialog.Add("Phòng",room);dialog.Add("Họ tên",name);dialog.Add("SĐT",phone);dialog.Add("CCCD / Hộ chiếu",identity);dialog.Add("Ngày đến",arrival);dialog.Add("Số ngày",days);dialog.Add("Hạn cuối nhận",receiveBy);dialog.Add("Lý do thay đổi",reason);
        dialog.Note($"Cọc đã thu: {stay.Deposit:N0} đ. Hạn tối đa: {QLKhachSan.BLL.HotelService.ReservationHoldLimit(stay.Created,stay.Deposit>0):dd/MM/yyyy HH:mm}. Không sửa được lượt quá hạn.");
        dialog.Action("LƯU THAY ĐỔI",async()=>
        {
            if(room.SelectedItem is not Room target)throw new BusinessException("Chọn phòng.");
            await Changed(()=>service.UpdateBookingAsync(stay,target,new GuestInput(name.Text,phone.Text,identity.Text),arrival.Value,(int)days.Value,receiveBy.Value,reason.Text));
        });
        dialog.ShowDialog(this);return Task.CompletedTask;
    }
    private async Task ShowOrderManagement()
    {
        var selected=SelectStay("Chọn lượt để xử lý dịch vụ");if(selected is null)return;
        using var dialog=new InputDialog("Xử lý từng dịch vụ",960,720);
        var grid=Ui.Grid();grid.Height=240;
        var quantity=Ui.Number(100);var reason=Ui.Text(300);
        dialog.Add("Dịch vụ (dòng đã hủy được giữ trong lịch sử)",grid);
        dialog.Add("Số lượng mới / số lượng giao thêm",quantity);dialog.Add("Lý do sửa hoặc hủy",reason);
        dialog.Note("Có thể giao từng phần. Không hủy phần đã giao; giảm số lượng xuống bằng số đã giao để bỏ phần còn lại.");
        async Task RefreshOrders()
        {
            await Reload();
            selected=data.Stays.SingleOrDefault(s=>s.Id==selected.Id)??throw new BusinessException("Lượt đã đóng.");
            grid.DataSource=await service.StayOrdersAsync(selected.Id);
            FormatGrid(grid,new() {{"Name","Dịch vụ"},{"Quantity","Số lượng"},{"DeliveredQuantity","Đã giao"},{"Price","Đơn giá"},{"Total","Thành tiền"},{"Cancelled","Đã hủy lúc"},{"Delivered","Giao xong lúc"},{"Ordered","Gọi lúc"},{"Category","Danh mục"}});
            HideColumns(grid,"Id","StayId");
        }
        ServiceLine Line()=>grid.CurrentRow?.DataBoundItem as ServiceLine??throw new BusinessException("Chọn một dịch vụ.");
        dialog.Action("GIAO THÊM SỐ LƯỢNG ĐÃ NHẬP",async()=>{await service.DeliverOrderAsync(selected,Line().Id,(int)quantity.Value);await RefreshOrders();},false);
        dialog.Action("SỬA SỐ LƯỢNG",async()=>{await service.ChangeOrderAsync(selected,Line().Id,(int)quantity.Value,reason.Text);await RefreshOrders();},false);
        dialog.Action("HỦY DÒNG ĐÃ CHỌN",async()=>
        {
            var line=Line();
            if(!Ui.Confirm(dialog,$"Hủy {line.Name}? Lịch sử vẫn được lưu."))return;
            await service.ChangeOrderAsync(selected,line.Id,0,reason.Text,true);await RefreshOrders();
        },false);
        dialog.Action("XÁC NHẬN TẤT CẢ",async()=>
        {
            if(!Ui.Confirm(dialog,$"Xác nhận đã giao toàn bộ dịch vụ còn chờ của {selected.Guest}?"))return;
            await service.DeliverAsync(selected);await RefreshOrders();
        },false);
        dialog.Action("HỦY TẤT CẢ CHƯA GIAO",async()=>
        {
            if(!Ui.Confirm(dialog,$"Hủy mọi phần dịch vụ chưa giao của {selected.Guest}? Phần đã giao vẫn tính tiền."))return;
            await service.CancelPendingOrdersAsync(selected,reason.Text);await RefreshOrders();
        },false);
        await RefreshOrders();dialog.CompactActions();dialog.AcceptButton=null;dialog.ShowDialog(this);
    }
    private async Task ShowStayHistory()
    {
        using var dialog=new InputDialog("Lịch đặt và lịch sử lưu trú",1150,800);
        var search=Ui.Text();var from=Ui.DatePicker(ServerNow.Date);var until=Ui.DatePicker(ServerNow.Date.AddDays(30));
        var all=new CheckBox {Text="Xem cả lịch sử (tối đa 500 lượt gần nhất theo tìm kiếm)",AutoSize=true};
        var grid=Ui.Grid();grid.Height=330;
        dialog.Add("Tên / SĐT / CCCD",search);dialog.Add("Từ ngày",from);dialog.Add("Đến hết ngày",until);dialog.Add("",all);dialog.Add("Lượt lưu trú",grid);
        dialog.Note("Phòng có dấu * trên sơ đồ đang có lịch đặt. Quá hạn không đến: tự hủy, không hoàn cọc; xem chi tiết tại báo cáo thu/chi và nhật ký.");
        async Task LoadRows()
        {
            if(until.Value.Date<from.Value.Date)throw new BusinessException("Khoảng ngày không hợp lệ.");
            var rows=all.Checked?await service.StayHistoryAsync(search.Text):(await service.DashboardAsync()).Stays.Where(s=>s.Arrival<until.Value.Date.AddDays(1) && s.Departure>from.Value.Date && (s.Guest.Contains(search.Text,StringComparison.OrdinalIgnoreCase)||s.Phone.Contains(search.Text)||s.Identity.Contains(search.Text,StringComparison.OrdinalIgnoreCase))).ToList();
            grid.DataSource=rows.Select(s=>new {Mã=s.Id,Phòng=data.Rooms.FirstOrDefault(r=>r.Id==s.RoomId)?.Number,Khách=s.Guest,TrạngThái=s.Status.ToString(),NgàyĐến=s.Arrival,NgàyTrả=s.Departure,HạnNhận=s.HoldUntil,Cọc=s.Deposit}).ToList();
            FormatGrid(grid,new() {{"TrạngThái","Trạng thái"},{"NgàyĐến","Ngày đến"},{"NgàyTrả","Ngày trả"},{"HạnNhận","Hạn nhận"}});
        }
        dialog.Action("TRA CỨU",LoadRows,false);dialog.Action("XUẤT CSV",()=>{ExportGrid(grid,"lich-luu-tru");return Task.CompletedTask;},false);
        await LoadRows();dialog.ShowDialog(this);
    }
    private async Task ShowAudit()
    {
        using var dialog=new InputDialog("Nhật ký thao tác",1100,650);
        var day=Ui.DatePicker(ServerNow.Date);var grid=Ui.Grid();grid.Height=380;
        dialog.Add("Ngày (tối đa 1.000 thao tác gần nhất)",day);dialog.Add("Nhật ký",grid);
        async Task LoadRows(){grid.DataSource=await service.AuditsAsync(day.Value);FormatGrid(grid,new() {{"Created","Thời điểm"},{"Username","Nhân viên"},{"Action","Thao tác"},{"Detail","Chi tiết"}});}
        dialog.Action("XEM",LoadRows,false);await LoadRows();dialog.ShowDialog(this);
    }
    private static void HideColumns(DataGridView grid,params string[] names)
    {
        foreach(var name in names)if(grid.Columns[name] is { } column)column.Visible=false;
    }
    private static void FormatGrid(DataGridView grid,Dictionary<string,string> names)
    {
        grid.AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.AllCells;
        foreach(DataGridViewColumn column in grid.Columns)
        {
            if(names.TryGetValue(column.Name,out var name))column.HeaderText=name;
            if(column.ValueType==typeof(decimal))column.DefaultCellStyle.Format="N0";
            if(column.ValueType==typeof(DateTime)||column.ValueType==typeof(DateTime?))column.DefaultCellStyle.Format="dd/MM/yyyy HH:mm";
        }
    }
}


