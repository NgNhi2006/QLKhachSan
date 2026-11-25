using Microsoft.Data.SqlClient;
using System.Globalization;
using System.Text.RegularExpressions;
using QLKhachSan.DAL;
using QLKhachSan.DTO;

namespace QLKhachSan.GUI;

internal static class Ui
{
    public static void Error(IWin32Window? owner,Exception ex)
    {
        var message=ex switch
        {
            BusinessException => ex.Message,
            SchemaMigrationException => ex.Message,
            SqlException s when s.Number is 26 or 40 or 53 or -1 => "Không kết nối được SQL Server (.\\SQLEXPRESS). Hãy mở Services, khởi động SQL Server (SQLEXPRESS), rồi bấm Thử lại kết nối. Nếu dịch vụ không khởi động, kiểm tra nhật ký lỗi SQL Server hoặc nhờ quản trị máy hỗ trợ.",
            SqlException s when s.Number==51001 => "Hệ thống đang xử lý giao dịch khác. Vui lòng thử lại.",
            SqlException s when s.Number==51101 => "Kỳ kế toán đã khóa; không thể ghi hoặc sửa chứng từ thuộc kỳ này.",
            SqlException s when s.Number==51102 => "Ca đã khóa; giao dịch thuộc ca không thể sửa hoặc hủy.",
            SqlException s when s.Number==51103 => "Số dư mở sổ chỉ được ghi một lần, không thể sửa hoặc xóa.",
            SqlException s when s.Number==51104 => "Kỳ kế toán đã khóa không thể sửa hoặc mở lại.",
            SqlException => "Không thể hoàn tất thao tác SQL Server. Dữ liệu không được thay bằng dữ liệu mẫu. Hãy kiểm tra kết nối, làm mới và kiểm tra kết quả trước khi thử lại.",
            _ => "Không thể hoàn tất thao tác. Hãy kiểm tra cấu hình hoặc dữ liệu và thử lại."
        };
        Log(ex);
        MessageBox.Show(owner,message,"Thông báo",MessageBoxButtons.OK,MessageBoxIcon.Warning);
    }
    public static void Info(IWin32Window? owner,string message) => MessageBox.Show(owner,message,"Thông báo",MessageBoxButtons.OK,MessageBoxIcon.Information);
    public static void Log(Exception ex)
    {
        // Stack frames identify the failing operation without logging inputs or SQL text.
        try
        {
            var folder=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"QLKhachSan");
            Directory.CreateDirectory(folder);
            var frames=new System.Diagnostics.StackTrace(ex,false).GetFrames();
            var context=string.Join(" > ",frames.Take(8).Select(f=>f.GetMethod()?.DeclaringType?.FullName+"."+f.GetMethod()?.Name));
            File.AppendAllText(Path.Combine(folder,"errors.log"),$"{DateTimeOffset.Now:O} {ex.GetType().Name} {(ex is SqlException sql?sql.Number:ex.HResult)} {context}{Environment.NewLine}");
        }
        catch(IOException) { } catch(UnauthorizedAccessException) { }
    }
    public static bool Confirm(IWin32Window owner,string text) => MessageBox.Show(owner,text,"Xác nhận",MessageBoxButtons.YesNo,MessageBoxIcon.Question)==DialogResult.Yes;
    public static string Status(RoomStatus status) => status switch
    {
        RoomStatus.Trong=>"Trống",RoomStatus.DaDat=>"Đã đặt",RoomStatus.DangO=>"Đang ở",RoomStatus.DangDon=>"Đang dọn",RoomStatus.BaoTri=>"Bảo trì",_=>"Không hợp lệ"
    };
    public static DataGridView Grid()
    {
        var grid=new DataGridView { Dock=DockStyle.Fill,ReadOnly=true,AllowUserToAddRows=false,AllowUserToDeleteRows=false,MultiSelect=false,SelectionMode=DataGridViewSelectionMode.FullRowSelect,AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill,RowHeadersVisible=false,BackgroundColor=Color.White,AutoGenerateColumns=true };
        ExcelExport.Attach(grid,"Báo cáo");
        AppTheme.Grid(grid);return grid;
    }
    public static void Clear(Control parent)
    {
        while(parent.Controls.Count>0) parent.Controls[0].Dispose();
    }
    public static ComboBox Combo<T>(IEnumerable<T> values) => new() { DropDownStyle=ComboBoxStyle.DropDownList,BindingContext=new BindingContext(),DataSource=values.ToList(),Dock=DockStyle.Top,Font=AppTheme.Body,ForeColor=AppTheme.Ink,BackColor=Color.White };
    public static DateTimePicker DatePicker(DateTime value) => new() { Format=DateTimePickerFormat.Custom,CustomFormat="dd/MM/yyyy HH:mm",Value=value,Dock=DockStyle.Top,Font=AppTheme.Body };
    public static NumericUpDown Number(int max,int value=1) => new() { Minimum=1,Maximum=max,Value=value,Dock=DockStyle.Top,Font=AppTheme.Body,ForeColor=AppTheme.Ink };
    public static NumericUpDown Money(decimal value=0) => new() {Minimum=0,Maximum=1_000_000_000,Value=Math.Clamp(value,0,1_000_000_000),ThousandsSeparator=true,Increment=10000,Dock=DockStyle.Top,Font=AppTheme.Body,ForeColor=AppTheme.Ink};
    public static NumericUpDown StockQuantity(decimal value=1,bool allowZero=false) => new()
    {
        Minimum=allowZero?0:0.001m,Maximum=1_000_000,Value=value,DecimalPlaces=3,
        Increment=1,ThousandsSeparator=true,Dock=DockStyle.Top,Font=AppTheme.Body,ForeColor=AppTheme.Ink
    };
    public static decimal ParseStockQuantity(string text)
    {
        var value=text.Trim().Replace(" ","").Replace("\u00a0","");
        if(Regex.IsMatch(value,@"^[1-9][0-9]{0,2}(?:[,.][0-9]{3})+$"))
            value=value.Replace(",","").Replace(".","");
        else value=value.Replace(',','.');
        if(!decimal.TryParse(value,NumberStyles.AllowDecimalPoint,CultureInfo.InvariantCulture,out var quantity)
            || quantity<=0 || quantity>1_000_000)
            throw new BusinessException("Số lượng phải lớn hơn 0 và không quá 1.000.000. Ví dụ: 11000 hoặc 11,000 đều là 11 nghìn.");
        return quantity;
    }
    public static TextBox Text(int max=100,bool password=false) => new() { MaxLength=max,UseSystemPasswordChar=password,Dock=DockStyle.Top,Font=AppTheme.Body,ForeColor=AppTheme.Ink,BackColor=Color.White,BorderStyle=BorderStyle.FixedSingle };
}

