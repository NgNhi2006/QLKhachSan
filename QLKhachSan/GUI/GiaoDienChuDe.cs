using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using QLKhachSan.DTO;

namespace QLKhachSan.GUI;

internal static class AppTheme
{
    public static Icon AppIcon {get;}=LoadAppIcon();
    private static Icon LoadAppIcon()
    {
        var path=Path.Combine(AppContext.BaseDirectory,"Assets","hotel-desk.ico");
        return File.Exists(path)?new Icon(path):SystemIcons.Application;
    }
    private const uint PrivateFont=0x10;
    [DllImport("gdi32.dll",EntryPoint="AddFontResourceExW",CharSet=CharSet.Unicode,SetLastError=true)]
    private static extern int AddFontResourceEx(string file,uint flags,nint reserved);
    private static readonly PrivateFontCollection FontCollection=new();
    private static readonly FontFamily FontFamily=RegisterFont();
    private static FontFamily RegisterFont()
    {
        var folder=Path.Combine(AppContext.BaseDirectory,"Assets","Fonts");
        var regular=Path.Combine(folder,"Inter-Regular.ttf");
        var bold=Path.Combine(folder,"Inter-Bold.ttf");
        if(File.Exists(regular) && File.Exists(bold))
        {
            FontCollection.AddFontFile(regular);
            FontCollection.AddFontFile(bold);
            AddFontResourceEx(regular,PrivateFont,0);
            AddFontResourceEx(bold,PrivateFont,0);
            if(FontCollection.Families.FirstOrDefault(x=>x.Name=="Inter") is { } family)return family;
        }
        return new FontFamily("Segoe UI");
    }
    public static Font TextFont(float size,FontStyle style=FontStyle.Regular)=>new(FontFamily,size,style);
    // Warm hospitality palette shared by navigation, forms and transaction screens.
    public static readonly Color Canvas=Color.FromArgb(249,247,242);
    public static readonly Color Navy=Color.FromArgb(20,55,43);
    public static readonly Color Ink=Color.FromArgb(35,47,40);
    public static readonly Color Muted=Color.FromArgb(107,116,106);
    public static readonly Color Border=Color.FromArgb(226,226,215);
    public static readonly Color Blue=Color.FromArgb(23,105,78);
    public static readonly Color Teal=Color.FromArgb(58,135,88);
    public static readonly Color Amber=Color.FromArgb(183,130,58);
    public static readonly Color Danger=Color.FromArgb(187,83,71);
    public static readonly Color Focus=Color.FromArgb(195,160,95);
    public static readonly Font Body=TextFont(10f);
    public static readonly Font Small=TextFont(9f);
    public static readonly Font Bold=TextFont(10f,FontStyle.Bold);
    public static readonly Font Title=TextFont(18f,FontStyle.Bold);
    public static readonly Font Metric=TextFont(21f,FontStyle.Bold);
    public static GraphicsPath Rounded(RectangleF rect,float radius)
    {
        var p=new GraphicsPath();var d=Math.Min(radius*2,Math.Min(rect.Width,rect.Height));
        p.AddArc(rect.Left,rect.Top,d,d,180,90);p.AddArc(rect.Right-d,rect.Top,d,d,270,90);
        p.AddArc(rect.Right-d,rect.Bottom-d,d,d,0,90);p.AddArc(rect.Left,rect.Bottom-d,d,d,90,90);p.CloseFigure();return p;
    }
    public static void Button(Button button,bool primary=false)
    {
        button.FlatStyle=FlatStyle.Flat;button.FlatAppearance.BorderSize=primary?0:1;
        button.FlatAppearance.BorderColor=Border;button.BackColor=primary?Blue:Color.White;
        button.ForeColor=primary?Color.White:Ink;button.Font=Bold;button.Cursor=Cursors.Hand;
        button.UseVisualStyleBackColor=false;button.FlatAppearance.MouseOverBackColor=primary?Navy:Canvas;
        button.FlatAppearance.MouseDownBackColor=primary?Color.FromArgb(12,45,33):Color.FromArgb(238,237,226);
        void Shape(object? _,EventArgs __)
        {
            if(button.Width<4||button.Height<4)return;
            var previous=button.Region;
            using var path=Rounded(new RectangleF(0,0,button.Width,button.Height),9);
            button.Region=new Region(path);
            previous?.Dispose();
        }
        button.Resize+=Shape;
        Shape(button,EventArgs.Empty);
    }
    public static void Grid(DataGridView grid)
    {
        grid.Font=Body;grid.EnableHeadersVisualStyles=false;grid.BorderStyle=BorderStyle.None;
        grid.BackgroundColor=Color.White;grid.GridColor=Border;grid.CellBorderStyle=DataGridViewCellBorderStyle.SingleHorizontal;
        grid.ColumnHeadersBorderStyle=DataGridViewHeaderBorderStyle.None;grid.RowHeadersVisible=false;
        grid.ColumnHeadersDefaultCellStyle=new DataGridViewCellStyle {BackColor=Color.FromArgb(243,243,235),ForeColor=Ink,Font=Bold,Padding=new Padding(8,0,8,0),SelectionBackColor=Color.FromArgb(243,243,235),SelectionForeColor=Ink};
        grid.ColumnHeadersHeight=40;grid.ColumnHeadersHeightSizeMode=DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        grid.DefaultCellStyle=new DataGridViewCellStyle {ForeColor=Ink,BackColor=Color.White,SelectionBackColor=Color.FromArgb(224,239,226),SelectionForeColor=Ink,Padding=new Padding(8,4,8,4)};
        grid.AlternatingRowsDefaultCellStyle.BackColor=Color.FromArgb(251,250,246);grid.RowTemplate.Height=40;
    }
    public static Color RoomColor(RoomStatus status)=>status switch
    {
        RoomStatus.Trong=>Teal,RoomStatus.DaDat=>Color.FromArgb(87,122,151),RoomStatus.DangO=>Blue,RoomStatus.DangDon=>Amber,RoomStatus.BaoTri=>Danger,_=>Muted
    };
    public static Color RoomTint(RoomStatus status)=>status switch
    {
        RoomStatus.Trong=>Color.FromArgb(231,243,229),RoomStatus.DaDat=>Color.FromArgb(235,241,245),
        RoomStatus.DangO=>Color.FromArgb(220,237,227),RoomStatus.DangDon=>Color.FromArgb(250,241,220),
        _=>Color.FromArgb(248,233,228)
    };
    public static Color BookingRoomColor(RoomStatus status)=>status switch
    {
        RoomStatus.Trong=>Color.FromArgb(31,139,83),
        RoomStatus.DaDat=>Color.FromArgb(116,91,167),
        RoomStatus.DangO=>Color.FromArgb(49,109,184),
        RoomStatus.DangDon=>Color.FromArgb(183,116,30),
        RoomStatus.BaoTri=>Color.FromArgb(189,75,72),
        _=>Muted
    };
    public static Color BookingRoomTint(RoomStatus status)=>status switch
    {
        RoomStatus.Trong=>Color.FromArgb(230,247,236),
        RoomStatus.DaDat=>Color.FromArgb(242,237,250),
        RoomStatus.DangO=>Color.FromArgb(231,240,252),
        RoomStatus.DangDon=>Color.FromArgb(255,245,225),
        RoomStatus.BaoTri=>Color.FromArgb(255,235,233),
        _=>Color.FromArgb(244,244,244)
    };
}

