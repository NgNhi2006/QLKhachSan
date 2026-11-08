using System.Drawing.Drawing2D;

namespace QLKhachSan.GUI;

internal sealed class DashboardLogo : Control
{
    private readonly Image icon = UiIcons.Create("building", Color.FromArgb(105,211,238), 38);

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
        e.Graphics.DrawImage(icon, (Width - 38) / 2, (Height - 38) / 2, 38, 38);
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
            AppTheme.Navy, Color.FromArgb(37, 91, 111), LinearGradientMode.Horizontal);
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
            using var photoTint=new SolidBrush(Color.FromArgb(65,26,48,65));
            e.Graphics.FillRectangle(photoTint,imageArea);
            using var fade=new LinearGradientBrush(new Rectangle(imageArea.Left-115,0,115,Height),
                Color.FromArgb(255,26,48,65),Color.FromArgb(0,26,48,65),LinearGradientMode.Horizontal);
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
    private byte[]? iconPng;
    private string iconKind = "bed";
    private Color accent = AppTheme.Blue;

    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public string Title { get; set; } = "";
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public string Description { get; set; } = "";
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public string IconKind { get => iconKind; set { iconKind = value; icon?.Dispose(); icon = null; Invalidate(); } }
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public byte[]? IconPng { get => iconPng; set { iconPng=value; icon?.Dispose(); icon=null; Invalidate(); } }
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
        var large=Height>=265;
        var inset=large?30:25;
        var iconSize=large?64:58;
        var iconGlyph=large?35:31;
        var titleTop=large?119:105;
        var detailTop=large?168:145;
        var arrowSize=large?38:34;
        using var shape = AppTheme.Rounded(new RectangleF(2, 2, Width - 5, Height - 5), 18);
        using var fill = new SolidBrush(hovered ? Color.FromArgb(250,252,255) : Color.White);
        using var border = new Pen(hovered || Focused ? AppTheme.Blue : AppTheme.Border, hovered || Focused ? 1.7F : 1F);
        g.FillPath(fill, shape);
        g.DrawPath(border, shape);
        using var accentLine = new Pen(Accent,4F) { StartCap=LineCap.Round,EndCap=LineCap.Round };
        g.DrawLine(accentLine,inset,3,inset+48,3);
        using var iconShape=AppTheme.Rounded(new RectangleF(inset,large?26:23,iconSize,iconSize),15);
        using var iconFill = new SolidBrush(Color.FromArgb(234,241,255));
        g.FillPath(iconFill,iconShape);
        icon ??= UiIcons.Create(IconKind, AppTheme.Blue, 64,IconPng);
        g.DrawImage(icon,inset+(iconSize-iconGlyph)/2,(large?26:23)+(iconSize-iconGlyph)/2,iconGlyph,iconGlyph);
        using var arrowFont = AppTheme.TextFont(large?22F:19F);
        using var titleFont = AppTheme.TextFont(large?21F:18F, FontStyle.Bold);
        var arrowX=Width-inset-arrowSize;
        var arrowY=large?36:31;
        using var arrowPen=new Pen(AppTheme.Border,1.4F);
        g.DrawEllipse(arrowPen,arrowX,arrowY,arrowSize,arrowSize);
        TextRenderer.DrawText(g, "›", arrowFont,
            new Rectangle(arrowX,arrowY-2,arrowSize,arrowSize+2), AppTheme.Blue,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        TextRenderer.DrawText(g, Title, titleFont,
            new Rectangle(inset,titleTop,Width-2*inset,large?43:35), AppTheme.Ink,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
        using var descriptionFont=AppTheme.TextFont(large?11F:10F);
        TextRenderer.DrawText(g, Description, descriptionFont,
            new Rectangle(inset+1,detailTop,Math.Max(80,Width-2*inset),Math.Max(39,Height-detailTop-14)), AppTheme.Muted,
            TextFormatFlags.Left | TextFormatFlags.WordBreak | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) icon?.Dispose();
        base.Dispose(disposing);
    }
}

internal sealed class DashboardFunctionButton : Button
{
    private bool hovered;
    private Image? icon;
    private string iconKind="bed";

    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public string IconKind
    {
        get=>iconKind;
        set {iconKind=value;icon?.Dispose();icon=null;Invalidate();}
    }

