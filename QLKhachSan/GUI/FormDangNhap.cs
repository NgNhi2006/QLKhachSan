using QLKhachSan.BLL;
using QLKhachSan.DAL;
using QLKhachSan.DTO;
using System.Runtime.InteropServices;

namespace QLKhachSan.GUI;

public partial class FormLogin : Form
{
    private readonly AuthService auth=new(new HotelRepository());
    private bool setup;
    private bool busy;
    private bool initialized;
    private bool syncingAccount;
    private readonly ComboBox accountPicker=new() {DropDownStyle=ComboBoxStyle.DropDown,MaxLength=50};
    private readonly CheckBox rememberPassword=new()
    {
        Text="Ghi nhớ mật khẩu",AutoSize=true,Location=new Point(323,437),
        Font=new Font("Segoe UI",9),ForeColor=Color.FromArgb(89,107,127)
    };
    public UserSession? Session { get; private set; }
    public FormLogin()
    {
        InitializeComponent(); AcceptButton=btnDangNhap;
        accountPicker.Bounds=new Rectangle(52,9,357,32);
        accountPicker.Font=new Font("Segoe UI",10.5f);
        accountPicker.FlatStyle=FlatStyle.Flat;
        accountPicker.BackColor=Color.White;
        accountPicker.TabIndex=0;accountPicker.AutoCompleteMode=AutoCompleteMode.SuggestAppend;
        accountPicker.AutoCompleteSource=AutoCompleteSource.ListItems;
        pnlAccountField.Controls.Add(accountPicker);
        pnlAccountField.TrackFocus(accountPicker);
        rememberPassword.TabIndex=3;
        pnlForm.Controls.Add(rememberPassword);
        pnlBanner.MouseDown+=BeginWindowDrag;
        foreach(Control control in pnlBanner.Controls)
            if(control is Label)control.MouseDown+=BeginWindowDrag;
        pnlForm.MouseDown+=BeginWindowDrag;
        lblTitleLogin.MouseDown+=BeginWindowDrag;
        lblSubtitle.MouseDown+=BeginWindowDrag;
        var accounts=RememberedLogin.Load();
        accountPicker.Items.AddRange(accounts.Select(x=>(object)x.Username).ToArray());
        if(accounts.Count>0)
        {
            accountPicker.Text=accounts[0].Username;
            txtMatKhau.Text=accounts[0].Password;
            rememberPassword.Checked=!string.IsNullOrEmpty(accounts[0].Password);
        }
        accountPicker.SelectedIndexChanged+=(_,_)=>SyncAccount();
        accountPicker.TextChanged+=(_,_)=>SyncAccount();
        rememberPassword.CheckedChanged+=(_,_)=>
        {
            if(!syncingAccount && !rememberPassword.Checked)
                try{RememberedLogin.ForgetPassword(accountPicker.Text.Trim());}
                catch(Exception ex)when(ex is IOException or UnauthorizedAccessException)
                {Ui.Error(this,ex);}
        };
        Shown+=async (_,_)=>await InitializeAsync();
        FormClosing+=(_,e)=> { if(busy)e.Cancel=true; };
    }
    [DllImport("user32.dll")] private static extern bool ReleaseCapture();
    [DllImport("user32.dll")] private static extern IntPtr SendMessage(IntPtr hWnd,int msg,IntPtr wParam,IntPtr lParam);
    private void BeginWindowDrag(object? sender,MouseEventArgs e)
    {
        if(e.Button!=MouseButtons.Left)return;
        ReleaseCapture();
        SendMessage(Handle,0xA1,(IntPtr)2,IntPtr.Zero);
    }
    private void SetWorking(bool working,string message="")
    {
        busy=working;
        btnDangNhap.Enabled=!working;
        progressLogin.Visible=working;
        lblStatus.Text=message;
        lblStatus.Visible=working || !string.IsNullOrEmpty(message);
        UseWaitCursor=working;
    }
    private void SyncAccount()
    {
        if(syncingAccount)return;
        syncingAccount=true;
        try
        {
            var account=RememberedLogin.Load().FirstOrDefault(x=>string.Equals(x.Username,accountPicker.Text.Trim(),StringComparison.OrdinalIgnoreCase));
            txtMatKhau.Text=account.Password??"";
            rememberPassword.Checked=!string.IsNullOrEmpty(account.Password);
        }
        finally{syncingAccount=false;}
    }
    private async Task InitializeAsync()
    {
        SetWorking(true,"Đang kiểm tra kết nối...");
        try
        {
            await SchemaMigrator.EnsureAsync();
            setup=await auth.NeedsSetupAsync();
            txtMatKhau.MaxLength=setup?5:128;
            initialized=true;
            lblTitleLogin.Text=setup?"Thiết lập quản trị":"Đăng nhập";
            lblSubtitle.Text=setup?"Tạo tài khoản quản trị đầu tiên. Mật khẩu gồm 1–5 ký tự.":"Nhập thông tin để tiếp tục công việc của bạn.";
            btnDangNhap.Text=setup?"TẠO QUẢN TRỊ  →":"ĐĂNG NHẬP  →";
        }
        catch(Exception ex){Ui.Error(this,ex);btnDangNhap.Text="THỬ LẠI KẾT NỐI  →";}
        finally{SetWorking(false);}
    }
    private void lblClose_Click(object? sender,EventArgs e){if(!busy)Close();}
    private void chkHienMatKhau_CheckedChanged(object? sender,EventArgs e)=>txtMatKhau.UseSystemPasswordChar=!chkHienMatKhau.Checked;
    private async void btnDangNhap_Click(object? sender,EventArgs e)
    {
        if(busy)return;
        if(!initialized){await InitializeAsync();return;}
        if(setup)
        {
            using var confirm=new InputDialog("Xác nhận mật khẩu quản trị",500,300);
            var password=Ui.Text(5,true); confirm.Add("Nhập lại mật khẩu",password);
            confirm.Action("XÁC NHẬN",()=> {if(password.Text!=txtMatKhau.Text)throw new BusinessException("Hai mật khẩu không khớp.");return Task.CompletedTask;});
            if(confirm.ShowDialog(this)!=DialogResult.OK)return;
        }
        SetWorking(true,setup?"Đang tạo tài khoản quản trị...":"Đang xác thực tài khoản...");
        try
        {
            Session=setup?await auth.SetupAsync(accountPicker.Text,txtMatKhau.Text):await auth.LoginAsync(accountPicker.Text,txtMatKhau.Text);
            try
            {
                RememberedLogin.Save(accountPicker.Text.Trim(),rememberPassword.Checked?txtMatKhau.Text:"");
            }
            catch(Exception ex)when(ex is IOException or UnauthorizedAccessException or System.Security.Cryptography.CryptographicException)
            {MessageBox.Show(this,"Đăng nhập thành công nhưng không lưu được mật khẩu trên máy này.","Lưu mật khẩu");}
            SetWorking(false);DialogResult=DialogResult.OK;Close();
        }
        catch(Exception ex){Ui.Error(this,ex);txtMatKhau.Clear();txtMatKhau.Focus();}
        finally{if(!IsDisposed)SetWorking(false);}
    }
}
