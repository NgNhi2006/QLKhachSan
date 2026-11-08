using QLKhachSan.DAL;
using QLKhachSan.DTO;

namespace QLKhachSan.GUI;

public partial class ucDashboard
{
    private void PrintDepositReceipt(DepositReceipt receipt)
    {
        using var document=new System.Drawing.Printing.PrintDocument {DocumentName=$"Phiếu cọc #{receipt.PaymentId}"};
        document.PrintPage+=(_,e)=>
        {
            if(e.Graphics is null)return;
            using var title=new Font("Segoe UI Semibold",19,FontStyle.Bold);
            using var bold=new Font("Segoe UI Semibold",11,FontStyle.Bold);
            using var body=new Font("Segoe UI",10);
            var g=e.Graphics;var x=e.MarginBounds.Left;var y=e.MarginBounds.Top;var width=e.MarginBounds.Width;
            g.DrawString(AppSettings.Load().HotelName,title,Brushes.Black,x,y);y+=49;
            g.DrawString($"PHIẾU THU TIỀN CỌC  #{receipt.PaymentId}",bold,Brushes.Black,x,y);y+=34;
            g.DrawString($"Ngày thu: {receipt.PaidAt:dd/MM/yyyy HH:mm}",body,Brushes.Black,x,y);y+=27;
            g.DrawString($"Khách hàng: {receipt.Guest}  •  Điện thoại: {receipt.Phone}",body,Brushes.Black,x,y);y+=27;
            g.DrawString($"Phòng: {receipt.Room}",body,Brushes.Black,x,y);y+=36;
            g.DrawLine(Pens.Gray,x,y,x+width,y);y+=17;
            g.DrawString($"Cọc thu lần này: {receipt.Amount:N0} đ",bold,Brushes.Black,x,y);y+=30;
            g.DrawString($"Tổng cọc đã thu: {receipt.TotalDeposited:N0} đ",body,Brushes.Black,x,y);y+=27;
            g.DrawString($"{(receipt.IsEstimate?"Tiền phòng dự kiến":"Tổng hóa đơn")}: {(receipt.FinalInvoiceTotal??receipt.EstimatedRoomCharge):N0} đ",body,Brushes.Black,x,y);y+=27;
            g.DrawString($"{(receipt.IsEstimate?"Dự kiến còn phải trả":"Còn phải trả sau cọc")}: {receipt.Outstanding:N0} đ",bold,Brushes.Black,x,y);y+=27;
            if(receipt.IsEstimate){g.DrawString("Số dự kiến chưa gồm dịch vụ/phụ thu; quyết toán khi trả phòng.",body,Brushes.Gray,x,y);y+=30;}
            g.DrawString($"Hình thức: {receipt.Method}  •  Ghi chú: {receipt.Note}",body,Brushes.Black,x,y);y+=48;
            g.DrawString("Thu ngân",bold,Brushes.Black,x+60,y);g.DrawString("Khách hàng",bold,Brushes.Black,x+width-175,y);
            g.DrawString("Phiếu cọc nội bộ, không phải hóa đơn GTGT.",body,Brushes.Gray,x,e.MarginBounds.Bottom-28);
        };
        using var preview=new PrintPreviewDialog {Document=document,Width=980,Height=780,StartPosition=FormStartPosition.CenterParent};
        preview.ShowDialog(this);
    }
}
