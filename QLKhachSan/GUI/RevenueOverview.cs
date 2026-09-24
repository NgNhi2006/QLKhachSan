using System.Drawing.Drawing2D;
using QLKhachSan.DTO;

namespace QLKhachSan.GUI;

internal sealed class RevenueOverview : UserControl
{
    private readonly ComboBox period=Ui.Combo(new[]{"7 ngày gần nhất","30 ngày gần nhất"});
    private readonly Label total=new(),average=new(),best=new(),range=new();
    private readonly RevenuePlot trend=new(false),categories=new(true);
    public int Days=>period.SelectedIndex==1?30:7;
    public event EventHandler? PeriodChanged;
    public RevenueOverview()
    {
        BackColor=AppTheme.Canvas;Padding=new Padding(18);MinimumSize=new Size(550,570);
        var layout=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=1,RowCount=3,BackColor=AppTheme.Canvas};
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute,74));layout.RowStyles.Add(new RowStyle(SizeType.Absolute,98));layout.RowStyles.Add(new RowStyle(SizeType.Percent,100));
        var header=new Panel {Dock=DockStyle.Fill};
        header.Controls.Add(new Label {Text="Tổng quan doanh thu",Font=AppTheme.Title,ForeColor=AppTheme.Ink,Location=new Point(0,0),AutoSize=true});
        range.Font=AppTheme.Small;range.ForeColor=AppTheme.Muted;range.Location=new Point(2,38);range.AutoSize=true;header.Controls.Add(range);
        period.Width=170;period.Dock=DockStyle.None;period.Anchor=AnchorStyles.Top|AnchorStyles.Right;period.Font=AppTheme.Body;header.Controls.Add(period);
        header.Resize+=(_,_)=>period.Location=new Point(Math.Max(0,header.Width-period.Width),6);
        period.SelectedIndexChanged+=(_,_)=>PeriodChanged?.Invoke(this,EventArgs.Empty);
        var metrics=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=3,Margin=new Padding(0,0,0,14)};
        foreach(var label in new[]{total,average,best})metrics.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100f/3));
        metrics.Controls.Add(Metric("TỔNG DOANH THU",total),0,0);metrics.Controls.Add(Metric("TRUNG BÌNH / NGÀY",average),1,0);metrics.Controls.Add(Metric("NGÀY CAO NHẤT",best),2,0);
        var plots=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=2,Margin=Padding.Empty};
        plots.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,66));plots.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,34));
        trend.Dock=DockStyle.Fill;categories.Dock=DockStyle.Fill;trend.Margin=new Padding(0,0,8,0);categories.Margin=new Padding(8,0,0,0);
        plots.Controls.Add(trend,0,0);plots.Controls.Add(categories,1,0);
        layout.Controls.Add(header,0,0);layout.Controls.Add(metrics,0,1);layout.Controls.Add(plots,0,2);Controls.Add(layout);
    }
    private static Panel Metric(string title,Label value)
    {
        var panel=new Panel {Dock=DockStyle.Fill,BackColor=Color.White,Margin=new Padding(0,0,12,0)};
        panel.Controls.Add(new Label {Text=title,Font=AppTheme.Small,ForeColor=AppTheme.Muted,AutoSize=true,Location=new Point(14,12)});
        value.Font=AppTheme.Title;value.ForeColor=AppTheme.Ink;value.Location=new Point(12,35);value.Height=36;value.AutoEllipsis=true;
        panel.Resize+=(_,_)=>value.Width=Math.Max(20,panel.Width-24);panel.Controls.Add(value);return panel;
    }
    public void SetData(DashboardData data)
    {
        var rows=data.Trend??[];var sum=rows.Sum(r=>r.Total);
        total.Text=$"{sum:N0} đ";average.Text=$"{(rows.Count==0?0:sum/rows.Count):N0} đ";
        var highest=rows.OrderByDescending(r=>r.Total).FirstOrDefault();
        best.Text=highest is {Total:>0}?$"{highest.Day:dd/MM} · {RevenuePlot.Short(highest.Total)}":"Chưa phát sinh";
        range.Text=rows.Count>0?$"{rows[0].Day:dd/MM/yyyy} – {rows[^1].Day:dd/MM/yyyy}  •  Theo ngày ghi nhận":"Theo ngày ghi nhận";
        trend.SetData(rows,[]);categories.SetData([],data.Revenue);categories.Caption=$"Cơ cấu hôm nay · {data.ServerNow:dd/MM}";
    }
}

