using System.Text.Json;
using QLKhachSan.BLL;
using QLKhachSan.DTO;

namespace QLKhachSan.GUI;

public partial class ucDashboard
{
    private void EditCustomScreen(ConfiguredCustomFunction? selected,long? submenuId,MenuConfiguration config,
        Form owner,Func<Task> refresh)
    {
        if(config.Submenus.Count==0){Ui.Error(owner,new BusinessException("Hãy tạo menu con trước."));return;}
        CustomScreenDesign initial;
        try{initial=selected is null
            ? new CustomScreenDesign([],"Lưu","Tạo mới","Xóa")
            : HotelService.ParseCustomDesign(selected.DesignJson);}
        catch(Exception ex){Ui.Error(owner,ex);return;}
        var fields=initial.Fields.ToList();
        var actionItems=initial.Actions?.ToList()??new List<CustomScreenAction>
        {
            new(initial.NewLabel,"new"),new(initial.SaveLabel,"save"),new(initial.DeleteLabel,"delete")
        };
        using var dialog=new Form {Text=selected is null?"Thiết kế chức năng mới":"Sửa thiết kế chức năng",
            Size=new Size(1220,820),MinimumSize=new Size(950,680),StartPosition=FormStartPosition.CenterParent,
            Font=AppTheme.Body,BackColor=AppTheme.Canvas,MinimizeBox=false};
        var root=new TableLayoutPanel {Dock=DockStyle.Fill,RowCount=4,Padding=new Padding(16)};
        root.RowStyles.Add(new RowStyle(SizeType.Absolute,76));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute,220));
        root.RowStyles.Add(new RowStyle(SizeType.Percent,100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute,62));dialog.Controls.Add(root);
        var titleBand=new Panel {Dock=DockStyle.Fill,BackColor=AppTheme.Navy,Margin=new Padding(0,0,0,10),Padding=new Padding(20,12,10,5)};
        titleBand.Controls.Add(new Label {Text=dialog.Text,Dock=DockStyle.Fill,Font=AppTheme.Title,ForeColor=Color.White});
        root.Controls.Add(titleBand,0,0);
        var header=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=2,RowCount=4,BackColor=Color.White,Padding=new Padding(12)};
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,50));header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,50));
        for(var i=0;i<4;i++)header.RowStyles.Add(new RowStyle(SizeType.Percent,25));
        var parent=Ui.Combo(config.Submenus.Select(x=>$"{config.Menus.First(m=>m.Code==x.MenuCode).Title} / {x.Title} [#{x.Id}]").ToArray());
        parent.SelectedIndex=Math.Max(0,config.Submenus.FindIndex(x=>x.Id==(selected?.SubmenuId??submenuId)));
        var title=Ui.Text(120);title.Text=selected?.Title??"";
        var order=new NumericUpDown {Minimum=0,Maximum=9999,Value=selected?.Order??config.Functions.Count+config.CustomFunctions.Count+1};
        var active=new CheckBox {Text="Hiển thị trên dashboard",Checked=selected?.Active??true,AutoSize=true};
        var roles=new FlowLayoutPanel {Dock=DockStyle.Fill,WrapContents=false};
        var roleChecks=new Dictionary<string,CheckBox>();
        foreach(var (role,label) in new[]{("Admin","Quản trị"),("Manager","Quản lý"),("Reception","Lễ tân"),("Accountant","Kế toán")})
        {
            var check=new CheckBox {Text=label,AutoSize=true,Checked=selected?.AllowedRoles.Split(',').Contains(role)??role=="Admin",
                Margin=new Padding(3,6,14,0)};
            roles.Controls.Add(check);roleChecks[role]=check;
        }
        static Control Labelled(string label,Control input)
        {
            var panel=new Panel {Dock=DockStyle.Fill,Padding=new Padding(6,1,8,1)};
            panel.Controls.Add(new Label {Text=label,Dock=DockStyle.Top,Height=19,Font=AppTheme.Small,ForeColor=AppTheme.Muted});
            input.Dock=DockStyle.Bottom;input.Height=28;panel.Controls.Add(input);return panel;
        }
        header.Controls.Add(Labelled("Thuộc menu con",parent),0,0);
        header.Controls.Add(Labelled("Tên chức năng",title),1,0);
        header.Controls.Add(Labelled("Thứ tự",order),0,1);
        header.Controls.Add(Labelled("Trạng thái",active),1,1);
        header.Controls.Add(Labelled("Vai trò được dùng",roles),0,2);header.SetColumnSpan(header.GetControlFromPosition(0,2)!,2);
        var hint=new Label {Text="Kéo ô hoặc nút từ Toolbox sang bản xem trước. Bấm nút để sửa tên; chuột phải để xóa. Có mẫu thuê xe để bắt đầu nhanh.",
            Dock=DockStyle.Fill,ForeColor=AppTheme.Muted,Padding=new Padding(8,8,0,0)};
        header.Controls.Add(hint,0,3);header.SetColumnSpan(hint,2);
        root.Controls.Add(header,0,1);
        var split=new SplitContainer {Dock=DockStyle.Fill};
        dialog.Shown+=(_,_)=>split.SplitterDistance=Math.Max(350,split.Width*43/100);
        root.Controls.Add(split,0,2);
        var left=new TableLayoutPanel {Dock=DockStyle.Fill,RowCount=5,BackColor=Color.White,Padding=new Padding(12)};
        left.RowStyles.Add(new RowStyle(SizeType.Absolute,34));
        left.RowStyles.Add(new RowStyle(SizeType.Absolute,224));
        left.RowStyles.Add(new RowStyle(SizeType.Percent,100));
        left.RowStyles.Add(new RowStyle(SizeType.Absolute,165));
        left.RowStyles.Add(new RowStyle(SizeType.Absolute,50));
        left.Controls.Add(new Label {Text="TOOLBOX  ·  Kéo vào màn hình",Dock=DockStyle.Fill,Font=AppTheme.Bold,ForeColor=AppTheme.Ink},0,0);
        var toolbox=new FlowLayoutPanel {Dock=DockStyle.Fill,WrapContents=true,BackColor=AppTheme.Canvas,AutoScroll=true};
        left.Controls.Add(toolbox,0,1);
        var grid=Ui.Grid();grid.Dock=DockStyle.Fill;grid.AllowDrop=true;left.Controls.Add(grid,0,2);
        var properties=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=2,RowCount=4,BackColor=AppTheme.Canvas,Padding=new Padding(7)};
        properties.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,50));
        properties.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,50));
        for(var i=0;i<4;i++)properties.RowStyles.Add(new RowStyle(SizeType.Percent,25));
        var propertyTitle=new Label {Text="THUỘC TÍNH Ô ĐÃ CHỌN",Dock=DockStyle.Fill,Font=AppTheme.Bold,ForeColor=AppTheme.Ink};
        properties.Controls.Add(propertyTitle,0,0);properties.SetColumnSpan(propertyTitle,2);
        var propName=Ui.Text(80);var propKind=Ui.Combo(new[]{"Văn bản","Nhiều dòng","Email","Điện thoại","Liên kết",
            "Số thập phân","Số nguyên","Tiền tệ","Phần trăm","Ngày","Giờ","Ô đánh dấu","Công tắc",
            "Danh sách chọn","Nhóm lựa chọn","Tiêu đề nhóm","Đường phân cách"});
        var propRequired=new CheckBox {Text="Bắt buộc",AutoSize=true};var propWide=new CheckBox {Text="Cả hàng",AutoSize=true};
        var propOptions=Ui.Text(500);propOptions.PlaceholderText="Lựa chọn 1;Lựa chọn 2";
        var propApply=new Button {Text="ÁP DỤNG THUỘC TÍNH",Dock=DockStyle.Fill,Margin=new Padding(3)};
        AppTheme.Button(propApply,true);
        properties.Controls.Add(propName,0,1);properties.Controls.Add(propKind,1,1);
        properties.Controls.Add(propRequired,0,2);properties.Controls.Add(propWide,1,2);
        properties.Controls.Add(propOptions,0,3);properties.Controls.Add(propApply,1,3);
        left.Controls.Add(properties,0,3);
        var tools=new FlowLayoutPanel {Dock=DockStyle.Fill,WrapContents=false};
        Button Tool(string text,int width)
        {
            var button=new Button {Text=text,Width=width,Height=38};AppTheme.Button(button);tools.Controls.Add(button);return button;
        }
        var add=Tool("+ Ô nhập",95);var edit=Tool("Sửa",74);var remove=Tool("Xóa",74);
        var up=Tool("↑",45);var down=Tool("↓",45);
        left.Controls.Add(tools,0,4);split.Panel1.Controls.Add(left);
        var right=new TableLayoutPanel {Dock=DockStyle.Fill,RowCount=4,BackColor=Color.White,Padding=new Padding(12)};
        right.RowStyles.Add(new RowStyle(SizeType.Absolute,38));right.RowStyles.Add(new RowStyle(SizeType.Percent,100));
        right.RowStyles.Add(new RowStyle(SizeType.Absolute,82));right.RowStyles.Add(new RowStyle(SizeType.Absolute,92));
        right.Controls.Add(new Label {Text="Xem trước màn hình",Dock=DockStyle.Fill,Font=AppTheme.Bold,ForeColor=AppTheme.Ink},0,0);
        var preview=new Panel {Dock=DockStyle.Fill,AutoScroll=true,BackColor=Color.White};
        right.Controls.Add(preview,0,1);
        var labels=new FlowLayoutPanel {Dock=DockStyle.Fill};
        var saveLabel=Ui.Text(40);saveLabel.Text=initial.SaveLabel;
        var newLabel=Ui.Text(40);newLabel.Text=initial.NewLabel;
        var deleteLabel=Ui.Text(40);deleteLabel.Text=initial.DeleteLabel;
        foreach(var (caption,input) in new[]{("Nút lưu",saveLabel),("Nút mới",newLabel),("Nút xóa",deleteLabel)})
        {
            var part=new Panel {Width=150,Height=72};
            part.Controls.Add(new Label {Text=caption,Dock=DockStyle.Top,Height=23,ForeColor=AppTheme.Muted});
            input.Dock=DockStyle.Bottom;part.Controls.Add(input);labels.Controls.Add(part);
        }
        right.Controls.Add(labels,0,2);
        var buttonPreview=new FlowLayoutPanel {Dock=DockStyle.Fill,WrapContents=true,AutoScroll=true,
            BackColor=AppTheme.Canvas,AllowDrop=true};
        right.Controls.Add(buttonPreview,0,3);split.Panel2.Controls.Add(right);
        var wiredDrops=new HashSet<Control>();
        var kindCodes=new[]{"text","multiline","email","phone","url","number","integer","currency",
            "percent","date","time","check","toggle","choice","radio","heading","separator"};
        var kindNames=new[]{"Văn bản","Nhiều dòng","Email","Điện thoại","Liên kết","Số thập phân",
            "Số nguyên","Tiền tệ","Phần trăm","Ngày","Giờ","Ô đánh dấu","Công tắc",
            "Danh sách chọn","Nhóm lựa chọn","Tiêu đề nhóm","Đường phân cách"};
        var kindDefaults=new[]{"Nội dung","Ghi chú","Email","Số điện thoại","Liên kết","Số lượng",
            "Số lượng","Số tiền","Tỷ lệ","Ngày thực hiện","Giờ thực hiện","Đã hoàn tất",
            "Bật / tắt","Trạng thái","Mức độ","Nhóm thông tin","Phân cách"};
        for(var k=0;k<kindCodes.Length;k++)
        {
            var group=k switch
            {
                0=>"VĂN BẢN & LIÊN HỆ",
                5=>"SỐ & TIỀN",
                9=>"THỜI GIAN",
                11=>"TRẠNG THÁI",
                13=>"LỰA CHỌN",
                15=>"BỐ CỤC",
                _=>null
            };
            if(group is not null)
            {
                var groupLabel=new Label {Text=group,Width=292,Height=24,Margin=new Padding(6,9,0,0),
                    Font=AppTheme.Bold,ForeColor=AppTheme.Muted};
                toolbox.Controls.Add(groupLabel);toolbox.SetFlowBreak(groupLabel,true);
            }
            var code=kindCodes[k];
            var tile=new Button {Text=kindNames[k],Width=132,Height=36,Margin=new Padding(5),
                Tag=code,Cursor=Cursors.Hand};
            AppTheme.Button(tile);
            toolbox.Controls.Add(tile);
            Point origin=Point.Empty;
            tile.MouseDown+=(_,e)=>origin=e.Location;
            tile.MouseMove+=(_,e)=>
            {
                if(e.Button==MouseButtons.Left &&
                   (Math.Abs(e.X-origin.X)>SystemInformation.DragSize.Width/2 ||
                    Math.Abs(e.Y-origin.Y)>SystemInformation.DragSize.Height/2))
                    tile.DoDragDrop("toolbox:"+code,DragDropEffects.Copy);
            };
            tile.Click+=(_,_)=>AddField(code,fields.Count);
        }
        var actionLabel=new Label {Text="NÚT HÀNH ĐỘNG",Width=292,Height=24,Margin=new Padding(6,9,0,0),
            Font=AppTheme.Bold,ForeColor=AppTheme.Muted};
        toolbox.Controls.Add(actionLabel);toolbox.SetFlowBreak(actionLabel,true);
        foreach(var (kind,caption) in new[]{("save","Lưu"),("new","Tạo mới"),("delete","Xóa"),
            ("duplicate","Sao chép"),("refresh","Làm mới"),("search","Tìm kiếm"),
            ("export","Xuất CSV"),("print","In bản ghi"),("clear","Xóa trắng"),
            ("rental_calc","Tính tiền thuê")})
        {
            var tile=new Button {Text="▣  "+caption,Width=132,Height=36,Margin=new Padding(5),
                Cursor=Cursors.Hand};
            AppTheme.Button(tile);toolbox.Controls.Add(tile);
            Point origin=Point.Empty;
            tile.MouseDown+=(_,e)=>origin=e.Location;
            tile.MouseMove+=(_,e)=>
            {
                if(e.Button==MouseButtons.Left &&
                   (Math.Abs(e.X-origin.X)>SystemInformation.DragSize.Width/2 ||
                    Math.Abs(e.Y-origin.Y)>SystemInformation.DragSize.Height/2))
                    tile.DoDragDrop("action:"+kind+":"+caption,DragDropEffects.Copy);
            };
            tile.Click+=(_,_)=>AddAction(kind,caption);
        }
        var rentalTemplate=new Button {Text="MẪU THUÊ XE: TẠO Ô + NÚT",Width=282,Height=38,
            Margin=new Padding(5),Cursor=Cursors.Hand};
        AppTheme.Button(rentalTemplate,true);toolbox.Controls.Add(rentalTemplate);
        rentalTemplate.Click+=(_,_)=>
        {
            if(fields.Count>0 && !Ui.Confirm(dialog,"Thay các ô hiện tại bằng mẫu thuê xe?"))return;
            var sample=new (string Label,string Kind,bool Required,string Options)[]
            {
                ("Khách thuê","text",true,""),("Số điện thoại","phone",true,""),
                ("Biển số xe","text",true,""),("Loại xe","choice",true,"Xe máy;Ô tô;Xe đạp"),
                ("Ngày nhận xe","date",true,""),("Ngày trả xe","date",true,""),
                ("Giá thuê/ngày","currency",true,""),("Tiền cọc","currency",false,""),
                ("Tổng tiền","currency",false,""),("Trạng thái","choice",true,"Đặt trước;Đang thuê;Đã trả")
            };
            fields=sample.Select((x,i)=>new CustomFieldDefinition($"f{i+1}",x.Label,x.Kind,
                x.Required,false,x.Options)).ToList();
            actionItems=[new("Tạo mới","new"),new("Tính tiền thuê","rental_calc"),
                new("Lưu phiếu","save"),new("Sao chép","duplicate"),new("In phiếu","print"),
                new("Xuất CSV","export"),new("Xóa phiếu","delete")];
            if(string.IsNullOrWhiteSpace(title.Text))title.Text="Thuê xe";
            Render();
        };
        void AddAction(string kind,string caption)
        {
            if(actionItems.Count>=20){Ui.Error(dialog,new BusinessException("Tối đa 20 nút hành động."));return;}
            actionItems.Add(new CustomScreenAction(caption,kind));Render();
        }
        int SelectedIndex()=>grid.CurrentRow?.Index??-1;
        void SelectField(int index)
        {
            if(index>=0 && index<grid.Rows.Count)grid.CurrentCell=grid.Rows[index].Cells[0];
        }
        void AddField(string kind,int index)
        {
            if(fields.Count>=60){Ui.Error(dialog,new BusinessException("Một chức năng có tối đa 60 thành phần."));return;}
            var kindIndex=Array.IndexOf(kindCodes,kind);
            if(kindIndex<0)return;
            var key=$"f{Enumerable.Range(1,999).First(n=>fields.All(f=>f.Key!=$"f{n}"))}";
            var field=new CustomFieldDefinition(key,kindDefaults[kindIndex],kind,false,
                kind is "multiline" or "heading" or "separator",
                kind is "choice" or "radio"?"Mới;Đang xử lý;Hoàn tất":"");
            index=Math.Clamp(index,0,fields.Count);
            fields.Insert(index,field);Render();SelectField(index);propName.Focus();propName.SelectAll();
        }
        void MoveField(int from,int to)
        {
            if(from<0 || from>=fields.Count)return;
            to=Math.Clamp(to,0,fields.Count);
            var field=fields[from];fields.RemoveAt(from);
            if(to>from)to--;
            fields.Insert(to,field);Render();SelectField(to);
        }
        void LoadProperties()
        {
            var i=SelectedIndex();
            properties.Enabled=i>=0 && i<fields.Count;
            if(!properties.Enabled)return;
            var field=fields[i];
            propName.Text=field.Label;
            propKind.SelectedIndex=Math.Max(0,Array.IndexOf(kindCodes,field.Kind));
            propRequired.Checked=field.Required;
            propWide.Checked=field.Wide;
            propOptions.Text=field.Options;
            propOptions.Enabled=field.Kind is "choice" or "radio";
        }
        grid.SelectionChanged+=(_,_)=>LoadProperties();
        propKind.SelectedIndexChanged+=(_,_)=>propOptions.Enabled=propKind.SelectedIndex is 13 or 14;
        propApply.Click+=(_,_)=>
        {
            var i=SelectedIndex();if(i<0 || i>=fields.Count)return;
            try
            {
                var copy=fields.ToList();
                copy[i]=new CustomFieldDefinition(copy[i].Key,propName.Text.Trim(),
                    kindCodes[Math.Max(0,propKind.SelectedIndex)],propRequired.Checked,
                    propWide.Checked,propOptions.Text.Trim());
                HotelService.ParseCustomDesign(JsonSerializer.Serialize(
                    new CustomScreenDesign(copy,"Lưu","Tạo mới","Xóa",actionItems)));
                fields=copy;Render();SelectField(i);
            }
            catch(Exception ex){Ui.Error(dialog,ex);}
        };
        void DropAt(DragEventArgs e,int index)
        {
            if(e.Data?.GetData(DataFormats.Text) is not string token)return;
            if(token.StartsWith("toolbox:",StringComparison.Ordinal))
                AddField(token["toolbox:".Length..],index);
            else if(token.StartsWith("action:",StringComparison.Ordinal))
            {
                var parts=token.Split(':',3);
                if(parts.Length==3)AddAction(parts[1],parts[2]);
            }
            else if(token.StartsWith("field:",StringComparison.Ordinal) &&
                int.TryParse(token["field:".Length..],out var from))
                MoveField(from,index);
        }
        void WireDrop(Control control)
        {
            if(wiredDrops.Add(control))
            {
                control.AllowDrop=true;
                control.DragEnter+=(_,e)=>
                {
                    var token=e.Data?.GetData(DataFormats.Text) as string;
                    e.Effect=token?.StartsWith("toolbox:",StringComparison.Ordinal)==true ||
                        token?.StartsWith("action:",StringComparison.Ordinal)==true
                        ? DragDropEffects.Copy
                        : token?.StartsWith("field:",StringComparison.Ordinal)==true
                            ? DragDropEffects.Move:DragDropEffects.None;
                };
                control.DragDrop+=(_,e)=>
                {
                    if(control==grid)
                    {
                        var point=grid.PointToClient(new Point(e.X,e.Y));
                        var hit=grid.HitTest(point.X,point.Y);
                        DropAt(e,hit.RowIndex<0?fields.Count:hit.RowIndex);return;
                    }
                    var target=control;
                    while(target is not null && target!=preview && target.Tag is not string)
                        target=target.Parent!;
                    var key=target?.Tag as string;
                    var index=key is null?fields.Count:fields.FindIndex(f=>f.Key==key);
                    DropAt(e,index<0?fields.Count:index);
                };
            }
            foreach(Control child in control.Controls)WireDrop(child);
        }
        WireDrop(grid);
        buttonPreview.DragEnter+=(_,e)=>e.Effect=(e.Data?.GetData(DataFormats.Text) as string)?
            .StartsWith("action:",StringComparison.Ordinal)==true?DragDropEffects.Copy:DragDropEffects.None;
        buttonPreview.DragDrop+=(_,e)=>DropAt(e,fields.Count);
        Point rowOrigin=Point.Empty;
        grid.MouseDown+=(_,e)=>rowOrigin=e.Location;
        grid.MouseMove+=(_,e)=>
        {
            if(e.Button!=MouseButtons.Left || SelectedIndex()<0)return;
            if(Math.Abs(e.X-rowOrigin.X)>SystemInformation.DragSize.Width/2 ||
               Math.Abs(e.Y-rowOrigin.Y)>SystemInformation.DragSize.Height/2)
                grid.DoDragDrop("field:"+SelectedIndex(),DragDropEffects.Move);
        };
        void Render()
        {
            var selectedIndex=SelectedIndex();
            grid.DataSource=fields.Select((f,i)=>new {ThứTự=i+1,TênÔ=f.Label,
                Loại=kindCodes.Contains(f.Kind)?kindNames[Array.IndexOf(kindCodes,f.Kind)]:f.Kind,
                BắtBuộc=f.Required,ĐộRộng=f.Wide?"Cả hàng":"Nửa hàng"}).ToList();
            if(selectedIndex>=0 && selectedIndex<grid.Rows.Count)grid.CurrentCell=grid.Rows[selectedIndex].Cells[0];
            foreach(Control child in preview.Controls.Cast<Control>().ToArray())child.Dispose();
            if(fields.Count>0)
            {
                BuildCustomFieldControls(preview,new CustomScreenDesign(fields,saveLabel.Text,newLabel.Text,deleteLabel.Text));
                foreach(Control child in preview.Controls.Cast<Control>().SelectMany(
                    c=>Descendants(c)).ToArray())
                {
                    if(child is TextBox box)box.ReadOnly=true;
                    else if(child is ComboBox or DateTimePicker or CheckBox or NumericUpDown)
                        child.Enabled=false;
                }
            }
            else preview.Controls.Add(new Label {Text="Kéo một ô từ Toolbox vào đây để bắt đầu",
                Dock=DockStyle.Fill,TextAlign=ContentAlignment.MiddleCenter,ForeColor=AppTheme.Muted});
            WireDrop(preview);
            foreach(Control child in buttonPreview.Controls.Cast<Control>().ToArray())child.Dispose();
            foreach(var action in actionItems.ToArray())
            {
                var button=new Button {Text=action.Label,Width=130,Height=36,
                    Tag=action,AutoEllipsis=true};
                AppTheme.Button(button);buttonPreview.Controls.Add(button);
                button.Click+=(_,_)=>EditAction(action);
                var context=new ContextMenuStrip();
                context.Items.Add("Đổi tên nút",null,(_,_)=>EditAction(action));
                context.Items.Add("Xóa nút",null,(_,_)=>
                {
                    actionItems.Remove(action);Render();
                });
                button.ContextMenuStrip=context;
            }
        }
        void EditAction(CustomScreenAction action)
        {
            using var input=new InputDialog("Sửa nút hành động",460,260);
            var name=Ui.Text(40);name.Text=action.Label;
            input.Add("Tên hiển thị trên nút",name);
            input.Action("LƯU NÚT",async()=>
            {
                var index=actionItems.IndexOf(action);
                if(index<0)return;
                var copy=actionItems.ToList();
                copy[index]=action with {Label=name.Text.Trim()};
                HotelService.ParseCustomDesign(JsonSerializer.Serialize(
                    new CustomScreenDesign(fields,"Lưu","Tạo mới","Xóa",copy)));
                actionItems=copy;Render();await Task.CompletedTask;
            });
            input.ShowDialog(dialog);
        }
        static IEnumerable<Control> Descendants(Control root)
        {
            yield return root;
            foreach(Control child in root.Controls)
                foreach(var nested in Descendants(child))yield return nested;
        }
        void EditField(int index)
        {
            var current=index>=0?fields[index]:null;
            using var fieldDialog=new InputDialog(current is null?"Thêm ô dữ liệu":"Sửa ô dữ liệu",560,590);
            var label=Ui.Text(80);label.Text=current?.Label??"";
            var kind=Ui.Combo(kindNames);kind.SelectedIndex=Math.Max(0,Array.IndexOf(kindCodes,current?.Kind??"text"));
            var required=new CheckBox {Text="Bắt buộc nhập",Checked=current?.Required??false,AutoSize=true};
            var wide=new CheckBox {Text="Cả hàng (ô rộng)",Checked=current?.Wide??false,AutoSize=true};
            var options=Ui.Text(500);options.Text=current?.Options??"";
            options.PlaceholderText="Ví dụ: Mới;Đang xử lý;Hoàn tất";
            fieldDialog.Add("Tên ô hiển thị",label);fieldDialog.Add("Loại ô",kind);
            fieldDialog.Add("Bắt buộc",required);fieldDialog.Add("Chiều rộng",wide);
            fieldDialog.Add("Các lựa chọn, ngăn bằng dấu ; (nếu chọn loại Lựa chọn)",options);
            fieldDialog.Action("LƯU Ô DỮ LIỆU",async()=>
            {
                var key=current?.Key??$"f{Enumerable.Range(1,999).First(n=>fields.All(f=>f.Key!=$"f{n}"))}";
                var field=new CustomFieldDefinition(key,label.Text.Trim(),kindCodes[kind.SelectedIndex],
                    required.Checked,wide.Checked,options.Text.Trim());
                var copy=fields.ToList();
                if(index<0)copy.Add(field);else copy[index]=field;
                HotelService.ParseCustomDesign(JsonSerializer.Serialize(new CustomScreenDesign(copy,"Lưu","Tạo mới","Xóa",actionItems)));
                fields=copy;Render();await Task.CompletedTask;
            });fieldDialog.ShowDialog(dialog);
        }
        add.Click+=(_,_)=>EditField(-1);
        edit.Click+=(_,_)=>{if(SelectedIndex() is var i && i>=0 && i<fields.Count)EditField(i);};
        remove.Click+=(_,_)=>
        {
            var i=SelectedIndex();if(i<0 || i>=fields.Count)return;
            if(selected is not null && !Ui.Confirm(dialog,$"Bỏ ô {fields[i].Label} khỏi màn hình? Dữ liệu cũ của ô này sẽ không còn hiển thị."))return;
            fields.RemoveAt(i);Render();
        };
        up.Click+=(_,_)=>{var i=SelectedIndex();if(i>0){(fields[i-1],fields[i])=(fields[i],fields[i-1]);Render();grid.CurrentCell=grid.Rows[i-1].Cells[0];}};
        down.Click+=(_,_)=>{var i=SelectedIndex();if(i>=0 && i<fields.Count-1){(fields[i+1],fields[i])=(fields[i],fields[i+1]);Render();grid.CurrentCell=grid.Rows[i+1].Cells[0];}};
        saveLabel.TextChanged+=(_,_)=>Render();newLabel.TextChanged+=(_,_)=>Render();deleteLabel.TextChanged+=(_,_)=>Render();
        var footer=new FlowLayoutPanel {Dock=DockStyle.Fill,FlowDirection=FlowDirection.RightToLeft,Padding=new Padding(8)};
        var save=new Button {Text="LƯU CHỨC NĂNG MỚI",Width=230,Height=42};
        AppTheme.Button(save,true);footer.Controls.Add(save);root.Controls.Add(footer,0,3);dialog.AcceptButton=save;
        save.Click+=async (_,_)=>
        {
            save.Enabled=false;
            try
            {
                var design=new CustomScreenDesign(fields,saveLabel.Text.Trim(),newLabel.Text.Trim(),deleteLabel.Text.Trim(),actionItems);
                var allowed=string.Join(",",roleChecks.Where(x=>x.Value.Checked).Select(x=>x.Key));
                await service.SaveCustomFunctionAsync(new ConfiguredCustomFunction(selected?.Id??0,
                    config.Submenus[parent.SelectedIndex].Id,title.Text,(int)order.Value,active.Checked,allowed,
                    JsonSerializer.Serialize(design)));
                await refresh();dialog.DialogResult=DialogResult.OK;dialog.Close();
            }
            catch(Exception ex){Ui.Error(dialog,ex);}
            finally{if(!dialog.IsDisposed)save.Enabled=true;}
        };
        Render();dialog.ShowDialog(owner);
    }
}
