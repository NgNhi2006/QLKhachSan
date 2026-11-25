using System.Drawing.Drawing2D;
using QLKhachSan.DTO;

namespace QLKhachSan.GUI;

public partial class ucDashboard
{
    private Control BuildRevenueWorkspace()
    {
        var scroll=new Panel {Dock=DockStyle.Fill,AutoScroll=true,BackColor=AppTheme.Canvas};
        var root=new TableLayoutPanel {ColumnCount=1,RowCount=7,BackColor=AppTheme.Canvas,Padding=new Padding(22,8,22,18),Height=1280};
        scroll.Controls.Add(root);
        scroll.Resize+=(_,_)=>root.Width=Math.Max(720,scroll.ClientSize.Width-SystemInformation.VerticalScrollBarWidth);
        root.Width=Math.Max(720,scroll.ClientSize.Width-SystemInformation.VerticalScrollBarWidth);
        root.RowStyles.Add(new RowStyle(SizeType.Absolute,65));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute,130));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute,110));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute,300));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute,270));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute,310));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute,38));
        var filters=new FlowLayoutPanel {Dock=DockStyle.Fill,WrapContents=false,AutoScroll=true,BackColor=AppTheme.Canvas,Padding=new Padding(0,10,0,0)};
        var mode=Ui.Combo(["Theo ngày","Theo tuần","Theo tháng","Theo năm"]);
        mode.Width=160;mode.Height=38;mode.SelectedIndex=2;mode.Margin=new Padding(0,0,10,0);
        var anchor=Ui.DatePicker(ServerNow.Date);anchor.Width=165;anchor.CustomFormat="MM/yyyy";anchor.Margin=new Padding(0,0,10,0);
        var refresh=new Button {Text="XEM DOANH THU",Width=175,Height=38,Margin=new Padding(0,0,10,0)};AppTheme.Button(refresh,true);
        var export=new Button {Text="XUẤT CSV",Width=125,Height=38,Margin=new Padding(0,0,10,0)};AppTheme.Button(export);
        var range=new Label {AutoSize=true,MinimumSize=new Size(260,38),TextAlign=ContentAlignment.MiddleLeft,ForeColor=AppTheme.Muted,Margin=new Padding(8,6,0,0)};
        filters.Controls.AddRange([mode,anchor,refresh,export,range]);root.Controls.Add(filters,0,0);

        var lifetime=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=4,RowCount=1,BackColor=AppTheme.Navy,Margin=new Padding(0,0,0,12),Padding=new Padding(20,14,16,12)};
        lifetime.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,43));
        for(var i=0;i<3;i++)lifetime.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,19));
        Label LifetimeMetric(string title,int column,bool primary=false)
        {
            var block=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=1,RowCount=2,BackColor=AppTheme.Navy,Margin=new Padding(column==0?0:10,0,4,0)};
            block.RowStyles.Add(new RowStyle(SizeType.Absolute,27));block.RowStyles.Add(new RowStyle(SizeType.Percent,100));
            block.Controls.Add(new Label {Text=title,Dock=DockStyle.Fill,ForeColor=Color.FromArgb(207,221,214),Font=AppTheme.Small},0,0);
            var value=new Label {Text="Đang tải...",Dock=DockStyle.Fill,ForeColor=primary?Color.White:Color.FromArgb(236,208,149),
                Font=AppTheme.TextFont(primary?24F:16F,FontStyle.Bold),AutoEllipsis=true};
            block.Controls.Add(value,0,1);lifetime.Controls.Add(block,column,0);return value;
        }
        var lifetimeNet=LifetimeMetric("DOANH THU KỲ ĐANG XEM",0,true);
        var lifetimeGross=LifetimeMetric("PHÁT SINH TRƯỚC GIẢM TRỪ",1);
        var lifetimeAdjustments=LifetimeMetric("GIẢM TRỪ",2);
        var lifetimeRoom=LifetimeMetric("TRONG ĐÓ: TIỀN PHÒNG",3);
        root.Controls.Add(lifetime,0,1);

        var cards=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=4,RowCount=1};
        for(var i=0;i<4;i++)cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,25));
        Label Card(string title,int index,Color color)
        {
            var card=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=1,RowCount=2,BackColor=Color.White,Margin=new Padding(index==0?0:5,0,index==3?0:5,10),Padding=new Padding(15,10,8,5)};
            card.RowStyles.Add(new RowStyle(SizeType.Absolute,26));card.RowStyles.Add(new RowStyle(SizeType.Percent,100));
            card.Controls.Add(new Label {Text=title,Dock=DockStyle.Fill,Font=AppTheme.Small,ForeColor=AppTheme.Muted},0,0);
            var value=new Label {Dock=DockStyle.Fill,Font=AppTheme.TextFont(17F,FontStyle.Bold),ForeColor=color,AutoEllipsis=true};
            card.Controls.Add(value,0,1);cards.Controls.Add(card,index,0);return value;
        }
        var total=Card("BÌNH QUÂN MỖI NGÀY",0,AppTheme.Blue);
        var room=Card("TIỀN PHÒNG",1,AppTheme.Teal);
        var services=Card("DỊCH VỤ VÀ PHỤ THU",2,AppTheme.Amber);
        var other=Card("CỌC KHÔNG HOÀN",3,AppTheme.Ink);
        root.Controls.Add(cards,0,2);
        var charts=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=2,RowCount=1,Margin=new Padding(0,0,0,10)};
        charts.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,67));charts.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,33));
        var lineChart=new RevenueTimelineChart(true) {Dock=DockStyle.Fill,Margin=new Padding(0,0,7,0)};
        var donut=new RevenueDonutChart {Dock=DockStyle.Fill,Margin=new Padding(7,0,0,0)};
        charts.Controls.Add(lineChart,0,0);charts.Controls.Add(donut,1,0);root.Controls.Add(charts,0,3);
        var barChart=new RevenueTimelineChart(false) {Dock=DockStyle.Fill,Margin=new Padding(0,0,0,10)};
        root.Controls.Add(barChart,0,4);
        var tabs=new TabControl {Dock=DockStyle.Fill,Font=AppTheme.Bold,Margin=Padding.Empty};
        var byCategory=new TabPage("Nguồn doanh thu") {BackColor=Color.White,Padding=new Padding(8)};
        var byItem=new TabPage("Chi tiết dịch vụ / hạng mục") {BackColor=Color.White,Padding=new Padding(8)};
        tabs.TabPages.AddRange([byCategory,byItem]);
        var categoryGrid=Ui.Grid();categoryGrid.Dock=DockStyle.Fill;categoryGrid.AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill;byCategory.Controls.Add(categoryGrid);
        var itemGrid=Ui.Grid();itemGrid.Dock=DockStyle.Fill;itemGrid.AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill;byItem.Controls.Add(itemGrid);
        root.Controls.Add(tabs,0,5);
        root.Controls.Add(new Label {Text="Tất cả số liệu trên màn hình tính theo kỳ đang chọn. Doanh thu = phát sinh trước giảm trừ − giảm trừ; chưa trừ giá vốn và chi phí. Cọc đang giữ chưa tính vào doanh thu.",Dock=DockStyle.Fill,ForeColor=AppTheme.Muted,Font=AppTheme.Small,Padding=new Padding(2,9,0,0)},0,6);

        List<RevenueEvent> currentEvents=[];
        DateTime currentFrom=ServerNow.Date,currentThrough=ServerNow.Date;
        var periodTitle=(Label)((TableLayoutPanel)lifetime.GetControlFromPosition(0,0)!)
            .GetControlFromPosition(0,0)!;
        var averageTitle=(Label)((TableLayoutPanel)cards.GetControlFromPosition(0,0)!)
            .GetControlFromPosition(0,0)!;
        var requestVersion=0;
        async Task LoadRevenue()
        {
            var selected=anchor.Value.Date;
            var chosen=mode.SelectedIndex;
            var request=++requestVersion;
            var from=chosen switch
            {
                0=>selected,
                1=>selected.AddDays(-((int)selected.DayOfWeek+6)%7),
                2=>new DateTime(selected.Year,selected.Month,1),
                _=>new DateTime(selected.Year,1,1)
            };
            var until=chosen switch {0=>from.AddDays(1),1=>from.AddDays(7),2=>from.AddMonths(1),_=>from.AddYears(1)};
            var previousFrom=chosen switch {0=>from.AddDays(-1),1=>from.AddDays(-7),2=>from.AddMonths(-1),_=>from.AddYears(-1)};
            var events=await service.RevenueEventsAsync(from,until);
            var previous=await service.RevenueEventsAsync(previousFrom,from);
            if(root.IsDisposed || request!=requestVersion)return;
            currentEvents=events;currentFrom=from;currentThrough=until.AddDays(-1);
            var categories=events.GroupBy(x=>x.Category).Select(g=>new RevenueItem(g.Key,g.Sum(x=>x.Amount))).OrderByDescending(x=>x.Total).ToList();
            var roomAmount=categories.Where(x=>x.Category=="Tiền phòng").Sum(x=>x.Total);
            var forfeit=categories.Where(x=>x.Category.StartsWith("Cọc không hoàn",StringComparison.Ordinal)).Sum(x=>x.Total);
            var reduction=categories.Where(x=>x.Total<0).Sum(x=>x.Total);
            var serviceAmount=categories.Sum(x=>x.Total)-roomAmount-forfeit-reduction;
            var periodTotal=events.Sum(x=>x.Amount);
            var gross=events.Where(x=>x.Amount>0).Sum(x=>x.Amount);
            lifetimeNet.Text=$"{periodTotal:N0} đ";
            lifetimeGross.Text=$"{gross:N0} đ";
            lifetimeAdjustments.Text=$"{Math.Abs(reduction):N0} đ";
            lifetimeRoom.Text=$"{roomAmount:N0} đ";
            periodTitle.Text=$"DOANH THU {mode.SelectedItem?.ToString()?.ToUpperInvariant()}";
            var units=chosen switch {0=>24,1=>7,2=>(until-from).Days,_=>12};
            averageTitle.Text=chosen switch {0=>"BÌNH QUÂN MỖI GIỜ",3=>"BÌNH QUÂN MỖI THÁNG",_=>"BÌNH QUÂN MỖI NGÀY"};
            total.Text=$"{periodTotal/units:N0} đ";room.Text=$"{roomAmount:N0} đ";
            services.Text=$"{serviceAmount:N0} đ";other.Text=$"{forfeit:N0} đ";
            range.Text=chosen switch
            {
                0=>$"Ngày {from:dd/MM/yyyy}",
                1=>$"Tuần {from:dd/MM/yyyy} – {until.AddDays(-1):dd/MM/yyyy}",
                2=>$"Tháng {from:MM/yyyy} · {from:dd/MM} – {until.AddDays(-1):dd/MM/yyyy}",
                _=>$"Năm {from:yyyy} · {from:dd/MM} – {until.AddDays(-1):dd/MM/yyyy}"
            };
            var points=chosen switch
            {
                0=>Enumerable.Range(0,24).Select(i=>(Key:from.AddHours(i),Label:$"{i:00}h")).ToList(),
                1=>Enumerable.Range(0,7).Select(i=>(Key:from.AddDays(i),Label:i==6?"CN":$"T{i+2}")).ToList(),
                2=>Enumerable.Range(0,(until-from).Days).Select(i=>(Key:from.AddDays(i),Label:$"{i+1:00}")).ToList(),
                _=>Enumerable.Range(0,12).Select(i=>(Key:from.AddMonths(i),Label:$"T{i+1}")).ToList()
            };
            DateTime Bucket(DateTime value)=>chosen switch
            {
                0=>new DateTime(value.Year,value.Month,value.Day,value.Hour,0,0),
                1=>value.Date,
                2=>value.Date,
                _=>new DateTime(value.Year,value.Month,1)
            };
            var amounts=events.GroupBy(x=>Bucket(x.At)).ToDictionary(g=>g.Key,g=>g.Sum(x=>x.Amount));
            var timeline=points.Select(p=>(p.Label,amounts.GetValueOrDefault(p.Key))).ToList();
            lineChart.SetData(timeline,"XU HƯỚNG DOANH THU");
            string PeriodLabel(DateTime day)=>chosen switch
            {
                0=>day.ToString("dd/MM"),1=>$"{day:dd/MM}",2=>day.ToString("MM/yyyy"),_=>day.ToString("yyyy")
            };
            barChart.SetData(new List<(string Label,decimal Amount)>
            {
                (PeriodLabel(previousFrom),previous.Sum(x=>x.Amount)),
                (PeriodLabel(from),periodTotal)
            },"SO SÁNH VỚI KỲ TRƯỚC");
            donut.SetData(categories);
            categoryGrid.DataSource=categories.Select(x=>new {Nguồn=x.Category,DoanhThu=x.Total}).ToList();
            if(categoryGrid.Columns["DoanhThu"] is { } amount)amount.DefaultCellStyle.Format="N0";
            itemGrid.DataSource=events.GroupBy(x=>new {x.Category,x.Item})
                .Select(g=>new {Nguồn=g.Key.Category,HạngMục=g.Key.Item,SốLượng=g.Sum(x=>x.Quantity),DoanhThu=g.Sum(x=>x.Amount)})
                .OrderByDescending(x=>x.DoanhThu).ToList();
            if(itemGrid.Columns["SốLượng"] is { } quantity)quantity.DefaultCellStyle.Format="N0.##";
            if(itemGrid.Columns["DoanhThu"] is { } itemAmount)itemAmount.DefaultCellStyle.Format="N0";
            byCategory.Text=$"Nguồn doanh thu ({categories.Count})";
            byItem.Text=$"Chi tiết dịch vụ / hạng mục ({itemGrid.RowCount})";
        }
        async Task Refresh()
        {
            refresh.Enabled=false;
            try
            {
                await LoadRevenue();
            }
            catch(Exception ex){if(!IsDisposed)Ui.Error(this,ex);}
            finally{if(!refresh.IsDisposed)refresh.Enabled=true;}
        }
        mode.SelectedIndexChanged+=(_,_)=>
        {
            anchor.CustomFormat=mode.SelectedIndex switch {0 or 1=>"dd/MM/yyyy",2=>"MM/yyyy",_=>"yyyy"};
            _=Refresh();
        };
        anchor.ValueChanged+=(_,_)=>_=Refresh();refresh.Click+=(_,_)=>_=Refresh();
        export.Click+=(_,_)=>
        {
            var rows=currentEvents.GroupBy(x=>new {x.Category,x.Item}).Select(g=>new object?[]
                {g.Key.Category,g.Key.Item,g.Sum(x=>x.Quantity),g.Sum(x=>x.Amount)})
                .OrderByDescending(x=>(decimal)x[3]!).ToArray();
            ExportReport("doanh-thu-theo-hang-muc","DOANH THU THEO HẠNG MỤC",currentFrom,currentThrough,
                ["Nguồn doanh thu","Hạng mục","Số lượng","Doanh thu (VND)"],rows,
                ["TỔNG CỘNG",null,null,currentEvents.Sum(x=>x.Amount)]);
        };
        _=Refresh();
        return scroll;
    }
}

