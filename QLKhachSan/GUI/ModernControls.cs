using System.Drawing.Drawing2D;

namespace QLKhachSan.GUI;

internal sealed class RoundedSurface : TableLayoutPanel
{
    private const int CornerRadius=14;

    public RoundedSurface()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer|
            ControlStyles.ResizeRedraw,true);
        BackColor=Color.White;
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        var old=Region;
        using var shape=AppTheme.Rounded(new RectangleF(0,0,Math.Max(1,Width),Math.Max(1,Height)),CornerRadius);
        Region=new Region(shape);
        old?.Dispose();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.SmoothingMode=SmoothingMode.AntiAlias;
        using var border=new Pen(AppTheme.Border,1);
        using var shape=AppTheme.Rounded(new RectangleF(.5F,.5F,Width-1.5F,Height-1.5F),CornerRadius);
        e.Graphics.DrawPath(border,shape);
    }
}

internal sealed class RoundedActionButton : Button
{
    private bool hovered;

    public RoundedActionButton()
    {
        SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|
            ControlStyles.OptimizedDoubleBuffer|ControlStyles.ResizeRedraw,true);
        FlatStyle=FlatStyle.Flat;
        FlatAppearance.BorderSize=0;
        Cursor=Cursors.Hand;
    }

    protected override void OnMouseEnter(EventArgs e){hovered=true;Invalidate();base.OnMouseEnter(e);}
    protected override void OnMouseLeave(EventArgs e){hovered=false;Invalidate();base.OnMouseLeave(e);}
    protected override void OnEnabledChanged(EventArgs e){Invalidate();base.OnEnabledChanged(e);}

    protected override void OnPaint(PaintEventArgs e)
    {
        var g=e.Graphics;
        g.Clear(Parent?.BackColor??AppTheme.Canvas);
        g.SmoothingMode=SmoothingMode.AntiAlias;
        var color=!Enabled?Color.FromArgb(153,171,199):hovered?Color.FromArgb(44,79,187):AppTheme.Blue;
        using var shape=AppTheme.Rounded(new RectangleF(1,1,Width-3,Height-3),12);
        using var fill=new SolidBrush(color);g.FillPath(fill,shape);
        TextRenderer.DrawText(g,Text,AppTheme.Bold,new Rectangle(10,1,Width-20,Height-2),Color.White,
            TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis|TextFormatFlags.NoPrefix);
        if(Focused)ControlPaint.DrawFocusRectangle(g,new Rectangle(6,6,Width-12,Height-12));
    }
}
