namespace QLKhachSan.GUI
{
    partial class ucDashboard
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

        #region Component Designer generated code

        private void InitializeComponent()
        {
            pnlHeader = new System.Windows.Forms.Panel();
            lblHeaderTitle = new System.Windows.Forms.Label();
            flpHeaderRight = new System.Windows.Forms.FlowLayoutPanel();
            lblClock = new System.Windows.Forms.Label();
            btnRefresh = new System.Windows.Forms.Button();
            btnDangXuat = new System.Windows.Forms.Button();
            tlpCards = new System.Windows.Forms.TableLayoutPanel();
            pnlCard1 = new System.Windows.Forms.Panel();
            lblCard1Sub = new System.Windows.Forms.Label();
            lblCard1Value = new System.Windows.Forms.Label();
            lblCard1Title = new System.Windows.Forms.Label();
            pnlCard2 = new System.Windows.Forms.Panel();
            lblCard2Sub = new System.Windows.Forms.Label();
            lblCard2Value = new System.Windows.Forms.Label();
            lblCard2Title = new System.Windows.Forms.Label();
            pnlCard3 = new System.Windows.Forms.Panel();
            lblCard3Sub = new System.Windows.Forms.Label();
            lblCard3Value = new System.Windows.Forms.Label();
            lblCard3Title = new System.Windows.Forms.Label();
            pnlCard4 = new System.Windows.Forms.Panel();
            lblCard4Sub = new System.Windows.Forms.Label();
            lblCard4Value = new System.Windows.Forms.Label();
            lblCard4Title = new System.Windows.Forms.Label();
            pnlCard5 = new System.Windows.Forms.Panel();
            lblCard5Sub = new System.Windows.Forms.Label();
            lblCard5Value = new System.Windows.Forms.Label();
            lblCard5Title = new System.Windows.Forms.Label();
            tlpBody = new System.Windows.Forms.TableLayoutPanel();
            pnlLeftTools = new System.Windows.Forms.Panel();
            btnBaoTri = new System.Windows.Forms.Button();
            btnGiaHan = new System.Windows.Forms.Button();
            btnDoiPhong = new System.Windows.Forms.Button();
            btnQuanLyKhach = new System.Windows.Forms.Button();
            btnBaoDonXong = new System.Windows.Forms.Button();
            btnGoiDichVu = new System.Windows.Forms.Button();
            btnCheckIn = new System.Windows.Forms.Button();
            btnDatLichPhong = new System.Windows.Forms.Button();
            btnTimPhong = new System.Windows.Forms.Button();
            txtTimPhong = new System.Windows.Forms.TextBox();
            lblToolsTitle = new System.Windows.Forms.Label();
            tabMainView = new System.Windows.Forms.TabControl();
            tabMatrix = new System.Windows.Forms.TabPage();
            scMatrix = new System.Windows.Forms.SplitContainer();
            tlpRoomColumns = new System.Windows.Forms.TableLayoutPanel();
            flpDon = new System.Windows.Forms.FlowLayoutPanel();
            flpDoi = new System.Windows.Forms.FlowLayoutPanel();
            flpVIP = new System.Windows.Forms.FlowLayoutPanel();
            grpDatCoc = new System.Windows.Forms.GroupBox();
            dgvDatCoc = new System.Windows.Forms.DataGridView();
            tabLichTrinh = new System.Windows.Forms.TabPage();
            dgvLichTrinh = new System.Windows.Forms.DataGridView();
            cboBoLocLich = new System.Windows.Forms.ComboBox();
            lblLichTitle = new System.Windows.Forms.Label();
            tabThongKe = new System.Windows.Forms.TabPage();
            pnlChartContainer = new System.Windows.Forms.Panel();
            pnlRight = new System.Windows.Forms.Panel();
            flpYeuCauKhach = new System.Windows.Forms.FlowLayoutPanel();
            lblRightTitle2 = new System.Windows.Forms.Label();
            flpDonPhong = new System.Windows.Forms.FlowLayoutPanel();
            lblRightTitle1 = new System.Windows.Forms.Label();
            pnlHeader.SuspendLayout();
            flpHeaderRight.SuspendLayout();
            tlpCards.SuspendLayout();
            pnlCard1.SuspendLayout();
            pnlCard2.SuspendLayout();
            pnlCard3.SuspendLayout();
            pnlCard4.SuspendLayout();
            pnlCard5.SuspendLayout();
            tlpBody.SuspendLayout();
            pnlLeftTools.SuspendLayout();
            tabMainView.SuspendLayout();
            tabMatrix.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)scMatrix).BeginInit();
            scMatrix.Panel1.SuspendLayout();
            scMatrix.Panel2.SuspendLayout();
            scMatrix.SuspendLayout();
            tlpRoomColumns.SuspendLayout();
            grpDatCoc.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvDatCoc).BeginInit();
            tabLichTrinh.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvLichTrinh).BeginInit();
            tabThongKe.SuspendLayout();
            pnlRight.SuspendLayout();
            SuspendLayout();
            // 
            // pnlHeader
            // 
            pnlHeader.BackColor = System.Drawing.Color.White;
            pnlHeader.Controls.Add(lblHeaderTitle);
            pnlHeader.Controls.Add(flpHeaderRight);
            pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            pnlHeader.Location = new System.Drawing.Point(0, 0);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Size = new System.Drawing.Size(1300, 55);
            pnlHeader.TabIndex = 0;
            // 
            // lblHeaderTitle
            // 
            lblHeaderTitle.AutoSize = true;
            lblHeaderTitle.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            lblHeaderTitle.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            lblHeaderTitle.Location = new System.Drawing.Point(15, 14);
            lblHeaderTitle.Name = "lblHeaderTitle";
            lblHeaderTitle.Size = new System.Drawing.Size(434, 30);
            lblHeaderTitle.TabIndex = 0;
            lblHeaderTitle.Text = "HỆ THỐNG ĐIỀU HÀNH KHÁCH SẠN (50P)";
            // 
            // flpHeaderRight
            // 
            flpHeaderRight.AutoSize = true;
            flpHeaderRight.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            flpHeaderRight.Controls.Add(lblClock);
            flpHeaderRight.Controls.Add(btnRefresh);
            flpHeaderRight.Controls.Add(btnDangXuat);
            flpHeaderRight.Dock = System.Windows.Forms.DockStyle.Right;
            flpHeaderRight.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            flpHeaderRight.Location = new System.Drawing.Point(820, 0);
            flpHeaderRight.Name = "flpHeaderRight";
            flpHeaderRight.Padding = new System.Windows.Forms.Padding(0, 10, 15, 0);
            flpHeaderRight.Size = new System.Drawing.Size(480, 55);
            flpHeaderRight.TabIndex = 1;
            flpHeaderRight.WrapContents = false;
            // 
            // lblClock
            // 
            lblClock.AutoSize = true;
            lblClock.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            lblClock.ForeColor = System.Drawing.Color.FromArgb(71, 85, 105);
            lblClock.Margin = new System.Windows.Forms.Padding(0, 7, 20, 0);
            lblClock.Name = "lblClock";
            lblClock.Size = new System.Drawing.Size(160, 23);
            lblClock.TabIndex = 0;
            lblClock.Text = "17/09/2026 19:00:00";
            // 
            // btnRefresh
            // 
            btnRefresh.BackColor = System.Drawing.Color.FromArgb(241, 245, 249);
            btnRefresh.Cursor = System.Windows.Forms.Cursors.Hand;
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnRefresh.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            btnRefresh.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            btnRefresh.Margin = new System.Windows.Forms.Padding(0, 0, 10, 0);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new System.Drawing.Size(110, 35);
            btnRefresh.TabIndex = 1;
            btnRefresh.Text = "⟳ Làm mới";
            btnRefresh.UseVisualStyleBackColor = false;
            btnRefresh.Click += btnRefresh_Click;
            // 
            // btnDangXuat
            // 
            btnDangXuat.BackColor = System.Drawing.Color.FromArgb(254, 242, 242);
            btnDangXuat.Cursor = System.Windows.Forms.Cursors.Hand;
            btnDangXuat.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(252, 165, 165);
            btnDangXuat.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnDangXuat.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            btnDangXuat.ForeColor = System.Drawing.Color.FromArgb(220, 38, 38);
            btnDangXuat.Margin = new System.Windows.Forms.Padding(0);
            btnDangXuat.Name = "btnDangXuat";
            btnDangXuat.Size = new System.Drawing.Size(130, 35);
            btnDangXuat.TabIndex = 2;
            btnDangXuat.Text = "🚪 Đăng xuất";
            btnDangXuat.UseVisualStyleBackColor = false;
            btnDangXuat.Click += btnDangXuat_Click;
            // 
            // tlpCards
            // 
            tlpCards.ColumnCount = 5;
            tlpCards.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            tlpCards.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            tlpCards.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            tlpCards.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            tlpCards.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            tlpCards.Controls.Add(pnlCard1, 0, 0);
            tlpCards.Controls.Add(pnlCard2, 1, 0);
            tlpCards.Controls.Add(pnlCard3, 2, 0);
            tlpCards.Controls.Add(pnlCard4, 3, 0);
            tlpCards.Controls.Add(pnlCard5, 4, 0);
            tlpCards.Dock = System.Windows.Forms.DockStyle.Top;
            tlpCards.Location = new System.Drawing.Point(0, 55);
            tlpCards.Name = "tlpCards";
            tlpCards.Padding = new System.Windows.Forms.Padding(10, 5, 10, 5);
            tlpCards.RowCount = 1;
            tlpCards.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tlpCards.Size = new System.Drawing.Size(1300, 100);
            tlpCards.TabIndex = 1;
            // 
            // pnlCard1
            // 
            pnlCard1.BackColor = System.Drawing.Color.White;
            pnlCard1.Controls.Add(lblCard1Sub);
            pnlCard1.Controls.Add(lblCard1Value);
            pnlCard1.Controls.Add(lblCard1Title);
            pnlCard1.Cursor = System.Windows.Forms.Cursors.Hand;
            pnlCard1.Dock = System.Windows.Forms.DockStyle.Fill;
            pnlCard1.Location = new System.Drawing.Point(13, 8);
            pnlCard1.Name = "pnlCard1";
            pnlCard1.Size = new System.Drawing.Size(250, 84);
            pnlCard1.TabIndex = 0;
            pnlCard1.Click += pnlCard1_Click;
            // 
            // lblCard1Sub
            // 
            lblCard1Sub.AutoSize = true;
            lblCard1Sub.Font = new System.Drawing.Font("Segoe UI", 8F);
            lblCard1Sub.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            lblCard1Sub.Location = new System.Drawing.Point(10, 60);
            lblCard1Sub.Name = "lblCard1Sub";
            lblCard1Sub.Size = new System.Drawing.Size(127, 19);
            lblCard1Sub.TabIndex = 2;
            lblCard1Sub.Text = "Nhấn xem danh sách";
            lblCard1Sub.Click += pnlCard1_Click;
            // 
            // lblCard1Value
            // 
            lblCard1Value.AutoSize = true;
            lblCard1Value.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            lblCard1Value.ForeColor = System.Drawing.Color.FromArgb(239, 68, 68);
            lblCard1Value.Location = new System.Drawing.Point(8, 24);
            lblCard1Value.Name = "lblCard1Value";
            lblCard1Value.Size = new System.Drawing.Size(100, 37);
            lblCard1Value.TabIndex = 1;
            lblCard1Value.Text = "0 / 50";
            lblCard1Value.Click += pnlCard1_Click;
            // 
            // lblCard1Title
            // 
            lblCard1Title.AutoSize = true;
            lblCard1Title.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            lblCard1Title.ForeColor = System.Drawing.Color.FromArgb(71, 85, 105);
            lblCard1Title.Location = new System.Drawing.Point(10, 6);
            lblCard1Title.Name = "lblCard1Title";
            lblCard1Title.Size = new System.Drawing.Size(137, 20);
            lblCard1Title.TabIndex = 0;
            lblCard1Title.Text = "ĐANG SỬ DỤNG";
            lblCard1Title.Click += pnlCard1_Click;
            // 
            // pnlCard2
            // 
            pnlCard2.BackColor = System.Drawing.Color.White;
            pnlCard2.Controls.Add(lblCard2Sub);
            pnlCard2.Controls.Add(lblCard2Value);
            pnlCard2.Controls.Add(lblCard2Title);
            pnlCard2.Cursor = System.Windows.Forms.Cursors.Hand;
            pnlCard2.Dock = System.Windows.Forms.DockStyle.Fill;
            pnlCard2.Location = new System.Drawing.Point(269, 8);
            pnlCard2.Name = "pnlCard2";
            pnlCard2.Size = new System.Drawing.Size(250, 84);
            pnlCard2.TabIndex = 1;
            pnlCard2.Click += pnlCard2_Click;
            // 
            // lblCard2Sub
            // 
            lblCard2Sub.AutoSize = true;
            lblCard2Sub.Font = new System.Drawing.Font("Segoe UI", 8F);
            lblCard2Sub.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            lblCard2Sub.Location = new System.Drawing.Point(10, 60);
            lblCard2Sub.Name = "lblCard2Sub";
            lblCard2Sub.Size = new System.Drawing.Size(148, 19);
            lblCard2Sub.TabIndex = 2;
            lblCard2Sub.Text = "Sẵn sàng đón khách mới";
            lblCard2Sub.Click += pnlCard2_Click;
            // 
            // lblCard2Value
            // 
            lblCard2Value.AutoSize = true;
            lblCard2Value.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            lblCard2Value.ForeColor = System.Drawing.Color.FromArgb(34, 197, 94);
            lblCard2Value.Location = new System.Drawing.Point(8, 24);
            lblCard2Value.Name = "lblCard2Value";
            lblCard2Value.Size = new System.Drawing.Size(126, 37);
            lblCard2Value.TabIndex = 1;
            lblCard2Value.Text = "50 Phòng";
            lblCard2Value.Click += pnlCard2_Click;
            // 
            // lblCard2Title
            // 
            lblCard2Title.AutoSize = true;
            lblCard2Title.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            lblCard2Title.ForeColor = System.Drawing.Color.FromArgb(71, 85, 105);
            lblCard2Title.Location = new System.Drawing.Point(10, 6);
            lblCard2Title.Name = "lblCard2Title";
            lblCard2Title.Size = new System.Drawing.Size(147, 20);
            lblCard2Title.TabIndex = 0;
            lblCard2Title.Text = "PHÒNG SẴN SÀNG";
            lblCard2Title.Click += pnlCard2_Click;
            // 
            // pnlCard3
            // 
            pnlCard3.BackColor = System.Drawing.Color.White;
            pnlCard3.Controls.Add(lblCard3Sub);
            pnlCard3.Controls.Add(lblCard3Value);
            pnlCard3.Controls.Add(lblCard3Title);
            pnlCard3.Cursor = System.Windows.Forms.Cursors.Hand;
            pnlCard3.Dock = System.Windows.Forms.DockStyle.Fill;
            pnlCard3.Location = new System.Drawing.Point(525, 8);
            pnlCard3.Name = "pnlCard3";
            pnlCard3.Size = new System.Drawing.Size(250, 84);
            pnlCard3.TabIndex = 2;
            pnlCard3.Click += pnlCard3_Click;
            // 
            // lblCard3Sub
            // 
            lblCard3Sub.AutoSize = true;
            lblCard3Sub.Font = new System.Drawing.Font("Segoe UI", 8F);
            lblCard3Sub.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            lblCard3Sub.Location = new System.Drawing.Point(10, 60);
            lblCard3Sub.Name = "lblCard3Sub";
            lblCard3Sub.Size = new System.Drawing.Size(161, 19);
            lblCard3Sub.TabIndex = 2;
            lblCard3Sub.Text = "Đã cọc: 0 | Chưa cọc: 0";
            lblCard3Sub.Click += pnlCard3_Click;
            // 
            // lblCard3Value
            // 
            lblCard3Value.AutoSize = true;
            lblCard3Value.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            lblCard3Value.ForeColor = System.Drawing.Color.FromArgb(59, 130, 246);
            lblCard3Value.Location = new System.Drawing.Point(8, 24);
            lblCard3Value.Name = "lblCard3Value";
            lblCard3Value.Size = new System.Drawing.Size(100, 37);
            lblCard3Value.TabIndex = 1;
            lblCard3Value.Text = "0 Lượt";
            lblCard3Value.Click += pnlCard3_Click;
            // 
            // lblCard3Title
            // 
            lblCard3Title.AutoSize = true;
            lblCard3Title.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            lblCard3Title.ForeColor = System.Drawing.Color.FromArgb(71, 85, 105);
            lblCard3Title.Location = new System.Drawing.Point(10, 6);
            lblCard3Title.Name = "lblCard3Title";
            lblCard3Title.Size = new System.Drawing.Size(176, 20);
            lblCard3Title.TabIndex = 0;
            lblCard3Title.Text = "ĐẶT TRƯỚC (GIỮ CHỖ)";
            lblCard3Title.Click += pnlCard3_Click;
            // 
            // pnlCard4
            // 
            pnlCard4.BackColor = System.Drawing.Color.White;
            pnlCard4.Controls.Add(lblCard4Sub);
            pnlCard4.Controls.Add(lblCard4Value);
            pnlCard4.Controls.Add(lblCard4Title);
            pnlCard4.Cursor = System.Windows.Forms.Cursors.Hand;
            pnlCard4.Dock = System.Windows.Forms.DockStyle.Fill;
            pnlCard4.Location = new System.Drawing.Point(781, 8);
            pnlCard4.Name = "pnlCard4";
            pnlCard4.Size = new System.Drawing.Size(250, 84);
            pnlCard4.TabIndex = 3;
            pnlCard4.Click += pnlCard4_Click;
            // 
            // lblCard4Sub
            // 
            lblCard4Sub.AutoSize = true;
            lblCard4Sub.Font = new System.Drawing.Font("Segoe UI", 8F);
            lblCard4Sub.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            lblCard4Sub.Location = new System.Drawing.Point(10, 60);
            lblCard4Sub.Name = "lblCard4Sub";
            lblCard4Sub.Size = new System.Drawing.Size(149, 19);
            lblCard4Sub.TabIndex = 2;
            lblCard4Sub.Text = "Xuất hóa đơn & QR Pay";
            lblCard4Sub.Click += pnlCard4_Click;
            // 
            // lblCard4Value
            // 
            lblCard4Value.AutoSize = true;
            lblCard4Value.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            lblCard4Value.ForeColor = System.Drawing.Color.FromArgb(245, 158, 11);
            lblCard4Value.Location = new System.Drawing.Point(8, 24);
            lblCard4Value.Name = "lblCard4Value";
            lblCard4Value.Size = new System.Drawing.Size(100, 37);
            lblCard4Value.TabIndex = 1;
            lblCard4Value.Text = "0 Lượt";
            lblCard4Value.Click += pnlCard4_Click;
            // 
            // lblCard4Title
            // 
            lblCard4Title.AutoSize = true;
            lblCard4Title.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            lblCard4Title.ForeColor = System.Drawing.Color.FromArgb(71, 85, 105);
            lblCard4Title.Location = new System.Drawing.Point(10, 6);
            lblCard4Title.Name = "lblCard4Title";
            lblCard4Title.Size = new System.Drawing.Size(163, 20);
            lblCard4Title.TabIndex = 0;
            lblCard4Title.Text = "CHECK-OUT HÔM NAY";
            lblCard4Title.Click += pnlCard4_Click;
            // 
            // pnlCard5
            // 
            pnlCard5.BackColor = System.Drawing.Color.White;
            pnlCard5.Controls.Add(lblCard5Sub);
            pnlCard5.Controls.Add(lblCard5Value);
            pnlCard5.Controls.Add(lblCard5Title);
            pnlCard5.Dock = System.Windows.Forms.DockStyle.Fill;
            pnlCard5.Location = new System.Drawing.Point(1037, 8);
            pnlCard5.Name = "pnlCard5";
            pnlCard5.Size = new System.Drawing.Size(250, 84);
            pnlCard5.TabIndex = 4;
            // 
            // lblCard5Sub
            // 
            lblCard5Sub.AutoSize = true;
            lblCard5Sub.Font = new System.Drawing.Font("Segoe UI", 8F);
            lblCard5Sub.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            lblCard5Sub.Location = new System.Drawing.Point(10, 60);
            lblCard5Sub.Name = "lblCard5Sub";
            lblCard5Sub.Size = new System.Drawing.Size(134, 19);
            lblCard5Sub.TabIndex = 2;
            lblCard5Sub.Text = "Đã thanh toán thực";
            // 
            // lblCard5Value
            // 
            lblCard5Value.AutoSize = true;
            lblCard5Value.Font = new System.Drawing.Font("Segoe UI", 15F, System.Drawing.FontStyle.Bold);
            lblCard5Value.ForeColor = System.Drawing.Color.FromArgb(16, 185, 129);
            lblCard5Value.Location = new System.Drawing.Point(8, 24);
            lblCard5Value.Name = "lblCard5Value";
            lblCard5Value.Size = new System.Drawing.Size(64, 35);
            lblCard5Value.TabIndex = 1;
            lblCard5Value.Text = "0 đ";
            // 
            // lblCard5Title
            // 
            lblCard5Title.AutoSize = true;
            lblCard5Title.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            lblCard5Title.ForeColor = System.Drawing.Color.FromArgb(71, 85, 105);
            lblCard5Title.Location = new System.Drawing.Point(10, 6);
            lblCard5Title.Name = "lblCard5Title";
            lblCard5Title.Size = new System.Drawing.Size(169, 20);
            lblCard5Title.TabIndex = 0;
            lblCard5Title.Text = "DOANH THU TẠM TÍNH";
            // 
            // tlpBody
            // 
            tlpBody.ColumnCount = 3;
            tlpBody.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 220F));
            tlpBody.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tlpBody.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 250F));
            tlpBody.Controls.Add(pnlLeftTools, 0, 0);
            tlpBody.Controls.Add(tabMainView, 1, 0);
            tlpBody.Controls.Add(pnlRight, 2, 0);
            tlpBody.Dock = System.Windows.Forms.DockStyle.Fill;
            tlpBody.Location = new System.Drawing.Point(0, 155);
            tlpBody.Name = "tlpBody";
            tlpBody.Padding = new System.Windows.Forms.Padding(10, 5, 10, 10);
            tlpBody.RowCount = 1;
            tlpBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tlpBody.Size = new System.Drawing.Size(1300, 565);
            tlpBody.TabIndex = 2;
            // 
            // pnlLeftTools
            // 
            pnlLeftTools.AutoScroll = true;
            pnlLeftTools.BackColor = System.Drawing.Color.White;
            pnlLeftTools.Controls.Add(btnBaoTri);
            pnlLeftTools.Controls.Add(btnGiaHan);
            pnlLeftTools.Controls.Add(btnDoiPhong);
            pnlLeftTools.Controls.Add(btnQuanLyKhach);
            pnlLeftTools.Controls.Add(btnBaoDonXong);
            pnlLeftTools.Controls.Add(btnGoiDichVu);
            pnlLeftTools.Controls.Add(btnCheckIn);
            pnlLeftTools.Controls.Add(btnDatLichPhong);
            pnlLeftTools.Controls.Add(btnTimPhong);
            pnlLeftTools.Controls.Add(txtTimPhong);
            pnlLeftTools.Controls.Add(lblToolsTitle);
            pnlLeftTools.Dock = System.Windows.Forms.DockStyle.Fill;
            pnlLeftTools.Location = new System.Drawing.Point(13, 8);
            pnlLeftTools.Name = "pnlLeftTools";
            pnlLeftTools.Padding = new System.Windows.Forms.Padding(10);
            pnlLeftTools.Size = new System.Drawing.Size(214, 544);
            pnlLeftTools.TabIndex = 0;
            // 
            // btnBaoTri
            // 
            btnBaoTri.BackColor = System.Drawing.Color.FromArgb(241, 245, 249);
            btnBaoTri.Cursor = System.Windows.Forms.Cursors.Hand;
            btnBaoTri.FlatAppearance.BorderSize = 0;
            btnBaoTri.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnBaoTri.Font = new System.Drawing.Font("Segoe UI Semibold", 8.5F);
            btnBaoTri.ForeColor = System.Drawing.Color.FromArgb(71, 85, 105);
            btnBaoTri.Location = new System.Drawing.Point(10, 465);
            btnBaoTri.Name = "btnBaoTri";
            btnBaoTri.Size = new System.Drawing.Size(190, 36);
            btnBaoTri.TabIndex = 10;
            btnBaoTri.Text = "🛠 Bật / Tắt Bảo Trì";
            btnBaoTri.UseVisualStyleBackColor = false;
            btnBaoTri.Click += btnBaoTri_Click;
            // 
            // btnGiaHan
            // 
            btnGiaHan.BackColor = System.Drawing.Color.FromArgb(241, 245, 249);
            btnGiaHan.Cursor = System.Windows.Forms.Cursors.Hand;
            btnGiaHan.FlatAppearance.BorderSize = 0;
            btnGiaHan.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnGiaHan.Font = new System.Drawing.Font("Segoe UI Semibold", 8.5F);
            btnGiaHan.ForeColor = System.Drawing.Color.FromArgb(71, 85, 105);
            btnGiaHan.Location = new System.Drawing.Point(10, 420);
            btnGiaHan.Name = "btnGiaHan";
            btnGiaHan.Size = new System.Drawing.Size(190, 36);
            btnGiaHan.TabIndex = 9;
            btnGiaHan.Text = "⏳ Gia Hạn Trả Phòng";
            btnGiaHan.UseVisualStyleBackColor = false;
            btnGiaHan.Click += btnGiaHan_Click;
            // 
            // btnDoiPhong
            // 
            btnDoiPhong.BackColor = System.Drawing.Color.FromArgb(241, 245, 249);
            btnDoiPhong.Cursor = System.Windows.Forms.Cursors.Hand;
            btnDoiPhong.FlatAppearance.BorderSize = 0;
            btnDoiPhong.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnDoiPhong.Font = new System.Drawing.Font("Segoe UI Semibold", 8.5F);
            btnDoiPhong.ForeColor = System.Drawing.Color.FromArgb(71, 85, 105);
            btnDoiPhong.Location = new System.Drawing.Point(10, 375);
            btnDoiPhong.Name = "btnDoiPhong";
            btnDoiPhong.Size = new System.Drawing.Size(190, 36);
            btnDoiPhong.TabIndex = 8;
            btnDoiPhong.Text = "🔄 Chuyển Đổi Phòng";
            btnDoiPhong.UseVisualStyleBackColor = false;
            btnDoiPhong.Click += btnDoiPhong_Click;
            // 
            // btnQuanLyKhach
            // 
            btnQuanLyKhach.BackColor = System.Drawing.Color.FromArgb(241, 245, 249);
            btnQuanLyKhach.Cursor = System.Windows.Forms.Cursors.Hand;
            btnQuanLyKhach.FlatAppearance.BorderSize = 0;
            btnQuanLyKhach.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnQuanLyKhach.Font = new System.Drawing.Font("Segoe UI Semibold", 8.5F);
            btnQuanLyKhach.ForeColor = System.Drawing.Color.FromArgb(71, 85, 105);
            btnQuanLyKhach.Location = new System.Drawing.Point(10, 330);
            btnQuanLyKhach.Name = "btnQuanLyKhach";
            btnQuanLyKhach.Size = new System.Drawing.Size(190, 36);
            btnQuanLyKhach.TabIndex = 7;
            btnQuanLyKhach.Text = "👥 Hồ Sơ Khách Hàng";
            btnQuanLyKhach.UseVisualStyleBackColor = false;
            btnQuanLyKhach.Click += btnQuanLyKhach_Click;
            // 
            // btnBaoDonXong
            // 
            btnBaoDonXong.BackColor = System.Drawing.Color.FromArgb(241, 245, 249);
            btnBaoDonXong.Cursor = System.Windows.Forms.Cursors.Hand;
            btnBaoDonXong.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(203, 213, 225);
            btnBaoDonXong.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnBaoDonXong.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            btnBaoDonXong.ForeColor = System.Drawing.Color.FromArgb(51, 65, 85);
            btnBaoDonXong.Location = new System.Drawing.Point(10, 275);
            btnBaoDonXong.Name = "btnBaoDonXong";
            btnBaoDonXong.Size = new System.Drawing.Size(190, 42);
            btnBaoDonXong.TabIndex = 6;
            btnBaoDonXong.Text = "✓ Đã Dọn Xong Tất Cả";
            btnBaoDonXong.UseVisualStyleBackColor = false;
            btnBaoDonXong.Click += btnBaoDonXong_Click;
            // 
            // btnGoiDichVu
            // 
            btnGoiDichVu.BackColor = System.Drawing.Color.FromArgb(16, 185, 129);
            btnGoiDichVu.Cursor = System.Windows.Forms.Cursors.Hand;
            btnGoiDichVu.FlatAppearance.BorderSize = 0;
            btnGoiDichVu.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnGoiDichVu.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            btnGoiDichVu.ForeColor = System.Drawing.Color.White;
            btnGoiDichVu.Location = new System.Drawing.Point(10, 220);
            btnGoiDichVu.Name = "btnGoiDichVu";
            btnGoiDichVu.Size = new System.Drawing.Size(190, 42);
            btnGoiDichVu.TabIndex = 5;
            btnGoiDichVu.Text = "+ Thêm Dịch Vụ / Bar";
            btnGoiDichVu.UseVisualStyleBackColor = false;
            btnGoiDichVu.Click += btnGoiDichVu_Click;
            // 
            // btnCheckIn
            // 
            btnCheckIn.BackColor = System.Drawing.Color.FromArgb(37, 99, 235);
            btnCheckIn.Cursor = System.Windows.Forms.Cursors.Hand;
            btnCheckIn.FlatAppearance.BorderSize = 0;
            btnCheckIn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnCheckIn.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            btnCheckIn.ForeColor = System.Drawing.Color.White;
            btnCheckIn.Location = new System.Drawing.Point(10, 165);
            btnCheckIn.Name = "btnCheckIn";
            btnCheckIn.Size = new System.Drawing.Size(190, 42);
            btnCheckIn.TabIndex = 4;
            btnCheckIn.Text = "+ Nhận Phòng Ngay";
            btnCheckIn.UseVisualStyleBackColor = false;
            btnCheckIn.Click += btnThemPhong_Click;
            // 
            // btnDatLichPhong
            // 
            btnDatLichPhong.BackColor = System.Drawing.Color.FromArgb(79, 70, 229);
            btnDatLichPhong.Cursor = System.Windows.Forms.Cursors.Hand;
            btnDatLichPhong.FlatAppearance.BorderSize = 0;
            btnDatLichPhong.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnDatLichPhong.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            btnDatLichPhong.ForeColor = System.Drawing.Color.White;
            btnDatLichPhong.Location = new System.Drawing.Point(10, 110);
            btnDatLichPhong.Name = "btnDatLichPhong";
            btnDatLichPhong.Size = new System.Drawing.Size(190, 42);
            btnDatLichPhong.TabIndex = 3;
            btnDatLichPhong.Text = "📅 Đặt Trước (Có cọc)";
            btnDatLichPhong.UseVisualStyleBackColor = false;
            btnDatLichPhong.Click += pnlCard3_Click;
            // 
            // btnTimPhong
            // 
            btnTimPhong.BackColor = System.Drawing.Color.FromArgb(226, 232, 240);
            btnTimPhong.Cursor = System.Windows.Forms.Cursors.Hand;
            btnTimPhong.FlatAppearance.BorderSize = 0;
            btnTimPhong.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnTimPhong.Font = new System.Drawing.Font("Segoe UI Semibold", 8.5F, System.Drawing.FontStyle.Bold);
            btnTimPhong.Location = new System.Drawing.Point(145, 50);
            btnTimPhong.Name = "btnTimPhong";
            btnTimPhong.Size = new System.Drawing.Size(55, 30);
            btnTimPhong.TabIndex = 2;
            btnTimPhong.Text = "Tìm";
            btnTimPhong.UseVisualStyleBackColor = false;
            btnTimPhong.Click += btnTimPhong_Click;
            // 
            // txtTimPhong
            // 
            txtTimPhong.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            txtTimPhong.Location = new System.Drawing.Point(10, 51);
            txtTimPhong.Name = "txtTimPhong";
            txtTimPhong.PlaceholderText = "Số phòng...";
            txtTimPhong.Size = new System.Drawing.Size(130, 29);
            txtTimPhong.TabIndex = 1;
            // 
            // lblToolsTitle
            // 
            lblToolsTitle.AutoSize = true;
            lblToolsTitle.Font = new System.Drawing.Font("Segoe UI", 10.5F, System.Drawing.FontStyle.Bold);
            lblToolsTitle.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            lblToolsTitle.Location = new System.Drawing.Point(10, 15);
            lblToolsTitle.Name = "lblToolsTitle";
            lblToolsTitle.Size = new System.Drawing.Size(147, 25);
            lblToolsTitle.TabIndex = 0;
            lblToolsTitle.Text = "TÁC VỤ LỄ TÂN";
            // 
            // tabMainView
            // 
            tabMainView.Controls.Add(tabMatrix);
            tabMainView.Controls.Add(tabLichTrinh);
            tabMainView.Controls.Add(tabThongKe);
            tabMainView.Dock = System.Windows.Forms.DockStyle.Fill;
            tabMainView.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            tabMainView.ItemSize = new System.Drawing.Size(220, 36);
            tabMainView.Location = new System.Drawing.Point(233, 8);
            tabMainView.Name = "tabMainView";
            tabMainView.Padding = new System.Drawing.Point(16, 4);
            tabMainView.SelectedIndex = 0;
            tabMainView.Size = new System.Drawing.Size(804, 544);
            tabMainView.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            tabMainView.TabIndex = 1;
            // 
            // tabMatrix
            // 
            tabMatrix.BackColor = System.Drawing.Color.FromArgb(248, 250, 252);
            tabMatrix.Controls.Add(scMatrix);
            tabMatrix.Location = new System.Drawing.Point(4, 40);
            tabMatrix.Name = "tabMatrix";
            tabMatrix.Padding = new System.Windows.Forms.Padding(6);
            tabMatrix.Size = new System.Drawing.Size(796, 500);
            tabMatrix.TabIndex = 0;
            tabMatrix.Text = "SƠ ĐỒ 50 PHÒNG (3 CỘT)";
            // 
            // scMatrix
            // 
            scMatrix.Dock = System.Windows.Forms.DockStyle.Fill;
            scMatrix.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            scMatrix.Location = new System.Drawing.Point(6, 6);
            scMatrix.Name = "scMatrix";
            scMatrix.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // scMatrix.Panel1
            // 
            scMatrix.Panel1.Controls.Add(tlpRoomColumns);
            // 
            // scMatrix.Panel2
            // 
            scMatrix.Panel2.Controls.Add(grpDatCoc);
            scMatrix.Size = new System.Drawing.Size(784, 488);
            scMatrix.SplitterDistance = 250;
            scMatrix.TabIndex = 0;
            // 
            // tlpRoomColumns
            // 
            tlpRoomColumns.ColumnCount = 3;
            tlpRoomColumns.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.333F));
            tlpRoomColumns.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.333F));
            tlpRoomColumns.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.334F));
            tlpRoomColumns.Controls.Add(flpDon, 0, 0);
            tlpRoomColumns.Controls.Add(flpDoi, 1, 0);
            tlpRoomColumns.Controls.Add(flpVIP, 2, 0);
            tlpRoomColumns.Dock = System.Windows.Forms.DockStyle.Fill;
            tlpRoomColumns.Location = new System.Drawing.Point(0, 0);
            tlpRoomColumns.Name = "tlpRoomColumns";
            tlpRoomColumns.RowCount = 1;
            tlpRoomColumns.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tlpRoomColumns.Size = new System.Drawing.Size(784, 250);
            tlpRoomColumns.TabIndex = 0;
            // 
            // flpDon
            // 
            flpDon.AutoScroll = true;
            flpDon.Dock = System.Windows.Forms.DockStyle.Fill;
            flpDon.Location = new System.Drawing.Point(3, 23);
            flpDon.Name = "flpDon";
            flpDon.Padding = new System.Windows.Forms.Padding(4);
            flpDon.Size = new System.Drawing.Size(285, 218);
            flpDon.TabIndex = 0;
            // 
            // flpDoi
            // 
            flpDoi.AutoScroll = true;
            flpDoi.Dock = System.Windows.Forms.DockStyle.Fill;
            flpDoi.Location = new System.Drawing.Point(3, 23);
            flpDoi.Name = "flpDoi";
            flpDoi.Padding = new System.Windows.Forms.Padding(4);
            flpDoi.Size = new System.Drawing.Size(285, 218);
            flpDoi.TabIndex = 0;
            // 
            // flpVIP
            // 
            flpVIP.AutoScroll = true;
            flpVIP.Dock = System.Windows.Forms.DockStyle.Fill;
            flpVIP.Location = new System.Drawing.Point(3, 23);
            flpVIP.Name = "flpVIP";
            flpVIP.Padding = new System.Windows.Forms.Padding(4);
            flpVIP.Size = new System.Drawing.Size(178, 218);
            flpVIP.TabIndex = 0;
            // 
            // grpDatCoc
            // 
            grpDatCoc.BackColor = System.Drawing.Color.White;
            grpDatCoc.Controls.Add(dgvDatCoc);
            grpDatCoc.Dock = System.Windows.Forms.DockStyle.Fill;
            grpDatCoc.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            grpDatCoc.ForeColor = System.Drawing.Color.FromArgb(30, 41, 59);
            grpDatCoc.Location = new System.Drawing.Point(0, 0);
            grpDatCoc.Name = "grpDatCoc";
            grpDatCoc.Padding = new System.Windows.Forms.Padding(8);
            grpDatCoc.Size = new System.Drawing.Size(784, 234);
            grpDatCoc.TabIndex = 0;
            grpDatCoc.TabStop = false;
            grpDatCoc.Text = "SỔ THEO DÕI ĐẶT CỌC & GIỮ CHỖ CHỜ KHÁCH ĐẾN";
            // 
            // dgvDatCoc
            // 
            dgvDatCoc.AllowUserToAddRows = false;
            dgvDatCoc.AllowUserToDeleteRows = false;
            dgvDatCoc.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            dgvDatCoc.BackgroundColor = System.Drawing.Color.White;
            dgvDatCoc.BorderStyle = System.Windows.Forms.BorderStyle.None;
            dgvDatCoc.ColumnHeadersHeight = 32;
            dgvDatCoc.Dock = System.Windows.Forms.DockStyle.Fill;
            dgvDatCoc.Font = new System.Drawing.Font("Segoe UI", 9F);
            dgvDatCoc.Location = new System.Drawing.Point(8, 30);
            dgvDatCoc.Name = "dgvDatCoc";
            dgvDatCoc.ReadOnly = true;
            dgvDatCoc.RowHeadersVisible = false;
            dgvDatCoc.RowTemplate.Height = 34;
            dgvDatCoc.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            dgvDatCoc.Size = new System.Drawing.Size(768, 196);
            dgvDatCoc.TabIndex = 0;
            dgvDatCoc.CellContentClick += dgvDatCoc_CellContentClick;
            // 
            // tabLichTrinh
            // 
            tabLichTrinh.BackColor = System.Drawing.Color.White;
            tabLichTrinh.Controls.Add(dgvLichTrinh);
            tabLichTrinh.Controls.Add(cboBoLocLich);
            tabLichTrinh.Controls.Add(lblLichTitle);
            tabLichTrinh.Location = new System.Drawing.Point(4, 40);
            tabLichTrinh.Name = "tabLichTrinh";
            tabLichTrinh.Padding = new System.Windows.Forms.Padding(10);
            tabLichTrinh.Size = new System.Drawing.Size(796, 500);
            tabLichTrinh.TabIndex = 1;
            tabLichTrinh.Text = "ĐIỀU PHỐI ĐẾN / ĐI";
            // 
            // dgvLichTrinh
            // 
            dgvLichTrinh.AllowUserToAddRows = false;
            dgvLichTrinh.AllowUserToDeleteRows = false;
            dgvLichTrinh.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            dgvLichTrinh.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            dgvLichTrinh.BackgroundColor = System.Drawing.Color.White;
            dgvLichTrinh.BorderStyle = System.Windows.Forms.BorderStyle.None;
            dgvLichTrinh.ColumnHeadersHeight = 35;
            dgvLichTrinh.Location = new System.Drawing.Point(10, 55);
            dgvLichTrinh.Name = "dgvLichTrinh";
            dgvLichTrinh.ReadOnly = true;
            dgvLichTrinh.RowHeadersVisible = false;
            dgvLichTrinh.RowTemplate.Height = 35;
            dgvLichTrinh.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            dgvLichTrinh.Size = new System.Drawing.Size(776, 435);
            dgvLichTrinh.TabIndex = 2;
            dgvLichTrinh.CellContentClick += dgvLichTrinh_CellContentClick;
            // 
            // cboBoLocLich
            // 
            cboBoLocLich.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            cboBoLocLich.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cboBoLocLich.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            cboBoLocLich.FormattingEnabled = true;
            cboBoLocLich.Items.AddRange(new object[] { "Tất cả nghiệp vụ", "Chờ Check-in", "Chờ Check-out" });
            cboBoLocLich.Location = new System.Drawing.Point(586, 12);
            cboBoLocLich.Name = "cboBoLocLich";
            cboBoLocLich.Size = new System.Drawing.Size(200, 29);
            cboBoLocLich.TabIndex = 1;
            cboBoLocLich.SelectedIndexChanged += cboBoLocLich_SelectedIndexChanged;
            // 
            // lblLichTitle
            // 
            lblLichTitle.AutoSize = true;
            lblLichTitle.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            lblLichTitle.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            lblLichTitle.Location = new System.Drawing.Point(10, 15);
            lblLichTitle.Name = "lblLichTitle";
            lblLichTitle.Size = new System.Drawing.Size(359, 25);
            lblLichTitle.TabIndex = 0;
            lblLichTitle.Text = "DANH SÁCH KHÁCH ĐẾN VÀ ĐI TRONG NGÀY";
            // 
            // tabThongKe
            // 
            tabThongKe.BackColor = System.Drawing.Color.White;
            tabThongKe.Controls.Add(pnlChartContainer);
            tabThongKe.Location = new System.Drawing.Point(4, 40);
            tabThongKe.Name = "tabThongKe";
            tabThongKe.Padding = new System.Windows.Forms.Padding(15);
            tabThongKe.Size = new System.Drawing.Size(796, 500);
            tabThongKe.TabIndex = 2;
            tabThongKe.Text = "BIỂU ĐỒ DOANH THU & HẠNG MỤC";
            // 
            // pnlChartContainer
            // 
            pnlChartContainer.AutoScroll = true;
            pnlChartContainer.BackColor = System.Drawing.Color.White;
            pnlChartContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            pnlChartContainer.Location = new System.Drawing.Point(15, 15);
            pnlChartContainer.Name = "pnlChartContainer";
            pnlChartContainer.Size = new System.Drawing.Size(766, 470);
            pnlChartContainer.TabIndex = 0;
            // 
            // pnlRight
            // 
            pnlRight.BackColor = System.Drawing.Color.White;
            pnlRight.Controls.Add(flpYeuCauKhach);
            pnlRight.Controls.Add(lblRightTitle2);
            pnlRight.Controls.Add(flpDonPhong);
            pnlRight.Controls.Add(lblRightTitle1);
            pnlRight.Dock = System.Windows.Forms.DockStyle.Fill;
            pnlRight.Location = new System.Drawing.Point(1043, 8);
            pnlRight.Name = "pnlRight";
            pnlRight.Padding = new System.Windows.Forms.Padding(10);
            pnlRight.Size = new System.Drawing.Size(244, 544);
            pnlRight.TabIndex = 2;
            // 
            // flpYeuCauKhach
            // 
            flpYeuCauKhach.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            flpYeuCauKhach.AutoScroll = true;
            flpYeuCauKhach.BackColor = System.Drawing.Color.FromArgb(248, 250, 252);
            flpYeuCauKhach.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            flpYeuCauKhach.Location = new System.Drawing.Point(10, 260);
            flpYeuCauKhach.Name = "flpYeuCauKhach";
            flpYeuCauKhach.Padding = new System.Windows.Forms.Padding(4);
            flpYeuCauKhach.Size = new System.Drawing.Size(224, 260);
            flpYeuCauKhach.TabIndex = 3;
            // 
            // lblRightTitle2
            // 
            lblRightTitle2.AutoSize = true;
            lblRightTitle2.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            lblRightTitle2.ForeColor = System.Drawing.Color.FromArgb(30, 41, 59);
            lblRightTitle2.Location = new System.Drawing.Point(10, 230);
            lblRightTitle2.Name = "lblRightTitle2";
            lblRightTitle2.Size = new System.Drawing.Size(187, 23);
            lblRightTitle2.TabIndex = 2;
            lblRightTitle2.Text = "Nhật ký / Yêu cầu khẩn";
            // 
            // flpDonPhong
            // 
            flpDonPhong.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            flpDonPhong.AutoScroll = true;
            flpDonPhong.BackColor = System.Drawing.Color.FromArgb(248, 250, 252);
            flpDonPhong.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            flpDonPhong.Location = new System.Drawing.Point(10, 40);
            flpDonPhong.Name = "flpDonPhong";
            flpDonPhong.Padding = new System.Windows.Forms.Padding(4);
            flpDonPhong.Size = new System.Drawing.Size(224, 180);
            flpDonPhong.TabIndex = 1;
            // 
            // lblRightTitle1
            // 
            lblRightTitle1.AutoSize = true;
            lblRightTitle1.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            lblRightTitle1.ForeColor = System.Drawing.Color.FromArgb(234, 88, 12);
            lblRightTitle1.Location = new System.Drawing.Point(10, 10);
            lblRightTitle1.Name = "lblRightTitle1";
            lblRightTitle1.Size = new System.Drawing.Size(161, 23);
            lblRightTitle1.TabIndex = 0;
            lblRightTitle1.Text = "Phòng đang dọn (0)";
            // 
            // ucDashboard
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.Color.FromArgb(241, 245, 249);
            Controls.Add(tlpBody);
            Controls.Add(tlpCards);
            Controls.Add(pnlHeader);
            Name = "ucDashboard";
            Size = new System.Drawing.Size(1300, 720);
            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            flpHeaderRight.ResumeLayout(false);
            flpHeaderRight.PerformLayout();
            tlpCards.ResumeLayout(false);
            pnlCard1.ResumeLayout(false);
            pnlCard1.PerformLayout();
            pnlCard2.ResumeLayout(false);
            pnlCard2.PerformLayout();
            pnlCard3.ResumeLayout(false);
            pnlCard3.PerformLayout();
            pnlCard4.ResumeLayout(false);
            pnlCard4.PerformLayout();
            pnlCard5.ResumeLayout(false);
            pnlCard5.PerformLayout();
            tlpBody.ResumeLayout(false);
            pnlLeftTools.ResumeLayout(false);
            pnlLeftTools.PerformLayout();
            tabMainView.ResumeLayout(false);
            tabMatrix.ResumeLayout(false);
            scMatrix.Panel1.ResumeLayout(false);
            scMatrix.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)scMatrix).EndInit();
            scMatrix.ResumeLayout(false);
            tlpRoomColumns.ResumeLayout(false);
            grpDatCoc.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvDatCoc).EndInit();
            tabLichTrinh.ResumeLayout(false);
            tabLichTrinh.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvLichTrinh).EndInit();
            tabThongKe.ResumeLayout(false);
            pnlRight.ResumeLayout(false);
            pnlRight.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Label lblHeaderTitle;
        private System.Windows.Forms.FlowLayoutPanel flpHeaderRight;
        private System.Windows.Forms.Label lblClock;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button btnDangXuat;
        private System.Windows.Forms.TableLayoutPanel tlpCards;
        private System.Windows.Forms.Panel pnlCard1;
        private System.Windows.Forms.Label lblCard1Title;
        private System.Windows.Forms.Label lblCard1Value;
        private System.Windows.Forms.Label lblCard1Sub;
        private System.Windows.Forms.Panel pnlCard2;
        private System.Windows.Forms.Label lblCard2Title;
        private System.Windows.Forms.Label lblCard2Value;
        private System.Windows.Forms.Label lblCard2Sub;
        private System.Windows.Forms.Panel pnlCard3;
        private System.Windows.Forms.Label lblCard3Title;
        private System.Windows.Forms.Label lblCard3Value;
        private System.Windows.Forms.Label lblCard3Sub;
        private System.Windows.Forms.Panel pnlCard4;
        private System.Windows.Forms.Label lblCard4Title;
        private System.Windows.Forms.Label lblCard4Value;
        private System.Windows.Forms.Label lblCard4Sub;
        private System.Windows.Forms.Panel pnlCard5;
        private System.Windows.Forms.Label lblCard5Title;
        private System.Windows.Forms.Label lblCard5Value;
        private System.Windows.Forms.Label lblCard5Sub;
        private System.Windows.Forms.TableLayoutPanel tlpBody;
        private System.Windows.Forms.Panel pnlLeftTools;
        private System.Windows.Forms.Label lblToolsTitle;
        private System.Windows.Forms.TextBox txtTimPhong;
        private System.Windows.Forms.Button btnTimPhong;
        private System.Windows.Forms.Button btnDatLichPhong;
        private System.Windows.Forms.Button btnCheckIn;
        private System.Windows.Forms.Button btnGoiDichVu;
        private System.Windows.Forms.Button btnBaoDonXong;
        private System.Windows.Forms.Button btnQuanLyKhach;
        private System.Windows.Forms.Button btnDoiPhong;
        private System.Windows.Forms.Button btnGiaHan;
        private System.Windows.Forms.Button btnBaoTri;
        private System.Windows.Forms.TabControl tabMainView;
        private System.Windows.Forms.TabPage tabMatrix;
        private System.Windows.Forms.SplitContainer scMatrix;
        private System.Windows.Forms.TableLayoutPanel tlpRoomColumns;
        private System.Windows.Forms.FlowLayoutPanel flpDon;
        private System.Windows.Forms.FlowLayoutPanel flpDoi;
        private System.Windows.Forms.FlowLayoutPanel flpVIP;
        private System.Windows.Forms.GroupBox grpDatCoc;
        private System.Windows.Forms.DataGridView dgvDatCoc;
        private System.Windows.Forms.TabPage tabLichTrinh;
        private System.Windows.Forms.Label lblLichTitle;
        private System.Windows.Forms.ComboBox cboBoLocLich;
        private System.Windows.Forms.DataGridView dgvLichTrinh;
        private System.Windows.Forms.TabPage tabThongKe;
        private System.Windows.Forms.Panel pnlChartContainer;
        private System.Windows.Forms.Panel pnlRight;
        private System.Windows.Forms.Label lblRightTitle1;
        private System.Windows.Forms.FlowLayoutPanel flpDonPhong;
        private System.Windows.Forms.Label lblRightTitle2;
        private System.Windows.Forms.FlowLayoutPanel flpYeuCauKhach;
    }
}