    public DashboardFunctionButton()
    {
        SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|
            ControlStyles.OptimizedDoubleBuffer|ControlStyles.ResizeRedraw,true);
        FlatStyle=FlatStyle.Flat;
        FlatAppearance.BorderSize=0;
        Cursor=Cursors.Hand;
        Height=54;
    }

    protected override void OnMouseEnter(EventArgs e){hovered=true;Invalidate();base.OnMouseEnter(e);}
    protected override void OnMouseLeave(EventArgs e){hovered=false;Invalidate();base.OnMouseLeave(e);}
    protected override void OnGotFocus(EventArgs e){Invalidate();base.OnGotFocus(e);}
    protected override void OnLostFocus(EventArgs e){Invalidate();base.OnLostFocus(e);}

    protected override void OnPaint(PaintEventArgs e)
    {
        var g=e.Graphics;
        g.Clear(Parent?.BackColor??Color.White);
        g.SmoothingMode=SmoothingMode.AntiAlias;
        using var shape=AppTheme.Rounded(new RectangleF(1,1,Width-3,Height-3),10);
        using var fill=new SolidBrush(hovered||Focused?Color.FromArgb(234,241,255):Color.FromArgb(247,249,253));
        using var border=new Pen(hovered||Focused?AppTheme.Blue:AppTheme.Border,1);
        g.FillPath(fill,shape);g.DrawPath(border,shape);
        icon??=UiIcons.Create(iconKind,AppTheme.Blue,21);
        g.DrawImage(icon,16,(Height-21)/2,21,21);
        TextRenderer.DrawText(g,Text,AppTheme.Bold,new Rectangle(48,4,Math.Max(20,Width-78),Height-8),
            AppTheme.Ink,TextFormatFlags.Left|TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis|TextFormatFlags.NoPrefix);
        TextRenderer.DrawText(g,"›",AppTheme.Title,new Rectangle(Width-31,2,22,Height-4),
            AppTheme.Blue,TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter);
        if(Focused)ControlPaint.DrawFocusRectangle(g,new Rectangle(5,5,Width-10,Height-10));
    }

    protected override void Dispose(bool disposing)
    {
        if(disposing)icon?.Dispose();
        base.Dispose(disposing);
    }
}

internal sealed class AvatarBadge : Control
{
    private Image? portrait;
    private string initials="U";

    public AvatarBadge()
    {
        SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|
            ControlStyles.OptimizedDoubleBuffer|ControlStyles.ResizeRedraw,true);
        Size=new Size(32,32);
    }

    public void SetProfile(string name,byte[]? png)
    {
        portrait?.Dispose();portrait=null;
        var words=name.Split(' ',StringSplitOptions.RemoveEmptyEntries);
        initials=string.Concat(words.Take(2).Select(x=>char.ToUpperInvariant(x[0])));
        if(initials.Length==0)initials="U";
        if(png is {Length:>0})
        {
            try
            {
                using var source=new MemoryStream(png);
                using var decoded=Image.FromStream(source);
                portrait=new Bitmap(decoded);
            }
            catch(ArgumentException){portrait=null;}
            catch(OutOfMemoryException){portrait=null;}
        }
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g=e.Graphics;
        g.Clear(Parent?.BackColor??AppTheme.Navy);
        g.SmoothingMode=SmoothingMode.AntiAlias;
        using var circle=new GraphicsPath();circle.AddEllipse(0,0,Width-1,Height-1);
        using var fill=new SolidBrush(AppTheme.Blue);g.FillPath(fill,circle);
        if(portrait is not null)
        {
            var saved=g.Save();g.SetClip(circle);
            var scale=Math.Max((float)Width/portrait.Width,(float)Height/portrait.Height);
            var w=(int)Math.Ceiling(portrait.Width*scale);
            var h=(int)Math.Ceiling(portrait.Height*scale);
            g.DrawImage(portrait,new Rectangle((Width-w)/2,(Height-h)/2,w,h));
            g.Restore(saved);
        }
        else TextRenderer.DrawText(g,initials,AppTheme.Bold,ClientRectangle,Color.White,
            TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter);
    }

    protected override void Dispose(bool disposing)
    {
        if(disposing)portrait?.Dispose();
        base.Dispose(disposing);
    }
}
