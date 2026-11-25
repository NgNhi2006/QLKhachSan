using QLKhachSan.BLL;
using QLKhachSan.DTO;

namespace QLKhachSan.GUI;

public partial class ucDashboard
{
    private ComboBox? bookingDetailChoice;
    private Label? bookingDetailText;
    private Button? bookingDetailEdit;
    private Button? bookingDetailDelete;
    private readonly HashSet<long> recentBookingIds=[];
    private void ShowSavedBookingWindow()
    {
        BeginInvoke(new Action(() =>
        {
            if(IsDisposed)return;
            using var window=new Form {Text="Thông tin khách vừa nhập",StartPosition=FormStartPosition.CenterParent,
                FormBorderStyle=FormBorderStyle.FixedDialog,MaximizeBox=false,MinimizeBox=false,
                ClientSize=new Size(530,485),BackColor=AppTheme.Canvas,Font=AppTheme.Body};
            var addRequested=false;
            var view=BuildBookingDetailView(() => {addRequested=true;window.Close();});
            view.Dock=DockStyle.Fill;window.Controls.Add(view);
            RefreshBookingDetail();
            window.ShowDialog(this);
            if(addRequested)ShowBookingScreen(true);
        }));
    }

    private sealed record BookingDetailChoice(Stay Stay,string RoomNumber)
    {
        public override string ToString()=>$"Phòng {RoomNumber} · {Stay.Guest}";
    }

