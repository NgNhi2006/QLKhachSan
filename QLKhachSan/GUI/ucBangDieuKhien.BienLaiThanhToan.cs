using System.Drawing.Printing;
using QLKhachSan.DAL;
using QLKhachSan.DTO;

namespace QLKhachSan.GUI;

public partial class ucDashboard
{
    private void ShowCheckoutReceipt(long invoiceId,BillQuote bill,string method,string reference,IWin32Window owner)
    {
        var settings=AppSettings.Load();
        var items=bill.DraftLines is { } draft
            ?draft.Where(x=>x.Category!="Room" && x.Quantity>0).ToArray()
            :bill.Lines.Where(line=>line.Cancelled is null)
                .Select(line=>new BillDraftLine(line.Name,line.Quantity,line.Price,"Service",line.Id)).ToArray();
        using var document=new PrintDocument {DocumentName=$"Bill thanh toán PT-{invoiceId:000000}"};
        // PrintDocument uses hundredths of an inch. 315 is approximately 80 mm.
        document.DefaultPageSettings.PaperSize=new PaperSize("Bill 80 mm",315,
            Math.Min(3200,650+items.Length*75));
        document.DefaultPageSettings.Margins=new Margins(12,12,12,12);
        document.PrintPage+=(_,e)=>
        {
            if(e.Graphics is null)return;
            var g=e.Graphics;
            var x=e.MarginBounds.Left;
            var width=e.MarginBounds.Width;
            var y=(float)e.MarginBounds.Top;
            using var title=new Font("Segoe UI",14,FontStyle.Bold);
            using var heading=new Font("Segoe UI",10,FontStyle.Bold);
            using var body=new Font("Segoe UI",9);
            using var small=new Font("Segoe UI",8);
            using var centered=new StringFormat {Alignment=StringAlignment.Center};
            using var right=new StringFormat {Alignment=StringAlignment.Far};

            void Line(string value,Font font,bool center=false)
            {
                var height=(int)Math.Ceiling(g.MeasureString(value,font,width).Height)+3;
                g.DrawString(value,font,Brushes.Black,new RectangleF(x,y,width,height),center?centered:StringFormat.GenericDefault);
                y+=height;
            }
            void Rule()
            {
                y+=4;g.DrawLine(Pens.Gray,x,y,x+width,y);y+=7;
            }
            void Pair(string label,string value,Font? font=null)
            {
                var f=font??body;
                g.DrawString(label,f,Brushes.Black,new RectangleF(x,y,width*.57f,24));
                g.DrawString(value,f,Brushes.Black,new RectangleF(x+width*.57f,y,width*.43f,24),right);
                y+=22;
            }

            Line(settings.HotelName.ToUpperInvariant(),title,true);
            if(!string.IsNullOrWhiteSpace(settings.HotelAddress))Line(settings.HotelAddress,small,true);
            if(!string.IsNullOrWhiteSpace(settings.HotelPhone))Line($"ĐT: {settings.HotelPhone}",small,true);
            Rule();
            Line("PHIẾU THANH TOÁN",heading,true);
            Line(method=="Công nợ OTA"?"ĐÃ GHI CÔNG NỢ":"ĐÃ THANH TOÁN",body,true);
            Line($"Số: PT-{invoiceId:000000}",body,true);
            Line($"Ngày: {bill.At:dd/MM/yyyy HH:mm}",body,true);
            Rule();
            Line($"Phòng: {bill.Room.Number}  •  {bill.Room.Type}",body);
            Line($"Khách: {bill.Stay.Guest}",body);
            Line($"Nhận: {(bill.Stay.CheckIn??bill.Stay.Arrival):dd/MM/yyyy HH:mm}",small);
            Line($"Trả: {bill.At:dd/MM/yyyy HH:mm}",small);
            Rule();
            Line("CHI TIẾT DỊCH VỤ",heading);
            Line("Tiền phòng",body);
            Pair($"{BilledRoomDays(bill)} ngày",$"{bill.RoomCharge:N0} đ",small);
            foreach(var item in items)
            {
                Line(item.Name,body);
                Pair(item.AmountOverride is null?$"{item.Quantity} × {item.UnitPrice:N0}":$"SL {item.Quantity} · đã chỉnh",
                    $"{item.Total:N0} đ",small);
            }
            Rule();
            if(bill.DiscountCode is { } code)
            {
                Pair("Tạm tính",$"{bill.RoomCharge+bill.Services:N0} đ");
                Pair($"Giảm giá {code}",$"−{bill.DiscountAmount:N0} đ");
            }
            Pair("TỔNG CỘNG",$"{bill.Total:N0} đ",heading);
            Pair("Cọc đã thu",$"{bill.Stay.Deposit:N0} đ");
            if(bill.ToCollect>0)Pair(method=="Công nợ OTA"?"Công nợ OTA":"Thu khi trả phòng",
                $"{bill.ToCollect:N0} đ",heading);
            if(bill.ToRefund>0)Pair("Hoàn cọc cho khách",$"{bill.ToRefund:N0} đ",heading);
            Line($"Hình thức: {method}",body);
            if(!string.IsNullOrWhiteSpace(reference))Line($"Tham chiếu: {reference}",small);
            Rule();
            Line("Cảm ơn quý khách!",heading,true);
            Line("Hẹn gặp lại quý khách.",small,true);
            y+=8;
            Line("Phiếu thanh toán, không phải hóa đơn GTGT.",small,true);
            e.HasMorePages=false;
        };

        using var form=new Form {Text=$"Bill đã thanh toán · PT-{invoiceId:000000}",
            Size=new Size(780,800),MinimumSize=new Size(550,580),
            StartPosition=FormStartPosition.CenterParent,Font=AppTheme.Body};
        var preview=new PrintPreviewControl {Document=document,Dock=DockStyle.Fill,Zoom=1};
        BillPreviewUi.EnableWheel(preview);
        var actions=new TableLayoutPanel {Dock=DockStyle.Bottom,Height=64,ColumnCount=2,
            Padding=new Padding(12,8,12,8)};
        actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,50));
        actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,50));
        var close=new Button {Text="ĐÓNG BILL",Dock=DockStyle.Fill,Margin=new Padding(0,0,6,0)};
        var print=new Button {Text="IN BILL CHO KHÁCH",Dock=DockStyle.Fill,Margin=new Padding(6,0,0,0)};
        AppTheme.Button(close);AppTheme.Button(print,true);
        actions.Controls.Add(close,0,0);actions.Controls.Add(print,1,0);
        form.Controls.Add(preview);
        form.Controls.Add(BillPreviewUi.CreateNavigation(preview));
        form.Controls.Add(actions);
        close.Click+=(_,_)=>form.Close();
        print.Click+=(_,_)=>PrintReceipt(form,document);
        form.ShowDialog(owner);
    }

    private static void PrintReceipt(IWin32Window owner,PrintDocument document)
    {
        using var printer=new PrintDialog {Document=document,UseEXDialog=true};
        if(printer.ShowDialog(owner)==DialogResult.OK)document.Print();
    }
}
