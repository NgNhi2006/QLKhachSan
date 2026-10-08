using QLKhachSan.DAL;
using QLKhachSan.DTO;

namespace QLKhachSan.GUI;

public partial class ucDashboard
{
    private async Task<bool> ShowGroupCheckout(Stay selected)
    {
        var stays=data.Stays.Where(x=>x.CustomerId==selected.CustomerId && x.Status==StayStatus.Occupied).ToList();
        var quotes=await service.GroupQuoteAsync(stays);
        using var form=new Form {Text=$"Thanh toán nhiều phòng · {selected.Guest}",Size=new Size(990,710),
            MinimumSize=new Size(800,590),StartPosition=FormStartPosition.CenterParent,Font=AppTheme.Body,BackColor=AppTheme.Canvas};
        var root=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=1,RowCount=5,Padding=new Padding(20)};
        root.RowStyles.Add(new RowStyle(SizeType.Absolute,84));root.RowStyles.Add(new RowStyle(SizeType.Percent,100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute,96));root.RowStyles.Add(new RowStyle(SizeType.Absolute,50));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute,62));form.Controls.Add(root);
        root.Controls.Add(new Label {Text=$"Phiếu thanh toán theo khách\n{selected.Guest}  •  {selected.Phone}  •  Chọn các phòng thanh toán cùng lúc",
            Dock=DockStyle.Fill,Font=AppTheme.Bold,ForeColor=Color.White,BackColor=AppTheme.Navy,
            Padding=new Padding(18,15,8,0)},0,0);
        var list=new ListView {Dock=DockStyle.Fill,View=View.Details,CheckBoxes=true,FullRowSelect=true,
            GridLines=true,BackColor=Color.White,Margin=new Padding(0,10,0,10)};
        list.Columns.Add("Phòng",90);list.Columns.Add("Tiền phòng",150,HorizontalAlignment.Right);
        list.Columns.Add("Dịch vụ",120,HorizontalAlignment.Right);list.Columns.Add("Tổng",130,HorizontalAlignment.Right);
        list.Columns.Add("Cọc",120,HorizontalAlignment.Right);list.Columns.Add("Cần thu",120,HorizontalAlignment.Right);
        foreach(var q in quotes)
        {
            var item=new ListViewItem(q.Room.Number){Tag=q,Checked=true};
            item.SubItems.Add($"{q.RoomCharge:N0}");item.SubItems.Add($"{q.Services:N0}");
            item.SubItems.Add($"{q.Total:N0}");item.SubItems.Add($"{q.Stay.Deposit:N0}");item.SubItems.Add($"{q.ToCollect:N0}");
            list.Items.Add(item);
        }
        root.Controls.Add(list,0,1);
        var summary=new Label {Dock=DockStyle.Fill,BackColor=Color.White,Font=AppTheme.Bold,ForeColor=AppTheme.Ink,
            Padding=new Padding(16,11,0,0)};root.Controls.Add(summary,0,2);
        List<BillQuote> Chosen()=>list.CheckedItems.Cast<ListViewItem>().Select(x=>(BillQuote)x.Tag!).ToList();
        void Summarize()
        {
            var chosen=Chosen();summary.Text=$"{chosen.Count} phòng  •  Tổng tiền: {chosen.Sum(x=>x.Total):N0} đ  •  Cọc đã thu: {chosen.Sum(x=>x.Stay.Deposit):N0} đ\n"+
                $"Cần thu thêm: {chosen.Sum(x=>x.ToCollect):N0} đ  •  Cần hoàn cọc thừa: {chosen.Sum(x=>x.ToRefund):N0} đ  •  Thu ròng: {chosen.Sum(x=>x.ToCollect-x.ToRefund):N0} đ";
        }
        list.ItemChecked+=(_,_)=>Summarize();Summarize();
        var paymentRow=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=3,RowCount=1};
        paymentRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,220));
        paymentRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
        paymentRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,155));
        var method=Ui.Combo(new[]{"Tiền mặt","Chuyển khoản","Thẻ POS"});method.Dock=DockStyle.Fill;
        var reference=Ui.Text(100);reference.PlaceholderText="Mã giao dịch hoặc nội dung chuyển khoản";reference.Dock=DockStyle.Fill;
        var transferCode=$"KS-{selected.Id}-{DateTime.Now:MMddHHmmss}";
        void UpdateReference()
        {
            if((string?)method.SelectedItem=="Chuyển khoản" && string.IsNullOrWhiteSpace(reference.Text))
                reference.Text=transferCode;
        }
        method.SelectedIndexChanged+=(_,_)=>UpdateReference();
        UpdateReference();
        var showQr=new Button {Text="XEM QR",Dock=DockStyle.Fill,Margin=new Padding(7,0,0,0)};
        AppTheme.Button(showQr);
        paymentRow.Controls.Add(method,0,0);paymentRow.Controls.Add(reference,1,0);
        paymentRow.Controls.Add(showQr,2,0);root.Controls.Add(paymentRow,0,3);
        showQr.Click+=(_,_) =>
        {
            var amount=Chosen().Sum(x=>x.ToCollect-x.ToRefund);
            if((string?)method.SelectedItem!="Chuyển khoản" || amount<=0)
            {Ui.Error(form,new BusinessException("Chọn Chuyển khoản và phòng có số tiền cần thu để xem QR."));return;}
            var settings=AppSettings.Load();
            if(string.IsNullOrWhiteSpace(settings.BankCode) || string.IsNullOrWhiteSpace(settings.BankAccount) ||
                string.IsNullOrWhiteSpace(settings.BankAccountName))
            {Ui.Error(form,new BusinessException("Chưa cấu hình tài khoản ngân hàng."));return;}
            using var qrForm=new Form {Text="QR thanh toán nhiều phòng",Size=new Size(440,550),
                StartPosition=FormStartPosition.CenterParent,Font=AppTheme.Body,BackColor=Color.White};
            var info=new Label {Dock=DockStyle.Top,Height=115,Padding=new Padding(16,14,12,0),
                Text=$"Số tiền: {amount:N0} đ\n{settings.BankAccountName} · STK {settings.BankAccount}\nNội dung: {reference.Text}"};
            var picture=new PictureBox {Dock=DockStyle.Fill,SizeMode=PictureBoxSizeMode.Zoom,
                Margin=new Padding(18),BackColor=Color.White};
            var url=$"https://img.vietqr.io/image/{Uri.EscapeDataString(settings.BankCode)}-{Uri.EscapeDataString(settings.BankAccount)}-qr_only.png?amount={amount:0}&addInfo={Uri.EscapeDataString(reference.Text)}&accountName={Uri.EscapeDataString(settings.BankAccountName)}";
            qrForm.Controls.Add(picture);qrForm.Controls.Add(info);
            qrForm.Shown+=(_,_)=>picture.LoadAsync(url);
            qrForm.FormClosed+=(_,_)=>picture.CancelAsync();
            qrForm.ShowDialog(form);
        };
        var buttons=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=3,RowCount=1,Padding=new Padding(0,8,0,0)};
        buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,25));
        buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,32));
        buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,43));
        var cancel=new Button {Text="QUAY LẠI",Dock=DockStyle.Fill,Margin=new Padding(0,0,7,0)};
        var viewBill=new Button {Text="XEM / IN BILL TẠM TÍNH",Dock=DockStyle.Fill,Margin=new Padding(7,0,7,0)};
        var pay=new Button {Text="XÁC NHẬN THANH TOÁN",Dock=DockStyle.Fill,Margin=new Padding(7,0,0,0)};
        AppTheme.Button(cancel);AppTheme.Button(viewBill);AppTheme.Button(pay,true);
        buttons.Controls.Add(cancel,0,0);buttons.Controls.Add(viewBill,1,0);
        buttons.Controls.Add(pay,2,0);root.Controls.Add(buttons,0,4);
        cancel.Click+=(_,_)=>form.Close();
        viewBill.Click+=async (_,_)=>
        {
            try{await ShowBillBeforePayment(Chosen(),(string)method.SelectedItem!,reference.Text);}
            catch(Exception ex){Ui.Error(form,ex);}
        };
        var completed=false;
        pay.Click+=async (_,_)=>
        {
            var chosen=Chosen();
            if(chosen.Count<2){Ui.Error(form,new BusinessException("Chọn ít nhất hai phòng của khách."));return;}
            if((string?)method.SelectedItem is "Chuyển khoản" or "Thẻ POS" &&
                string.IsNullOrWhiteSpace(reference.Text))
            {Ui.Error(form,new BusinessException("Nhập mã giao dịch hoặc nội dung chuyển khoản trước khi xác nhận."));return;}
            if(!Ui.Confirm(form,$"Xác nhận đã thu ròng {chosen.Sum(x=>x.ToCollect-x.ToRefund):N0} đ cho {chosen.Count} phòng?"))return;
            pay.Enabled=false;
            try
            {
                GroupCheckoutResult? result=null;
                await Changed(async()=>result=await service.GroupCheckoutAsync(chosen,(string)method.SelectedItem!,reference.Text));
                completed=true;
                if(result is not null)
                {
                    await PrintGroupCheckout(result,chosen,(string)method.SelectedItem!,reference.Text);
                }
                form.Close();
            }
            catch(Exception ex){Ui.Error(form,ex);}
            finally{if(!form.IsDisposed)pay.Enabled=true;}
        };
        form.ShowDialog(this);
        return completed;
    }

    private async Task PrintGroupCheckout(GroupCheckoutResult result,IReadOnlyList<BillQuote> quotes,string method,string reference)
    {
        var settings=AppSettings.Load();
        var amount=result.Collected-result.Refunded;
        Image? qrImage=null;
        if(method=="Chuyển khoản" && amount>0 && !string.IsNullOrWhiteSpace(settings.BankCode) &&
            !string.IsNullOrWhiteSpace(settings.BankAccount) && !string.IsNullOrWhiteSpace(settings.BankAccountName))
        {
            var url=$"https://img.vietqr.io/image/{Uri.EscapeDataString(settings.BankCode)}-{Uri.EscapeDataString(settings.BankAccount)}-qr_only.png?amount={amount:0}&addInfo={Uri.EscapeDataString(reference)}&accountName={Uri.EscapeDataString(settings.BankAccountName)}";
            try
            {
                using var client=new HttpClient {Timeout=TimeSpan.FromSeconds(8)};
                var bytes=await client.GetByteArrayAsync(url);
                using var stream=new MemoryStream(bytes);
                using var loaded=Image.FromStream(stream);
                qrImage=(Image)loaded.Clone();
            }
            catch(HttpRequestException){} catch(TaskCanceledException){} catch(ArgumentException){}
        }
        using(qrImage)
        {
        using var document=new System.Drawing.Printing.PrintDocument {DocumentName=$"Phiếu thanh toán tổng #{result.GroupId}"};
        document.DefaultPageSettings.Margins=new System.Drawing.Printing.Margins(60,60,55,55);
        var details=BuildBillDetailRows(quotes);
        var nextRow=0;
        document.BeginPrint+=(_,_)=>nextRow=0;
        document.PrintPage+=(_,e)=>
        {
            if(e.Graphics is null)return;
            using var title=new Font("Segoe UI Semibold",19,FontStyle.Bold);
            using var body=new Font("Segoe UI",10);
            using var bold=new Font("Segoe UI Semibold",10,FontStyle.Bold);
            using var labelFormat=new StringFormat {Trimming=StringTrimming.EllipsisCharacter};
            using var moneyFormat=new StringFormat {Alignment=StringAlignment.Far};
            var g=e.Graphics;var x=e.MarginBounds.Left;var y=e.MarginBounds.Top;var width=e.MarginBounds.Width;
            g.DrawString(settings.HotelName,title,Brushes.Black,x,y);y+=44;
            if(!string.IsNullOrWhiteSpace(settings.HotelAddress))
            {g.DrawString(settings.HotelAddress,body,Brushes.Black,x,y);y+=23;}
            if(!string.IsNullOrWhiteSpace(settings.HotelPhone))
            {g.DrawString($"Điện thoại: {settings.HotelPhone}",body,Brushes.Black,x,y);y+=23;}
            g.DrawString($"PHIẾU THANH TOÁN TỔNG  #{result.GroupId}",bold,Brushes.Black,x,y);y+=31;
            g.DrawString($"Khách hàng: {result.Guest} · {quotes[0].Stay.Phone}",body,Brushes.Black,x,y);y+=25;
            g.DrawString($"Ngày lập: {DateTime.Now:dd/MM/yyyy HH:mm} · Hình thức: {method}",body,Brushes.Black,x,y);y+=30;
            g.DrawLine(Pens.Gray,x,y,x+width,y);y+=12;
            var footerSpace=method=="Chuyển khoản" && amount>0?390:220;
            while(nextRow<details.Count && y+footerSpace<e.MarginBounds.Bottom)
            {
                var row=details[nextRow++];
                g.DrawString(row.Label,row.Strong?bold:body,Brushes.Black,
                    new RectangleF(x,y,width-185,23),labelFormat);
                if(row.Value.Length>0)
                    g.DrawString(row.Value,row.Strong?bold:body,Brushes.Black,
                        new RectangleF(x+width-175,y,175,23),moneyFormat);
                y+=row.Label.Length==0?12:24;
            }
            if(nextRow<details.Count){e.HasMorePages=true;return;}
            g.DrawLine(Pens.Gray,x,y,x+width,y);y+=16;
            g.DrawString($"TỔNG TIỀN: {result.Total:N0} đ",bold,Brushes.Black,x,y);y+=29;
            g.DrawString($"Cọc đã thu: {result.Deposits:N0} đ",body,Brushes.Black,x,y);y+=26;
            g.DrawString($"Thu thêm: {result.Collected:N0} đ  •  Hoàn cọc: {result.Refunded:N0} đ",bold,Brushes.Black,x,y);y+=40;
            if(method=="Chuyển khoản" && amount>0)
            {
                g.DrawString($"Chuyển khoản: {amount:N0} đ",bold,Brushes.Black,x,y);y+=24;
                g.DrawString($"{settings.BankAccountName}  •  STK {settings.BankAccount}",body,Brushes.Black,x,y);y+=24;
                g.DrawString($"Nội dung: {reference}",body,Brushes.Black,x,y);y+=27;
                if(qrImage is not null){g.DrawImage(qrImage,new Rectangle(x,y,155,155));y+=165;}
            }
            g.DrawString("Thu ngân",bold,Brushes.Black,x+65,y);g.DrawString("Khách hàng",bold,Brushes.Black,x+width-170,y);
            g.DrawString("Phiếu thanh toán nội bộ, không phải hóa đơn GTGT.",body,Brushes.Gray,x,e.MarginBounds.Bottom-28);
        };
        using var billForm=new Form {Text=$"Bill thanh toán tổng #{result.GroupId}",
            Size=new Size(1000,800),MinimumSize=new Size(720,580),
            StartPosition=FormStartPosition.CenterParent,Font=AppTheme.Body};
        var preview=new PrintPreviewControl {Document=document,Dock=DockStyle.Fill,Zoom=0.9};
        BillPreviewUi.EnableWheel(preview);
        var actions=new TableLayoutPanel {Dock=DockStyle.Bottom,Height=64,ColumnCount=2,
            Padding=new Padding(12,8,12,8)};
        actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,50));
        actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,50));
        var skip=new Button {Text="KHÔNG IN · ĐÓNG BILL",Dock=DockStyle.Fill,Margin=new Padding(0,0,6,0)};
        var print=new Button {Text="IN BILL CHO KHÁCH",Dock=DockStyle.Fill,Margin=new Padding(6,0,0,0)};
        AppTheme.Button(skip);AppTheme.Button(print,true);
        actions.Controls.Add(skip,0,0);actions.Controls.Add(print,1,0);
        billForm.Controls.Add(preview);
        billForm.Controls.Add(BillPreviewUi.CreateNavigation(preview));
        billForm.Controls.Add(actions);
        skip.Click+=(_,_)=>billForm.Close();
        print.Click+=(_,_)=>
        {
            using var printer=new PrintDialog {Document=document,UseEXDialog=true};
            if(printer.ShowDialog(billForm)==DialogResult.OK)document.Print();
        };
        billForm.ShowDialog(this);
        }
    }
}
