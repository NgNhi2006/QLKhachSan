using QLKhachSan.BLL;
using QLKhachSan.DTO;

namespace QLKhachSan.GUI;

public partial class ucDashboard
{
    private static readonly (string Code,string Name)[] MenuIconChoices=
    [
        ("bed","Giường / phòng"),("building","Tòa nhà"),("door","Cửa phòng"),("bath","Phòng tắm"),
        ("service","Dịch vụ"),("food","Đồ ăn"),("coffee","Đồ uống"),("wifi","Wi-Fi"),
        ("car","Bãi xe"),("phone","Điện thoại"),("bell","Chuông lễ tân"),("bag","Hành lý"),
        ("people","Khách / nhân viên"),("calendar","Lịch"),("clock","Đồng hồ"),
        ("money","Tiền"),("receipt","Hóa đơn"),("chart","Biểu đồ"),("key","Chìa khóa"),
        ("tools","Công cụ"),("gear","Cài đặt"),("folder","Hồ sơ"),("shield","Bảo mật"),
        ("star","Ngôi sao"),("heart","Yêu thích"),("search","Tìm kiếm"),("swap","Chuyển đổi"),("refresh","Làm mới")
    ];
    private static byte[] ValidateMenuPng(byte[] bytes)
    {
        if(bytes.Length>1048576 || bytes.Length<8 || !bytes.Take(8).SequenceEqual(new byte[]{137,80,78,71,13,10,26,10}))
            throw new BusinessException("Chỉ nhận ảnh PNG tối đa 1 MB.");
        using var stream=new MemoryStream(bytes);
        using var image=Image.FromStream(stream);
        if(image.Width>2048 || image.Height>2048)throw new BusinessException("Ảnh PNG cần nhỏ hơn 2048 × 2048 px.");
        return bytes;
    }
    private async Task ShowMenuConfiguration()
    {
        if(user.Role!="Admin")throw new BusinessException("Chỉ quản trị viên được tùy chỉnh menu.");
        using var form=new Form {Text="Tùy chỉnh menu và chức năng",Size=new Size(1280,800),MinimumSize=new Size(1020,650),MinimizeBox=false,
            StartPosition=FormStartPosition.CenterParent,Font=AppTheme.Body,BackColor=AppTheme.Canvas};
        var root=new TableLayoutPanel {Dock=DockStyle.Fill,RowCount=3,ColumnCount=1};
        root.RowStyles.Add(new RowStyle(SizeType.Absolute,104));root.RowStyles.Add(new RowStyle(SizeType.Percent,100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute,54));form.Controls.Add(root);
        var header=new Panel {Dock=DockStyle.Fill,BackColor=Color.White};root.Controls.Add(header,0,0);
        header.Controls.Add(new Label {Text="Thiết kế không gian làm việc",Location=new Point(24,13),AutoSize=true,
            Font=AppTheme.Title,ForeColor=AppTheme.Ink});
        header.Controls.Add(new Label {Text="Sắp xếp menu, thêm menu con và tự dựng màn hình chức năng.",Location=new Point(26,56),AutoSize=true,
            Font=AppTheme.Body,ForeColor=AppTheme.Muted});
        var summary=new Label {AutoSize=false,TextAlign=ContentAlignment.MiddleRight,ForeColor=AppTheme.Blue,
            Font=AppTheme.Bold,Height=34,Anchor=AnchorStyles.Top|AnchorStyles.Right};
        header.Controls.Add(summary);
        header.Resize+=(_,_)=>summary.Bounds=new Rectangle(Math.Max(0,header.Width-430),28,400,34);
        summary.Bounds=new Rectangle(Math.Max(0,header.Width-430),28,400,34);
        var tabs=new TabControl {Dock=DockStyle.Fill,Margin=new Padding(18,12,18,8),Font=AppTheme.Bold};root.Controls.Add(tabs,0,1);
        var structure=new TabPage("Cấu trúc menu") {BackColor=Color.White,Padding=new Padding(14)};
        tabs.TabPages.Add(structure);
        var structureLayout=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=2,Padding=new Padding(6)};
        structureLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,40));
        structureLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,60));structure.Controls.Add(structureLayout);
        var treePanel=new Panel {Dock=DockStyle.Fill,Padding=new Padding(8),BackColor=AppTheme.Canvas};
        var structureTree=new TreeView {Dock=DockStyle.Fill,BorderStyle=BorderStyle.None,HideSelection=false,
            ItemHeight=34,Font=AppTheme.Body,BackColor=AppTheme.Canvas};
        treePanel.Controls.Add(structureTree);structureLayout.Controls.Add(treePanel,0,0);
        var detailPanel=new TableLayoutPanel {Dock=DockStyle.Fill,RowCount=3,Padding=new Padding(24,18,16,16)};
        detailPanel.RowStyles.Add(new RowStyle(SizeType.Absolute,64));
        detailPanel.RowStyles.Add(new RowStyle(SizeType.Percent,100));
        detailPanel.RowStyles.Add(new RowStyle(SizeType.Absolute,170));
        var detailTitle=new Label {Text="Chọn một mục ở cây menu",Dock=DockStyle.Fill,Font=AppTheme.Title,
            ForeColor=AppTheme.Ink};detailPanel.Controls.Add(detailTitle,0,0);
        var detailText=new Label {Text="Chọn menu chính để thêm menu con, rồi chọn menu con để thiết kế chức năng.",
            Dock=DockStyle.Fill,ForeColor=AppTheme.Muted,Font=AppTheme.Body,Padding=new Padding(0,10,0,0)};
        detailPanel.Controls.Add(detailText,0,1);
        var treeActions=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=2,RowCount=3};
        treeActions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,50));
        treeActions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,50));
        for(var i=0;i<3;i++)treeActions.RowStyles.Add(new RowStyle(SizeType.Percent,100f/3));
        var treeAdd=new Button {Text="+ MENU CHÍNH",Dock=DockStyle.Fill,Margin=new Padding(4)};
        var treeChild=new Button {Text="+ MENU CON",Dock=DockStyle.Fill,Margin=new Padding(4)};
        var treeFunction=new Button {Text="+ CHỨC NĂNG MỚI",Dock=DockStyle.Fill,Margin=new Padding(4)};
        var treeEdit=new Button {Text="SỬA MỤC ĐÃ CHỌN",Dock=DockStyle.Fill,Margin=new Padding(4)};
        var treeDelete=new Button {Text="XÓA MỤC ĐÃ CHỌN",Dock=DockStyle.Fill,Margin=new Padding(4)};
        AppTheme.Button(treeAdd,true);AppTheme.Button(treeChild);AppTheme.Button(treeFunction,true);AppTheme.Button(treeEdit);AppTheme.Button(treeDelete);
        treeActions.Controls.Add(treeAdd,0,0);treeActions.Controls.Add(treeChild,1,0);
        treeActions.Controls.Add(treeFunction,0,1);treeActions.Controls.Add(treeEdit,1,1);
        treeActions.Controls.Add(treeDelete,0,2);treeActions.SetColumnSpan(treeDelete,2);
        detailPanel.Controls.Add(treeActions,0,2);
        structureLayout.Controls.Add(detailPanel,1,0);
        var note=new Label {Text="Tạo menu chính → menu con → thiết kế chức năng mới. Thêm ô dữ liệu, chọn vai trò và xem trước trước khi lưu.",
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
        addFunction.Text="+  THIẾT KẾ CHỨC NĂNG";
        editFunction.Text="SỬA / THIẾT KẾ";
        MenuConfiguration config=await service.MenuConfigurationAsync();
        async Task RefreshConfig()
        {
            config=await service.MenuConfigurationAsync();
            menus.DataSource=config.Menus.Select(x=>new {Mã=x.Code,Tên=x.Title,MôTả=x.Description,ThứTự=x.Order,HiểnThị=x.Active,CóSẵn=x.BuiltIn}).ToList();
            submenus.DataSource=config.Submenus.Select(x=>new {Mã=x.Id,MenuChính=config.Menus.FirstOrDefault(m=>m.Code==x.MenuCode)?.Title,Tên=x.Title,ThứTự=x.Order,HiểnThị=x.Active,CóSẵn=x.BuiltIn}).ToList();
            functions.DataSource=config.Functions.Select(x=>new {Mã=$"builtin:{x.Id}",MenuCon=config.Submenus.FirstOrDefault(s=>s.Id==x.SubmenuId)?.Title,
                Tên=x.Title,Loại="Chức năng có sẵn",ThứTự=x.Order,HiểnThị=x.Active,CóSẵn=x.BuiltIn})
                .Concat(config.CustomFunctions.Select(x=>new {Mã=$"custom:{x.Id}",MenuCon=config.Submenus.FirstOrDefault(s=>s.Id==x.SubmenuId)?.Title,
                    Tên=x.Title,Loại="Màn hình tự thiết kế",ThứTự=x.Order,HiểnThị=x.Active,CóSẵn=false})).ToList();
            summary.Text=$"{config.Menus.Count} menu  ·  {config.Submenus.Count} menu con  ·  {config.CustomFunctions.Count} chức năng mới";
            RebuildStructure();
            await ReloadMenuConfiguration();
        }
        ConfiguredMenu? SelectedMenu()=>config.Menus.FirstOrDefault(x=>x.Code==(string?)menus.CurrentRow?.Cells["Mã"].Value);
        ConfiguredSubmenu? SelectedSub()=>config.Submenus.FirstOrDefault(x=>x.Id==Convert.ToInt64(submenus.CurrentRow?.Cells["Mã"].Value??0));
        ConfiguredFunction? SelectedFunction()=>config.Functions.FirstOrDefault(x=>$"builtin:{x.Id}"==(string?)functions.CurrentRow?.Cells["Mã"].Value);
        ConfiguredCustomFunction? SelectedCustom()=>config.CustomFunctions.FirstOrDefault(x=>$"custom:{x.Id}"==(string?)functions.CurrentRow?.Cells["Mã"].Value);
        static void SelectRow(DataGridView grid,string code)
        {
            foreach(DataGridViewRow row in grid.Rows)
                if(Convert.ToString(row.Cells["Mã"].Value)==code)
                {
                    grid.ClearSelection();row.Selected=true;grid.CurrentCell=row.Cells[0];return;
                }
        }

        void EditMenu(ConfiguredMenu? selected)
        {
            using var dialog=new Form {Text=selected is null?"Thêm menu chính":"Sửa menu chính",Size=new Size(1060,650),
                MinimumSize=new Size(900,580),StartPosition=FormStartPosition.CenterParent,Font=AppTheme.Body,
                BackColor=Color.White,MinimizeBox=false};
            var shell=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=1,RowCount=3};
            shell.RowStyles.Add(new RowStyle(SizeType.Absolute,68));
            shell.RowStyles.Add(new RowStyle(SizeType.Percent,100));
            shell.RowStyles.Add(new RowStyle(SizeType.Absolute,72));dialog.Controls.Add(shell);
            shell.Controls.Add(new Label {Text=dialog.Text,Dock=DockStyle.Fill,Font=AppTheme.Title,
                ForeColor=AppTheme.Ink,Padding=new Padding(24,18,0,0)},0,0);
            var columns=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=2,Padding=new Padding(18,8,18,8)};
            columns.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,48));
            columns.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,52));shell.Controls.Add(columns,0,1);
            var details=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=1,AutoScroll=true,Padding=new Padding(8)};
            var appearance=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=1,AutoScroll=true,Padding=new Padding(8)};
            columns.Controls.Add(details,0,0);columns.Controls.Add(appearance,1,0);
            static void AddField(TableLayoutPanel panel,string label,Control control,int height=0)
            {
                panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                panel.Controls.Add(new Label {Text=label,AutoSize=true,Font=AppTheme.Bold,ForeColor=AppTheme.Muted,
                    Margin=new Padding(0,8,0,4)},0,panel.RowCount++);
                if(height>0)control.Height=height;
                control.Dock=DockStyle.Top;control.Margin=new Padding(0,0,8,8);
                panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                panel.Controls.Add(control,0,panel.RowCount++);
            }
            var code=Ui.Text(40);code.Text=selected?.Code??"";code.Enabled=selected is null || !selected.BuiltIn;
            var name=Ui.Text(100);name.Text=selected?.Title??"";
            var description=Ui.Text(180);description.Text=selected?.Description??"";
            var icon=Ui.Combo(MenuIconChoices.Select(x=>x.Name).ToArray());
            icon.DrawMode=DrawMode.OwnerDrawFixed;icon.ItemHeight=32;icon.IntegralHeight=false;icon.DropDownHeight=240;icon.MaxDropDownItems=7;
            icon.DrawItem+=(_,e)=>
            {
                if(e.Index<0)return;
                e.DrawBackground();
                using var symbol=UiIcons.Create(MenuIconChoices[e.Index].Code,AppTheme.Blue,24);
                e.Graphics.DrawImage(symbol,e.Bounds.Left+6,e.Bounds.Top+4,24,24);
                TextRenderer.DrawText(e.Graphics,MenuIconChoices[e.Index].Name,AppTheme.Body,
                    new Rectangle(e.Bounds.Left+38,e.Bounds.Top,e.Bounds.Width-42,e.Bounds.Height),AppTheme.Ink,
                    TextFormatFlags.Left|TextFormatFlags.VerticalCenter);
                e.DrawFocusRectangle();
            };
            icon.SelectedIndex=Math.Max(0,Array.FindIndex(MenuIconChoices,x=>x.Code==(selected?.Icon??"receipt")));
            byte[]? iconPng=selected?.IconPng;
            var preview=new PictureBox {Size=new Size(72,72),SizeMode=PictureBoxSizeMode.CenterImage};
            var previewPanel=new FlowLayoutPanel {Height=82,Dock=DockStyle.Top,WrapContents=false};
            previewPanel.Controls.Add(preview);
            var choosePng=new Button {Text="Chọn PNG từ máy",Width=160,Height=36,Margin=new Padding(10,18,4,0)};
            var fromWeb=new Button {Text="Tải PNG từ URL",Width=150,Height=36,Margin=new Padding(4,18,4,0)};
            previewPanel.Controls.Add(choosePng);previewPanel.Controls.Add(fromWeb);
            var color=Ui.Text(7);color.Text=selected?.Color??"#534C84";
            var colorPanel=new FlowLayoutPanel {Height=45,Dock=DockStyle.Top,WrapContents=false};
            var colorSwatch=new Panel {Size=new Size(34,34),Margin=new Padding(0,4,8,0)};
            var chooseColor=new Button {Text="Bảng màu…",Width=120,Height=34,Margin=new Padding(0,4,8,0)};
            colorPanel.Controls.Add(colorSwatch);colorPanel.Controls.Add(chooseColor);
            foreach(var hex in new[]{"#534C84","#377A68","#B56B3E","#916079","#3E699D","#6A6D82","#E89B3C","#263F6A"})
            {
                var swatch=new Button {BackColor=ColorTranslator.FromHtml(hex),Size=new Size(34,34),Margin=new Padding(2,4,2,0),FlatStyle=FlatStyle.Flat,AccessibleName=hex};
                swatch.Click+=(_,_)=>color.Text=hex;colorPanel.Controls.Add(swatch);
            }
            void UpdatePreview()
            {
                preview.Image?.Dispose();
                if(!System.Text.RegularExpressions.Regex.IsMatch(color.Text,@"^#[0-9A-Fa-f]{6}$")){colorSwatch.BackColor=Color.White;preview.Image=null;return;}
                var accent=ColorTranslator.FromHtml(color.Text);colorSwatch.BackColor=accent;
                preview.Image=UiIcons.Create(MenuIconChoices[icon.SelectedIndex].Code,accent,64,iconPng);
            }
            icon.SelectedIndexChanged+=(_,_)=>{iconPng=null;UpdatePreview();};
            color.TextChanged+=(_,_)=>UpdatePreview();
            chooseColor.Click+=(_,_)=>{using var picker=new ColorDialog {FullOpen=true,Color=colorSwatch.BackColor};
                if(picker.ShowDialog(dialog)==DialogResult.OK)color.Text=$"#{picker.Color.R:X2}{picker.Color.G:X2}{picker.Color.B:X2}";};
            choosePng.Click+=(_,_)=>
            {
                using var picker=new OpenFileDialog {Filter="Ảnh PNG (*.png)|*.png",Title="Chọn biểu tượng PNG"};
                if(picker.ShowDialog(dialog)!=DialogResult.OK)return;
                try{iconPng=ValidateMenuPng(File.ReadAllBytes(picker.FileName));UpdatePreview();}catch(Exception ex){Ui.Error(dialog,ex);}
            };
            fromWeb.Click+=async (_,_)=>
            {
                using var urlDialog=new InputDialog("Tải biểu tượng PNG từ mạng",600,280);
                var url=Ui.Text(100);url.PlaceholderText="https://example.com/icon.png";urlDialog.Add("Đường dẫn HTTPS đến ảnh PNG",url);
                urlDialog.Action("TẢI ẢNH",async()=>
                {
                    if(!Uri.TryCreate(url.Text.Trim(),UriKind.Absolute,out var uri) || uri.Scheme!=Uri.UriSchemeHttps)
                        throw new BusinessException("Hãy nhập đường dẫn HTTPS hợp lệ.");
                    using var client=new HttpClient {Timeout=TimeSpan.FromSeconds(15)};
                    using var response=await client.GetAsync(uri,HttpCompletionOption.ResponseHeadersRead);
                    response.EnsureSuccessStatusCode();
                    if(response.Content.Headers.ContentLength>1048576)throw new BusinessException("Ảnh PNG vượt quá 1 MB.");
                    await using var stream=await response.Content.ReadAsStreamAsync();
                    await using var buffer=new MemoryStream();
                    var chunk=new byte[8192];int count;
                    while((count=await stream.ReadAsync(chunk))>0)
                    {
                        if(buffer.Length+count>1048576)throw new BusinessException("Ảnh PNG vượt quá 1 MB.");
                        await buffer.WriteAsync(chunk.AsMemory(0,count));
                    }
                    iconPng=ValidateMenuPng(buffer.ToArray());UpdatePreview();
                });urlDialog.ShowDialog(dialog);
            };
            UpdatePreview();
            var order=new NumericUpDown {Minimum=0,Maximum=9999,Value=selected?.Order??config.Menus.Count+1};
            var active=new CheckBox {Text="Hiển thị",Checked=selected?.Active??true,AutoSize=true};
            AddField(details,"Mã duy nhất (1–40 ký tự: chữ thường, số, _ hoặc -)",code);
            AddField(details,"Tên menu",name);AddField(details,"Mô tả ngắn",description);
            AddField(details,"Thứ tự",order);AddField(details,"Trạng thái",active);
            AddField(appearance,"Chọn biểu tượng",icon);
            AddField(appearance,"Xem trước / ảnh PNG riêng",previewPanel,82);
            AddField(appearance,"Mã màu HEX",color);
            AddField(appearance,"Bảng màu và màu gợi ý",colorPanel,45);
            var footer=new FlowLayoutPanel {Dock=DockStyle.Fill,FlowDirection=FlowDirection.RightToLeft,
                Padding=new Padding(16,12,24,8),BackColor=AppTheme.Canvas};
            var save=new Button {Text="LƯU MENU",Width=180,Height=42};AppTheme.Button(save,true);
            footer.Controls.Add(save);shell.Controls.Add(footer,0,2);
            dialog.AcceptButton=save;
            save.Click+=async (_,_)=>
            {
                save.Enabled=false;
                try
                {
                    if(config.Menus.Any(x=>x.Code.Equals(code.Text.Trim(),StringComparison.OrdinalIgnoreCase) && x.Code!=selected?.Code))
                        throw new BusinessException("Mã menu này đã tồn tại. Hãy nhập mã khác.");
                    var item=new ConfiguredMenu(code.Text,name.Text,description.Text,MenuIconChoices[icon.SelectedIndex].Code,
                        color.Text,(int)order.Value,active.Checked,selected?.BuiltIn??false,iconPng);
                    if(selected is not null && !selected.Code.Equals(code.Text.Trim(),StringComparison.OrdinalIgnoreCase))
                        await service.RenameConfiguredMenuAsync(selected.Code,item);
                    else await service.SaveConfiguredMenuAsync(item);
                    await RefreshConfig();
                    SelectRow(menus,code.Text.Trim().ToLowerInvariant());
                    tabs.SelectedTab=structure;
                    SelectStructure($"m:{code.Text.Trim().ToLowerInvariant()}");
                    dialog.DialogResult=DialogResult.OK;dialog.Close();
                }
                catch(Exception ex){Ui.Error(dialog,ex);}
                finally{if(!dialog.IsDisposed)save.Enabled=true;}
            };
            dialog.ShowDialog(form);
        }
        void EditSub(ConfiguredSubmenu? selected,string? menuCode=null)
        {
            if(config.Menus.Count==0){Ui.Error(form,new BusinessException("Hãy tạo menu chính trước."));return;}
            using var dialog=new InputDialog(selected is null?"Thêm menu con":"Sửa menu con",590,520);
            var parent=Ui.Combo(config.Menus.Select(x=>$"{x.Title} [{x.Code}]").ToArray());
            parent.SelectedIndex=Math.Max(0,config.Menus.FindIndex(x=>x.Code==(selected?.MenuCode??menuCode)));
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
                var saved=config.Submenus.LastOrDefault(x=>x.MenuCode==menu.Code && x.Title==name.Text.Trim());
                if(saved is not null)SelectRow(submenus,saved.Id.ToString());
                tabs.SelectedTab=structure;
                if(saved is not null)SelectStructure($"s:{saved.Id}");
            });dialog.ShowDialog(form);
        }
        void EditFunction(ConfiguredFunction? selected,long? submenuId=null)
        {
            if(config.Submenus.Count==0){Ui.Error(form,new BusinessException("Hãy tạo menu con trước."));return;}
            using var dialog=new InputDialog(selected is null?"Thêm chức năng":"Sửa chức năng",660,570);
            var parent=Ui.Combo(config.Submenus.Select(x=>$"{config.Menus.First(m=>m.Code==x.MenuCode).Title} / {x.Title} [#{x.Id}]").ToArray());
            parent.SelectedIndex=Math.Max(0,config.Submenus.FindIndex(x=>x.Id==(selected?.SubmenuId??submenuId)));
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
                tabs.SelectedTab=structure;
                if(selected is not null)SelectStructure($"b:{selected.Id}");
            });dialog.ShowDialog(form);
        }
        async Task RefreshCustom()
        {
            await RefreshConfig();
            tabs.SelectedTab=structure;
        }
        static string NodeKey(object? item) => item switch
        {
            ConfiguredMenu menu=>$"m:{menu.Code}",
            ConfiguredSubmenu sub=>$"s:{sub.Id}",
            ConfiguredFunction function=>$"b:{function.Id}",
            ConfiguredCustomFunction custom=>$"c:{custom.Id}",_=>""
        };
        void SelectStructure(string key)
        {
            TreeNode? Find(TreeNodeCollection nodes)
            {
                foreach(TreeNode node in nodes)
                {
                    if(NodeKey(node.Tag)==key)return node;
                    if(Find(node.Nodes) is { } found)return found;
                }
                return null;
            }
            if(Find(structureTree.Nodes) is { } match)
            {
                match.EnsureVisible();structureTree.SelectedNode=match;
            }
        }
        void RebuildStructure()
        {
            var selectedKey=NodeKey(structureTree.SelectedNode?.Tag);
            structureTree.BeginUpdate();structureTree.Nodes.Clear();
            foreach(var menu in config.Menus.OrderBy(x=>x.Order).ThenBy(x=>x.Title))
            {
                var menuNode=new TreeNode($"{menu.Title}{(menu.Active?"":" (ẩn)")}") {Tag=menu};
                foreach(var sub in config.Submenus.Where(x=>x.MenuCode==menu.Code).OrderBy(x=>x.Order))
                {
                    var subNode=new TreeNode($"{sub.Title}{(sub.Active?"":" (ẩn)")}") {Tag=sub};
                    foreach(var function in config.Functions.Where(x=>x.SubmenuId==sub.Id).OrderBy(x=>x.Order))
                        subNode.Nodes.Add(new TreeNode($"{function.Title}  ·  có sẵn") {Tag=function});
                    foreach(var custom in config.CustomFunctions.Where(x=>x.SubmenuId==sub.Id).OrderBy(x=>x.Order))
                        subNode.Nodes.Add(new TreeNode($"{custom.Title}  ·  tự thiết kế") {Tag=custom});
                    menuNode.Nodes.Add(subNode);
                }
                structureTree.Nodes.Add(menuNode);
                menuNode.Expand();
            }
            structureTree.EndUpdate();
            SelectStructure(selectedKey);
            if(structureTree.SelectedNode is null && structureTree.Nodes.Count>0)structureTree.SelectedNode=structureTree.Nodes[0];
        }
        long? TreeSubmenuId() => structureTree.SelectedNode?.Tag switch
        {
            ConfiguredSubmenu sub=>sub.Id,
            ConfiguredFunction function=>function.SubmenuId,
            ConfiguredCustomFunction custom=>custom.SubmenuId,
            ConfiguredMenu menu when config.Submenus.Count(x=>x.MenuCode==menu.Code)==1 =>
                config.Submenus.First(x=>x.MenuCode==menu.Code).Id,
            _=>null
        };
        string? TreeMenuCode() => structureTree.SelectedNode?.Tag switch
        {
            ConfiguredMenu menu=>menu.Code,
            ConfiguredSubmenu sub=>sub.MenuCode,
            ConfiguredFunction function=>config.Submenus.FirstOrDefault(x=>x.Id==function.SubmenuId)?.MenuCode,
            ConfiguredCustomFunction custom=>config.Submenus.FirstOrDefault(x=>x.Id==custom.SubmenuId)?.MenuCode,
            _=>null
        };
        structureTree.AfterSelect+=(_,_)=>
        {
            var item=structureTree.SelectedNode?.Tag;
            (detailTitle.Text,detailText.Text)=item switch
            {
                ConfiguredMenu menu=>(menu.Title,$"Menu chính · mã {menu.Code}\n{config.Submenus.Count(x=>x.MenuCode==menu.Code)} menu con. Bấm + Menu con để nối mục mới vào menu này."),
                ConfiguredSubmenu sub=>(sub.Title,$"Menu con thuộc {config.Menus.FirstOrDefault(x=>x.Code==sub.MenuCode)?.Title}\nBấm + Chức năng mới để thiết kế một màn hình riêng trong menu con này."),
                ConfiguredFunction function=>(function.Title,$"Chức năng có sẵn · chạy nghiệp vụ {function.ActionCode}\nCó thể đổi tên, thứ tự, trạng thái và chuyển sang menu con khác."),
                ConfiguredCustomFunction custom=>(custom.Title,$"Màn hình tự thiết kế · vai trò: {custom.AllowedRoles}\nCó thể sửa ô dữ liệu, bố cục, tên nút và chuyển sang menu con khác."),
                _=>("Chọn một mục ở cây menu","Chọn menu chính để thêm menu con, rồi chọn menu con để thiết kế chức năng.")
            };
            treeChild.Enabled=TreeMenuCode() is not null;
            treeFunction.Enabled=TreeSubmenuId() is not null;
            treeEdit.Enabled=item is not null;treeDelete.Enabled=item is not null;
        };
        treeAdd.Click+=(_,_)=>EditMenu(null);
        treeChild.Click+=(_,_)=>EditSub(null,TreeMenuCode());
        treeFunction.Click+=(_,_)=>EditCustomScreen(null,TreeSubmenuId(),config,form,RefreshCustom);
        treeEdit.Click+=(_,_)=>{switch(structureTree.SelectedNode?.Tag)
        {
            case ConfiguredMenu menu:EditMenu(menu);break;
            case ConfiguredSubmenu sub:EditSub(sub);break;
            case ConfiguredFunction function:EditFunction(function);break;
            case ConfiguredCustomFunction custom:EditCustomScreen(custom,null,config,form,RefreshCustom);break;
        }};
        treeDelete.Click+=async (_,_)=>
        {
            var item=structureTree.SelectedNode?.Tag;
            if(item is null)return;
            if(item is ConfiguredMenu {BuiltIn:true} or ConfiguredSubmenu {BuiltIn:true} or ConfiguredFunction {BuiltIn:true})
            {Ui.Error(form,new BusinessException("Mục có sẵn chỉ có thể ẩn, đổi tên hoặc chuyển sang menu khác."));return;}
            var label=item switch {ConfiguredMenu m=>m.Title,ConfiguredSubmenu s=>s.Title,
                ConfiguredFunction f=>f.Title,ConfiguredCustomFunction c=>c.Title,_=>""};
            if(!Ui.Confirm(form,$"Xóa {label} cùng các mục tự tạo bên trong? Chức năng có bản ghi sẽ không bị xóa."))return;
            try
            {
                switch(item)
                {
                    case ConfiguredMenu menu:await service.DeleteConfiguredMenuAsync(menu.Code);break;
                    case ConfiguredSubmenu sub:await service.DeleteConfiguredSubmenuAsync(sub.Id);break;
                    case ConfiguredFunction function:await service.DeleteConfiguredFunctionAsync(function.Id);break;
                    case ConfiguredCustomFunction custom:await service.DeleteCustomFunctionAsync(custom.Id);break;
                }
                await RefreshConfig();
            }
            catch(Exception ex){Ui.Error(form,ex);}
        };
        addMenu.Click+=(_,_)=>EditMenu(null);editMenu.Click+=(_,_)=>{if(SelectedMenu() is { } x)EditMenu(x);};
        addSub.Click+=(_,_)=>EditSub(null,SelectedMenu()?.Code);editSub.Click+=(_,_)=>{if(SelectedSub() is { } x)EditSub(x);};
        addFunction.Click+=(_,_)=>EditCustomScreen(null,SelectedSub()?.Id,config,form,RefreshCustom);
        editFunction.Click+=(_,_)=>{if(SelectedCustom() is { } custom)EditCustomScreen(custom,null,config,form,RefreshCustom);
            else if(SelectedFunction() is { } builtIn)EditFunction(builtIn);};
        deleteMenu.Click+=async (_,_)=>
        {
            if(SelectedMenu() is not { } item)return;
            if(item.BuiltIn){Ui.Error(form,new BusinessException("Menu có sẵn chỉ có thể ẩn hoặc đổi tên."));return;}
            if(!Ui.Confirm(form,$"Xóa menu {item.Title} cùng các menu con và chức năng tự tạo bên trong?"))return;
            try{await service.DeleteConfiguredMenuAsync(item.Code);await RefreshConfig();}catch(Exception ex){Ui.Error(form,ex);}
        };
        deleteSub.Click+=async (_,_)=>
        {
            if(SelectedSub() is not { } item)return;
            if(item.BuiltIn){Ui.Error(form,new BusinessException("Menu con có sẵn chỉ có thể ẩn hoặc đổi tên."));return;}
            if(!Ui.Confirm(form,$"Xóa menu con {item.Title} cùng các chức năng tự tạo bên trong?"))return;
            try{await service.DeleteConfiguredSubmenuAsync(item.Id);await RefreshConfig();}catch(Exception ex){Ui.Error(form,ex);}
        };
        deleteFunction.Click+=async (_,_)=>
        {
            if(SelectedCustom() is { } custom)
            {
                if(!Ui.Confirm(form,$"Xóa chức năng {custom.Title}? Chỉ xóa được khi chưa có bản ghi."))return;
                try{await service.DeleteCustomFunctionAsync(custom.Id);await RefreshConfig();}catch(Exception ex){Ui.Error(form,ex);}
                return;
            }
            if(SelectedFunction() is not { } item)return;
            if(item.BuiltIn){Ui.Error(form,new BusinessException("Chức năng có sẵn chỉ có thể ẩn hoặc đổi tên."));return;}
            if(!Ui.Confirm(form,$"Xóa chức năng {item.Title}?"))return;
            try{await service.DeleteConfiguredFunctionAsync(item.Id);await RefreshConfig();}catch(Exception ex){Ui.Error(form,ex);}
        };
        await RefreshConfig();form.ShowDialog(this);
    }
}
