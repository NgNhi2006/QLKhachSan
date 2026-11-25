using System.Drawing.Drawing2D;

namespace QLKhachSan.GUI;

internal sealed class DashboardLogo : Control
{
    private readonly Image icon = UiIcons.Create("building", AppTheme.Focus, 38);

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
    private bool featured;
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
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public bool Featured {get=>featured;set{featured=value;Invalidate();}}

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
        var large=Height>=230;
        var inset=large?24:20;
        var iconSize=large?50:42;
        var iconGlyph=large?29:25;
        var titleTop=large?87:75;
        var detailTop=large?126:112;
        var arrowSize=large?32:28;
        using var shape = AppTheme.Rounded(new RectangleF(2, 2, Width - 5, Height - 5), 14);
        using var fill = new SolidBrush(hovered ? Color.FromArgb(249,246,239) :
            Featured?Color.FromArgb(250,239,220):Color.White);
        using var border = new Pen(hovered || Focused ? AppTheme.Blue : AppTheme.Border, hovered || Focused ? 1.7F : 1F);
        g.FillPath(fill, shape);
        g.DrawPath(border, shape);
        using var iconShape=AppTheme.Rounded(new RectangleF(inset,large?22:18,iconSize,iconSize),12);
        using var iconFill = new SolidBrush(Color.FromArgb(
            (int)(255*.81+Accent.R*.19),(int)(255*.81+Accent.G*.19),(int)(255*.81+Accent.B*.19)));
        g.FillPath(iconFill,iconShape);
        icon ??= UiIcons.Create(IconKind, Accent, 64,IconPng);
        g.DrawImage(icon,inset+(iconSize-iconGlyph)/2,(large?22:18)+(iconSize-iconGlyph)/2,iconGlyph,iconGlyph);
        using var arrowFont = AppTheme.TextFont(large?22F:19F);
        using var titleFont = AppTheme.TextFont(large?16F:15F, FontStyle.Bold);
        var arrowX=Width-inset-arrowSize;
        var arrowY=large?29:24;
        using var arrowPen=new Pen(AppTheme.Border,1.4F);
        g.DrawEllipse(arrowPen,arrowX,arrowY,arrowSize,arrowSize);
        TextRenderer.DrawText(g, "›", arrowFont,
            new Rectangle(arrowX,arrowY-2,arrowSize,arrowSize+2), Accent,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        TextRenderer.DrawText(g, Title, titleFont,
            new Rectangle(inset,titleTop,Width-2*inset,large?35:32), AppTheme.Ink,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
        using var descriptionFont=AppTheme.TextFont(large?10F:9F);
        TextRenderer.DrawText(g, Description, descriptionFont,
            new Rectangle(inset+1,detailTop,Math.Max(80,Width-2*inset),Math.Max(32,Height-detailTop-12)), AppTheme.Muted,
            TextFormatFlags.Left | TextFormatFlags.WordBreak | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) icon?.Dispose();
        base.Dispose(disposing);
    }
}

internal sealed class DashboardOccupancyCard : Control
{
    private int occupied;
    private int total;
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public int Occupied {get=>occupied;set{occupied=value;Invalidate();}}
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public int Total {get=>total;set{total=value;Invalidate();}}
    public DashboardOccupancyCard()
    {
        SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|
            ControlStyles.OptimizedDoubleBuffer|ControlStyles.ResizeRedraw,true);
    }
    protected override void OnPaint(PaintEventArgs e)
    {
        var g=e.Graphics;g.Clear(Parent?.BackColor??AppTheme.Canvas);g.SmoothingMode=SmoothingMode.AntiAlias;
        using var shape=AppTheme.Rounded(new RectangleF(1,1,Width-2,Height-2),13);
        using var fill=new SolidBrush(AppTheme.Blue);g.FillPath(fill,shape);
        var rate=Total==0?0:(int)Math.Round(100.0*Occupied/Total);
        TextRenderer.DrawText(g,"TỶ LỆ LẤP ĐẦY PHÒNG",AppTheme.Small,
            new Rectangle(16,9,Width-32,18),Color.FromArgb(222,241,228),
            TextFormatFlags.Left|TextFormatFlags.VerticalCenter);
        using var numberFont=AppTheme.TextFont(20F,FontStyle.Bold);
        TextRenderer.DrawText(g,$"{rate}%",numberFont,new Rectangle(15,28,Width-30,36),Color.White,
            TextFormatFlags.Left|TextFormatFlags.VerticalCenter);
        var barWidth=Math.Max(30,Width-32);
        using var rail=AppTheme.Rounded(new RectangleF(16,80,barWidth,4),2);
        using var railFill=new SolidBrush(Color.FromArgb(86,255,255,255));g.FillPath(railFill,rail);
        if(rate>0)
        {
            using var progress=AppTheme.Rounded(new RectangleF(16,80,Math.Max(5,barWidth*rate/100F),4),2);
            using var progressFill=new SolidBrush(Color.FromArgb(239,215,167));g.FillPath(progressFill,progress);
        }
        TextRenderer.DrawText(g,$"{Occupied} / {Total} phòng",AppTheme.Small,
            new Rectangle(Math.Max(16,Width-112),53,96,23),Color.White,
            TextFormatFlags.Right|TextFormatFlags.VerticalCenter);
    }
}

