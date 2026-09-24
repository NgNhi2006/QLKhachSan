using QLKhachSan.BLL;
using QLKhachSan.DTO;

namespace QLKhachSan.GUI;

public partial class ucDashboard
{
    private async Task ShowPricing()
    {
        using var form=new Form {Text="Bảng giá phòng và dịch vụ",Size=new Size(1160,740),MinimumSize=new Size(850,550),StartPosition=FormStartPosition.CenterParent,Font=AppTheme.Body,BackColor=AppTheme.Canvas};
        var layout=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=2,RowCount=1,Padding=new Padding(16)};
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,50));layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,50));
        form.Controls.Add(layout);
        var rooms=Ui.Grid();var services=Ui.Grid();
        var roomPrice=Ui.Money();var servicePrice=Ui.Money();
        TableLayoutPanel Side(string title,DataGridView grid,NumericUpDown price,string buttonText,EventHandler save)
        {
            var side=new TableLayoutPanel {Dock=DockStyle.Fill,BackColor=Color.White,Padding=new Padding(14),RowCount=5,ColumnCount=1,Margin=new Padding(6)};
            side.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
            side.RowStyles.Add(new RowStyle(SizeType.Absolute,42));side.RowStyles.Add(new RowStyle(SizeType.Percent,100));
            side.RowStyles.Add(new RowStyle(SizeType.Absolute,28));side.RowStyles.Add(new RowStyle(SizeType.Absolute,40));side.RowStyles.Add(new RowStyle(SizeType.Absolute,52));
            side.Controls.Add(new Label {Text=title,Dock=DockStyle.Fill,Font=AppTheme.Title,ForeColor=AppTheme.Ink},0,0);
            grid.Dock=DockStyle.Fill;side.Controls.Add(grid,0,1);
            side.Controls.Add(new Label {Text="Giá mới (đ)",Dock=DockStyle.Fill,Font=AppTheme.Bold,ForeColor=AppTheme.Muted},0,2);
            price.Dock=DockStyle.Fill;side.Controls.Add(price,0,3);
            var button=new Button {Text=buttonText,Dock=DockStyle.Fill};AppTheme.Button(button,true);button.Click+=save;side.Controls.Add(button,0,4);
            return side;
        }
        async Task LoadRooms()
        {
            await Reload();rooms.DataSource=data.Rooms.OrderBy(x=>x.Number).ToList();
            FormatGrid(rooms,new() {{"Number","Phòng"},{"Type","Loại"},{"Rate","Giá/ngày"},{"Deposit","Cọc"},{"Status","Trạng thái"}});
            HideColumns(rooms,"Id","Version");
        }
        async Task LoadServices()
        {
            services.DataSource=await service.CatalogAsync();
            FormatGrid(services,new() {{"Category","Nhóm"},{"Name","Dịch vụ"},{"Price","Đơn giá"},{"Unit","Đơn vị"},{"Active","Đang dùng"}});
            HideColumns(services,"Id","Version");
        }
        rooms.SelectionChanged+=(_,_)=>{if(rooms.CurrentRow?.DataBoundItem is Room room)roomPrice.Value=Math.Clamp(room.Rate,roomPrice.Minimum,roomPrice.Maximum);};
        services.SelectionChanged+=(_,_)=>{if(services.CurrentRow?.DataBoundItem is ServiceCatalogItem item)servicePrice.Value=Math.Clamp(item.Price,servicePrice.Minimum,servicePrice.Maximum);};
        layout.Controls.Add(Side("GIÁ PHÒNG",rooms,roomPrice,"LƯU GIÁ PHÒNG",async (_,_)=>
        {
            if(rooms.CurrentRow?.DataBoundItem is not Room room){Ui.Error(form,new BusinessException("Chọn phòng cần đổi giá."));return;}
            try{await service.SaveRoomAsync(room with {Rate=roomPrice.Value});await LoadRooms();}
            catch(Exception ex){Ui.Error(form,ex);}
        }),0,0);
        layout.Controls.Add(Side("GIÁ DỊCH VỤ",services,servicePrice,"LƯU GIÁ DỊCH VỤ",async (_,_)=>
        {
            if(services.CurrentRow?.DataBoundItem is not ServiceCatalogItem item){Ui.Error(form,new BusinessException("Chọn dịch vụ cần đổi giá."));return;}
            try{await service.SaveServiceAsync(item with {Price=servicePrice.Value});await LoadServices();await Reload();}
            catch(Exception ex){Ui.Error(form,ex);}
        }),1,0);
        await LoadRooms();await LoadServices();form.ShowDialog(this);
    }

    private async Task ShowRoomCatalog()
    {
        using var form=new Form {Text="Quản lý phòng",Size=new Size(1180,830),MinimumSize=new Size(960,700),StartPosition=FormStartPosition.CenterParent,Font=AppTheme.Body,BackColor=AppTheme.Canvas};
        var root=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=1,RowCount=2};
        root.RowStyles.Add(new RowStyle(SizeType.Absolute,78));root.RowStyles.Add(new RowStyle(SizeType.Percent,100));form.Controls.Add(root);
        var heading=new Panel {Dock=DockStyle.Fill,BackColor=Color.White,Padding=new Padding(24,12,24,8)};
        heading.Controls.Add(new Label {Text="Quản lý phòng",Dock=DockStyle.Top,Height=34,Font=AppTheme.Title,ForeColor=AppTheme.Ink});
        heading.Controls.Add(new Label {Text="Thêm phòng mới hoặc cập nhật nhiều phòng trong một lần",Dock=DockStyle.Bottom,Height=22,ForeColor=AppTheme.Muted});
        root.Controls.Add(heading,0,0);
        var body=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=2,RowCount=1,Padding=new Padding(16,14,16,16)};
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,62));body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,38));root.Controls.Add(body,0,1);

        var listCard=new TableLayoutPanel {Dock=DockStyle.Fill,BackColor=Color.White,Margin=new Padding(0,0,12,0),Padding=new Padding(16),ColumnCount=1,RowCount=4};
        listCard.RowStyles.Add(new RowStyle(SizeType.Absolute,36));listCard.RowStyles.Add(new RowStyle(SizeType.Percent,100));
        listCard.RowStyles.Add(new RowStyle(SizeType.Absolute,36));listCard.RowStyles.Add(new RowStyle(SizeType.Absolute,42));body.Controls.Add(listCard,0,0);
        var tabs=new TabControl {Dock=DockStyle.Fill,Font=AppTheme.Bold};
        foreach(var title in new[]{"Phòng đơn","Phòng đôi","Phòng VIP"})tabs.TabPages.Add(title);
        listCard.Controls.Add(tabs,0,0);
        var grid=Ui.Grid();grid.MultiSelect=true;grid.SelectionMode=DataGridViewSelectionMode.FullRowSelect;grid.Dock=DockStyle.Fill;
        listCard.Controls.Add(grid,0,1);
        var selection=new Label {Dock=DockStyle.Fill,ForeColor=AppTheme.Muted,TextAlign=ContentAlignment.MiddleLeft};listCard.Controls.Add(selection,0,2);
        var selectAll=new Button {Text="Chọn tất cả phòng trong mục",Dock=DockStyle.Fill};AppTheme.Button(selectAll);listCard.Controls.Add(selectAll,0,3);

        var editor=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=1,RowCount=2,Margin=Padding.Empty};
        editor.RowStyles.Add(new RowStyle(SizeType.Percent,51));editor.RowStyles.Add(new RowStyle(SizeType.Percent,49));body.Controls.Add(editor,1,0);
        TableLayoutPanel Card()
        {
            var card=new TableLayoutPanel {Dock=DockStyle.Fill,BackColor=Color.White,ColumnCount=1,AutoScroll=true,Padding=new Padding(16),Margin=new Padding(0,0,0,10)};
            card.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));return card;
        }
        void AddRow(TableLayoutPanel card,Control control,int height)
        {
            var row=card.RowCount++;card.RowStyles.Add(new RowStyle(SizeType.Absolute,height));
            control.Dock=DockStyle.Fill;control.Margin=new Padding(0,0,0,4);card.Controls.Add(control,0,row);
        }
        Label Caption(string text)=>new() {Text=text,ForeColor=AppTheme.Muted,Font=AppTheme.Bold,TextAlign=ContentAlignment.BottomLeft};
        var addCard=Card();editor.Controls.Add(addCard,0,0);
        AddRow(addCard,new Label {Text="THÊM PHÒNG MỚI",Font=AppTheme.Bold,ForeColor=AppTheme.Ink},32);
        var number=Ui.Text(10);number.PlaceholderText="Ví dụ: 411";
        var newType=Ui.Combo(new[]{"Đơn","Đôi","VIP"});var newRate=Ui.Money();var newDeposit=Ui.Money();
        AddRow(addCard,Caption("Số phòng"),24);AddRow(addCard,number,34);
        AddRow(addCard,Caption("Loại phòng"),24);AddRow(addCard,newType,34);
        AddRow(addCard,Caption("Giá mỗi ngày (đ)"),24);AddRow(addCard,newRate,34);
        AddRow(addCard,Caption("Cọc gợi ý (đ)"),24);AddRow(addCard,newDeposit,34);
        var addButton=new Button {Text="+  THÊM PHÒNG",Height=40};AppTheme.Button(addButton,true);AddRow(addCard,addButton,44);

        var bulkCard=Card();bulkCard.Margin=new Padding(0);editor.Controls.Add(bulkCard,0,1);
        AddRow(bulkCard,new Label {Text="CHỈNH CÁC PHÒNG ĐÃ CHỌN",Font=AppTheme.Bold,ForeColor=AppTheme.Ink},32);
        AddRow(bulkCard,new Label {Text="Tích thông tin cần đổi; ô không tích sẽ giữ nguyên.",ForeColor=AppTheme.Muted},28);
        var changeRate=new CheckBox {Text="Đổi giá mỗi ngày",AutoSize=true};var bulkRate=Ui.Money();
        var changeDeposit=new CheckBox {Text="Đổi cọc gợi ý",AutoSize=true};var bulkDeposit=Ui.Money();
        var changeType=new CheckBox {Text="Đổi loại phòng",AutoSize=true};var bulkType=Ui.Combo(new[]{"Đơn","Đôi","VIP"});
        bulkRate.Enabled=false;bulkDeposit.Enabled=false;bulkType.Enabled=false;
        changeRate.CheckedChanged+=(_,_)=>bulkRate.Enabled=changeRate.Checked;
        changeDeposit.CheckedChanged+=(_,_)=>bulkDeposit.Enabled=changeDeposit.Checked;
        changeType.CheckedChanged+=(_,_)=>bulkType.Enabled=changeType.Checked;
        AddRow(bulkCard,changeRate,26);AddRow(bulkCard,bulkRate,34);
        AddRow(bulkCard,changeDeposit,26);AddRow(bulkCard,bulkDeposit,34);
        AddRow(bulkCard,changeType,26);AddRow(bulkCard,bulkType,34);
        var applyButton=new Button {Text="ÁP DỤNG CHO PHÒNG ĐÃ CHỌN"};AppTheme.Button(applyButton,true);AddRow(bulkCard,applyButton,44);

        List<Room> SelectedRooms()=>grid.SelectedRows.Cast<DataGridViewRow>().Select(r=>r.DataBoundItem).OfType<Room>().ToList();
        void UpdateSelection(){selection.Text=$"{SelectedRooms().Count} phòng đã chọn  •  Giữ Ctrl hoặc Shift để chọn nhiều phòng";}
        grid.SelectionChanged+=(_,_)=>UpdateSelection();selectAll.Click+=(_,_)=>{grid.SelectAll();UpdateSelection();};
        async Task LoadRows()
        {
            await Reload();
            var group=tabs.SelectedIndex switch {1=>"Đôi",2=>"VIP",_=>"Đơn"};
            grid.DataSource=data.Rooms.Where(r=>r.Type==group).OrderBy(r=>r.Number).ToList();
            FormatGrid(grid,new() {{"Number","Số phòng"},{"Type","Loại"},{"Rate","Giá/ngày"},{"Deposit","Cọc gợi ý"},{"Status","Trạng thái"}});
            HideColumns(grid,"Id","Version");grid.ClearSelection();UpdateSelection();
        }
        void SuggestNewRoom()
        {
            var group=tabs.SelectedIndex switch {1=>"Đôi",2=>"VIP",_=>"Đơn"};
            var groupRooms=data.Rooms.Where(r=>r.Type==group).ToList();
            var start=tabs.SelectedIndex switch {1=>200,2=>300,_=>100};
            number.Text=(Math.Max(start,groupRooms.Select(r=>int.TryParse(r.Number,out var value)?value:start).DefaultIfEmpty(start).Max())+1).ToString();
            if(groupRooms.FirstOrDefault() is { } sample)
            {
                newRate.Value=Math.Clamp(sample.Rate,newRate.Minimum,newRate.Maximum);
                newDeposit.Value=Math.Clamp(sample.Deposit,newDeposit.Minimum,newDeposit.Maximum);
            }
        }
        tabs.SelectedIndexChanged+=async (_,_)=>{newType.SelectedItem=tabs.SelectedIndex switch {1=>"Đôi",2=>"VIP",_=>"Đơn"};await LoadRows();SuggestNewRoom();};
        addButton.Click+=async (_,_)=>
        {
            addButton.Enabled=false;
            try
            {
                var room=new Room(0,number.Text,(string)newType.SelectedItem!,newRate.Value,newDeposit.Value,RoomStatus.Trong,0);
                await service.SaveRoomAsync(room);
                var targetTab=Array.IndexOf(new[]{"Đơn","Đôi","VIP"},room.Type);
                if(tabs.SelectedIndex==targetTab){await LoadRows();SuggestNewRoom();}
                else tabs.SelectedIndex=targetTab;
                number.Focus();
            }
            catch(Exception ex){Ui.Error(form,ex);}
            finally{addButton.Enabled=true;}
        };
        applyButton.Click+=async (_,_)=>
        {
            var selected=SelectedRooms();
            if(selected.Count==0){Ui.Error(form,new BusinessException("Chọn ít nhất một phòng trong danh sách."));return;}
            if(!changeRate.Checked && !changeDeposit.Checked && !changeType.Checked){Ui.Error(form,new BusinessException("Tích ít nhất một thông tin cần thay đổi."));return;}
            if(!Ui.Confirm(form,$"Áp dụng thay đổi cho {selected.Count} phòng đã chọn?"))return;
            applyButton.Enabled=false;
            try
            {
                await service.UpdateRoomsAsync(selected,changeRate.Checked?bulkRate.Value:null,changeDeposit.Checked?bulkDeposit.Value:null,changeType.Checked?(string)bulkType.SelectedItem!:null);
                await LoadRows();changeRate.Checked=false;changeDeposit.Checked=false;changeType.Checked=false;
            }
            catch(Exception ex){Ui.Error(form,ex);}
            finally{applyButton.Enabled=true;}
        };
        tabs.SelectedIndex=0;newType.SelectedItem="Đơn";await LoadRows();SuggestNewRoom();form.ShowDialog(this);
    }

    private async Task ShowServiceCatalog()
    {
        using var dialog=new InputDialog("Quản lý danh mục dịch vụ",850,750);
        var grid=Ui.Grid();grid.Height=250;
        var category=Ui.Text(100);var name=Ui.Text(150);var unit=Ui.Text(20);
        var price=Ui.Money();var active=new CheckBox {Text="Đang cung cấp",Checked=true,AutoSize=true};
        dialog.Add("Danh sách dịch vụ",grid);dialog.Add("Danh mục",category);
        dialog.Add("Tên dịch vụ",name);dialog.Add("Đơn vị",unit);
        dialog.Add("Đơn giá",price);dialog.Add("Trạng thái",active);
        ServiceCatalogItem? selected=null;
        void Fill(ServiceCatalogItem? item)
        {
            selected=item;category.Text=item?.Category??"";name.Text=item?.Name??"";
            unit.Text=item?.Unit??"";price.Value=Math.Clamp(item?.Price??0,price.Minimum,price.Maximum);
            active.Checked=item?.Active??true;
        }
        grid.SelectionChanged+=(_,_)=>Fill(grid.CurrentRow?.DataBoundItem as ServiceCatalogItem);
        async Task LoadRows()
        {
            grid.DataSource=await service.CatalogAsync();
            FormatGrid(grid,new() {{"Category","Danh mục"},{"Name","Dịch vụ"},{"Price","Đơn giá"},{"Unit","Đơn vị"},{"Active","Đang dùng"}});
            HideColumns(grid,"Id","Version");
        }
        dialog.Action("THÊM DỊCH VỤ",()=>{grid.ClearSelection();Fill(null);return Task.CompletedTask;},false);
        dialog.Action("LƯU DỊCH VỤ",async()=>
        {
            var item=new ServiceCatalogItem(selected?.Id??0,category.Text,name.Text,price.Value,unit.Text,active.Checked,selected?.Version??0);
            await service.SaveServiceAsync(item);await LoadRows();await Reload();
        },false);
        await LoadRows();dialog.ShowDialog(this);
    }
}
