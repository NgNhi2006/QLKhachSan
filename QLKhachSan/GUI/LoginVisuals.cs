using System.Drawing.Drawing2D;

namespace QLKhachSan.GUI;

internal sealed class LoginBrandPanel : Panel
{
    private readonly Image? roomImage;
    private readonly Image logo = UiIcons.Create("bed", Color.White, 35);
    private readonly Font brandFont = new("Segoe UI Semibold", 16F, FontStyle.Bold);
    private readonly Font captionFont = new("Segoe UI", 7.5F, FontStyle.Bold);
    private readonly Font eyebrowFont = new("Segoe UI Semibold", 9F, FontStyle.Bold);
    private readonly Font heroFont = new("Segoe UI Semibold", 25F, FontStyle.Bold);
    private readonly Font bodyFont = new("Segoe UI", 10F);
    private readonly Font footerFont = new("Segoe UI", 8F);

    public LoginBrandPanel()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        using var stream=typeof(LoginBrandPanel).Assembly.GetManifestResourceStream("QLKhachSan.Assets.hotel-room.jpg");
        if(stream is not null){using var decoded=Image.FromStream(stream);roomImage=new Bitmap(decoded);}
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        using var background = new LinearGradientBrush(ClientRectangle,
            AppTheme.Navy, Color.FromArgb(38, 91, 110), 125F);
        g.FillRectangle(background, ClientRectangle);

        if(roomImage is not null)
        {
            var scale=Math.Max((float)Width/roomImage.Width,(float)Height/roomImage.Height);
            var imageWidth=(int)Math.Ceiling(roomImage.Width*scale);
            var imageHeight=(int)Math.Ceiling(roomImage.Height*scale);
            g.DrawImage(roomImage,new Rectangle((Width-imageWidth)/2,(Height-imageHeight)/2,imageWidth,imageHeight));
            using var tint=new LinearGradientBrush(ClientRectangle,
                Color.FromArgb(220,AppTheme.Navy),Color.FromArgb(180,AppTheme.Navy),LinearGradientMode.Vertical);
            g.FillRectangle(tint,ClientRectangle);
        }

        using var glow = new SolidBrush(Color.FromArgb(24, 236, 192, 153));
        g.FillEllipse(glow, Width - 180, -110, 340, 340);
        g.FillEllipse(glow, -200, Height - 180, 340, 340);

        // Quiet architectural lines keep the brand panel visually tied to the hotel.
        using var line = new Pen(Color.FromArgb(56, 238, 214, 196), 1F);
        var baseY = Height - 91;
        g.DrawLine(line, 0, baseY, Width, baseY);
        for (var i = 0; i < 5; i++)
        {
            var x = 232 + i * 37;
            var top = baseY - (i % 2 == 0 ? 57 : 79);
            g.DrawRectangle(line, x, top, 29, baseY - top);
            g.DrawLine(line, x + 8, top + 16, x + 20, top + 16);
            g.DrawLine(line, x + 8, top + 33, x + 20, top + 33);
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        using var logoShape = AppTheme.Rounded(new RectangleF(52, 52, 54, 54), 12);
        using var logoFill = new SolidBrush(Color.FromArgb(42, 83, 111));
        g.FillPath(logoFill, logoShape);
        g.DrawImage(logo, 62, 62, 35, 35);
        using var gold=new SolidBrush(AppTheme.Amber);
        g.FillRectangle(gold,54,196,54,4);
        static void Draw(Graphics graphics, string value, Font font, Rectangle area, Color color) =>
            TextRenderer.DrawText(graphics, value, font, area, color,
                TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix | TextFormatFlags.WordBreak);
        Draw(g, "HOTEL DESK", brandFont, new Rectangle(122, 60, 260, 32), Color.White);
        Draw(g, "HỆ THỐNG ĐIỀU HÀNH", captionFont,
            new Rectangle(123, 91, 270, 20), Color.FromArgb(224, 202, 219));
        Draw(g, "MỘT NƠI CHO MỖI NGÀY LÀM VIỆC", eyebrowFont,
            new Rectangle(54, 218, 345, 25), Color.FromArgb(244, 191, 151));
        Draw(g, "Mọi phòng.", heroFont,
            new Rectangle(50, 256, 350, 50), Color.White);
        Draw(g, "Một nhịp làm việc.", heroFont,
            new Rectangle(50, 306, 360, 50), Color.White);
        Draw(g, "Theo dõi lưu trú, đón khách và xử lý công việc\ntrong một giao diện tập trung.",
            bodyFont, new Rectangle(54, 382, 345, 58), Color.FromArgb(224, 218, 232));
        Draw(g, "HOTEL DESK  /  WORKSPACE", footerFont,
            new Rectangle(54, Height - 50, 300, 24), Color.FromArgb(193, 181, 206));
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            roomImage?.Dispose();
            logo.Dispose();
            brandFont.Dispose();
            captionFont.Dispose();
            eyebrowFont.Dispose();
            heroFont.Dispose();
            bodyFont.Dispose();
            footerFont.Dispose();
        }
        base.Dispose(disposing);
    }
}

