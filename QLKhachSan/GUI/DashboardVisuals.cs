using System.Drawing.Drawing2D;

namespace QLKhachSan.GUI;

internal sealed class DashboardLogo : Control
{
    private readonly Image icon = UiIcons.Create("bed", Color.White, 28);

    public DashboardLogo()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        Size = new Size(42, 42);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.Clear(Parent?.BackColor ?? AppTheme.Navy);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        using var shape = AppTheme.Rounded(new RectangleF(1, 1, Width - 3, Height - 3), 11);
        using var fill = new SolidBrush(Color.FromArgb(39, 91, 113));
        e.Graphics.FillPath(fill, shape);
        e.Graphics.DrawImage(icon, (Width - 28) / 2, (Height - 28) / 2, 28, 28);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) icon.Dispose();
        base.Dispose(disposing);
    }
}

internal sealed class DashboardMetricCard : Panel
{
    private Color accent = AppTheme.Blue;
    private bool hovered;

    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public Color Accent
    {
        get => accent;
        set { accent = value; Invalidate(); }
    }

    public void SetHovered(bool value)
    {
        if (hovered == value) return;
        hovered = value;
        Invalidate();
    }

    public DashboardMetricCard()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        BackColor = Color.White;
    }

    protected override void OnResize(EventArgs eventargs)
    {
        base.OnResize(eventargs);
        Invalidate();
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        e.Graphics.Clear(Parent?.BackColor ?? AppTheme.Canvas);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        using var shape = AppTheme.Rounded(new RectangleF(0, 0, Width - 1, Height - 1), 12);
        using var fill = new SolidBrush(Color.White);
        e.Graphics.FillPath(fill, shape);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        using var shape = AppTheme.Rounded(new RectangleF(1, 1, Width - 3, Height - 3), 11);
        using var border = new Pen(hovered ? accent : AppTheme.Border, hovered ? 1.6F : 1F);
        e.Graphics.DrawPath(border, shape);
        using var highlight = new Pen(accent, 3F) { StartCap = LineCap.Round, EndCap = LineCap.Round };
        e.Graphics.DrawLine(highlight, 16, 2, 47, 2);
    }
}
