using System.Globalization;
using System.Drawing.Printing;
using System.Text.Json;
using QLKhachSan.BLL;
using QLKhachSan.DTO;

namespace QLKhachSan.GUI;

public partial class ucDashboard
{
    private async Task ShowCustomScreen(long id)
    {
        var definition=menuConfiguration?.CustomFunctions.FirstOrDefault(x=>x.Id==id);
        if(definition is null || !definition.Active || !definition.AllowedRoles.Split(',').Contains(user.Role))
            throw new BusinessException("Bạn không có quyền sử dụng chức năng này.");
        var design=HotelService.ParseCustomDesign(definition.DesignJson);
        using var form=new Form {Text=definition.Title,Size=new Size(1180,780),MinimumSize=new Size(850,600),
            StartPosition=FormStartPosition.CenterParent,Font=AppTheme.Body,BackColor=AppTheme.Canvas};
        var root=new TableLayoutPanel {Dock=DockStyle.Fill,RowCount=2,Padding=new Padding(16)};
        root.RowStyles.Add(new RowStyle(SizeType.Absolute,58));root.RowStyles.Add(new RowStyle(SizeType.Percent,100));
        root.Controls.Add(new Label {Text=definition.Title,Dock=DockStyle.Fill,Font=AppTheme.Title,
            ForeColor=AppTheme.Ink,Padding=new Padding(6,10,0,0)},0,0);
        var split=new SplitContainer {Dock=DockStyle.Fill};
        form.Shown+=(_,_)=>split.SplitterDistance=Math.Max(240,split.Width/3);
        root.Controls.Add(split,0,1);form.Controls.Add(root);
        var search=Ui.Text(100);search.PlaceholderText="Tìm trong bản ghi";
        search.Dock=DockStyle.Top;search.Margin=new Padding(0,0,0,8);
        var list=Ui.Grid();list.Dock=DockStyle.Fill;list.AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill;
        var listPanel=new Panel {Dock=DockStyle.Fill,Padding=new Padding(8),BackColor=Color.White};
        listPanel.Controls.Add(list);listPanel.Controls.Add(search);split.Panel1.Controls.Add(listPanel);
        var editor=new Panel {Dock=DockStyle.Fill,BackColor=Color.White,Padding=new Padding(12)};
        var actions=new FlowLayoutPanel {Dock=DockStyle.Bottom,Height=72,AutoScroll=true,
            WrapContents=false,Padding=new Padding(4,10,4,4)};
        var fieldsPanel=new Panel {Dock=DockStyle.Fill,AutoScroll=true};
        var controls=BuildCustomFieldControls(fieldsPanel,design);
        editor.Controls.Add(fieldsPanel);editor.Controls.Add(actions);split.Panel2.Controls.Add(editor);
        var create=new Button {Text=design.NewLabel,Dock=DockStyle.Fill,Margin=new Padding(4,0,4,0),AutoEllipsis=true};
        var save=new Button {Text=design.SaveLabel,Dock=DockStyle.Fill,Margin=new Padding(4,0,4,0),AutoEllipsis=true};
        var delete=new Button {Text=design.DeleteLabel,Dock=DockStyle.Fill,Margin=new Padding(4,0,4,0),AutoEllipsis=true};
        AppTheme.Button(create);AppTheme.Button(save,true);AppTheme.Button(delete);
        if(design.Actions is null)
        {
            foreach(var button in new[]{create,save,delete})
            {
                button.Dock=DockStyle.None;button.Width=145;button.Height=42;actions.Controls.Add(button);
            }
        }
        var records=new List<CustomScreenRecord>();long selectedId=0;
        void ClearFields()
        {
            selectedId=0;list.ClearSelection();
            foreach(var field in design.Fields)
            {
                var control=controls[field.Key];
                switch(control)
                {
                    case TextBox text: text.Clear();break;
                    case ComboBox combo: combo.SelectedIndex=0;break;
                    case CheckBox check: check.Checked=false;break;
                    case FlowLayoutPanel radio:
                        foreach(var option in radio.Controls.OfType<RadioButton>())option.Checked=false;break;
                    case DateTimePicker date: date.Value=DateTime.Today;date.Checked=field.Required;break;
                }
            }
        }
        void LoadRecord(CustomScreenRecord record)
        {
            selectedId=record.Id;
            var values=JsonSerializer.Deserialize<Dictionary<string,string>>(record.ValuesJson)??[];
            foreach(var field in design.Fields)
            {
                values.TryGetValue(field.Key,out var value);value??="";
                var control=controls[field.Key];
                switch(control)
                {
                    case TextBox text: text.Text=value;break;
                    case ComboBox combo: combo.SelectedItem=value;
                        if(combo.SelectedIndex<0)combo.SelectedIndex=0;break;
                    case CheckBox check: check.Checked=value=="true";break;
                    case FlowLayoutPanel radio:
                        foreach(var option in radio.Controls.OfType<RadioButton>())
                            option.Checked=option.Text==value;break;
                    case DateTimePicker date:
                        date.Checked=DateTime.TryParseExact(value,field.Kind=="time"?"HH:mm":"yyyy-MM-dd",
                            CultureInfo.InvariantCulture,DateTimeStyles.None,out var parsed);
                        if(date.Checked)date.Value=parsed;break;
                }
            }
        }
        string Summary(CustomScreenRecord record)
        {
            var values=JsonSerializer.Deserialize<Dictionary<string,string>>(record.ValuesJson)??[];
            return string.Join(" · ",design.Fields.Take(2).Select(f=>values.GetValueOrDefault(f.Key,""))).Trim(' ','·');
        }
        void Filter()
        {
            var needle=search.Text.Trim();
            list.DataSource=records.Where(x=>needle.Length==0 || Summary(x).Contains(needle,StringComparison.OrdinalIgnoreCase))
                .Select(x=>new {Mã=x.Id,NộiDung=Summary(x),CậpNhật=x.UpdatedAt.ToLocalTime()}).ToList();
            list.ClearSelection();
        }
        async Task RefreshRecords()
        {
            records=await service.CustomRecordsAsync(id);
            Filter();
        }
        search.TextChanged+=(_,_)=>Filter();
        list.SelectionChanged+=(_,_)=>
        {
            if(list.CurrentRow?.Selected==true && list.CurrentRow.Cells["Mã"].Value is long recordId &&
               records.FirstOrDefault(x=>x.Id==recordId) is { } record)LoadRecord(record);
        };
        create.Click+=(_,_)=>ClearFields();
        async Task SaveRecord()
        {
            save.Enabled=false;
            try
            {
                var values=new Dictionary<string,string>();
                foreach(var field in design.Fields)
                {
                    var control=controls[field.Key];
                    values[field.Key]=control switch
                    {
                        CheckBox check=>check.Checked?"true":"false",
                        DateTimePicker date=>date.Checked?date.Value.ToString(
                            field.Kind=="time"?"HH:mm":"yyyy-MM-dd",CultureInfo.InvariantCulture):"",
                        FlowLayoutPanel radio=>radio.Controls.OfType<RadioButton>()
                            .FirstOrDefault(x=>x.Checked)?.Text??"",
                        ComboBox combo=>combo.SelectedItem?.ToString()??"",
                        TextBox text when field.Kind is "number" or "currency" or "percent" &&
                            decimal.TryParse(text.Text,NumberStyles.Number,CultureInfo.CurrentCulture,out var number)
                            =>number.ToString(CultureInfo.InvariantCulture),
                        TextBox text when field.Kind=="integer" && long.TryParse(text.Text,out var integer)
                            =>integer.ToString(CultureInfo.InvariantCulture),
                        TextBox text=>text.Text.Trim(),_=>""
                    };
                }
                await service.SaveCustomRecordAsync(id,selectedId,JsonSerializer.Serialize(values));
                await RefreshRecords();ClearFields();
            }
            catch(Exception ex){Ui.Error(form,ex);}
            finally{if(!form.IsDisposed)save.Enabled=true;}
        }
        async Task DeleteRecord()
        {
            if(selectedId==0 || !Ui.Confirm(form,"Xóa bản ghi đã chọn?"))return;
            try{await service.DeleteCustomRecordAsync(id,selectedId);await RefreshRecords();ClearFields();}
            catch(Exception ex){Ui.Error(form,ex);}
        }
        save.Click+=async (_,_)=>await SaveRecord();
        delete.Click+=async (_,_)=>await DeleteRecord();
        async Task ExportAsync()
        {
            using var picker=new SaveFileDialog {Filter="Tệp CSV (*.csv)|*.csv",
                FileName=definition.Title+".csv"};
            if(picker.ShowDialog(form)!=DialogResult.OK)return;
            static string Csv(string value)=>"\""+value.Replace("\"","\"\"")+"\"";
            var rows=new List<string> {string.Join(",",new[]{"Mã"}.Concat(design.Fields
                .Where(x=>x.Kind is not ("heading" or "separator")).Select(x=>x.Label)).Select(Csv))};
            foreach(var record in records)
            {
                var values=JsonSerializer.Deserialize<Dictionary<string,string>>(record.ValuesJson)??[];
                rows.Add(string.Join(",",new[]{record.Id.ToString()}.Concat(design.Fields
                    .Where(x=>x.Kind is not ("heading" or "separator"))
                    .Select(x=>values.GetValueOrDefault(x.Key,""))).Select(Csv)));
            }
            await File.WriteAllLinesAsync(picker.FileName,rows,System.Text.Encoding.UTF8);
        }
        void PrintSelected()
        {
            var record=records.FirstOrDefault(x=>x.Id==selectedId);
            if(record is null){Ui.Error(form,new BusinessException("Hãy chọn một bản ghi để in."));return;}
            var values=JsonSerializer.Deserialize<Dictionary<string,string>>(record.ValuesJson)??[];
            using var document=new PrintDocument();
            document.DocumentName=definition.Title;
            document.PrintPage+=(_,e)=>
            {
                if(e.Graphics is null)return;
                var y=e.MarginBounds.Top;
                using var headingFont=new Font(AppTheme.Body.FontFamily,17,FontStyle.Bold);
                e.Graphics.DrawString(definition.Title,headingFont,Brushes.Black,e.MarginBounds.Left,y);y+=48;
                foreach(var field in design.Fields.Where(x=>x.Kind is not ("heading" or "separator")))
                {
                    e.Graphics.DrawString($"{field.Label}: {values.GetValueOrDefault(field.Key,"")}",
                        AppTheme.Body,Brushes.Black,new RectangleF(e.MarginBounds.Left,y,e.MarginBounds.Width,44));
                    y+=44;
                }
            };
            using var preview=new PrintPreviewDialog {Document=document,Width=900,Height=700};
            preview.ShowDialog(form);
        }
        void CalculateRental()
        {
            Control Field(string label)=>controls[design.Fields.FirstOrDefault(x=>x.Label==label)?.Key
                ??throw new BusinessException($"Mẫu thuê xe thiếu ô {label}.")];
            var pickup=(DateTimePicker)Field("Ngày nhận xe");
            var dropoff=(DateTimePicker)Field("Ngày trả xe");
            var daily=(TextBox)Field("Giá thuê/ngày");
            var total=(TextBox)Field("Tổng tiền");
            if(!pickup.Checked || !dropoff.Checked ||
                !decimal.TryParse(daily.Text,NumberStyles.Number,CultureInfo.CurrentCulture,out var rate) ||
                rate<0)throw new BusinessException("Hãy nhập ngày nhận, ngày trả và giá thuê/ngày hợp lệ.");
            var days=(dropoff.Value.Date-pickup.Value.Date).Days;
            if(days<0)throw new BusinessException("Ngày trả xe phải từ ngày nhận xe trở đi.");
            total.Text=(Math.Max(1,days)*rate).ToString("0.##",CultureInfo.CurrentCulture);
        }
        if(design.Actions is { } customActions)
            foreach(var action in customActions)
            {
                var button=new Button {Text=action.Label,Width=145,Height=42,Margin=new Padding(4),
                    AutoEllipsis=true};
                AppTheme.Button(button,action.Kind=="save");
                button.Click+=async (_,_)=>
                {
                    try
                    {
                        switch(action.Kind)
                        {
                            case "save":await SaveRecord();break;
                            case "new" or "clear":ClearFields();break;
                            case "delete":await DeleteRecord();break;
                            case "duplicate":
                                if(selectedId==0)throw new BusinessException("Hãy chọn bản ghi để sao chép.");
                                selectedId=0;list.ClearSelection();break;
                            case "refresh":await RefreshRecords();break;
                            case "search":search.Focus();break;
                            case "export":await ExportAsync();break;
                            case "print":PrintSelected();break;
                            case "rental_calc":CalculateRental();break;
                        }
                    }
                    catch(Exception ex){Ui.Error(form,ex);}
                };
                actions.Controls.Add(button);
            }
        try{await RefreshRecords();ClearFields();form.ShowDialog(this);}
        catch(Exception ex){Ui.Error(this,ex);}
    }

