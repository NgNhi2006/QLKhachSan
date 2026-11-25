using QLKhachSan.BLL;
using QLKhachSan.DAL;
using QLKhachSan.DTO;

namespace QLKhachSan.GUI;

public partial class ucDashboard
{
    private static int BilledRoomDays(BillQuote quote)
    {
        var start=quote.Stay.CheckIn??quote.Stay.Arrival;
        return Math.Max(1,(int)Math.Ceiling(Math.Max(0,(quote.At-start).TotalDays)));
    }

    private void ShowCheckoutWorkspace(Stay stay,BillQuote bill)
    {
        ClearPage(functionPage);
        activeFunction="room.checkout";
        var heading=PageHeading("Thanh toán",$"Phòng {bill.Room.Number}  /  {stay.Guest}  /  Chi tiết hóa đơn");
        AddBackButton(heading,"← Sơ đồ phòng",()=>ShowEmbeddedFunction("room.map"));

        var scroll=new Panel {Dock=DockStyle.Fill,AutoScroll=true,BackColor=AppTheme.Canvas};
        var content=new Panel {BackColor=AppTheme.Canvas};scroll.Controls.Add(content);
        var left=new DashboardMetricCard {Accent=AppTheme.Blue,BackColor=Color.White};
        var right=new DashboardMetricCard {Accent=AppTheme.Amber,BackColor=Color.White};
        content.Controls.Add(left);content.Controls.Add(right);

        var roomPicture=new PictureBox {Location=new Point(20,20),Size=new Size(112,86),
            SizeMode=PictureBoxSizeMode.Zoom,BackColor=Color.FromArgb(239,236,226)};
        using(var stream=typeof(ucDashboard).Assembly.GetManifestResourceStream("QLKhachSan.Assets.hotel-room.jpg"))
            if(stream is not null){using var decoded=Image.FromStream(stream);roomPicture.Image=new Bitmap(decoded);}
        roomPicture.Disposed+=(_,_)=>roomPicture.Image?.Dispose();left.Controls.Add(roomPicture);
        var roomTitle=new Label {Text=$"Phòng {bill.Room.Number}",Font=AppTheme.TextFont(18F,FontStyle.Bold),
            ForeColor=AppTheme.Navy,Location=new Point(148,20),Height=34};left.Controls.Add(roomTitle);
        var roomMeta=new Label {Text=$"{bill.Room.Type}  •  {stay.Guest}\nNhận phòng: {stay.CheckIn:dd/MM/yyyy HH:mm}  •  Tính đến: {bill.At:dd/MM/yyyy HH:mm}",
            Font=AppTheme.Body,ForeColor=AppTheme.Muted,Location=new Point(149,57),Height=52};left.Controls.Add(roomMeta);
        var sectionTitle=new Label {Text="CHI TIẾT HÓA ĐƠN",Font=AppTheme.Bold,ForeColor=AppTheme.Navy,
            Location=new Point(20,124),Height=28};left.Controls.Add(sectionTitle);
        var tableHead=new Panel {BackColor=Color.FromArgb(244,243,235),Location=new Point(20,158),Height=36};left.Controls.Add(tableHead);
        var headName=new Label {Text="HẠNG MỤC",Font=AppTheme.Small,ForeColor=AppTheme.Muted,TextAlign=ContentAlignment.MiddleLeft,
            Location=new Point(12,0),Height=36};tableHead.Controls.Add(headName);
        var headQty=new Label {Text="SL",Font=AppTheme.Small,ForeColor=AppTheme.Muted,TextAlign=ContentAlignment.MiddleRight,
            Height=36};tableHead.Controls.Add(headQty);
        var headTotal=new Label {Text="THÀNH TIỀN",Font=AppTheme.Small,ForeColor=AppTheme.Muted,
            TextAlign=ContentAlignment.MiddleRight,Height=36};tableHead.Controls.Add(headTotal);
        var linesPanel=new Panel {Location=new Point(20,194),Height=165,AutoScroll=true,BackColor=Color.White};left.Controls.Add(linesPanel);
        var lineRows=new List<Panel>();
        void AddLine(string name,string quantity,decimal amount)
        {
            var row=new Panel {Height=38,BackColor=lineRows.Count%2==0?Color.White:Color.FromArgb(251,250,246)};
            row.Controls.Add(new Label {Name="name",Text=name,Font=AppTheme.Body,ForeColor=AppTheme.Ink,
                TextAlign=ContentAlignment.MiddleLeft,Location=new Point(12,0),Height=38,AutoEllipsis=true});
            row.Controls.Add(new Label {Name="qty",Text=quantity,Font=AppTheme.Body,ForeColor=AppTheme.Muted,
                TextAlign=ContentAlignment.MiddleRight,Height=38});
            row.Controls.Add(new Label {Name="amount",Text=$"{amount:N0} đ",Font=AppTheme.Body,
                ForeColor=AppTheme.Ink,TextAlign=ContentAlignment.MiddleRight,Height=38});
            linesPanel.Controls.Add(row);lineRows.Add(row);
        }
        AddLine("Tiền phòng",$"{BilledRoomDays(bill)} ngày",bill.RoomCharge);
        foreach(var line in bill.Lines.Where(x=>x.Cancelled is null))AddLine(line.Name,line.Quantity.ToString(),line.Total);
        var totals=new Panel {BackColor=Color.FromArgb(249,247,242),Location=new Point(20,367),Height=88};left.Controls.Add(totals);
        var totalLabel=new Label {Text=$"Tổng hóa đơn     {bill.Total:N0} đ\nCọc đã thu          {stay.Deposit:N0} đ\n{(bill.ToCollect>0?"Cần thanh toán":"Cần hoàn khách")}    {(bill.ToCollect>0?bill.ToCollect:bill.ToRefund):N0} đ",
            Font=AppTheme.Bold,ForeColor=AppTheme.Navy,TextAlign=ContentAlignment.MiddleRight,
            Dock=DockStyle.Fill,Padding=new Padding(14,10,14,0)};totals.Controls.Add(totalLabel);
        var editBill=new Button {Text="XEM / SỬA BILL TẠM TÍNH",Location=new Point(20,470),Size=new Size(260,38)};
        AppTheme.Button(editBill);left.Controls.Add(editBill);

        var amountCaption=new Label {Text=bill.ToCollect>0?"SỐ TIỀN CẦN THANH TOÁN":"SỐ TIỀN CẦN HOÀN",
            Font=AppTheme.Small,ForeColor=AppTheme.Muted,Location=new Point(18,22),Height=22};right.Controls.Add(amountCaption);
        var amount=new Label {Text=$"{(bill.ToCollect>0?bill.ToCollect:bill.ToRefund):N0} đ",
            Font=AppTheme.TextFont(20F,FontStyle.Bold),ForeColor=AppTheme.Blue,
            Location=new Point(17,45),Height=43};right.Controls.Add(amount);
        var methodTitle=new Label {Text="Hình thức thanh toán",Font=AppTheme.Bold,
            ForeColor=AppTheme.Ink,Location=new Point(18,92),Height=25};right.Controls.Add(methodTitle);
        var choices=new[]
        {
            new PaymentMethodChoice {Text="Tiền mặt",IconKind="money",Checked=true},
            new PaymentMethodChoice {Text="Thẻ POS",IconKind="receipt"},
            new PaymentMethodChoice {Text="Chuyển khoản",IconKind="finance"},
            new PaymentMethodChoice {Text="Công nợ OTA",IconKind="calendar"}
        };
        choices[3].Enabled=false;
        for(var i=0;i<choices.Length;i++)
        {
            choices[i].Location=new Point(18,116+i*38);
            choices[i].Height=35;
            right.Controls.Add(choices[i]);
        }
        var referenceTitle=new Label {Text="Mã giao dịch / tham chiếu",Font=AppTheme.Small,
            ForeColor=AppTheme.Muted,Location=new Point(18,274),Height=22};right.Controls.Add(referenceTitle);
        var reference=Ui.Text(100);reference.Location=new Point(18,298);reference.Height=32;reference.Dock=DockStyle.None;
        reference.PlaceholderText="Mã giao dịch hoặc ghi chú (nếu có)";
        right.Controls.Add(reference);
        var bankInfo=new Label {Text="Kiểm tra số tiền trước khi xác nhận giao dịch.",Font=AppTheme.Small,
            ForeColor=AppTheme.Muted,Location=new Point(18,340),Height=52};right.Controls.Add(bankInfo);
        var qr=new PictureBox {Location=new Point(18,340),Size=new Size(190,190),
            SizeMode=PictureBoxSizeMode.Zoom,BackColor=Color.White,Visible=false};right.Controls.Add(qr);
        var qrStatus=new Label {Text="Đang tải mã QR...",Font=AppTheme.Small,ForeColor=AppTheme.Muted,
            TextAlign=ContentAlignment.MiddleCenter,BackColor=Color.White,Visible=false};right.Controls.Add(qrStatus);
        qrStatus.BringToFront();
        var confirm=new CheckBox {Text="Đã thu đủ tiền / hoàn đủ tiền cho khách",Font=AppTheme.Small,
            ForeColor=AppTheme.Ink,Location=new Point(18,410),Height=30};right.Controls.Add(confirm);
        var complete=new RoundedActionButton {Text="XÁC NHẬN THANH TOÁN",Location=new Point(18,451),Height=44};
        right.Controls.Add(complete);
        var settings=AppSettings.Load();
        var transferMode=false;

        var groupStays=data.Stays.Where(x=>x.CustomerId==stay.CustomerId && x.Status==StayStatus.Occupied).ToList();
        var groupMode=false;
        var groupQuotes=new List<BillQuote>();
        var groupHeader=new Label {Text=$"THANH TOÁN NHIỀU PHÒNG\n{stay.Guest}  •  Chọn ít nhất 2 phòng bên dưới",
            Font=AppTheme.Bold,ForeColor=AppTheme.Navy,Location=new Point(20,27),Height=75,
            Visible=false};left.Controls.Add(groupHeader);
        var groupList=new ListView {View=View.Details,CheckBoxes=true,FullRowSelect=true,
            GridLines=false,ShowItemToolTips=true,BackColor=Color.White,Visible=false,
            Location=new Point(20,110),Height=240};
        groupList.Columns.Add("Phòng",75);
        groupList.Columns.Add("Dịch vụ đã gọi",210);
        groupList.Columns.Add("Tổng",115,HorizontalAlignment.Right);
        groupList.Columns.Add("Cọc",110,HorizontalAlignment.Right);
        groupList.Columns.Add("Cần thu",115,HorizontalAlignment.Right);
        left.Controls.Add(groupList);
        var serviceHeading=new Label {Text="DỊCH VỤ CỦA PHÒNG ĐANG CHỌN",Font=AppTheme.Bold,
            ForeColor=AppTheme.Navy,Location=new Point(20,359),Height=25,Visible=false};left.Controls.Add(serviceHeading);
        var serviceDetails=new ListView {View=View.Details,FullRowSelect=true,GridLines=false,
            BackColor=Color.White,Location=new Point(20,386),Height=122,Visible=false};
        serviceDetails.Columns.Add("Dịch vụ",260);
        serviceDetails.Columns.Add("SL",52,HorizontalAlignment.Right);
        serviceDetails.Columns.Add("Thành tiền",130,HorizontalAlignment.Right);
        left.Controls.Add(serviceDetails);
        var groupSummary=new Label {Font=AppTheme.Bold,ForeColor=AppTheme.Navy,
            BackColor=Color.FromArgb(249,247,242),Location=new Point(20,520),Height=68,
            Padding=new Padding(10,8,5,0),Visible=false};left.Controls.Add(groupSummary);
        Button? groupAction=null;
        if(groupAction is not null){AppTheme.Button(groupAction);left.Controls.Add(groupAction);}
        var viewGroupBill=new Button {Text="XEM / IN BILL TỔNG",Height=38,Visible=false};
        AppTheme.Button(viewGroupBill);left.Controls.Add(viewGroupBill);
        var editGroupBill=new Button {Text="SỬA BILL TẤT CẢ PHÒNG",Height=38,Visible=false};
        AppTheme.Button(editGroupBill);left.Controls.Add(editGroupBill);
        static List<BillDraftLine> ServiceLines(BillQuote quote)=>quote.DraftLines is { } draft
            ?draft.Where(x=>x.Category!="Room" && x.Quantity>0).ToList()
            :quote.Lines.Where(x=>x.Cancelled is null)
                .Select(x=>new BillDraftLine(x.Name,x.Quantity,x.Price,"Service",x.Id,
                    x.Total==x.Quantity*x.Price?null:x.Total)).ToList();
        static string ServiceSummary(BillQuote quote)
        {
            var lines=ServiceLines(quote);
            return lines.Count==0?"Chưa gọi dịch vụ":string.Join(", ",lines.Select(x=>$"{x.Name} ×{x.Quantity}"));
        }
        void ShowRoomServices()
        {
            serviceDetails.Items.Clear();
            if(groupList.SelectedItems.Count==0)return;
            var quote=(BillQuote)groupList.SelectedItems[0].Tag!;
            serviceHeading.Text=$"DỊCH VỤ PHÒNG {quote.Room.Number}";
            var lines=ServiceLines(quote);
            if(lines.Count==0)
            {serviceDetails.Items.Add(new ListViewItem("Chưa gọi dịch vụ"));return;}
            foreach(var line in lines)
            {
                var item=new ListViewItem(line.Name);
                item.SubItems.Add(line.Quantity.ToString());item.SubItems.Add($"{line.Total:N0} đ");
                serviceDetails.Items.Add(item);
            }
        }
        groupList.SelectedIndexChanged+=(_,_)=>ShowRoomServices();
        void UpdateGroupItem(ListViewItem item,BillQuote quote)
        {
            item.Tag=quote;
            item.SubItems[1].Text=ServiceSummary(quote);
            item.SubItems[2].Text=$"{quote.Total:N0}";
            item.SubItems[3].Text=$"{quote.Stay.Deposit:N0}";
            item.SubItems[4].Text=$"{quote.ToCollect-quote.ToRefund:N0}";
            item.ToolTipText=$"Phòng {quote.Room.Number}: {ServiceSummary(quote)}";
        }
        List<BillQuote> Chosen()=>groupList.CheckedItems.Cast<ListViewItem>().Select(x=>(BillQuote)x.Tag!).ToList();
        decimal Due()=>groupMode?Chosen().Sum(x=>x.ToCollect-x.ToRefund):bill.ToCollect-bill.ToRefund;
        void RefreshPayment()
        {
            var due=Due();
            amountCaption.Text=due>=0?"SỐ TIỀN CẦN THANH TOÁN":"SỐ TIỀN CẦN HOÀN";
            amount.Text=$"{Math.Abs(due):N0} đ";
            complete.Text="XÁC NHẬN THANH TOÁN";
            var chosen=Chosen();
            groupSummary.Text=$"{chosen.Count} phòng  •  Tổng {chosen.Sum(x=>x.Total):N0} đ  •  Cọc {chosen.Sum(x=>x.Stay.Deposit):N0} đ\n"+
                $"Cần thu {chosen.Sum(x=>x.ToCollect):N0} đ  •  Cần hoàn {chosen.Sum(x=>x.ToRefund):N0} đ";
            choices[3].Enabled=false;
            if(groupMode && choices[3].Checked)choices[0].Checked=true;
            confirm.Checked=false;
            UpdateMethod();
        }
        groupList.ItemChecked+=(_,_)=>{if(groupMode)RefreshPayment();};
        if(groupAction is not null)groupAction.Click+=async (_,_)=>await Run(async()=>
        {
            if(!groupMode)
            {
                groupQuotes=await service.GroupQuoteAsync(groupStays);
                groupList.Items.Clear();
                foreach(var q in groupQuotes)
                {
                    var item=new ListViewItem(q.Room.Number){Tag=q,Checked=true};
                    item.SubItems.Add(ServiceSummary(q));item.SubItems.Add($"{q.Total:N0}");
                    item.SubItems.Add($"{q.Stay.Deposit:N0}");
                    item.SubItems.Add($"{q.ToCollect-q.ToRefund:N0}");
                    groupList.Items.Add(item);
                }
                groupList.Items.Cast<ListViewItem>().First(x=>((BillQuote)x.Tag!).Stay.Id==stay.Id).Selected=true;
                ShowRoomServices();
            }
            groupMode=!groupMode;
            groupAction.Text=groupMode?"CHỈ THANH TOÁN PHÒNG NÀY":"THANH TOÁN NHIỀU PHÒNG";
            foreach(var control in new Control[]{roomPicture,roomTitle,roomMeta,sectionTitle,tableHead,linesPanel,totals})
                control.Visible=!groupMode;
            groupHeader.Visible=groupMode;groupList.Visible=groupMode;
            serviceHeading.Visible=groupMode;serviceDetails.Visible=groupMode;
            groupSummary.Visible=groupMode;viewGroupBill.Visible=groupMode;editGroupBill.Visible=groupMode;
            editBill.Text=groupMode?"XEM / SỬA BILL PHÒNG":"XEM / SỬA BILL TẠM TÍNH";
            RefreshPayment();LayoutWorkspace();
        });
        viewGroupBill.Click+=async (_,_)=>
        {
            try{await ShowBillBeforePayment(Chosen(),SelectedMethod(),reference.Text,this);}
            catch(Exception ex){Ui.Error(this,ex);}
        };
        editGroupBill.Click+=(_,_)=>
        {
            var chosen=Chosen();
            if(chosen.Count==0){Ui.Error(this,new BusinessException("Chọn phòng cần sửa bill."));return;}
            var updated=EditGroupDraftBills(chosen,this);
            if(updated is null)return;
            foreach(var item in groupList.Items.Cast<ListViewItem>())
                if(updated.TryGetValue(((BillQuote)item.Tag!).Stay.Id,out var quote))UpdateGroupItem(item,quote);
            ShowRoomServices();RefreshPayment();
        };
        qr.LoadCompleted+=(_,e)=>
        {
            if(right.IsDisposed)return;
            var valid=e.Error is null && qr.Image is {Width:>=100,Height:>=100};
            qr.Visible=valid && transferMode;
            qrStatus.Visible=!valid && transferMode;
            if(!valid)
            {
                qrStatus.Text="Không tải được mã QR";
                qrStatus.BringToFront();
                bankInfo.Text=$"Dùng thông tin chuyển khoản bên dưới.\nSTK: {settings.BankAccount} • {settings.BankAccountName}";
            }
        };
        string SelectedMethod()=>choices.First(x=>x.Checked).Text;
        void UpdateMethod()
        {
            var transfer=SelectedMethod()=="Chuyển khoản" && Due()>0;
            var configured=!string.IsNullOrWhiteSpace(settings.BankCode) &&
                !string.IsNullOrWhiteSpace(settings.BankAccount) &&
                !string.IsNullOrWhiteSpace(settings.BankAccountName);
            reference.Enabled=true;
            if(transfer && string.IsNullOrWhiteSpace(reference.Text))
            {
                reference.Text=$"KS-{stay.Id}";
                return;
            }
            transferMode=transfer && configured;
            qr.Visible=false;
            qrStatus.Visible=transferMode;
            qrStatus.Text="Đang tải mã QR...";
            if(qrStatus.Visible)qrStatus.BringToFront();
            if(!transfer)
            {
                qr.CancelAsync();bankInfo.Text="Kiểm tra số tiền trước khi xác nhận giao dịch.";
                LayoutWorkspace();return;
            }
            if(!configured)
            {bankInfo.Text="Chưa cấu hình tài khoản ngân hàng.";LayoutWorkspace();return;}
            bankInfo.Text=$"Chuyển khoản đến {settings.BankAccountName}\nSTK: {settings.BankAccount}";
            LayoutWorkspace();
            var url=$"https://img.vietqr.io/image/{Uri.EscapeDataString(settings.BankCode)}-{Uri.EscapeDataString(settings.BankAccount)}-qr_only.png?amount={Due():0}&addInfo={Uri.EscapeDataString(reference.Text)}&accountName={Uri.EscapeDataString(settings.BankAccountName)}";
            qr.CancelAsync();
            try{qr.LoadAsync(url);}catch(InvalidOperationException){bankInfo.Text+="\nKhông tải được QR.";}
        }
        foreach(var choice in choices)choice.CheckedChanged+=(_,_)=>{if(choice.Checked)UpdateMethod();};
        reference.TextChanged+=(_,_)=>{if(SelectedMethod()=="Chuyển khoản")UpdateMethod();};
        UpdateMethod();
        scroll.Disposed+=(_,_)=>qr.CancelAsync();

        editBill.Click+=(_,_)=>
        {
            if(groupMode)
            {
                if(groupList.SelectedItems.Count==0)
                {Ui.Error(this,new BusinessException("Chọn phòng cần sửa bill."));return;}
                var item=groupList.SelectedItems[0];
                var updated=EditDraftBill((BillQuote)item.Tag!,SelectedMethod(),reference.Text);
                if(updated is null)return;
                UpdateGroupItem(item,updated);
                ShowRoomServices();
                RefreshPayment();return;
            }
            var changed=EditDraftBill(bill,SelectedMethod(),reference.Text);
            if(changed is null)return;
            if(ReferenceEquals(changed,bill))return;
            bill=changed;
            Ui.Clear(linesPanel);lineRows.Clear();
            if(bill.DraftLines is { } draft)
                foreach(var line in draft.Where(x=>x.Quantity>0))
                    AddLine(line.Name,line.Category=="Room"?$"{BilledRoomDays(bill)} ngày":line.Quantity.ToString(),line.Total);
            else
            {
                AddLine("Tiền phòng",$"{BilledRoomDays(bill)} ngày",bill.RoomCharge);
                foreach(var line in bill.Lines.Where(x=>x.Cancelled is null))
                    AddLine(line.Name,line.Quantity.ToString(),line.Total);
            }
            if(bill.DiscountCode is { } code)AddLine($"Mã giảm giá {code}","",-bill.DiscountAmount);
            totalLabel.Text=$"Tổng hóa đơn     {bill.Total:N0} đ\nCọc đã thu          {stay.Deposit:N0} đ\n{(bill.ToCollect>0?"Cần thanh toán":"Cần hoàn khách")}    {(bill.ToCollect>0?bill.ToCollect:bill.ToRefund):N0} đ";
            RefreshPayment();LayoutWorkspace();
            linesPanel.AutoScrollPosition=Point.Empty;
        };

        complete.Click+=async (_,_)=>await Run(async()=>
        {
            if(!confirm.Checked)throw new BusinessException("Cần xác nhận đã hoàn tất thu/hoàn tiền.");
            var method=SelectedMethod();
            var paidReference=reference.Text;
            if(groupMode)
            {
                var chosen=Chosen();
                if(chosen.Count<2)throw new BusinessException("Chọn ít nhất hai phòng của cùng khách.");
                if(method=="Công nợ OTA")throw new BusinessException("Thanh toán nhiều phòng chưa hỗ trợ công nợ OTA.");
                if(method is "Chuyển khoản" or "Thẻ POS" && string.IsNullOrWhiteSpace(paidReference))
                    throw new BusinessException("Nhập mã giao dịch hoặc nội dung chuyển khoản.");
                GroupCheckoutResult? result=null;
                complete.Enabled=false;
                try
                {
                    await Changed(async()=>result=await service.GroupCheckoutAsync(chosen,method,paidReference));
                    if(result is not null)await PrintGroupCheckout(result,chosen,method,paidReference);
                    ShowEmbeddedFunction("room.map");
                }
                finally{if(!complete.IsDisposed)complete.Enabled=true;}
                return;
            }
            long invoiceId=0;
            await Changed(async()=>invoiceId=await service.CheckoutAsync(bill,method,paidReference));
            ShowCheckoutReceipt(invoiceId,bill,method,paidReference,this);
            ShowEmbeddedFunction("room.map");
        });

        void LayoutWorkspace()
        {
            var width=Math.Max(820,scroll.ClientSize.Width-SystemInformation.VerticalScrollBarWidth-24);
            content.Width=width;
            var available=width-48;
            var rightWidth=Math.Clamp((int)(available*.37),300,420);
            var leftWidth=available-rightWidth-18;
            var transfer=transferMode;
            var qrSize=Math.Clamp((int)(rightWidth*.56),190,230);
            var bankTop=transfer?340+qrSize+8:340;
            var confirmTop=transfer?bankTop+57:410;
            var completeTop=transfer?confirmTop+38:451;
            var rightHeight=transfer?completeTop+60:525;
            var leftHeight=groupMode?725:groupAction is null?540:590;
            left.Bounds=new Rectangle(24,18,leftWidth,leftHeight);
            right.Bounds=new Rectangle(24+leftWidth+18,18,rightWidth,rightHeight);
            roomTitle.Width=Math.Max(100,leftWidth-170);roomMeta.Width=roomTitle.Width;
            sectionTitle.Width=leftWidth-40;tableHead.Width=leftWidth-40;
            linesPanel.Width=leftWidth-40;totals.Width=leftWidth-40;
            groupHeader.Width=leftWidth-40;
            groupList.Width=leftWidth-40;groupSummary.Width=leftWidth-40;
            serviceHeading.Width=leftWidth-40;serviceDetails.Width=leftWidth-40;
            if(groupAction is not null)
            {
                groupAction.SetBounds(20,groupMode?671:516,Math.Min(270,leftWidth-40),38);
                if(groupMode)
                {
                    var buttonWidth=(leftWidth-60)/2;
                    editBill.SetBounds(20,598,buttonWidth,34);
                    editGroupBill.SetBounds(30+buttonWidth,598,buttonWidth,34);
                    viewGroupBill.SetBounds(30+buttonWidth,637,buttonWidth,34);
                }
                else editBill.SetBounds(20,470,Math.Min(260,leftWidth-40),38);
            }
            groupList.Columns[0].Width=70;
            groupList.Columns[1].Width=Math.Max(150,(int)((leftWidth-50)*.37));
            for(var i=2;i<groupList.Columns.Count;i++)
                groupList.Columns[i].Width=Math.Max(90,(leftWidth-50-groupList.Columns[0].Width-groupList.Columns[1].Width)/3);
            serviceDetails.Columns[0].Width=Math.Max(150,serviceDetails.Width-195);
            serviceDetails.Columns[1].Width=50;serviceDetails.Columns[2].Width=125;
            headName.Width=(int)(tableHead.Width*.57);
            headQty.SetBounds((int)(tableHead.Width*.57),0,45,36);
            headTotal.SetBounds((int)(tableHead.Width*.67),0,(int)(tableHead.Width*.30),36);
            for(var i=0;i<lineRows.Count;i++)
            {
                var row=lineRows[i];row.SetBounds(0,i*38,linesPanel.ClientSize.Width-2,38);
                row.Controls["name"]!.Width=(int)(row.Width*.56);
                row.Controls["qty"]!.SetBounds((int)(row.Width*.56),0,45,38);
                row.Controls["amount"]!.SetBounds((int)(row.Width*.67),0,(int)(row.Width*.30),38);
            }
            linesPanel.AutoScrollMinSize=new Size(0,lineRows.Count*38);
            amountCaption.Width=rightWidth-36;amount.Width=rightWidth-36;methodTitle.Width=rightWidth-36;
            for(var i=0;i<choices.Length;i++)choices[i].SetBounds(18,116+i*38,rightWidth-36,35);
            referenceTitle.Width=rightWidth-36;reference.Width=rightWidth-36;
            bankInfo.SetBounds(18,bankTop,rightWidth-36,52);
            qr.SetBounds((rightWidth-qrSize)/2,340,qrSize,qrSize);
            qrStatus.Bounds=qr.Bounds;
            confirm.Location=new Point(18,confirmTop);
            complete.Location=new Point(18,completeTop);
            confirm.Width=rightWidth-36;complete.Width=rightWidth-36;
            content.Height=Math.Max(leftHeight,rightHeight)+36;
            scroll.AutoScrollMinSize=new Size(0,content.Height);
        }
        scroll.Resize+=(_,_)=>LayoutWorkspace();
        functionPage.Controls.Add(scroll);functionPage.Controls.Add(heading);
        LayoutWorkspace();ActivatePage(functionPage);UpdateNavigationTitle();
    }
}

