using QLKhachSan.BLL;
using QLKhachSan.DTO;

namespace QLKhachSan.GUI;

public partial class ucDashboard
{
    private sealed record MainMenuItem(string Code, string Title, string Description, Color Accent);
    private static readonly MainMenuItem[] MainMenus =
    [
        new("rooms", "Quản lý phòng", "Sơ đồ phòng, đặt và nhận phòng", Color.FromArgb(62, 105, 157)),
        new("services", "Dịch vụ", "Yêu cầu, danh mục và kho", Color.FromArgb(170, 105, 59)),
        new("customers", "Khách hàng", "Hồ sơ và lịch sử lưu trú", Color.FromArgb(52, 132, 119)),
        new("shifts", "Ca trực", "Theo dõi và bàn giao ca", Color.FromArgb(130, 96, 157)),
        new("cash", "Thu chi", "Giao dịch, sổ quỹ và công nợ", Color.FromArgb(52, 128, 151)),
        new("invoices", "Hóa đơn", "Danh sách và đối soát", Color.FromArgb(181, 107, 62)),
        new("reports", "Báo cáo", "Doanh thu và kết quả kinh doanh", Color.FromArgb(81, 110, 171)),
        new("staff", "Nhân viên", "Nhân sự và tài khoản", Color.FromArgb(76, 126, 91)),
        new("system", "Hệ thống", "Bảo mật và nhật ký hoạt động", Color.FromArgb(105, 109, 130))
    ];
    private MenuConfiguration? menuConfiguration;
    private readonly List<Label> menuMetricValues=[];
    private sealed record VisibleFunction(string Title,string Group,string ActionCode);
    private IEnumerable<MainMenuItem> VisibleMenus() => menuConfiguration is null ? MainMenus :
        menuConfiguration.Menus.Where(x=>x.Active).OrderBy(x=>x.Order)
            .Select(x=>new MainMenuItem(x.Code,x.Title,x.Description,ColorTranslator.FromHtml(x.Color)));
    private List<VisibleFunction> MenuFunctions(string menu)
    {
        if(menuConfiguration is null)
            return FunctionPolicy.All.Where(x=>x.Menu==menu && FunctionPolicy.Can(user,x.Code))
                .Select(x=>new VisibleFunction(x.Name,x.Submenu,x.Code)).ToList();
        return (from sub in menuConfiguration.Submenus
                join function in menuConfiguration.Functions on sub.Id equals function.SubmenuId
                where sub.MenuCode==menu && sub.Active && function.Active && FunctionPolicy.Can(user,function.ActionCode)
                orderby sub.Order,function.Order
                select new VisibleFunction(function.Title,sub.Title,function.ActionCode)).ToList();
    }
    private async Task ReloadMenuConfiguration()
    {
        menuConfiguration=await service.MenuConfigurationAsync();
        if(activeModule=="menu")
        {
            BuildMenuPage();ActivatePage(menuPage);UpdateNavigationTitle();
        }
        else if(VisibleMenus().Any(x=>x.Code==activeModule))ShowSubmenu(activeModule);
        else ShowMainMenu();
    }
    private static string MenuIcon(string code) => code switch
    {
        "rooms" => "bed", "services" => "service", "customers" or "staff" => "people",
        "shifts" => "clock", "cash" => "money", "invoices" => "receipt",
        "reports" => "chart", "system" => "key", _ => "receipt"
    };
    private static string FunctionIcon(string code) => code switch
    {
        "room.search" => "search", "room.reserve" or "room.schedule" or "room.history" => "calendar",
        "room.deposit" or "room.pricing" or "cash.flow" or "cash.book" or "cash.debt" => "money",
        "room.checkout" or "invoice.create" or "invoice.list" or "invoice.control" or "invoice.groups" => "receipt",
        "room.transfer" => "swap", "room.extend" or "shift.manage" or "system.audit" => "clock",
        "room.maintain" or "room.booking_edit" or "room.booking_cancel" => "tools",
        "service.order" or "service.manage" or "service.catalog" or "service.stock" => "service",
        "customer.profile" or "customer.history" or "staff.view" or "staff.manage" => "people",
        "report.revenue" or "report.profit" => "chart", "system.password" => "key",
        _ => "bed"
    };

