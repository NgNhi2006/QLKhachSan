using System.ComponentModel;
using QLKhachSan.BLL;
using QLKhachSan.DAL;
using QLKhachSan.DTO;

namespace QLKhachSan.GUI;

public partial class ucDashboard : UserControl
{
    private readonly HotelService service;
    private readonly AuthService auth;
    private readonly UserSession user;
    private readonly System.Windows.Forms.Timer clock=new() {Interval=1000};
    private readonly System.Windows.Forms.Timer refreshTimer=new() {Interval=60000};
    private readonly Dictionary<int,Button> roomButtons=[];
    private DashboardData data=new([],[],[],[],[]);
    private bool busy;
    private bool loaded;
    private DateTime lastDate=DateTime.Today;
    private RevenueOverview? revenueOverview;
    private PeriodReport? accountingToday;
    private readonly System.Diagnostics.Stopwatch serverElapsed = new();
    private DateTime ServerNow => data.ServerNow==default ? DateTime.Now : data.ServerNow+serverElapsed.Elapsed;
    public bool IsBusy=>busy;
    public event EventHandler? LogoutRequested;

    public ucDashboard(UserSession user)
    {
        this.user=user;
        var repository=new HotelRepository();service=new HotelService(repository,user);auth=new AuthService(repository);
        InitializeComponent();
        pnlLeftTools.AutoScroll=true;
        lblClock.Text=DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
        components??=new Container(); components.Add(clock);components.Add(refreshTimer);
        clock.Tick+=(_,_)=>lblClock.Text=ServerNow.ToString("dd/MM/yyyy HH:mm:ss");
        refreshTimer.Tick+=async (_,_)=>
        {
            if(busy || !Visible)return;
            await Run(async()=> {if(RolePolicy.CanOperate(user.Role))await service.ExpireReservationsAsync();await Reload();},true);
        };
        Load+=async (_,_)=>
        {
            if(loaded)return;loaded=true;
            ReflowSidebar();
            clock.Start();refreshTimer.Start();
            await Run(async()=> {if(RolePolicy.CanOperate(user.Role))await service.ExpireReservationsAsync();await Reload();});
        };
        cboBoLocLich.SelectedIndex=0;
        lblHeaderTitle.Text=$"QUẢN LÝ KHÁCH SẠN • {user.Username} ({RolePolicy.Name(user.Role)})";
        lblCard5Title.Text="DOANH THU HÔM NAY";lblCard5Sub.Text="Hóa đơn + cọc không hoàn";
        tabMatrix.Text="SƠ ĐỒ PHÒNG";lblLichTitle.Text="LỊCH ĐẾN / ĐI HÔM NAY";
        tabThongKe.Text="DOANH THU";tabLichTrinh.Text="LỊCH ĐẾN / ĐI";
        lblCard4Sub.Text="Dự kiến trả trong ngày";btnDatLichPhong.Text="Đặt trước / Giữ chỗ";
        btnBaoTri.Visible=RolePolicy.CanMaintainRooms(user.Role);
        var operatorAccess=RolePolicy.CanOperate(user.Role);
        foreach(var button in new[]{btnCheckIn,btnDatLichPhong,btnGoiDichVu,btnBaoDonXong,btnDoiPhong,btnGiaHan})button.Visible=operatorAccess;
        btnTimPhong.Visible=operatorAccess;txtTimPhong.Visible=operatorAccess;
        btnQuanLyKhach.Visible=RolePolicy.CanViewOperations(user.Role);
        if(!RolePolicy.CanViewOperations(user.Role)){tabMainView.TabPages.Remove(tabMatrix);tabMainView.TabPages.Remove(tabLichTrinh);pnlRight.Visible=false;}
        if(!RolePolicy.CanViewOperations(user.Role))tlpCards.Visible=true;
        if(!RolePolicy.CanViewFinance(user.Role)){tabMainView.TabPages.Remove(tabThongKe);pnlCard5.Visible=false;}
        var accounts=new Button {Text=user.Role=="Admin"?"Tài khoản cá nhân / Quyền":"Đổi mật khẩu",Width=190,Height=38,Location=new Point(10,510)};
        accounts.Click+=async (_,_)=>await Run(ShowAccounts);pnlLeftTools.Controls.Add(accounts);
        if(user.Role=="Admin")
        {
            AddTool("Đăng ký tài khoản",555,ShowRegisterAccount);
            AddTool("Đặt lại mật khẩu",600,ShowResetPassword);
        }
        if(RolePolicy.CanViewFinance(user.Role))
        {
            var invoices=new Button {Text="Hóa đơn / Doanh thu",Width=190,Height=38,Location=new Point(10,user.Role=="Admin"?645:555)};
            invoices.Click+=async (_,_)=>await Run(ShowInvoices);pnlLeftTools.Controls.Add(invoices);
        }
        if(RolePolicy.CanViewOperations(user.Role))AddTool("Lịch đặt / Lịch sử",user.Role=="Admin"?690:600,ShowStayHistory);
        if(operatorAccess){AddTool("Thu cọc bổ sung",user.Role=="Admin"?735:645,ShowDeposit);AddTool("Xử lý dịch vụ",user.Role=="Admin"?780:690,ShowOrderManagement);}
        if(user.Role is "Admin" or "Manager")AddTool("Nhật ký thao tác",user.Role=="Admin"?825:735,ShowAudit);
        if(user.Role=="Manager")AddTool("Nhân viên",780,ShowEmployees);
        if(RolePolicy.CanManageCatalog(user.Role))
        {
            AddTool("Danh mục phòng",870,ShowRoomCatalog);
            AddTool("Danh mục dịch vụ",915,ShowServiceCatalog);
            AddTool("Bảng giá",960,ShowPricing);
        }
        dgvDatCoc.AutoGenerateColumns=true;dgvLichTrinh.AutoGenerateColumns=true;
        ApplyAppearance();
        ConfigureRoleDashboard();
        ReflowSidebar();
    }
    public void StopTimers(){clock.Stop();refreshTimer.Stop();}
    private async Task Run(Func<Task> action,bool background=false)
    {
        if(busy || IsDisposed)return;
        busy=true;tlpBody.Enabled=false;tlpCards.Enabled=false;btnRefresh.Enabled=false;btnDangXuat.Enabled=false;
        try{await action();}
        catch(Exception ex)
        {
            if(background) {Ui.Log(ex);lblHeaderTitle.Text="Mất đồng bộ — nhấn Làm mới để kết nối lại";}
            else Ui.Error(this,ex);
        }
        finally
        {
            busy=false;
            if(!IsDisposed){UseWaitCursor=false;tlpBody.Enabled=true;tlpCards.Enabled=true;btnRefresh.Enabled=true;btnDangXuat.Enabled=true;}
        }
    }
    private async Task Reload()
    {
        var fresh=await service.DashboardAsync(revenueOverview?.Days??7);
        accountingToday=user.Role=="Accountant" ? await service.PeriodReportAsync(fresh.ServerNow.Date,fresh.ServerNow.Date) : null;
        if(IsDisposed)return;
        data=fresh;serverElapsed.Restart();lastDate=fresh.ServerNow.Date;
        lblHeaderTitle.Text=$"{DashboardTitle()} • {user.Username}";
        Render();
    }
    private async Task Changed(Func<Task> command)
    {
        await command();
        try{await Reload();}
        catch(Exception ex)
        {
            Ui.Error(this,ex);
            lblHeaderTitle.Text="ĐÃ LƯU — chưa tải lại được. Nhấn Làm mới.";
        }
    }
    private Stay? RoomStay(Room room)=>data.Stays.SingleOrDefault(s=>s.RoomId==room.Id && s.Status==StayStatus.Occupied);
    private Room? StayRoom(Stay stay)=>data.Rooms.SingleOrDefault(r=>r.Id==stay.RoomId);
    private void Render()
    {
        var occupied=data.Rooms.Count(r=>r.Status==RoomStatus.DangO);
        var reserved=data.Stays.Where(s=>s.Status==StayStatus.Reserved).ToList();
        lblCard1Value.Text=$"{occupied} / {data.Rooms.Count}";
        lblCard1Sub.Text=$"Công suất: {(data.Rooms.Count==0?0:100m*occupied/data.Rooms.Count):0.0}%";
        lblCard2Value.Text=$"{data.Rooms.Count(r=>r.Status==RoomStatus.Trong)} phòng";
        lblCard3Value.Text=$"{reserved.Count} lượt";
        lblCard3Sub.Text=$"Có cọc: {reserved.Count(s=>s.Deposit>0)} | Chưa cọc: {reserved.Count(s=>s.Deposit==0)}";
        lblCard4Value.Text=$"{data.Stays.Count(s=>s.Status==StayStatus.Occupied && s.Departure.Date==lastDate)} lượt";
        lblCard5Value.Text=$"{data.Revenue.Sum(x=>x.Total):N0} đ";
        if(user.Role=="Accountant" && accountingToday is { } report)
        {
            lblCard1Value.Text=report.Invoices.Count.ToString();
            lblCard2Value.Text=$"{report.Invoices.Sum(x=>x.Total):N0} đ";
            lblCard3Value.Text=$"{report.Payments.Where(x=>x.Kind=="Deposit" || x.Kind=="Checkout").Sum(x=>x.Amount):N0} đ";
            lblCard4Value.Text=$"{report.Payments.Where(x=>x.Kind=="Refund").Sum(x=>x.Amount):N0} đ";
        }
        void Summary(Label label,Func<Room,bool> group)
        {
            var rooms=data.Rooms.Where(group).ToList();
            label.Text=$"{rooms.Count} phòng  •  {rooms.Count(r=>r.Status==RoomStatus.Trong)} trống";
        }
        Summary(lblDonSummary,r=>r.Type=="Đơn");
        Summary(lblDoiSummary,r=>r.Type=="Đôi");
        Summary(lblVipSummary,r=>r.Type is not ("Đơn" or "Đôi"));
        foreach(var removed in roomButtons.Keys.Except(data.Rooms.Select(r=>r.Id)).ToArray()) {roomButtons[removed].Dispose();roomButtons.Remove(removed);}
        foreach(var room in data.Rooms)
        {
            if(!roomButtons.TryGetValue(room.Id,out var button))
            {
                button=new RoomTile();
                button.Click+=async (sender,_)=> {if(RolePolicy.CanOperate(user.Role) && sender is Button {Tag:Room selected})await Run(()=>RoomAction(selected));};
                roomButtons.Add(room.Id,button);
            }
            var parent=room.Type switch {"Đơn"=>flpDon,"Đôi"=>flpDoi,_=>flpVIP};
            if(button.Parent!=parent)parent.Controls.Add(button);
            var shortStatus=room.Status switch {RoomStatus.DangDon=>"Dọn",RoomStatus.DangO=>"Đang ở",RoomStatus.DaDat=>"Đã đặt",RoomStatus.BaoTri=>"B.Trì",_=>"Trống"};
            var reservations=data.Stays.Count(s=>s.RoomId==room.Id && s.Status==StayStatus.Reserved);
            button.Tag=room;button.Text=$"{room.Number}{(reservations>0?" *":"")}\n{shortStatus}";
            if(button is RoomTile tile){tile.Reservations=reservations;tile.AccessibleName=$"Phòng {room.Number}, {Ui.Status(room.Status)}, {reservations} lịch đặt";tile.Invalidate();}
            button.BackColor=AppTheme.RoomTint(room.Status);
        }
        ResizeRoomTiles(flpDon);ResizeRoomTiles(flpDoi);ResizeRoomTiles(flpVIP);
        RenderBookings();RenderSchedule();RenderHousekeeping();RenderOrders();RenderRevenue();
    }
    private sealed record BookingRow(long Id,string Phòng,string Khách,string SĐT,string CCCD,DateTime NgàyĐến,DateTime? HạnGiữ,decimal Cọc,string TìnhTrạng);
    private sealed record ScheduleRow(long Id,string Phòng,string Khách,string SĐT,string NghiệpVụ,DateTime ThờiGian);
    private void RenderBookings()
    {
        dgvDatCoc.DataSource=data.Stays.Where(s=>s.Status==StayStatus.Reserved).OrderBy(s=>s.Arrival).Select(s=>new BookingRow(s.Id,StayRoom(s)?.Number??"?",s.Guest,s.Phone,s.Identity,s.Arrival,s.HoldUntil,s.Deposit,s.HoldUntil<=ServerNow?"QUÁ HẠN — KHÔNG HOÀN CỌC":"Chờ nhận")).ToList();
        if(dgvDatCoc.Columns["Id"] is { } idColumn)idColumn.Visible=false;
        if(RolePolicy.CanOperate(user.Role)){AddAction(dgvDatCoc,"checkin","Nhận phòng");AddAction(dgvDatCoc,"edit","Sửa lịch / khách");AddAction(dgvDatCoc,"cancel","Hủy / xử lý cọc");}
        dgvDatCoc.AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.AllCells;
        if(dgvDatCoc.Columns["NgàyĐến"] is { } arrival){arrival.HeaderText="Ngày đến";arrival.DefaultCellStyle.Format="dd/MM/yyyy HH:mm";}
        if(dgvDatCoc.Columns["HạnGiữ"] is { } hold){hold.HeaderText="Hạn giữ";hold.DefaultCellStyle.Format="dd/MM/yyyy HH:mm";}
        if(dgvDatCoc.Columns["Cọc"] is { } deposit)deposit.DefaultCellStyle.Format="N0";
        if(dgvDatCoc.Columns["TìnhTrạng"] is { } state)state.HeaderText="Tình trạng";
    }
    private static void AddAction(DataGridView grid,string name,string text)
    {
        if(!grid.Columns.Contains(name))grid.Columns.Add(new DataGridViewButtonColumn {Name=name,HeaderText=text,Text=text,UseColumnTextForButtonValue=true});
    }
    private void RenderSchedule()
    {
        var filter=cboBoLocLich.SelectedIndex;
        dgvLichTrinh.DataSource=data.Stays.Where(s=>(s.Status==StayStatus.Reserved?s.Arrival:s.Departure).Date==ServerNow.Date)
            .Where(s=>filter<=0 || (filter==1?s.Status==StayStatus.Reserved:s.Status==StayStatus.Occupied))
            .Select(s=>new ScheduleRow(s.Id,StayRoom(s)?.Number??"?",s.Guest,s.Phone,s.Status==StayStatus.Reserved?"Chờ Check-in":"Chờ Check-out",s.Status==StayStatus.Reserved?s.Arrival:s.Departure)).OrderBy(x=>x.ThờiGian).ToList();
        if(dgvLichTrinh.Columns["Id"] is { } idColumn)idColumn.Visible=false;
        if(RolePolicy.CanOperate(user.Role))AddAction(dgvLichTrinh,"action","Tiến hành");
        dgvLichTrinh.AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.AllCells;
        if(dgvLichTrinh.Columns["NghiệpVụ"] is { } task)task.HeaderText="Nghiệp vụ";
        if(dgvLichTrinh.Columns["ThờiGian"] is { } when){when.HeaderText="Thời gian";when.DefaultCellStyle.Format="dd/MM/yyyy HH:mm";}
    }
    private void RenderHousekeeping()
    {
        Ui.Clear(flpDonPhong);
        var rooms=data.Rooms.Where(r=>r.Status==RoomStatus.DangDon).ToList();
        lblRightTitle1.Text=$"Phòng đang dọn ({rooms.Count})";
        foreach(var room in rooms)
        {
            var button=new Button {Text=$"{room.Number}\nXong",Width=58,Height=50};AppTheme.Button(button);button.ForeColor=AppTheme.Amber;
            button.Enabled=RolePolicy.CanOperate(user.Role);button.Click+=async (_,_)=>await Run(()=>RoomAction(room));flpDonPhong.Controls.Add(button);
        }
    }
    private void RenderOrders()
    {
        Ui.Clear(flpYeuCauKhach);
        foreach(var group in data.Pending.GroupBy(x=>x.StayId))
        {
            var stay=data.Stays.SingleOrDefault(x=>x.Id==group.Key);
            if(stay is null)continue;
            var panel=new FlowLayoutPanel {Width=195,AutoSize=true,FlowDirection=FlowDirection.TopDown,BackColor=Color.White,Padding=new Padding(8),Margin=new Padding(2,2,2,10)};
            panel.Controls.Add(new Label {Text=$"P.{StayRoom(stay)?.Number}  •  {stay.Guest}",Font=AppTheme.Bold,ForeColor=AppTheme.Ink,AutoSize=true,MaximumSize=new Size(175,0)});
            panel.Controls.Add(new Label {Text=string.Join("\n",group.Select(x=>$"{x.Name}  × {x.Quantity-x.DeliveredQuantity}")),ForeColor=AppTheme.Muted,AutoSize=true,MaximumSize=new Size(175,0)});
            var done=new Button {Text="✓  Xác nhận tất cả",Width=170,Height=34};AppTheme.Button(done,true);
            done.Click+=async (_,_)=>await Run(async()=>
            {
                if(Ui.Confirm(this,$"Xác nhận đã giao tất cả dịch vụ của {stay.Guest}?"))await Changed(()=>service.DeliverAsync(stay));
            });
            var cancel=new Button {Text="×  Hủy tất cả",Width=170,Height=34};AppTheme.Button(cancel);
            cancel.ForeColor=Color.FromArgb(185,62,78);
            cancel.Click+=async (_,_)=>await Run(async()=>
            {
                if(!Ui.Confirm(this,$"Hủy toàn bộ phần dịch vụ chưa giao của {stay.Guest}? Phần đã giao vẫn được tính tiền."))return;
                using var reasonDialog=new InputDialog("Lý do hủy dịch vụ",500,300);
                var reason=Ui.Text(300);reasonDialog.Add("Lý do hủy",reason);
                reasonDialog.Action("XÁC NHẬN HỦY",async()=>await Changed(()=>service.CancelPendingOrdersAsync(stay,reason.Text)));
                reasonDialog.ShowDialog(this);
            });
            if(RolePolicy.CanOperate(user.Role)){panel.Controls.Add(done);panel.Controls.Add(cancel);}flpYeuCauKhach.Controls.Add(panel);
        }
        if(data.Pending.Count==0)flpYeuCauKhach.Controls.Add(new Label {Text="Không có dịch vụ đang chờ.",AutoSize=true});
    }
    private void RenderRevenue()
    {
        if(revenueOverview is null)
        {
            revenueOverview=new RevenueOverview {Dock=DockStyle.Fill};
            revenueOverview.PeriodChanged+=async (_,_)=>await Run(Reload);
            pnlChartContainer.Controls.Add(revenueOverview);
        }
        revenueOverview.SetData(data);
    }
    private async void btnRefresh_Click(object? sender,EventArgs e)=>await Run(async()=> {if(RolePolicy.CanOperate(user.Role))await service.ExpireReservationsAsync();await Reload();});
    private void btnDangXuat_Click(object? sender,EventArgs e)
    {
        if(!busy && Ui.Confirm(this,"Đăng xuất khỏi ca trực? Dữ liệu đã lưu vẫn được giữ.")){StopTimers();LogoutRequested?.Invoke(this,EventArgs.Empty);}
    }
    private void cboBoLocLich_SelectedIndexChanged(object? sender,EventArgs e)=>RenderSchedule();
    private void pnlCard1_Click(object? sender,EventArgs e){if(tabMainView.TabPages.Contains(tabMatrix))tabMainView.SelectedTab=tabMatrix;}
    private async void pnlCard2_Click(object? sender,EventArgs e){if(RolePolicy.CanOperate(user.Role))await Run(()=>ShowBooking(false));}
    private async void pnlCard3_Click(object? sender,EventArgs e){if(RolePolicy.CanOperate(user.Role))await Run(()=>ShowBooking(true));}
    private async void btnThemPhong_Click(object? sender,EventArgs e){if(RolePolicy.CanOperate(user.Role))await Run(()=>ShowBooking(false));}
    private async void pnlCard4_Click(object? sender,EventArgs e){if(RolePolicy.CanOperate(user.Role))await Run(async()=> {var stay=SelectStay("Chọn phòng trả");if(stay!=null)await ShowCheckout(stay);});}
    private async void btnTimPhong_Click(object? sender,EventArgs e)=>await Run(async()=>
    {
        var room=data.Rooms.SingleOrDefault(r=>r.Number.Equals(txtTimPhong.Text.Trim(),StringComparison.OrdinalIgnoreCase))??throw new BusinessException("Không tìm thấy phòng.");
        if(RolePolicy.CanOperate(user.Role))await RoomAction(room);
    });
    private async void dgvDatCoc_CellContentClick(object? sender,DataGridViewCellEventArgs e)
    {
        if(!RolePolicy.CanOperate(user.Role) || e.RowIndex<0 || e.ColumnIndex<0 || dgvDatCoc.Rows[e.RowIndex].DataBoundItem is not BookingRow row)return;
        var stay=data.Stays.SingleOrDefault(s=>s.Id==row.Id);if(stay==null)return;
        var column=dgvDatCoc.Columns[e.ColumnIndex].Name;
        if(column=="checkin")await Run(async()=> {if(Ui.Confirm(this,$"Nhận phòng cho {stay.Guest}?"))await Changed(()=>service.CheckInAsync(stay));});
        if(column=="cancel")await Run(()=>ShowCancel(stay));
        if(column=="edit")await Run(()=>ShowEditBooking(stay));
    }
    private async void dgvLichTrinh_CellContentClick(object? sender,DataGridViewCellEventArgs e)
    {
        if(!RolePolicy.CanOperate(user.Role) || e.RowIndex<0 || e.ColumnIndex<0 || dgvLichTrinh.Columns[e.ColumnIndex].Name!="action" || dgvLichTrinh.Rows[e.RowIndex].DataBoundItem is not ScheduleRow row)return;
        var stay=data.Stays.SingleOrDefault(s=>s.Id==row.Id);
        if(stay!=null)await Run(async()=>
        {
            if(stay.Status==StayStatus.Reserved)
            {
                if(Ui.Confirm(this,$"Nhận phòng cho {stay.Guest}?"))await Changed(()=>service.CheckInAsync(stay));
            }
            else await ShowCheckout(stay);
        });
    }
}
