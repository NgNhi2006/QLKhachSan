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
        using var dialog=new InputDialog("Thu cọc bổ sung",760,Math.Min(660,Screen.FromControl(this).WorkingArea.Height-30));
        var choice=Ui.Combo(data.Stays.Where(s=>s.Status==StayStatus.Reserved).Select(s=>new StayChoice(s,$"#{s.Id} • P.{StayRoom(s)?.Number} • {s.Guest} • Đã cọc {s.Deposit:N0} đ")));
        if(choice.Items.Count==0)throw new BusinessException("Không có lượt đặt trước chờ nhận phòng để thu cọc.");
        var amount=Ui.Money();var method=Ui.Combo(new[]{"Tiền mặt","Chuyển khoản","Thẻ POS"});var reference=Ui.Text(100);
        var summary=new Label {AutoSize=true,MinimumSize=new Size(0,56),MaximumSize=new Size(680,0),BackColor=AppTheme.Canvas,ForeColor=AppTheme.Ink,Padding=new Padding(12,8,8,6)};
        void UpdateSummary()
        {
            if(choice.SelectedItem is not StayChoice selected)return;
            summary.Text=$"P.{StayRoom(selected.Stay)?.Number}  •  {selected.Stay.Guest}  •  Cọc đã thu: {selected.Stay.Deposit:N0} đ\nSau bổ sung: {selected.Stay.Deposit+amount.Value:N0} đ  •  Hạn nhận: {selected.Stay.HoldUntil:dd/MM/yyyy HH:mm}";
        }
        choice.SelectedIndexChanged+=(_,_)=>UpdateSummary();amount.ValueChanged+=(_,_)=>UpdateSummary();
        var confirm=new CheckBox {Text="Tôi xác nhận đã nhận đủ số tiền bổ sung",AutoSize=true};
        dialog.Add("Lượt đặt trước chưa nhận phòng",choice);dialog.Add("Thu thêm (đồng)",amount);dialog.Add("Hình thức",method);dialog.Add("Mã giao dịch QR/POS",reference);
        dialog.Add("THÔNG TIN THU CỌC",summary);
        PaymentQr.Add(dialog,method,()=>amount.Value,()=>choice.SelectedItem is StayChoice item?$"COC-{item.Stay.Id}":"COC",amount,null,choice,transfer=>
        {
            var area=Screen.FromControl(this).WorkingArea;
            dialog.Height=Math.Min(transfer?1010:660,area.Height-30);
            if(dialog.Visible)dialog.Top=area.Top+(area.Height-dialog.Height)/2;
        });
        dialog.Add("Xác nhận giao dịch",confirm);
        dialog.Note("Cọc đầu tiên chuyển hạn giữ tối đa sang 15 ngày tính từ lúc đặt phòng. Quá hạn chưa nhận: tự hủy và không hoàn cọc.");
        dialog.Action("GHI NHẬN THU",async()=>
        {
            if(!confirm.Checked)throw new BusinessException("Cần xác nhận đã nhận tiền.");
            if(choice.SelectedItem is StayChoice selected)await Changed(()=>service.AddDepositAsync(selected.Stay,amount.Value,(string)method.SelectedItem!,reference.Text));
        });
        UpdateSummary();dialog.ShowDialog(this);return Task.CompletedTask;
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
        using var dialog=new Form {Text="Xử lý dịch vụ",Size=new Size(1100,700),MinimumSize=new Size(900,570),StartPosition=FormStartPosition.CenterParent,Font=AppTheme.Body,BackColor=AppTheme.Canvas,AutoScaleMode=AutoScaleMode.Dpi};
        var root=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=1,RowCount=3};
        root.RowStyles.Add(new RowStyle(SizeType.Absolute,80));root.RowStyles.Add(new RowStyle(SizeType.Percent,100));root.RowStyles.Add(new RowStyle(SizeType.Absolute,118));dialog.Controls.Add(root);
        var heading=new Panel {Dock=DockStyle.Fill,BackColor=Color.White,Padding=new Padding(24,10,20,8)};
        heading.Controls.Add(new Label {Text=$"Dịch vụ phòng P.{StayRoom(selected)?.Number}",Dock=DockStyle.Top,Height=36,Font=AppTheme.Title,ForeColor=AppTheme.Ink});
        heading.Controls.Add(new Label {Text=$"{selected.Guest}  •  Chọn một dịch vụ để giao, sửa hoặc hủy",Dock=DockStyle.Bottom,Height=22,ForeColor=AppTheme.Muted});root.Controls.Add(heading,0,0);
        var body=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=2,RowCount=1,Padding=new Padding(18,14,18,8)};
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,66));body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,34));root.Controls.Add(body,0,1);
        var listPanel=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=1,RowCount=2,BackColor=Color.White,Padding=new Padding(14),Margin=new Padding(0,0,7,0)};
        listPanel.RowStyles.Add(new RowStyle(SizeType.Absolute,32));listPanel.RowStyles.Add(new RowStyle(SizeType.Percent,100));body.Controls.Add(listPanel,0,0);
        var count=new Label {Dock=DockStyle.Fill,Font=AppTheme.Bold,ForeColor=AppTheme.Blue};listPanel.Controls.Add(count,0,0);
        var grid=Ui.Grid();grid.AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill;listPanel.Controls.Add(grid,0,1);
        var editor=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=1,RowCount=8,BackColor=Color.White,Padding=new Padding(16,12,16,10),Margin=new Padding(7,0,0,0)};
        editor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
        foreach(var height in new[]{32,80,28,42,28,42,0,0})editor.RowStyles.Add(new RowStyle(height==0?SizeType.Percent:SizeType.Absolute,height==0?50:height));body.Controls.Add(editor,1,0);
        editor.Controls.Add(new Label {Text="DỊCH VỤ ĐANG CHỌN",Dock=DockStyle.Fill,Font=AppTheme.Bold,ForeColor=AppTheme.Blue},0,0);
        var detail=new Label {Dock=DockStyle.Fill,BackColor=AppTheme.Canvas,Padding=new Padding(10,9,6,6),ForeColor=AppTheme.Ink};editor.Controls.Add(detail,0,1);
        editor.Controls.Add(new Label {Text="Số lượng mới / số lượng giao thêm",Dock=DockStyle.Fill,ForeColor=AppTheme.Muted},0,2);
        var quantity=Ui.Number(100);quantity.Dock=DockStyle.Fill;editor.Controls.Add(quantity,0,3);
        editor.Controls.Add(new Label {Text="Lý do sửa hoặc hủy",Dock=DockStyle.Fill,ForeColor=AppTheme.Muted},0,4);
        var reason=Ui.Text(300);reason.Dock=DockStyle.Fill;editor.Controls.Add(reason,0,5);
        editor.Controls.Add(new Label {Text="Có thể giao từng phần. Giảm số lượng xuống bằng số đã giao để bỏ phần còn lại.",Dock=DockStyle.Fill,ForeColor=AppTheme.Muted,Padding=new Padding(0,12,0,0)},0,6);
        var footer=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=3,RowCount=2,Padding=new Padding(18,10,18,14)};
        for(var i=0;i<3;i++)footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100f/3));
        footer.RowStyles.Add(new RowStyle(SizeType.Percent,50));footer.RowStyles.Add(new RowStyle(SizeType.Percent,50));root.Controls.Add(footer,0,2);
        Button ActionButton(string text,int column,int row,bool primary=false)
        {
            var button=new Button {Text=text,Dock=DockStyle.Fill,Margin=new Padding(4)};AppTheme.Button(button,primary);footer.Controls.Add(button,column,row);return button;
        }
        var deliver=ActionButton("GIAO THÊM",0,0,true);var change=ActionButton("SỬA SỐ LƯỢNG",1,0);var cancel=ActionButton("HỦY DÒNG",2,0);
        var deliverAll=ActionButton("XÁC NHẬN TẤT CẢ",0,1);var cancelAll=ActionButton("HỦY TẤT CẢ CHƯA GIAO",1,1);var close=ActionButton("ĐÓNG",2,1);close.Click+=(_,_)=>dialog.Close();
        void ShowSelection()
        {
            if(grid.CurrentRow?.DataBoundItem is not ServiceLine line){detail.Text="Chưa chọn dịch vụ.";deliver.Enabled=change.Enabled=cancel.Enabled=false;return;}
            detail.Text=$"{line.Name}\nĐã gọi: {line.Quantity}  •  Đã giao: {line.DeliveredQuantity}\nĐơn giá: {line.Price:N0} đ";
            var editable=line.Cancelled is null;deliver.Enabled=editable && line.DeliveredQuantity<line.Quantity;
            change.Enabled=editable;cancel.Enabled=editable && line.DeliveredQuantity==0;
        }
        async Task RefreshOrders()
        {
            await Reload();
            selected=data.Stays.SingleOrDefault(s=>s.Id==selected.Id)??throw new BusinessException("Lượt đã đóng.");
            var rows=await service.StayOrdersAsync(selected.Id);grid.DataSource=rows;
            foreach(DataGridViewColumn column in grid.Columns)column.Visible=column.Name is "Name" or "Quantity" or "DeliveredQuantity" or "Price" or "Total" or "Cancelled";
            var names=new Dictionary<string,string>{{"Name","Dịch vụ"},{"Quantity","Đã gọi"},{"DeliveredQuantity","Đã giao"},{"Price","Đơn giá"},{"Total","Thành tiền"},{"Cancelled","Đã hủy"}};
            foreach(DataGridViewColumn column in grid.Columns)
            {
                if(names.TryGetValue(column.Name,out var title))column.HeaderText=title;
                if(column.Name is "Price" or "Total")column.DefaultCellStyle.Format="N0";
                if(column.Name=="Cancelled")column.DefaultCellStyle.Format="dd/MM HH:mm";
            }
            count.Text=$"DANH SÁCH DỊCH VỤ ({rows.Count})";
            deliverAll.Enabled=cancelAll.Enabled=rows.Any(x=>x.Cancelled is null && x.DeliveredQuantity<x.Quantity);
            ShowSelection();
        }
        ServiceLine Line()=>grid.CurrentRow?.DataBoundItem as ServiceLine??throw new BusinessException("Chọn một dịch vụ.");
        async Task Execute(Func<Task> work)
        {
            footer.Enabled=false;
            try{await work();await RefreshOrders();}catch(Exception ex){Ui.Error(dialog,ex);}finally{footer.Enabled=true;ShowSelection();}
        }
        grid.SelectionChanged+=(_,_)=>ShowSelection();
        deliver.Click+=async (_,_)=>await Execute(()=>service.DeliverOrderAsync(selected,Line().Id,(int)quantity.Value));
        change.Click+=async (_,_)=>await Execute(()=>service.ChangeOrderAsync(selected,Line().Id,(int)quantity.Value,reason.Text));
        cancel.Click+=async (_,_)=>
        {
            var line=Line();if(Ui.Confirm(dialog,$"Hủy {line.Name}? Lịch sử vẫn được lưu."))await Execute(()=>service.ChangeOrderAsync(selected,line.Id,0,reason.Text,true));
        };
        deliverAll.Click+=async (_,_)=>
        {
            if(Ui.Confirm(dialog,$"Xác nhận đã giao toàn bộ dịch vụ còn chờ của {selected.Guest}?"))await Execute(()=>service.DeliverAsync(selected));
        };
        cancelAll.Click+=async (_,_)=>
        {
            if(Ui.Confirm(dialog,$"Hủy mọi phần dịch vụ chưa giao của {selected.Guest}? Phần đã giao vẫn tính tiền."))await Execute(()=>service.CancelPendingOrdersAsync(selected,reason.Text));
        };
        await RefreshOrders();dialog.ShowDialog(this);
    }
    private async Task ShowStayHistory()
    {
        using var dialog=new Form {Text="Lịch đặt và lịch sử lưu trú",Size=new Size(1150,720),MinimumSize=new Size(900,580),StartPosition=FormStartPosition.CenterParent,Font=AppTheme.Body,BackColor=AppTheme.Canvas,AutoScaleMode=AutoScaleMode.Dpi};
        var root=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=1,RowCount=4};
        root.RowStyles.Add(new RowStyle(SizeType.Absolute,76));root.RowStyles.Add(new RowStyle(SizeType.Absolute,126));
        root.RowStyles.Add(new RowStyle(SizeType.Percent,100));root.RowStyles.Add(new RowStyle(SizeType.Absolute,76));dialog.Controls.Add(root);
        var heading=new Panel {Dock=DockStyle.Fill,BackColor=Color.White,Padding=new Padding(24,10,20,8)};
        heading.Controls.Add(new Label {Text="Lịch đặt và lịch sử lưu trú",Dock=DockStyle.Top,Height=36,Font=AppTheme.Title,ForeColor=AppTheme.Ink});
        heading.Controls.Add(new Label {Text="Tra cứu lượt đặt, lượt đang ở và lịch sử theo khách hoặc khoảng ngày",Dock=DockStyle.Bottom,Height=22,ForeColor=AppTheme.Muted});root.Controls.Add(heading,0,0);
        var filters=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=3,RowCount=3,Padding=new Padding(22,10,22,6)};
        filters.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,50));filters.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,25));filters.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,25));
        filters.RowStyles.Add(new RowStyle(SizeType.Absolute,25));filters.RowStyles.Add(new RowStyle(SizeType.Absolute,42));filters.RowStyles.Add(new RowStyle(SizeType.Percent,100));root.Controls.Add(filters,0,1);
        filters.Controls.Add(new Label {Text="Tên khách / SĐT / CCCD",Dock=DockStyle.Fill,ForeColor=AppTheme.Muted},0,0);
        filters.Controls.Add(new Label {Text="Từ ngày",Dock=DockStyle.Fill,ForeColor=AppTheme.Muted},1,0);
        filters.Controls.Add(new Label {Text="Đến hết ngày",Dock=DockStyle.Fill,ForeColor=AppTheme.Muted},2,0);
        var search=Ui.Text();search.PlaceholderText="Nhập thông tin khách cần tìm...";search.Dock=DockStyle.Fill;search.Margin=new Padding(0,0,12,0);filters.Controls.Add(search,0,1);
        var from=Ui.DatePicker(ServerNow.Date);from.CustomFormat="dd/MM/yyyy";from.Dock=DockStyle.Fill;from.Margin=new Padding(0,0,12,0);filters.Controls.Add(from,1,1);
        var until=Ui.DatePicker(ServerNow.Date.AddDays(30));until.CustomFormat="dd/MM/yyyy";until.Dock=DockStyle.Fill;filters.Controls.Add(until,2,1);
        var all=new CheckBox {Text="Xem cả lịch sử (tối đa 500 lượt gần nhất)",Dock=DockStyle.Fill};filters.Controls.Add(all,0,2);filters.SetColumnSpan(all,3);
        all.CheckedChanged+=(_,_)=>{from.Enabled=!all.Checked;until.Enabled=!all.Checked;};
        var results=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=1,RowCount=3,BackColor=Color.White,Margin=new Padding(22,0,22,8),Padding=new Padding(14,10,14,10)};
        results.RowStyles.Add(new RowStyle(SizeType.Absolute,34));results.RowStyles.Add(new RowStyle(SizeType.Percent,100));results.RowStyles.Add(new RowStyle(SizeType.Absolute,42));root.Controls.Add(results,0,2);
        var count=new Label {Dock=DockStyle.Fill,Font=AppTheme.Bold,ForeColor=AppTheme.Blue};results.Controls.Add(count,0,0);
        var grid=Ui.Grid();grid.AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill;results.Controls.Add(grid,0,1);
        results.Controls.Add(new Label {Text="Phòng có dấu * trên sơ đồ đang có lịch đặt. Lượt quá hạn không đến được xử lý theo chính sách cọc.",Dock=DockStyle.Fill,ForeColor=AppTheme.Muted,TextAlign=ContentAlignment.MiddleLeft},0,2);
        var footer=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=2,RowCount=1,Padding=new Padding(22,10,22,14)};
        footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,50));footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,50));root.Controls.Add(footer,0,3);
        var lookup=new Button {Text="TRA CỨU",Dock=DockStyle.Fill,Margin=new Padding(0,0,6,0)};AppTheme.Button(lookup,true);footer.Controls.Add(lookup,0,0);
        var export=new Button {Text="XUẤT CSV",Dock=DockStyle.Fill,Margin=new Padding(6,0,0,0)};AppTheme.Button(export);footer.Controls.Add(export,1,0);
        async Task LoadRows()
        {
            if(until.Value.Date<from.Value.Date)throw new BusinessException("Khoảng ngày không hợp lệ.");
            var rows=all.Checked?await service.StayHistoryAsync(search.Text):(await service.DashboardAsync()).Stays.Where(s=>s.Arrival<until.Value.Date.AddDays(1) && s.Departure>from.Value.Date && (s.Guest.Contains(search.Text,StringComparison.OrdinalIgnoreCase)||s.Phone.Contains(search.Text)||s.Identity.Contains(search.Text,StringComparison.OrdinalIgnoreCase))).ToList();
            grid.DataSource=rows.Select(s=>new {Mã=s.Id,Phòng=data.Rooms.FirstOrDefault(r=>r.Id==s.RoomId)?.Number,Khách=s.Guest,
                TrạngThái=s.Status switch {StayStatus.Reserved=>"Chờ nhận",StayStatus.Occupied=>"Đang ở",StayStatus.Paid=>"Đã trả",StayStatus.Cancelled=>"Đã hủy",_=>"Khác"},
                NgàyĐến=s.Arrival,NgàyTrả=s.Departure,HạnNhận=s.HoldUntil,Cọc=s.Deposit}).ToList();
            foreach(DataGridViewColumn column in grid.Columns)
            {
                if(column.Name=="TrạngThái")column.HeaderText="Trạng thái";
                if(column.Name=="NgàyĐến")column.HeaderText="Ngày đến";
                if(column.Name=="NgàyTrả")column.HeaderText="Ngày trả";
                if(column.Name=="HạnNhận")column.HeaderText="Hạn nhận";
                if(column.Name is "NgàyĐến" or "NgàyTrả" or "HạnNhận")column.DefaultCellStyle.Format="dd/MM/yyyy HH:mm";
                if(column.Name=="Cọc")column.DefaultCellStyle.Format="N0";
            }
            count.Text=$"KẾT QUẢ: {rows.Count} LƯỢT"+(all.Checked?"  •  Lịch sử gần nhất":"  •  Trong khoảng ngày đã chọn");
        }
        lookup.Click+=async (_,_)=>{lookup.Enabled=false;try{await LoadRows();}catch(Exception ex){Ui.Error(dialog,ex);}finally{lookup.Enabled=true;}};
        search.KeyDown+=(_,e)=>{if(e.KeyCode==Keys.Enter){e.SuppressKeyPress=true;lookup.PerformClick();}};
        export.Click+=(_,_)=>ExportGrid(grid,"lich-luu-tru");
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


