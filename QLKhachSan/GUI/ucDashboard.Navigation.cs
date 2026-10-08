using QLKhachSan.BLL;
using QLKhachSan.DTO;
using Krypton.Toolkit;

namespace QLKhachSan.GUI;

public partial class ucDashboard
{
    private sealed record MainMenuItem(string Code, string Title, string Description, Color Accent);
    private static readonly MainMenuItem[] MainMenus =
    [
        new("rooms", "Quản lý phòng", "Sơ đồ phòng, đặt và nhận phòng", AppTheme.Blue),
        new("services", "Dịch vụ", "Yêu cầu, danh mục và kho", AppTheme.Amber),
        new("customers", "Khách hàng", "Hồ sơ và lịch sử lưu trú", AppTheme.Teal),
        new("shifts", "Ca trực", "Theo dõi và bàn giao ca", Color.FromArgb(91, 111, 124)),
        new("finance", "Tài chính", "Thu chi, hóa đơn và báo cáo", Color.FromArgb(49, 107, 128)),
        new("staff", "Nhân viên & hệ thống", "Nhân sự, tài khoản và bảo mật", Color.FromArgb(78, 111, 91))
    ];
    private MenuConfiguration? menuConfiguration;
    private readonly List<Label> menuMetricValues=[];
    private sealed record VisibleFunction(string Title,string Group,string ActionCode,int SubOrder=0,int Order=0);
    private bool CanOpenSchedule() => FunctionPolicy.Can(user,"room.schedule") ||
        FunctionPolicy.Can(user,"room.checkin") ||
        FunctionPolicy.Can(user,"room.booking_edit") ||
        FunctionPolicy.Can(user,"room.booking_cancel");

