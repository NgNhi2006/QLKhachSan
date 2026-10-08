using System.Drawing.Printing;
using QLKhachSan.DAL;
using QLKhachSan.DTO;

namespace QLKhachSan.GUI;

public partial class ucDashboard
{
    private sealed record BillDetailRow(string Label,string Value,bool Strong=false);

    private static List<BillDetailRow> BuildBillDetailRows(IReadOnlyList<BillQuote> quotes)
    {
        var rows=new List<BillDetailRow>();
        foreach(var q in quotes)
        {
            rows.Add(new BillDetailRow($"PHÒNG {q.Room.Number} · {q.Room.Type}","",true));
            rows.Add(new BillDetailRow(
                $"Nhận {(q.Stay.CheckIn??q.Stay.Arrival):dd/MM/yyyy HH:mm} · Trả {q.At:dd/MM/yyyy HH:mm}",""));
            rows.Add(new BillDetailRow("Tiền phòng",$"{q.RoomCharge:N0} đ"));
            foreach(var line in q.Lines.Where(line=>line.Cancelled is null))
                rows.Add(new BillDetailRow(
                    $"  {line.Name} · {line.Quantity} × {line.Price:N0} đ",$"{line.Total:N0} đ"));
            rows.Add(new BillDetailRow("Tổng phòng và dịch vụ",$"{q.Total:N0} đ",true));
            rows.Add(new BillDetailRow("Cọc đã thu",$"{q.Stay.Deposit:N0} đ"));
            rows.Add(new BillDetailRow(q.ToCollect>0?"Cần thu thêm":"Cần hoàn cọc",
                $"{(q.ToCollect>0?q.ToCollect:q.ToRefund):N0} đ",true));
            rows.Add(new BillDetailRow("",""));
        }
        return rows;
    }

    private async Task ShowBillBeforePayment(IReadOnlyList<BillQuote> quotes,string method,string reference)
    {
        if(quotes.Count==0)throw new BusinessException("Chưa chọn phòng để xem bill.");
        var settings=AppSettings.Load();
        var net=quotes.Sum(x=>x.ToCollect-x.ToRefund);
        Image? qrImage=null;
        if(method=="Chuyển khoản" && net>0 &&
            !string.IsNullOrWhiteSpace(settings.BankCode) &&
            !string.IsNullOrWhiteSpace(settings.BankAccount) &&
            !string.IsNullOrWhiteSpace(settings.BankAccountName))
        {
            var url=$"https://img.vietqr.io/image/{Uri.EscapeDataString(settings.BankCode)}-{Uri.EscapeDataString(settings.BankAccount)}-qr_only.png?amount={net:0}&addInfo={Uri.EscapeDataString(reference)}&accountName={Uri.EscapeDataString(settings.BankAccountName)}";
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
        using(var document=new PrintDocument {DocumentName="Bill tạm tính"})
        {
            document.DefaultPageSettings.Margins=new Margins(60,60,55,55);
            var details=BuildBillDetailRows(quotes);
            var nextRow=0;
            document.BeginPrint+=(_,_)=>nextRow=0;
            document.PrintPage+=(_,e)=>
            {
                if(e.Graphics is null)return;
                var g=e.Graphics;var x=e.MarginBounds.Left;var y=e.MarginBounds.Top;
                var width=e.MarginBounds.Width;
                using var title=new Font("Segoe UI",18,FontStyle.Bold);
                using var bold=new Font("Segoe UI",10,FontStyle.Bold);
                using var body=new Font("Segoe UI",10);
                using var labelFormat=new StringFormat {Trimming=StringTrimming.EllipsisCharacter};
                using var moneyFormat=new StringFormat {Alignment=StringAlignment.Far};
                g.DrawString(settings.HotelName,title,Brushes.Black,x,y);y+=45;
                if(!string.IsNullOrWhiteSpace(settings.HotelAddress))
                {g.DrawString(settings.HotelAddress,body,Brushes.Black,x,y);y+=23;}
                if(!string.IsNullOrWhiteSpace(settings.HotelPhone))
                {g.DrawString($"Điện thoại: {settings.HotelPhone}",body,Brushes.Black,x,y);y+=23;}
                g.DrawString("BILL TẠM TÍNH · CHƯA THANH TOÁN",bold,Brushes.Black,x,y);y+=31;
                g.DrawString($"Khách: {quotes[0].Stay.Guest} · {quotes[0].Stay.Phone}",body,Brushes.Black,x,y);y+=25;
                g.DrawString($"Lập lúc: {ServerNow:dd/MM/yyyy HH:mm} · Hình thức: {method}",body,Brushes.Black,x,y);y+=30;
                g.DrawLine(Pens.Gray,x,y,x+width,y);y+=14;
                var footerSpace=method=="Chuyển khoản" && net>0?380:210;
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
                g.DrawString($"Tổng bill: {quotes.Sum(q=>q.Total):N0} đ",bold,Brushes.Black,x,y);y+=28;
                g.DrawString($"Cọc đã thu: {quotes.Sum(q=>q.Stay.Deposit):N0} đ",body,Brushes.Black,x,y);y+=27;
                g.DrawString(net>=0?$"Cần thu thêm: {net:N0} đ":$"Cần hoàn: {-net:N0} đ",bold,Brushes.Black,x,y);y+=33;
                if(method=="Chuyển khoản" && net>0)
                {
                    g.DrawString($"Chuyển khoản: {net:N0} đ",bold,Brushes.Black,x,y);y+=25;
                    g.DrawString($"{settings.BankAccountName} · STK {settings.BankAccount}",body,Brushes.Black,x,y);y+=25;
                    g.DrawString($"Nội dung: {reference}",body,Brushes.Black,x,y);y+=30;
                    if(qrImage is not null){g.DrawImage(qrImage,new Rectangle(x,y,150,150));y+=160;}
                }
                g.DrawString("Số tiền có thể thay đổi nếu phát sinh thêm dịch vụ trước khi thanh toán.",
                    body,Brushes.Gray,x,Math.Min(y+12,e.MarginBounds.Bottom-28));
            };
            using var form=new Form {Text="Xem bill trước khi thanh toán",Size=new Size(1000,800),
                MinimumSize=new Size(720,580),StartPosition=FormStartPosition.CenterParent,
                Font=AppTheme.Body};
            var preview=new PrintPreviewControl {Document=document,Dock=DockStyle.Fill,Zoom=0.9};
            BillPreviewUi.EnableWheel(preview);
            var footer=new TableLayoutPanel {Dock=DockStyle.Bottom,Height=64,ColumnCount=2,
                Padding=new Padding(12,8,12,8)};
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,50));
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,50));
            var close=new Button {Text="ĐÓNG · TIẾP TỤC THANH TOÁN",Dock=DockStyle.Fill,
                Margin=new Padding(0,0,6,0)};
            var print=new Button {Text="IN BILL TẠM TÍNH",Dock=DockStyle.Fill,
                Margin=new Padding(6,0,0,0)};
            AppTheme.Button(close);AppTheme.Button(print,true);
            footer.Controls.Add(close,0,0);footer.Controls.Add(print,1,0);
            form.Controls.Add(preview);
            form.Controls.Add(BillPreviewUi.CreateNavigation(preview));
            form.Controls.Add(footer);
            close.Click+=(_,_)=>form.Close();
            print.Click+=(_,_)=>
            {
                using var printer=new PrintDialog {Document=document,UseEXDialog=true};
                if(printer.ShowDialog(form)==DialogResult.OK)document.Print();
            };
            form.ShowDialog(this);
        }
    }
}
