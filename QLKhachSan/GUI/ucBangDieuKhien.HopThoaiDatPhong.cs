using QLKhachSan.BLL;
using QLKhachSan.DAL;
using QLKhachSan.DTO;

namespace QLKhachSan.GUI;

public partial class ucDashboard
{
    private Task ShowBookingDialog(bool reserve,Room? selected,DateTime? requestedArrival,int requestedDays)
    {
        var candidates=data.Rooms.Where(r=>reserve?r.Status!=RoomStatus.BaoTri:r.Status==RoomStatus.Trong)
            .OrderBy(r=>r.Number).ToList();
        if(candidates.Count==0)throw new BusinessException("Không có phòng phù hợp.");
        var area=Screen.FromControl(this).WorkingArea;
        using var form=new Form {Text=reserve?"Đặt phòng trước":"Nhận phòng trực tiếp",
            Size=new Size(Math.Min(1280,area.Width-30),Math.Min(900,area.Height-30)),
            MinimumSize=new Size(Math.Min(900,area.Width-30),Math.Min(640,area.Height-30)),StartPosition=FormStartPosition.CenterParent,
            AutoScaleMode=AutoScaleMode.Dpi,Font=AppTheme.Body,BackColor=AppTheme.Canvas};
        var root=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=1,RowCount=3};
        root.RowStyles.Add(new RowStyle(SizeType.Absolute,76));root.RowStyles.Add(new RowStyle(SizeType.Percent,100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute,66));form.Controls.Add(root);
        var header=new Panel {Dock=DockStyle.Fill,BackColor=Color.White,Padding=new Padding(24,13,20,7)};
        header.Controls.Add(new Label {Text=reserve?"Đặt phòng trước":"Nhận phòng trực tiếp",Dock=DockStyle.Top,Height=38,
            Font=AppTheme.Title,ForeColor=AppTheme.Ink});
        header.Controls.Add(new Label {Text=reserve
                ?"Chọn mã phòng, nhập thông tin khách và kiểm tra tiền cọc trước khi lưu."
                :"Chọn mã phòng và nhập thông tin khách để nhận phòng.",
            Dock=DockStyle.Bottom,Height=23,ForeColor=AppTheme.Muted});root.Controls.Add(header,0,0);
        var body=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=2,Padding=new Padding(16,14,16,10)};
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,49));body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,51));
        root.Controls.Add(body,0,1);
        var leftHost=new Panel {Dock=DockStyle.Fill,AutoScroll=true,BackColor=Color.White,Margin=new Padding(0,0,10,0)};
        var fields=new TableLayoutPanel {Dock=DockStyle.Top,AutoSize=true,ColumnCount=1,Padding=new Padding(18,14,18,14)};
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));leftHost.Controls.Add(fields);body.Controls.Add(leftHost,0,0);
        void AddField(string caption,Control control)
        {
            fields.RowCount++;fields.RowStyles.Add(new RowStyle(SizeType.Absolute,25));
            fields.Controls.Add(new Label {Text=caption,Dock=DockStyle.Fill,ForeColor=AppTheme.Muted,Font=AppTheme.Bold},0,fields.RowCount-1);
            fields.RowCount++;fields.RowStyles.Add(new RowStyle(SizeType.Absolute,42));
            control.Dock=DockStyle.Top;control.Height=32;fields.Controls.Add(control,0,fields.RowCount-1);
        }
        void Section(string text)
        {
            fields.RowCount++;fields.RowStyles.Add(new RowStyle(SizeType.Absolute,38));
            fields.Controls.Add(new Label {Text=text,Dock=DockStyle.Fill,Font=AppTheme.Bold,
                ForeColor=AppTheme.Blue,TextAlign=ContentAlignment.MiddleLeft},0,fields.RowCount-1);
        }
        var room=Ui.Combo(candidates);
        if(selected is not null)room.SelectedItem=candidates.Single(r=>r.Id==selected.Id);
        var name=Ui.Text();var phone=Ui.Text(20);var identity=Ui.Text(20);
        var arrival=Ui.DatePicker(requestedArrival??ServerNow.AddHours(2));var days=Ui.Number(60,requestedDays);
        var receiveBy=Ui.DatePicker(ServerNow.AddDays(1));
        Section("01  PHÒNG VÀ THỜI GIAN");AddField("Mã phòng",room);
        if(reserve)AddField("Ngày giờ dự kiến đến",arrival);
        AddField("Số ngày thuê dự kiến",days);
        if(reserve)AddField("Hạn cuối nhận phòng",receiveBy);
        Section("02  KHÁCH ĐỨNG TÊN");AddField("Họ và tên",name);AddField("Số điện thoại",phone);
        AddField("CCCD (12 số) / Hộ chiếu",identity);
        var right=new TableLayoutPanel {Dock=DockStyle.Fill,BackColor=Color.White,ColumnCount=1,RowCount=5,
            Padding=new Padding(18,15,18,12),Margin=new Padding(10,0,0,0)};
        right.RowStyles.Add(new RowStyle(SizeType.Absolute,reserve?86:100));
        right.RowStyles.Add(new RowStyle(SizeType.Absolute,reserve?170:86));
        right.RowStyles.Add(new RowStyle(SizeType.Absolute,0));
        right.RowStyles.Add(new RowStyle(SizeType.Percent,100));
        right.RowStyles.Add(new RowStyle(SizeType.Absolute,0));body.Controls.Add(right,1,0);
        var roomSummary=new Label {Dock=DockStyle.Fill,Font=AppTheme.Bold,ForeColor=AppTheme.Ink,
            BackColor=Color.FromArgb(239,245,251),Padding=new Padding(13,12,8,8)};right.Controls.Add(roomSummary,0,0);
        var deposit=new CheckBox {Text="Đã thu tiền cọc",AutoSize=true};
        var amount=Ui.Money();var method=Ui.Combo(new[]{"Tiền mặt","Chuyển khoản","Thẻ POS"});
        if(room.SelectedItem is Room initialRoom)amount.Value=Math.Min(amount.Maximum,initialRoom.Deposit);
        var reference=Ui.Text(100);reference.PlaceholderText="Nhập mã giao dịch QR/POS";
        var payment=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=1,RowCount=4};
        for(var i=0;i<4;i++)payment.RowStyles.Add(new RowStyle(SizeType.Absolute,i==0?32:45));
        payment.Controls.Add(deposit,0,0);
        var moneyRow=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=2};
        moneyRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,42));moneyRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,58));
        moneyRow.Controls.Add(new Label {Text="Cọc thực thu (đ)",Dock=DockStyle.Fill,TextAlign=ContentAlignment.MiddleLeft,ForeColor=AppTheme.Muted},0,0);
        moneyRow.Controls.Add(amount,1,0);payment.Controls.Add(moneyRow,0,1);
        payment.Controls.Add(method,0,2);payment.Controls.Add(reference,0,3);
        if(reserve)right.Controls.Add(payment,0,1);
        else right.Controls.Add(new Label {Text="NHẬN PHÒNG KHÔNG THU CỌC\nTiền phòng và dịch vụ sẽ được tính khi khách trả phòng.",
            Dock=DockStyle.Fill,Font=AppTheme.Bold,ForeColor=AppTheme.Ink,
            BackColor=Color.FromArgb(247,249,252),Padding=new Padding(13,13,8,5)},0,1);
        var qrArea=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=1,RowCount=2,
            BackColor=Color.FromArgb(247,249,252),Padding=new Padding(12,8,12,8)};
        qrArea.RowStyles.Add(new RowStyle(SizeType.Absolute,43));qrArea.RowStyles.Add(new RowStyle(SizeType.Percent,100));
        var qrInfo=new Label {Dock=DockStyle.Fill,Font=AppTheme.Bold,ForeColor=AppTheme.Ink};
        var qr=new PictureBox {Dock=DockStyle.Fill,SizeMode=PictureBoxSizeMode.Zoom,BackColor=Color.White,
            Margin=new Padding(12,0,12,2)};
        qr.LoadCompleted+=(_,e)=>
        {
            if(e.Error is not null && !form.IsDisposed)
                qrInfo.Text="Không tải được QR. Kiểm tra mạng hoặc dùng thông tin tài khoản để chuyển khoản.";
        };
        qrArea.Controls.Add(qrInfo,0,0);qrArea.Controls.Add(qr,0,1);
        if(reserve)right.Controls.Add(qrArea,0,3);
        else right.Controls.Add(new Label {Text="Khách có thể sử dụng dịch vụ trong thời gian lưu trú.\nBill sẽ tổng hợp tiền phòng và dịch vụ khi trả phòng.",
            Dock=DockStyle.Fill,Font=AppTheme.Bold,ForeColor=AppTheme.Muted,
            TextAlign=ContentAlignment.MiddleCenter},0,3);
        var settings=AppSettings.Load();
        var qrTimer=new System.Windows.Forms.Timer {Interval=450};
        qrTimer.Tick+=(_,_)=>
        {
            qrTimer.Stop();if(form.IsDisposed)return;
            var content=$"COC P{(room.SelectedItem as Room)?.Number} {phone.Text.Trim()}";
            var url=$"https://img.vietqr.io/image/{Uri.EscapeDataString(settings.BankCode)}-{Uri.EscapeDataString(settings.BankAccount)}-qr_only.png?amount={amount.Value:0}&addInfo={Uri.EscapeDataString(content)}&accountName={Uri.EscapeDataString(settings.BankAccountName)}";
            try{qr.CancelAsync();qr.LoadAsync(url);}
            catch(InvalidOperationException){qrInfo.Text="Không tải được QR. Kiểm tra kết nối mạng.";}
        };
        void Refresh()
        {
            var current=room.SelectedItem as Room;
            roomSummary.Text=current is null?"Không còn phòng trống trong thời gian đã chọn.":
                reserve
                    ?$"MÃ PHÒNG  P.{current.Number}  •  ID #{current.Id}\n{current.Type}  ·  {current.Rate:N0} đ/ngày\nCọc gợi ý: {current.Deposit:N0} đ"
                    :$"MÃ PHÒNG  P.{current.Number}  •  ID #{current.Id}\n{current.Type}  ·  {current.Rate:N0} đ/ngày\nThanh toán khi trả phòng";
            var paid=reserve && deposit.Checked;
            amount.Enabled=paid;method.Enabled=paid;reference.Enabled=paid && (string?)method.SelectedItem!="Tiền mặt";
            var show=paid && amount.Value>0 && (string?)method.SelectedItem=="Chuyển khoản";
            if(show)
            {
                var configured=!string.IsNullOrWhiteSpace(settings.BankCode) &&
                    !string.IsNullOrWhiteSpace(settings.BankAccount) &&
                    !string.IsNullOrWhiteSpace(settings.BankAccountName);
                qrInfo.Text=!configured
                    ?"Chưa cấu hình tài khoản ngân hàng.":$"QUÉT QR ĐỂ CHUYỂN CỌC  ·  {amount.Value:N0} đ\nSTK {settings.BankAccount}  ·  {settings.BankAccountName}";
                qr.Visible=configured;
                if(configured)
                {qrTimer.Stop();qrTimer.Start();}
            }
            else
            {
                qrInfo.Text=paid?"Chọn Chuyển khoản để hiển thị mã QR tại đây.":
                    "Chưa thu cọc: giữ phòng tối đa 24 giờ.\nĐã thu cọc: giữ tối đa 15 ngày; quá hạn nhận phòng không hoàn cọc.";
                qr.Visible=false;qrTimer.Stop();qr.CancelAsync();
            }
        }
        void FilterRooms()
        {
            var previous=(room.SelectedItem as Room)?.Id;
            var from=reserve?arrival.Value:ServerNow;var until=from.AddDays((int)days.Value);
            var available=candidates.Where(r=>!data.Stays.Any(s=>s.RoomId==r.Id &&
                (s.CheckIn??s.Arrival)<until && s.Departure>from));
            if(selected is not null)available=available.Where(r=>r.Id==selected.Id);
            room.DataSource=available.ToList();
            if(previous is { } id && room.Items.Cast<Room>().FirstOrDefault(r=>r.Id==id) is { } old)room.SelectedItem=old;
            Refresh();
        }
        void UpdateHold()
        {
            if(!reserve)return;
            var limit=HotelService.ReservationHoldLimit(ServerNow,deposit.Checked && amount.Value>0);
            var departure=arrival.Value.AddDays((int)days.Value);
            receiveBy.Value=limit<departure?limit:departure.AddMinutes(-1);
        }
        room.SelectedIndexChanged+=(_,_)=>
        {
            if(room.SelectedItem is Room r && amount.Value==0)amount.Value=Math.Min(amount.Maximum,r.Deposit);
            Refresh();
        };
        arrival.ValueChanged+=(_,_)=>{FilterRooms();UpdateHold();};
        days.ValueChanged+=(_,_)=>{FilterRooms();UpdateHold();};
        deposit.CheckedChanged+=(_,_)=>
        {
            if(deposit.Checked && amount.Value==0 && room.SelectedItem is Room chosen)
                amount.Value=Math.Min(amount.Maximum,chosen.Deposit);
            UpdateHold();Refresh();
        };
        amount.ValueChanged+=(_,_)=>{UpdateHold();Refresh();};
        method.SelectedIndexChanged+=(_,_)=>Refresh();phone.TextChanged+=(_,_)=>Refresh();
        FilterRooms();UpdateHold();Refresh();
        var footer=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=2,Padding=new Padding(16,12,16,12),BackColor=Color.White};
        footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,32));footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,68));
        var cancel=new Button {Text="QUAY LẠI",Dock=DockStyle.Fill,Margin=new Padding(0,0,8,0)};
        var save=new Button {Text=reserve?"LƯU ĐẶT PHÒNG":"XÁC NHẬN NHẬN PHÒNG",Dock=DockStyle.Fill,Margin=new Padding(8,0,0,0)};
        AppTheme.Button(cancel);AppTheme.Button(save,true);footer.Controls.Add(cancel,0,0);footer.Controls.Add(save,1,0);
        root.Controls.Add(footer,0,2);cancel.Click+=(_,_)=>form.Close();
        save.Click+=async (_,_)=>
        {
            if(room.SelectedItem is not Room chosen){Ui.Error(form,new BusinessException("Chưa chọn mã phòng còn trống."));return;}
            if(reserve && deposit.Checked && amount.Value==0){Ui.Error(form,new BusinessException("Nhập số tiền cọc thực thu lớn hơn 0 hoặc bỏ chọn đã thu tiền cọc."));return;}
            save.Enabled=false;
            try
            {
                await Changed(()=>service.CreateStayAsync(chosen,new GuestInput(name.Text,phone.Text,identity.Text),reserve,
                    arrival.Value,(int)days.Value,reserve && deposit.Checked,(string)method.SelectedItem!,
                    reserve && deposit.Checked?amount.Value:0,reserve?receiveBy.Value:null,reference.Text));
                form.DialogResult=DialogResult.OK;form.Close();
            }
            catch(Exception ex){Ui.Error(form,ex);save.Enabled=true;}
        };
        form.FormClosed+=(_,_)=>{qrTimer.Stop();qrTimer.Dispose();qr.CancelAsync();};
        form.AcceptButton=save;form.ShowDialog(this);
        return Task.CompletedTask;
    }
}
