using QLKhachSan.DAL;
using QLKhachSan.DTO;
using QLKhachSan.BLL;

namespace QLKhachSan.GUI;

public partial class ucDashboard
{
    private async Task ShowCheckout(Stay stay)
    {
        var bill=await service.QuoteAsync(stay);
        using var dialog=new InputDialog($"Thanh toán P.{bill.Room.Number}",660,800);
        dialog.Note($"Lượt #{stay.Id} • {stay.Guest}\nSĐT: {stay.Phone} • CCCD/Hộ chiếu: {stay.Identity}\nNhận: {stay.CheckIn:dd/MM/yyyy HH:mm}\nTính đến: {bill.At:dd/MM/yyyy HH:mm}\n\nTiền phòng: {bill.RoomCharge:N0} đ\nDịch vụ: {bill.Services:N0} đ\nTổng hóa đơn: {bill.Total:N0} đ\nCọc đã thu: {stay.Deposit:N0} đ\nTHU THÊM: {bill.ToCollect:N0} đ\nHOÀN KHÁCH: {bill.ToRefund:N0} đ");
        dialog.Note("Làm tròn toàn bộ lượt ở lên ngày 24 giờ, tối thiểu 1 ngày. Nếu đổi phòng, chia tiền theo thời gian và giá từng phòng; phần ngày lẻ làm tròn theo giá phòng cuối. Bảng tính có hiệu lực 10 phút.");
        var lines=new ListBox {Height=80};lines.Items.AddRange(bill.Lines.Select(x=>$"{x.Name} × {x.Quantity}: {x.Total:N0} đ ({(x.Delivered is null?"CHƯA GIAO":"đã giao")})").ToArray());dialog.Add("Chi tiết dịch vụ",lines);
        var method=Ui.Combo(new[]{"Tiền mặt","Chuyển khoản"});dialog.Add("Phương thức thanh toán / hoàn tiền",method);
        if(bill.ToCollect>0)PaymentQr.Add(dialog,method,()=>bill.ToCollect,()=>$"KS-{stay.Id}");
        var confirm=new CheckBox {Text="Đã thu đủ tiền / hoàn đủ tiền cho khách"};dialog.AddActionConfirmation("Xác nhận thu chi",confirm);
        dialog.Action("HOÀN TẤT CHECK-OUT",async()=>
        {
            if(!confirm.Checked)throw new BusinessException("Cần xác nhận đã hoàn tất thu/hoàn tiền.");
            long invoiceId=0;
            await Changed(async()=>invoiceId=await service.CheckoutAsync(bill,(string)method.SelectedItem!));
            MessageBox.Show(dialog,$"Đã lưu hóa đơn #{invoiceId}. Phòng chuyển sang đang dọn.","Hoàn tất");
        });
        dialog.ShowDialog(this);
    }
    private async void btnQuanLyKhach_Click(object? sender,EventArgs e)=>await Run(async()=>
    {
        using var dialog=new InputDialog("Hồ sơ khách hàng",1000,800);
        var search=Ui.Text();var grid=Ui.Grid();grid.Height=240;
        var history=new ListBox {Height=120};var invoices=Ui.Grid();invoices.Height=160;
        dialog.Add("Tìm theo tên / SĐT / CCCD (tối đa 200 kết quả gần nhất)",search);
        dialog.Add("Danh sách khách",grid);dialog.Add("500 dịch vụ gần nhất của khách đã chọn",history);
        dialog.Add("Hóa đơn của khách đã chọn",invoices);
        async Task LoadCustomers()
        {
            var customers=await service.CustomersAsync(search.Text);
            grid.DataSource=customers;
            var headers=new Dictionary<string,string>{{"Name","Họ tên"},{"Phone","SĐT"},{"Identity","CCCD/Hộ chiếu"},{"Visits","Lượt đã thanh toán"},{"Total","Tổng chi"}};
            foreach(DataGridViewColumn col in grid.Columns)
            {
                if(col.Name=="Id")col.Visible=false;
                if(headers.TryGetValue(col.Name,out var header))col.HeaderText=header;
                if(col.Name=="Total")col.DefaultCellStyle.Format="N0";
            }
        }
        var historyBusy=false;
        grid.SelectionChanged+=async (_,_)=>
        {
            if(historyBusy)return;
            historyBusy=true;
            try
            {
                while(!dialog.IsDisposed && grid.CurrentRow?.DataBoundItem is CustomerSummary customer)
                {
                    var rows=await service.CustomerOrdersAsync(customer.Id);
                    if(dialog.IsDisposed)break;
                    if(grid.CurrentRow?.DataBoundItem is not CustomerSummary current)break;
                    if(current.Id!=customer.Id)continue;
                    history.Items.Clear();history.Items.AddRange(rows.Select(x=>$"{x.Ordered:dd/MM/yyyy HH:mm} • {x.Name} × {x.Quantity} • {x.Total:N0} đ{(x.Cancelled is null?"":" • ĐÃ HỦY")}").ToArray());
                    invoices.DataSource=await service.CustomerInvoicesAsync(customer.Id);
                    FormatGrid(invoices,new() {{"Id","Mã hóa đơn"},{"Room","Phòng"},{"Guest","Khách"},{"Issued","Ngày lập"},{"Total","Tổng tiền"}});
                    HideColumns(invoices,"StayId");break;
                }
            }
            catch(Exception ex){if(!dialog.IsDisposed)Ui.Error(dialog,ex);}
            finally{historyBusy=false;}
        };
        dialog.Action("TÌM KHÁCH",LoadCustomers,false);
        await LoadCustomers();dialog.ShowDialog(this);
    });
    private async Task ShowInvoices()
    {
        using var dialog=new InputDialog("Hóa đơn, doanh thu và thu chi",1150,900);
        var day=Ui.DatePicker(ServerNow.Date);day.CustomFormat="dd/MM/yyyy";
        var through=Ui.DatePicker(ServerNow.Date);through.CustomFormat="dd/MM/yyyy";
        var grid=Ui.Grid();grid.Height=210;var totals=new Label {AutoSize=true,MaximumSize=new Size(1000,0)};var categories=new ListBox {Height=90};
        var cash=Ui.Grid();cash.Height=210;
        PeriodReport? currentReport=null;
        DateTime reportFrom=day.Value,reportThrough=through.Value;
        dialog.Add("Từ ngày",day);dialog.Add("Đến hết ngày",through);dialog.Add("Hóa đơn đã thanh toán",grid);dialog.Add("Tổng hợp",totals);dialog.Add("Doanh thu theo hạng mục",categories);dialog.Add("Thu chi và ghi nhận cọc không hoàn",cash);
        async Task LoadReport()
        {
            var report=await service.PeriodReportAsync(day.Value,through.Value);
            currentReport=report;
            reportFrom=day.Value;reportThrough=through.Value;
            var invoices=report.Invoices;
            var revenue=report.Revenue;
            grid.DataSource=invoices;
            var names=new Dictionary<string,string>{{"Id","Hóa đơn"},{"StayId","Lượt ở"},{"Room","Phòng"},{"Guest","Khách"},{"Issued","Lập lúc"},{"RoomCharge","Tiền phòng"},{"ServiceCharge","Dịch vụ"},{"Deposit","Cọc"},{"Collected","Thu thêm"},{"Refunded","Hoàn cọc"},{"Method","Hình thức"},{"Total","Tổng tiền"}};
            FormatGrid(grid,names);
            var payments=report.Payments;
            totals.Text=$"{invoices.Count} hóa đơn • Doanh thu hóa đơn: {invoices.Sum(x=>x.Total):N0} đ • Cọc không hoàn: {payments.Where(p=>p.Kind=="Forfeit").Sum(p=>p.Amount):N0} đ\nThu cọc: {payments.Where(p=>p.Kind=="Deposit").Sum(p=>p.Amount):N0} đ • Thu checkout: {payments.Where(p=>p.Kind=="Checkout").Sum(p=>p.Amount):N0} đ • Hoàn: {payments.Where(p=>p.Kind=="Refund").Sum(p=>p.Amount):N0} đ\n"+string.Join(" • ",payments.Where(p=>p.Kind!="Forfeit").GroupBy(p=>p.Method).Select(g=>$"{g.Key} (thu − hoàn): {g.Sum(p=>p.CashFlow):N0} đ"))+"\nCọc không hoàn là ghi nhận từ tiền đã thu trước đó, không phải khoản thu mới.";
            cash.DataSource=payments.Select(p=>new {Mã=p.Id,Lượt=p.StayId,Khách=p.Guest,Loại=p.Kind switch {"Deposit"=>"Thu cọc","Checkout"=>"Thu checkout","Refund"=>"Hoàn cọc",_=>"Cọc không hoàn"},SốTiền=p.Amount,DòngTiền=p.CashFlow,ThờiĐiểm=p.Created,HìnhThức=p.Method,NhânViên=p.Username,GhiChú=p.Note}).ToList();
            FormatGrid(cash,new() {{"SốTiền","Số tiền"},{"DòngTiền","Thu − chi"},{"ThờiĐiểm","Thời điểm"},{"HìnhThức","Hình thức"},{"NhânViên","Nhân viên"},{"GhiChú","Ghi chú"}});
            categories.Items.Clear();categories.Items.AddRange(revenue.Select(x=>$"{x.Category}: {x.Total:N0} đ").ToArray());
        }
        dialog.Action("XEM BÁO CÁO",LoadReport,false);
        dialog.Action("XUẤT HÓA ĐƠN CSV",()=>{if(currentReport is not null)ExportInvoices(currentReport,reportFrom,reportThrough);return Task.CompletedTask;},false);
        dialog.Action("XUẤT THU CHI CSV",()=>{if(currentReport is not null)ExportPayments(currentReport,reportFrom,reportThrough);return Task.CompletedTask;},false);
        dialog.Action("XEM / IN HÓA ĐƠN ĐÃ CHỌN",async()=>
        {
            if(grid.CurrentRow?.DataBoundItem is not Invoice invoice)throw new BusinessException("Chọn hóa đơn đã thanh toán.");
            var stay=await service.InvoiceStayAsync(invoice.StayId);
            var lines=await service.InvoiceOrdersAsync(invoice.StayId);
            PrintInvoice(invoice,stay,lines);
        },false);
        await LoadReport();dialog.CompactActions();dialog.AcceptButton=null;dialog.ShowDialog(this);
    }
    private async Task ShowAccounts()
    {
        using var dialog=new InputDialog(user.Role=="Admin"?"Tài khoản cá nhân và phân quyền":"Đổi mật khẩu",900,user.Role=="Admin"?900:790);
        var oldPassword=Ui.Text(128,true);var password=Ui.Text(5,true);var again=Ui.Text(5,true);
        dialog.Add("Mật khẩu hiện tại",oldPassword);dialog.Add("Mật khẩu mới (1–5 ký tự)",password);dialog.Add("Nhập lại mật khẩu mới",again);
        dialog.Action(user.Role=="Admin"?"ĐỔI MẬT KHẨU CỦA TÔI":"ĐỔI MẬT KHẨU",async()=>
        {
            if(password.Text!=again.Text)throw new BusinessException("Hai mật khẩu mới không khớp.");
            await auth.ChangePasswordAsync(user,oldPassword.Text,password.Text);
            oldPassword.Clear();password.Clear();again.Clear();MessageBox.Show(dialog,"Đã đổi mật khẩu.");
        },false);
        if(user.Role=="Admin")
        {
            var grid=Ui.Grid();grid.Height=180;var editRole=Ui.Combo(RolePolicy.Roles);
            var active=new CheckBox {Text="Cho phép đăng nhập",Checked=true,AutoSize=true};
            dialog.Add("DANH SÁCH NHÂN VIÊN",grid);dialog.Add("Đổi vai trò",editRole);dialog.Add("Trạng thái",active);
            UserInfo Selected()=>grid.CurrentRow?.DataBoundItem as UserInfo??throw new BusinessException("Chọn tài khoản.");
            async Task LoadRows()
            {
                grid.DataSource=await auth.UsersAsync(user);
                foreach(DataGridViewColumn column in grid.Columns)if(column.Name is "Id" or "Version")column.Visible=false;
            }
            grid.SelectionChanged+=(_,_)=>{if(grid.CurrentRow?.DataBoundItem is UserInfo item){editRole.SelectedItem=item.Role;active.Checked=item.Active;}};
            dialog.Action("LƯU QUYỀN / TRẠNG THÁI",async()=>{await auth.UpdateUserAsync(user,Selected(),(string)editRole.SelectedItem!,active.Checked);await LoadRows();},false);
            dialog.Action("ĐỔI MẬT KHẨU NHÂN VIÊN",async()=>
            {
                var selected=Selected();
                if(selected.Id==user.Id)throw new BusinessException("Mật khẩu của bạn được đổi ở phần đầu cửa sổ này.");
                using var reset=new InputDialog($"Đổi mật khẩu: {selected.Username}",520,340);
                var staffPassword=Ui.Text(5,true);var staffAgain=Ui.Text(5,true);
                reset.Add("Mật khẩu mới (1–5 ký tự)",staffPassword);
                reset.Add("Nhập lại mật khẩu mới",staffAgain);
                reset.Action("XÁC NHẬN ĐỔI MẬT KHẨU",async()=>
                {
                    if(staffPassword.Text!=staffAgain.Text)throw new BusinessException("Hai mật khẩu nhân viên không khớp.");
                    await auth.ResetPasswordAsync(user,selected,staffPassword.Text);
                });
                if(reset.ShowDialog(dialog)==DialogResult.OK)
                {
                    await LoadRows();
                    MessageBox.Show(dialog,$"Đã đổi mật khẩu của {selected.Username} và thu hồi phiên đăng nhập cũ.");
                }
            },false);
            await LoadRows();
        }
        dialog.AcceptButton=null;
        dialog.ShowDialog(this);
    }
    private Task ShowRegisterAccount()
    {
        using var dialog=new InputDialog("Đăng ký tài khoản nhân viên",620,530);
        var name=Ui.Text(50);var password=Ui.Text(5,true);var again=Ui.Text(5,true);var role=Ui.Combo(RolePolicy.Roles);
        dialog.Add("Tên đăng nhập",name);
        dialog.Add("Mật khẩu (1–5 ký tự)",password);
        dialog.Add("Nhập lại mật khẩu",again);
        dialog.Add("Vai trò nhân viên",role);
        dialog.Action("ĐĂNG KÝ TÀI KHOẢN",async()=>
        {
            if(password.Text!=again.Text)throw new BusinessException("Hai mật khẩu không khớp.");
            await auth.CreateUserAsync(user,name.Text,password.Text,(string)role.SelectedItem!);
            name.Clear();password.Clear();again.Clear();
            MessageBox.Show(dialog,"Đã đăng ký tài khoản nhân viên.");
        },false);
        dialog.ShowDialog(this);return Task.CompletedTask;
    }
    private async Task ShowResetPassword()
    {
        using var dialog=new InputDialog("Đặt lại mật khẩu nhân viên",750,610);
        var grid=Ui.Grid();grid.Height=260;
        var password=Ui.Text(5,true);var again=Ui.Text(5,true);
        dialog.Add("Chọn tài khoản nhân viên",grid);
        dialog.Add("Mật khẩu mới (1–5 ký tự)",password);
        dialog.Add("Nhập lại mật khẩu mới",again);
        async Task LoadRows()
        {
            grid.DataSource=(await auth.UsersAsync(user)).Where(x=>x.Id!=user.Id).ToList();
            HideColumns(grid,"Id","Version");
        }
        dialog.Action("ĐẶT LẠI MẬT KHẨU",async()=>
        {
            if(grid.CurrentRow?.DataBoundItem is not UserInfo selected)throw new BusinessException("Chọn tài khoản cần đặt lại mật khẩu.");
            if(password.Text!=again.Text)throw new BusinessException("Hai mật khẩu không khớp.");
            await auth.ResetPasswordAsync(user,selected,password.Text);
            password.Clear();again.Clear();await LoadRows();
            MessageBox.Show(dialog,"Đã đặt lại mật khẩu và thu hồi phiên đăng nhập cũ.");
        },false);
        await LoadRows();dialog.ShowDialog(this);
    }
}


