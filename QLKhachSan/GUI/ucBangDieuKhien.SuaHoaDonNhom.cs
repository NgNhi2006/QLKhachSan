using System.Globalization;
using QLKhachSan.BLL;
using QLKhachSan.DTO;

namespace QLKhachSan.GUI;

public partial class ucDashboard
{
    private sealed record GroupBillRow(long StayId,BillDraftLine Line);

    private Dictionary<long,BillQuote>? EditGroupDraftBills(IReadOnlyList<BillQuote> quotes,IWin32Window owner)
    {
        using var form=new Form {Text="Sửa bill tạm tính nhiều phòng",Size=new Size(1120,740),
            MinimumSize=new Size(850,580),StartPosition=FormStartPosition.CenterParent,
            Font=AppTheme.Body,BackColor=AppTheme.Canvas};
        var root=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=1,RowCount=5,Padding=new Padding(18)};
        root.RowStyles.Add(new RowStyle(SizeType.Absolute,66));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute,48));
        root.RowStyles.Add(new RowStyle(SizeType.Percent,100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute,88));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute,52));
        form.Controls.Add(root);
        root.Controls.Add(new Label {Text="BILL TẠM TÍNH CÁC PHÒNG\nSửa số lượng hoặc thành tiền ngay trên từng dòng; mọi thay đổi được lưu cùng lúc.",
            Dock=DockStyle.Fill,Font=AppTheme.Bold,ForeColor=AppTheme.Navy},0,0);

        var toolbar=new FlowLayoutPanel {Dock=DockStyle.Fill,WrapContents=false};
        var roomBox=Ui.Combo(quotes.Select(x=>x.Room.Number).ToArray());roomBox.Width=115;roomBox.Dock=DockStyle.None;
        var catalog=data.Menu.Cast<ServiceItem>().ToArray();
        var serviceBox=new ComboBox {Width=260,DropDownStyle=ComboBoxStyle.DropDownList,Font=AppTheme.Body};
        foreach(var item in catalog)serviceBox.Items.Add(item);
        serviceBox.DisplayMember="Name";
        if(serviceBox.Items.Count>0)serviceBox.SelectedIndex=0;
        var add=new Button {Text="THÊM DỊCH VỤ",Width=165,Height=34};AppTheme.Button(add);
        var remove=new Button {Text="XÓA DÒNG",Width=130,Height=34};AppTheme.Button(remove);
        toolbar.Controls.Add(roomBox);toolbar.Controls.Add(serviceBox);
        toolbar.Controls.Add(add);toolbar.Controls.Add(remove);root.Controls.Add(toolbar,0,1);

        var grid=Ui.Grid();grid.AutoGenerateColumns=false;grid.ReadOnly=false;
        grid.AllowUserToAddRows=false;grid.SelectionMode=DataGridViewSelectionMode.FullRowSelect;
        grid.RowTemplate.Height=34;
        grid.Columns.Add(new DataGridViewTextBoxColumn {Name="Room",HeaderText="Phòng",FillWeight=65,ReadOnly=true});
        grid.Columns.Add(new DataGridViewTextBoxColumn {Name="Name",HeaderText="Hạng mục / dịch vụ",FillWeight=210,ReadOnly=true});
        grid.Columns.Add(new DataGridViewTextBoxColumn {Name="Quantity",HeaderText="SL",FillWeight=55});
        grid.Columns.Add(new DataGridViewTextBoxColumn {Name="Price",HeaderText="Đơn giá",FillWeight=100,ReadOnly=true});
        grid.Columns.Add(new DataGridViewTextBoxColumn {Name="Amount",HeaderText="Thành tiền · có thể sửa",FillWeight=140});
        root.Controls.Add(grid,0,2);
        var originals=new Dictionary<long,List<BillDraftLine>>();
        foreach(var quote in quotes)
        {
            var lines=quote.DraftLines??
                [new BillDraftLine("Tiền phòng",1,quote.RoomCharge,"Room"),
                 ..quote.Lines.Where(x=>x.Cancelled is null).Select(x=>
                     new BillDraftLine(x.Name,x.Quantity,x.Price,"Service",x.Id,
                         x.Total==x.Quantity*x.Price?null:x.Total))];
            originals.Add(quote.Stay.Id,lines);
            foreach(var line in lines)AddRow(quote,line);
        }
        var total=new Label {Dock=DockStyle.Fill,Font=AppTheme.Bold,ForeColor=AppTheme.Navy,
            TextAlign=ContentAlignment.MiddleRight};
        void AddRow(BillQuote quote,BillDraftLine line)
        {
            var index=grid.Rows.Add(quote.Room.Number,line.Name,
                line.Category=="Room"?$"{BilledRoomDays(quote)} ngày":line.Quantity,
                line.UnitPrice.ToString("N0"),line.Total.ToString("N0"));
            var row=grid.Rows[index];row.Tag=new GroupBillRow(quote.Stay.Id,line);
            row.Cells["Quantity"].ReadOnly=line.Category=="Room";
            row.DefaultCellStyle.BackColor=index%2==0?Color.White:Color.FromArgb(249,248,244);
        }
        add.Click+=(_,_)=>
        {
            if(roomBox.SelectedItem is not string room || serviceBox.SelectedItem is not ServiceItem service)
            {Ui.Error(form,new BusinessException("Chọn phòng và dịch vụ cần thêm."));return;}
            var quote=quotes.Single(x=>x.Room.Number==room);
            AddRow(quote,new BillDraftLine(service.Name,1,service.Price,"Service",null,null,service.Id));
            RefreshTotals();
        };
        remove.Click+=(_,_)=>
        {
            if(grid.SelectedRows.Count==0)return;
            var row=grid.SelectedRows[0];
            if(((GroupBillRow)row.Tag!).Line.Category=="Room")
            {Ui.Error(form,new BusinessException("Không thể xóa dòng tiền phòng."));return;}
            grid.Rows.Remove(row);RefreshTotals();
        };
        grid.CellEndEdit+=(_,e)=>
        {
            if(e.RowIndex<0)return;
            var row=grid.Rows[e.RowIndex];
            if(grid.Columns[e.ColumnIndex].Name=="Quantity" &&
                int.TryParse(Convert.ToString(row.Cells["Quantity"].Value),out var quantity))
            {
                var price=((GroupBillRow)row.Tag!).Line.UnitPrice;
                row.Cells["Amount"].Value=(price*quantity).ToString("N0");
            }
            RefreshTotals();
        };
        var info=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=2,RowCount=2};
        info.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,180));
        info.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
        info.RowStyles.Add(new RowStyle(SizeType.Absolute,38));
        info.RowStyles.Add(new RowStyle(SizeType.Absolute,42));
        info.Controls.Add(new Label {Text="Tổng sau chỉnh sửa",Dock=DockStyle.Fill,
            TextAlign=ContentAlignment.MiddleLeft,ForeColor=AppTheme.Muted},0,0);
        info.Controls.Add(total,1,0);
        info.Controls.Add(new Label {Text="Lý do chỉnh sửa",Dock=DockStyle.Fill,
            TextAlign=ContentAlignment.MiddleLeft,ForeColor=AppTheme.Muted},0,1);
        var reason=Ui.Text(300);reason.Dock=DockStyle.Fill;
        reason.PlaceholderText="Ví dụ: điều chỉnh dịch vụ cho các phòng";
        info.Controls.Add(reason,1,1);root.Controls.Add(info,0,3);
        void RefreshTotals()
        {
            decimal sum=0;
            foreach(DataGridViewRow row in grid.Rows)
                if(decimal.TryParse(Convert.ToString(row.Cells["Amount"].Value),NumberStyles.Number,
                    CultureInfo.CurrentCulture,out var value))sum+=value;
            total.Text=$"{quotes.Count} phòng  •  Tổng bill {sum:N0} đ  •  Cọc {quotes.Sum(x=>x.Stay.Deposit):N0} đ  •  Thu ròng {sum-quotes.Sum(x=>x.Stay.Deposit):N0} đ";
        }
        RefreshTotals();
        var actions=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=2};
        actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,50));
        actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,50));
        var cancel=new Button {Text="QUAY LẠI",Dock=DockStyle.Fill,Margin=new Padding(0,0,6,0)};
        var save=new Button {Text="LƯU BILL TẤT CẢ PHÒNG",Dock=DockStyle.Fill,Margin=new Padding(6,0,0,0)};
        AppTheme.Button(cancel);AppTheme.Button(save,true);
        actions.Controls.Add(cancel,0,0);actions.Controls.Add(save,1,0);root.Controls.Add(actions,0,4);
        cancel.Click+=(_,_)=>form.Close();
        Dictionary<long,BillQuote>? result=null;
        save.Click+=(_,_)=>
        {
            try
            {
                grid.EndEdit();
                var draftByStay=quotes.ToDictionary(x=>x.Stay.Id,_=>new List<BillDraftLine>());
                foreach(DataGridViewRow row in grid.Rows)
                {
                    var meta=(GroupBillRow)row.Tag!;
                    var source=meta.Line;
                    var quantity=source.Category=="Room"?1:
                        int.TryParse(Convert.ToString(row.Cells["Quantity"].Value),out var enteredQuantity)?enteredQuantity:-1;
                    if(quantity is <0 or >100 || (source.ServiceId is not null && quantity==0))
                        throw new BusinessException($"Số lượng {source.Name} không hợp lệ.");
                    if(!decimal.TryParse(Convert.ToString(row.Cells["Amount"].Value),NumberStyles.Number,
                        CultureInfo.CurrentCulture,out var amount) || amount<0 || amount>1_000_000_000 ||
                       amount!=decimal.Round(amount,0) || (quantity==0 && amount!=0))
                        throw new BusinessException($"Thành tiền {source.Name} không hợp lệ.");
                    var overrideAmount=amount==quantity*source.UnitPrice?null:(decimal?)amount;
                    draftByStay[meta.StayId].Add(source with {Quantity=quantity,AmountOverride=overrideAmount});
                }
                var edited=new Dictionary<long,BillQuote>();
                foreach(var quote in quotes)
                {
                    var lines=draftByStay[quote.Stay.Id];
                    if(lines.Count(x=>x.Category=="Room")!=1 || lines.Count>100)
                        throw new BusinessException($"Chi tiết bill phòng {quote.Room.Number} không hợp lệ.");
                    if(lines.SequenceEqual(originals[quote.Stay.Id]))
                    {edited.Add(quote.Stay.Id,quote);continue;}
                    var editReason=reason.Text.Trim();
                    if(editReason.Length==0)editReason="Điều chỉnh bill nhiều phòng";
                    if(editReason.Length is <3 or >300)
                        throw new BusinessException("Lý do chỉnh sửa cần từ 3 đến 300 ký tự.");
                    var roomCharge=lines.Single(x=>x.Category=="Room").Total;
                    var services=lines.Where(x=>x.Category!="Room").Sum(x=>x.Total);
                    if(roomCharge>1_000_000_000 || services>1_000_000_000)
                        throw new BusinessException($"Tổng bill phòng {quote.Room.Number} vượt giới hạn.");
                    edited.Add(quote.Stay.Id,quote with {RoomCharge=roomCharge,Services=services,
                        DraftLines=lines,BaseRoomCharge=quote.BaseRoomCharge??quote.RoomCharge,
                        BaseServices=quote.BaseServices??quote.Services,EditReason=editReason,
                        DiscountCode=null,DiscountAmount=0});
                }
                result=edited;form.DialogResult=DialogResult.OK;form.Close();
            }
            catch(Exception ex){Ui.Error(form,ex);}
        };
        form.ShowDialog(owner);
        return result;
    }
}