    private List<VisibleFunction> MergeBookingMenu(List<VisibleFunction> functions,string menu)
    {
        var schedule=functions.FirstOrDefault(x=>x.ActionCode=="room.schedule");
        if(menu=="rooms" && schedule is null && CanOpenSchedule())
            functions.Add(new VisibleFunction("Lịch đến / đi","Lưu trú","room.schedule",10,0));
        functions.RemoveAll(x=>x.ActionCode is "room.checkout" or "room.checkin" or "room.booking_edit" or "room.booking_cancel");
        var bookings=functions.Where(x=>x.ActionCode is "room.walkin" or "room.reserve").ToArray();
        if(bookings.Length==0)return functions;
        var preferred=bookings.FirstOrDefault(x=>x.ActionCode=="room.reserve")??bookings[0];
        var first=bookings.OrderBy(x=>x.SubOrder).ThenBy(x=>x.Order).First();
        functions.RemoveAll(x=>x.ActionCode is "room.walkin" or "room.reserve");
        functions.Add(new VisibleFunction("Đặt / nhận phòng",first.Group,preferred.ActionCode,first.SubOrder,first.Order));
        return functions.OrderBy(x=>x.SubOrder).ThenBy(x=>x.Order).ToList();
    }
    private IEnumerable<MainMenuItem> VisibleMenus() => menuConfiguration is null ? MainMenus :
        menuConfiguration.Menus.Where(x=>x.Active).OrderBy(x=>x.Order)
            .Select(x=>new MainMenuItem(x.Code,x.Title,x.Description,ColorTranslator.FromHtml(x.Color)));
    private List<VisibleFunction> MenuFunctions(string menu)
    {
        if(menu=="finance")
        {
            var finance=new List<VisibleFunction>();
            if(FunctionPolicy.Can(user,"invoice.list"))
                finance.Add(new VisibleFunction("Hóa đơn, doanh thu & thu chi","Báo cáo","invoice.list",0,0));
            if(FunctionPolicy.Can(user,"cash.book") || FunctionPolicy.Can(user,"cash.bank") ||
               FunctionPolicy.Can(user,"cash.debt") || FunctionPolicy.Can(user,"invoice.control") ||
               FunctionPolicy.Can(user,"invoice.groups") || FunctionPolicy.Can(user,"report.profit"))
                finance.Add(new VisibleFunction("Sổ quỹ, công nợ & kế toán","Nghiệp vụ tài chính","finance.workspace",1,0));
            if(FunctionPolicy.Can(user,"report.revenue"))
                finance.Add(new VisibleFunction("Biểu đồ doanh thu","Báo cáo","report.revenue",0,1));
            if(menuConfiguration is not null)
                finance.AddRange(from sub in menuConfiguration.Submenus
                    join function in menuConfiguration.CustomFunctions on sub.Id equals function.SubmenuId
                    where sub.MenuCode==menu && sub.Active && function.Active &&
                          function.AllowedRoles.Split(',').Contains(user.Role)
                    select new VisibleFunction(function.Title,sub.Title,$"custom:{function.Id}",sub.Order,function.Order));
            return finance;
        }
        if(menuConfiguration is null)
            return MergeBookingMenu(FunctionPolicy.All.Where(x=>((menu=="rooms" && x.Code is ("invoice.create" or "invoice.list")) || (x.Code is not ("invoice.create" or "invoice.list") && (x.Menu==menu || (menu=="finance" && x.Menu is ("cash" or "invoices" or "reports")) ||
                (menu=="staff" && x.Menu=="system")))) &&
                (FunctionPolicy.Can(user,x.Code) || (x.Code=="room.schedule" && CanOpenSchedule())))
                .Select(x=>new VisibleFunction(x.Name,x.Submenu,x.Code)).ToList(),menu);
        var builtIn=(from sub in menuConfiguration.Submenus
                join function in menuConfiguration.Functions on sub.Id equals function.SubmenuId
                where sub.MenuCode==menu && sub.Active && function.Active &&
                    (FunctionPolicy.Can(user,function.ActionCode) ||
                        (function.ActionCode=="room.schedule" && CanOpenSchedule()))
                select new VisibleFunction(function.Title,sub.Title,function.ActionCode,sub.Order,function.Order));
        var custom=(from sub in menuConfiguration.Submenus
                join function in menuConfiguration.CustomFunctions on sub.Id equals function.SubmenuId
                where sub.MenuCode==menu && sub.Active && function.Active &&
                      function.AllowedRoles.Split(',').Contains(user.Role)
                select new VisibleFunction(function.Title,sub.Title,$"custom:{function.Id}",sub.Order,function.Order));
        var functions=builtIn.Concat(custom)
            .Where(x=>menu=="rooms" || x.ActionCode is not ("invoice.create" or "invoice.list"))
            .ToList();
        if(menu=="rooms")
            functions.AddRange(new[]{("invoice.create","Lập hóa đơn thanh toán"),("invoice.list","Xem / in hóa đơn")}
                .Where(x=>FunctionPolicy.Can(user,x.Item1))
                .Where(x=>!functions.Any(f=>f.ActionCode==x.Item1))
                .Select(x=>new VisibleFunction(x.Item2,"Thanh toán",x.Item1,100,0)));
        return MergeBookingMenu(functions,menu);
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
        "rooms" => "bed", "services" => "service", "customers" => "people2", "staff" => "gear",
        "shifts" => "calendar-clock", "finance" or "cash" => "finance", "invoices" => "receipt",
        "reports" => "chart", "system" => "key", _ => "receipt"
    };
    private static string FunctionIcon(string code) => code.StartsWith("custom:",StringComparison.Ordinal)?"folder":code switch
    {
        "room.search" => "search", "room.reserve" or "room.schedule" or "room.history" => "calendar",
        "room.deposit" or "room.pricing" or "cash.flow" or "cash.book" or "cash.debt" => "money",
        "room.checkout" or "invoice.create" or "invoice.list" or "invoice.control" or "invoice.groups" => "receipt",
        "finance.workspace" => "money",
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
        var items = VisibleMenus().Where(x => user.Role=="Admin" || MenuFunctions(x.Code).Count>0).ToArray();
        var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = AppTheme.Canvas };
        var content = new Panel { BackColor = AppTheme.Canvas };
        scroll.Controls.Add(content);

