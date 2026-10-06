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

internal sealed class DashboardMenuHero : Panel
{
    private readonly Image? roomImage;
    public DashboardMenuHero()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true);
        using var stream=typeof(DashboardMenuHero).Assembly.GetManifestResourceStream("QLKhachSan.Assets.hotel-room.jpg");
        if(stream is not null){using var decoded=Image.FromStream(stream);roomImage=new Bitmap(decoded);}
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        e.Graphics.Clear(Parent?.BackColor ?? AppTheme.Canvas);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        using var shape = AppTheme.Rounded(new RectangleF(0, 0, Width - 1, Height - 1), 17);
        using var background = new LinearGradientBrush(ClientRectangle,
            Color.FromArgb(43, 42, 68), Color.FromArgb(85, 73, 121), LinearGradientMode.Horizontal);
        e.Graphics.FillPath(background, shape);
        if(roomImage is not null && Width>700)
        {
            var state=e.Graphics.Save();
            e.Graphics.SetClip(shape);
            var imageArea=new Rectangle((int)(Width*0.60),0,(int)(Width*0.40),Height);
            var scale=Math.Max((float)imageArea.Width/roomImage.Width,(float)imageArea.Height/roomImage.Height);
            var imageWidth=(int)(roomImage.Width*scale);
            var imageHeight=(int)(roomImage.Height*scale);
            e.Graphics.DrawImage(roomImage,new Rectangle(imageArea.Left+(imageArea.Width-imageWidth)/2,
                (imageArea.Height-imageHeight)/2,imageWidth,imageHeight));
            using var photoTint=new SolidBrush(Color.FromArgb(65,43,42,68));
            e.Graphics.FillRectangle(photoTint,imageArea);
            using var fade=new LinearGradientBrush(new Rectangle(imageArea.Left-115,0,115,Height),
                Color.FromArgb(255,67,60,100),Color.FromArgb(0,67,60,100),LinearGradientMode.Horizontal);
            e.Graphics.FillRectangle(fade,imageArea.Left-115,0,115,Height);
            e.Graphics.Restore(state);
        }
        using var decoration = new Pen(Color.FromArgb(31, 255, 255, 255), 2);
        e.Graphics.DrawEllipse(decoration, Width - 184, -95, 265, 265);
        e.Graphics.DrawEllipse(decoration, Width - 115, -31, 177, 177);
    }
    protected override void Dispose(bool disposing){if(disposing)roomImage?.Dispose();base.Dispose(disposing);}
}

internal sealed class DashboardMenuCard : Button
{
    private bool hovered;
    private Image? icon;
    private string iconKind = "bed";
    private Color accent = AppTheme.Blue;

    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public string Title { get; set; } = "";
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public string Description { get; set; } = "";
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public int FunctionCount { get; set; }
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public string IconKind { get => iconKind; set { iconKind = value; icon?.Dispose(); icon = null; Invalidate(); } }
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public Color Accent { get => accent; set { accent = value; icon?.Dispose(); icon = null; Invalidate(); } }

    public DashboardMenuCard()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        Cursor = Cursors.Hand;
        TabStop = true;
    }

    protected override void OnMouseEnter(EventArgs e) { hovered = true; Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { hovered = false; Invalidate(); base.OnMouseLeave(e); }
    protected override void OnGotFocus(EventArgs e) { Invalidate(); base.OnGotFocus(e); }
    protected override void OnLostFocus(EventArgs e) { Invalidate(); base.OnLostFocus(e); }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.Clear(Parent?.BackColor ?? AppTheme.Canvas);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        using var shape = AppTheme.Rounded(new RectangleF(1, 1, Width - 3, Height - 3), 14);
        using var fill = new SolidBrush(hovered ? Color.FromArgb(251, 253, 255) : Color.White);
        using var border = new Pen(hovered || Focused ? Accent : AppTheme.Border, hovered || Focused ? 1.5F : 1F);
        g.FillPath(fill, shape);
        g.DrawPath(border, shape);
        using var iconShape = AppTheme.Rounded(new RectangleF(18, 17, 43, 43), 11);
        using var iconFill = new SolidBrush(Color.FromArgb(25, Accent));
        g.FillPath(iconFill, iconShape);
        icon ??= UiIcons.Create(IconKind, Accent, 24);
        g.DrawImage(icon, 27, 26, 24, 24);
        using var arrowFont = new Font("Segoe UI", 16F);
        using var titleFont = new Font("Segoe UI Semibold", 12F, FontStyle.Bold);
        TextRenderer.DrawText(g, "↗", arrowFont,
            new Rectangle(Width - 45, 21, 25, 30), hovered ? Accent : AppTheme.Muted,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        TextRenderer.DrawText(g, Title, titleFont,
            new Rectangle(18, 66, Width - 36, 27), AppTheme.Ink,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        TextRenderer.DrawText(g, Description, AppTheme.Small,
            new Rectangle(18, 94, Width - 36, 21), AppTheme.Muted,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        TextRenderer.DrawText(g, $"{FunctionCount} chức năng", AppTheme.Small,
            new Rectangle(18, 115, Width - 36, 19), Accent,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) icon?.Dispose();
        base.Dispose(disposing);
    }
}
