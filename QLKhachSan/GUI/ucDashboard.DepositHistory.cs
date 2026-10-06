using QLKhachSan.DTO;

namespace QLKhachSan.GUI;

public partial class ucDashboard
{
    private async Task ShowCustomerDepositHistory(long stayId)
    {
        var selected=data.Stays.FirstOrDefault(x=>x.Id==stayId) ??
            (await service.StayHistoryAsync("")).FirstOrDefault(x=>x.Id==stayId) ??
            throw new BusinessException("Không tìm thấy lượt đặt. Hãy làm mới danh sách.");
        var history=await service.DepositHistoryAsync(selected.CustomerId);
        using var form=new Form {Text=$"Lịch sử đặt cọc · {selected.Guest}",Size=new Size(1180,790),
            MinimumSize=new Size(920,630),StartPosition=FormStartPosition.CenterParent,Font=AppTheme.Body,BackColor=AppTheme.Canvas};
        var root=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=1,RowCount=4,Padding=new Padding(18)};
        root.RowStyles.Add(new RowStyle(SizeType.Absolute,93));root.RowStyles.Add(new RowStyle(SizeType.Percent,52));
        root.RowStyles.Add(new RowStyle(SizeType.Percent,48));root.RowStyles.Add(new RowStyle(SizeType.Absolute,54));form.Controls.Add(root);
        var roomNumbers=history.Stays.Select(s=>data.Rooms.FirstOrDefault(r=>r.Id==s.RoomId)?.Number??$"#{s.RoomId}").Distinct().ToArray();
        var deposits=history.Payments.Where(p=>p.Kind=="Deposit").Sum(p=>p.Amount);
        var refunds=history.Payments.Where(p=>p.Kind=="Refund").Sum(p=>p.Amount);
        var heading=new Label {Text=$"{selected.Guest}  ·  {selected.Phone}\n{history.Stays.Count} lượt / {roomNumbers.Length} phòng  •  Đã thu cọc {deposits:N0} đ  •  Đã hoàn {refunds:N0} đ",
            Dock=DockStyle.Fill,Font=AppTheme.Bold,ForeColor=AppTheme.Ink,BackColor=Color.White,Padding=new Padding(18,12,0,0)};
        root.Controls.Add(heading,0,0);
        TableLayoutPanel Section(string title,int row,out DataGridView grid)
        {
            var card=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=1,RowCount=2,BackColor=Color.White,
                Padding=new Padding(12),Margin=new Padding(0,10,0,0)};
            card.RowStyles.Add(new RowStyle(SizeType.Absolute,32));card.RowStyles.Add(new RowStyle(SizeType.Percent,100));
            card.Controls.Add(new Label {Text=title,Dock=DockStyle.Fill,Font=AppTheme.Bold,ForeColor=AppTheme.Blue},0,0);
            grid=Ui.Grid();grid.Dock=DockStyle.Fill;grid.AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill;
            card.Controls.Add(grid,0,1);root.Controls.Add(card,0,row);return card;
        }
        Section("CÁC LƯỢT ĐẶT / LƯU TRÚ",1,out var stays);
        stays.DataSource=history.Stays.Select(s=>new {MãLượt=s.Id,Phòng=data.Rooms.FirstOrDefault(r=>r.Id==s.RoomId)?.Number??"?",
            Khách=s.Guest,NgàyĐến=s.Arrival,NgàyTrả=s.Departure,TrạngThái=s.Status switch
            {StayStatus.Reserved=>"Chờ nhận",StayStatus.Occupied=>"Đang ở",StayStatus.Paid=>"Đã trả",_=>"Đã hủy"},
            CọcĐãThu=s.Deposit,HạnNhận=s.HoldUntil}).ToList();
        foreach(DataGridViewColumn c in stays.Columns)
        {
            if(c.Name is "NgàyĐến" or "NgàyTrả" or "HạnNhận")c.DefaultCellStyle.Format="dd/MM/yyyy HH:mm";
            if(c.Name=="CọcĐãThu")c.DefaultCellStyle.Format="N0";
        }
        Section("CHI TIẾT THU CỌC / HOÀN CỌC",2,out var payments);
        payments.DataSource=history.Payments.Where(p=>p.Kind is "Deposit" or "Refund" or "Forfeit")
            .Select(p=>new {ThờiĐiểm=p.Created,MãLượt=p.StayId,Phòng=data.Rooms.FirstOrDefault(r=>r.Id==history.Stays.First(s=>s.Id==p.StayId).RoomId)?.Number,
                Loại=p.Kind switch {"Deposit"=>"Thu cọc","Refund"=>"Hoàn cọc",_=>"Cọc không hoàn"},
                SốTiền=p.Amount,PhươngThức=p.Method,GhiChú=p.Note,NhânViên=p.Username}).ToList();
        if(payments.Columns["ThờiĐiểm"] is { } when)when.DefaultCellStyle.Format="dd/MM/yyyy HH:mm";
        if(payments.Columns["SốTiền"] is { } amount)amount.DefaultCellStyle.Format="N0";
        var close=new Button {Text="ĐÓNG",Dock=DockStyle.Right,Width=150,Margin=new Padding(0,10,0,0)};
        AppTheme.Button(close);close.Click+=(_,_)=>form.Close();root.Controls.Add(close,0,3);
        form.ShowDialog(this);
    }
}