        var heading = new Panel { Location = new Point(24, 0), Height = 128, BackColor = AppTheme.Canvas };
        content.Controls.Add(heading);
        heading.Controls.Add(new Panel { Location = new Point(2, 5), Size = new Size(60, 4), BackColor = AppTheme.Focus });
        var title = new Label
        {
            Text = "Không gian làm việc", Font = AppTheme.TextFont(27F,FontStyle.Bold),
            ForeColor = AppTheme.Navy, Location = new Point(0, 20), Height = 68, AutoEllipsis = true
        };
        var introduction = new Label
        {
            Text = "Quản lý khách sạn tập trung, rõ ràng và nhanh chóng", Font = AppTheme.TextFont(11F),
            ForeColor = AppTheme.Muted, Location = new Point(3, 87), Height = 32, AutoEllipsis = true
        };
        heading.Controls.Add(title);
        heading.Controls.Add(introduction);
        var headerActions=new FlowLayoutPanel {Size=new Size(408,46),WrapContents=false,BackColor=AppTheme.Canvas};
        heading.Controls.Add(headerActions);
        foreach(var (code,label) in new[]
        {
            ("room.map","Sơ đồ phòng  ↗"),
            (FunctionPolicy.Can(user,"room.reserve")?"room.reserve":"room.walkin","Đặt / nhận phòng  ↗")
        })
        {
            if(!FunctionPolicy.Can(user,code))continue;
            var action=new KryptonButton {Size=new Size(190,42),Margin=new Padding(0,0,10,0),Cursor=Cursors.Hand};
            action.Values.Text=label;
            action.StateCommon.Back.Color1=code=="room.map"?Color.White:AppTheme.Blue;
            action.StateCommon.Back.Color2=action.StateCommon.Back.Color1;
            action.StateCommon.Content.ShortText.Color1=code=="room.map"?AppTheme.Ink:Color.White;
            action.StateCommon.Content.ShortText.Font=AppTheme.Bold;
            action.StateCommon.Border.Color1=code=="room.map"?AppTheme.Border:AppTheme.Blue;
            action.StateCommon.Border.Color2=action.StateCommon.Border.Color1;
            action.StateCommon.Border.Rounding=12;
            action.Click+=async (_,_)=>await OpenFunction(code);
            headerActions.Controls.Add(action);
        }
        var footerActions=new FlowLayoutPanel {Height=54,BackColor=AppTheme.Canvas,WrapContents=false};
        content.Controls.Add(footerActions);
        var shortcuts = new List<Button>();
        foreach (var (code, label) in new[]
        {
            ("room.map", "Xem sơ đồ phòng  →"),
            (FunctionPolicy.Can(user,"room.reserve")?"room.reserve":"room.walkin", "Đặt / nhận phòng  →")
        })
        {
            if (!FunctionPolicy.Can(user, code)) continue;
            var shortcut = new Button
            {
                Text = label, Size = new Size(182, 38), Font = AppTheme.Bold,
                Cursor = Cursors.Hand, UseMnemonic = false
            };
            AppTheme.Button(shortcut);
            shortcut.Click += async (_, _) => await OpenFunction(code);
            shortcuts.Add(shortcut);
            footerActions.Controls.Add(shortcut);
        }

