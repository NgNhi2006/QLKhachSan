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
            Dock=DockStyle.Fill,Font=AppTheme.Bold,ForeColor=AppTheme.Ink,BackColor=Color.White,
            Padding=new Padding(17,12,0,0)},0,0);
        var list=new ListView {Dock=DockStyle.Fill,View=View.Details,CheckBoxes=true,FullRowSelect=true,
            GridLines=true,BackColor=Color.White,Margin=new Padding(0,10,0,10)};
        list.Columns.Add("Phòng",90);list.Columns.Add("Lượt",90);list.Columns.Add("Tiền phòng",130,HorizontalAlignment.Right);
        list.Columns.Add("Dịch vụ",120,HorizontalAlignment.Right);list.Columns.Add("Tổng",130,HorizontalAlignment.Right);
        list.Columns.Add("Cọc",120,HorizontalAlignment.Right);list.Columns.Add("Cần thu",120,HorizontalAlignment.Right);
        foreach(var q in quotes)
        {
            var item=new ListViewItem(q.Room.Number){Tag=q,Checked=true};
            item.SubItems.Add(q.Stay.Id.ToString());item.SubItems.Add($"{q.RoomCharge:N0}");item.SubItems.Add($"{q.Services:N0}");
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
        var paymentRow=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=2,RowCount=1};
        paymentRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,260));paymentRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
        var method=Ui.Combo(new[]{"Tiền mặt","Chuyển khoản","Thẻ POS"});method.Dock=DockStyle.Fill;
        var reference=Ui.Text(100);reference.PlaceholderText="Mã giao dịch chung (nếu có)";reference.Dock=DockStyle.Fill;
        paymentRow.Controls.Add(method,0,0);paymentRow.Controls.Add(reference,1,0);root.Controls.Add(paymentRow,0,3);
        var buttons=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=2,RowCount=1,Padding=new Padding(0,8,0,0)};
        buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,40));buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,60));
        var cancel=new Button {Text="QUAY LẠI",Dock=DockStyle.Fill,Margin=new Padding(0,0,7,0)};
        var pay=new Button {Text="XÁC NHẬN THU / HOÀN MỘT LẦN",Dock=DockStyle.Fill,Margin=new Padding(7,0,0,0)};
        AppTheme.Button(cancel);AppTheme.Button(pay,true);buttons.Controls.Add(cancel,0,0);buttons.Controls.Add(pay,1,0);root.Controls.Add(buttons,0,4);
        cancel.Click+=(_,_)=>form.Close();
        var completed=false;
        pay.Click+=async (_,_)=>
        {
            var chosen=Chosen();
            if(chosen.Count<2){Ui.Error(form,new BusinessException("Chọn ít nhất hai phòng của khách."));return;}
            if(!Ui.Confirm(form,$"Xác nhận đã thu ròng {chosen.Sum(x=>x.ToCollect-x.ToRefund):N0} đ cho {chosen.Count} phòng?"))return;
            pay.Enabled=false;
            try
            {
                GroupCheckoutResult? result=null;
                await Changed(async()=>result=await service.GroupCheckoutAsync(chosen,(string)method.SelectedItem!,reference.Text));
                completed=true;
                if(result is not null)
                {
                    if(MessageBox.Show(form,$"Đã lập phiếu thanh toán tổng #{result.GroupId} cho {chosen.Count} phòng. Xem bản in?",
                        "Hoàn tất",MessageBoxButtons.YesNo,MessageBoxIcon.Information)==DialogResult.Yes)
                        PrintGroupCheckout(result,chosen);
                }
                form.Close();
            }
            catch(Exception ex){Ui.Error(form,ex);}
            finally{if(!form.IsDisposed)pay.Enabled=true;}
        };
        form.ShowDialog(this);
        return completed;
    }

    private void PrintGroupCheckout(GroupCheckoutResult result,IReadOnlyList<BillQuote> quotes)
    {
        using var document=new System.Drawing.Printing.PrintDocument {DocumentName=$"Phiếu thanh toán tổng #{result.GroupId}"};
        document.PrintPage+=(_,e)=>
        {
            if(e.Graphics is null)return;
            using var title=new Font("Segoe UI Semibold",19,FontStyle.Bold);
            using var body=new Font("Segoe UI",10);
            using var bold=new Font("Segoe UI Semibold",10,FontStyle.Bold);
            var g=e.Graphics;var x=e.MarginBounds.Left;var y=e.MarginBounds.Top;var width=e.MarginBounds.Width;
            g.DrawString(AppSettings.Load().HotelName,title,Brushes.Black,x,y);y+=44;
            g.DrawString($"PHIẾU THANH TOÁN TỔNG  #{result.GroupId}",bold,Brushes.Black,x,y);y+=31;
            g.DrawString($"Khách hàng: {result.Guest}  •  Ngày lập: {DateTime.Now:dd/MM/yyyy HH:mm}",body,Brushes.Black,x,y);y+=37;
            g.DrawLine(Pens.Gray,x,y,x+width,y);y+=12;
            foreach(var q in quotes)
            {
                g.DrawString($"Phòng {q.Room.Number}  •  Lượt #{q.Stay.Id}",bold,Brushes.Black,x,y);
                g.DrawString($"{q.Total:N0} đ  (cọc {q.Stay.Deposit:N0} đ)",body,Brushes.Black,x+width-270,y);y+=30;
            }
            g.DrawLine(Pens.Gray,x,y,x+width,y);y+=16;
            g.DrawString($"TỔNG TIỀN: {result.Total:N0} đ",bold,Brushes.Black,x,y);y+=29;
            g.DrawString($"Cọc đã thu: {result.Deposits:N0} đ",body,Brushes.Black,x,y);y+=26;
            g.DrawString($"Thu thêm: {result.Collected:N0} đ  •  Hoàn cọc: {result.Refunded:N0} đ",bold,Brushes.Black,x,y);y+=40;
            g.DrawString("Thu ngân",bold,Brushes.Black,x+65,y);g.DrawString("Khách hàng",bold,Brushes.Black,x+width-170,y);
            g.DrawString("Phiếu thanh toán nội bộ, không phải hóa đơn GTGT.",body,Brushes.Gray,x,e.MarginBounds.Bottom-28);
        };
        using var preview=new PrintPreviewDialog {Document=document,Width=1000,Height=800,StartPosition=FormStartPosition.CenterParent};
        preview.ShowDialog(this);
    }
}
