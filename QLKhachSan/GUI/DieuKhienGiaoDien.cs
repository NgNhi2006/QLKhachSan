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
        var color=!Enabled?Color.FromArgb(151,167,154):hovered?AppTheme.Navy:AppTheme.Blue;
        using var shape=AppTheme.Rounded(new RectangleF(1,1,Width-3,Height-3),12);
        using var fill=new SolidBrush(color);g.FillPath(fill,shape);
        TextRenderer.DrawText(g,Text,AppTheme.Bold,new Rectangle(10,1,Width-20,Height-2),Color.White,
            TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis|TextFormatFlags.NoPrefix);
        if(Focused)ControlPaint.DrawFocusRectangle(g,new Rectangle(6,6,Width-12,Height-12));
    }
}

internal sealed class PaymentMethodChoice : RadioButton
{
    private Image? icon;
    private string iconKind="money";
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public string IconKind {get=>iconKind;set{iconKind=value;icon?.Dispose();icon=null;Invalidate();}}
    public PaymentMethodChoice()
    {
        SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|
            ControlStyles.OptimizedDoubleBuffer|ControlStyles.ResizeRedraw,true);
        Appearance=Appearance.Button;AutoSize=false;Height=44;Cursor=Cursors.Hand;
    }
    protected override void OnCheckedChanged(EventArgs e){icon?.Dispose();icon=null;base.OnCheckedChanged(e);Invalidate();}
    protected override void OnMouseEnter(EventArgs e){base.OnMouseEnter(e);Invalidate();}
    protected override void OnMouseLeave(EventArgs e){base.OnMouseLeave(e);Invalidate();}
    protected override void OnPaint(PaintEventArgs e)
    {
        var g=e.Graphics;g.Clear(Parent?.BackColor??Color.White);g.SmoothingMode=SmoothingMode.AntiAlias;
        using var shape=AppTheme.Rounded(new RectangleF(1,1,Width-3,Height-3),9);
        using var fill=new SolidBrush(Checked?Color.FromArgb(230,241,231):Color.White);
        using var border=new Pen(Checked?AppTheme.Blue:AppTheme.Border,Checked?1.6F:1F);
        g.FillPath(fill,shape);g.DrawPath(border,shape);
        icon??=UiIcons.Create(IconKind,Checked?AppTheme.Blue:AppTheme.Muted,20);
        g.DrawImage(icon,13,(Height-20)/2,20,20);
        TextRenderer.DrawText(g,Text,AppTheme.Body,new Rectangle(43,2,Width-78,Height-4),AppTheme.Ink,
            TextFormatFlags.Left|TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis|TextFormatFlags.NoPrefix);
        using var ring=new Pen(Checked?AppTheme.Blue:AppTheme.Border,1.4F);
        g.DrawEllipse(ring,Width-26,(Height-16)/2,15,15);
        if(Checked){using var dot=new SolidBrush(AppTheme.Blue);g.FillEllipse(dot,Width-22,(Height-8)/2,8,8);}
    }
    protected override void Dispose(bool disposing){if(disposing)icon?.Dispose();base.Dispose(disposing);}
}
