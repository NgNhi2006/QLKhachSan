using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using QLKhachSan.DTO;

namespace QLKhachSan.GUI;

internal static class AppTheme
{
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
    public static readonly Color Canvas=Color.FromArgb(244,247,251);
    public static readonly Color Navy=Color.FromArgb(12,24,47);
    public static readonly Color Ink=Color.FromArgb(24,38,61);
    public static readonly Color Muted=Color.FromArgb(100,116,137);
    public static readonly Color Border=Color.FromArgb(222,230,240);
    public static readonly Color Blue=Color.FromArgb(56,95,218);
    public static readonly Color Teal=Color.FromArgb(15,153,143);
    public static readonly Color Amber=Color.FromArgb(209,137,36);
    public static readonly Color Danger=Color.FromArgb(209,75,91);
    public static readonly Color Focus=Color.FromArgb(54,176,222);
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
        button.UseVisualStyleBackColor=false;button.FlatAppearance.MouseOverBackColor=primary?Color.FromArgb(44,79,187):Canvas;
        button.FlatAppearance.MouseDownBackColor=primary?Navy:Color.FromArgb(229,237,248);
    }
    public static void Grid(DataGridView grid)
    {
        grid.Font=Body;grid.EnableHeadersVisualStyles=false;grid.BorderStyle=BorderStyle.None;
        grid.BackgroundColor=Color.White;grid.GridColor=Border;grid.CellBorderStyle=DataGridViewCellBorderStyle.SingleHorizontal;
        grid.ColumnHeadersBorderStyle=DataGridViewHeaderBorderStyle.None;grid.RowHeadersVisible=false;
        grid.ColumnHeadersDefaultCellStyle=new DataGridViewCellStyle {BackColor=Color.FromArgb(238,243,250),ForeColor=Ink,Font=Bold,Padding=new Padding(8,0,8,0),SelectionBackColor=Color.FromArgb(238,243,250),SelectionForeColor=Ink};
        grid.ColumnHeadersHeight=40;grid.ColumnHeadersHeightSizeMode=DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        grid.DefaultCellStyle=new DataGridViewCellStyle {ForeColor=Ink,BackColor=Color.White,SelectionBackColor=Color.FromArgb(225,235,255),SelectionForeColor=Ink,Padding=new Padding(8,4,8,4)};
        grid.AlternatingRowsDefaultCellStyle.BackColor=Color.FromArgb(248,250,253);grid.RowTemplate.Height=40;
    }
    public static Color RoomColor(RoomStatus status)=>status switch
    {
        RoomStatus.Trong=>Teal,RoomStatus.DaDat=>Color.FromArgb(89,104,145),RoomStatus.DangO=>Blue,RoomStatus.DangDon=>Amber,RoomStatus.BaoTri=>Muted,_=>Muted
    };
    public static Color RoomTint(RoomStatus status)=>status switch
    {
        RoomStatus.Trong=>Color.FromArgb(223,245,241),RoomStatus.DaDat=>Color.FromArgb(234,238,247),
        RoomStatus.DangO=>Color.FromArgb(225,239,245),RoomStatus.DangDon=>Color.FromArgb(253,241,219),
        _=>Color.FromArgb(232,235,241)
    };
}

internal sealed class RoomTile : Button
{
    private bool hovered;
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public int Reservations {get;set;}
    public RoomTile()
    {
        SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer|ControlStyles.ResizeRedraw,true);
        Width=112;Height=78;Margin=new Padding(0,0,7,8);FlatStyle=FlatStyle.Flat;FlatAppearance.BorderSize=0;Cursor=Cursors.Hand;
    }
    protected override void OnMouseEnter(EventArgs e){hovered=true;Invalidate();base.OnMouseEnter(e);}
    protected override void OnMouseLeave(EventArgs e){hovered=false;Invalidate();base.OnMouseLeave(e);}
    protected override void OnPaint(PaintEventArgs e)
    {
        if(Tag is not Room room){base.OnPaint(e);return;}
        var g=e.Graphics;g.Clear(Color.White);g.SmoothingMode=SmoothingMode.AntiAlias;
        var accent=Enabled?AppTheme.RoomColor(room.Status):AppTheme.Muted;
        using var shape=AppTheme.Rounded(new RectangleF(1,1,Width-3,Height-3),11);
        using var fill=new SolidBrush(hovered?Color.FromArgb(245,249,255):Color.White);
        using var outline=new Pen(hovered?accent:Color.FromArgb(215,224,236),hovered?2:1);
        g.FillPath(fill,shape);g.DrawPath(outline,shape);
        using var strip=new SolidBrush(accent);g.FillRectangle(strip,9,11,3,21);
        using var numberFont=new Font(Font.FontFamily,11,FontStyle.Bold);
        TextRenderer.DrawText(g,room.Number,numberFont,new Rectangle(17,7,Width-29,27),AppTheme.Ink,TextFormatFlags.Left|TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis);
        TextRenderer.DrawText(g,room.Type,AppTheme.Small,new Rectangle(17,30,Width-29,16),AppTheme.Muted,
            TextFormatFlags.Left|TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis|TextFormatFlags.NoPrefix);
        using var badge=AppTheme.Rounded(new RectangleF(8,45,Width-16,24),8);
        using var badgeFill=new SolidBrush(AppTheme.RoomTint(room.Status));g.FillPath(badgeFill,badge);
        TextRenderer.DrawText(g,Ui.Status(room.Status),AppTheme.Small,new Rectangle(8,45,Width-16,24),accent,TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis);
        if(Reservations>0){using var dot=new SolidBrush(AppTheme.Amber);g.FillEllipse(dot,Width-16,12,7,7);}
        if(Focused)ControlPaint.DrawFocusRectangle(g,new Rectangle(5,5,Width-10,Height-10));
    }
}