internal sealed class RevenueTimelineChart : Control
{
    private readonly bool lineChart;
    private IReadOnlyList<(string Label,decimal Amount)> points=[];
    private string caption="Doanh thu theo ngày";
    private readonly ToolTip tips=new() {InitialDelay=100};
    public RevenueTimelineChart(bool lineChart)
    {
        this.lineChart=lineChart;
        BackColor=Color.White;Font=AppTheme.Body;AccessibleRole=AccessibleRole.Chart;
        SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer|ControlStyles.ResizeRedraw,true);
    }
    public void SetData(IReadOnlyList<(string Label,decimal Amount)> values,string title)
    {
        points=values;caption=title;AccessibleName=title;
        AccessibleDescription=string.Join("; ",values.Select(x=>$"{x.Label}: {x.Amount:N0} đồng"));Invalidate();
    }
    protected override void Dispose(bool disposing){if(disposing)tips.Dispose();base.Dispose(disposing);}
    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);var plot=new RectangleF(76,64,Math.Max(1,Width-102),Math.Max(1,Height-112));
        if(!plot.Contains(e.Location)||points.Count==0){tips.SetToolTip(this,"");return;}
        var index=Math.Clamp((int)((e.X-plot.Left)/(plot.Width/points.Count)),0,points.Count-1);
        tips.SetToolTip(this,$"{points[index].Label}: {points[index].Amount:N0} đ");
    }
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);if(Width<180||Height<150)return;
        var g=e.Graphics;g.SmoothingMode=SmoothingMode.AntiAlias;
        using(var border=new Pen(AppTheme.Border))g.DrawRectangle(border,0,0,Width-1,Height-1);
        TextRenderer.DrawText(g,caption,AppTheme.Bold,new Rectangle(18,15,Width-36,30),AppTheme.Ink);
        var plot=new RectangleF(76,64,Width-102,Height-112);
        var max=Math.Max(0,points.Select(x=>x.Amount).DefaultIfEmpty(0).Max());
        var min=Math.Min(0,points.Select(x=>x.Amount).DefaultIfEmpty(0).Min());
        if(max==min)max=1;
        var span=max-min;
        float Y(decimal amount)=>plot.Top+(float)((max-amount)/span)*plot.Height;
        var zero=Y(0);
        using var line=new Pen(AppTheme.Border);
        for(var i=0;i<=4;i++)
        {
            var value=max-span*i/4;var y=Y(value);g.DrawLine(line,plot.Left,y,plot.Right,y);
            TextRenderer.DrawText(g,RevenuePlot.Short(value),AppTheme.Small,new Rectangle(5,(int)y-12,64,25),AppTheme.Muted,TextFormatFlags.Right);
        }
        if(points.Count==0||points.All(x=>x.Amount==0))
        {
            TextRenderer.DrawText(g,"Chưa có doanh thu trong kỳ",AppTheme.Body,new Rectangle((int)plot.Left,(int)plot.Top,(int)plot.Width,(int)plot.Height),AppTheme.Muted,TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter);return;
        }
        var step=plot.Width/points.Count;var bar=Math.Min(38,step*.65f);
        if(lineChart)
        {
            var coords=points.Select((p,i)=>new PointF(plot.Left+(i+.5f)*step,Y(p.Amount))).ToArray();
            if(coords.Length>1)
            {
                var fill=new PointF[coords.Length+2];fill[0]=new PointF(coords[0].X,zero);
                Array.Copy(coords,0,fill,1,coords.Length);fill[^1]=new PointF(coords[^1].X,zero);
                using var tint=new SolidBrush(Color.FromArgb(32,AppTheme.Blue));g.FillPolygon(tint,fill);
                using var stroke=new Pen(AppTheme.Blue,3F) {LineJoin=LineJoin.Round};g.DrawLines(stroke,coords);
            }
            foreach(var p in coords)
            {
                using var halo=new SolidBrush(Color.White);using var dot=new SolidBrush(AppTheme.Blue);
                g.FillEllipse(halo,p.X-6,p.Y-6,12,12);g.FillEllipse(dot,p.X-4,p.Y-4,8,8);
            }
        }
        for(var i=0;i<points.Count;i++)
        {
            var x=plot.Left+i*step+(step-bar)/2;var target=Y(points[i].Amount);
            if(!lineChart)
            {
                using var brush=new SolidBrush(points[i].Amount<0?AppTheme.Danger:AppTheme.Teal);
                if(points[i].Amount!=0)g.FillRectangle(brush,x,Math.Min(zero,target),bar,Math.Abs(zero-target));
            }
            if(points.Count<=12||i%Math.Max(1,points.Count/8)==0||i==points.Count-1)
                TextRenderer.DrawText(g,points[i].Label,AppTheme.Small,new Rectangle((int)(x+bar/2)-30,(int)plot.Bottom+7,60,25),AppTheme.Muted,TextFormatFlags.HorizontalCenter);
        }
    }
}

