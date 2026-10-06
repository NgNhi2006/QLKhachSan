using QLKhachSan.BLL;
using QLKhachSan.DTO;

namespace QLKhachSan.GUI;

public partial class ucDashboard
{
    private async Task ShowMenuConfiguration()
    {
        if(user.Role!="Admin")throw new BusinessException("Chỉ quản trị viên được tùy chỉnh menu.");
        using var form=new Form {Text="Tùy chỉnh menu và chức năng",Size=new Size(1100,740),MinimumSize=new Size(850,600),
            StartPosition=FormStartPosition.CenterParent,Font=AppTheme.Body,BackColor=AppTheme.Canvas};
        var root=new TableLayoutPanel {Dock=DockStyle.Fill,RowCount=3,ColumnCount=1};
        root.RowStyles.Add(new RowStyle(SizeType.Absolute,80));root.RowStyles.Add(new RowStyle(SizeType.Percent,100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute,54));form.Controls.Add(root);
        var title=new Label {Text="Cấu hình menu làm việc",Dock=DockStyle.Fill,Padding=new Padding(24,16,0,0),
            Font=AppTheme.Title,ForeColor=AppTheme.Ink,BackColor=Color.White};root.Controls.Add(title,0,0);
        var tabs=new TabControl {Dock=DockStyle.Fill,Margin=new Padding(18,12,18,8)};root.Controls.Add(tabs,0,1);
        var note=new Label {Text="Chức năng mới sử dụng nghiệp vụ đã có. Việc tạo menu không tự sinh mã xử lý nghiệp vụ mới.",
            Dock=DockStyle.Fill,Padding=new Padding(24,9,0,0),ForeColor=AppTheme.Muted};root.Controls.Add(note,0,2);
        DataGridView Page(string name,out Button add,out Button edit,out Button remove)
        {
            var page=new TabPage(name){BackColor=Color.White,Padding=new Padding(14)};tabs.TabPages.Add(page);
            var layout=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=1,RowCount=2};
            layout.RowStyles.Add(new RowStyle(SizeType.Percent,100));layout.RowStyles.Add(new RowStyle(SizeType.Absolute,52));page.Controls.Add(layout);
            var grid=Ui.Grid();grid.Dock=DockStyle.Fill;grid.SelectionMode=DataGridViewSelectionMode.FullRowSelect;
            grid.MultiSelect=false;grid.AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill;layout.Controls.Add(grid,0,0);
            var actions=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=3,RowCount=1,Padding=new Padding(0,9,0,0)};
            for(var i=0;i<3;i++)actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100f/3));
            add=new Button {Text="+  THÊM MỚI",Dock=DockStyle.Fill,Margin=new Padding(0,0,6,0)};
            edit=new Button {Text="SỬA MỤC ĐÃ CHỌN",Dock=DockStyle.Fill,Margin=new Padding(6,0,6,0)};
            remove=new Button {Text="XÓA MỤC TỰ TẠO",Dock=DockStyle.Fill,Margin=new Padding(6,0,0,0)};
            AppTheme.Button(add,true);AppTheme.Button(edit);AppTheme.Button(remove);
            actions.Controls.Add(add,0,0);actions.Controls.Add(edit,1,0);actions.Controls.Add(remove,2,0);
            layout.Controls.Add(actions,0,1);return grid;
        }
        var menus=Page("1 · Menu chính",out var addMenu,out var editMenu,out var deleteMenu);
        var submenus=Page("2 · Menu con",out var addSub,out var editSub,out var deleteSub);
        var functions=Page("3 · Chức năng",out var addFunction,out var editFunction,out var deleteFunction);
        MenuConfiguration config=await service.MenuConfigurationAsync();
        async Task RefreshConfig()
        {
            config=await service.MenuConfigurationAsync();
            menus.DataSource=config.Menus.Select(x=>new {Mã=x.Code,Tên=x.Title,MôTả=x.Description,ThứTự=x.Order,HiểnThị=x.Active,CóSẵn=x.BuiltIn}).ToList();
            submenus.DataSource=config.Submenus.Select(x=>new {Mã=x.Id,MenuChính=config.Menus.FirstOrDefault(m=>m.Code==x.MenuCode)?.Title,Tên=x.Title,ThứTự=x.Order,HiểnThị=x.Active,CóSẵn=x.BuiltIn}).ToList();
            functions.DataSource=config.Functions.Select(x=>new {Mã=x.Id,MenuCon=config.Submenus.FirstOrDefault(s=>s.Id==x.SubmenuId)?.Title,Tên=x.Title,NghiệpVụ=FunctionPolicy.All.FirstOrDefault(f=>f.Code==x.ActionCode)?.Name,ThứTự=x.Order,HiểnThị=x.Active,CóSẵn=x.BuiltIn}).ToList();
            await ReloadMenuConfiguration();
        }
        ConfiguredMenu? SelectedMenu()=>config.Menus.FirstOrDefault(x=>x.Code==(string?)menus.CurrentRow?.Cells["Mã"].Value);
        ConfiguredSubmenu? SelectedSub()=>config.Submenus.FirstOrDefault(x=>x.Id==Convert.ToInt64(submenus.CurrentRow?.Cells["Mã"].Value??0));
        ConfiguredFunction? SelectedFunction()=>config.Functions.FirstOrDefault(x=>x.Id==Convert.ToInt64(functions.CurrentRow?.Cells["Mã"].Value??0));

        void EditMenu(ConfiguredMenu? selected)
        {
            using var dialog=new InputDialog(selected is null?"Thêm menu chính":"Sửa menu chính",600,640);
            var code=Ui.Text(40);code.Text=selected?.Code??"";code.Enabled=selected is null;
            var name=Ui.Text(100);name.Text=selected?.Title??"";
            var description=Ui.Text(180);description.Text=selected?.Description??"";
            var icon=Ui.Combo(new[]{"bed","service","people","clock","money","receipt","chart","key","tools","calendar"});icon.SelectedItem=selected?.Icon??"receipt";
            var color=Ui.Combo(new[]{"#534C84","#377A68","#B56B3E","#916079","#3E699D","#6A6D82"});
            color.SelectedItem=selected?.Color??"#534C84";
            var order=new NumericUpDown {Minimum=0,Maximum=9999,Value=selected?.Order??config.Menus.Count+1};
            var active=new CheckBox {Text="Hiển thị",Checked=selected?.Active??true,AutoSize=true};
            dialog.Add("Mã duy nhất (chữ thường, số, _ hoặc -)",code);dialog.Add("Tên menu",name);
            dialog.Add("Mô tả ngắn",description);dialog.Add("Biểu tượng",icon);dialog.Add("Màu",color);
            dialog.Add("Thứ tự",order);dialog.Add("Trạng thái",active);
            dialog.Action("LƯU MENU",async()=>
            {
                await service.SaveConfiguredMenuAsync(new ConfiguredMenu(code.Text,name.Text,description.Text,(string)icon.SelectedItem!,
                    (string)color.SelectedItem!,(int)order.Value,active.Checked,selected?.BuiltIn??false));
                await RefreshConfig();
            });dialog.ShowDialog(form);
        }
        void EditSub(ConfiguredSubmenu? selected)
        {
            if(config.Menus.Count==0){Ui.Error(form,new BusinessException("Hãy tạo menu chính trước."));return;}
            using var dialog=new InputDialog(selected is null?"Thêm menu con":"Sửa menu con",590,520);
            var parent=Ui.Combo(config.Menus.Select(x=>$"{x.Title} [{x.Code}]").ToArray());
            parent.SelectedIndex=Math.Max(0,config.Menus.FindIndex(x=>x.Code==selected?.MenuCode));
            var name=Ui.Text(100);name.Text=selected?.Title??"";
            var order=new NumericUpDown {Minimum=0,Maximum=9999,Value=selected?.Order??config.Submenus.Count+1};
            var active=new CheckBox {Text="Hiển thị",Checked=selected?.Active??true,AutoSize=true};
            dialog.Add("Thuộc menu chính",parent);dialog.Add("Tên menu con",name);
            dialog.Add("Thứ tự",order);dialog.Add("Trạng thái",active);
            dialog.Action("LƯU MENU CON",async()=>
            {
                var menu=config.Menus[parent.SelectedIndex];
                await service.SaveConfiguredSubmenuAsync(new ConfiguredSubmenu(selected?.Id??0,menu.Code,name.Text,(int)order.Value,active.Checked,selected?.BuiltIn??false));
                await RefreshConfig();
            });dialog.ShowDialog(form);
        }
        void EditFunction(ConfiguredFunction? selected)
        {
            if(config.Submenus.Count==0){Ui.Error(form,new BusinessException("Hãy tạo menu con trước."));return;}
            using var dialog=new InputDialog(selected is null?"Thêm chức năng":"Sửa chức năng",660,570);
            var parent=Ui.Combo(config.Submenus.Select(x=>$"{config.Menus.First(m=>m.Code==x.MenuCode).Title} / {x.Title} [#{x.Id}]").ToArray());
            parent.SelectedIndex=Math.Max(0,config.Submenus.FindIndex(x=>x.Id==selected?.SubmenuId));
            var name=Ui.Text(120);name.Text=selected?.Title??"";
            var available=FunctionPolicy.All.ToArray();
            var action=Ui.Combo(available.Select(x=>$"{x.Name} [{x.Code}]").ToArray());
            action.SelectedIndex=Math.Max(0,Array.FindIndex(available,x=>x.Code==selected?.ActionCode));
            var order=new NumericUpDown {Minimum=0,Maximum=9999,Value=selected?.Order??config.Functions.Count+1};
            var active=new CheckBox {Text="Hiển thị",Checked=selected?.Active??true,AutoSize=true};
            dialog.Add("Menu con",parent);dialog.Add("Tên nút hiển thị",name);
            dialog.Add("Nghiệp vụ sẽ chạy",action);dialog.Add("Thứ tự",order);dialog.Add("Trạng thái",active);
            dialog.Action("LƯU CHỨC NĂNG",async()=>
            {
                await service.SaveConfiguredFunctionAsync(new ConfiguredFunction(selected?.Id??0,config.Submenus[parent.SelectedIndex].Id,
                    name.Text,available[action.SelectedIndex].Code,(int)order.Value,active.Checked,selected?.BuiltIn??false));
                await RefreshConfig();
            });dialog.ShowDialog(form);
        }
        addMenu.Click+=(_,_)=>EditMenu(null);editMenu.Click+=(_,_)=>{if(SelectedMenu() is { } x)EditMenu(x);};
        addSub.Click+=(_,_)=>EditSub(null);editSub.Click+=(_,_)=>{if(SelectedSub() is { } x)EditSub(x);};
        addFunction.Click+=(_,_)=>EditFunction(null);editFunction.Click+=(_,_)=>{if(SelectedFunction() is { } x)EditFunction(x);};
        deleteMenu.Click+=async (_,_)=>
        {
            if(SelectedMenu() is not { } item)return;
            if(item.BuiltIn){Ui.Error(form,new BusinessException("Menu có sẵn chỉ có thể ẩn hoặc đổi tên."));return;}
            if(!Ui.Confirm(form,$"Xóa menu {item.Title}? Menu phải không còn menu con."))return;
            try{await service.DeleteConfiguredMenuAsync(item.Code);await RefreshConfig();}catch(Exception ex){Ui.Error(form,ex);}
        };
        deleteSub.Click+=async (_,_)=>
        {
            if(SelectedSub() is not { } item)return;
            if(item.BuiltIn){Ui.Error(form,new BusinessException("Menu con có sẵn chỉ có thể ẩn hoặc đổi tên."));return;}
            if(!Ui.Confirm(form,$"Xóa menu con {item.Title}? Menu con phải không còn chức năng."))return;
            try{await service.DeleteConfiguredSubmenuAsync(item.Id);await RefreshConfig();}catch(Exception ex){Ui.Error(form,ex);}
        };
        deleteFunction.Click+=async (_,_)=>
        {
            if(SelectedFunction() is not { } item)return;
            if(item.BuiltIn){Ui.Error(form,new BusinessException("Chức năng có sẵn chỉ có thể ẩn hoặc đổi tên."));return;}
            if(!Ui.Confirm(form,$"Xóa chức năng {item.Title}?"))return;
            try{await service.DeleteConfiguredFunctionAsync(item.Id);await RefreshConfig();}catch(Exception ex){Ui.Error(form,ex);}
        };
        await RefreshConfig();form.ShowDialog(this);
    }
}
