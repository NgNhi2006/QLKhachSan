using QLKhachSan.BLL;
using QLKhachSan.DTO;

namespace QLKhachSan.GUI;

internal sealed class DiscountCodesForm : Form
{
    private readonly HotelService service;
    private readonly DataGridView grid=Ui.Grid();

    public DiscountCodesForm(HotelService service)
    {
        this.service=service;
        Text="Quản lý mã giảm giá";Size=new Size(940,600);MinimumSize=new Size(720,480);
        StartPosition=FormStartPosition.CenterParent;Font=AppTheme.Body;BackColor=AppTheme.Canvas;
        var root=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=1,RowCount=3,Padding=new Padding(18)};
        root.RowStyles.Add(new RowStyle(SizeType.Absolute,67));
        root.RowStyles.Add(new RowStyle(SizeType.Percent,100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute,60));Controls.Add(root);
        root.Controls.Add(new Label {Text="MÃ GIẢM GIÁ\nTạo mã cho lễ tân áp dụng trên bill trước thanh toán.",
            Dock=DockStyle.Fill,Font=AppTheme.Bold,ForeColor=AppTheme.Navy},0,0);
        grid.ReadOnly=true;grid.AutoGenerateColumns=true;grid.AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill;
        grid.SelectionMode=DataGridViewSelectionMode.FullRowSelect;root.Controls.Add(grid,0,1);
        var actions=new FlowLayoutPanel {Dock=DockStyle.Fill,WrapContents=false,Padding=new Padding(0,10,0,0)};
        root.Controls.Add(actions,0,2);
        Button Action(string text,Func<Task> action)
        {
            var button=new Button {Text=text,Width=155,Height=38,Margin=new Padding(0,0,10,0)};
            AppTheme.Button(button);actions.Controls.Add(button);
            button.Click+=async (_,_)=>
            {
                button.Enabled=false;
                try{await action();}catch(Exception ex){Ui.Error(this,ex);}
                finally{if(!IsDisposed)button.Enabled=true;}
            };
            return button;
        }
        Action("Tạo mã",()=>EditAsync(null));
        Action("Sửa mã",()=>Selected() is { } selected?EditAsync(selected):
            Task.FromException(new BusinessException("Chọn mã cần sửa.")));
        Action("Bật / tắt mã",async()=>
        {
            var selected=Selected();
            if(selected is null)throw new BusinessException("Chọn mã cần thay đổi.");
            await service.SaveDiscountCodeAsync(selected with {Active=!selected.Active},false);
            await RefreshAsync();
        });
        Shown+=async (_,_)=>
        {
            try{await RefreshAsync();}catch(Exception ex){Ui.Error(this,ex);}
        };
    }

    private DiscountCode? Selected()=>grid.CurrentRow?.DataBoundItem as DiscountCode;

    private async Task RefreshAsync()
    {
        grid.DataSource=await service.DiscountCodesAsync();
        var captions=new Dictionary<string,string>
        {
            ["Code"]="Mã",["Kind"]="Loại",["Value"]="Giá trị",["StartsAt"]="Bắt đầu",
            ["EndsAt"]="Kết thúc",["UsageLimit"]="Giới hạn",["UsedCount"]="Đã dùng",
            ["Active"]="Đang bật",["Description"]="Mức giảm",["TypeLabel"]="Cách giảm"
        };
        foreach(DataGridViewColumn column in grid.Columns)
            if(captions.TryGetValue(column.Name,out var caption))column.HeaderText=caption;
        if(grid.Columns["Value"] is { } raw)raw.Visible=false;
        if(grid.Columns["Kind"] is { } internalKind)internalKind.Visible=false;
        grid.ClearSelection();
    }

    private async Task EditAsync(DiscountCode? current)
    {
        var create=current is null;
        using var dialog=new InputDialog(create?"Tạo mã giảm giá":$"Sửa mã {current!.Code}",600,700);
        var code=Ui.Text(32);code.Text=current?.Code??"";code.Enabled=create;
        var kind=Ui.Combo(new[]{"Giảm theo %","Giảm số tiền"});kind.SelectedIndex=current?.Kind=="Amount"?1:0;
        var value=Ui.Money(current?.Value??10);
        void SetValueMode()
        {
            var percent=kind.SelectedIndex==0;
            value.Maximum=percent?100:1_000_000_000;
            value.DecimalPlaces=percent?2:0;
            value.Increment=percent?1:10000;
        }
        kind.SelectedIndexChanged+=(_,_)=>SetValueMode();SetValueMode();
        var start=Ui.DatePicker(current?.StartsAt??DateTime.Today);
        var end=Ui.DatePicker(current?.EndsAt??DateTime.Today.AddMonths(1));
        var limit=Ui.Number(1_000_000);limit.Minimum=0;limit.Value=current?.UsageLimit??0;
        var active=new CheckBox {Text="Đang hoạt động",Checked=current?.Active??true,AutoSize=true};
        dialog.Add("Mã dùng khi thanh toán",code);
        dialog.Add("Cách giảm",kind);
        dialog.Add("Phần trăm hoặc số tiền",value);
        dialog.Add("Bắt đầu",start);dialog.Add("Kết thúc",end);
        dialog.Add("Giới hạn lượt dùng · 0 là không giới hạn",limit);
        dialog.Add("Trạng thái",active);
        dialog.Action("LƯU MÃ",async()=>
        {
            var item=new DiscountCode(code.Text,kind.SelectedIndex==0?"Percent":"Amount",value.Value,
                start.Value,end.Value,limit.Value==0?null:(int)limit.Value,current?.UsedCount??0,active.Checked);
            await service.SaveDiscountCodeAsync(item,create);
            await RefreshAsync();
        });
        dialog.ShowDialog(this);
    }
}
