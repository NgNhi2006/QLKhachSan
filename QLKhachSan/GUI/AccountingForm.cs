using QLKhachSan.BLL;
using QLKhachSan.DAL;
using QLKhachSan.DTO;

namespace QLKhachSan.GUI;

internal sealed class AccountingForm : Form
{
    private sealed record Choice(string Code,string Label) {public override string ToString()=>Label;}
    private readonly HotelService service;
    private readonly UserSession user;
    private readonly DateTimePicker from=new() {Format=DateTimePickerFormat.Custom,CustomFormat="dd/MM/yyyy"};
    private readonly DateTimePicker through=new() {Format=DateTimePickerFormat.Custom,CustomFormat="dd/MM/yyyy"};
    private readonly TabControl tabs=new() {Dock=DockStyle.Fill};
    private readonly Dictionary<string,DataGridView> grids=[];
    private readonly Label summary=new() {Dock=DockStyle.Fill,Font=new Font("Segoe UI",10,FontStyle.Bold),Padding=new Padding(12,6,4,0)};
    private bool loading;

    public AccountingForm(UserSession user)
    {
        this.user=user;service=new HotelService(new HotelRepository(),user);
        Text="Tài chính - Kế toán";Size=new Size(1280,820);MinimumSize=new Size(950,650);
        StartPosition=FormStartPosition.CenterParent;Font=AppTheme.Body;BackColor=AppTheme.Canvas;
        var root=new TableLayoutPanel {Dock=DockStyle.Fill,RowCount=3,ColumnCount=1,Padding=new Padding(14)};
        root.RowStyles.Add(new RowStyle(SizeType.Absolute,55));root.RowStyles.Add(new RowStyle(SizeType.Percent,100));root.RowStyles.Add(new RowStyle(SizeType.Absolute,44));
        var bar=new FlowLayoutPanel {Dock=DockStyle.Fill,WrapContents=false,AutoScroll=true};
        bar.Controls.Add(new Label {Text="Từ",AutoSize=true,Margin=new Padding(5,10,8,0)});
        from.Value=DateTime.Today.AddDays(-30);from.Width=135;bar.Controls.Add(from);
        bar.Controls.Add(new Label {Text="Đến",AutoSize=true,Margin=new Padding(15,10,8,0)});
        through.Value=DateTime.Today;through.Width=135;bar.Controls.Add(through);
        AddButton(bar,"Làm mới",async()=>await RefreshAll());
        AddButton(bar,"Xuất Excel",()=>{ExcelExport.ExportGrid(this,grids[tabs.SelectedTab!.Text],tabs.SelectedTab.Text,DateTime.Now);return Task.CompletedTask;});
        root.Controls.Add(bar,0,0);root.Controls.Add(tabs,0,1);root.Controls.Add(summary,0,2);Controls.Add(root);
        AddTab("Ca trực",BuildShiftTab);
        if(RolePolicy.CanViewFinance(user.Role))
        {
            AddTab("Sổ quỹ",BuildVoucherTab);
            AddTab("Đối soát ngân hàng",BuildBankTab);
            AddTab("Công nợ",BuildDebtTab);
            AddTab("Kho minibar",BuildStockTab);
            AddTab("Hóa đơn",BuildInvoiceTab);
            AddTab("Nhóm bill",BuildBillGroupTab);
            AddTab("Lãi lỗ",BuildProfitTab);
        }
        Shown+=async (_,_)=>await RefreshAll();
    }
    private void AddTab(string title,Action<FlowLayoutPanel> actions)
    {
        var page=new TabPage(title) {BackColor=Color.White,Padding=new Padding(8)};
        var layout=new TableLayoutPanel {Dock=DockStyle.Fill,RowCount=2,ColumnCount=1};
        layout.RowStyles.Add(new RowStyle(SizeType.Percent,100));layout.RowStyles.Add(new RowStyle(SizeType.Absolute,100));
        var grid=Ui.Grid();grid.AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.DisplayedCells;
        var footer=new FlowLayoutPanel {Dock=DockStyle.Fill,AutoScroll=true,WrapContents=true};
        layout.Controls.Add(grid,0,0);layout.Controls.Add(footer,0,1);page.Controls.Add(layout);tabs.TabPages.Add(page);grids[title]=grid;
        actions(footer);
    }
    private static void AddButton(FlowLayoutPanel panel,string label,Func<Task> action)
    {
        var button=new Button {Text=label,AutoSize=true,MinimumSize=new Size(130,38),Margin=new Padding(5)};
        AppTheme.Button(button,false);button.Click+=async (_,_)=>
        {
            button.Enabled=false;
            try{await action();}catch(Exception ex){Ui.Error(button.FindForm(),ex);}finally{if(!button.IsDisposed)button.Enabled=true;}
        };
        panel.Controls.Add(button);
    }
    private InputDialog Dialog(string title) => new(title,620,650);
    private async Task RefreshAll()
    {
        if(loading)return;loading=true;
        try
        {
            if(through.Value.Date<from.Value.Date || through.Value.Date-from.Value.Date>TimeSpan.FromDays(366))throw new BusinessException("Khoảng báo cáo tối đa 367 ngày.");
            var shifts=await service.CashShiftsAsync(from.Value,through.Value);
            Bind("Ca trực",shifts.Select(x=>new {x.Id,NhânViên=x.Cashier,MởCa=x.OpenedAt,ĐóngCa=x.ClosedAt,TrạngThái=x.Status switch {"Open"=>"Đang mở","Submitted"=>"Đã bàn giao","Locked"=>"Đã khóa",_=>x.Status},ĐầuCa=x.OpeningCash,ThuTiềnMặt=x.CashIn,ChiTiềnMặt=x.CashOut,POS=x.Pos,ChuyểnKhoản=x.Bank,OTA=x.Ota,LýThuyết=x.ExpectedCash,ThựcĐếm=x.CountedCash,ChênhLệch=x.Status=="Open"?null:(decimal?)x.Discrepancy,GiảiTrình=x.Explanation}).ToList());
            if(RolePolicy.CanViewFinance(user.Role))
            {
                var vouchers=await service.VouchersAsync(from.Value,through.Value);
                Bind("Sổ quỹ",vouchers.Select(x=>new {x.Id,Ngày=x.PostedAt,Loại=x.Type,Kênh=x.Channel,HạngMục=x.Category,SốTiền=x.Amount,ĐốiTượng=x.Counterparty,MãGiaoDịch=x.Reference,DiễnGiải=x.Note,NhânViên=x.Username,Hủy=x.Reversed}).ToList());
                var book=await service.BookAsync("Cash",from.Value,through.Value);
                var bankBook=await service.BookAsync("Bank",from.Value,through.Value);
                var bankLines=await service.BankLinesAsync(from.Value,through.Value);
                Bind("Đối soát ngân hàng",bankLines.Select(x=>new {x.Id,Ngày=x.OccurredAt,Kênh=x.Channel,MãGiaoDịch=x.Reference,SốTiền=x.Amount,ThanhToán=x.PaymentId,Phiếu=x.VoucherId,x.Status}).ToList());
                var debts=await service.DebtsAsync();
                Bind("Công nợ",debts.Select(x=>new {x.Id,Loại=x.Type,ĐốiTượng=x.Counterparty,HóaĐơn=x.InvoiceId,NgàyGhi=x.IssuedAt,Hạn=x.DueAt,SốGốc=x.Amount,ĐãTrả=x.Paid,CònLại=x.Outstanding,TuổiNợ=x.AgeBucket(DateTime.Today),DiễnGiải=x.Note}).ToList());
                var stock=await service.StockAsync();var reconciliation=await service.ReconcileMinibarAsync(from.Value,through.Value);
                Bind("Kho minibar",stock.Select(x=>new {x.Id,DịchVụ=x.ServiceId,Tên=x.Name,ĐơnVị=x.Unit,Tồn=x.Quantity,GiáVốnBìnhQuân=x.AverageCost,NgưỡngNhập=x.ReorderLevel,CảnhBáo=x.Quantity<=x.ReorderLevel,BánTrênBill=reconciliation.FirstOrDefault(r=>r.Item==x.Name)?.Billed??0,BuồngPhòngBáo=reconciliation.FirstOrDefault(r=>r.Item==x.Name)?.Housekeeping??0,XuấtKhoBán=reconciliation.FirstOrDefault(r=>r.Item==x.Name)?.BookSold??0}).ToList());
                var report=await service.AllFinanceInvoicesAsync(from.Value,through.Value);
                var controls=(await service.InvoiceControlsAsync(from.Value,through.Value)).ToDictionary(x=>x.Id);
                Bind("Hóa đơn",report.Select(x=>new {x.Id,Ngày=x.Issued,Phòng=x.Room,Khách=x.Guest,TiềnPhòng=x.RoomCharge,DịchVụ=x.ServiceCharge,Tổng=x.Total,Cọc=x.Deposit,ThuThêm=x.Collected,Hoàn=x.Refunded,Kênh=x.Method,TrạngThái=controls[x.Id].Voided?"Đã hủy":"Có hiệu lực",GiảmTrừ=controls[x.Id].Adjustments,VAT=controls[x.Id].VatRate,PhíPhụcVụ=controls[x.Id].ServiceRate,HóaĐơnĐiệnTử=controls[x.Id].EInvoiceNumber}).ToList());
                Bind("Nhóm bill",(await service.BillSharesAsync()).Select(x=>new {MãNhóm=x.GroupId,Đoàn=x.GroupName,HóaĐơn=x.InvoiceId,NgườiTrả=x.Payer,SốTiền=x.Amount,NgàyTạo=x.CreatedAt}).ToList());
                var pnl=await service.FinanceSummaryAsync(from.Value,through.Value);
                Bind("Lãi lỗ",new[]{new {DoanhThuPhòng=pnl.RoomRevenue,Minibar=pnl.MinibarRevenue,DịchVụKhác=pnl.OtherRevenue,GiảmTrừ=pnl.Discounts,DoanhThuThuần=pnl.NetRevenue,GiáVốn=pnl.CostOfGoods,ChiPhíVậnHành=pnl.OperatingExpenses,LợiNhuậnGộp=pnl.GrossProfit,PhòngĐêmBán=pnl.RoomNightsSold,PhòngĐêmSẵnCó=pnl.RoomNightsAvailable,ADR=pnl.Adr,RevPAR=pnl.RevPar,LấpĐầyPhầnTrăm=pnl.Occupancy}}.ToList());
                summary.Text=$"Quỹ tiền mặt: {book.Opening:N0} + {book.Receipts:N0} - {book.Payments:N0} = {book.Closing:N0} VNĐ  |  Ngân hàng: {bankBook.Opening:N0} + {bankBook.Receipts:N0} - {bankBook.Payments:N0} = {bankBook.Closing:N0} VNĐ  |  Nợ đến hạn: {debts.Count(x=>x.Outstanding>0 && x.DueAt.Date<=DateTime.Today):N0}";
            }
            else summary.Text="Ca trực cá nhân • tiền mặt cần nhập số đếm thực tế khi bàn giao.";
        }
        catch(Exception ex){Ui.Error(this,ex);}finally{loading=false;}
    }
    private void Bind<T>(string tab,List<T> rows)
    {
        var grid=grids[tab];grid.DataSource=rows;
        foreach(DataGridViewColumn column in grid.Columns)
        {
            if(column.ValueType==typeof(decimal) || column.ValueType==typeof(decimal?))column.DefaultCellStyle.Format="N0";
            if(column.ValueType==typeof(DateTime) || column.ValueType==typeof(DateTime?))column.DefaultCellStyle.Format="dd/MM/yyyy HH:mm";
        }
    }
    private void BuildShiftTab(FlowLayoutPanel actions)
    {
        if(user.Role is "Admin" or "Reception" or "Accountant" or "Manager")
        {
            AddButton(actions,"Mở ca",()=>
            {
                using var d=Dialog("Mở ca trực");var amount=Ui.Money();d.Add("Tiền mặt đầu ca",amount);
                d.Action("MỞ CA",async()=>{await service.OpenCashShiftAsync(amount.Value);await RefreshAll();});d.ShowDialog(this);return Task.CompletedTask;
            });
            AddButton(actions,"Bàn giao ca",()=>
            {
                using var d=Dialog("Kiểm đếm và bàn giao ca");var id=Ui.Number(int.MaxValue);var counted=Ui.Money();var reason=Ui.Text(500);
                d.Add("Mã ca",id);d.Add("Tiền mặt đếm thực tế",counted);d.Add("Giải trình chênh lệch",reason);
                d.Action("NỘP CA",async()=>{await service.SubmitCashShiftAsync((long)id.Value,counted.Value,reason.Text);await RefreshAll();});d.ShowDialog(this);return Task.CompletedTask;
            });
        }
        if(user.Role is "Admin" or "Accountant")
        {
            AddButton(actions,"Khóa ca",()=>
            {
                using var d=Dialog("Duyệt và khóa ca");var id=Ui.Number(int.MaxValue);d.Add("Mã ca đã bàn giao",id);
                d.Action("KHÓA CA",async()=>{await service.LockCashShiftAsync((long)id.Value);await RefreshAll();});d.ShowDialog(this);return Task.CompletedTask;
            });
            AddButton(actions,"Khóa kỳ tháng",()=>
            {
                using var d=Dialog("Khóa kỳ kế toán");var month=Ui.DatePicker(DateTime.Today.AddMonths(-1));var note=Ui.Text(300);
                d.Add("Tháng cần khóa",month);d.Add("Ghi chú",note);
                d.Action("KHÓA KỲ",async()=>{await service.LockAccountingPeriodAsync(month.Value,note.Text);await RefreshAll();});d.ShowDialog(this);return Task.CompletedTask;
            });
        }
        AddButton(actions,"In biên bản ca",()=>{PrintSelected("Ca trực");return Task.CompletedTask;});
    }
    private void BuildVoucherTab(FlowLayoutPanel actions)
    {
        AddButton(actions,"Nhập số dư mở sổ",()=>
        {
            using var d=Dialog("Số dư khi bắt đầu dùng phân hệ");var channel=Ui.Combo(new[]{"Cash","Bank","POS","OTA"});var asOf=Ui.DatePicker(DateTime.Today);var amount=Ui.Money();var note=Ui.Text(300);
            d.Add("Kênh",channel);d.Add("Ngày mở sổ",asOf);d.Add("Số dư thực tế",amount);d.Add("Biên bản đối chiếu",note);
            d.Action("GHI SỐ DƯ",async()=>{await service.SetOpeningBalanceAsync((string)channel.SelectedItem!,asOf.Value,amount.Value,note.Text);if(from.Value.Date<asOf.Value.Date)from.Value=asOf.Value.Date;await RefreshAll();});d.ShowDialog(this);return Task.CompletedTask;
        });
        AddButton(actions,"Lập phiếu thu / chi",()=>
        {
            using var d=Dialog("Phiếu thu / chi");var type=Ui.Combo(new[]{new Choice("Receipt","Phiếu thu"),new Choice("Payment","Phiếu chi")});var channel=Ui.Combo(new[]{new Choice("Cash","Tiền mặt"),new Choice("Bank","Chuyển khoản"),new Choice("POS","Thẻ POS"),new Choice("OTA","Công nợ OTA")});
            var category=Ui.Combo(new[]{"Tiền cọc chưa ghi doanh thu","Thu hồi công nợ","Thanh lý tài sản","Bồi thường","Mua hàng tồn kho","Điện nước","Internet","Bảo trì","Giặt ủi","Tạm ứng","Trả công nợ","Chi phí khác"});
            var amount=Ui.Money();var party=Ui.Text(150);var reference=Ui.Text(100);var note=Ui.Text(500);
            d.Add("Loại",type);d.Add("Kênh",channel);d.Add("Hạng mục",category);d.Add("Số tiền",amount);d.Add("Đối tượng",party);d.Add("Mã tham chiếu QR/POS",reference);d.Add("Diễn giải",note);
            d.Action("GHI SỔ",async()=>{await service.PostVoucherAsync(((Choice)type.SelectedItem!).Code,((Choice)channel.SelectedItem!).Code,(string)category.SelectedItem!,amount.Value,party.Text,reference.Text,note.Text);await RefreshAll();});d.ShowDialog(this);return Task.CompletedTask;
        });
        AddButton(actions,"In phiếu",()=>{PrintSelected("Sổ quỹ");return Task.CompletedTask;});
    }
    private void BuildDebtTab(FlowLayoutPanel actions)
    {
        AddButton(actions,"Ghi công nợ",()=>
        {
            using var d=Dialog("Ghi công nợ phải thu / trả");var type=Ui.Combo(new[]{new Choice("AR","Phải thu"),new Choice("AP","Phải trả")});var party=Ui.Text(150);var invoice=Ui.Text(30);var due=Ui.DatePicker(DateTime.Today.AddDays(30));var amount=Ui.Money();var note=Ui.Text(500);
            d.Add("AR phải thu / AP phải trả",type);d.Add("Đối tượng",party);d.Add("Mã hóa đơn (nếu có)",invoice);d.Add("Hạn thanh toán",due);d.Add("Số gốc",amount);d.Add("Diễn giải",note);
            d.Action("GHI NỢ",async()=>{long? bill=string.IsNullOrWhiteSpace(invoice.Text)?null:long.Parse(invoice.Text);await service.CreateDebtAsync(((Choice)type.SelectedItem!).Code,party.Text,bill,due.Value,amount.Value,note.Text);await RefreshAll();});d.ShowDialog(this);return Task.CompletedTask;
        });
        AddButton(actions,"Gạch nợ theo phiếu",()=>
        {
            using var d=Dialog("Gạch nợ từng đợt");var debt=Ui.Number(int.MaxValue);var voucher=Ui.Number(int.MaxValue);var amount=Ui.Money();
            d.Add("Mã công nợ",debt);d.Add("Mã phiếu thu/chi",voucher);d.Add("Số gạch nợ",amount);
            d.Action("GẠCH NỢ",async()=>{await service.AllocateDebtAsync((long)debt.Value,(long)voucher.Value,amount.Value);await RefreshAll();});d.ShowDialog(this);return Task.CompletedTask;
        });
        AddButton(actions,"In báo cáo công nợ",()=>{PrintGrid("Công nợ",true);return Task.CompletedTask;});
    }
    private void BuildBankTab(FlowLayoutPanel actions)
    {
        AddButton(actions,"Đối chiếu lại",async()=>{var count=await service.ReconcilePendingBankAsync();await RefreshAll();MessageBox.Show(this,$"Đã khớp thêm {count} dòng.","Đối soát");});
        AddButton(actions,"Nhập sao kê",()=>
        {
            using var d=Dialog("Nhập dòng sao kê QR/POS");
            var lines=new TextBox {Multiline=true,ScrollBars=ScrollBars.Both,WordWrap=false,Height=300,Dock=DockStyle.Top};
            d.Add("Mỗi dòng: yyyy-MM-dd HH:mm; Bank/POS; mã giao dịch; số tiền có dấu",lines,300);
            d.Note("Ví dụ: 2026-09-29 14:30; Bank; FT123456; 500000. Số âm là khoản hoàn/chi. Mã và số tiền phải khớp duy nhất.");
            d.Action("NHẬP VÀ ĐỐI CHIẾU",async()=>
            {
                var records=lines.Lines.Where(x=>!string.IsNullOrWhiteSpace(x)).Select(x=>
                {
                    var p=x.Split(';');if(p.Length!=4)throw new BusinessException("Mỗi dòng sao kê cần 4 trường cách nhau bởi dấu ;.");
                    return (DateTime.ParseExact(p[0].Trim(),"yyyy-MM-dd HH:mm",System.Globalization.CultureInfo.InvariantCulture),p[1].Trim(),p[2].Trim(),decimal.Parse(p[3].Trim(),System.Globalization.CultureInfo.InvariantCulture));
                }).ToList();
                var matched=await service.ImportBankStatementsAsync(records);await RefreshAll();
                MessageBox.Show(d,$"Đã khớp {matched}/{records.Count} dòng.","Đối soát");
            });d.ShowDialog(this);return Task.CompletedTask;
        });
    }
    private void BuildStockTab(FlowLayoutPanel actions)
    {
        AddButton(actions,"Thêm mặt hàng",()=>
        {
            using var d=Dialog("Mặt hàng kho");var name=Ui.Text(150);var unit=Ui.Text(30);var serviceId=Ui.Text(20);var reorder=Ui.Money();
            d.Add("Tên mặt hàng",name);d.Add("Đơn vị",unit);d.Add("Mã dịch vụ liên kết",serviceId);d.Add("Ngưỡng nhập",reorder);
            d.Action("LƯU",async()=>{int? id=string.IsNullOrWhiteSpace(serviceId.Text)?null:int.Parse(serviceId.Text);await service.AddStockItemAsync(name.Text,unit.Text,id,reorder.Value);await RefreshAll();});d.ShowDialog(this);return Task.CompletedTask;
        });
        AddButton(actions,"Nhập / xuất kho",()=>
        {
            using var d=Dialog("Biến động kho");var item=Ui.Number(int.MaxValue);var kind=Ui.Combo(new[]{new Choice("Purchase","Nhập mua"),new Choice("Spoilage","Xuất hủy/hư hỏng"),new Choice("Internal","Xuất dùng nội bộ"),new Choice("Adjustment","Điều chỉnh tăng tồn")});var quantity=Ui.Money();var cost=Ui.Money();var reason=Ui.Text(500);
            d.Add("Mã hàng",item);d.Add("Loại",kind);d.Add("Số lượng",quantity);d.Add("Đơn giá vốn (khi nhập)",cost);d.Add("Lý do",reason);
            d.Action("GHI KHO",async()=>{await service.MoveStockAsync((int)item.Value,((Choice)kind.SelectedItem!).Code,quantity.Value,cost.Value,reason.Text);await RefreshAll();});d.ShowDialog(this);return Task.CompletedTask;
        });
        AddButton(actions,"Buồng phòng báo dùng",()=>
        {
            using var d=Dialog("Báo tiêu thụ buồng phòng");var item=Ui.Number(int.MaxValue);var stay=Ui.Text(30);var quantity=Ui.Money();var note=Ui.Text(300);
            d.Add("Mã hàng",item);d.Add("Mã lượt ở",stay);d.Add("Số lượng",quantity);d.Add("Ghi chú",note);
            d.Action("BÁO DÙNG",async()=>{long? id=string.IsNullOrWhiteSpace(stay.Text)?null:long.Parse(stay.Text);await service.ReportHousekeepingAsync((int)item.Value,id,quantity.Value,note.Text);await RefreshAll();});d.ShowDialog(this);return Task.CompletedTask;
        });
    }
    private void BuildInvoiceTab(FlowLayoutPanel actions)
    {
        AddButton(actions,"Giảm trừ",()=>
        {
            using var d=Dialog("Phê duyệt giảm trừ");var id=Ui.Number(int.MaxValue);var kind=Ui.Combo(new[]{new Choice("ServiceFailure","Dịch vụ kém"),new Choice("VIP","Khách VIP"),new Choice("Voucher","Voucher")});var amount=Ui.Money();var reason=Ui.Text(500);
            d.Add("Mã hóa đơn",id);d.Add("Loại",kind);d.Add("Số giảm",amount);d.Add("Lý do",reason);
            d.Action("PHÊ DUYỆT",async()=>{await service.AddInvoiceAdjustmentAsync((long)id.Value,((Choice)kind.SelectedItem!).Code,amount.Value,reason.Text);await RefreshAll();});d.ShowDialog(this);return Task.CompletedTask;
        });
        AddButton(actions,"Hủy hóa đơn",()=>
        {
            using var d=Dialog("Hủy hóa đơn có vết");var id=Ui.Number(int.MaxValue);var kind=Ui.Combo(new[]{new Choice("GuestCancelled","Khách hủy"),new Choice("WrongRoomType","Nhập sai hạng phòng"),new Choice("RoomChange","Đổi phòng")});var reason=Ui.Text(500);
            d.Add("Mã hóa đơn",id);d.Add("Lý do chuẩn",kind);d.Add("Giải trình",reason);
            d.Note("Hủy hóa đơn không hoàn tiền tự động. Nếu đã thu, lập chứng từ hoàn/điều chỉnh riêng và đối chiếu tiền thực tế.");
            d.Action("XÁC NHẬN HỦY",async()=>{await service.VoidInvoiceAsync((long)id.Value,((Choice)kind.SelectedItem!).Code,reason.Text);await RefreshAll();});d.ShowDialog(this);return Task.CompletedTask;
        });
        AddButton(actions,"Đánh dấu e-Invoice",()=>
        {
            using var d=Dialog("Hóa đơn điện tử đã phát hành");var id=Ui.Number(int.MaxValue);var number=Ui.Text(100);var vat=Ui.Combo(new[]{"8","10"});var fee=Ui.Combo(new[]{"0","5"});var round=Ui.Combo(new[]{"1","100","500","1000"});
            d.Add("Mã hóa đơn",id);d.Add("Số hóa đơn điện tử",number);d.Add("VAT %",vat);d.Add("Phí phục vụ %",fee);d.Add("Làm tròn VNĐ",round);
            d.Action("GHI NHẬN",async()=>{await service.MarkEInvoiceAsync((long)id.Value,number.Text,decimal.Parse((string)vat.SelectedItem!),decimal.Parse((string)fee.SelectedItem!),int.Parse((string)round.SelectedItem!));await RefreshAll();});d.ShowDialog(this);return Task.CompletedTask;
        });
        AddButton(actions,"Tách / gộp nhóm bill",()=>
        {
            using var d=Dialog("Tách / gộp phần thanh toán");var name=Ui.Text(150);var lines=new TextBox {Multiline=true,ScrollBars=ScrollBars.Vertical,Height=130,Dock=DockStyle.Top};
            d.Add("Tên đoàn / hóa đơn tổng",name);d.Add("Mỗi dòng: mã hóa đơn; người trả; số tiền",lines,130);
            d.Note("Mỗi hóa đơn phải được phân bổ đủ tổng tiền. Nhiều dòng cùng mã là tách; nhiều mã trong một nhóm là gộp.");
            d.Action("TẠO NHÓM",async()=>{var shares=lines.Lines.Where(x=>!string.IsNullOrWhiteSpace(x)).Select(x=>{var p=x.Split(';');if(p.Length!=3)throw new BusinessException("Mỗi dòng cần 3 trường cách nhau bởi dấu ;.");return (long.Parse(p[0]),p[1],decimal.Parse(p[2]));}).ToList();await service.GroupBillsAsync(name.Text,shares);await RefreshAll();});d.ShowDialog(this);return Task.CompletedTask;
        });
    }
    private void BuildProfitTab(FlowLayoutPanel actions)
    {
        AddButton(actions,"Xem chỉ số",RefreshAll);
    }
    private void BuildBillGroupTab(FlowLayoutPanel actions)
    {
        AddButton(actions,"Tạo từ tab Hóa đơn",()=>{tabs.SelectedTab=tabs.TabPages.Cast<TabPage>().Single(x=>x.Text=="Hóa đơn");return Task.CompletedTask;});
    }
    private void PrintSelected(string title) => PrintGrid(title,false);
    private void PrintGrid(string title,bool all)
    {
        var grid=grids[title];if(!all && grid.CurrentRow is null)throw new BusinessException("Chọn một dòng để in.");
        var columns=grid.Columns.Cast<DataGridViewColumn>().Where(c=>c.Visible).OrderBy(c=>c.DisplayIndex).ToArray();
        var rows=all?grid.Rows.Cast<DataGridViewRow>().Where(x=>!x.IsNewRow).ToArray():new[]{grid.CurrentRow!};
        var lines=rows.SelectMany(row=>new[]{new string('─',45)}.Concat(columns.Select(c=>$"{c.HeaderText}: {row.Cells[c.Index].FormattedValue}"))).ToArray();
        using var document=new System.Drawing.Printing.PrintDocument {DocumentName=title};
        var index=0;document.BeginPrint+=(_,_)=>index=0;
        document.PrintPage+=(_,e)=>
        {
            if(e.Graphics is null)return;
            using var head=new Font("Segoe UI",16,FontStyle.Bold);using var body=new Font("Segoe UI",10);
            var y=e.MarginBounds.Top;e.Graphics.DrawString(AppSettings.Load().HotelName,head,Brushes.Black,e.MarginBounds.Left,y);y+=40;
            e.Graphics.DrawString(title+" • "+DateTime.Now.ToString("dd/MM/yyyy HH:mm"),body,Brushes.Black,e.MarginBounds.Left,y);y+=35;
            while(index<lines.Length && y+75<e.MarginBounds.Bottom)
            {e.Graphics.DrawString(lines[index++],body,Brushes.Black,new RectangleF(e.MarginBounds.Left,y,e.MarginBounds.Width,30));y+=30;}
            if(index<lines.Length)e.HasMorePages=true;
            else e.Graphics.DrawString("Người lập                                      Người duyệt",body,Brushes.Black,e.MarginBounds.Left,y+35);
        };
        using var preview=new PrintPreviewDialog {Document=document,Width=900,Height=750};preview.ShowDialog(this);
    }
}
