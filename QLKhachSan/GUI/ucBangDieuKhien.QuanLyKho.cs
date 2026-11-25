using QLKhachSan.BLL;
using QLKhachSan.DTO;

namespace QLKhachSan.GUI;

public partial class ucDashboard
{
    private async Task ShowServiceInventory()
    {
        using var dialog=new Form {Text="Tồn kho dịch vụ",Size=new Size(1040,700),MinimumSize=new Size(790,520),
            StartPosition=FormStartPosition.CenterParent,Font=AppTheme.Body,BackColor=AppTheme.Canvas,
            AutoScaleMode=AutoScaleMode.Dpi};
        var root=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=1,RowCount=3,Padding=new Padding(20)};
        root.RowStyles.Add(new RowStyle(SizeType.Absolute,96));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute,58));
        root.RowStyles.Add(new RowStyle(SizeType.Percent,100));
        dialog.Controls.Add(root);
        var header=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=3,RowCount=2};
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,190));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,140));
        header.RowStyles.Add(new RowStyle(SizeType.Absolute,43));
        header.RowStyles.Add(new RowStyle(SizeType.Absolute,42));
        header.Controls.Add(new Label {Text="Tồn kho dịch vụ",Font=AppTheme.Title,ForeColor=AppTheme.Ink,Dock=DockStyle.Fill},0,0);
        var summary=new Label {Dock=DockStyle.Fill,ForeColor=AppTheme.Muted,Font=AppTheme.Body};
        header.Controls.Add(summary,0,1);
        var refresh=new Button {Text="LÀM MỚI",Dock=DockStyle.Fill};AppTheme.Button(refresh);
        header.Controls.Add(refresh,2,0);
        var returns=new Button {Text="NHẬN LẠI XE",Dock=DockStyle.Fill};AppTheme.Button(returns,true);
        returns.Visible=FunctionPolicy.Can(user,"service.manage") || FunctionPolicy.Can(user,"service.order");
        header.Controls.Add(returns,1,0);
        root.Controls.Add(header,0,0);
        var filters=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=2,Padding=new Padding(0,8,0,8)};
        filters.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,70));
        filters.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,30));
        var search=Ui.Text();search.PlaceholderText="Tìm tên dịch vụ hoặc mặt hàng...";search.Dock=DockStyle.Fill;
        var status=Ui.Combo(new[]{"Tất cả","Sắp hết","Hết hàng","Còn hàng","Chưa theo dõi kho"});status.Dock=DockStyle.Fill;
        filters.Controls.Add(search,0,0);filters.Controls.Add(status,1,0);root.Controls.Add(filters,0,1);
        var grid=new DataGridView {Dock=DockStyle.Fill,ReadOnly=true,AllowUserToAddRows=false,AllowUserToDeleteRows=false,
            AllowUserToResizeRows=false,RowHeadersVisible=false,SelectionMode=DataGridViewSelectionMode.FullRowSelect,
            MultiSelect=false,AutoGenerateColumns=false,BackgroundColor=Color.White,BorderStyle=BorderStyle.None,
            ColumnHeadersHeight=44,RowTemplate={Height=43}};
        grid.Columns.Add(new DataGridViewTextBoxColumn {HeaderText="DỊCH VỤ",DataPropertyName="ServiceName",AutoSizeMode=DataGridViewAutoSizeColumnMode.Fill,FillWeight=170});
        grid.Columns.Add(new DataGridViewTextBoxColumn {HeaderText="DANH MỤC",DataPropertyName="Category",Width=125});
        grid.Columns.Add(new DataGridViewTextBoxColumn {HeaderText="TRONG KHO",DataPropertyName="OnHandText",Width=115});
        grid.Columns.Add(new DataGridViewTextBoxColumn {HeaderText="ĐÃ GIỮ",DataPropertyName="ReservedText",Width=105});
        grid.Columns.Add(new DataGridViewTextBoxColumn {HeaderText="CÒN GỌI",DataPropertyName="AvailableText",Width=115});
        grid.Columns.Add(new DataGridViewTextBoxColumn {HeaderText="TRẠNG THÁI",DataPropertyName="Status",Width=155});
        grid.DefaultCellStyle.Font=AppTheme.Body;grid.ColumnHeadersDefaultCellStyle.Font=AppTheme.Bold;
        root.Controls.Add(grid,0,2);
        List<ServiceStockAvailability> rows=[];
        void Render()
        {
            var query=search.Text.Trim();
            var visible=rows.Where(x=>(query.Length==0 || x.ServiceName.Contains(query,StringComparison.CurrentCultureIgnoreCase) ||
                    (x.StockName?.Contains(query,StringComparison.CurrentCultureIgnoreCase)??false)) &&
                    (status.SelectedIndex==0 || x.Status==(string?)status.SelectedItem)).ToList();
            grid.DataSource=visible.Select(x=>new InventoryRow(x)).ToList();
            foreach(DataGridViewRow row in grid.Rows)
            {
                if(row.DataBoundItem is not InventoryRow item)continue;
                row.DefaultCellStyle.BackColor=item.Status switch
                {
                    "Hết hàng"=>Color.FromArgb(255,235,232),
                    "Sắp hết"=>Color.FromArgb(255,246,222),
                    "Còn hàng"=>Color.FromArgb(231,246,236),
                    _=>Color.White
                };
                row.DefaultCellStyle.ForeColor=AppTheme.Ink;
            }
            summary.Text=$"{rows.Count(x=>x.Status=="Còn hàng")} còn hàng   •   {rows.Count(x=>x.Status=="Sắp hết")} sắp hết   •   {rows.Count(x=>x.Status=="Hết hàng")} hết hàng   •   {rows.Count(x=>x.Status=="Chưa theo dõi kho")} chưa liên kết kho";
        }
        async Task Reload()
        {
            rows=await service.ServiceStockAvailabilityAsync();Render();
        }
        search.TextChanged+=(_,_)=>Render();status.SelectedIndexChanged+=(_,_)=>Render();
        refresh.Click+=async (_,_)=>{try{await Reload();}catch(Exception ex){Ui.Error(dialog,ex);}};
        returns.Click+=async (_,_)=>
        {
            try{await ShowRentalReturns(dialog);await Reload();}
            catch(Exception ex){Ui.Error(dialog,ex);}
        };
        await Reload();dialog.ShowDialog(this);
    }
    private async Task ShowRentalReturns(Form owner)
    {
        var rentals=await service.RentalOutAsync();
        if(rentals.Count==0)
        {MessageBox.Show(owner,"Hiện không có xe đang thuê chưa nhận lại.","Xe đang thuê");return;}
        using var dialog=new InputDialog("Nhận lại xe cho thuê",640,440);
        var choice=Ui.Combo(rentals.Select(x=>new RentalChoice(x)).ToArray());
        var quantity=Ui.Number(rentals.Max(x=>x.Outstanding));
        void UpdateQuantity()
        {
            if(choice.SelectedItem is not RentalChoice current)return;
            quantity.Maximum=current.Item.Outstanding;
            quantity.Value=current.Item.Outstanding;
        }
        choice.SelectedIndexChanged+=(_,_)=>UpdateQuantity();UpdateQuantity();
        dialog.Add("Xe đang thuê / phòng / khách",choice);
        dialog.Add("Số xe đã nhận lại",quantity);
        dialog.Note("Nhận lại xe sẽ tăng số xe còn trống. Tổng số xe trong kho không thay đổi.");
        dialog.Action("XÁC NHẬN NHẬN LẠI",async()=>
        {
            if(choice.SelectedItem is RentalChoice current)
                await service.ReturnRentalAsync(current.Item.OrderId,(int)quantity.Value);
        });
        dialog.ShowDialog(owner);
    }
    private sealed record RentalChoice(RentalOut Item)
    {
        public override string ToString()=>
            $"P.{Item.Room}  •  {Item.Guest}  •  {Item.Service}  •  còn {Item.Outstanding} xe chưa trả";
    }
    private sealed record InventoryRow(ServiceStockAvailability Item)
    {
        public string ServiceName=>Item.ServiceName;
        public string Category=>Item.Category;
        public string OnHandText=>Item.OnHand is decimal amount?$"{amount:N0} {Item.Unit}":"—";
        public string ReservedText=>Item.OnHand is null?"—":$"{Item.Reserved:N0}";
        public string AvailableText=>Item.Available is decimal amount?$"{Math.Max(0,amount):N0}":"—";
        public string Status=>Item.Status;
    }
}