// Dialog layout is shared; transaction and validation remain in the BLL.
internal sealed class InputDialog : Form
{
    private readonly TableLayoutPanel fields=new() { Dock=DockStyle.Fill,ColumnCount=1,AutoScroll=true,Padding=new Padding(16) };
    private readonly TableLayoutPanel actions=new() { Dock=DockStyle.Bottom,ColumnCount=1,AutoSize=true,Padding=new Padding(16,4,16,12) };
    private bool saving;
    private readonly bool labelsBeside;
    private int fieldRow;
    private readonly Font dialogFont=AppTheme.TextFont(10);
    public InputDialog(string title,int width=540,int height=620,bool labelsBeside=false)
    {
        this.labelsBeside=labelsBeside;
        Text=title;Icon=AppTheme.AppIcon; Size=new Size(width,height); MinimumSize=new Size(420,320);
        StartPosition=FormStartPosition.CenterParent; AutoScaleMode=AutoScaleMode.Dpi;
        Font=dialogFont; BackColor=AppTheme.Canvas;ForeColor=AppTheme.Ink;
        Load+=(_,_)=>
        {
            var working=Screen.FromControl(this).WorkingArea;
            MaximumSize=new Size(Math.Max(420,working.Width-24),Math.Max(320,working.Height-24));
            Size=new Size(Math.Min(Width,MaximumSize.Width),Math.Min(Height,MaximumSize.Height));
        };
        fields.Padding=new Padding(26,20,26,20);fields.BackColor=Color.White;
        actions.Padding=new Padding(24,12,24,16);actions.BackColor=AppTheme.Canvas;
        if(labelsBeside)
        {
            fields.ColumnCount=2;
            fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,35));
            fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,65));
        }
        else fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
        actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
        Controls.Add(fields);
        Controls.Add(actions);
        var heading=new Panel {Dock=DockStyle.Top,Height=76,BackColor=Color.White};
        heading.Paint+=(_,e)=>
        {
            using var accent=new SolidBrush(AppTheme.Focus);
            using var edge=new Pen(AppTheme.Border);
            e.Graphics.FillRectangle(accent,24,18,40,3);
            e.Graphics.DrawLine(edge,0,heading.Height-1,heading.Width,heading.Height-1);
        };
        heading.Controls.Add(new Label {Text=title,Font=AppTheme.Title,ForeColor=AppTheme.Navy,AutoEllipsis=true,Dock=DockStyle.Fill,Padding=new Padding(24,28,12,0)});
        Controls.Add(heading);
        FormClosing+=(_,e)=> { if(saving)e.Cancel=true; };
    }
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if(disposing)dialogFont.Dispose();
    }
    public Label? Add(string label,Control control,int height=0)
    {
        control.Dock=DockStyle.Top; control.Margin=new Padding(0,0,0,8);
        if(height>0) control.Height=height;
        if(labelsBeside)
        {
            fields.RowStyles.Add(new RowStyle(SizeType.AutoSize));fields.RowCount=fieldRow+1;
            var caption=new Label {Text=label,Font=AppTheme.Body,ForeColor=AppTheme.Muted,AutoSize=true,Dock=DockStyle.Top,Margin=new Padding(0,4,12,14)};
            fields.Controls.Add(caption,0,fieldRow);
            fields.Controls.Add(control,1,fieldRow++);
            return caption;
        }
        else
        {
            Label? caption=null;
            if(!string.IsNullOrEmpty(label))
            {
                caption=new Label {Text=label,Font=AppTheme.Bold,ForeColor=AppTheme.Muted,AutoSize=true,Margin=new Padding(0,8,0,6)};
                fields.RowStyles.Add(new RowStyle(SizeType.AutoSize));fields.RowCount=++fieldRow;fields.Controls.Add(caption,0,fieldRow-1);
            }
            fields.RowStyles.Add(new RowStyle(SizeType.AutoSize));fields.RowCount=++fieldRow;fields.Controls.Add(control,0,fieldRow-1);
            return caption;
        }
    }
    public void Note(string text)
    {
        var note=new Label {Text=text,ForeColor=AppTheme.Muted,Font=AppTheme.Small,AutoSize=true,MaximumSize=new Size(900,0),Margin=new Padding(0,12,0,12)};
        fields.RowStyles.Add(new RowStyle(SizeType.AutoSize));fields.RowCount=fieldRow+1;
        fields.Controls.Add(note,0,fieldRow++);
        if(labelsBeside)fields.SetColumnSpan(note,2);
    }
    public void ScrollTo(Control control)
    {
        if(IsHandleCreated)BeginInvoke(() => fields.ScrollControlIntoView(control));
    }
    public void AddActionConfirmation(string label,CheckBox confirmation)
    {
        var panel=new Panel {Dock=DockStyle.Top,Height=58,BackColor=AppTheme.Canvas,Margin=new Padding(0,0,0,8)};
        panel.Controls.Add(new Label {Text=label,Dock=DockStyle.Top,Height=24,Font=AppTheme.Bold,ForeColor=AppTheme.Muted});
        confirmation.AutoSize=false;confirmation.Dock=DockStyle.Bottom;confirmation.Height=30;
        panel.Controls.Add(confirmation);
        actions.Controls.Add(panel);
    }
    public void CompactActions()
    {
        var buttons=actions.Controls.Cast<Control>().ToArray();
        actions.Controls.Clear();actions.ColumnCount=2;actions.ColumnStyles.Clear();
        actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,50));actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,50));
        for(var i=0;i<buttons.Length;i++)actions.Controls.Add(buttons[i],i%2,i/2);
    }
    public Button Action(string text,Func<Task> save,bool close=true)
    {
        var button=new RoundedActionButton { Text=text,Height=44,Dock=DockStyle.Top,BackColor=AppTheme.Blue,ForeColor=Color.White };
        button.Click+=async (_,_)=>
        {
            if(saving)return; saving=true; fields.Enabled=false; actions.Enabled=false; UseWaitCursor=true;
            try { await save(); if(close){saving=false;DialogResult=DialogResult.OK;Close();} }
            catch(Exception ex){ Ui.Error(this,ex); }
            finally { saving=false;if(!IsDisposed){fields.Enabled=true;actions.Enabled=true;UseWaitCursor=false;} }
        };
        actions.Controls.Add(button); AcceptButton=button; return button;
    }
}
