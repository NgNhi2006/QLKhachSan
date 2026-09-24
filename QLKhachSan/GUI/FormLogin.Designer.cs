namespace QLKhachSan.GUI
{
    partial class FormLogin
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            pnlBanner = new System.Windows.Forms.Panel();
            lblHotelSub = new System.Windows.Forms.Label();
            lblHotelTitle = new System.Windows.Forms.Label();
            lblClose = new System.Windows.Forms.Label();
            lblTitleLogin = new System.Windows.Forms.Label();
            lblSubtitle = new System.Windows.Forms.Label();
            lblUsername = new System.Windows.Forms.Label();
            txtTenDangNhap = new System.Windows.Forms.TextBox();
            lblPassword = new System.Windows.Forms.Label();
            txtMatKhau = new System.Windows.Forms.TextBox();
            chkHienMatKhau = new System.Windows.Forms.CheckBox();
            btnDangNhap = new System.Windows.Forms.Button();
            pnlBanner.SuspendLayout();
            SuspendLayout();
            // 
            // pnlBanner (Cột trái xanh navy)
            // 
            pnlBanner.BackColor = System.Drawing.Color.FromArgb(30, 41, 59);
            pnlBanner.Controls.Add(lblHotelSub);
            pnlBanner.Controls.Add(lblHotelTitle);
            pnlBanner.Dock = System.Windows.Forms.DockStyle.Left;
            pnlBanner.Location = new System.Drawing.Point(0, 0);
            pnlBanner.Name = "pnlBanner";
            pnlBanner.Size = new System.Drawing.Size(320, 480);
            pnlBanner.TabIndex = 0;
            // 
            // lblHotelTitle
            // 
            lblHotelTitle.AutoSize = true;
            lblHotelTitle.Font = new System.Drawing.Font("Segoe UI", 20F, System.Drawing.FontStyle.Bold);
            lblHotelTitle.ForeColor = System.Drawing.Color.White;
            lblHotelTitle.Location = new System.Drawing.Point(40, 180);
            lblHotelTitle.Name = "lblHotelTitle";
            lblHotelTitle.Size = new System.Drawing.Size(220, 46);
            lblHotelTitle.TabIndex = 0;
            lblHotelTitle.Text = "GRAND HOTEL";
            // 
            // lblHotelSub
            // 
            lblHotelSub.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            lblHotelSub.ForeColor = System.Drawing.Color.FromArgb(203, 213, 225);
            lblHotelSub.Location = new System.Drawing.Point(42, 235);
            lblHotelSub.Name = "lblHotelSub";
            lblHotelSub.Size = new System.Drawing.Size(230, 50);
            lblHotelSub.TabIndex = 1;
            lblHotelSub.Text = "Hệ thống quản lý phòng và dịch vụ khách sạn";
            // 
            // lblClose (Nút đóng X góc trên phải)
            // 
            lblClose.AutoSize = true;
            lblClose.Cursor = System.Windows.Forms.Cursors.Hand;
            lblClose.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            lblClose.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            lblClose.Location = new System.Drawing.Point(765, 12);
            lblClose.Name = "lblClose";
            lblClose.Size = new System.Drawing.Size(25, 28);
            lblClose.TabIndex = 8;
            lblClose.Text = "✕";
            lblClose.Click += lblClose_Click;
            // 
            // lblTitleLogin
            // 
            lblTitleLogin.AutoSize = true;
            lblTitleLogin.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
            lblTitleLogin.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            lblTitleLogin.Location = new System.Drawing.Point(375, 60);
            lblTitleLogin.Name = "lblTitleLogin";
            lblTitleLogin.Size = new System.Drawing.Size(201, 41);
            lblTitleLogin.TabIndex = 1;
            lblTitleLogin.Text = "ĐĂNG NHẬP";
            // 
            // lblSubtitle
            // 
            lblSubtitle.AutoSize = true;
            lblSubtitle.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblSubtitle.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            lblSubtitle.Location = new System.Drawing.Point(378, 105);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new System.Drawing.Size(256, 20);
            lblSubtitle.TabIndex = 2;
            lblSubtitle.Text = "Vui lòng nhập thông tin để đăng nhập";
            // 
            // lblUsername
            // 
            lblUsername.AutoSize = true;
            lblUsername.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F, System.Drawing.FontStyle.Bold);
            lblUsername.ForeColor = System.Drawing.Color.FromArgb(51, 65, 85);
            lblUsername.Location = new System.Drawing.Point(378, 150);
            lblUsername.Name = "lblUsername";
            lblUsername.Size = new System.Drawing.Size(124, 21);
            lblUsername.TabIndex = 3;
            lblUsername.Text = "Tên đăng nhập";
            // 
            // txtTenDangNhap
            // 
            txtTenDangNhap.Font = new System.Drawing.Font("Segoe UI", 11F);
            txtTenDangNhap.Location = new System.Drawing.Point(380, 175);
            txtTenDangNhap.Name = "txtTenDangNhap";
            txtTenDangNhap.PlaceholderText = "Nhập tài khoản...";
            txtTenDangNhap.Size = new System.Drawing.Size(370, 32);
            txtTenDangNhap.TabIndex = 0;
            // 
            // lblPassword
            // 
            lblPassword.AutoSize = true;
            lblPassword.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F, System.Drawing.FontStyle.Bold);
            lblPassword.ForeColor = System.Drawing.Color.FromArgb(51, 65, 85);
            lblPassword.Location = new System.Drawing.Point(378, 230);
            lblPassword.Name = "lblPassword";
            lblPassword.Size = new System.Drawing.Size(77, 21);
            lblPassword.TabIndex = 4;
            lblPassword.Text = "Mật khẩu";
            // 
            // txtMatKhau
            // 
            txtMatKhau.Font = new System.Drawing.Font("Segoe UI", 11F);
            txtMatKhau.Location = new System.Drawing.Point(380, 255);
            txtMatKhau.Name = "txtMatKhau";
            txtMatKhau.PlaceholderText = "Nhập mật khẩu...";
            txtMatKhau.Size = new System.Drawing.Size(370, 32);
            txtMatKhau.TabIndex = 1;
            txtMatKhau.UseSystemPasswordChar = true;
            // 
            // chkHienMatKhau
            // 
            chkHienMatKhau.AutoSize = true;
            chkHienMatKhau.Font = new System.Drawing.Font("Segoe UI", 9F);
            chkHienMatKhau.ForeColor = System.Drawing.Color.FromArgb(71, 85, 105);
            chkHienMatKhau.Location = new System.Drawing.Point(380, 298);
            chkHienMatKhau.Name = "chkHienMatKhau";
            chkHienMatKhau.Size = new System.Drawing.Size(127, 24);
            chkHienMatKhau.TabIndex = 2;
            chkHienMatKhau.Text = "Hiện mật khẩu";
            chkHienMatKhau.UseVisualStyleBackColor = true;
            chkHienMatKhau.CheckedChanged += chkHienMatKhau_CheckedChanged;
            // 
            // btnDangNhap
            // 
            btnDangNhap.BackColor = System.Drawing.Color.FromArgb(37, 99, 235);
            btnDangNhap.Cursor = System.Windows.Forms.Cursors.Hand;
            btnDangNhap.FlatAppearance.BorderSize = 0;
            btnDangNhap.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnDangNhap.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            btnDangNhap.ForeColor = System.Drawing.Color.White;
            btnDangNhap.Location = new System.Drawing.Point(380, 350);
            btnDangNhap.Name = "btnDangNhap";
            btnDangNhap.Size = new System.Drawing.Size(370, 44);
            btnDangNhap.TabIndex = 3;
            btnDangNhap.Text = "ĐĂNG NHẬP";
            btnDangNhap.UseVisualStyleBackColor = false;
            btnDangNhap.Click += btnDangNhap_Click;
            // 
            // FormLogin
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.Color.White;
            ClientSize = new System.Drawing.Size(800, 480);
            Controls.Add(btnDangNhap);
            Controls.Add(chkHienMatKhau);
            Controls.Add(txtMatKhau);
            Controls.Add(lblPassword);
            Controls.Add(txtTenDangNhap);
            Controls.Add(lblUsername);
            Controls.Add(lblSubtitle);
            Controls.Add(lblTitleLogin);
            Controls.Add(lblClose);
            Controls.Add(pnlBanner);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            Name = "FormLogin";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Đăng nhập";
            pnlBanner.ResumeLayout(false);
            pnlBanner.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Panel pnlBanner;
        private System.Windows.Forms.Label lblHotelTitle;
        private System.Windows.Forms.Label lblHotelSub;
        private System.Windows.Forms.Label lblClose;
        private System.Windows.Forms.Label lblTitleLogin;
        private System.Windows.Forms.Label lblSubtitle;
        private System.Windows.Forms.Label lblUsername;
        private System.Windows.Forms.TextBox txtTenDangNhap;
        private System.Windows.Forms.Label lblPassword;
        private System.Windows.Forms.TextBox txtMatKhau;
        private System.Windows.Forms.CheckBox chkHienMatKhau;
        private System.Windows.Forms.Button btnDangNhap;
    }
}