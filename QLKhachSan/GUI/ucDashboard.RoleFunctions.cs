using QLKhachSan.BLL;

namespace QLKhachSan.GUI;

public partial class ucDashboard
{
    private async Task ShowEmployees()
    {
        using var dialog=new InputDialog("Nhân viên khách sạn",850,560);
        var grid=Ui.Grid();grid.Height=350;
        dialog.Add("Tài khoản nhân viên (chỉ xem)",grid);
        async Task LoadRows()
        {
            grid.DataSource=await auth.EmployeesAsync(user);
            FormatGrid(grid,new() {{"Username","Tên đăng nhập"},{"Role","Vai trò"},{"Active","Đang hoạt động"},{"LockedUntil","Khóa đến"}});
            HideColumns(grid,"Id","Version");
        }
        dialog.Action("LÀM MỚI",LoadRows,false);
        await LoadRows();dialog.ShowDialog(this);
    }

    private async Task ShowCashFlow()
    {
        using var dialog=new InputDialog("Các khoản thu và hoàn tiền",1000,700);
        var from=Ui.DatePicker(ServerNow.Date);var through=Ui.DatePicker(ServerNow.Date);
        var summary=new Label {AutoSize=true};var grid=Ui.Grid();grid.Height=390;
        dialog.Add("Từ ngày",from);dialog.Add("Đến hết ngày",through);
        dialog.Add("Tổng hợp dòng tiền",summary);dialog.Add("Chi tiết",grid);
        async Task LoadRows()
        {
            var report=await service.PeriodReportAsync(from.Value,through.Value);
            var payments=report.Payments;
            summary.Text=$"Thu: {payments.Where(x=>x.CashFlow>0).Sum(x=>x.CashFlow):N0} đ  •  Hoàn: {-payments.Where(x=>x.CashFlow<0).Sum(x=>x.CashFlow):N0} đ";
            grid.DataSource=payments;
            FormatGrid(grid,new() {{"Kind","Loại"},{"Amount","Số tiền"},{"CashFlow","Dòng tiền"},{"Created","Thời điểm"},{"Method","Hình thức"},{"Username","Nhân viên"},{"Note","Ghi chú"}});
            HideColumns(grid,"Id","StayId");
        }
        dialog.Action("XEM THU CHI",LoadRows,false);
        dialog.Action("XUẤT CSV",()=>{ExportGrid(grid,"thu-chi");return Task.CompletedTask;},false);
        await LoadRows();dialog.ShowDialog(this);
    }

    private Task ShowRevenueChart()
    {
        tabMainView.SelectedTab=tabThongKe;
        return Task.CompletedTask;
    }
}