    private readonly Panel navigationContent = new() { Dock = DockStyle.Fill };
    private readonly Panel menuPage = new() { Dock = DockStyle.Fill, BackColor = AppTheme.Canvas };
    private readonly Panel modulePage = new() { Dock = DockStyle.Fill, BackColor = AppTheme.Canvas };
    private readonly Panel functionPage = new() { Dock = DockStyle.Fill, BackColor = AppTheme.Canvas };
    private readonly TabControl financeChart = new() { Dock = DockStyle.Fill };
    private bool navigationReady;
    private string activeModule = "menu";
    private string? activeFunction;

    private void InitializeNavigation()
    {
        SuspendLayout();
        tlpCards.Visible = false;
        pnlLeftTools.Visible = false;
        tlpBody.ColumnStyles[0].Width = 0;
        pnlRight.Visible = false;
        tlpBody.ColumnStyles[2].Width = 0;
        tlpBody.Controls.Remove(tabMainView);
        tabMainView.TabPages.Remove(tabThongKe);
        financeChart.TabPages.Add(tabThongKe);
        navigationContent.Controls.Add(menuPage);
        navigationContent.Controls.Add(modulePage);
        navigationContent.Controls.Add(functionPage);
        tlpBody.Controls.Add(navigationContent, 1, 0);
        navigationReady = true;
        ShowMainMenu();
        ResumeLayout(true);
    }

    private void ShowMainMenu()
    {
        if (busy) return;
        activeModule = "menu";
        activeFunction = null;
        BuildMenuPage();
        ActivatePage(menuPage);
        UpdateNavigationTitle();
    }

    private void BuildMenuPage()
    {
        ClearPage(menuPage);
        menuMetricValues.Clear();
        var items = VisibleMenus().Where(x => MenuFunctions(x.Code).Count>0).ToArray();
        var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = AppTheme.Canvas };
        var content = new Panel { BackColor = AppTheme.Canvas };
        scroll.Controls.Add(content);