    private static Dictionary<string,Control> BuildCustomFieldControls(Control host,CustomScreenDesign design)
    {
        var layout=new TableLayoutPanel {Dock=DockStyle.Top,AutoSize=true,ColumnCount=2,Padding=new Padding(8)};
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,50));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,50));
        var controls=new Dictionary<string,Control>();int row=0,column=0;
        foreach(var field in design.Fields)
        {
            if(field.Wide && column==1){row++;column=0;}
            var fieldHeight=field.Kind=="multiline"?138:field.Kind=="radio"?112:82;
            while(layout.RowStyles.Count<=row)layout.RowStyles.Add(new RowStyle(SizeType.Absolute,fieldHeight+8));
            if(layout.RowStyles[row].Height<fieldHeight+8)layout.RowStyles[row].Height=fieldHeight+8;
            var panel=new Panel {Height=fieldHeight,Dock=DockStyle.Fill,Margin=new Padding(4,2,8,4),Tag=field.Key};
            panel.Controls.Add(new Label {Text=field.Label+(field.Required?" *":""),Dock=DockStyle.Top,Height=27,
                Font=AppTheme.Bold,ForeColor=AppTheme.Ink,AutoEllipsis=true});
            Control input=field.Kind switch
            {
                "check" or "toggle"=>new CheckBox {Text=field.Kind=="toggle"?"Bật":"Có / Đã chọn",AutoSize=true},
                "date"=>new DateTimePicker {Format=DateTimePickerFormat.Custom,CustomFormat="dd/MM/yyyy",ShowCheckBox=true,Checked=field.Required},
                "time"=>new DateTimePicker {Format=DateTimePickerFormat.Custom,CustomFormat="HH:mm",ShowUpDown=true,
                    ShowCheckBox=true,Checked=field.Required},
                "choice"=>Ui.Combo((field.Required?Array.Empty<string>():new[]{""}).Concat(
                    field.Options.Split(';',StringSplitOptions.RemoveEmptyEntries|StringSplitOptions.TrimEntries)).ToArray()),
                "radio"=>new FlowLayoutPanel {AutoScroll=true,WrapContents=true},
                "heading"=>new Label {Text=field.Label,Font=AppTheme.Title,ForeColor=AppTheme.Ink},
                "separator"=>new Label {BorderStyle=BorderStyle.Fixed3D,Height=2},
                _=>new TextBox {Multiline=field.Kind=="multiline",MaxLength=2000,ScrollBars=field.Kind=="multiline"?ScrollBars.Vertical:ScrollBars.None}
            };
            if(input is FlowLayoutPanel radio)
                foreach(var option in field.Options.Split(';',StringSplitOptions.RemoveEmptyEntries|StringSplitOptions.TrimEntries))
                    radio.Controls.Add(new RadioButton {Text=option,AutoSize=true});
            if(field.Kind is "heading" or "separator")panel.Controls[0].Visible=false;
            input.Location=new Point(0,29);input.Height=field.Kind=="multiline"?94:32;
            if(field.Kind=="radio")input.Height=76;
            if(field.Kind is "heading" or "separator")input.Location=new Point(0,10);
            input.Width=Math.Max(120,panel.ClientSize.Width-8);
            panel.Controls.Add(input);
            panel.Resize+=(_,_)=>input.Width=Math.Max(120,panel.ClientSize.Width-8);
            layout.Controls.Add(panel,column,row);
            if(field.Wide)layout.SetColumnSpan(panel,2);
            controls[field.Key]=input;
            if(field.Wide || column==1){row++;column=0;}else column=1;
        }
        host.Controls.Add(layout);
        EventHandler resize=(_,_)=>layout.Width=Math.Max(420,host.ClientSize.Width-24);
        host.Resize+=resize;layout.Disposed+=(_,_)=>host.Resize-=resize;
        resize(host,EventArgs.Empty);
        return controls;
    }
}
