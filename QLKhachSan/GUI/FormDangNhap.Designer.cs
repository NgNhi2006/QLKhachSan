#nullable enable
namespace QLKhachSan.GUI;

partial class FormLogin
{
    private System.ComponentModel.IContainer? components;
    private LoginBrandPanel pnlBanner = null!;
    private Panel pnlForm = null!;
    private LoginFieldPanel pnlAccountField = null!;
    private LoginFieldPanel pnlPasswordField = null!;
    private LoginCloseButton lblClose = null!;
    private Label lblTitleLogin = null!;
    private Label lblSubtitle = null!;
    private Label lblUsername = null!;
    private TextBox txtTenDangNhap = null!;
    private Label lblPassword = null!;
    private TextBox txtMatKhau = null!;
    private CheckBox chkHienMatKhau = null!;
    private LoginActionButton btnDangNhap = null!;
    private Label lblStatus = null!;
    private ProgressBar progressLogin = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing) components?.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        SuspendLayout();
        AutoScaleMode = AutoScaleMode.Dpi;
        BackColor = AppTheme.Canvas;
        ClientSize = new Size(980, 600);
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.CenterScreen;
        Name = "FormLogin";
        Text = "Đăng nhập · Hotel Desk";
        Font = new Font("Segoe UI", 10F);

        pnlBanner = new LoginBrandPanel { Dock = DockStyle.Left, Width = 410, TabStop = false };
        pnlForm = new Panel { Dock = DockStyle.Fill, BackColor = AppTheme.Canvas };
        Controls.Add(pnlForm);
        Controls.Add(pnlBanner);
        pnlForm.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 6, BackColor = AppTheme.Amber, TabStop = false });

        lblClose = new LoginCloseButton
        {
            Size = new Size(42, 42),
            Location = new Point(515, 16), TabIndex = 7
        };
        lblClose.Click += lblClose_Click;
        pnlForm.Controls.Add(lblClose);
        pnlForm.Controls.Add(new Label
        {
            Text = "HOTEL DESK  /  ĐIỀU HÀNH KHÁCH SẠN", Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold),
            ForeColor = AppTheme.Amber, AutoSize = true, Location = new Point(68, 100)
        });
        lblTitleLogin = new Label
        {
            Text = "Đăng nhập", Font = new Font("Segoe UI Semibold", 27F, FontStyle.Bold),
            ForeColor = AppTheme.Ink, AutoSize = true, Location = new Point(63, 130)
        };
        pnlForm.Controls.Add(lblTitleLogin);
        lblSubtitle = new Label
        {
            Text = "Nhập thông tin để tiếp tục công việc của bạn.", Font = new Font("Segoe UI", 10F),
            ForeColor = AppTheme.Muted, Size = new Size(430, 44),
            Location = new Point(68, 190)
        };
        pnlForm.Controls.Add(lblSubtitle);

        lblUsername = new Label
        {
            Text = "Tên đăng nhập", Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold),
            ForeColor = AppTheme.Ink, AutoSize = true, Location = new Point(68, 246)
        };
        pnlForm.Controls.Add(lblUsername);
        pnlAccountField = new LoginFieldPanel("people") { Bounds = new Rectangle(68, 273, 430, 50), TabIndex = 0, TabStop = false };
        pnlForm.Controls.Add(pnlAccountField);
        txtTenDangNhap = new TextBox { Visible = false, TabStop = false };
        pnlForm.Controls.Add(txtTenDangNhap);

        lblPassword = new Label
        {
            Text = "Mật khẩu", Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold),
            ForeColor = AppTheme.Ink, AutoSize = true, Location = new Point(68, 341)
        };
        pnlForm.Controls.Add(lblPassword);
        pnlPasswordField = new LoginFieldPanel("key") { Bounds = new Rectangle(68, 368, 430, 50), TabIndex = 1, TabStop = false };
        pnlForm.Controls.Add(pnlPasswordField);
        txtMatKhau = new TextBox
        {
            BorderStyle = BorderStyle.None, BackColor = Color.White, Font = new Font("Segoe UI", 11F),
            PlaceholderText = "Nhập mật khẩu", UseSystemPasswordChar = true, MaxLength = 128,
            Location = new Point(52, 14), Size = new Size(357, 25), TabIndex = 1
        };
        pnlPasswordField.Controls.Add(txtMatKhau);
        pnlPasswordField.TrackFocus(txtMatKhau);

        chkHienMatKhau = new CheckBox
        {
            Text = "Hiện mật khẩu", AutoSize = true, Font = new Font("Segoe UI", 9F),
            ForeColor = Color.FromArgb(89, 107, 127), BackColor = pnlForm.BackColor,
            Location = new Point(68, 437), TabIndex = 2
        };
        chkHienMatKhau.CheckedChanged += chkHienMatKhau_CheckedChanged;
        pnlForm.Controls.Add(chkHienMatKhau);
        btnDangNhap = new LoginActionButton
        {
            Text = "ĐĂNG NHẬP  →", Bounds = new Rectangle(68, 480, 430, 52), TabIndex = 4
        };
        btnDangNhap.Click += btnDangNhap_Click;
        pnlForm.Controls.Add(btnDangNhap);
        lblStatus = new Label
        {
            Text = "Đang kiểm tra kết nối...", ForeColor = Color.FromArgb(90, 113, 133),
            Font = new Font("Segoe UI", 8.5F), TextAlign = ContentAlignment.MiddleCenter,
            Bounds = new Rectangle(68, 547, 430, 24), Visible = false
        };
        pnlForm.Controls.Add(lblStatus);
        progressLogin = new ProgressBar
        {
            Style = ProgressBarStyle.Marquee, MarqueeAnimationSpeed = 28,
            Bounds = new Rectangle(68, 537, 430, 3), Visible = false, TabStop = false
        };
        pnlForm.Controls.Add(progressLogin);
        ResumeLayout(false);
    }
}
