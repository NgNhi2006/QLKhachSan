using QLKhachSan.DTO;
using QLKhachSan.DAL;
using QLKhachSan.BLL;

namespace QLKhachSan.GUI;

public partial class ucDashboard
{
    private sealed record StayChoice(Stay Stay,string Label) {public override string ToString()=>Label;}
    private ComboBox StayCombo() => Ui.Combo(data.Stays.Where(s=>s.Status==StayStatus.Occupied).Select(s=>new StayChoice(s,$"P.{StayRoom(s)?.Number} — {s.Guest}")));
    private sealed class StaySelectCard : Control
    {
        public StayChoice Choice {get;}
        public string RoomNumber {get;}
        public string RoomCode {get;}
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public bool Selected {get;set;}
        public StaySelectCard(StayChoice choice,string roomNumber,string roomCode)
        {
            Choice=choice;RoomNumber=roomNumber;RoomCode=roomCode;Height=76;Cursor=Cursors.Hand;
            SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|
                ControlStyles.OptimizedDoubleBuffer|ControlStyles.ResizeRedraw|ControlStyles.StandardDoubleClick,true);
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            var g=e.Graphics;g.SmoothingMode=System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using var shape=AppTheme.Rounded(new RectangleF(1,1,Width-3,Height-3),12);
            using var fill=new SolidBrush(Selected?Color.FromArgb(229,242,231):Color.White);
            using var border=new Pen(Selected?AppTheme.Blue:AppTheme.Border,Selected?2:1);
            g.FillPath(fill,shape);g.DrawPath(border,shape);
            using var roomShape=AppTheme.Rounded(new RectangleF(14,17,86,40),9);
            using var roomFill=new SolidBrush(Color.FromArgb(235,241,232));
            g.FillPath(roomFill,roomShape);
            TextRenderer.DrawText(g,$"P.{RoomNumber}",AppTheme.Bold,new Rectangle(17,18,80,38),
                AppTheme.Blue,TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis);
            TextRenderer.DrawText(g,Choice.Stay.Guest,AppTheme.Bold,new Rectangle(115,14,Math.Max(60,Width-245),25),
                AppTheme.Ink,TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis);
            TextRenderer.DrawText(g,$"{Choice.Stay.Phone}   ·   {RoomCode}",AppTheme.Small,new Rectangle(115,41,Math.Max(60,Width-245),20),
                AppTheme.Muted,TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis);
            TextRenderer.DrawText(g,Selected?"ĐÃ CHỌN":"ĐANG Ở",AppTheme.Small,new Rectangle(Width-116,27,100,22),
                AppTheme.Blue,TextFormatFlags.Right|TextFormatFlags.VerticalCenter);
        }
    }
    private Stay? SelectStay(string title)
    {
        var occupied=data.Stays.Where(s=>s.Status==StayStatus.Occupied)
            .OrderBy(s=>StayRoom(s)?.Number).Select(s=>new StayChoice(s,$"P.{StayRoom(s)?.Number}  •  {s.Guest}  •  {s.Phone}  •  {StayRoom(s)?.Code}"))
            .ToArray();
        if(occupied.Length==0)throw new BusinessException("Không có phòng đang ở.");
        var forService=title.Contains("dịch vụ",StringComparison.OrdinalIgnoreCase);
        var preferredHeight=Math.Min(650,Math.Max(460,380+occupied.Length*82));
        using var dialog=new Form {Text=title,Size=new Size(720,Math.Min(preferredHeight,Screen.FromControl(this).WorkingArea.Height-40)),
            MinimumSize=new Size(580,460),StartPosition=FormStartPosition.CenterParent,
            AutoScaleMode=AutoScaleMode.Dpi,Font=AppTheme.Body,BackColor=AppTheme.Canvas};
        var root=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=1,RowCount=3};
        root.RowStyles.Add(new RowStyle(SizeType.Absolute,92));
        root.RowStyles.Add(new RowStyle(SizeType.Percent,100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute,78));dialog.Controls.Add(root);
        var header=new Panel {Dock=DockStyle.Fill,BackColor=Color.White};root.Controls.Add(header,0,0);
        header.Paint+=(_,e)=>
        {
            using var accent=new SolidBrush(AppTheme.Focus);
            using var edge=new Pen(AppTheme.Border);
            e.Graphics.FillRectangle(accent,24,15,42,3);
            e.Graphics.DrawLine(edge,0,header.Height-1,header.Width,header.Height-1);
        };
        header.Controls.Add(new Label {Text=title,Dock=DockStyle.Top,Height=48,Padding=new Padding(24,23,20,0),
            Font=AppTheme.Title,ForeColor=AppTheme.Navy,AutoEllipsis=true});
        header.Controls.Add(new Label {Text=forService?"Chọn phòng đang ở để xử lý yêu cầu dịch vụ":"Chọn phòng đang ở để tiếp tục thanh toán",
            Dock=DockStyle.Bottom,Height=28,
            Padding=new Padding(25,0,20,0),ForeColor=AppTheme.Muted});
        var body=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=1,RowCount=4,
            Padding=new Padding(24,16,24,14),BackColor=AppTheme.Canvas};root.Controls.Add(body,0,1);
        body.RowStyles.Add(new RowStyle(SizeType.Absolute,25));
        body.RowStyles.Add(new RowStyle(SizeType.Absolute,46));
        body.RowStyles.Add(new RowStyle(SizeType.Absolute,34));
        body.RowStyles.Add(new RowStyle(SizeType.Percent,100));
        body.Controls.Add(new Label {Text="TÌM PHÒNG HOẶC KHÁCH",Dock=DockStyle.Fill,Font=AppTheme.Bold,
            ForeColor=AppTheme.Muted},0,0);
        var search=Ui.Text();search.PlaceholderText="Số phòng, tên khách hoặc số điện thoại";
        search.BorderStyle=BorderStyle.None;search.Dock=DockStyle.Fill;
        var searchBox=new Panel {Dock=DockStyle.Fill,BackColor=Color.White,
            Padding=new Padding(13,11,12,7),Margin=new Padding(0,0,0,5)};
        searchBox.Controls.Add(search);body.Controls.Add(searchBox,0,1);
        searchBox.Paint+=(_,e)=>
        {
            e.Graphics.SmoothingMode=System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using var border=new Pen(search.Focused?AppTheme.Blue:AppTheme.Border,search.Focused?2:1);
            using var shape=AppTheme.Rounded(new RectangleF(1,1,searchBox.Width-3,searchBox.Height-3),10);
            e.Graphics.DrawPath(border,shape);
        };
        search.GotFocus+=(_,_)=>searchBox.Invalidate();search.LostFocus+=(_,_)=>searchBox.Invalidate();
        var count=new Label {Dock=DockStyle.Fill,ForeColor=AppTheme.Muted,Padding=new Padding(0,7,0,0)};
        body.Controls.Add(count,0,2);
        var list=new FlowLayoutPanel {Dock=DockStyle.Fill,AutoScroll=true,WrapContents=false,
            FlowDirection=FlowDirection.TopDown,Margin=Padding.Empty,Padding=new Padding(0,0,5,0)};
        body.Controls.Add(list,0,3);
        var footer=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=2,
            Padding=new Padding(24,14,24,16),BackColor=Color.White};root.Controls.Add(footer,0,2);
        footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,36));
        footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,64));
        var cancel=new Button {Text="HỦY",Dock=DockStyle.Fill,Margin=new Padding(0,0,6,0)};
        var select=new Button {Text=forService?"CHỌN LƯỢT Ở  →":"TIẾP TỤC THANH TOÁN  →",
            Dock=DockStyle.Fill,Margin=new Padding(6,0,0,0)};
        AppTheme.Button(cancel);AppTheme.Button(select,true);
        footer.Controls.Add(cancel,0,0);footer.Controls.Add(select,1,0);
        StayChoice? selected=null;
        void Choose(StaySelectCard card)
        {
            selected=card.Choice;
            foreach(var item in list.Controls.OfType<StaySelectCard>())
            {item.Selected=item==card;item.Invalidate();}
            select.Enabled=true;
        }
        void ResizeCards()
        {
            foreach(var card in list.Controls.OfType<StaySelectCard>())
                card.Width=Math.Max(200,list.ClientSize.Width-(list.VerticalScroll.Visible?26:8));
        }
        void Filter()
        {
            var query=search.Text.Trim();
            var matches=occupied.Where(x=>query.Length==0 || x.Label.Contains(query,StringComparison.CurrentCultureIgnoreCase)).ToArray();
            list.SuspendLayout();
            var previous=list.Controls.Cast<Control>().ToArray();list.Controls.Clear();
            foreach(var old in previous)old.Dispose();
            foreach(var choice in matches)
            {
                var card=new StaySelectCard(choice,StayRoom(choice.Stay)?.Number??"?",StayRoom(choice.Stay)?.Code??"")
                {Width=Math.Max(200,list.ClientSize.Width-26),Margin=new Padding(0,0,0,8)};
                card.Click+=(_,_)=>Choose(card);
                card.DoubleClick+=(_,_)=>{Choose(card);dialog.DialogResult=DialogResult.OK;dialog.Close();};
                list.Controls.Add(card);
            }
            list.ResumeLayout();ResizeCards();
            selected=null;select.Enabled=false;
            if(matches.Length==1)Choose((StaySelectCard)list.Controls[0]);
            count.Text=$"{matches.Length} phòng phù hợp";
        }
        search.TextChanged+=(_,_)=>Filter();
        search.KeyDown+=(_,e)=>
        {
            if(e.KeyCode==Keys.Down && list.Controls.Count>0){list.Controls[0].Focus();e.Handled=true;}
        };
        list.Resize+=(_,_)=>ResizeCards();
        select.Click+=(_,_)=>{if(selected is not null){dialog.DialogResult=DialogResult.OK;dialog.Close();}};
        cancel.Click+=(_,_)=>dialog.Close();
        dialog.AcceptButton=select;dialog.CancelButton=cancel;
        Filter();dialog.Shown+=(_,_)=>search.Focus();
        return dialog.ShowDialog(this)==DialogResult.OK?selected?.Stay:null;
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
    private async void btnGoiDichVu_Click(object? sender,EventArgs e)=>await Run(async()=>
    {
        var availability=(await service.ServiceStockAvailabilityAsync()).ToDictionary(x=>x.ServiceId);
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
            UpdateSelectedService();
        }
        void UpdateSelectedService()
        {
            if(items.SelectedItem is not ServiceItem item)
            {selected.Text="Không tìm thấy dịch vụ phù hợp.";return;}
            var stock=availability.GetValueOrDefault(item.Id);
            selected.Text=stock?.Available is decimal available
                ? $"{item.Name}  •  {item.Price:N0} đ/{item.Unit}\n{stock.Status}: còn gọi {Math.Max(0,available):N0} {stock.Unit} (đã giữ/đang thuê {stock.Reserved:N0})"
                : $"{item.Name}  •  {item.Price:N0} đ/{item.Unit}\nChưa theo dõi kho";
            selected.ForeColor=stock?.Status=="Hết hàng"?Color.Firebrick:AppTheme.Ink;
        }
        categories.SelectedIndexChanged+=(_,_)=>FilterServices();search.TextChanged+=(_,_)=>FilterServices();
        items.SelectedIndexChanged+=(_,_)=>UpdateSelectedService();
        var cart=new List<(Stay Stay,ServiceItem Item,int Quantity)>();
        var list=new ListBox {Dock=DockStyle.Fill,Font=AppTheme.Body,HorizontalScrollbar=true};
        var total=new Label {Dock=DockStyle.Fill,Font=AppTheme.Bold,ForeColor=AppTheme.Ink,TextAlign=ContentAlignment.MiddleRight};
        void UpdateCart()
        {
            list.DataSource=null;
            list.DataSource=cart.Select(x=>$"P.{StayRoom(x.Stay)?.Number}  •  {x.Item.Name} × {x.Quantity} = {x.Item.Price*x.Quantity:N0} đ").ToList();
            list.HorizontalExtent=cart.Count==0?0:cart.Max(x=>TextRenderer.MeasureText($"P.{StayRoom(x.Stay)?.Number}  •  {x.Item.Name} × {x.Quantity} = {x.Item.Price*x.Quantity:N0} đ",list.Font).Width)+12;
            total.Text=$"{cart.Select(x=>x.Stay.Id).Distinct().Count()} phòng  •  Tạm tính: {cart.Sum(x=>x.Item.Price*x.Quantity):N0} đ";
        }
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
        Field("Phòng nhận cho dịch vụ đang thêm",stay,1);Field("Danh mục",categories,3);Field("Tìm dịch vụ",search,5);
        pick.Controls.Add(items,0,7);pick.Controls.Add(selected,0,8);Field("Số lượng",quantity,9);
        var add=new Button {Text="+ THÊM VÀO YÊU CẦU",Dock=DockStyle.Fill};AppTheme.Button(add,true);
        add.Click+=(_,_)=>
        {
            if(items.SelectedItem is not ServiceItem item || stay.SelectedItem is not StayChoice room)return;
            if(availability.GetValueOrDefault(item.Id)?.Available is decimal available &&
                cart.Where(x=>x.Item.Id==item.Id).Sum(x=>x.Quantity)+quantity.Value>available)
            {Ui.Error(dialog,new BusinessException($"{item.Name} chỉ còn {Math.Max(0,available):N0} {item.Unit} có thể gọi cho tất cả phòng."));return;}
            var index=cart.FindIndex(x=>x.Stay.Id==room.Stay.Id && x.Item.Id==item.Id);
            if(index>=0)
            {
                var current=cart[index];
                if(current.Quantity+quantity.Value>100)
                {Ui.Error(dialog,new BusinessException("Mỗi dịch vụ của một phòng tối đa 100 đơn vị."));return;}
                cart[index]=(current.Stay,current.Item,current.Quantity+(int)quantity.Value);
            }
            else if(cart.Count<100)cart.Add((room.Stay,item,(int)quantity.Value));
            UpdateCart();
        };
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
        dialog.Action("GỬI YÊU CẦU",async()=>
        {
            await Changed(()=>service.AddServicesForRoomsAsync(cart
                .Select(x=>new OrderAssignment(x.Stay,x.Item.Id,x.Quantity)).ToArray()));
        });
        dialog.ShowDialog(this);
    });
}


