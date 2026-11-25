using System.Data;
using QLKhachSan.BLL;
using QLKhachSan.DAL;
using QLKhachSan.DTO;

namespace QLKhachSan.GUI;

public sealed class FormCaTruc : Form
{
    private readonly HotelService service;
    private readonly UserSession user;

    // KPI Cards Controls
    private readonly Label lblTotalRooms = new();
    private readonly Label lblVacantRooms = new();
    private readonly Label lblOccupiedRooms = new();
    private readonly Label lblReservedRooms = new();
    private readonly Label lblCleaningRooms = new();

    // Tab 1: Room Status
    private readonly DataGridView gridRooms = Ui.Grid();
    private readonly TextBox txtSearchRoom = new();
    private string roomFilter = "All";
    private List<RoomDisplayItem> allRoomsDisplay = [];

    // Tab 2: Shift Operations
    private readonly DataGridView gridShifts = Ui.Grid();
    private readonly Label lblCurrentShiftStatus = new();
    private readonly Label lblCurrentShiftDetail = new();
    private readonly DateTimePicker dtpFrom = new() { Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy", Width = 120 };
    private readonly DateTimePicker dtpTo = new() { Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy", Width = 120 };
    private CashShift? activeOpenShift;
    private bool isLoading;

    private sealed record RoomDisplayItem(
        int Id,
        string Number,
        string Type,
        decimal Rate,
        string StatusText,
        string Guest,
        string Phone,
        DateTime? Arrival,
        DateTime? Departure,
        decimal Deposit,
        RoomStatus Status
    );

    public FormCaTruc(HotelService service, UserSession user)
    {
        this.service = service;
        this.user = user;

        Text = "Quản lý Ca trực & Trạng thái phòng";
        Size = new Size(1280, 840);
        MinimumSize = new Size(1000, 680);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = AppTheme.Canvas;
        Font = AppTheme.Body;

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(16, 12, 16, 14),
            BackColor = AppTheme.Canvas
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 74));  // Header
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 95));  // KPI Cards
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // Main Tabs
        Controls.Add(root);

        // 1. Header
        root.Controls.Add(BuildHeader(), 0, 0);

        // 2. KPI Cards
        root.Controls.Add(BuildKpiCards(), 0, 1);

        // 3. TabControl (Phòng & Ca trực)
        root.Controls.Add(BuildTabs(), 0, 2);

        Shown += async (_, _) => await RefreshDataAsync();
    }

    private Control BuildHeader()
    {
        var header = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(16, 10, 16, 10),
            Margin = new Padding(0, 0, 0, 8)
        };

        var title = new Label
        {
            Text = "QUẢN LÝ CA TRỰC & TRẠNG THÁI PHÒNG",
            Font = AppTheme.Title,
            ForeColor = AppTheme.Navy,
            Location = new Point(14, 12),
            AutoSize = true
        };

        var subtitle = new Label
        {
            Text = "Theo dõi số lượng phòng trống, phòng đang ở, phòng đã đặt và quản lý mở/giao ca thu ngân",
            Font = AppTheme.Small,
            ForeColor = AppTheme.Muted,
            Location = new Point(16, 42),
            AutoSize = true
        };

        var userBadge = new Label
        {
            Text = $"👤 {user.Username} • {RolePolicy.Name(user.Role)}",
            Font = AppTheme.Bold,
            ForeColor = AppTheme.Blue,
            BackColor = Color.FromArgb(238, 242, 255),
            Padding = new Padding(12, 6, 12, 6),
            AutoSize = true,
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        userBadge.Location = new Point(header.Width - userBadge.PreferredWidth - 20, 18);
        header.Resize += (_, _) => userBadge.Location = new Point(header.Width - userBadge.PreferredWidth - 20, 18);

        header.Controls.Add(title);
        header.Controls.Add(subtitle);
        header.Controls.Add(userBadge);

        return header;
    }

    private Control BuildKpiCards()
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 5,
            RowCount = 1,
            Margin = new Padding(0, 0, 0, 8),
            BackColor = AppTheme.Canvas
        };

        for (int i = 0; i < 5; i++)
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));

        panel.Controls.Add(CreateCard("TỔNG SỐ PHÒNG", lblTotalRooms, "Tất cả phòng", Color.FromArgb(30, 58, 138), Color.White), 0, 0);
        panel.Controls.Add(CreateCard("PHÒNG TRỐNG", lblVacantRooms, "Sẵn sàng đón khách", Color.FromArgb(4, 120, 87), Color.FromArgb(236, 253, 245)), 1, 0);
        panel.Controls.Add(CreateCard("PHÒNG ĐANG Ở", lblOccupiedRooms, "Khách đang lưu trú", Color.FromArgb(185, 28, 28), Color.FromArgb(254, 242, 242)), 2, 0);
        panel.Controls.Add(CreateCard("PHÒNG ĐÃ ĐẶT", lblReservedRooms, "Chờ nhận phòng", Color.FromArgb(180, 83, 9), Color.FromArgb(255, 251, 235)), 3, 0);
        panel.Controls.Add(CreateCard("DỌN / BẢO TRÌ", lblCleaningRooms, "Đang xử lý buồng", Color.FromArgb(75, 85, 99), Color.FromArgb(243, 244, 246)), 4, 0);

        return panel;
    }

    private Control CreateCard(string caption, Label valueLabel, string subCaption, Color accentColor, Color bg)
    {
        var card = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = bg,
            Margin = new Padding(4),
            Padding = new Padding(12, 8, 12, 8)
        };

        // Top accent line
        card.Paint += (_, e) =>
        {
            using var brush = new SolidBrush(accentColor);
            e.Graphics.FillRectangle(brush, 0, 0, card.Width, 4);
            using var borderPen = new Pen(Color.FromArgb(226, 232, 240));
            e.Graphics.DrawRectangle(borderPen, 0, 0, card.Width - 1, card.Height - 1);
        };

        var lblCap = new Label
        {
            Text = caption,
            Font = new Font("Segoe UI", 8.25F, FontStyle.Bold),
            ForeColor = accentColor,
            Location = new Point(10, 8),
            AutoSize = true
        };

        valueLabel.Text = "0";
        valueLabel.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
        valueLabel.ForeColor = accentColor;
        valueLabel.Location = new Point(8, 26);
        valueLabel.AutoSize = true;

        var lblSub = new Label
        {
            Text = subCaption,
            Font = new Font("Segoe UI", 8F, FontStyle.Regular),
            ForeColor = Color.FromArgb(100, 116, 139),
            Location = new Point(10, 63),
            AutoSize = true
        };

        card.Controls.Add(lblCap);
        card.Controls.Add(valueLabel);
        card.Controls.Add(lblSub);

        return card;
    }

    private Control BuildTabs()
    {
        var tabs = new TabControl
        {
            Dock = DockStyle.Fill,
            Font = AppTheme.Bold,
            Appearance = TabAppearance.FlatButtons,
            SizeMode = TabSizeMode.Fixed,
            ItemSize = new Size(240, 40),
            Padding = new Point(16, 6)
        };

        var tabRoom = new TabPage("SƠ ĐỒ & TRẠNG THÁI PHÒNG") { BackColor = AppTheme.Canvas };
        tabRoom.Controls.Add(BuildRoomStatusTab());

        var tabShift = new TabPage("QUẢN LÝ CA TRỰC") { BackColor = AppTheme.Canvas };
        tabShift.Controls.Add(BuildShiftTab());

        tabs.TabPages.Add(tabRoom);
        tabs.TabPages.Add(tabShift);

        return tabs;
    }

    private Control BuildRoomStatusTab()
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = Color.White,
            Padding = new Padding(12)
        };
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        // Filter toolbar
        var toolbar = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            WrapContents = false,
            AutoScroll = true,
            Padding = new Padding(0, 4, 0, 4)
        };

        Button MakeFilterBtn(string text, string filterKey, Color activeColor)
        {
            var btn = new Button
            {
                Text = text,
                AutoSize = true,
                MinimumSize = new Size(110, 36),
                Margin = new Padding(0, 0, 8, 0),
                FlatStyle = FlatStyle.Flat,
                Font = AppTheme.Bold,
                BackColor = Color.FromArgb(241, 245, 249),
                ForeColor = AppTheme.Ink,
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);

            btn.Click += (_, _) =>
            {
                roomFilter = filterKey;
                ApplyRoomFilter();
            };
            return btn;
        }

        toolbar.Controls.Add(MakeFilterBtn("Tất cả phòng", "All", AppTheme.Navy));
        toolbar.Controls.Add(MakeFilterBtn("🟢 Phòng trống", "Trong", Color.FromArgb(4, 120, 87)));
        toolbar.Controls.Add(MakeFilterBtn("🔴 Đang ở", "DangO", Color.FromArgb(185, 28, 28)));
        toolbar.Controls.Add(MakeFilterBtn("🟡 Đã đặt", "DaDat", Color.FromArgb(180, 83, 9)));
        toolbar.Controls.Add(MakeFilterBtn("⚪ Dọn / Bảo trì", "Cleaning", Color.FromArgb(75, 85, 99)));

        txtSearchRoom.Width = 220;
        txtSearchRoom.Height = 36;
        txtSearchRoom.PlaceholderText = "🔍 Tìm phòng, khách...";
        txtSearchRoom.Margin = new Padding(16, 2, 8, 0);
        txtSearchRoom.TextChanged += (_, _) => ApplyRoomFilter();
        toolbar.Controls.Add(txtSearchRoom);

        var btnRefresh = new Button
        {
            Text = "🔄 Làm mới",
            AutoSize = true,
            MinimumSize = new Size(100, 36),
            Margin = new Padding(8, 0, 0, 0)
        };
        AppTheme.Button(btnRefresh, false);
        btnRefresh.Click += async (_, _) => await RefreshDataAsync();
        toolbar.Controls.Add(btnRefresh);

        panel.Controls.Add(toolbar, 0, 0);

        // Room grid
        gridRooms.ReadOnly = true;
        gridRooms.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        gridRooms.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        gridRooms.CellFormatting += GridRooms_CellFormatting;

        panel.Controls.Add(gridRooms, 0, 1);
        return panel;
    }

    private void GridRooms_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
        var colName = gridRooms.Columns[e.ColumnIndex].Name;

        if (colName == "TrạngThái" && e.Value is string statusStr)
        {
            e.CellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            switch (statusStr)
            {
                case "Trống":
                    e.CellStyle.BackColor = Color.FromArgb(209, 250, 229);
                    e.CellStyle.ForeColor = Color.FromArgb(6, 95, 70);
                    break;
                case "Đang ở":
                    e.CellStyle.BackColor = Color.FromArgb(254, 226, 226);
                    e.CellStyle.ForeColor = Color.FromArgb(153, 27, 27);
                    break;
                case "Đã đặt":
                    e.CellStyle.BackColor = Color.FromArgb(254, 243, 199);
                    e.CellStyle.ForeColor = Color.FromArgb(146, 64, 14);
                    break;
                case "Đang dọn":
                case "Bảo trì":
                    e.CellStyle.BackColor = Color.FromArgb(243, 244, 246);
                    e.CellStyle.ForeColor = Color.FromArgb(55, 65, 81);
                    break;
            }
        }
    }

    private Control BuildShiftTab()
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            BackColor = Color.White,
            Padding = new Padding(12)
        };
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 90));  // Current shift status box
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));  // Action buttons
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // Shift history grid

        // 1. Current shift box
        var shiftBox = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(248, 250, 252),
            Padding = new Padding(16, 10, 16, 10),
            Margin = new Padding(0, 0, 0, 8)
        };
        shiftBox.Paint += (_, e) =>
        {
            using var borderPen = new Pen(Color.FromArgb(226, 232, 240));
            e.Graphics.DrawRectangle(borderPen, 0, 0, shiftBox.Width - 1, shiftBox.Height - 1);
        };

        lblCurrentShiftStatus.Text = "TRẠNG THÁI CA TRỰC: ĐANG KIỂM TRA...";
        lblCurrentShiftStatus.Font = AppTheme.Bold;
        lblCurrentShiftStatus.ForeColor = AppTheme.Navy;
        lblCurrentShiftStatus.Location = new Point(14, 12);
        lblCurrentShiftStatus.AutoSize = true;

        lblCurrentShiftDetail.Text = "Đang tải thông tin...";
        lblCurrentShiftDetail.Font = AppTheme.Body;
        lblCurrentShiftDetail.ForeColor = AppTheme.Ink;
        lblCurrentShiftDetail.Location = new Point(14, 40);
        lblCurrentShiftDetail.AutoSize = true;

        shiftBox.Controls.Add(lblCurrentShiftStatus);
        shiftBox.Controls.Add(lblCurrentShiftDetail);
        panel.Controls.Add(shiftBox, 0, 0);

        // 2. Actions toolbar
        var toolbar = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            WrapContents = false,
            AutoScroll = true,
            Padding = new Padding(0, 4, 0, 4)
        };

        Button AddActionBtn(string text, Func<Task> action, bool primary = false)
        {
            var btn = new Button
            {
                Text = text,
                AutoSize = true,
                MinimumSize = new Size(135, 38),
                Margin = new Padding(0, 0, 10, 0)
            };
            AppTheme.Button(btn, primary);
            btn.Click += async (_, _) =>
            {
                btn.Enabled = false;
                try { await action(); }
                catch (Exception ex) { Ui.Error(this, ex); }
                finally { if (!btn.IsDisposed) btn.Enabled = true; }
            };
            toolbar.Controls.Add(btn);
            return btn;
        }

        AddActionBtn("➕ Mở ca trực", OpenShiftDialogAsync, true);
        AddActionBtn("📋 Bàn giao / Chốt ca", SubmitShiftDialogAsync);
        if (user.Role is "Admin" or "Accountant" or "Manager")
        {
            AddActionBtn("🔒 Duyệt & Khóa ca", LockShiftDialogAsync);
        }

        toolbar.Controls.Add(new Label { Text = "Từ:", AutoSize = true, Margin = new Padding(12, 10, 4, 0) });
        dtpFrom.Value = DateTime.Today.AddDays(-15);
        toolbar.Controls.Add(dtpFrom);

        toolbar.Controls.Add(new Label { Text = "Đến:", AutoSize = true, Margin = new Padding(8, 10, 4, 0) });
        dtpTo.Value = DateTime.Today;
        toolbar.Controls.Add(dtpTo);

        AddActionBtn("🔄 Tải lịch sử", async () => await LoadShiftsAsync());

        panel.Controls.Add(toolbar, 0, 1);

        // 3. Shift history grid
        gridShifts.ReadOnly = true;
        gridShifts.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        gridShifts.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
        panel.Controls.Add(gridShifts, 0, 2);

        return panel;
    }

    private async Task RefreshDataAsync()
    {
        if (isLoading) return;
        isLoading = true;
        try
        {
            await LoadRoomStatusAsync();
            await LoadShiftsAsync();
        }
        catch (Exception ex)
        {
            Ui.Error(this, ex);
        }
        finally
        {
            isLoading = false;
        }
    }

    private async Task LoadRoomStatusAsync()
    {
        var dashboardData = await service.DashboardAsync();
        var rooms = dashboardData.Rooms;
        var stays = dashboardData.Stays;

        lblTotalRooms.Text = rooms.Count.ToString();
        lblVacantRooms.Text = rooms.Count(r => r.Status == RoomStatus.Trong).ToString();
        lblOccupiedRooms.Text = rooms.Count(r => r.Status == RoomStatus.DangO).ToString();
        lblReservedRooms.Text = rooms.Count(r => r.Status == RoomStatus.DaDat).ToString();
        lblCleaningRooms.Text = rooms.Count(r => r.Status is RoomStatus.DangDon or RoomStatus.BaoTri).ToString();

        allRoomsDisplay = rooms.OrderBy(r => r.Number).Select(r =>
        {
            var stay = stays.FirstOrDefault(s => s.RoomId == r.Id && (s.Status == StayStatus.Occupied || s.Status == StayStatus.Reserved));
            string statusStr = r.Status switch
            {
                RoomStatus.Trong => "Trống",
                RoomStatus.DangO => "Đang ở",
                RoomStatus.DaDat => "Đã đặt",
                RoomStatus.DangDon => "Đang dọn",
                RoomStatus.BaoTri => "Bảo trì",
                _ => r.Status.ToString()
            };

            return new RoomDisplayItem(
                r.Id,
                r.Number,
                r.Type,
                r.Rate,
                statusStr,
                stay?.Guest ?? (r.Status == RoomStatus.Trong ? "—" : "Đang trống"),
                stay?.Phone ?? "—",
                stay?.CheckIn ?? stay?.Arrival,
                stay?.Departure,
                stay?.Deposit ?? 0,
                r.Status
            );
        }).ToList();

        ApplyRoomFilter();
    }

    private void ApplyRoomFilter()
    {
        var query = allRoomsDisplay.AsEnumerable();

        if (roomFilter == "Trong")
            query = query.Where(r => r.Status == RoomStatus.Trong);
        else if (roomFilter == "DangO")
            query = query.Where(r => r.Status == RoomStatus.DangO);
        else if (roomFilter == "DaDat")
            query = query.Where(r => r.Status == RoomStatus.DaDat);
        else if (roomFilter == "Cleaning")
            query = query.Where(r => r.Status is RoomStatus.DangDon or RoomStatus.BaoTri);

        var search = txtSearchRoom.Text.Trim();
        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(r =>
                r.Number.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                r.Type.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                r.Guest.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                r.Phone.Contains(search, StringComparison.OrdinalIgnoreCase)
            );
        }

        var list = query.Select(r => new
        {
            MãPhòng = r.Id,
            SốPhòng = "P." + r.Number,
            LoạiPhòng = r.Type,
            GiáNgày = r.Rate,
            TrạngThái = r.StatusText,
            KháchHàng = r.Guest,
            ĐiệnThoại = r.Phone,
            NhậnPhòng = r.Arrival,
            DựKiếnTrả = r.Departure,
            TiềnCọc = r.Deposit
        }).ToList();

        gridRooms.DataSource = list;
        foreach (DataGridViewColumn col in gridRooms.Columns)
        {
            if (col.ValueType == typeof(decimal)) col.DefaultCellStyle.Format = "N0";
            if (col.ValueType == typeof(DateTime) || col.ValueType == typeof(DateTime?)) col.DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
        }
    }

    private async Task LoadShiftsAsync()
    {
        try
        {
            var shifts = await service.CashShiftsAsync(dtpFrom.Value.Date, dtpTo.Value.Date);
            activeOpenShift = shifts.FirstOrDefault(s => s.Status == "Open");

            if (activeOpenShift is not null)
            {
                lblCurrentShiftStatus.Text = $"🟢 CA TRỰC ĐANG MỞ #{activeOpenShift.Id} — Thu ngân: {activeOpenShift.Cashier}";
                lblCurrentShiftStatus.ForeColor = Color.FromArgb(4, 120, 87);
                lblCurrentShiftDetail.Text = $"Mở lúc: {activeOpenShift.OpenedAt:dd/MM/yyyy HH:mm}   •   Tiền mặt đầu ca: {activeOpenShift.OpeningCash:N0} đ   •   Thu tiền mặt: {activeOpenShift.CashIn:N0} đ   •   Lý thuyết: {activeOpenShift.ExpectedCash ?? activeOpenShift.OpeningCash:N0} đ";
            }
            else
            {
                lblCurrentShiftStatus.Text = "⚪ HIỆN TẠI CHƯA CÓ CA TRỰC ĐANG MỞ";
                lblCurrentShiftStatus.ForeColor = Color.FromArgb(100, 116, 139);
                lblCurrentShiftDetail.Text = "Nhân viên vui lòng bấm nút '➕ Mở ca trực' bên dưới và nhập số tiền mặt đầu ca để bắt đầu ca làm việc.";
            }

            var list = shifts.Select(s => new
            {
                MãCa = s.Id,
                ThuNgân = s.Cashier,
                MởCa = s.OpenedAt,
                ĐóngCa = s.ClosedAt,
                TrạngThái = s.Status switch { "Open" => "Đang mở", "Submitted" => "Đã bàn giao", "Locked" => "Đã khóa", _ => s.Status },
                TiềnĐầuCa = s.OpeningCash,
                ThuTiềnMặt = s.CashIn,
                ChiTiềnMặt = s.CashOut,
                ThẻPOS = s.Pos,
                ChuyểnKhoản = s.Bank,
                LýThuyết = s.ExpectedCash,
                ThựcĐếm = s.CountedCash,
                ChênhLệch = s.Status == "Open" ? null : (decimal?)s.Discrepancy,
                GiảiTrình = s.Explanation
            }).ToList();

            gridShifts.DataSource = list;
            foreach (DataGridViewColumn col in gridShifts.Columns)
            {
                if (col.ValueType == typeof(decimal) || col.ValueType == typeof(decimal?)) col.DefaultCellStyle.Format = "N0";
                if (col.ValueType == typeof(DateTime) || col.ValueType == typeof(DateTime?)) col.DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
            }
        }
        catch (Exception ex)
        {
            lblCurrentShiftStatus.Text = "⚠️ Chưa thể lấy danh sách ca trực";
            lblCurrentShiftDetail.Text = ex.Message;
        }
    }

    private Task OpenShiftDialogAsync()
    {
        if (activeOpenShift is not null)
            throw new BusinessException($"Đang có ca #{activeOpenShift.Id} của {activeOpenShift.Cashier} mở. Hãy bàn giao ca trước khi mở ca mới.");

        using var dialog = new InputDialog("Mở ca trực mới", 540, 360);
        var numOpening = Ui.Money();
        numOpening.Value = 1000000; // 1,000,000 VND default cash float
        dialog.Add("Tiền mặt đầu ca (VNĐ)", numOpening);
        dialog.Note("Số tiền mặt quỹ có sẵn tại quầy lễ tân khi bắt đầu ca trực để thối lại cho khách.");

        dialog.Action("XÁC NHẬN MỞ CA", async () =>
        {
            var shiftId = await service.OpenCashShiftAsync(numOpening.Value);
            Ui.Info(this, $"Đã mở ca trực thành công! Mã ca: #{shiftId}");
            await RefreshDataAsync();
        });

        dialog.ShowDialog(this);
        return Task.CompletedTask;
    }

    private Task SubmitShiftDialogAsync()
    {
        if (activeOpenShift is null)
            throw new BusinessException("Hiện tại không có ca trực nào đang mở để bàn giao.");

        using var dialog = new InputDialog("Kiểm đếm và Bàn giao ca", 580, 480);
        var txtCashier = new TextBox { Text = activeOpenShift.Cashier, ReadOnly = true };
        var numExpected = new TextBox { Text = (activeOpenShift.ExpectedCash ?? activeOpenShift.OpeningCash).ToString("N0") + " đ", ReadOnly = true };
        var numCounted = Ui.Money();
        numCounted.Value = activeOpenShift.ExpectedCash ?? activeOpenShift.OpeningCash;
        var txtDiff = new TextBox { Text = "0 đ (Khớp số liệu)", ReadOnly = true };
        var txtReason = Ui.Text(350);

        numCounted.ValueChanged += (_, _) =>
        {
            decimal diff = numCounted.Value - (activeOpenShift.ExpectedCash ?? activeOpenShift.OpeningCash);
            if (diff == 0)
            {
                txtDiff.Text = "0 đ (Khớp số liệu)";
                txtDiff.ForeColor = Color.Green;
            }
            else if (diff > 0)
            {
                txtDiff.Text = $"+{diff:N0} đ (Thừa tiền)";
                txtDiff.ForeColor = Color.Blue;
            }
            else
            {
                txtDiff.Text = $"{diff:N0} đ (Thiếu tiền)";
                txtDiff.ForeColor = Color.Red;
            }
        };

        dialog.Add("Mã ca trực", new TextBox { Text = $"#{activeOpenShift.Id}", ReadOnly = true });
        dialog.Add("Thu ngân trực ca", txtCashier);
        dialog.Add("Tiền mặt lý thuyết (dự kiến)", numExpected);
        dialog.Add("Tiền mặt kiểm đếm thực tế", numCounted);
        dialog.Add("Chênh lệch thừa / thiếu", txtDiff);
        dialog.Add("Giải trình (nếu có chênh lệch)", txtReason);

        dialog.Action("NỘP & BÀN GIAO CA", async () =>
        {
            await service.SubmitCashShiftAsync(activeOpenShift.Id, numCounted.Value, txtReason.Text);
            Ui.Info(this, $"Bàn giao ca #{activeOpenShift.Id} thành công!");
            await RefreshDataAsync();
        });

        dialog.ShowDialog(this);
        return Task.CompletedTask;
    }

    private Task LockShiftDialogAsync()
    {
        using var dialog = new InputDialog("Khóa ca trực (Duyệt bàn giao)", 500, 280);
        var numId = Ui.Number(int.MaxValue);
        dialog.Add("Nhập mã ca đã bàn giao", numId);
        dialog.Note("Chỉ khóa được ca ở trạng thái 'Đã bàn giao' sau khi đã kiểm tra đầy đủ số liệu.");

        dialog.Action("XÁC NHẬN KHÓA CA", async () =>
        {
            await service.LockCashShiftAsync((long)numId.Value);
            Ui.Info(this, $"Đã duyệt và khóa ca #{numId.Value} thành công!");
            await RefreshDataAsync();
        });

        dialog.ShowDialog(this);
        return Task.CompletedTask;
    }
}
