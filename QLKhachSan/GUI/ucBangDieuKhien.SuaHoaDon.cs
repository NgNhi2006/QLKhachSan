using System.Globalization;
using QLKhachSan.BLL;
using QLKhachSan.DTO;

namespace QLKhachSan.GUI;

public partial class ucDashboard
{
    private sealed class BillEditorRow(string category,long? orderId,bool amountEdited,int? serviceId=null)
    {
        public string Category {get;}=category;
        public long? OrderId {get;}=orderId;
        public int? ServiceId {get;}=serviceId;
        public bool AmountEdited {get;set;}=amountEdited;
    }

    private BillQuote? EditDraftBill(BillQuote bill,string method,string reference,IWin32Window? owner=null)
    {
        using var form=new Form {Text=$"Xem / sửa bill tạm tính · Phòng {bill.Room.Number}",
            Size=new Size(980,630),MinimumSize=new Size(760,530),StartPosition=FormStartPosition.CenterParent,
            Font=AppTheme.Body,BackColor=AppTheme.Canvas};
        var layout=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=1,RowCount=6,Padding=new Padding(18)};
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute,66));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute,44));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent,100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute,0));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute,78));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute,54));
        form.Controls.Add(layout);
        layout.Controls.Add(new Label {Text="BILL TẠM TÍNH  /  XEM VÀ SỬA\nChọn dịch vụ từ danh mục. Nhấp đôi vào số tiền để điều chỉnh giá.",
            Dock=DockStyle.Fill,Font=AppTheme.Bold,ForeColor=AppTheme.Navy},0,0);
        var toolbar=new FlowLayoutPanel {Dock=DockStyle.Fill,FlowDirection=FlowDirection.LeftToRight,
            WrapContents=false,BackColor=AppTheme.Canvas};
        var add=new Button {Text="＋ Thêm dịch vụ",Width=155,Height=34};AppTheme.Button(add,true);
        var remove=new Button {Text="Xóa dòng",Width=115,Height=34};AppTheme.Button(remove);
        toolbar.Controls.Add(add);toolbar.Controls.Add(remove);
        toolbar.Controls.Add(new Label {Text="Chọn dòng dịch vụ trong bảng để xóa",AutoSize=true,
            ForeColor=AppTheme.Muted,Margin=new Padding(12,9,0,0)});
        layout.Controls.Add(toolbar,0,1);
        var grid=Ui.Grid();grid.AutoGenerateColumns=false;grid.ReadOnly=false;
        grid.AllowUserToAddRows=false;grid.SelectionMode=DataGridViewSelectionMode.FullRowSelect;
        grid.MultiSelect=false;
        grid.BorderStyle=BorderStyle.None;grid.CellBorderStyle=DataGridViewCellBorderStyle.SingleHorizontal;
        grid.RowTemplate.Height=42;
        grid.Columns.Add(new DataGridViewTextBoxColumn {Name="Name",HeaderText="Hạng mục",FillWeight=210});
        grid.Columns.Add(new DataGridViewTextBoxColumn {Name="Quantity",HeaderText="Số lượng",FillWeight=70});
        grid.Columns.Add(new DataGridViewTextBoxColumn {Name="UnitPrice",HeaderText="Đơn giá (đ)",FillWeight=105,ReadOnly=true});
        grid.Columns.Add(new DataGridViewTextBoxColumn {Name="Total",HeaderText="Thành tiền (đ) · nhấp đôi để sửa",FillWeight=150,ReadOnly=true});
        layout.Controls.Add(grid,0,2);
        var original=bill.DraftLines??
            [new BillDraftLine("Tiền phòng",1,bill.RoomCharge,"Room"),
             ..bill.Lines.Where(x=>x.Cancelled is null).Select(x=>new BillDraftLine(x.Name,x.Quantity,x.Price,"Service",x.Id))];
        foreach(var line in original)
        {
            var index=grid.Rows.Add(line.Name,line.Category=="Room"?$"{BilledRoomDays(bill)} ngày":line.Quantity,
                line.UnitPrice.ToString("N0"),line.Total.ToString("N0"));
            grid.Rows[index].Tag=new BillEditorRow(line.Category,line.OrderId,line.AmountOverride is not null,line.ServiceId);
            grid.Rows[index].Cells["Name"].ReadOnly=line.Category!="Other";
            if(line.Category=="Room")
            {
                grid.Rows[index].Cells["Name"].ReadOnly=true;
                grid.Rows[index].Cells["Quantity"].ReadOnly=true;
            }
        }
        if(grid.Rows.Cast<DataGridViewRow>().FirstOrDefault(row=>row.Tag is BillEditorRow {Category:not "Room"}) is { } firstService)
            grid.CurrentCell=firstService.Cells["Name"];
        var appliedCode=bill.DiscountCode;
        var appliedAmount=bill.DiscountAmount;
        var info=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=2,RowCount=2};
        info.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,210));
        info.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
        info.RowStyles.Add(new RowStyle(SizeType.Absolute,44));
        info.RowStyles.Add(new RowStyle(SizeType.Absolute,34));
        info.Controls.Add(new Label {Text="Tạm tính sau chỉnh sửa",Dock=DockStyle.Fill,
            TextAlign=ContentAlignment.MiddleLeft,ForeColor=AppTheme.Muted},0,0);
        var total=new Label {Dock=DockStyle.Fill,TextAlign=ContentAlignment.MiddleRight,
            Font=AppTheme.Bold,ForeColor=AppTheme.Navy};info.Controls.Add(total,1,0);
        info.Controls.Add(new Label {Text="Lý do chỉnh sửa",Dock=DockStyle.Fill,
            TextAlign=ContentAlignment.MiddleLeft,ForeColor=AppTheme.Muted},0,1);
        var reason=Ui.Text(300);reason.Dock=DockStyle.Fill;reason.Text=bill.EditReason??"";
        reason.PlaceholderText="Tùy chọn · tự ghi khi thêm dịch vụ hoặc điều chỉnh giá";
        info.Controls.Add(reason,1,1);layout.Controls.Add(info,0,4);
        var actions=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=2};
        actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,45));
        actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,55));
        var print=new Button {Text="XEM / IN BILL",Dock=DockStyle.Fill,Margin=new Padding(0,0,6,0)};
        var save=new Button {Text="LƯU BILL TẠM TÍNH",Dock=DockStyle.Fill,Margin=new Padding(6,0,0,0)};
        AppTheme.Button(print);AppTheme.Button(save,true);
        actions.Controls.Add(print,0,0);
        actions.Controls.Add(save,1,0);layout.Controls.Add(actions,0,5);
        add.Click+=(_,_)=>
        {
            if(data.Menu.Count==0){Ui.Error(form,new BusinessException("Danh mục dịch vụ đang trống."));return;}
            using var picker=new Form {Text="Chọn dịch vụ",Size=new Size(560,510),MinimumSize=new Size(450,390),
                StartPosition=FormStartPosition.CenterParent,Font=AppTheme.Body,BackColor=Color.White};
            var panel=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=1,RowCount=5,Padding=new Padding(18)};
            foreach(var height in new[]{38,42,0,44,48})
                panel.RowStyles.Add(height==0?new RowStyle(SizeType.Percent,100):new RowStyle(SizeType.Absolute,height));
            picker.Controls.Add(panel);
            panel.Controls.Add(new Label {Text="DỊCH VỤ TRONG DANH MỤC",Dock=DockStyle.Fill,
                Font=AppTheme.Bold,ForeColor=AppTheme.Navy},0,0);
            var search=Ui.Text(100);search.Dock=DockStyle.Fill;search.PlaceholderText="Tìm tên hoặc nhóm dịch vụ";
            panel.Controls.Add(search,0,1);
            var list=new ListBox {Dock=DockStyle.Fill,Font=AppTheme.Body,IntegralHeight=false};panel.Controls.Add(list,0,2);
            void Filter()
            {
                list.BeginUpdate();list.Items.Clear();
                foreach(var item in data.Menu.Where(x=>(x.Name+" "+x.Category).Contains(search.Text.Trim(),StringComparison.OrdinalIgnoreCase)))
                    list.Items.Add(item);
                list.EndUpdate();
                if(list.Items.Count>0)list.SelectedIndex=0;
            }
            search.TextChanged+=(_,_)=>Filter();Filter();
            var quantityRow=new FlowLayoutPanel {Dock=DockStyle.Fill,WrapContents=false};
            quantityRow.Controls.Add(new Label {Text="Số lượng",Width=100,Height=34,TextAlign=ContentAlignment.MiddleLeft});
            var quantity=new NumericUpDown {Minimum=1,Maximum=100,Value=1,Width=105};quantityRow.Controls.Add(quantity);
            panel.Controls.Add(quantityRow,0,3);
            var choose=new Button {Text="THÊM VÀO BILL",Dock=DockStyle.Fill};AppTheme.Button(choose,true);
            panel.Controls.Add(choose,0,4);
            choose.Click+=(_,_)=>{if(list.SelectedItem is not ServiceItem)return;picker.DialogResult=DialogResult.OK;picker.Close();};
            list.DoubleClick+=(_,_)=>choose.PerformClick();
            if(picker.ShowDialog(form)!=DialogResult.OK || list.SelectedItem is not ServiceItem item)return;
            var index=grid.Rows.Add(item.Name,(int)quantity.Value,item.Price.ToString("N0"),(item.Price*quantity.Value).ToString("N0"));
            grid.Rows[index].Tag=new BillEditorRow("Service",null,false,item.Id);
            grid.Rows[index].Cells["Name"].ReadOnly=true;
            grid.CurrentCell=grid.Rows[index].Cells["Name"];
            InvalidateCode();UpdateTotal();UpdateRemove();
        };
        remove.Click+=(_,_)=>
        {
            if(grid.CurrentRow is not { } row)
            {Ui.Error(form,new BusinessException("Chọn dòng dịch vụ cần xóa trong bảng."));return;}
            if(row.Tag is BillEditorRow {Category:"Room"})
            {Ui.Error(form,new BusinessException("Chọn dòng dịch vụ cần xóa. Dòng tiền phòng chỉ có thể sửa thành tiền."));return;}
            grid.EndEdit();grid.Rows.Remove(row);InvalidateCode();UpdateTotal();UpdateRemove();
        };
        void UpdateRemove()
        {
            remove.Enabled=grid.Rows.Cast<DataGridViewRow>()
                .Any(row=>row.Tag is BillEditorRow {Category:not "Room"});
        }
        grid.CurrentCellChanged+=(_,_)=>UpdateRemove();UpdateRemove();
        void InvalidateCode()
        {
            appliedCode=null;appliedAmount=0;
        }
        void UpdateTotal()
        {
            decimal sum=0;
            foreach(DataGridViewRow row in grid.Rows)
            {
                var meta=(BillEditorRow)row.Tag!;
                var quantity=meta.Category=="Room"?1:
                    int.TryParse(Convert.ToString(row.Cells["Quantity"].Value),out var enteredQuantity)?enteredQuantity:-1;
                if(quantity<0 ||
                   !decimal.TryParse(Convert.ToString(row.Cells["UnitPrice"].Value),NumberStyles.Number,
                       CultureInfo.CurrentCulture,out var price))
                {total.Text="Kiểm tra số lượng hoặc đơn giá.";return;}
                var amount=quantity*price;
                if(meta.AmountEdited)
                {
                    if(!decimal.TryParse(Convert.ToString(row.Cells["Total"].Value),NumberStyles.Number,
                        CultureInfo.CurrentCulture,out var entered))
                    {total.Text="Kiểm tra thành tiền.";return;}
                    amount=entered;
                }
                else row.Cells["Total"].Value=amount.ToString("N0");
                sum+=amount;
            }
            var discount=appliedCode is not null?appliedAmount:0;
            var net=sum-discount-bill.Stay.Deposit;
            total.Text=$"Tạm tính {sum:N0} đ  ·  Giảm {discount:N0} đ  ·  Cọc {bill.Stay.Deposit:N0} đ\n{(net>=0?"CẦN THU":"CẦN HOÀN")}: {Math.Abs(net):N0} đ";
        }
        grid.CellEndEdit+=(_,e)=>
        {
            if(grid.Rows[e.RowIndex].Tag is BillEditorRow meta)
            {
                if(grid.Columns[e.ColumnIndex].Name=="Quantity")meta.AmountEdited=false;
            }
            InvalidateCode();
            UpdateTotal();
        };
        grid.CellDoubleClick+=(_,e)=>
        {
            if(e.RowIndex<0 || grid.Columns[e.ColumnIndex].Name is not ("UnitPrice" or "Total"))return;
            var row=grid.Rows[e.RowIndex];var meta=(BillEditorRow)row.Tag!;
            var quantity=meta.Category=="Room"?1:
                int.TryParse(Convert.ToString(row.Cells["Quantity"].Value),out var enteredQuantity)?enteredQuantity:-1;
            if(quantity<0)return;
            decimal.TryParse(Convert.ToString(row.Cells["UnitPrice"].Value),NumberStyles.Number,CultureInfo.CurrentCulture,out var currentPrice);
            decimal.TryParse(Convert.ToString(row.Cells["Total"].Value),NumberStyles.Number,CultureInfo.CurrentCulture,out var currentAmount);
            using var editor=new Form {Text=$"Điều chỉnh giá · {row.Cells["Name"].Value}",Size=new Size(420,300),
                StartPosition=FormStartPosition.CenterParent,Font=AppTheme.Body,BackColor=Color.White};
            var stack=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=1,RowCount=5,Padding=new Padding(22)};
            foreach(var height in new[]{30,42,30,42,50})stack.RowStyles.Add(new RowStyle(SizeType.Absolute,height));
            editor.Controls.Add(stack);
            stack.Controls.Add(new Label {Text="Đơn giá",Dock=DockStyle.Fill,ForeColor=AppTheme.Navy},0,0);
            var price=Ui.Money(currentPrice);price.Dock=DockStyle.Fill;
            price.Enabled=meta.ServiceId is null && meta.Category!="Room";
            stack.Controls.Add(price,0,1);
            stack.Controls.Add(new Label {Text=$"Thành tiền · số lượng {quantity}",Dock=DockStyle.Fill,ForeColor=AppTheme.Navy},0,2);
            var amount=Ui.Money(currentAmount);amount.Dock=DockStyle.Fill;stack.Controls.Add(amount,0,3);
            price.ValueChanged+=(_,_)=>amount.Value=Math.Min(amount.Maximum,price.Value*quantity);
            var accept=new Button {Text="LƯU GIÁ",Dock=DockStyle.Fill};AppTheme.Button(accept,true);
            stack.Controls.Add(accept,0,4);accept.Click+=(_,_)=>{editor.DialogResult=DialogResult.OK;editor.Close();};
            if(editor.ShowDialog(form)!=DialogResult.OK)return;
            row.Cells["UnitPrice"].Value=price.Value.ToString("N0");
            row.Cells["Total"].Value=amount.Value.ToString("N0");
            meta.AmountEdited=amount.Value!=price.Value*quantity;
            InvalidateCode();UpdateTotal();
        };
        UpdateTotal();
        BillQuote? result=null;
        BillQuote BuildCandidate()
        {
                grid.EndEdit();
                var lines=new List<BillDraftLine>();
                foreach(DataGridViewRow row in grid.Rows)
                {
                    var name=Convert.ToString(row.Cells["Name"].Value)?.Trim()??"";
                    if(name.Length is <2 or >150)throw new BusinessException("Tên hạng mục cần từ 2 đến 150 ký tự.");
                    var meta=(BillEditorRow)row.Tag!;
                    var quantity=meta.Category=="Room"?1:
                        int.TryParse(Convert.ToString(row.Cells["Quantity"].Value),out var enteredQuantity)?enteredQuantity:-1;
                    if(quantity is <0 or >100)
                        throw new BusinessException($"Số lượng của {name} phải từ 0 đến 100.");
                    if(!decimal.TryParse(Convert.ToString(row.Cells["UnitPrice"].Value),NumberStyles.Number,CultureInfo.CurrentCulture,out var price) || price<0 || price>1_000_000_000 || price!=decimal.Round(price,0))
                        throw new BusinessException($"Đơn giá của {name} không hợp lệ.");
                    if(meta.ServiceId is not null && quantity==0)
                        throw new BusinessException($"Dịch vụ {name} cần số lượng lớn hơn 0. Hãy xóa dòng nếu không dùng.");
                    decimal? amountOverride=null;
                    if(meta.AmountEdited)
                    {
                        if(!decimal.TryParse(Convert.ToString(row.Cells["Total"].Value),NumberStyles.Number,
                            CultureInfo.CurrentCulture,out var entered) || entered<0 || entered>1_000_000_000 || entered!=decimal.Round(entered,0))
                            throw new BusinessException($"Thành tiền của {name} không hợp lệ.");
                        amountOverride=entered;
                    }
                    if(quantity==0 && amountOverride is >0)
                        throw new BusinessException($"{name}: số lượng 0 phải có thành tiền 0.");
                    lines.Add(new BillDraftLine(name,quantity,price,meta.Category,meta.OrderId,amountOverride,meta.ServiceId));
                }
                var roomCharge=lines.Single(x=>x.Category=="Room").Total;
                var services=lines.Where(x=>x.Category!="Room").Sum(x=>x.Total);
                if(roomCharge>1_000_000_000 || services>1_000_000_000)
                    throw new BusinessException("Tổng tiền vượt giới hạn cho phép.");
                if(lines.SequenceEqual(original) && appliedCode==bill.DiscountCode && appliedAmount==bill.DiscountAmount)return bill;
                var editReason=reason.Text.Trim();
                if(editReason.Length==0)editReason=lines.Any(x=>x.ServiceId is not null)?
                    "Thêm dịch vụ từ danh mục":"Điều chỉnh bill tạm tính";
                if(editReason.Length<3)throw new BusinessException("Lý do chỉnh sửa bill không hợp lệ.");
                return bill with {RoomCharge=roomCharge,Services=services,DraftLines=lines,
                    BaseRoomCharge=bill.BaseRoomCharge??bill.RoomCharge,
                    BaseServices=bill.BaseServices??bill.Services,EditReason=editReason,
                    DiscountCode=appliedCode,DiscountAmount=appliedAmount};
        }
        save.Click+=(_,_)=>
        {
            try{result=BuildCandidate();form.DialogResult=DialogResult.OK;form.Close();}
            catch(Exception ex){Ui.Error(form,ex);}
        };
        print.Click+=async (_,_)=>
        {
            print.Enabled=false;
            try
            {
                var candidate=BuildCandidate();
                await ShowBillBeforePayment(new[]{candidate},method,reference,form);
                result=candidate;form.DialogResult=DialogResult.OK;form.Close();
            }
            catch(Exception ex){Ui.Error(form,ex);}
            finally{if(!form.IsDisposed)print.Enabled=true;}
        };
        form.ShowDialog(owner??this);
        return result;
    }
}