        var showMetrics=RolePolicy.CanViewOperations(user.Role);
        var metricStrip=new TableLayoutPanel {Height=82,ColumnCount=4,RowCount=1,BackColor=AppTheme.Canvas};
        for(var i=0;i<4;i++)metricStrip.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,25));
        if(showMetrics)
        {
            var values=new[]
            {
                ("Đang có khách",data.Rooms.Count(x=>x.Status==RoomStatus.DangO),AppTheme.Blue),
                ("Phòng sẵn sàng",data.Rooms.Count(x=>x.Status==RoomStatus.Trong),AppTheme.Teal),
                ("Lượt đặt trước",data.Stays.Count(x=>x.Status==StayStatus.Reserved),AppTheme.Amber),
                ("Dịch vụ đang chờ",data.Pending.Count,AppTheme.Amber)
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

        var cards = new List<DashboardMenuCard>();
        for (var i = 0; i < items.Length; i++)
        {
            var item = items[i];
            var description=item.Description==MainMenus.FirstOrDefault(x=>x.Code==item.Code)?.Description
                ? item.Code switch
                {
                    "rooms"=>"Quản lý tình trạng phòng, đặt phòng, nhận và trả phòng.",
                    "services"=>"Quản lý dịch vụ khách sạn, yêu cầu và kho minibar.",
                    "customers"=>"Quản lý hồ sơ khách hàng và lịch sử lưu trú.",
                    "shifts"=>"Theo dõi ca trực và bàn giao công việc.",
                    "finance"=>"Quản lý doanh thu, thu chi, hóa đơn và báo cáo.",
                    "staff"=>"Quản lý nhân viên, phân quyền và cấu hình hệ thống.",
                    _=>item.Description
                }
                : item.Description;
            var configuredIcon=menuConfiguration?.Menus.FirstOrDefault(x=>x.Code==item.Code)?.Icon;
            var card = new DashboardMenuCard
            {
                Title = item.Title, Description = description,
                Accent = item.Accent, IconKind = configuredIcon is null or "people" or "clock" or "money"
                    ? MenuIcon(item.Code) : configuredIcon,
                IconPng = menuConfiguration?.Menus.FirstOrDefault(x=>x.Code==item.Code)?.IconPng, Height = 220,
                AccessibleName = item.Title
            };
            card.Click += async (_, _) =>
            {
                if(item.Code=="shifts" && FunctionPolicy.Can(user,"shift.manage"))await OpenFunction("shift.manage");
                else ShowSubmenu(item.Code);
            };
            cards.Add(card);
            content.Controls.Add(card);
        }
        void LayoutMenu()
        {
            var width = Math.Max(560, scroll.ClientSize.Width);
            var innerWidth = width - 48;
            content.Width = width;
            heading.Width = innerWidth;
            title.Width = Math.Max(200, innerWidth - 20);
            introduction.Width = Math.Max(200, innerWidth - 20);
            headerActions.Location=new Point(Math.Max(24,innerWidth-headerActions.Width),51);
            headerActions.Visible=innerWidth>=850;
            title.Width=innerWidth>=850?Math.Max(250,innerWidth-headerActions.Width-18):innerWidth;
            var columns = innerWidth >= 980 ? 3 : innerWidth >= 610 ? 2 : 1;
            const int gap = 24;
            var cardWidth = (innerWidth - gap * (columns - 1)) / columns;
            const int cardTop=130;
            var rows=(cards.Count + columns - 1) / columns;
            var available=scroll.ClientSize.Height-cardTop-40-gap*(rows-1);
            var cardHeight=columns==3?Math.Clamp(available/Math.Max(1,rows),215,326):220;
            var rowStep=cardHeight+gap;
            for (var i = 0; i < cards.Count; i++)
            {
                cards[i].Bounds = new Rectangle(24 + (i % columns) * (cardWidth + gap),
                    cardTop + (i / columns) * rowStep, cardWidth, cardHeight);
            }
            var footerTop=cardTop+rows*cardHeight+(rows-1)*gap+108;
            footerActions.Location=new Point(24,footerTop);
            footerActions.Width=innerWidth;
            if(showMetrics)
            {
                metricStrip.Location=new Point(24,footerTop+70);
                metricStrip.Width=innerWidth;
            }
            content.Height=footerTop+(showMetrics?165:70);
        }
        scroll.Resize += (_, _) => LayoutMenu();
        menuPage.Controls.Add(scroll);
        if(user.Role=="Admin" && FunctionPolicy.Can(user,"staff.manage"))
        {
            var manage=new Button {Text="⚙  Tùy chỉnh menu",Size=new Size(170,38),Anchor=AnchorStyles.Top|AnchorStyles.Right};
            AppTheme.Button(manage);
            manage.Click+=async (_,_)=>{try{await ShowMenuConfiguration();}catch(Exception ex){Ui.Error(this,ex);}};
            footerActions.Controls.Add(manage);
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
        if (MenuFunctions(module).Count==0 && user.Role!="Admin") return;
        var menu = VisibleMenus().Single(x => x.Code == module);
        activeModule = module;
        activeFunction = null;
        ClearPage(modulePage);
        var heading = PageHeading(menu.Title, "Chọn chức năng cần làm.");
        AddBackButton(heading, "← Menu tổng", ShowMainMenu);
        var groups = new Panel
        {
            Dock = DockStyle.Fill, AutoScroll = true, BackColor = AppTheme.Canvas
        };
        var groupLayouts=new List<(DashboardMetricCard Card,Action Size)>();
        if (MenuFunctions(module).Count==0)
        {
            var empty=new Label {Text="Menu này chưa có chức năng. Vào Tùy chỉnh menu để thêm menu con và chức năng.",
                AutoSize=true,Font=AppTheme.Body,ForeColor=AppTheme.Muted,Margin=new Padding(18)};
            groups.Controls.Add(empty);
        }
        foreach (var group in MenuFunctions(module).GroupBy(x => x.Group))
        {
            var functions = group.ToArray();
            var card = new DashboardMetricCard { BackColor = Color.White,
                Accent=menu.Accent, Margin = new Padding(0,0,0,12) };
            var title = new Label
            {
                Text = group.Key, Location=new Point(20,14),Height = 28,
                Font = AppTheme.TextFont(11F,FontStyle.Bold), ForeColor = AppTheme.Ink,
                UseMnemonic = false,TextAlign=ContentAlignment.MiddleLeft,AutoEllipsis=true
            };
            var count = new Label
            {
                Text=$"{functions.Length} chức năng",Height=22,TextAlign=ContentAlignment.MiddleLeft,
                Font=AppTheme.Small,ForeColor=AppTheme.Muted
            };
            var actions = new Panel {BackColor=Color.White};
            foreach (var function in functions)
            {
                var button = new DashboardFunctionButton
                {
                    Text=function.Title,IconKind=FunctionIcon(function.ActionCode),
                    AccessibleName=function.Title,UseMnemonic=false
                };
                button.Click += async (_, _) => await OpenFunction(function.ActionCode);
                actions.Controls.Add(button);
            }
            card.Controls.Add(actions);
            card.Controls.Add(title);
            card.Controls.Add(count);
            void SizeSection()
            {
            var width=Math.Max(500,groups.ClientSize.Width-88);
                var columns=Math.Min(functions.Length,width>=1120?4:width>=800?3:2);
                var inner=width-212;
                var buttonWidth=(inner-10*(columns-1))/columns;
                var rows=(functions.Length+columns-1)/columns;
                card.Width=width;
                title.Width=165;
                count.SetBounds(20,43,150,22);
                actions.SetBounds(192,16,inner,rows*62);
                for(var i=0;i<actions.Controls.Count;i++)
                    actions.Controls[i].Bounds=new Rectangle((i%columns)*(buttonWidth+10),(i/columns)*62,
                        buttonWidth,54);
                card.Height=Math.Max(86,actions.Height+32);
            }
            groups.Controls.Add(card);
            groupLayouts.Add((card,SizeSection));
        }
        void LayoutGroups()
        {
            var top=12;
            foreach(var (card,size) in groupLayouts)
            {
                size();
                card.Location=new Point(20,top);
                top+=card.Height+12;
            }
            groups.AutoScrollMinSize=new Size(0,top+12);
            groups.HorizontalScroll.Visible=false;
        }
        groups.Resize+=(_,_)=>LayoutGroups();
        LayoutGroups();
        modulePage.Controls.Add(groups);
        modulePage.Controls.Add(heading);
        ActivatePage(modulePage);
        UpdateNavigationTitle();
    }

    private async Task OpenFunction(string code)
    {
        if(code=="finance.workspace")
        {
            await Run(()=>OpenAccounting("Sổ quỹ"));
            return;
        }
        if(code.StartsWith("custom:",StringComparison.Ordinal) && long.TryParse(code.AsSpan(7),out var customId))
        {
            try{await ShowCustomScreen(customId);}catch(Exception ex){Ui.Error(this,ex);}
            return;
        }
        if(code=="room.schedule")
        {
            if(!CanOpenSchedule())throw new BusinessException("Bạn không được cấp quyền sử dụng chức năng này.");
        }
        else FunctionPolicy.Require(user, code);
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
        if (code == "room.map")
        {
            functionPage.Controls.Add(BuildRoomOverview());
        }
        else if (code == "report.revenue")
        {
            functionPage.Controls.Add(financeChart);
        }
        else
        {
            tabMainView.TabPages.Clear();
            tabMainView.TabPages.Add(tabLichTrinh);
            tabMainView.SelectedTab = tabLichTrinh;
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
        heading.Resize += (_, _) => backToMenu.Location = new Point(Math.Max(0, heading.ClientSize.Width - 350), 36);
        backToMenu.Location = new Point(Math.Max(0, heading.ClientSize.Width - 350), 36);
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
        var heading = new Panel { Dock = DockStyle.Top, Height = 100, BackColor = AppTheme.Canvas, Padding = new Padding(22, 12, 20, 8) };
        heading.Paint+=(_,e)=>{using var mark=new SolidBrush(AppTheme.Focus);e.Graphics.FillRectangle(mark,24,13,42,3);};
        var detailLabel = new Label { Text = detail, Location = new Point(25, 68), Height = 23, ForeColor = AppTheme.Muted, Font=AppTheme.Small };
        var titleLabel = new Label
        {
            Text = title, Location = new Point(25, 27), Height = 38,
            Font = AppTheme.TextFont(21F,FontStyle.Bold), ForeColor = AppTheme.Ink, UseMnemonic = false
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
        heading.Resize += (_, _) => back.Location = new Point(Math.Max(0, heading.ClientSize.Width - 182), 36);
        back.Location = new Point(Math.Max(0, heading.ClientSize.Width - 182), 36);
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
        lblHeaderTitle.Text = "HOTEL DESK";
        lblHeaderSubtitle.Text = $"{title}   /   {user.DisplayName}  •  {user.Role}";
    }
}