    private Panel BuildBookingDetailView(Action addBooking)
    {
        var page=new Panel {BackColor=AppTheme.Canvas,Padding=new Padding(22)};
        var layout=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=1,RowCount=4,
            BackColor=AppTheme.Canvas};
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute,58));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute,52));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent,100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute,64));
        page.Controls.Add(layout);
        layout.Controls.Add(new Label {Text="THÔNG TIN KHÁCH VỪA NHẬP",Dock=DockStyle.Fill,
            Font=AppTheme.TextFont(17F,FontStyle.Bold),ForeColor=AppTheme.Navy,
            TextAlign=ContentAlignment.MiddleLeft},0,0);
        bookingDetailChoice=new ComboBox {Dock=DockStyle.Left,Width=310,DropDownStyle=ComboBoxStyle.DropDownList,
            Font=AppTheme.Body};
        bookingDetailChoice.SelectedIndexChanged+=(_,_)=>RefreshBookingDetailText();
        layout.Controls.Add(bookingDetailChoice,0,1);
        var card=new Panel {Dock=DockStyle.Fill,BackColor=Color.White,Padding=new Padding(24),
            Margin=new Padding(0,4,0,10)};
        bookingDetailText=new Label {Dock=DockStyle.Fill,Font=AppTheme.TextFont(11F),
            ForeColor=AppTheme.Ink,TextAlign=ContentAlignment.TopLeft};
        card.Controls.Add(bookingDetailText);layout.Controls.Add(card,0,2);
        var toolbar=new FlowLayoutPanel {Dock=DockStyle.Fill,BackColor=AppTheme.Canvas,
            WrapContents=false,FlowDirection=FlowDirection.LeftToRight};
        layout.Controls.Add(toolbar,0,3);
        var tips=new ToolTip();page.Disposed+=(_,_)=>tips.Dispose();
        Button IconButton(string icon,string label,bool primary=false)
        {
            var button=new Button {Size=new Size(48,46),Margin=new Padding(0,4,12,4),
                Image=UiIcons.Create(icon,primary?Color.White:AppTheme.Blue,24),
                ImageAlign=ContentAlignment.MiddleCenter,AccessibleName=label,
                Cursor=Cursors.Hand,FlatStyle=FlatStyle.Flat,
                BackColor=primary?AppTheme.Blue:Color.FromArgb(230,241,235)};
            button.FlatAppearance.BorderSize=0;
            button.FlatAppearance.MouseOverBackColor=primary?AppTheme.Navy:Color.FromArgb(211,231,219);
            void Round(object? _,EventArgs __)
            {
                var old=button.Region;
                using var shape=AppTheme.Rounded(new RectangleF(0,0,button.Width,button.Height),10);
                button.Region=new Region(shape);old?.Dispose();
            }
            button.Resize+=Round;Round(button,EventArgs.Empty);
            button.Disposed+=(_,_)=>button.Image?.Dispose();
            tips.SetToolTip(button,label);toolbar.Controls.Add(button);return button;
        }
        var add=IconButton("plus","Thêm thông tin đặt phòng",true);
        bookingDetailEdit=IconButton("pencil","Sửa thông tin");
        bookingDetailDelete=IconButton("trash","Xóa thông tin");
        add.Enabled=FunctionPolicy.Can(user,"room.reserve");
        add.Click+=(_,_)=>addBooking();
        bookingDetailEdit.Click+=async (_,_)=>await EditBookingDetail();
        bookingDetailDelete.Click+=async (_,_)=>await DeleteBookingDetail();
        RefreshBookingDetail();
        return page;
    }

    private Stay? SelectedBookingDetail()=>bookingDetailChoice?.SelectedItem is BookingDetailChoice item
        ?data.Stays.FirstOrDefault(x=>x.Id==item.Stay.Id):null;

    private void RefreshBookingDetail()
    {
        if(bookingDetailChoice is not {IsDisposed:false} choice)return;
        var selected=SelectedBookingDetail()?.Id;
        var stays=data.Stays.Where(x=>recentBookingIds.Contains(x.Id) &&
            x.Status is StayStatus.Reserved or StayStatus.Occupied).ToArray();
        choice.BeginUpdate();choice.Items.Clear();
        foreach(var stay in stays)
            choice.Items.Add(new BookingDetailChoice(stay,StayRoom(stay)?.Number??"?"));
        var index=Array.FindIndex(stays,x=>x.Id==selected);
        choice.SelectedIndex=stays.Length==0?-1:Math.Max(0,index);
        choice.EndUpdate();
        RefreshBookingDetailText();
    }

    private void RefreshBookingDetailText()
    {
        if(bookingDetailText is not {IsDisposed:false} detail)return;
        var stay=SelectedBookingDetail();
        detail.Text=stay is null?"Chưa có thông tin vừa nhập. Nhấn + để thêm đặt phòng.":
            $"{(stay.Status==StayStatus.Reserved?"ĐẶT PHÒNG":"NHẬN PHÒNG")} THÀNH CÔNG\n\n"+
            $"Khách hàng: {stay.Guest}\nĐiện thoại: {stay.Phone}\nCCCD / Hộ chiếu: {stay.Identity}\n"+
            $"Phòng: {StayRoom(stay)?.Number}\nNgày đến: {stay.Arrival:dd/MM/yyyy HH:mm}\n"+
            $"Ngày đi: {stay.Departure:dd/MM/yyyy HH:mm}\nCọc đã thu: {stay.Deposit:N0} đ\n"+
            $"Trạng thái: {(stay.Status==StayStatus.Reserved?"Chờ nhận phòng":"Đang ở")}";
        if(bookingDetailEdit is {IsDisposed:false} edit)
            edit.Enabled=stay is not null && FunctionPolicy.Can(user,"room.booking_edit");
        if(bookingDetailDelete is {IsDisposed:false} delete)
            delete.Enabled=stay is not null && FunctionPolicy.Can(user,"room.booking_cancel");
    }

    private async Task EditBookingDetail()
    {
        var stay=SelectedBookingDetail();if(stay is null)return;
        await Run(()=>stay.Status==StayStatus.Occupied?ShowEditOccupiedGuest(stay):ShowEditBooking(stay));
        RefreshBookingDetail();
    }

    private async Task DeleteBookingDetail()
    {
        var stay=SelectedBookingDetail();if(stay is null)return;
        if(stay.Status==StayStatus.Reserved)await Run(()=>ShowCancel(stay));
        else
        {
            var answer=MessageBox.Show(this,
                $"Hủy lượt nhận phòng {StayRoom(stay)?.Number} của {stay.Guest} và trả phòng về trạng thái trống?\nChỉ thực hiện được khi chưa phát sinh cọc, dịch vụ hoặc hóa đơn.",
                "Xóa thông tin vừa nhập",MessageBoxButtons.YesNo,MessageBoxIcon.Warning);
            if(answer!=DialogResult.Yes)return;
            await Run(()=>Changed(()=>service.CancelUnbilledOccupiedAsync(stay)));
        }
        if(data.Stays.All(x=>x.Id!=stay.Id || x.Status is not (StayStatus.Reserved or StayStatus.Occupied)))
            recentBookingIds.Remove(stay.Id);
        RefreshBookingDetail();
    }

    private Task ShowEditOccupiedGuest(Stay stay)
    {
        using var dialog=new InputDialog("Sửa thông tin khách đang ở",650,500,true);
        var name=Ui.Text();name.Text=stay.Guest;
        var phone=Ui.Text(20);phone.Text=stay.Phone;
        var identity=Ui.Text(20);identity.Text=stay.Identity;
        var reason=Ui.Text(300);
        dialog.Add("Họ tên",name);dialog.Add("Số điện thoại",phone);
        dialog.Add("CCCD / Hộ chiếu",identity);dialog.Add("Lý do thay đổi",reason);
        dialog.Note($"Phòng {StayRoom(stay)?.Number} đang có khách. Chỉ sửa thông tin khách; ngày ở và tiền phòng được giữ nguyên.");
        dialog.Action("LƯU THAY ĐỔI",async()=>await Changed(()=>service.UpdateOccupiedGuestAsync(stay,
            new GuestInput(name.Text,phone.Text,identity.Text),reason.Text)));
        dialog.ShowDialog(this);
        return Task.CompletedTask;
    }

}