internal sealed class RevenueDonutChart : Control
{
    private IReadOnlyList<RevenueItem> items=[];
    private readonly ToolTip tips=new() {InitialDelay=100};
    private readonly Color[] colors=[AppTheme.Blue,AppTheme.Teal,AppTheme.Amber,
        Color.FromArgb(112,92,166),Color.FromArgb(55,132,172),Color.FromArgb(183,103,92)];
    public RevenueDonutChart()
    {
        BackColor=Color.White;Font=AppTheme.Body;AccessibleRole=AccessibleRole.Chart;
        SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer|ControlStyles.ResizeRedraw,true);
    }
    public void SetData(IReadOnlyList<RevenueItem> values)
    {
        items=values.Where(x=>x.Total>0).OrderByDescending(x=>x.Total).ToList();
        AccessibleName="Cơ cấu nguồn doanh thu";
        AccessibleDescription=string.Join("; ",items.Select(x=>$"{x.Category}: {x.Total:N0} đồng"));Invalidate();
    }
    protected override void Dispose(bool disposing){if(disposing)tips.Dispose();base.Dispose(disposing);}
    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if(items.Count==0){tips.SetToolTip(this,"");return;}
        var chart=ChartBounds();var cx=chart.Left+chart.Width/2;var cy=chart.Top+chart.Height/2;
        var dx=e.X-cx;var dy=e.Y-cy;var distance=Math.Sqrt(dx*dx+dy*dy);
        if(distance<chart.Width*.34||distance>chart.Width*.5){tips.SetToolTip(this,"");return;}
        var angle=(Math.Atan2(dy,dx)*180/Math.PI+450)%360;
        var total=items.Sum(x=>x.Total);var cursor=0m;
        foreach(var item in items)
        {
            cursor+=item.Total/total*360;
            if((decimal)angle<=cursor){tips.SetToolTip(this,$"{item.Category}: {item.Total:N0} đ ({item.Total/total:P1})");return;}
        }
    }
    private RectangleF ChartBounds()
    {
        var diameter=Math.Min(Width>420?Height-92:Math.Min(Width-36,Height-135),190);
        diameter=Math.Max(70,diameter);
        return new RectangleF(Width>420?22:(Width-diameter)/2,56,diameter,diameter);
    }
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);if(Width<180||Height<150)return;
        var g=e.Graphics;g.SmoothingMode=SmoothingMode.AntiAlias;
        using(var border=new Pen(AppTheme.Border))g.DrawRectangle(border,0,0,Width-1,Height-1);
        TextRenderer.DrawText(g,"CƠ CẤU NGUỒN THU",AppTheme.Bold,new Rectangle(18,15,Width-36,30),AppTheme.Ink);
        if(items.Count==0)
        {
            TextRenderer.DrawText(g,"Chưa có doanh thu",AppTheme.Body,new Rectangle(16,55,Width-32,Height-70),AppTheme.Muted,TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter);return;
        }
        var chart=ChartBounds();var total=items.Sum(x=>x.Total);var angle=-90F;
        for(var i=0;i<items.Count;i++)
        {
            var sweep=(float)(items[i].Total/total)*360F;
            using var slice=new SolidBrush(colors[i%colors.Length]);g.FillPie(slice,chart,angle,sweep);
            angle+=sweep;
        }
        var center=new RectangleF(chart.Left+chart.Width*.16F,chart.Top+chart.Height*.16F,
            chart.Width*.68F,chart.Height*.68F);
        using(var white=new SolidBrush(Color.White))g.FillEllipse(white,center);
        var totalText=$"{total:N0} đ";
        Font? totalFont=null;
        for(var size=11F;size>=6F;size-=.5F)
        {
            var candidate=AppTheme.TextFont(size,FontStyle.Bold);
            if(TextRenderer.MeasureText(g,totalText,candidate,Size.Empty,TextFormatFlags.NoPadding).Width<=center.Width-8 || size==6F)
            {totalFont=candidate;break;}
            candidate.Dispose();
        }
        using(totalFont)
            TextRenderer.DrawText(g,totalText,totalFont!,Rectangle.Round(center),AppTheme.Ink,
                TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter|TextFormatFlags.SingleLine|TextFormatFlags.NoPadding);
        var side=Width>420;var startX=side?(int)chart.Right+14:18;var startY=side?55:(int)chart.Bottom+7;
        var visible=items.Take(side?6:3).ToList();
        for(var i=0;i<visible.Count;i++)
        {
            var y=startY+i*(side?32:22);
            using var marker=new SolidBrush(colors[i%colors.Length]);g.FillEllipse(marker,startX,y+6,9,9);
            var text=$"{visible[i].Category}  {visible[i].Total/total:P0}";
            TextRenderer.DrawText(g,text,AppTheme.Small,new Rectangle(startX+15,y,Math.Max(20,Width-startX-20),24),AppTheme.Ink,TextFormatFlags.EndEllipsis);
        }
        if(items.Count>visible.Count)
            TextRenderer.DrawText(g,$"+ {items.Count-visible.Count} nguồn khác (xem bảng dưới)",AppTheme.Small,
                new Rectangle(startX+15,startY+visible.Count*(side?32:22),Math.Max(20,Width-startX-20),23),AppTheme.Muted,TextFormatFlags.EndEllipsis);
        TextRenderer.DrawText(g,"Tỷ trọng trước giảm trừ",AppTheme.Small,new Rectangle(18,Height-27,Width-36,20),AppTheme.Muted);
    }
}