internal sealed class RoomTile : Button
{
    private bool hovered;
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public int Reservations {get;set;}
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public bool ShowCode {get;set;}
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public bool SelectedForBooking {get;set;}
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public bool BookingUnavailable {get;set;}
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public bool BookingConflict {get;set;}
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public bool BookingCheckout {get;set;}
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public bool ShowBookingRates {get;set;}
    public RoomTile()
    {
        SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer|ControlStyles.ResizeRedraw|ControlStyles.StandardDoubleClick,true);
        Width=112;Height=78;Margin=new Padding(0,0,7,8);FlatStyle=FlatStyle.Flat;FlatAppearance.BorderSize=0;Cursor=Cursors.Hand;
    }
    protected override void OnMouseEnter(EventArgs e){hovered=true;Invalidate();base.OnMouseEnter(e);}
    protected override void OnMouseLeave(EventArgs e){hovered=false;Invalidate();base.OnMouseLeave(e);}
    protected override void OnPaint(PaintEventArgs e)
    {
        if(Tag is not Room room){base.OnPaint(e);return;}
        var g=e.Graphics;g.Clear(Color.White);g.SmoothingMode=SmoothingMode.AntiAlias;
        var bookingStatus=BookingConflict?RoomStatus.DaDat:room.Status;
        var accent=ShowBookingRates?AppTheme.BookingRoomColor(bookingStatus):
            SelectedForBooking?AppTheme.Blue:BookingUnavailable?AppTheme.Muted:
            Enabled?AppTheme.RoomColor(room.Status):AppTheme.Muted;
        using var shape=AppTheme.Rounded(new RectangleF(1,1,Width-3,Height-3),11);
        using var fill=new SolidBrush(ShowBookingRates?AppTheme.BookingRoomTint(bookingStatus):
            SelectedForBooking?Color.FromArgb(225,241,251):
            BookingUnavailable?Color.FromArgb(245,245,245):hovered?Color.FromArgb(245,249,255):Color.White);
        using var outline=new Pen(ShowBookingRates?
            (SelectedForBooking?AppTheme.Focus:accent):
            SelectedForBooking||hovered?accent:Color.FromArgb(215,224,236),
            ShowBookingRates?(SelectedForBooking?3:1):SelectedForBooking||hovered?2:1);
        g.FillPath(fill,shape);g.DrawPath(outline,shape);
        if(ShowBookingRates)
        {
            using var marker=new SolidBrush(accent);
            g.FillRectangle(marker,9,13,5,24);
            using var bookingFont=new Font(Font.FontFamily,12,FontStyle.Bold);
            TextRenderer.DrawText(g,$"P.{room.Number}",bookingFont,new Rectangle(22,11,Width-83,29),AppTheme.Ink,
                TextFormatFlags.Left|TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis);
            TextRenderer.DrawText(g,SelectedForBooking?"✓ ĐÃ CHỌN":BookingCheckout?"THANH TOÁN":room.Type,AppTheme.Small,
                new Rectangle(Width-104,14,90,22),SelectedForBooking?Color.FromArgb(121,86,24):
                    BookingCheckout?AppTheme.BookingRoomColor(RoomStatus.DangO):AppTheme.Muted,
                TextFormatFlags.Right|TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis|TextFormatFlags.NoPrefix);
            TextRenderer.DrawText(g,$"{room.Rate:N0} đ / ngày",AppTheme.Small,
                new Rectangle(13,42,Width-26,21),AppTheme.Ink,
                TextFormatFlags.Left|TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis);
            using var bookingBadge=AppTheme.Rounded(new RectangleF(11,73,Width-22,24),8);
            using var bookingFill=new SolidBrush(accent);
            g.FillPath(bookingFill,bookingBadge);
            TextRenderer.DrawText(g,BookingConflict?"Có lịch đặt":Ui.Status(room.Status),AppTheme.Small,
                new Rectangle(11,73,Width-22,24),Color.White,
                TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis);
            if(Focused)ControlPaint.DrawFocusRectangle(g,new Rectangle(5,5,Width-10,Height-10));
            return;
        }
        using var strip=new SolidBrush(accent);g.FillRectangle(strip,9,11,3,21);
        using var numberFont=new Font(Font.FontFamily,11,FontStyle.Bold);
        TextRenderer.DrawText(g,room.Number,numberFont,new Rectangle(17,7,Width-29,27),AppTheme.Ink,TextFormatFlags.Left|TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis);
        if(SelectedForBooking)
            TextRenderer.DrawText(g,"✓ ĐÃ CHỌN",AppTheme.Small,new Rectangle(Width-88,9,78,23),AppTheme.Blue,
                TextFormatFlags.Right|TextFormatFlags.VerticalCenter|TextFormatFlags.NoPrefix);
        if(ShowCode)
            TextRenderer.DrawText(g,$"Mã: {room.Code}",AppTheme.Small,new Rectangle(17,30,Width-29,18),AppTheme.Navy,
                TextFormatFlags.Left|TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis|TextFormatFlags.NoPrefix);
        TextRenderer.DrawText(g,room.Type,AppTheme.Small,new Rectangle(17,ShowCode?50:30,Width-29,16),AppTheme.Muted,
            TextFormatFlags.Left|TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis|TextFormatFlags.NoPrefix);
        var badgeTop=ShowCode?70:45;
        using var badge=AppTheme.Rounded(new RectangleF(8,badgeTop,Width-16,24),8);
        using var badgeFill=new SolidBrush(AppTheme.RoomTint(room.Status));g.FillPath(badgeFill,badge);
        TextRenderer.DrawText(g,Ui.Status(room.Status),AppTheme.Small,new Rectangle(8,badgeTop,Width-16,24),accent,TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis);
        if(Reservations>0){using var dot=new SolidBrush(AppTheme.Amber);g.FillEllipse(dot,Width-16,12,7,7);}
        if(Focused)ControlPaint.DrawFocusRectangle(g,new Rectangle(5,5,Width-10,Height-10));
    }
}