internal sealed class DashboardFunctionButton : Button
{
    private bool hovered;
    private Image? icon;
    private string iconKind="bed";
    private Color accent=AppTheme.Blue;
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public Color Accent {get=>accent;set{accent=value;icon?.Dispose();icon=null;Invalidate();}}
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public string Description {get;set;}="";

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
        var tile=Height>=90;
        using var shape=AppTheme.Rounded(new RectangleF(1,1,Width-3,Height-3),10);
        var tint=Color.FromArgb((int)(255*.91+Accent.R*.09),(int)(255*.91+Accent.G*.09),(int)(255*.91+Accent.B*.09));
        using var fill=new SolidBrush(hovered||Focused?Color.FromArgb((int)(255*.86+Accent.R*.14),(int)(255*.86+Accent.G*.14),(int)(255*.86+Accent.B*.14)):tile?tint:Color.FromArgb(251,250,246));
        using var border=new Pen(hovered||Focused?Accent:AppTheme.Border,1);
        g.FillPath(fill,shape);g.DrawPath(border,shape);
        if(tile)
        {
            var top=Height>=170?20:14;
            var iconBox=Height>=170?55:43;
            using var iconShape=AppTheme.Rounded(new RectangleF(16,top,iconBox,iconBox),11);
            using var iconFill=new SolidBrush(Color.FromArgb((int)(255*.78+Accent.R*.22),(int)(255*.78+Accent.G*.22),(int)(255*.78+Accent.B*.22)));g.FillPath(iconFill,iconShape);
            icon??=UiIcons.Create(iconKind,Accent,36);
            var glyph=Height>=170?34:26;
            g.DrawImage(icon,16+(iconBox-glyph)/2,top+(iconBox-glyph)/2,glyph,glyph);
            var titleTop=Height>=170?91:67;
            using var tileTitleFont=AppTheme.TextFont(Height>=170?15F:12F,FontStyle.Bold);
            TextRenderer.DrawText(g,Text,tileTitleFont,new Rectangle(16,titleTop,Math.Max(20,Width-42),31),
                AppTheme.Ink,TextFormatFlags.Left|TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis|TextFormatFlags.NoPrefix);
            TextRenderer.DrawText(g,Description,AppTheme.Small,new Rectangle(16,titleTop+35,Math.Max(20,Width-32),Math.Max(20,Height-titleTop-44)),
                AppTheme.Muted,TextFormatFlags.Left|TextFormatFlags.WordBreak|TextFormatFlags.EndEllipsis|TextFormatFlags.NoPrefix);
            TextRenderer.DrawText(g,"›",AppTheme.Title,new Rectangle(Width-36,15,24,28),
                Accent,TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter);
            if(Focused)ControlPaint.DrawFocusRectangle(g,new Rectangle(5,5,Width-10,Height-10));
            return;
        }
        icon??=UiIcons.Create(iconKind,Accent,21);
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

internal sealed class DashboardSidebarButton : Button
{
    private bool hovered;
    private bool selected;
    private Image? icon;
    private string iconKind="bed";

    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public string IconKind { get=>iconKind; set { iconKind=value;icon?.Dispose();icon=null;Invalidate(); } }
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public bool Selected { get=>selected;set { if(selected==value)return;selected=value;icon?.Dispose();icon=null;Invalidate(); } }

    public DashboardSidebarButton()
    {
        SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|
            ControlStyles.OptimizedDoubleBuffer|ControlStyles.ResizeRedraw,true);
        FlatStyle=FlatStyle.Flat;FlatAppearance.BorderSize=0;Cursor=Cursors.Hand;
        Height=44;UseMnemonic=false;
    }
    protected override void OnMouseEnter(EventArgs e){hovered=true;Invalidate();base.OnMouseEnter(e);}
    protected override void OnMouseLeave(EventArgs e){hovered=false;Invalidate();base.OnMouseLeave(e);}
    protected override void OnPaint(PaintEventArgs e)
    {
        var g=e.Graphics;g.Clear(Parent?.BackColor??AppTheme.Navy);g.SmoothingMode=SmoothingMode.AntiAlias;
        if(selected||hovered)
        {
            using var shape=AppTheme.Rounded(new RectangleF(1,1,Width-2,Height-2),10);
            using var fill=new SolidBrush(selected?Color.FromArgb(46,91,69):Color.FromArgb(33,76,57));
            g.FillPath(fill,shape);
        }
        var foreground=selected?Color.FromArgb(241,214,157):Color.FromArgb(229,237,229);
        icon??=UiIcons.Create(iconKind,foreground,20);
        g.DrawImage(icon,14,(Height-20)/2,20,20);
        using var labelFont=AppTheme.TextFont(Text.Length>19?9F:10F,FontStyle.Bold);
        TextRenderer.DrawText(g,Text,labelFont,new Rectangle(44,1,Math.Max(20,Width-49),Height-2),foreground,
            TextFormatFlags.Left|TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis|TextFormatFlags.NoPrefix);
        if(Focused)ControlPaint.DrawFocusRectangle(g,new Rectangle(5,5,Width-10,Height-10));
    }
    protected override void Dispose(bool disposing){if(disposing)icon?.Dispose();base.Dispose(disposing);}
}

