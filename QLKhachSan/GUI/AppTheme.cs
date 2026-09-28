using System.Drawing.Drawing2D;
using QLKhachSan.DTO;

namespace QLKhachSan.GUI;

internal static class AppTheme
{
    public static readonly Color Canvas=Color.FromArgb(248,250,253);
    public static readonly Color Navy=Color.FromArgb(19,41,66);
    public static readonly Color Ink=Color.FromArgb(26,46,66);
    public static readonly Color Muted=Color.FromArgb(102,119,138);
    public static readonly Color Border=Color.FromArgb(213,224,233);
    public static readonly Color Blue=Color.FromArgb(32,121,137);
    public static readonly Color Teal=Color.FromArgb(30,148,143);
    public static readonly Color Amber=Color.FromArgb(202,144,49);
    public static readonly Font Body=new("Segoe UI",9.5f);
    public static readonly Font Small=new("Segoe UI",8.5f);
    public static readonly Font Bold=new("Segoe UI Semibold",9.5f);
    public static readonly Font Title=new("Segoe UI Semibold",17f);
    public static readonly Font Metric=new("Segoe UI Semibold",19f);
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
        button.UseVisualStyleBackColor=false;button.FlatAppearance.MouseOverBackColor=primary?Color.FromArgb(29,137,143):Canvas;
        button.FlatAppearance.MouseDownBackColor=primary?Color.FromArgb(17,105,110):Color.FromArgb(228,239,242);
    }
    public static void Grid(DataGridView grid)
    {
        grid.Font=Body;grid.EnableHeadersVisualStyles=false;grid.BorderStyle=BorderStyle.None;
        grid.BackgroundColor=Color.White;grid.GridColor=Border;grid.CellBorderStyle=DataGridViewCellBorderStyle.SingleHorizontal;
        grid.ColumnHeadersBorderStyle=DataGridViewHeaderBorderStyle.None;grid.RowHeadersVisible=false;
        grid.ColumnHeadersDefaultCellStyle=new DataGridViewCellStyle {BackColor=Color.FromArgb(239,246,248),ForeColor=Ink,Font=Bold,Padding=new Padding(8,0,8,0),SelectionBackColor=Color.FromArgb(239,246,248),SelectionForeColor=Ink};
        grid.ColumnHeadersHeight=40;grid.ColumnHeadersHeightSizeMode=DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        grid.DefaultCellStyle=new DataGridViewCellStyle {ForeColor=Ink,BackColor=Color.White,SelectionBackColor=Color.FromArgb(222,241,241),SelectionForeColor=Ink,Padding=new Padding(8,4,8,4)};
        grid.AlternatingRowsDefaultCellStyle.BackColor=Color.FromArgb(250,252,253);grid.RowTemplate.Height=38;
    }
    public static Color RoomColor(RoomStatus status)=>status switch
    {
        RoomStatus.Trong=>Teal,RoomStatus.DaDat=>Color.FromArgb(123,96,168),RoomStatus.DangO=>Color.FromArgb(62,118,169),RoomStatus.DangDon=>Amber,RoomStatus.BaoTri=>Muted,_=>Muted
    };
    public static Color RoomTint(RoomStatus status)=>status switch
    {
        RoomStatus.Trong=>Color.FromArgb(223,245,241),RoomStatus.DaDat=>Color.FromArgb(240,234,248),
        RoomStatus.DangO=>Color.FromArgb(226,239,249),RoomStatus.DangDon=>Color.FromArgb(253,241,219),
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
        Width=88;Height=74;Margin=new Padding(0,0,6,7);FlatStyle=FlatStyle.Flat;FlatAppearance.BorderSize=0;Cursor=Cursors.Hand;
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
        using var badge=AppTheme.Rounded(new RectangleF(8,42,Width-16,22),8);
        using var badgeFill=new SolidBrush(AppTheme.RoomTint(room.Status));g.FillPath(badgeFill,badge);
        TextRenderer.DrawText(g,Ui.Status(room.Status),AppTheme.Small,new Rectangle(8,42,Width-16,22),accent,TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis);
        if(Reservations>0){using var dot=new SolidBrush(AppTheme.Amber);g.FillEllipse(dot,Width-16,12,7,7);}
        if(Focused)ControlPaint.DrawFocusRectangle(g,new Rectangle(5,5,Width-10,Height-10));
    }
}