// Native vector rendering: sharp at different DPI, no network or chart package required.
internal sealed class RevenuePlot : Control
{
    private readonly bool breakdown;
    private IReadOnlyList<RevenueDay> days=[];
    private IReadOnlyList<RevenueItem> items=[];
    private readonly ToolTip tooltip=new() {InitialDelay=150,ReshowDelay=50,AutoPopDelay=10000};
    private readonly List<(RectangleF Bounds,string Text)> hits=[];
    private string? hovered;
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public string Caption {get;set;}="Cơ cấu hôm nay";
    public RevenuePlot(bool breakdown)
    {
        this.breakdown=breakdown;BackColor=Color.White;Font=AppTheme.Body;
        SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer|ControlStyles.ResizeRedraw,true);
        AccessibleRole=AccessibleRole.Chart;
    }
    public static string Short(decimal value)=>value>=1_000_000_000?$"{value/1_000_000_000:0.#} tỷ":value>=1_000_000?$"{value/1_000_000:0.#} tr":value>=1000?$"{value/1000:0.#} nghìn":$"{value:0}";
    public void SetData(IReadOnlyList<RevenueDay> days,IReadOnlyList<RevenueItem> items)
    {
        this.days=days;this.items=items;
        AccessibleName=breakdown?"Cơ cấu doanh thu hôm nay":"Biểu đồ cột chồng doanh thu theo ngày";
        AccessibleDescription=breakdown?string.Join("; ",items.Select(x=>$"{x.Category}: {x.Total:N0} đồng")):string.Join("; ",days.Select(x=>$"{x.Day:dd/MM}: {x.Total:N0} đồng"));
        Invalidate();
    }
    protected override void Dispose(bool disposing){if(disposing)tooltip.Dispose();base.Dispose(disposing);}
    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);var hit=hits.FirstOrDefault(h=>h.Bounds.Contains(e.Location));
        if(hit.Text!=hovered){hovered=hit.Text;tooltip.SetToolTip(this,hovered??"");}
    }
    protected override void OnMouseLeave(EventArgs e){base.OnMouseLeave(e);tooltip.SetToolTip(this,"");hovered=null;}
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);hits.Clear();if(Width<160||Height<140)return;
        e.Graphics.SmoothingMode=SmoothingMode.AntiAlias;
        DrawLabel(e.Graphics,breakdown?Caption:"Doanh thu theo ngày",AppTheme.Bold,AppTheme.Ink,new Rectangle(18,16,Width-36,26));
        if(breakdown)DrawCategories(e.Graphics);else DrawColumns(e.Graphics);
    }
    private static void DrawLabel(Graphics g,string value,Font font,Color color,Rectangle rect,TextFormatFlags align=TextFormatFlags.Left)
        =>TextRenderer.DrawText(g,value,font,rect,color,align|TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis|TextFormatFlags.NoPrefix);
    private void Empty(Graphics g)=>DrawLabel(g,"Chưa có doanh thu trong kỳ",AppTheme.Body,AppTheme.Muted,new Rectangle(12,Height/2-18,Width-24,36),TextFormatFlags.HorizontalCenter);
    private void DrawColumns(Graphics g)
    {
        var colors=new[]{AppTheme.Blue,AppTheme.Teal,AppTheme.Amber};var labels=new[]{"Tiền phòng","Dịch vụ","Cọc không hoàn"};
        var legendWidth=(Width-36)/3;
        for(var i=0;i<3;i++)
        {
            using var brush=new SolidBrush(colors[i]);g.FillRectangle(brush,20+i*legendWidth,55,9,9);
            DrawLabel(g,labels[i],AppTheme.Small,AppTheme.Muted,new Rectangle(33+i*legendWidth,46,legendWidth-15,28));
        }
        var plot=new RectangleF(69,104,Math.Max(40,Width-92),Math.Max(35,Height-163));
        var maximum=days.Select(x=>x.Total).DefaultIfEmpty().Max();
        var scale=maximum>0?(decimal)Math.Pow(10,Math.Floor(Math.Log10((double)maximum))):100000;
        var top=Math.Max(scale,decimal.Ceiling(maximum/scale/0.5m)*scale*0.5m);
        using var grid=new Pen(AppTheme.Border);
        for(var i=0;i<=4;i++)
        {
            var y=plot.Bottom-plot.Height*i/4;
            g.DrawLine(grid,plot.Left,y,plot.Right,y);
            DrawLabel(g,Short(top*i/4),AppTheme.Small,AppTheme.Muted,new Rectangle(0,(int)y-12,61,24),TextFormatFlags.Right);
        }
        DrawLabel(g,"VNĐ",AppTheme.Small,AppTheme.Muted,new Rectangle(18,78,50,22));
        if(maximum==0){Empty(g);return;}
        var step=plot.Width/Math.Max(1,days.Count);var barWidth=Math.Min(44,step*0.6f);
        for(var i=0;i<days.Count;i++)
        {
            var day=days[i];var x=plot.Left+i*step+(step-barWidth)/2;var y=plot.Bottom;
            var values=new[]{day.Rooms,day.Services,day.Forfeits};
            for(var j=0;j<3;j++)
            {
                var height=(float)(values[j]/top)*plot.Height;y-=height;
                if(height>0){using var brush=new SolidBrush(colors[j]);g.FillRectangle(brush,x,y,barWidth,height);}
            }
            if(day.Total>0 && days.Count<=7)DrawLabel(g,Short(day.Total),AppTheme.Small,AppTheme.Ink,new Rectangle((int)(x+barWidth/2)-43,(int)y-27,86,24),TextFormatFlags.HorizontalCenter);
            if(days.Count<=7||i%5==0||i==days.Count-1)DrawLabel(g,day.Day.ToString("dd/MM"),AppTheme.Small,AppTheme.Muted,new Rectangle((int)(x+barWidth/2)-26,(int)plot.Bottom+9,52,26),TextFormatFlags.HorizontalCenter);
            hits.Add((new RectangleF(plot.Left+i*step,plot.Top,step,plot.Height),$"{day.Day:dd/MM/yyyy}\nTiền phòng: {day.Rooms:N0} đ\nDịch vụ: {day.Services:N0} đ\nCọc không hoàn: {day.Forfeits:N0} đ\nTổng: {day.Total:N0} đ"));
        }
        DrawLabel(g,"Cọc không hoàn được ghi nhận riêng; không phải tiền thu mới.",AppTheme.Small,AppTheme.Muted,new Rectangle(16,Height-27,Width-32,24));
    }
    private void DrawCategories(Graphics g)
    {
        var all=items.Where(x=>x.Total>0).OrderByDescending(x=>x.Total).ToList();var total=all.Sum(x=>x.Total);
        if(total==0){Empty(g);return;}
        var rows=all.Take(4).ToList();if(all.Count>4)rows.Add(new RevenueItem("Hạng mục khác",all.Skip(4).Sum(x=>x.Total)));
        var rowHeight=Math.Min(75,Math.Max(60,(Height-80)/Math.Max(1,rows.Count)));
        for(var i=0;i<rows.Count;i++)
        {
            var row=rows[i];var y=60+i*rowHeight;
            var color=row.Category=="Tiền phòng"?AppTheme.Blue:row.Category.StartsWith("Cọc",StringComparison.Ordinal)?AppTheme.Amber:AppTheme.Teal;
            DrawLabel(g,row.Category,AppTheme.Small,AppTheme.Ink,new Rectangle(18,y,Width-36,23));
            DrawLabel(g,$"{row.Total:N0} đ  ·  {row.Total/total:0.#%}",AppTheme.Small,AppTheme.Muted,new Rectangle(18,y+22,Width-36,22));
            using var track=new SolidBrush(AppTheme.Canvas);using var fill=new SolidBrush(color);
            g.FillRectangle(track,20,y+48,Width-40,5);g.FillRectangle(fill,20,y+48,(float)(row.Total/total)*(Width-40),5);
            hits.Add((new RectangleF(14,y,Width-28,rowHeight),$"{row.Category}\n{row.Total:N0} đ ({row.Total/total:0.#%})"));
        }
    }
}