internal sealed class DashboardSidebarScene : Control
{
    private readonly Image? roomImage;
    public DashboardSidebarScene()
    {
        SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|
            ControlStyles.OptimizedDoubleBuffer|ControlStyles.ResizeRedraw,true);
        using var stream=typeof(DashboardSidebarScene).Assembly.GetManifestResourceStream("QLKhachSan.Assets.sidebar-lounge.png");
        if(stream is not null){using var decoded=Image.FromStream(stream);roomImage=new Bitmap(decoded);}
    }
    protected override void OnPaint(PaintEventArgs e)
    {
        var g=e.Graphics;g.Clear(AppTheme.Navy);g.SmoothingMode=SmoothingMode.AntiAlias;
        if(roomImage is not null)
        {
            var targetRatio=(double)Width/Math.Max(1,Height);
            var imageRatio=(double)roomImage.Width/roomImage.Height;
            Rectangle source;
            if(targetRatio>imageRatio)
            {
                var cropHeight=Math.Min(roomImage.Height,(int)Math.Ceiling(roomImage.Width/targetRatio));
                var cropTop=Math.Min(roomImage.Height-cropHeight,roomImage.Height/4);
                source=new Rectangle(0,cropTop,roomImage.Width,cropHeight);
            }
            else
            {
                var cropWidth=Math.Min(roomImage.Width,(int)Math.Ceiling(roomImage.Height*targetRatio));
                source=new Rectangle((roomImage.Width-cropWidth)/2,0,cropWidth,roomImage.Height);
            }
            g.DrawImage(roomImage,ClientRectangle,source,GraphicsUnit.Pixel);
        }
        using(var shade=new SolidBrush(Color.FromArgb(75,11,45,32)))g.FillRectangle(shade,ClientRectangle);
        var fadeHeight=Math.Min(Math.Clamp(Height/3,85,150),Math.Max(1,Height-8));
        using(var topFade=new LinearGradientBrush(new Rectangle(0,8,Width,fadeHeight),
            AppTheme.Navy,Color.FromArgb(0,AppTheme.Navy),LinearGradientMode.Vertical))
            g.FillRectangle(topFade,0,8,Width,fadeHeight);
        using(var seamless=new SolidBrush(AppTheme.Navy))g.FillRectangle(seamless,0,0,Width,9);
        using(var bottomFade=new LinearGradientBrush(new Rectangle(0,Math.Max(0,Height-95),Width,Math.Min(95,Height)),
            Color.FromArgb(0,AppTheme.Navy),Color.FromArgb(210,AppTheme.Navy),LinearGradientMode.Vertical))
            g.FillRectangle(bottomFade,0,Math.Max(0,Height-95),Width,Math.Min(95,Height));
        using var gold=new SolidBrush(AppTheme.Focus);g.FillRectangle(gold,20,Height-71,38,2);
        TextRenderer.DrawText(g,"Tinh tế trong từng\ntrải nghiệm lưu trú",AppTheme.Bold,
            new Rectangle(20,Height-62,Width-35,52),Color.White,
            TextFormatFlags.Left|TextFormatFlags.WordBreak|TextFormatFlags.NoPrefix);
    }
    protected override void Dispose(bool disposing){if(disposing)roomImage?.Dispose();base.Dispose(disposing);}
}

internal sealed class HeaderAccountCard : Panel
{
    private bool hovered;
    public HeaderAccountCard()
    {
        SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|
            ControlStyles.OptimizedDoubleBuffer|ControlStyles.ResizeRedraw,true);
        BackColor=Color.White;
        Cursor=Cursors.Hand;
    }
    protected override void OnMouseEnter(EventArgs e){hovered=true;Invalidate();base.OnMouseEnter(e);}
    protected override void OnMouseLeave(EventArgs e){hovered=false;Invalidate();base.OnMouseLeave(e);}
    protected override void OnPaintBackground(PaintEventArgs e)
    {
        e.Graphics.Clear(Parent?.BackColor??Color.White);
        e.Graphics.SmoothingMode=SmoothingMode.AntiAlias;
        using var shape=AppTheme.Rounded(new RectangleF(1,1,Width-3,Height-3),16);
        using var fill=new SolidBrush(hovered?Color.FromArgb(246,248,242):Color.White);
        e.Graphics.FillPath(fill,shape);
    }
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.SmoothingMode=SmoothingMode.AntiAlias;
        using var shape=AppTheme.Rounded(new RectangleF(1,1,Width-3,Height-3),16);
        if(hovered)
        {
            using var pen=new Pen(AppTheme.Border);
            e.Graphics.DrawPath(pen,shape);
        }
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
