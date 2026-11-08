using System.Drawing.Printing;
using System.Text;
using System.Globalization;
using QLKhachSan.DAL;
using QLKhachSan.DTO;

namespace QLKhachSan.GUI;

public partial class ucDashboard
{
    private static string Csv(object? value)
    {
        var text=value switch {DateTime date=>date.ToString("dd/MM/yyyy HH:mm:ss"),decimal money=>money.ToString("0.##",CultureInfo.InvariantCulture),_=>value?.ToString()??""};
        if(value is string && text.TrimStart().FirstOrDefault() is '=' or '+' or '-' or '@')text="'"+text;
        return "\""+text.Replace("\"","\"\"")+"\"";
    }
    private void ExportReport(string filename,string title,DateTime from,DateTime through,string[] headers,IEnumerable<object?[]> rows,object?[] totals)
    {
        using var save=new SaveFileDialog {Filter="CSV UTF-8 (*.csv)|*.csv",FileName=$"{filename}-{from:yyyyMMdd}-{through:yyyyMMdd}.csv",OverwritePrompt=true};
        if(save.ShowDialog(this)!=DialogResult.OK)return;
        var settings=AppSettings.Load();
        using var writer=new StreamWriter(save.FileName,false,new UTF8Encoding(true));
        void Row(params object?[] values)=>writer.WriteLine(string.Join(",",values.Select(Csv)));
        Row(settings.HotelName,title);
        Row("Mã đơn vị nội bộ",settings.InternalIssuerCode,"Kỳ báo cáo",$"{from:dd/MM/yyyy} - {through:dd/MM/yyyy}");
        Row("Xuất lúc",ServerNow);
        writer.WriteLine();Row(headers.Cast<object?>().ToArray());
        foreach(var row in rows)Row(row);
        Row(totals);
        writer.WriteLine();Row("Số liệu xuất từ hệ thống quản lý khách sạn; đối chiếu với chứng từ gốc khi quyết toán.");
        MessageBox.Show(this,"Đã xuất báo cáo CSV UTF-8.","Xuất báo cáo");
    }
    private void ExportInvoices(PeriodReport report,DateTime from,DateTime through)
    {
        var rows=report.Invoices.OrderBy(x=>x.Issued).ThenBy(x=>x.Id).Select(x=>new object?[]
        {$"PT-{x.Issued:yyyyMMdd}-{x.Id:000000}",x.Issued,x.Room,x.Guest,x.RoomCharge,x.ServiceCharge,x.Total,x.Deposit,x.Collected,x.Refunded,x.Method}).ToArray();
        ExportReport("bao-cao-hoa-don","BẢNG KÊ PHIẾU THANH TOÁN",from,through,
            ["Số phiếu","Ngày lập","Phòng","Khách hàng","Tiền phòng (VND)","Dịch vụ (VND)","Tổng hóa đơn (VND)","Cọc đã thu (VND)","Thu thêm (VND)","Hoàn khách (VND)","Hình thức"],rows,
            ["TỔNG CỘNG",null,null,$"{rows.Length} phiếu",report.Invoices.Sum(x=>x.RoomCharge),report.Invoices.Sum(x=>x.ServiceCharge),report.Invoices.Sum(x=>x.Total),report.Invoices.Sum(x=>x.Deposit),report.Invoices.Sum(x=>x.Collected),report.Invoices.Sum(x=>x.Refunded),null]);
    }
    private void ExportPayments(PeriodReport report,DateTime from,DateTime through)
    {
        var rows=report.Payments.OrderBy(x=>x.Created).ThenBy(x=>x.Id).Select(x=>new object?[]
        {$"GD-{x.Created:yyyyMMdd}-{x.Id:000000}",x.Created,x.Guest,x.Kind switch {"Deposit"=>"Thu cọc","Checkout"=>"Thu thanh toán","Refund"=>"Hoàn khách","Forfeit"=>"Cọc không hoàn",_=>x.Kind},x.Amount,x.CashFlow,x.Method,x.Username,x.Note}).ToArray();
        ExportReport("bao-cao-thu-chi","BẢNG KÊ THU, HOÀN TIỀN",from,through,
            ["Mã giao dịch","Thời điểm","Khách hàng","Loại giao dịch","Số tiền (VND)","Dòng tiền (VND)","Hình thức","Nhân viên","Ghi chú"],rows,
            ["TỔNG CỘNG",null,$"{rows.Length} giao dịch",null,report.Payments.Sum(x=>x.Amount),report.Payments.Sum(x=>x.CashFlow),null,null,null]);
    }
    private void ExportGrid(DataGridView grid,string filename)
    {
        ExcelExport.ExportGrid(this,grid,filename,ServerNow);
    }
    private void PrintInvoice(Invoice invoice,Stay stay,IReadOnlyList<ServiceLine> services)
    {
        var settings=AppSettings.Load();
        var items=services.Where(s=>s.Cancelled is null).ToArray();
        using var document=new PrintDocument {DocumentName=$"HoaDon-{invoice.Id}"};
        using var normal=new Font("Segoe UI",9);
        using var bold=new Font("Segoe UI",9,FontStyle.Bold);
        using var title=new Font("Segoe UI",17,FontStyle.Bold);
        var index=0;
        document.BeginPrint+=(_,_)=>index=0;
        document.PrintPage+=(_,e)=>
        {
            if(e.Graphics is null)return;
            var g=e.Graphics;var bounds=e.MarginBounds;var left=bounds.Left;var right=bounds.Right;
            var y=(float)bounds.Top;
            void Text(string value,Font font,float x,float top,float width,StringAlignment align=StringAlignment.Near)
            {
                using var format=new StringFormat {Alignment=align,Trimming=StringTrimming.EllipsisCharacter};
                g.DrawString(value,font,Brushes.Black,new RectangleF(x,top,width,25),format);
            }
            void Rule(float top)=>g.DrawLine(Pens.SteelBlue,left,top,right,top);
            var logo=new Rectangle(left,(int)y,92,82);
            using(var logoFill=new SolidBrush(Color.FromArgb(234,179,75)))
            using(var logoInk=new SolidBrush(Color.FromArgb(27,50,79)))
            using(var logoFont=new Font("Segoe UI",21,FontStyle.Bold))
            {
                g.FillRectangle(logoInk,logo);
                using var roof=new Pen(logoFill,4);
                g.DrawLine(roof,left+17,y+30,left+46,y+12);
                g.DrawLine(roof,left+46,y+12,left+75,y+30);
                using var format=new StringFormat {Alignment=StringAlignment.Center};
                g.DrawString("KS",logoFont,Brushes.White,new RectangleF(left+8,y+35,76,40),format);
            }
            var infoX=left+107;var infoWidth=bounds.Width-107;
            Text(settings.HotelName.ToUpperInvariant(),bold,infoX,y,infoWidth);y+=20;
            if(!string.IsNullOrWhiteSpace(settings.HotelAddress)){Text(settings.HotelAddress,normal,infoX,y,infoWidth);y+=18;}
            if(!string.IsNullOrWhiteSpace(settings.HotelPhone)){Text("Điện thoại: "+settings.HotelPhone,normal,infoX,y,infoWidth);y+=18;}
            if(!string.IsNullOrWhiteSpace(settings.HotelEmail)){Text("Email: "+settings.HotelEmail,normal,infoX,y,infoWidth);y+=18;}
            if(!string.IsNullOrWhiteSpace(settings.HotelWebsite)){Text("Website: "+settings.HotelWebsite,normal,infoX,y,infoWidth);y+=18;}
            y=Math.Max(y,logo.Bottom+7);Rule(y);y+=15;Text("PHIẾU THANH TOÁN",title,left,y,bounds.Width,StringAlignment.Center);y+=39;
            Text($"Số: PT-{invoice.Issued:yyyyMMdd}-{invoice.Id:000000}",bold,left,y,330);Text($"Ngày lập: {invoice.Issued:dd/MM/yyyy HH:mm}",normal,right-250,y,250,StringAlignment.Far);y+=25;
            var second=left+bounds.Width/2+10;var half=bounds.Width/2-15;
            Text($"Khách hàng: {invoice.Guest}",normal,left,y,half);Text($"Ngày đến: {(stay.CheckIn??stay.Arrival):dd/MM/yyyy HH:mm}",normal,second,y,half);y+=23;
            Text($"Điện thoại: {stay.Phone}",normal,left,y,half);Text($"Ngày đi: {invoice.Issued:dd/MM/yyyy HH:mm}",normal,second,y,half);y+=23;
            Text($"Phòng: {invoice.Room}",normal,left,y,half);Text($"Hình thức: {invoice.Method}",normal,second,y,half);y+=28;
            Rule(y);y+=8;
            Text("STT",bold,left,y,35);Text("Hạng mục",bold,left+38,y,bounds.Width-260);Text("SL",bold,right-218,y,45,StringAlignment.Far);Text("Đơn giá",bold,right-168,y,78,StringAlignment.Far);Text("Thành tiền",bold,right-83,y,83,StringAlignment.Far);y+=23;Rule(y);y+=7;
            if(index==0){Text("1",normal,left,y,35);Text("Tiền phòng",normal,left+38,y,bounds.Width-260);Text(invoice.RoomCharge.ToString("N0"),normal,right-83,y,83,StringAlignment.Far);y+=25;}
            while(index<items.Length && y+215<bounds.Bottom)
            {
                var item=items[index];Text((index+2).ToString(),normal,left,y,35);Text(item.Name,normal,left+38,y,bounds.Width-260);
                Text(item.Quantity.ToString(),normal,right-218,y,45,StringAlignment.Far);Text(item.Price.ToString("N0"),normal,right-168,y,78,StringAlignment.Far);Text(item.Total.ToString("N0"),normal,right-83,y,83,StringAlignment.Far);y+=25;index++;
            }
            if(index<items.Length){e.HasMorePages=true;return;}
            Rule(y);y+=12;
            void Amount(string label,decimal value,bool emphasize=false){Text(label,emphasize?bold:normal,right-300,y,205,StringAlignment.Far);Text(value.ToString("N0")+" đ",emphasize?bold:normal,right-90,y,90,StringAlignment.Far);y+=23;}
            Amount("Tiền phòng",invoice.RoomCharge);Amount("Dịch vụ",invoice.ServiceCharge);Amount("TỔNG HÓA ĐƠN",invoice.Total,true);Amount("Cọc đã thu",invoice.Deposit);Amount("Cần thu sau cọc",Math.Max(0,invoice.Total-invoice.Deposit),true);Amount("Đã thu thêm",invoice.Collected);Amount("Hoàn khách",invoice.Refunded);
            y+=10;Rule(y);y+=12;Text("Thu ngân",bold,left+35,y,170,StringAlignment.Center);Text("Khách hàng",bold,right-205,y,170,StringAlignment.Center);y+=22;
            Text("(Ký, ghi rõ họ tên)",normal,left+35,y,170,StringAlignment.Center);Text("(Ký, ghi rõ họ tên)",normal,right-205,y,170,StringAlignment.Center);
            Text("Phiếu thanh toán nội bộ, không phải hóa đơn GTGT hoặc chứng từ thuế.",normal,left,bounds.Bottom-25,bounds.Width,StringAlignment.Center);
            e.HasMorePages=false;
        };
        using var preview=new PrintPreviewDialog {Document=document,Width=1000,Height=800,StartPosition=FormStartPosition.CenterParent};
        preview.ShowDialog(this);
    }
}
