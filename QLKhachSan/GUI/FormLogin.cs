using QLKhachSan.BLL;
using QLKhachSan.DAL;
using QLKhachSan.DTO;

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
        Text="Lưu mật khẩu trên máy này",AutoSize=true,Location=new Point(545,298),
        Font=new Font("Segoe UI",9),ForeColor=Color.FromArgb(71,85,105)
    };
    public UserSession? Session { get; private set; }
    public FormLogin()
    {
        InitializeComponent(); AcceptButton=btnDangNhap;
        pnlBanner.BackColor=AppTheme.Navy;lblHotelTitle.Text="HOTEL DESK";lblHotelTitle.Font=AppTheme.Title;
        pnlBanner.Controls.Add(new PictureBox {Image=UiIcons.Create("bed",Color.FromArgb(234,179,75),64),SizeMode=PictureBoxSizeMode.Zoom,Location=new Point(41,77),Size=new Size(78,78)});
        lblHotelSub.Text="Quản lý lưu trú\nVận hành rõ ràng, phục vụ chu đáo.";
        lblTitleLogin.ForeColor=AppTheme.Ink;AppTheme.Button(btnDangNhap,true);
        txtTenDangNhap.Visible=false;txtTenDangNhap.TabStop=false;txtMatKhau.MaxLength=128;
        accountPicker.Bounds=txtTenDangNhap.Bounds;accountPicker.Font=txtTenDangNhap.Font;
        accountPicker.TabIndex=0;accountPicker.AutoCompleteMode=AutoCompleteMode.SuggestAppend;
        accountPicker.AutoCompleteSource=AutoCompleteSource.ListItems;
        Controls.Add(accountPicker);accountPicker.BringToFront();
        Controls.Add(rememberPassword);
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
        btnDangNhap.Enabled=false; busy=true;
        try
        {
            await SchemaMigrator.EnsureAsync();
            setup=await auth.NeedsSetupAsync();
            txtMatKhau.MaxLength=setup?5:128;
            initialized=true;
            lblSubtitle.Text=setup?"Tạo quản trị đầu tiên (mật khẩu 1–5 ký tự)":"Vui lòng nhập thông tin để đăng nhập";
            lblSubtitle.MaximumSize=new Size(370,0);lblSubtitle.AutoSize=true;
            btnDangNhap.Text=setup?"TẠO QUẢN TRỊ":"ĐĂNG NHẬP";
        }
        catch(Exception ex){Ui.Error(this,ex);btnDangNhap.Text="THỬ LẠI KẾT NỐI";}
        finally{busy=false;btnDangNhap.Enabled=true;}
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
        busy=true;btnDangNhap.Enabled=false;
        try
        {
            Session=setup?await auth.SetupAsync(accountPicker.Text,txtMatKhau.Text):await auth.LoginAsync(accountPicker.Text,txtMatKhau.Text);
            try
            {
                RememberedLogin.Save(accountPicker.Text.Trim(),rememberPassword.Checked?txtMatKhau.Text:"");
            }
            catch(Exception ex)when(ex is IOException or UnauthorizedAccessException or System.Security.Cryptography.CryptographicException)
            {MessageBox.Show(this,"Đăng nhập thành công nhưng không lưu được mật khẩu trên máy này.","Lưu mật khẩu");}
            busy=false;DialogResult=DialogResult.OK;Close();
        }
        catch(Exception ex){Ui.Error(this,ex);txtMatKhau.Clear();txtMatKhau.Focus();}
        finally{busy=false;btnDangNhap.Enabled=true;}
    }
}