        var hero = new DashboardMenuHero { Location = new Point(24, 20), Height = 158 };
        content.Controls.Add(hero);
        var greeting = new Label
        {
            Text = "KHÔNG GIAN LÀM VIỆC", Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold),
            ForeColor = Color.FromArgb(201, 221, 236), BackColor = Color.Transparent,
            Location = new Point(26, 20), AutoSize = true
        };
        var welcome = new Label
        {
            Text = $"Xin chào, {user.Username}", Font = new Font("Segoe UI Semibold", 22F, FontStyle.Bold),
            ForeColor = Color.White, BackColor = Color.Transparent,
            Location = new Point(24, 42), Height = 43, AutoEllipsis = true
        };
        var introduction = new Label
        {
            Text = "Mọi công việc của khách sạn trong một nơi.", Font = AppTheme.Body,
            ForeColor = Color.FromArgb(220, 231, 241), BackColor = Color.Transparent,
            Location = new Point(27, 91), Height = 27, AutoEllipsis = true
        };
        hero.Controls.Add(greeting);
        hero.Controls.Add(welcome);
        hero.Controls.Add(introduction);
        var shortcuts = new List<Button>();
        foreach (var (code, label) in new[]
        {
            ("room.map", "Xem sơ đồ phòng  →"),
            ("room.reserve", "Đặt phòng mới  →")
        })
        {
            if (!FunctionPolicy.Can(user, code)) continue;
            var shortcut = new Button
            {
                Text = label, Size = new Size(186, 34), Font = AppTheme.Bold,
                ForeColor = Color.White, BackColor = Color.FromArgb(73, 111, 151),
                FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand, UseMnemonic = false
            };
            shortcut.FlatAppearance.BorderSize = 0;
            shortcut.FlatAppearance.MouseOverBackColor = Color.FromArgb(91, 134, 172);
            shortcut.Click += async (_, _) => await OpenFunction(code);
            shortcuts.Add(shortcut);
            hero.Controls.Add(shortcut);
        }

        var showMetrics=RolePolicy.CanViewOperations(user.Role);
        var metricStrip=new TableLayoutPanel {Location=new Point(24,194),Height=82,ColumnCount=4,RowCount=1,BackColor=AppTheme.Canvas};
        for(var i=0;i<4;i++)metricStrip.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,25));
        if(showMetrics)
        {
            var values=new[]
            {
                ("Đang có khách",data.Rooms.Count(x=>x.Status==RoomStatus.DangO),AppTheme.Blue),
                ("Phòng sẵn sàng",data.Rooms.Count(x=>x.Status==RoomStatus.Trong),AppTheme.Teal),
                ("Lượt đặt trước",data.Stays.Count(x=>x.Status==StayStatus.Reserved),AppTheme.Amber),
                ("Dịch vụ đang chờ",data.Pending.Count,Color.FromArgb(145,94,121))
            };
            for(var i=0;i<values.Length;i++)
            {
                var stat=values[i];
                var card=new Panel {Dock=DockStyle.Fill,BackColor=Color.White,Margin=new Padding(i==0?0:6,0,i==3?0:6,0)};
                card.Paint+=(_,e)=>{using var edge=new Pen(AppTheme.Border);e.Graphics.DrawRectangle(edge,0,0,card.Width-1,card.Height-1);
                    using var mark=new SolidBrush(stat.Item3);e.Graphics.FillRectangle(mark,0,0,4,card.Height);};
                var value=new Label {Text=stat.Item2.ToString(),Font=new Font("Segoe UI Semibold",19F,FontStyle.Bold),
                    ForeColor=stat.Item3,Location=new Point(18,8),Size=new Size(100,36)};
                card.Controls.Add(value);menuMetricValues.Add(value);
                card.Controls.Add(new Label {Text=stat.Item1,Font=AppTheme.Small,ForeColor=AppTheme.Muted,
                    Location=new Point(19,51),AutoSize=true});
                metricStrip.Controls.Add(card,i,0);
            }
            content.Controls.Add(metricStrip);
        }

        var sectionTitle = new Label
        {
            Text = "Khám phá chức năng", Font = new Font("Segoe UI Semibold", 15F, FontStyle.Bold),
            ForeColor = AppTheme.Ink, Location = new Point(27, showMetrics?300:202), AutoSize = true
        };
        var sectionHint = new Label
        {
            Text = "Chọn một khu vực để bắt đầu công việc", Font = AppTheme.Body,
            ForeColor = AppTheme.Muted, Location = new Point(28, showMetrics?335:237), AutoSize = true
        };
        content.Controls.Add(sectionTitle);
        content.Controls.Add(sectionHint);
        var cards = new List<DashboardMenuCard>();
        for (var i = 0; i < items.Length; i++)
        {
            var item = items[i];
            var count = MenuFunctions(item.Code).Count;
            var card = new DashboardMenuCard
            {
                Title = item.Title, Description = item.Description, FunctionCount = count,
                Accent = item.Accent, IconKind = menuConfiguration?.Menus.FirstOrDefault(x=>x.Code==item.Code)?.Icon ?? MenuIcon(item.Code), Height = 138,
                AccessibleName = $"{item.Title}, {count} chức năng"
            };
            card.Click += (_, _) => ShowSubmenu(item.Code);
            cards.Add(card);
            content.Controls.Add(card);
        }
        void LayoutMenu()
        {
            var width = Math.Max(560, scroll.ClientSize.Width);
            var innerWidth = width - 48;
            content.Width = width;
            hero.Width = innerWidth;
            if(showMetrics)metricStrip.Width=innerWidth;
            var showShortcuts = innerWidth >= 760;
            welcome.Width = Math.Max(200, innerWidth - (showShortcuts ? 250 : 54));
            introduction.Width = Math.Max(200, innerWidth - (showShortcuts ? 250 : 54));
            for (var i = 0; i < shortcuts.Count; i++)
            {
                shortcuts[i].Visible = showShortcuts;
                shortcuts[i].Location = new Point(innerWidth - 211, 42 + i * 43);
            }
            var columns = innerWidth >= 980 ? 3 : innerWidth >= 610 ? 2 : 1;
            const int gap = 16;
            var cardWidth = (innerWidth - gap * (columns - 1)) / columns;
            var cardTop=showMetrics?378:280;
            for (var i = 0; i < cards.Count; i++)
            {
                cards[i].Bounds = new Rectangle(24 + (i % columns) * (cardWidth + gap),
                    cardTop + (i / columns) * 154, cardWidth, 138);
            }
            content.Height = cardTop+20 + ((cards.Count + columns - 1) / columns) * 154;
        }
        scroll.Resize += (_, _) => LayoutMenu();
        menuPage.Controls.Add(scroll);
        if(user.Role=="Admin" && FunctionPolicy.Can(user,"staff.manage"))
        {
            var manage=new Button {Text="⚙  Tùy chỉnh menu",Size=new Size(170,38),Anchor=AnchorStyles.Top|AnchorStyles.Right};
            AppTheme.Button(manage);
            manage.Click+=async (_,_)=>{try{await ShowMenuConfiguration();}catch(Exception ex){Ui.Error(this,ex);}};
            menuPage.Controls.Add(manage);
            EventHandler placeManage=(_,_)=>manage.Location=new Point(Math.Max(16,menuPage.ClientSize.Width-194),showMetrics?294:195);
            menuPage.Resize+=placeManage;
            manage.Disposed+=(_,_)=>menuPage.Resize-=placeManage;
            placeManage(menuPage,EventArgs.Empty);
            manage.BringToFront();
        }
        LayoutMenu();
    }
    private void UpdateMenuMetrics()
    {
        if(menuMetricValues.Count!=4 || menuMetricValues.Any(x=>x.IsDisposed))return;
        var values=new[] {data.Rooms.Count(x=>x.Status==RoomStatus.DangO),
            data.Rooms.Count(x=>x.Status==RoomStatus.Trong),data.Stays.Count(x=>x.Status==StayStatus.Reserved),data.Pending.Count};
        for(var i=0;i<4;i++)menuMetricValues[i].Text=values[i].ToString();
    }

    private void ShowModule(string module) => ShowSubmenu(module);

    private void ShowSubmenu(string module)
    {
        if (busy) return;
        if (module == "menu") { ShowMainMenu(); return; }
        if (MenuFunctions(module).Count==0) return;
        var menu = VisibleMenus().Single(x => x.Code == module);
        activeModule = module;
        activeFunction = null;
        ClearPage(modulePage);
        var heading = PageHeading(menu.Title, "Chọn chức năng cần làm.");
        AddBackButton(heading, "← Menu tổng", ShowMainMenu);
        var groups = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill, AutoScroll = true, WrapContents = true,
            Padding = new Padding(14, 14, 8, 14), BackColor = AppTheme.Canvas
        };
        foreach (var group in MenuFunctions(module).GroupBy(x => x.Group))
        {
            var functions = group.ToArray();
            var actionRows = (functions.Length + 1) / 2;
            var card = new Panel { Height = 65 + actionRows * 64, BackColor = Color.White, Margin = new Padding(8), Padding = new Padding(15) };
            card.Paint+=(_,e)=>{using var border=new Pen(AppTheme.Border);e.Graphics.DrawRectangle(border,0,0,card.Width-1,card.Height-1);};
            var title = new Label
            {
                Text = group.Key, Dock = DockStyle.Top, Height = 35,
                Font = AppTheme.Bold, ForeColor = AppTheme.Ink, UseMnemonic = false
            };
            var actions = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 2, RowCount = actionRows, BackColor = Color.White
            };
            actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            for (var row = 0; row < actionRows; row++) actions.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));
            for (var i = 0; i < functions.Length; i++)
            {
                var function = functions[i];
                var button = new Button
                {
                    Text = function.Title, Image = UiIcons.Create(FunctionIcon(function.ActionCode), AppTheme.Blue, 22),
                    ImageAlign = ContentAlignment.MiddleLeft, TextImageRelation = TextImageRelation.ImageBeforeText,
                    Dock = DockStyle.Fill, Margin = new Padding(3),
                    TextAlign = ContentAlignment.MiddleLeft, Font = AppTheme.Body,
                    UseMnemonic = false, Cursor = Cursors.Hand
                };
                AppTheme.Button(button);
                button.Disposed += (_, _) => button.Image?.Dispose();
                button.Click += async (_, _) => await OpenFunction(function.ActionCode);
                actions.Controls.Add(button, i % 2, i / 2);
            }
            card.Controls.Add(actions);
            card.Controls.Add(title);
            void SizeCard()
            {
                var columns=groups.ClientSize.Width>=1050?3:groups.ClientSize.Width>=670?2:1;
                var width = Math.Max(245, (groups.ClientSize.Width - 56 - 16*columns) / columns);
                card.Width = width;
            }
            groups.Resize += (_, _) => SizeCard();
            groups.Controls.Add(card);
            SizeCard();
        }
        modulePage.Controls.Add(groups);
        modulePage.Controls.Add(heading);
        ActivatePage(modulePage);
        UpdateNavigationTitle();
    }

    private async Task OpenFunction(string code)
    {
        FunctionPolicy.Require(user, code);
        activeFunction = code;
        UpdateNavigationTitle();
        if (code is "room.map" or "room.schedule" or "report.revenue")
        {
            ShowEmbeddedFunction(code);
            return;
        }
        if (code is "room.walkin" or "room.reserve")
        {
            ShowBookingScreen(code == "room.reserve");
            return;
        }
        if (code is "room.deposit" or "room.checkin" or "room.booking_edit" or "room.booking_cancel")
        {
            ShowReservedBookingScreen(code);
            return;
        }
        switch (code)
        {
            case "room.search": await Run(ShowRoomSearch); break;
            case "room.checkout": await Run(async () =>
            {
                var stay = SelectStay("Chọn phòng trả");
                if (stay is not null) await ShowCheckout(stay);
            }); break;
            case "room.transfer": InvokeLegacy(btnDoiPhong_Click); break;
            case "room.extend": InvokeLegacy(btnGiaHan_Click); break;
            case "room.history": await Run(ShowStayHistory); break;
            case "room.clean": InvokeLegacy(btnBaoDonXong_Click); break;
            case "room.maintain": InvokeLegacy(btnBaoTri_Click); break;
            case "room.catalog": await Run(ShowRoomCatalog); break;
            case "room.pricing": await Run(ShowPricing); break;
            case "service.order": InvokeLegacy(btnGoiDichVu_Click); break;
            case "service.manage": await Run(ShowOrderManagement); break;
            case "service.catalog": await Run(ShowServiceCatalog); break;
            case "service.stock": await Run(() => OpenAccounting("Kho minibar")); break;
            case "customer.profile": InvokeLegacy(btnQuanLyKhach_Click); break;
            case "customer.history": await Run(ShowStayHistory); break;
            case "shift.manage": await Run(() => OpenAccounting("Ca trực")); break;
            case "cash.flow": await Run(ShowCashFlow); break;
            case "cash.book": await Run(() => OpenAccounting("Sổ quỹ")); break;
            case "cash.bank": await Run(() => OpenAccounting("Đối soát ngân hàng")); break;
            case "cash.debt": await Run(() => OpenAccounting("Công nợ")); break;
            case "invoice.list": await Run(ShowInvoices); break;
            case "invoice.create": await Run(async()=>
            {
                var stay=SelectStay("Chọn phòng đang ở để lập hóa đơn");
                if(stay is not null)await ShowCheckout(stay);
            }); break;
            case "invoice.control": await Run(() => OpenAccounting("Hóa đơn")); break;
            case "invoice.groups": await Run(() => OpenAccounting("Nhóm bill")); break;
            case "report.profit": await Run(() => OpenAccounting("Lãi lỗ")); break;
            case "staff.view": await Run(ShowEmployees); break;
            case "staff.manage": await Run(ShowAccounts); break;
            case "system.password": await Run(ShowAccounts); break;
            case "system.audit": await Run(ShowAudit); break;
        }
        activeFunction = null;
        UpdateNavigationTitle();
    }

    private void ShowEmbeddedFunction(string code)
    {
        ClearPage(functionPage);
        var heading = FunctionHeading(code);
        if (code == "report.revenue")
        {
            functionPage.Controls.Add(financeChart);
        }
        else
        {
            tabMainView.TabPages.Clear();
            if (code == "room.map") tabMainView.TabPages.Add(tabMatrix);
            else tabMainView.TabPages.Add(tabLichTrinh);
            tabMainView.SelectedTab = code == "room.map" ? tabMatrix : tabLichTrinh;
            functionPage.Controls.Add(tabMainView);
        }
        functionPage.Controls.Add(heading);
        ActivatePage(functionPage);
        UpdateNavigationTitle();
    }

    private Panel FunctionHeading(string code, string detail = "Màn hình chức năng")
    {
        var selected = FunctionPolicy.All.Single(x => x.Code == code);
        var heading = PageHeading(selected.Name, detail);
        AddBackButton(heading, "← Menu con", () => ShowSubmenu(activeModule));
        var backToMenu = new Button { Text = "← Menu tổng", Size = new Size(150, 40), Anchor = AnchorStyles.Top | AnchorStyles.Right, UseMnemonic = false };
        AppTheme.Button(backToMenu);
        backToMenu.Click += (_, _) => ShowMainMenu();
        heading.Controls.Add(backToMenu);
        heading.Resize += (_, _) => backToMenu.Location = new Point(Math.Max(0, heading.ClientSize.Width - 350), 29);
        backToMenu.Location = new Point(Math.Max(0, heading.ClientSize.Width - 350), 29);
        backToMenu.BringToFront();
        return heading;
    }

    private Task ShowRoomSearch()
    {
        using var dialog = new InputDialog("Tìm phòng", 540, 290);
        var number = Ui.Text(10);
        number.PlaceholderText = "Nhập đúng số phòng";
        dialog.Add("Số phòng", number);
        dialog.Action("MỞ PHÒNG", async () =>
        {
            var room = data.Rooms.SingleOrDefault(r => r.Number.Equals(number.Text.Trim(), StringComparison.OrdinalIgnoreCase))
                ?? throw new BusinessException("Không tìm thấy phòng.");
            MessageBox.Show(dialog, $"Phòng {room.Number} • {room.Type} • {Ui.Status(room.Status)}", "Thông tin phòng");
            await Task.CompletedTask;
        });
        dialog.ShowDialog(this);
        return Task.CompletedTask;
    }

    private Task OpenAccounting(string tab)
    {
        using var form = new AccountingForm(user, tab);
        form.ShowDialog(this);
        return Task.CompletedTask;
    }

    private static void InvokeLegacy(EventHandler handler) => handler(null, EventArgs.Empty);

    private void ShowFinanceChart() => _ = OpenFunction("report.revenue");

    private Panel PageHeading(string title, string detail)
    {
        var heading = new Panel { Dock = DockStyle.Top, Height = 100, BackColor = Color.White, Padding = new Padding(22, 12, 20, 8) };
        var detailLabel = new Label { Text = detail, Location = new Point(22, 65), Height = 27, ForeColor = AppTheme.Muted };
        var titleLabel = new Label
        {
            Text = title, Location = new Point(22, 12), Height = 39,
            Font = AppTheme.Title, ForeColor = AppTheme.Ink, UseMnemonic = false
        };
        heading.Controls.Add(detailLabel);
        heading.Controls.Add(titleLabel);
        heading.Resize += (_, _) =>
        {
            var width = Math.Max(120, heading.ClientSize.Width - 390);
            detailLabel.Width = width;
            titleLabel.Width = width;
        };
        return heading;
    }

    private static void AddBackButton(Panel heading, string title, Action action)
    {
        var back = new Button
        {
            Text = title, Size = new Size(160, 40), Anchor = AnchorStyles.Top | AnchorStyles.Right,
            UseMnemonic = false, Cursor = Cursors.Hand
        };
        AppTheme.Button(back);
        back.Click += (_, _) => action();
        heading.Controls.Add(back);
        heading.Resize += (_, _) => back.Location = new Point(Math.Max(0, heading.ClientSize.Width - 182), 29);
        back.Location = new Point(Math.Max(0, heading.ClientSize.Width - 182), 29);
        back.BringToFront();
    }

    private void ActivatePage(Panel page)
    {
        menuPage.Visible = ReferenceEquals(page, menuPage);
        modulePage.Visible = ReferenceEquals(page, modulePage);
        functionPage.Visible = ReferenceEquals(page, functionPage);
        page.BringToFront();
    }

    private void ClearPage(Panel page)
    {
        foreach (Control child in page.Controls.Cast<Control>().ToArray())
        {
            page.Controls.Remove(child);
            if (!ReferenceEquals(child, financeChart) && !ReferenceEquals(child, tabMainView)) child.Dispose();
        }
    }

    private void UpdateRoomSidePanel()
    {
        pnlRight.Visible = false;
        tlpBody.ColumnStyles[2].Width = 0;
    }

    private void UpdateNavigationTitle()
    {
        var title = activeFunction is not null
            ? FunctionPolicy.All.FirstOrDefault(x => x.Code == activeFunction)?.Name ?? "Chức năng"
            : activeModule == "menu" ? "Menu tổng" : VisibleMenus().FirstOrDefault(x => x.Code == activeModule)?.Title ?? "Menu";
        lblHeaderTitle.Text = title;
    }
}
