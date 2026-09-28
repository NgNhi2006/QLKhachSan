using Microsoft.Data.SqlClient;
using QLKhachSan.DTO;

namespace QLKhachSan.GUI;

internal static class Ui
{
    public static void Error(IWin32Window? owner,Exception ex)
    {
        var message=ex switch
        {
            BusinessException => ex.Message,
            SqlException s when s.Number==51001 => "Hệ thống đang xử lý giao dịch khác. Vui lòng thử lại.",
            SqlException => "Không thể hoàn tất thao tác SQL Server. Dữ liệu không được thay bằng dữ liệu mẫu. Hãy kiểm tra kết nối, làm mới và kiểm tra kết quả trước khi thử lại.",
            _ => "Không thể hoàn tất thao tác. Hãy kiểm tra cấu hình hoặc dữ liệu và thử lại."
        };
        Log(ex);
        MessageBox.Show(owner,message,"Thông báo",MessageBoxButtons.OK,MessageBoxIcon.Warning);
    }
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
        AppTheme.Grid(grid);return grid;
    }
    public static void Clear(Control parent)
    {
        while(parent.Controls.Count>0) parent.Controls[0].Dispose();
    }
    public static ComboBox Combo<T>(IEnumerable<T> values) => new() { DropDownStyle=ComboBoxStyle.DropDownList,BindingContext=new BindingContext(),DataSource=values.ToList(),Dock=DockStyle.Top };
    public static DateTimePicker DatePicker(DateTime value) => new() { Format=DateTimePickerFormat.Custom,CustomFormat="dd/MM/yyyy HH:mm",Value=value,Dock=DockStyle.Top };
    public static NumericUpDown Number(int max,int value=1) => new() { Minimum=1,Maximum=max,Value=value,Dock=DockStyle.Top };
    public static NumericUpDown Money(decimal value=0) => new() {Minimum=0,Maximum=1_000_000_000,Value=Math.Clamp(value,0,1_000_000_000),ThousandsSeparator=true,Increment=10000,Dock=DockStyle.Top};
    public static TextBox Text(int max=100,bool password=false) => new() { MaxLength=max,UseSystemPasswordChar=password,Dock=DockStyle.Top };
}

// Dialog layout is shared; transaction and validation remain in the BLL.
internal sealed class InputDialog : Form
{
    private readonly TableLayoutPanel fields=new() { Dock=DockStyle.Fill,ColumnCount=1,AutoScroll=true,Padding=new Padding(16) };
    private readonly TableLayoutPanel actions=new() { Dock=DockStyle.Bottom,ColumnCount=1,AutoSize=true,Padding=new Padding(16,4,16,12) };
    private bool saving;
    private readonly bool labelsBeside;
    private int fieldRow;
    private readonly Font dialogFont=new("Segoe UI",10);
    public InputDialog(string title,int width=540,int height=620,bool labelsBeside=false)
    {
        this.labelsBeside=labelsBeside;
        Text=title; Size=new Size(width,height); MinimumSize=new Size(420,320);
        StartPosition=FormStartPosition.CenterParent; AutoScaleMode=AutoScaleMode.Dpi;
        Font=dialogFont; BackColor=Color.White;ForeColor=AppTheme.Ink;
        fields.Padding=new Padding(24,16,24,16);actions.Padding=new Padding(20,12,20,16);actions.BackColor=AppTheme.Canvas;
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
        var heading=new Panel {Dock=DockStyle.Top,Height=74,BackColor=Color.White};
        heading.Paint+=(_,e)=>
        {
            using var accent=new SolidBrush(AppTheme.Teal);
            using var edge=new Pen(AppTheme.Border);
            e.Graphics.FillRectangle(accent,0,0,4,heading.Height);
            e.Graphics.DrawLine(edge,0,heading.Height-1,heading.Width,heading.Height-1);
        };
        heading.Controls.Add(new Label {Text=title,Font=AppTheme.Title,ForeColor=AppTheme.Ink,AutoEllipsis=true,Dock=DockStyle.Fill,Padding=new Padding(22,18,12,0)});
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
        var button=new Button { Text=text,Height=44,Dock=DockStyle.Top,BackColor=Color.FromArgb(37,99,235),ForeColor=Color.White,FlatStyle=FlatStyle.Flat };
        AppTheme.Button(button,true);
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