internal sealed class LoginCloseButton : Button
{
    private bool hovered;

    public LoginCloseButton()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer, true);
        Cursor = Cursors.Hand;
        AccessibleName = "Đóng cửa sổ đăng nhập";
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
    }

    protected override void OnMouseEnter(EventArgs e) { hovered = true; Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { hovered = false; Invalidate(); base.OnMouseLeave(e); }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.Clear(Parent?.BackColor ?? Color.White);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        if (hovered)
        {
            using var hoverFill = new SolidBrush(Color.FromArgb(228, 239, 242));
            g.FillEllipse(hoverFill, 2, 2, Width - 4, Height - 4);
        }
        using var cross = new Pen(hovered ? Color.FromArgb(23, 121, 128) :
            Color.FromArgb(112, 130, 147), 2F) { StartCap = LineCap.Round, EndCap = LineCap.Round };
        var middleX = Width / 2;
        var middleY = Height / 2;
        g.DrawLine(cross, middleX - 6, middleY - 6, middleX + 6, middleY + 6);
        g.DrawLine(cross, middleX + 6, middleY - 6, middleX - 6, middleY + 6);
        if (Focused) ControlPaint.DrawFocusRectangle(g, new Rectangle(4, 4, Width - 8, Height - 8));
    }
}

internal sealed class LoginFieldPanel : Panel
{
    private readonly Image icon;

    public LoginFieldPanel(string iconKind)
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        BackColor = Color.White;
        icon = UiIcons.Create(iconKind, AppTheme.Blue, 21);
    }

    public void TrackFocus(Control child)
    {
        child.GotFocus += (_, _) => Invalidate();
        child.LostFocus += (_, _) => Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.Clear(Parent?.BackColor ?? Color.White);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        using var shape = AppTheme.Rounded(new RectangleF(1, 1, Width - 3, Height - 3), 11);
        using var fill = new SolidBrush(Color.White);
        using var outline = new Pen(ContainsFocus ? AppTheme.Blue :
            AppTheme.Border, ContainsFocus ? 1.8F : 1F);
        g.FillPath(fill, shape);
        g.DrawPath(outline, shape);
        g.DrawImage(icon, 19, (Height - 21) / 2, 21, 21);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) icon.Dispose();
        base.Dispose(disposing);
    }
}

internal sealed class LoginActionButton : Button
{
    private bool hovered;
    private bool pressed;

    public LoginActionButton()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        Cursor = Cursors.Hand;
        Font = new Font("Segoe UI Semibold", 10.5F, FontStyle.Bold);
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        TabStop = true;
    }

    protected override void OnMouseEnter(EventArgs e) { hovered = true; Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { hovered = false; pressed = false; Invalidate(); base.OnMouseLeave(e); }
    protected override void OnMouseDown(MouseEventArgs e) { pressed = true; Invalidate(); base.OnMouseDown(e); }
    protected override void OnMouseUp(MouseEventArgs e) { pressed = false; Invalidate(); base.OnMouseUp(e); }
    protected override void OnEnabledChanged(EventArgs e) { Invalidate(); base.OnEnabledChanged(e); }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.Clear(Parent?.BackColor ?? Color.White);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var top = !Enabled ? Color.FromArgb(153, 172, 186) :
            pressed ? AppTheme.Navy :
            hovered ? Color.FromArgb(40, 109, 139) : AppTheme.Blue;
        var bottom = !Enabled ? top : pressed ? Color.FromArgb(22, 59, 78) :
            hovered ? AppTheme.Blue : AppTheme.Navy;
        using var shape = AppTheme.Rounded(new RectangleF(1, 1, Width - 3, Height - 3), 12);
        using var gradient = new LinearGradientBrush(ClientRectangle, top, bottom, LinearGradientMode.Horizontal);
        g.FillPath(gradient, shape);
        TextRenderer.DrawText(g, Text, Font, ClientRectangle, Color.White,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
        if (Focused)
        {
            using var focus = new Pen(Color.FromArgb(190, Color.White)) { DashStyle = DashStyle.Dot };
            using var inset = AppTheme.Rounded(new RectangleF(5, 5, Width - 11, Height - 11), 9);
            g.DrawPath(focus, inset);
        }
    }
}
