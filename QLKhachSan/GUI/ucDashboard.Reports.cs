using QLKhachSan.DAL;
using QLKhachSan.DTO;
using QLKhachSan.BLL;

namespace QLKhachSan.GUI;

public partial class ucDashboard
{
    private async Task ShowCheckout(Stay stay)
    {
        var bill=await service.QuoteAsync(stay);
        var screen=Screen.FromControl(this).WorkingArea;
        using var dialog=new InputDialog($"Thanh toán P.{bill.Room.Number}",900,Math.Min(760,screen.Height-40))
        {
            MaximizeBox=false,
            MaximumSize=new Size(Math.Min(1000,screen.Width-40),screen.Height-40)
        };
        var settings=AppSettings.Load();
        var billPanel=new RoundedSurface {Height=360,ColumnCount=2,RowCount=1,
            BackColor=Color.White,Padding=new Padding(10),Margin=new Padding(0,0,0,14)};
        billPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
        billPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,0));
        billPanel.RowStyles.Add(new RowStyle(SizeType.Percent,100));
        var invoice=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=1,RowCount=5,Padding=new Padding(8)};
        invoice.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
        foreach(var height in new[]{34,27,48,130,75})invoice.RowStyles.Add(new RowStyle(SizeType.Absolute,height));
        invoice.Controls.Add(new Label {Text=settings.HotelName.ToUpperInvariant(),Dock=DockStyle.Fill,Font=AppTheme.Title,ForeColor=AppTheme.Ink},0,0);
        invoice.Controls.Add(new Label {Text=$"PHIẾU THANH TOÁN  •  P.{bill.Room.Number}",Dock=DockStyle.Fill,Font=AppTheme.Bold,ForeColor=Color.FromArgb(177,75,59)},0,1);
        invoice.Controls.Add(new Label {Text=$"Khách: {stay.Guest}  •  SĐT: {stay.Phone}\nNhận: {stay.CheckIn:dd/MM/yyyy HH:mm}  •  Tính đến: {bill.At:dd/MM/yyyy HH:mm}",Dock=DockStyle.Fill,ForeColor=AppTheme.Muted},0,2);
        var lines=new ListView {Dock=DockStyle.Fill,View=View.Details,FullRowSelect=true,
            GridLines=false,BorderStyle=BorderStyle.None,Font=AppTheme.Body,
            HeaderStyle=ColumnHeaderStyle.Nonclickable,BackColor=Color.FromArgb(248,250,254)};
        lines.Columns.Add("HẠNG MỤC",180);lines.Columns.Add("SL",45,HorizontalAlignment.Right);
        lines.Columns.Add("ĐƠN GIÁ",100,HorizontalAlignment.Right);lines.Columns.Add("THÀNH TIỀN",110,HorizontalAlignment.Right);
        void AddLine(string name,string quantity,string price,decimal total)
        {
            var row=new ListViewItem(name);row.SubItems.Add(quantity);row.SubItems.Add(price);row.SubItems.Add($"{total:N0} đ");lines.Items.Add(row);
        }
        AddLine("Tiền phòng","1","",bill.RoomCharge);
        foreach(var line in bill.Lines.Where(x=>x.Cancelled is null))AddLine(line.Name,line.Quantity.ToString(),$"{line.Price:N0} đ",line.Total);
        invoice.Controls.Add(lines,0,3);
        invoice.Controls.Add(new Label {Text=$"Tổng hóa đơn: {bill.Total:N0} đ    •    Cọc đã thu: {stay.Deposit:N0} đ\n{(bill.ToCollect>0?$"CẦN THU: {bill.ToCollect:N0} đ":$"CẦN HOÀN: {bill.ToRefund:N0} đ")}\nBảng tính có hiệu lực 10 phút.",Dock=DockStyle.Fill,Font=AppTheme.Bold,ForeColor=AppTheme.Ink,TextAlign=ContentAlignment.MiddleRight},0,4);
        billPanel.Controls.Add(invoice,0,0);
        var bank=new RoundedSurface {Dock=DockStyle.Fill,ColumnCount=1,RowCount=3,
            Padding=new Padding(12),BackColor=Color.FromArgb(239,245,255)};
        bank.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
        bank.RowStyles.Add(new RowStyle(SizeType.Absolute,32));bank.RowStyles.Add(new RowStyle(SizeType.Absolute,88));bank.RowStyles.Add(new RowStyle(SizeType.Percent,100));
        bank.Controls.Add(new Label {Text="QUÉT QR CHUYỂN KHOẢN",Dock=DockStyle.Fill,Font=AppTheme.Bold,ForeColor=AppTheme.Ink},0,0);
        var bankInfo=new Label {Dock=DockStyle.Fill,ForeColor=AppTheme.Ink};bank.Controls.Add(bankInfo,0,1);
        var qr=new PictureBox {Dock=DockStyle.Fill,SizeMode=PictureBoxSizeMode.Zoom,BackColor=Color.White};bank.Controls.Add(qr,0,2);
        billPanel.Controls.Add(bank,1,0);
        dialog.Add("",billPanel);
        var method=Ui.Combo(new[]{"Tiền mặt","Chuyển khoản","Thẻ POS","Công nợ OTA"});var reference=Ui.Text(100);dialog.Add("Phương thức thanh toán / hoàn tiền",method);dialog.Add("Mã giao dịch QR/POS/OTA",reference);
        void UpdatePayment()
        {
            var transfer=(string?)method.SelectedItem=="Chuyển khoản" && bill.ToCollect>0;
            if(transfer && string.IsNullOrWhiteSpace(reference.Text))reference.Text=$"KS-{stay.Id}";
            bank.Visible=transfer;
            billPanel.ColumnStyles[1].Width=transfer?315:0;
            if(!transfer){qr.CancelAsync();return;}
            bankInfo.Text=$"{settings.BankAccountName}\nSTK: {settings.BankAccount}\nSố tiền: {bill.ToCollect:N0} đ\nNội dung: {reference.Text}";
            if(string.IsNullOrWhiteSpace(settings.BankCode) || string.IsNullOrWhiteSpace(settings.BankAccount) || string.IsNullOrWhiteSpace(settings.BankAccountName))
            {
                bankInfo.Text="Chưa cấu hình tài khoản ngân hàng.";return;
            }
            var url=$"https://img.vietqr.io/image/{Uri.EscapeDataString(settings.BankCode)}-{Uri.EscapeDataString(settings.BankAccount)}-qr_only.png?amount={bill.ToCollect:0}&addInfo={Uri.EscapeDataString(reference.Text)}&accountName={Uri.EscapeDataString(settings.BankAccountName)}";
            qr.CancelAsync();
            try{qr.LoadAsync(url);}
            catch(InvalidOperationException){bankInfo.Text+="\nKhông tải được QR; dùng thông tin tài khoản ở trên.";}
        }
        method.SelectedIndexChanged+=(_,_)=>UpdatePayment();
        reference.TextChanged+=(_,_)=>{if((string?)method.SelectedItem=="Chuyển khoản")UpdatePayment();};
        UpdatePayment();
        dialog.FormClosed+=(_,_)=>qr.CancelAsync();
        dialog.Action("XEM / IN BILL TRƯỚC THANH TOÁN",
            ()=>ShowBillBeforePayment(new[]{bill},(string)method.SelectedItem!,reference.Text),false);
        var confirm=new CheckBox {Text="Đã thu đủ tiền / hoàn đủ tiền cho khách"};dialog.AddActionConfirmation("Xác nhận thu chi",confirm);
        var checkoutAction=dialog.Action("HOÀN TẤT CHECK-OUT",async()=>
        {
            if(!confirm.Checked)throw new BusinessException("Cần xác nhận đã hoàn tất thu/hoàn tiền.");
            long invoiceId=0;
            await Changed(async()=>invoiceId=await service.CheckoutAsync(bill,(string)method.SelectedItem!,reference.Text));
            MessageBox.Show(dialog,$"Đã lưu hóa đơn #{invoiceId}. Phòng chuyển sang đang dọn.","Hoàn tất");
        });
        if(data.Stays.Count(x=>x.CustomerId==stay.CustomerId && x.Status==StayStatus.Occupied)>1)
            dialog.Action("THANH TOÁN CÁC PHÒNG CÙNG KHÁCH",async()=>
            {
                if(await ShowGroupCheckout(stay))dialog.Close();
            },false);
        dialog.AcceptButton=checkoutAction;
        dialog.ShowDialog(this);
    }
    private async void btnQuanLyKhach_Click(object? sender,EventArgs e)=>await Run(async()=>
    {
        using var dialog=new Form {Text="Hồ sơ khách hàng",Size=new Size(1050,800),MinimumSize=new Size(820,600),StartPosition=FormStartPosition.CenterParent,Font=AppTheme.Body,BackColor=AppTheme.Canvas};
        var root=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=1,RowCount=4,Padding=new Padding(20,16,20,20)};
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute,76));root.RowStyles.Add(new RowStyle(SizeType.Absolute,64));
        root.RowStyles.Add(new RowStyle(SizeType.Percent,46));root.RowStyles.Add(new RowStyle(SizeType.Percent,54));dialog.Controls.Add(root);
        var heading=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=1,RowCount=2};
        heading.RowStyles.Add(new RowStyle(SizeType.Absolute,40));heading.RowStyles.Add(new RowStyle(SizeType.Absolute,28));
        heading.Controls.Add(new Label {Text="Hồ sơ khách hàng",Dock=DockStyle.Fill,Font=AppTheme.Title,ForeColor=AppTheme.Ink},0,0);
        heading.Controls.Add(new Label {Text="Tra cứu khách và xem lịch sử dịch vụ, hóa đơn",Dock=DockStyle.Fill,ForeColor=AppTheme.Muted},0,1);root.Controls.Add(heading,0,0);
        var searchRow=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=2,RowCount=1,Margin=new Padding(0,0,0,12)};
        searchRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));searchRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,140));
        var search=Ui.Text();search.PlaceholderText="Tìm theo họ tên, số điện thoại hoặc CCCD / hộ chiếu";search.Dock=DockStyle.Fill;
        var searchButton=new Button {Text="TÌM KHÁCH",Dock=DockStyle.Fill,Margin=new Padding(8,0,0,0)};AppTheme.Button(searchButton,true);
        searchRow.Controls.Add(search,0,0);searchRow.Controls.Add(searchButton,1,0);root.Controls.Add(searchRow,0,1);
        var grid=Ui.Grid();grid.Dock=DockStyle.Fill;grid.AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill;
        var customerCard=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=1,RowCount=2,BackColor=Color.White,Padding=new Padding(12),Margin=new Padding(0,0,0,12)};
        customerCard.RowStyles.Add(new RowStyle(SizeType.Absolute,34));customerCard.RowStyles.Add(new RowStyle(SizeType.Percent,100));
        customerCard.Controls.Add(new Label {Text="DANH SÁCH KHÁCH HÀNG",Dock=DockStyle.Fill,Font=AppTheme.Bold,ForeColor=AppTheme.Ink},0,0);
        customerCard.Controls.Add(grid,0,1);root.Controls.Add(customerCard,0,2);
        var tabs=new TabControl {Dock=DockStyle.Fill,Font=AppTheme.Bold};
        var serviceTab=new TabPage("DỊCH VỤ ĐÃ SỬ DỤNG") {BackColor=Color.White,Padding=new Padding(10)};
        var invoiceTab=new TabPage("HÓA ĐƠN") {BackColor=Color.White,Padding=new Padding(10)};
        var history=Ui.Grid();history.Dock=DockStyle.Fill;history.AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill;
        var invoices=Ui.Grid();invoices.Dock=DockStyle.Fill;invoices.AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill;
        serviceTab.Controls.Add(history);invoiceTab.Controls.Add(invoices);tabs.TabPages.Add(serviceTab);tabs.TabPages.Add(invoiceTab);root.Controls.Add(tabs,0,3);
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
            if(grid.Columns["Name"] is { } name)name.FillWeight=130;
            if(grid.Columns["Visits"] is { } visits)visits.FillWeight=80;
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
                    history.DataSource=rows.Select(x=>new {Ngày=x.Ordered,TênDịchVụ=x.Name,SốLượng=x.Quantity,ThànhTiền=x.Total,TrạngThái=x.Cancelled is null?"Có hiệu lực":"Đã hủy"}).ToList();
                    if(history.Columns["Ngày"] is { } date)date.DefaultCellStyle.Format="dd/MM/yyyy HH:mm";
                    if(history.Columns["ThànhTiền"] is { } amount)amount.DefaultCellStyle.Format="N0";
                    var bills=await service.CustomerInvoicesAsync(customer.Id);
                    if(dialog.IsDisposed)break;
                    invoices.DataSource=bills.Select(x=>new {MãHóaĐơn=x.Id,Phòng=x.Room,NgàyLập=x.Issued,TổngTiền=x.Total,HìnhThức=x.Method}).ToList();
                    if(invoices.Columns["NgàyLập"] is { } issued)issued.DefaultCellStyle.Format="dd/MM/yyyy HH:mm";
                    if(invoices.Columns["TổngTiền"] is { } total)total.DefaultCellStyle.Format="N0";
                    break;
                }
            }
            catch(Exception ex){if(!dialog.IsDisposed)Ui.Error(dialog,ex);}
            finally{historyBusy=false;}
        };
        searchButton.Click+=async (_,_)=>{try{await LoadCustomers();}catch(Exception ex){Ui.Error(dialog,ex);}};
        search.KeyDown+=(_,e)=>{if(e.KeyCode==Keys.Enter){e.SuppressKeyPress=true;searchButton.PerformClick();}};
        await LoadCustomers();dialog.ShowDialog(this);
    });
    private async Task ShowInvoices()
    {
        using var dialog=new Form {Text="Hóa đơn, doanh thu và thu chi",Size=new Size(1180,800),MinimumSize=new Size(940,650),StartPosition=FormStartPosition.CenterParent,Font=AppTheme.Body,BackColor=AppTheme.Canvas,AutoScaleMode=AutoScaleMode.Dpi};
        var root=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=1,RowCount=5};
        root.RowStyles.Add(new RowStyle(SizeType.Absolute,76));root.RowStyles.Add(new RowStyle(SizeType.Absolute,78));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute,116));root.RowStyles.Add(new RowStyle(SizeType.Percent,100));root.RowStyles.Add(new RowStyle(SizeType.Absolute,76));dialog.Controls.Add(root);
        var heading=new Panel {Dock=DockStyle.Fill,BackColor=Color.White,Padding=new Padding(24,10,20,8)};
        heading.Controls.Add(new Label {Text="Hóa đơn, doanh thu và thu chi",Dock=DockStyle.Top,Height=36,Font=AppTheme.Title,ForeColor=AppTheme.Ink});
        heading.Controls.Add(new Label {Text="Theo dõi doanh thu, dòng tiền và chi tiết hóa đơn theo khoảng ngày",Dock=DockStyle.Bottom,Height=22,ForeColor=AppTheme.Muted});root.Controls.Add(heading,0,0);
        var filters=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=3,RowCount=2,Padding=new Padding(22,8,22,4)};
        filters.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,220));filters.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,220));filters.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
        filters.RowStyles.Add(new RowStyle(SizeType.Absolute,24));filters.RowStyles.Add(new RowStyle(SizeType.Percent,100));root.Controls.Add(filters,0,1);
        filters.Controls.Add(new Label {Text="Từ ngày",ForeColor=AppTheme.Muted,Dock=DockStyle.Fill},0,0);
        filters.Controls.Add(new Label {Text="Đến hết ngày",ForeColor=AppTheme.Muted,Dock=DockStyle.Fill},1,0);
        var day=Ui.DatePicker(ServerNow.Date);day.CustomFormat="dd/MM/yyyy";day.Dock=DockStyle.Fill;day.Margin=new Padding(0,0,12,0);filters.Controls.Add(day,0,1);
        var through=Ui.DatePicker(ServerNow.Date);through.CustomFormat="dd/MM/yyyy";through.Dock=DockStyle.Fill;through.Margin=new Padding(0,0,12,0);filters.Controls.Add(through,1,1);
        var refresh=new Button {Text="XEM BÁO CÁO",Dock=DockStyle.Right,Width=180};AppTheme.Button(refresh,true);filters.Controls.Add(refresh,2,1);
        var metrics=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=5,RowCount=1,Padding=new Padding(22,8,22,8)};
        for(var i=0;i<5;i++)metrics.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,20));root.Controls.Add(metrics,0,2);
        Label Metric(string title,int column)
        {
            var card=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=1,RowCount=2,BackColor=Color.White,Padding=new Padding(14,12,10,8),Margin=new Padding(column==0?0:6,0,column==4?0:6,0)};
            card.RowStyles.Add(new RowStyle(SizeType.Absolute,28));card.RowStyles.Add(new RowStyle(SizeType.Percent,100));
            card.Controls.Add(new Label {Text=title,Dock=DockStyle.Fill,Font=AppTheme.Bold,ForeColor=AppTheme.Muted},0,0);
            var value=new Label {Dock=DockStyle.Fill,Font=new Font("Segoe UI Semibold",15),ForeColor=column==4?AppTheme.Teal:AppTheme.Ink,AutoEllipsis=true};card.Controls.Add(value,0,1);metrics.Controls.Add(card,column,0);return value;
        }
        var invoiceTotal=Metric("DOANH THU HÓA ĐƠN",0);var deposits=Metric("THU CỌC",1);
        var refunds=Metric("HOÀN CỌC",2);var forfeits=Metric("CỌC KHÔNG HOÀN",3);var netCash=Metric("DÒNG TIỀN THUẦN",4);
        var tabs=new TabControl {Dock=DockStyle.Fill,Margin=new Padding(22,4,22,8),Font=AppTheme.Bold};root.Controls.Add(tabs,0,3);
        var invoiceTab=new TabPage("Hóa đơn") {BackColor=Color.White,Padding=new Padding(10)};
        var depositTab=new TabPage("Phiếu cọc") {BackColor=Color.White,Padding=new Padding(10)};
        var cashTab=new TabPage("Thu chi") {BackColor=Color.White,Padding=new Padding(10)};
        var categoryTab=new TabPage("Theo hạng mục") {BackColor=Color.White,Padding=new Padding(16)};
        tabs.TabPages.AddRange([invoiceTab,depositTab,cashTab,categoryTab]);
        var invoiceLayout=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=1,RowCount=2};invoiceLayout.RowStyles.Add(new RowStyle(SizeType.Percent,100));invoiceLayout.RowStyles.Add(new RowStyle(SizeType.Absolute,58));invoiceTab.Controls.Add(invoiceLayout);
        var grid=Ui.Grid();grid.AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill;invoiceLayout.Controls.Add(grid,0,0);
        var invoiceDetail=new Label {Dock=DockStyle.Fill,BackColor=AppTheme.Canvas,Padding=new Padding(12,7,8,4),ForeColor=AppTheme.Ink};invoiceLayout.Controls.Add(invoiceDetail,0,1);
        var depositLayout=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=1,RowCount=3};
        depositLayout.RowStyles.Add(new RowStyle(SizeType.Percent,100));depositLayout.RowStyles.Add(new RowStyle(SizeType.Absolute,58));
        depositLayout.RowStyles.Add(new RowStyle(SizeType.Absolute,48));depositTab.Controls.Add(depositLayout);
        var depositGrid=Ui.Grid();depositGrid.AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill;depositLayout.Controls.Add(depositGrid,0,0);
        var depositDetail=new Label {Dock=DockStyle.Fill,BackColor=AppTheme.Canvas,Padding=new Padding(12,7,8,4),ForeColor=AppTheme.Ink};depositLayout.Controls.Add(depositDetail,0,1);
        var printDeposit=new Button {Text="XEM / IN PHIẾU CỌC",Dock=DockStyle.Right,Width=220,Margin=new Padding(0,6,0,0)};
        AppTheme.Button(printDeposit,true);depositLayout.Controls.Add(printDeposit,0,2);
        printDeposit.Click+=(_,_)=>
        {
            if(depositGrid.CurrentRow?.DataBoundItem is not DepositReceipt receipt){Ui.Error(dialog,new BusinessException("Chọn phiếu cọc cần in."));return;}
            PrintDepositReceipt(receipt);
        };
        var cashLayout=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=1,RowCount=2};cashLayout.RowStyles.Add(new RowStyle(SizeType.Percent,100));cashLayout.RowStyles.Add(new RowStyle(SizeType.Absolute,58));cashTab.Controls.Add(cashLayout);
        var cash=Ui.Grid();cash.AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill;cashLayout.Controls.Add(cash,0,0);
        var cashDetail=new Label {Dock=DockStyle.Fill,BackColor=AppTheme.Canvas,Padding=new Padding(12,7,8,4),ForeColor=AppTheme.Ink};cashLayout.Controls.Add(cashDetail,0,1);
        var categoryLayout=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=1,RowCount=2};
        categoryLayout.RowStyles.Add(new RowStyle(SizeType.Percent,100));categoryLayout.RowStyles.Add(new RowStyle(SizeType.Absolute,56));categoryTab.Controls.Add(categoryLayout);
        var categories=Ui.Grid();categories.AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill;categoryLayout.Controls.Add(categories,0,0);
        var categoryNote=new Label {Dock=DockStyle.Fill,BackColor=AppTheme.Canvas,Padding=new Padding(12,9,8,4),ForeColor=AppTheme.Muted};categoryLayout.Controls.Add(categoryNote,0,1);
        var footer=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=4,RowCount=1,Padding=new Padding(22,10,22,14)};
        for(var i=0;i<4;i++)footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,25));root.Controls.Add(footer,0,4);
        Button FooterButton(string title,int column,bool primary=false)
        {
            var button=new Button {Text=title,Dock=DockStyle.Fill,Margin=new Padding(column==0?0:5,0,column==3?0:5,0)};AppTheme.Button(button,primary);footer.Controls.Add(button,column,0);return button;
        }
        var exportInvoices=FooterButton("XUẤT HÓA ĐƠN CSV",0);var exportCash=FooterButton("XUẤT THU CHI CSV",1);
        var print=FooterButton("XEM / IN HÓA ĐƠN",2,true);var close=FooterButton("ĐÓNG",3);close.Click+=(_,_)=>dialog.Close();
        PeriodReport? currentReport=null;
        DateTime reportFrom=day.Value,reportThrough=through.Value;
        grid.SelectionChanged+=(_,_)=>
        {
            invoiceDetail.Text=grid.CurrentRow?.DataBoundItem is Invoice invoice
                ?$"Hóa đơn #{invoice.Id} • P.{invoice.Room} • {invoice.Guest}\nTổng: {invoice.Total:N0} đ  |  Cọc đã thu: {invoice.Deposit:N0} đ  |  Cần thu khi trả: {Math.Max(0,invoice.Total-invoice.Deposit):N0} đ  |  Đã thu thêm: {invoice.Collected:N0} đ  |  Đã hoàn: {invoice.Refunded:N0} đ"
                :"Chọn một hóa đơn để xem chi tiết.";
        };
        depositGrid.SelectionChanged+=(_,_)=>depositDetail.Text=depositGrid.CurrentRow?.DataBoundItem is DepositReceipt receipt
            ?$"Phiếu cọc #{receipt.PaymentId}  •  {receipt.Guest}  •  P.{receipt.Room}  •  {receipt.Phone}\nLần này: {receipt.Amount:N0} đ  |  Tổng cọc: {receipt.TotalDeposited:N0} đ  |  {(receipt.IsEstimate?"Dự kiến tiền phòng":"Tổng hóa đơn")}: {(receipt.FinalInvoiceTotal??receipt.EstimatedRoomCharge):N0} đ  |  {(receipt.IsEstimate?"Dự kiến còn lại, chưa gồm dịch vụ":"Cần thanh toán sau cọc")}: {receipt.Outstanding:N0} đ"
            :"Chọn phiếu cọc để xem số tiền và tình trạng thanh toán.";
        cash.SelectionChanged+=(_,_)=>
        {
            cashDetail.Text=cash.CurrentRow?.DataBoundItem is PaymentEntry payment
                ?$"{payment.Guest} • {payment.Method} • Nhân viên: {payment.Username}\n{payment.Note}"
                :"Chọn một dòng thu chi để xem ghi chú.";
        };
        cash.CellFormatting+=(_,e)=>
        {
            if(cash.Columns[e.ColumnIndex].Name!="Kind" || e.Value is not string kind)return;
            e.Value=kind switch {"Deposit"=>"Thu cọc","Checkout"=>"Thu checkout","Refund"=>"Hoàn cọc","Forfeit"=>"Cọc không hoàn",_=>kind};
            e.FormattingApplied=true;
        };
        async Task LoadReport()
        {
            var report=await service.PeriodReportAsync(day.Value,through.Value);
            var depositReceipts=await service.DepositReceiptsAsync(day.Value,through.Value);
            currentReport=report;
            reportFrom=day.Value;reportThrough=through.Value;
            var invoices=report.Invoices;
            var revenue=report.Revenue;
            grid.DataSource=invoices;
            depositGrid.DataSource=depositReceipts;
            foreach(DataGridViewColumn column in depositGrid.Columns)column.Visible=column.Name is "PaymentId" or "Guest" or "Room" or "PaidAt" or "Amount" or "TotalDeposited" or "Outstanding" or "Method";
            if(depositGrid.Columns["PaymentId"] is { } depositId)depositId.HeaderText="Phiếu cọc";
            if(depositGrid.Columns["Guest"] is { } depositGuest)depositGuest.HeaderText="Khách";
            if(depositGrid.Columns["Room"] is { } depositRoom)depositRoom.HeaderText="Phòng";
            if(depositGrid.Columns["PaidAt"] is { } paidAt){paidAt.HeaderText="Thu lúc";paidAt.DefaultCellStyle.Format="dd/MM/yyyy HH:mm";}
            foreach(var key in new[]{"Amount","TotalDeposited","Outstanding"})if(depositGrid.Columns[key] is { } money)money.DefaultCellStyle.Format="N0";
            if(depositGrid.Columns["Amount"] is { } paid)paid.HeaderText="Thu lần này";
            if(depositGrid.Columns["TotalDeposited"] is { } deposited)deposited.HeaderText="Tổng cọc";
            if(depositGrid.Columns["Outstanding"] is { } outstanding)outstanding.HeaderText="Còn lại / dự kiến";
            if(depositGrid.Columns["Method"] is { } depositMethod)depositMethod.HeaderText="Hình thức";
            foreach(DataGridViewColumn column in grid.Columns)column.Visible=column.Name is "Id" or "Room" or "Guest" or "Issued" or "Total" or "Collected" or "Method";
            if(grid.Columns["Id"] is { } id)id.HeaderText="Hóa đơn";
            if(grid.Columns["Room"] is { } room)room.HeaderText="Phòng";
            if(grid.Columns["Guest"] is { } guest)guest.HeaderText="Khách";
            if(grid.Columns["Issued"] is { } issued){issued.HeaderText="Lập lúc";issued.DefaultCellStyle.Format="dd/MM/yyyy HH:mm";}
            if(grid.Columns["Total"] is { } total){total.HeaderText="Tổng tiền";total.DefaultCellStyle.Format="N0";}
            if(grid.Columns["Collected"] is { } collected){collected.HeaderText="Thu thêm";collected.DefaultCellStyle.Format="N0";}
            if(grid.Columns["Method"] is { } method)method.HeaderText="Hình thức";
            var payments=report.Payments;
            invoiceTotal.Text=$"{invoices.Sum(x=>x.Total):N0} đ";deposits.Text=$"{payments.Where(p=>p.Kind=="Deposit").Sum(p=>p.Amount):N0} đ";
            refunds.Text=$"{payments.Where(p=>p.Kind=="Refund").Sum(p=>p.Amount):N0} đ";
            forfeits.Text=$"{payments.Where(p=>p.Kind=="Forfeit").Sum(p=>p.Amount):N0} đ";netCash.Text=$"{payments.Sum(p=>p.CashFlow):N0} đ";
            invoiceTab.Text=$"Hóa đơn ({invoices.Count})";depositTab.Text=$"Phiếu cọc ({depositReceipts.Count})";cashTab.Text=$"Thu chi ({payments.Count})";
            cash.DataSource=payments;
            foreach(DataGridViewColumn column in cash.Columns)column.Visible=column.Name is "Created" or "Guest" or "Kind" or "Amount" or "CashFlow" or "Method";
            if(cash.Columns["Created"] is { } date){date.HeaderText="Thời điểm";date.DefaultCellStyle.Format="dd/MM/yyyy HH:mm";}
            if(cash.Columns["Guest"] is { } customer)customer.HeaderText="Khách";
            if(cash.Columns["Kind"] is { } kind)kind.HeaderText="Loại";
            if(cash.Columns["Amount"] is { } amount){amount.HeaderText="Số tiền";amount.DefaultCellStyle.Format="N0";}
            if(cash.Columns["CashFlow"] is { } flow){flow.HeaderText="Thu − chi";flow.DefaultCellStyle.Format="N0";}
            if(cash.Columns["Method"] is { } paymentMethod)paymentMethod.HeaderText="Hình thức";
            categories.DataSource=revenue.Select(x=>new {HạngMục=x.Category,DoanhThu=x.Total}).ToList();
            if(categories.Columns["DoanhThu"] is { } categoryAmount)categoryAmount.DefaultCellStyle.Format="N0";
            categoryNote.Text="Cọc không hoàn là doanh thu ghi nhận từ tiền đã thu trước đó, không phải khoản thu mới. Dòng tiền thuần chỉ gồm tiền thực thu trừ tiền hoàn.";
            invoiceDetail.Text=grid.CurrentRow?.DataBoundItem is Invoice?invoiceDetail.Text:"Không có hóa đơn trong khoảng ngày này.";
            cashDetail.Text=cash.CurrentRow?.DataBoundItem is PaymentEntry?cashDetail.Text:"Không có dòng thu chi trong khoảng ngày này.";
        }
        refresh.Click+=async (_,_)=>{refresh.Enabled=false;try{await LoadReport();}catch(Exception ex){Ui.Error(dialog,ex);}finally{refresh.Enabled=true;}};
        exportInvoices.Click+=(_,_)=>{if(currentReport is not null)ExportInvoices(currentReport,reportFrom,reportThrough);};
        exportCash.Click+=(_,_)=>{if(currentReport is not null)ExportPayments(currentReport,reportFrom,reportThrough);};
        print.Click+=async (_,_)=>
        {
            print.Enabled=false;
            try
            {
                if(grid.CurrentRow?.DataBoundItem is not Invoice invoice)throw new BusinessException("Chọn hóa đơn đã thanh toán.");
                var stay=await service.InvoiceStayAsync(invoice.StayId);var lines=await service.InvoiceOrdersAsync(invoice.StayId);
                PrintInvoice(invoice,stay,lines);
            }
            catch(Exception ex){Ui.Error(dialog,ex);}
            finally{print.Enabled=true;}
        };
        await LoadReport();dialog.ShowDialog(this);
    }
    private async Task ShowAccounts()
    {
        using var dialog=new Form {Text=FunctionPolicy.Can(user,"staff.manage")?"Tài khoản cá nhân và phân quyền":"Tài khoản cá nhân",Size=new Size(920,680),MinimumSize=new Size(720,560),StartPosition=FormStartPosition.CenterParent,Font=AppTheme.Body,BackColor=AppTheme.Canvas,AutoScaleMode=AutoScaleMode.Dpi};
        var root=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=1,RowCount=2};
        root.RowStyles.Add(new RowStyle(SizeType.Absolute,82));root.RowStyles.Add(new RowStyle(SizeType.Percent,100));dialog.Controls.Add(root);
        var heading=new Panel {Dock=DockStyle.Fill,BackColor=Color.White,Padding=new Padding(24,12,20,8)};
        heading.Controls.Add(new Label {Text=dialog.Text,Dock=DockStyle.Top,Height=36,Font=AppTheme.Title,ForeColor=AppTheme.Ink});
        heading.Controls.Add(new Label {Text=$"Đang đăng nhập: {user.DisplayName}  •  {user.Role}",Dock=DockStyle.Bottom,Height=22,ForeColor=AppTheme.Muted});root.Controls.Add(heading,0,0);
        var tabs=new TabControl {Dock=DockStyle.Fill,Margin=new Padding(18,14,18,16),Font=AppTheme.Bold};root.Controls.Add(tabs,0,1);
        var myProfile=await auth.ProfileAsync(user,user.Id);
        var profileTab=new TabPage("Hồ sơ của tôi") {BackColor=AppTheme.Canvas,Padding=new Padding(22)};
        tabs.TabPages.Add(profileTab);
        var profileCard=new RoundedSurface {Dock=DockStyle.Top,Height=208,ColumnCount=2,RowCount=2,
            BackColor=Color.White,Padding=new Padding(20)};
        profileCard.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,108));
        profileCard.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
        profileCard.RowStyles.Add(new RowStyle(SizeType.Absolute,113));
        profileCard.RowStyles.Add(new RowStyle(SizeType.Percent,100));
        profileTab.Controls.Add(profileCard);
        var profileAvatar=new AvatarBadge {Size=new Size(88,88),Margin=new Padding(0,3,0,0)};
        profileCard.Controls.Add(profileAvatar,0,0);
        var profileInfo=new Label {Dock=DockStyle.Fill,Font=AppTheme.Body,
            ForeColor=AppTheme.Ink,Padding=new Padding(2,9,0,0)};
        profileCard.Controls.Add(profileInfo,1,0);
        void UpdateProfileInfo()
        {
            profileInfo.Text=$"{myProfile.DisplayName}\n@{myProfile.Username}   •   {myProfile.Role}";
            profileAvatar.SetProfile(myProfile.DisplayName,myProfile.AvatarPng);
        }
        UpdateProfileInfo();
        var editMyProfile=new RoundedActionButton {Text="CHỈNH SỬA TÊN VÀ ẢNH ĐẠI DIỆN",Dock=DockStyle.Fill,
            Margin=new Padding(0,2,0,0)};
        profileCard.Controls.Add(editMyProfile,0,1);profileCard.SetColumnSpan(editMyProfile,2);
        editMyProfile.Click+=async (_,_)=>
        {
            if(await EditStaffProfile(myProfile,dialog))
            {
                myProfile=await auth.ProfileAsync(user,user.Id);
                UpdateProfileInfo();
                UpdateHeaderAccountProfile();UpdateNavigationTitle();
            }
        };
        if(FunctionPolicy.Can(user,"system.password"))
        {
        var personal=new TabPage("Mật khẩu của tôi") {BackColor=Color.White,Padding=new Padding(22,20,22,18)};tabs.TabPages.Add(personal);
        var personalFields=new TableLayoutPanel {Dock=DockStyle.Top,Height=300,ColumnCount=1,RowCount=8};
        personalFields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
        foreach(var height in new[]{32,28,40,28,40,28,40,64})personalFields.RowStyles.Add(new RowStyle(SizeType.Absolute,height));personal.Controls.Add(personalFields);
        personalFields.Controls.Add(new Label {Text="ĐỔI MẬT KHẨU ĐĂNG NHẬP",Dock=DockStyle.Fill,Font=AppTheme.Bold,ForeColor=AppTheme.Blue},0,0);
        void PersonalField(string title,TextBox box,int row)
        {
            personalFields.Controls.Add(new Label {Text=title,Dock=DockStyle.Fill,ForeColor=AppTheme.Muted},0,row);
            box.Dock=DockStyle.Fill;personalFields.Controls.Add(box,0,row+1);
        }
        var oldPassword=Ui.Text(128,true);var password=Ui.Text(5,true);var again=Ui.Text(5,true);
        PersonalField("Mật khẩu hiện tại",oldPassword,1);PersonalField("Mật khẩu mới (1–5 ký tự)",password,3);PersonalField("Nhập lại mật khẩu mới",again,5);
        var changeMine=new Button {Text="ĐỔI MẬT KHẨU CỦA TÔI",Dock=DockStyle.Fill,Margin=new Padding(0,12,0,0)};AppTheme.Button(changeMine,true);personalFields.Controls.Add(changeMine,0,7);
        changeMine.Click+=async (_,_)=>await ChangeMine();
        async Task ChangeMine()
        {
            changeMine.Enabled=false;
            try
            {
                if(password.Text!=again.Text)throw new BusinessException("Hai mật khẩu mới không khớp.");
                await auth.ChangePasswordAsync(user,oldPassword.Text,password.Text);
                oldPassword.Clear();password.Clear();again.Clear();MessageBox.Show(dialog,"Đã đổi mật khẩu.");
            }
            catch(Exception ex){Ui.Error(dialog,ex);}
            finally{changeMine.Enabled=true;}
        }
        }
        if(FunctionPolicy.Can(user,"staff.manage"))
        {
            var permissions=new TabPage("Nhân viên và phân quyền") {BackColor=Color.White,Padding=new Padding(18,16,18,14)};tabs.TabPages.Add(permissions);
            var layout=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=1,RowCount=5};
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute,32));layout.RowStyles.Add(new RowStyle(SizeType.Percent,100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute,32));layout.RowStyles.Add(new RowStyle(SizeType.Absolute,72));layout.RowStyles.Add(new RowStyle(SizeType.Absolute,54));permissions.Controls.Add(layout);
            layout.Controls.Add(new Label {Text="CHỌN TÀI KHOẢN NHÂN VIÊN",Dock=DockStyle.Fill,Font=AppTheme.Bold,ForeColor=AppTheme.Blue},0,0);
            var grid=Ui.Grid();grid.MultiSelect=false;layout.Controls.Add(grid,0,1);
            var selectedLabel=new Label {Dock=DockStyle.Fill,ForeColor=AppTheme.Muted,TextAlign=ContentAlignment.MiddleLeft};layout.Controls.Add(selectedLabel,0,2);
            var editor=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=2,RowCount=2};
            editor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,50));editor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,50));
            editor.RowStyles.Add(new RowStyle(SizeType.Absolute,27));editor.RowStyles.Add(new RowStyle(SizeType.Absolute,40));layout.Controls.Add(editor,0,3);
            editor.Controls.Add(new Label {Text="Vai trò",Dock=DockStyle.Fill,ForeColor=AppTheme.Muted},0,0);
            editor.Controls.Add(new Label {Text="Trạng thái",Dock=DockStyle.Fill,ForeColor=AppTheme.Muted},1,0);
            var editRole=Ui.Combo(RolePolicy.Roles);editRole.Dock=DockStyle.Fill;editor.Controls.Add(editRole,0,1);
            var active=new CheckBox {Text="Cho phép đăng nhập",Checked=true,Dock=DockStyle.Fill,Padding=new Padding(18,0,0,0)};editor.Controls.Add(active,1,1);
            var actions=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=4,RowCount=1};
            for(var i=0;i<4;i++)actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,25));layout.Controls.Add(actions,0,4);
            var save=new Button {Text="LƯU VAI TRÒ",Dock=DockStyle.Fill,Margin=new Padding(0,6,4,0)};AppTheme.Button(save,true);actions.Controls.Add(save,0,0);
            var functionPermissions=new Button {Text="PHÂN QUYỀN",Dock=DockStyle.Fill,Margin=new Padding(4,6,4,0)};AppTheme.Button(functionPermissions);actions.Controls.Add(functionPermissions,1,0);
            var resetPassword=new Button {Text="ĐỔI MẬT KHẨU",Dock=DockStyle.Fill,Margin=new Padding(4,6,4,0)};AppTheme.Button(resetPassword);actions.Controls.Add(resetPassword,2,0);
            var editProfile=new Button {Text="TÊN / ẢNH",Dock=DockStyle.Fill,Margin=new Padding(4,6,0,0)};AppTheme.Button(editProfile);actions.Controls.Add(editProfile,3,0);
            UserInfo Selected()=>grid.CurrentRow?.DataBoundItem as UserInfo??throw new BusinessException("Chọn tài khoản.");
            void ShowSelection()
            {
                if(grid.CurrentRow?.DataBoundItem is not UserInfo item)return;
                selectedLabel.Text=$"Đang chọn: {item.DisplayName} ({item.Username})"+(item.Id==user.Id?"  •  Không thể tự đổi quyền":"");
                editRole.SelectedItem=item.Role;active.Checked=item.Active;
                save.Enabled=item.Id!=user.Id;functionPermissions.Enabled=item.Id!=user.Id;resetPassword.Enabled=item.Id!=user.Id;
                editProfile.Enabled=true;
            }
            async Task LoadRows(int? selectId=null)
            {
                grid.DataSource=await auth.UsersAsync(user);
                HideColumns(grid,"Id","Version","AvatarPng");
                if(grid.Columns["Username"] is { } name)name.HeaderText="Tên đăng nhập";
                if(grid.Columns["DisplayName"] is { } display)display.HeaderText="Tên hiển thị";
                if(grid.Columns["Role"] is { } role)role.HeaderText="Vai trò";
                if(grid.Columns["Active"] is { } status)status.HeaderText="Đang hoạt động";
                if(grid.Columns["LockedUntil"] is { } locked){locked.HeaderText="Khóa đến";locked.DefaultCellStyle.Format="dd/MM/yyyy HH:mm";}
                if(selectId is int id)foreach(DataGridViewRow row in grid.Rows)if(row.DataBoundItem is UserInfo item && item.Id==id){grid.CurrentCell=row.Cells.Cast<DataGridViewCell>().First(c=>c.Visible);break;}
                ShowSelection();
            }
            grid.SelectionChanged+=(_,_)=>ShowSelection();
            editProfile.Click+=async (_,_)=>
            {
                try
                {
                    var selected=Selected();
                    if(await EditStaffProfile(selected,dialog))
                    {
                        await LoadRows(selected.Id);
                        if(selected.Id==user.Id)
                        {
                            myProfile=await auth.ProfileAsync(user,user.Id);
                            UpdateProfileInfo();UpdateHeaderAccountProfile();UpdateNavigationTitle();
                        }
                    }
                }
                catch(Exception ex){Ui.Error(dialog,ex);}
            };
            save.Click+=async (_,_)=>
            {
                save.Enabled=false;
                try{var selected=Selected();await auth.UpdateUserAsync(user,selected,(string)editRole.SelectedItem!,active.Checked);await LoadRows(selected.Id);}
                catch(Exception ex){Ui.Error(dialog,ex);}
                finally{ShowSelection();}
            };
            functionPermissions.Click+=async (_,_)=>
            {
                try
                {
                    var selected=Selected();
                    var granted=await auth.UserFunctionsAsync(user,selected);
                    var available=FunctionPolicy.All.Where(x=>FunctionPolicy.RoleAllows(selected.Role,x.Code)).ToArray();
                    using var grantDialog=new Form {Text=$"Chức năng của {selected.Username}",Size=new Size(720,720),MinimumSize=new Size(560,520),StartPosition=FormStartPosition.CenterParent,Font=AppTheme.Body,BackColor=AppTheme.Canvas};
                    var grantRoot=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=1,RowCount=3,Padding=new Padding(18)};
                    grantRoot.RowStyles.Add(new RowStyle(SizeType.Absolute,65));grantRoot.RowStyles.Add(new RowStyle(SizeType.Percent,100));grantRoot.RowStyles.Add(new RowStyle(SizeType.Absolute,60));grantDialog.Controls.Add(grantRoot);
                    grantRoot.Controls.Add(new Label {Text=$"Chọn chức năng được phép dùng cho {selected.Username}\nThay đổi quyền sẽ yêu cầu nhân viên đăng nhập lại.",Dock=DockStyle.Fill,ForeColor=AppTheme.Ink},0,0);
                    var list=new CheckedListBox {Dock=DockStyle.Fill,CheckOnClick=true,IntegralHeight=false,Font=AppTheme.Body};
                    for(var i=0;i<available.Length;i++)
                    {
                        var item=available[i];
                        var menuName=MainMenus.Single(x=>x.Code==item.Menu).Title;
                        list.Items.Add($"{menuName}  ›  {item.Submenu}  ›  {item.Name}",granted.Contains(item.Code));
                    }
                    grantRoot.Controls.Add(list,0,1);
                    var saveFunctions=new Button {Text="LƯU QUYỀN CHỨC NĂNG",Dock=DockStyle.Fill};AppTheme.Button(saveFunctions,true);grantRoot.Controls.Add(saveFunctions,0,2);
                    saveFunctions.Click+=async (_,_) =>
                    {
                        saveFunctions.Enabled=false;
                        try
                        {
                            var codes=Enumerable.Range(0,available.Length).Where(list.GetItemChecked).Select(i=>available[i].Code).ToArray();
                            await auth.SaveUserFunctionsAsync(user,selected,codes);
                            grantDialog.DialogResult=DialogResult.OK;
                            grantDialog.Close();
                        }
                        catch(Exception ex){Ui.Error(grantDialog,ex);saveFunctions.Enabled=true;}
                    };
                    if(grantDialog.ShowDialog(dialog)==DialogResult.OK)await LoadRows(selected.Id);
                }
                catch(Exception ex){Ui.Error(dialog,ex);}
            };
            resetPassword.Click+=async (_,_)=>
            {
                try
                {
                    var selected=Selected();
                    using var reset=new InputDialog($"Đổi mật khẩu: {selected.Username}",520,340);
                    var staffPassword=Ui.Text(5,true);var staffAgain=Ui.Text(5,true);
                    reset.Add("Mật khẩu mới (1–5 ký tự)",staffPassword);reset.Add("Nhập lại mật khẩu mới",staffAgain);
                    reset.Action("XÁC NHẬN ĐỔI MẬT KHẨU",async()=>
                    {
                        if(staffPassword.Text!=staffAgain.Text)throw new BusinessException("Hai mật khẩu nhân viên không khớp.");
                        await auth.ResetPasswordAsync(user,selected,staffPassword.Text);
                    });
                    if(reset.ShowDialog(dialog)==DialogResult.OK){await LoadRows(selected.Id);MessageBox.Show(dialog,$"Đã đổi mật khẩu của {selected.Username} và thu hồi phiên đăng nhập cũ.");}
                }
                catch(Exception ex){Ui.Error(dialog,ex);}
            };
            await LoadRows();
        }
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
        using var dialog=new Form {Text="Đặt lại mật khẩu nhân viên",Size=new Size(850,560),MinimumSize=new Size(740,500),StartPosition=FormStartPosition.CenterParent,Font=AppTheme.Body,BackColor=AppTheme.Canvas,AutoScaleMode=AutoScaleMode.Dpi};
        var root=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=1,RowCount=3};
        root.RowStyles.Add(new RowStyle(SizeType.Absolute,82));root.RowStyles.Add(new RowStyle(SizeType.Percent,100));root.RowStyles.Add(new RowStyle(SizeType.Absolute,76));dialog.Controls.Add(root);
        var heading=new Panel {Dock=DockStyle.Fill,BackColor=Color.White,Padding=new Padding(24,12,20,8)};
        heading.Controls.Add(new Label {Text="Đặt lại mật khẩu nhân viên",Dock=DockStyle.Top,Height=36,Font=AppTheme.Title,ForeColor=AppTheme.Ink});
        heading.Controls.Add(new Label {Text="Chọn nhân viên rồi nhập và xác nhận mật khẩu mới",Dock=DockStyle.Bottom,Height=22,ForeColor=AppTheme.Muted});root.Controls.Add(heading,0,0);
        var body=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=2,RowCount=1,Padding=new Padding(18,14,18,8)};
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,56));body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,44));root.Controls.Add(body,0,1);
        var users=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=1,RowCount=2,BackColor=Color.White,Padding=new Padding(14),Margin=new Padding(0,0,8,0)};
        users.RowStyles.Add(new RowStyle(SizeType.Absolute,32));users.RowStyles.Add(new RowStyle(SizeType.Percent,100));body.Controls.Add(users,0,0);
        users.Controls.Add(new Label {Text="CHỌN TÀI KHOẢN NHÂN VIÊN",Dock=DockStyle.Fill,Font=AppTheme.Bold,ForeColor=AppTheme.Blue},0,0);
        var grid=Ui.Grid();grid.MultiSelect=false;users.Controls.Add(grid,0,1);
        var editor=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=1,RowCount=7,BackColor=Color.White,Padding=new Padding(16,14,16,12),Margin=new Padding(8,0,0,0)};
        editor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
        foreach(var height in new[]{32,72,34,42,34,42,0})editor.RowStyles.Add(new RowStyle(height==0?SizeType.Percent:SizeType.Absolute,height==0?100:height));body.Controls.Add(editor,1,0);
        editor.Controls.Add(new Label {Text="MẬT KHẨU MỚI",Dock=DockStyle.Fill,Font=AppTheme.Bold,ForeColor=AppTheme.Blue},0,0);
        var selectedLabel=new Label {Dock=DockStyle.Fill,BackColor=AppTheme.Canvas,Padding=new Padding(10,10,6,4),ForeColor=AppTheme.Ink};editor.Controls.Add(selectedLabel,0,1);
        editor.Controls.Add(new Label {Text="Mật khẩu mới (1–5 ký tự)",Dock=DockStyle.Fill,ForeColor=AppTheme.Muted,TextAlign=ContentAlignment.BottomLeft},0,2);
        var password=Ui.Text(5,true);password.Dock=DockStyle.Fill;editor.Controls.Add(password,0,3);
        editor.Controls.Add(new Label {Text="Nhập lại mật khẩu mới",Dock=DockStyle.Fill,ForeColor=AppTheme.Muted,TextAlign=ContentAlignment.BottomLeft},0,4);
        var again=Ui.Text(5,true);again.Dock=DockStyle.Fill;editor.Controls.Add(again,0,5);
        editor.Controls.Add(new Label {Text="Phiên đăng nhập cũ của nhân viên sẽ bị thu hồi sau khi đổi mật khẩu.",Dock=DockStyle.Fill,ForeColor=AppTheme.Muted,Padding=new Padding(0,16,0,0)},0,6);
        var footer=new Panel {Dock=DockStyle.Fill,Padding=new Padding(22,12,22,16)};root.Controls.Add(footer,0,2);
        var save=new Button {Text="ĐẶT LẠI MẬT KHẨU",Dock=DockStyle.Fill};AppTheme.Button(save,true);footer.Controls.Add(save);
        void ShowSelection()
        {
            if(grid.CurrentRow?.DataBoundItem is UserInfo selected)selectedLabel.Text=$"Tài khoản: {selected.Username}\nVai trò: {RolePolicy.Name(selected.Role)}";
            else selectedLabel.Text="Chưa chọn tài khoản.";
            save.Enabled=grid.CurrentRow?.DataBoundItem is UserInfo;
        }
        async Task LoadRows(int? selectedId=null)
        {
            grid.DataSource=(await auth.UsersAsync(user)).Where(x=>x.Id!=user.Id).ToList();
            HideColumns(grid,"Id","Version");
            if(grid.Columns["Username"] is { } name)name.HeaderText="Tên đăng nhập";
            if(grid.Columns["Role"] is { } role)role.HeaderText="Vai trò";
            if(grid.Columns["Active"] is { } active)active.HeaderText="Hoạt động";
            if(grid.Columns["LockedUntil"] is { } locked)locked.Visible=false;
            if(selectedId is int id)foreach(DataGridViewRow row in grid.Rows)if(row.DataBoundItem is UserInfo item && item.Id==id){grid.CurrentCell=row.Cells.Cast<DataGridViewCell>().First(c=>c.Visible);break;}
            ShowSelection();
        }
        grid.SelectionChanged+=(_,_)=>ShowSelection();
        save.Click+=async (_,_)=>
        {
            save.Enabled=false;
            try
            {
                if(grid.CurrentRow?.DataBoundItem is not UserInfo selected)throw new BusinessException("Chọn tài khoản cần đặt lại mật khẩu.");
                if(password.Text!=again.Text)throw new BusinessException("Hai mật khẩu không khớp.");
                await auth.ResetPasswordAsync(user,selected,password.Text);
                password.Clear();again.Clear();await LoadRows(selected.Id);
                MessageBox.Show(dialog,$"Đã đặt lại mật khẩu của {selected.Username} và thu hồi phiên đăng nhập cũ.");
            }
            catch(Exception ex){Ui.Error(dialog,ex);}
            finally{ShowSelection();}
        };
        await LoadRows();dialog.ShowDialog(this);
    }
}


