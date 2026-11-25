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
        new("customers", "Khách hàng", "Hồ sơ và lịch sử lưu trú", AppTheme.Teal),
        new("shifts", "Ca trực", "Theo dõi và bàn giao ca", Color.FromArgb(91, 111, 124)),
        new("finance", "Tài chính", "Hóa đơn và doanh thu", Color.FromArgb(49, 107, 128)),
        new("staff", "Nhân viên & hệ thống", "Nhân sự, tài khoản và bảo mật", Color.FromArgb(78, 111, 91))
    ];
    private MenuConfiguration? menuConfiguration;
    private readonly List<Label> menuMetricValues=[];
    private readonly List<Label> menuStatusChips=[];
    private DashboardOccupancyCard? menuOccupancyCard;
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
        functions.RemoveAll(x=>x.ActionCode=="room.map");
        var preferred=bookings.FirstOrDefault(x=>x.ActionCode=="room.walkin")??bookings[0];
        var first=bookings.OrderBy(x=>x.SubOrder).ThenBy(x=>x.Order).First();
        functions.RemoveAll(x=>x.ActionCode is "room.walkin" or "room.reserve");
        functions.Add(new VisibleFunction("Sơ đồ & đặt phòng",first.Group,preferred.ActionCode,first.SubOrder,first.Order));
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
                finance.Add(new VisibleFunction("Hóa đơn","Tài chính","invoice.list",0,0));
            if(FunctionPolicy.Can(user,"report.revenue"))
                finance.Add(new VisibleFunction("Doanh thu","Tài chính","report.revenue",0,1));
            if(FunctionPolicy.Can(user,"cash.flow"))
                finance.Add(new VisibleFunction("Thu chi","Tài chính","cash.flow",0,2));
            if(FunctionPolicy.Can(user,"cash.book"))
                finance.Add(new VisibleFunction("Sổ quỹ","Tài chính","cash.book",0,3));
            if(FunctionPolicy.Can(user,"cash.debt"))
                finance.Add(new VisibleFunction("Công nợ","Tài chính","cash.debt",0,4));
            if(FunctionPolicy.Can(user,"report.profit"))
                finance.Add(new VisibleFunction("Báo cáo","Tài chính","report.profit",0,5));
            if(menuConfiguration is not null)
                finance.AddRange(from sub in menuConfiguration.Submenus
                    join function in menuConfiguration.CustomFunctions on sub.Id equals function.SubmenuId
                    where sub.MenuCode==menu && sub.Active && function.Active &&
                          function.AllowedRoles.Split(',').Contains(user.Role)
                    select new VisibleFunction(function.Title,sub.Title,$"custom:{function.Id}",sub.Order,function.Order));
            return finance;
        }
        if(menuConfiguration is null)
            return MergeServiceInventoryMenu(MergeBookingMenu(FunctionPolicy.All.Where(x=>((menu=="rooms" && x.Code is ("invoice.create" or "invoice.list")) || (x.Code is not ("invoice.create" or "invoice.list") && (x.Menu==menu || (menu=="finance" && x.Menu is ("cash" or "invoices" or "reports")) ||
                (menu=="staff" && x.Menu=="system")))) &&
                (FunctionPolicy.Can(user,x.Code) || (x.Code=="room.schedule" && CanOpenSchedule())))
                .Select(x=>new VisibleFunction(x.Name,x.Submenu,x.Code)).ToList(),menu),menu);
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
        return MergeServiceInventoryMenu(MergeBookingMenu(functions,menu),menu);
    }
    private List<VisibleFunction> MergeServiceInventoryMenu(List<VisibleFunction> functions,string menu)
    {
        if(menu=="services" && (FunctionPolicy.Can(user,"service.order") || FunctionPolicy.Can(user,"service.manage") ||
            FunctionPolicy.Can(user,"service.catalog") || FunctionPolicy.Can(user,"service.stock")))
        {
            if(!functions.Any(x=>x.ActionCode=="service.inventory"))
                functions.Insert(Math.Min(2,functions.Count),new VisibleFunction("Tồn kho dịch vụ","Kho dịch vụ","service.inventory",2,0));
            var stockIndex=functions.FindIndex(x=>x.ActionCode=="service.stock");
            if(stockIndex>=0)functions[stockIndex]=functions[stockIndex] with {Title="Nhập / xuất kho minibar"};
        }
        return functions;
    }
    private async Task ReloadMenuConfiguration()
    {
        try
        {
            var config = await service.MenuConfigurationAsync();
            menuConfiguration = config.Menus.Count > 0 ? config : null;
        }
        catch (Exception ex)
        {
            Ui.Log(ex);
            menuConfiguration = null;
        }
        if(navigationReady)BuildNavigationSidebar();
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
    private static Color WarmMenuAccent(MainMenuItem item)=>item.Code switch
    {
        "rooms"=>AppTheme.Blue,
        "finance"=>AppTheme.Amber,
        "services" or "shifts"=>AppTheme.Amber,
        "customers"=>AppTheme.Teal,
        "staff"=>Color.FromArgb(80,105,94),
        _=>item.Accent
    };
    private static string FunctionIcon(string code) => code.StartsWith("custom:",StringComparison.Ordinal)?"folder":code switch
    {
        "room.search" => "search", "room.reserve" or "room.schedule" or "room.history" => "calendar",
        "room.deposit" or "room.pricing" or "cash.flow" or "cash.book" or "cash.debt" => "money",
        "room.checkout" or "invoice.create" or "invoice.list" or "invoice.control" or "invoice.groups" => "receipt",
        "finance.workspace" => "money",
        "room.transfer" => "swap", "room.extend" or "shift.manage" or "system.audit" => "clock",
        "room.maintain" or "room.booking_edit" or "room.booking_cancel" => "tools",
        "service.order" or "service.manage" or "service.catalog" or "service.stock" or "service.inventory" => "service",
        "customer.profile" or "customer.history" or "staff.view" or "staff.manage" => "people",
        "report.revenue" or "report.profit" => "chart", "system.password" => "key",
        _ => "bed"
    };
    private static string FunctionTileDescription(string code)=>code switch
    {
        "room.map"=>"Xem trạng thái phòng theo tầng và mở nhanh từng phòng.",
        "room.reserve" or "room.walkin"=>"Tạo đặt phòng mới và hỗ trợ nhận phòng nhanh.",
        "room.schedule"=>"Theo dõi khách đến, khách đi và lịch lưu trú.",
        "invoice.create" or "room.checkout"=>"Kiểm tra bill, thu tiền và hoàn tất trả phòng.",
        "invoice.list"=>"Tra cứu hóa đơn và các khoản đã thanh toán.",
        "finance.workspace"=>"Quản lý sổ quỹ, công nợ và nghiệp vụ kế toán.",
        "cash.flow"=>"Theo dõi các khoản tiền thu và hoàn trả.",
        "cash.book"=>"Kiểm soát số dư và các phiếu thu chi.",
        "cash.debt"=>"Quản lý công nợ khách hàng và đối tác.",
        "report.profit"=>"Xem lãi lỗ và các chỉ số tài chính.",
        "report.revenue"=>"Xem biểu đồ và xu hướng doanh thu.",
        "service.order"=>"Tạo yêu cầu dịch vụ cho khách đang lưu trú.",
        "service.inventory"=>"Xem hàng còn bán, đang chờ giao và mặt hàng sắp hết.",
        "service.stock"=>"Nhập hàng và ghi nhận các biến động kho minibar.",
        "customer.profile"=>"Tra cứu thông tin và lịch sử của khách.",
        "staff.manage"=>"Quản lý tài khoản, hồ sơ và phân quyền.",
        _=>"Mở chức năng và xử lý nghiệp vụ liên quan."
    };
    private static Color FunctionTileAccent(string module,int index)=>module=="finance"?(index%6) switch
    {
        0=>Color.FromArgb(44,112,74),1=>Color.FromArgb(195,95,70),
        2=>Color.FromArgb(181,126,51),3=>Color.FromArgb(50,114,82),
        4=>Color.FromArgb(49,119,171),_=>Color.FromArgb(105,84,161)
    }:(index%4) switch
    {
        0=>AppTheme.Blue,1=>AppTheme.Amber,2=>AppTheme.Teal,_=>Color.FromArgb(87,112,151)
    };

    private readonly Panel navigationContent = new() { Dock = DockStyle.Fill, Margin=Padding.Empty };
    private readonly Panel navigationSidebar = new() { Dock = DockStyle.Left, Width=242, BackColor=AppTheme.Navy };
    private DashboardSidebarScene? sidebarScene;
    private int sidebarSceneTop;
    private readonly Dictionary<string,DashboardSidebarButton> sidebarButtons=new();
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
        navigationContent.Controls.Add(navigationSidebar);
        navigationSidebar.BringToFront();
        navigationSidebar.Resize+=(_,_)=>LayoutSidebarScene();
        BuildNavigationSidebar();
        tlpBody.Controls.Add(navigationContent, 1, 0);
        navigationReady = true;
        ShowMainMenu();
        ResumeLayout(true);
    }

    private void BuildNavigationSidebar()
    {
        foreach(Control child in navigationSidebar.Controls.Cast<Control>().ToArray())child.Dispose();
        sidebarButtons.Clear();
        navigationSidebar.Controls.Add(new Label {Text="KHÔNG GIAN LÀM VIỆC",Font=AppTheme.TextFont(8.5F,FontStyle.Bold),
            ForeColor=Color.FromArgb(197,173,119),Location=new Point(18,22),Size=new Size(216,24)});
        var entries=new[]{(Code:"menu",Title:"Tổng quan",Icon:"building")}
            .Concat(VisibleMenus().Where(x=>user.Role=="Admin"||MenuFunctions(x.Code).Count>0)
                .Select(x=>(Code:x.Code,Title:x.Title,Icon:MenuIcon(x.Code))));
        var top=58;
        foreach(var entry in entries)
        {
            var code=entry.Code;
            var button=new DashboardSidebarButton {Text=entry.Title,IconKind=entry.Icon,
                Location=new Point(12,top),Size=new Size(navigationSidebar.Width-24,44),Anchor=AnchorStyles.Left|AnchorStyles.Right|AnchorStyles.Top};
            button.Click+=(_,_)=>{if(code=="menu")ShowMainMenu();else ShowSubmenu(code);};
            navigationSidebar.Controls.Add(button);sidebarButtons[code]=button;top+=51;
        }
        sidebarSceneTop=top+12;
        sidebarScene=new DashboardSidebarScene {Dock=DockStyle.Bottom};
        navigationSidebar.Controls.Add(sidebarScene);
        LayoutSidebarScene();
        UpdateSidebarSelection();
    }
    private void LayoutSidebarScene()
    {
        if(sidebarScene is {IsDisposed:false})
            sidebarScene.Height=Math.Max(0,navigationSidebar.ClientSize.Height-sidebarSceneTop);
    }
    private void UpdateSidebarSelection()
    {
        foreach(var (code,button) in sidebarButtons)button.Selected=code==activeModule;
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
        menuStatusChips.Clear();
        menuOccupancyCard=null;
        var items = VisibleMenus().Where(x => user.Role=="Admin" || MenuFunctions(x.Code).Count>0).ToArray();
        var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = AppTheme.Canvas };
        var content = new Panel { BackColor = AppTheme.Canvas };
        scroll.Controls.Add(content);

        var heading = new Panel { Location = new Point(24, 0), Height = 118, BackColor = AppTheme.Canvas };
        content.Controls.Add(heading);
        heading.Controls.Add(new Panel { Location = new Point(2, 8), Size = new Size(42, 3), BackColor = AppTheme.Focus });
        var title = new Label
        {
            Text = "Tổng quan hoạt động", Font = AppTheme.TextFont(24F,FontStyle.Bold),
            ForeColor = AppTheme.Navy, Location = new Point(0, 20), Height = 53, AutoEllipsis = true
        };
        var introduction = new Label
        {
            Text = "Quản lý phòng, khách và tài chính trong một không gian làm việc", Font = AppTheme.TextFont(10F),
            ForeColor = AppTheme.Muted, Location = new Point(3, 74), Height = 30, AutoEllipsis = true
        };
        heading.Controls.Add(title);
        heading.Controls.Add(introduction);
        var headerActions=new FlowLayoutPanel {Size=new Size(620,46),WrapContents=false,BackColor=AppTheme.Canvas};
        heading.Controls.Add(headerActions);
        foreach(var (code,label) in new[]
        {
            (FunctionPolicy.Can(user,"room.walkin")?"room.walkin":
                FunctionPolicy.Can(user,"room.reserve")?"room.reserve":"room.map","Sơ đồ & đặt phòng  ↗"),
            ("shift.manage","Ca trực & trạng thái phòng  ↗")
        })
        {
            if(!FunctionPolicy.Can(user,code))continue;
            var action=new RoundedActionButton {Text=label,Size=new Size(220,42),Margin=new Padding(0,0,10,0)};
            action.Click+=async (_,_)=>await OpenFunction(code);
            headerActions.Controls.Add(action);
        }
        var footerActions=new Panel {Height=82,BackColor=AppTheme.Canvas};
        content.Controls.Add(footerActions);
        footerActions.Controls.Add(new Label {Text="Tình trạng phòng hôm nay",Font=AppTheme.Bold,
            ForeColor=AppTheme.Navy,Location=new Point(0,0),Size=new Size(280,26)});
        var statusFlow=new FlowLayoutPanel {Location=new Point(0,32),Height=38,WrapContents=false,
            AutoScroll=true,BackColor=AppTheme.Canvas};footerActions.Controls.Add(statusFlow);
        foreach(var (caption,count,color,tint) in new[]
        {
            ("Tất cả",data.Rooms.Count,AppTheme.Blue,Color.FromArgb(226,239,229)),
            ("Đang ở",data.Rooms.Count(x=>x.Status==RoomStatus.DangO),AppTheme.Blue,Color.FromArgb(226,239,229)),
            ("Trống",data.Rooms.Count(x=>x.Status==RoomStatus.Trong),AppTheme.Teal,Color.FromArgb(231,243,229)),
            ("Đặt trước",data.Stays.Count(x=>x.Status==StayStatus.Reserved),AppTheme.Amber,Color.FromArgb(249,239,223)),
            ("Bảo trì",data.Rooms.Count(x=>x.Status==RoomStatus.BaoTri),AppTheme.Danger,Color.FromArgb(249,232,227))
        })
        {
            var chip=new Label {Text=$"{caption} ({count})",Size=new Size(caption=="Đặt trước"?132:116,30),
                BackColor=tint,ForeColor=color,Font=AppTheme.Small,TextAlign=ContentAlignment.MiddleCenter,
                Margin=new Padding(0,0,8,0)};
            using(var chipShape=AppTheme.Rounded(new RectangleF(0,0,chip.Width,chip.Height),15))
                chip.Region=new Region(chipShape);
            statusFlow.Controls.Add(chip);menuStatusChips.Add(chip);
        }

        var showMetrics=RolePolicy.CanViewOperations(user.Role);
        var metricStrip=new TableLayoutPanel {Height=96,ColumnCount=4,RowCount=1,BackColor=AppTheme.Canvas};
        for(var i=0;i<4;i++)metricStrip.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,25));
        if(showMetrics)
        {
            menuOccupancyCard=new DashboardOccupancyCard
            {
                Dock=DockStyle.Fill,Occupied=data.Rooms.Count(x=>x.Status==RoomStatus.DangO),
                Total=data.Rooms.Count,Margin=new Padding(0,0,6,0)
            };
            metricStrip.Controls.Add(menuOccupancyCard,0,0);
            var values=new[]
            {
                ("Phòng đang ở",data.Rooms.Count(x=>x.Status==RoomStatus.DangO),AppTheme.Blue),
                ("Phòng trống",data.Rooms.Count(x=>x.Status==RoomStatus.Trong),AppTheme.Teal),
                ("Phòng bảo trì",data.Rooms.Count(x=>x.Status==RoomStatus.BaoTri),AppTheme.Danger)
            };
            for(var i=0;i<values.Length;i++)
            {
                var stat=values[i];
                var card=new DashboardMetricCard {Dock=DockStyle.Fill,BackColor=Color.White,Accent=stat.Item3,Margin=new Padding(6,0,i==2?0:6,0)};
                var value=new Label {Text=stat.Item2.ToString(),Font=AppTheme.TextFont(19F,FontStyle.Bold),
                    ForeColor=stat.Item3,Location=new Point(18,8),Size=new Size(100,36)};
                card.Controls.Add(value);menuMetricValues.Add(value);
                card.Controls.Add(new Label {Text=stat.Item1,Font=AppTheme.Small,ForeColor=AppTheme.Muted,
                    Location=new Point(19,51),AutoSize=true});
                metricStrip.Controls.Add(card,i+1,0);
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
                Accent = WarmMenuAccent(item),
                Featured=item.Code=="finance",
                IconKind = configuredIcon is null or "people" or "clock" or "money"
                    ? MenuIcon(item.Code) : configuredIcon,
                IconPng = menuConfiguration?.Menus.FirstOrDefault(x=>x.Code==item.Code)?.IconPng, Height = 185,
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
            headerActions.Location=new Point(Math.Max(24,innerWidth-headerActions.Width),34);
            headerActions.Visible=innerWidth>=850;
            title.Width=innerWidth>=850?Math.Max(250,innerWidth-headerActions.Width-18):innerWidth;
            var columns = innerWidth >= 950 ? 3 : innerWidth >= 590 ? 2 : 1;
            const int gap = 16;
            var cardWidth = (innerWidth - gap * (columns - 1)) / columns;
            var cardTop=showMetrics?238:128;
            var rows=(cards.Count + columns - 1) / columns;
            var baseCardHeight=columns==3?158:170;
            var availableHeight=Math.Max(0,scroll.ClientSize.Height-cardTop-110-Math.Max(0,rows-1)*gap);
            var cardHeight=rows>0?Math.Clamp(availableHeight/rows,baseCardHeight,220):baseCardHeight;
            var rowStep=cardHeight+gap;
            for (var i = 0; i < cards.Count; i++)
            {
                cards[i].Bounds = new Rectangle(24 + (i % columns) * (cardWidth + gap),
                    cardTop + (i / columns) * rowStep, cardWidth, cardHeight);
            }
            var footerTop=cardTop+rows*cardHeight+Math.Max(0,rows-1)*gap+16;
            footerActions.Location=new Point(24,footerTop);
            footerActions.Width=innerWidth;
            statusFlow.Width=innerWidth;
            if(showMetrics)
            {
                metricStrip.Location=new Point(24,122);
                metricStrip.Width=innerWidth;
                metricStrip.Height=92;
            }
            content.Height=footerTop+92;
        }
        scroll.Resize += (_, _) => LayoutMenu();
        menuPage.Controls.Add(scroll);
        LayoutMenu();
    }
    private void UpdateMenuMetrics()
    {
        if(menuOccupancyCard is {IsDisposed:false})
        {
            menuOccupancyCard.Occupied=data.Rooms.Count(x=>x.Status==RoomStatus.DangO);
            menuOccupancyCard.Total=data.Rooms.Count;
        }
        if(menuMetricValues.Count!=3 || menuMetricValues.Any(x=>x.IsDisposed))return;
        var values=new[] {data.Rooms.Count(x=>x.Status==RoomStatus.DangO),
            data.Rooms.Count(x=>x.Status==RoomStatus.Trong),data.Rooms.Count(x=>x.Status==RoomStatus.BaoTri)};
        for(var i=0;i<3;i++)menuMetricValues[i].Text=values[i].ToString();
        if(menuStatusChips.Count==5 && menuStatusChips.All(x=>!x.IsDisposed))
        {
            var counts=new[]{data.Rooms.Count,data.Rooms.Count(x=>x.Status==RoomStatus.DangO),
                data.Rooms.Count(x=>x.Status==RoomStatus.Trong),data.Stays.Count(x=>x.Status==StayStatus.Reserved),
                data.Rooms.Count(x=>x.Status==RoomStatus.BaoTri)};
            var labels=new[]{"Tất cả","Đang ở","Trống","Đặt trước","Bảo trì"};
            for(var i=0;i<5;i++)menuStatusChips[i].Text=$"{labels[i]} ({counts[i]})";
        }
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
        var heading = PageHeading(menu.Title, menu.Code=="finance"
            ? "Quản lý doanh thu, công nợ, thu chi và các báo cáo tài chính."
            : menu.Description);
        AddBackButton(heading, "← Menu tổng", ShowMainMenu);
        var functions=MenuFunctions(module).ToArray();
        var scroll=new Panel {Dock=DockStyle.Fill,AutoScroll=true,BackColor=AppTheme.Canvas};
        var content=new Panel {BackColor=AppTheme.Canvas};scroll.Controls.Add(content);
        if(functions.Length==0)
            content.Controls.Add(new Label {Text="Menu này chưa có chức năng trong bản 7 bảng.",
                Location=new Point(24,24),AutoSize=true,Font=AppTheme.Body,ForeColor=AppTheme.Muted});
        var tiles=new List<DashboardFunctionButton>();
        for(var i=0;i<functions.Length;i++)
        {
            var function=functions[i];
            var tile=new DashboardFunctionButton {Text=function.Title,
                IconKind=FunctionIcon(function.ActionCode),
                Description=FunctionTileDescription(function.ActionCode),
                Accent=FunctionTileAccent(module,i),AccessibleName=function.Title,UseMnemonic=false};
            tile.Click+=async (_,_)=>await OpenFunction(function.ActionCode);
            content.Controls.Add(tile);tiles.Add(tile);
        }
        void LayoutTiles()
        {
            var width=Math.Max(560,scroll.ClientSize.Width);
            content.Width=width;
            var inner=width-48;
            var columns=inner>=1290?3:inner>=580?2:1;
            const int gap=20;
            var tileWidth=(inner-gap*(columns-1))/columns;
            var tileHeight=module=="finance"?150:142;
            for(var i=0;i<tiles.Count;i++)
                tiles[i].Bounds=new Rectangle(24+(i%columns)*(tileWidth+gap),
                    18+(i/columns)*(tileHeight+gap),tileWidth,tileHeight);
            var rows=(tiles.Count+columns-1)/columns;
            content.Height=20+rows*(tileHeight+gap)+20;
            scroll.AutoScrollMinSize=new Size(0,content.Height);
        }
        scroll.Resize+=(_,_)=>LayoutTiles();
        modulePage.Controls.Add(scroll);
        modulePage.Controls.Add(heading);
        LayoutTiles();
        ActivatePage(modulePage);
        UpdateNavigationTitle();
    }

    private async Task OpenFunction(string code)
    {
        if(code=="system.password")
        {
            await Run(ShowMyAccountWorkspace);
            return;
        }
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
        else if(code=="service.inventory")
        {
            if(!(FunctionPolicy.Can(user,"service.order") || FunctionPolicy.Can(user,"service.manage") ||
                FunctionPolicy.Can(user,"service.catalog") || FunctionPolicy.Can(user,"service.stock")))
                throw new BusinessException("Bạn không được cấp quyền sử dụng chức năng này.");
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
            case "service.inventory": await Run(ShowServiceInventory); break;
            case "service.stock": await Run(() => OpenAccounting("Kho minibar")); break;
            case "customer.profile": InvokeLegacy(btnQuanLyKhach_Click); break;
            case "customer.history": await Run(ShowStayHistory); break;
            case "shift.manage": await Run(ShowShiftManagement); break;
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
            case "system.audit": await Run(ShowAudit); break;
        }
        if(code is not ("room.checkout" or "invoice.create") || !functionPage.Visible)
            activeFunction = null;
        UpdateNavigationTitle();
    }

    private void ShowEmbeddedFunction(string code)
    {
        if(code=="room.map" && (FunctionPolicy.Can(user,"room.reserve") || FunctionPolicy.Can(user,"room.walkin")))
        {
            var reserve=!FunctionPolicy.Can(user,"room.walkin") && FunctionPolicy.Can(user,"room.reserve");
            activeFunction=reserve?"room.reserve":"room.walkin";
            ShowBookingScreen(reserve);
            return;
        }
        ClearPage(functionPage);
        var heading = FunctionHeading(code);
        if (code == "room.map")
        {
            functionPage.Controls.Add(BuildRoomOverview());
        }
        else if (code == "report.revenue")
        {
            functionPage.Controls.Add(BuildRevenueWorkspace());
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

    private Panel FunctionHeading(string code, string detail = "Màn hình chức năng",string? titleOverride=null)
    {
        var selected = FunctionPolicy.All.Single(x => x.Code == code);
        var heading = PageHeading(titleOverride??selected.Name, detail);
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

    private async Task ShowShiftManagement()
    {
        using var form = new FormCaTruc(service, user);
        form.ShowDialog(this);
        await Reload();
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
        UpdateSidebarSelection();
        var title = activeFunction is not null
            ? FunctionPolicy.All.FirstOrDefault(x => x.Code == activeFunction)?.Name ?? "Chức năng"
            : activeModule == "menu" ? "Menu tổng" : VisibleMenus().FirstOrDefault(x => x.Code == activeModule)?.Title ?? "Menu";
        lblHeaderTitle.Text = "HOTEL DESK";
        lblHeaderSubtitle.Text = "QUẢN LÝ KHÁCH SẠN";
    }
}
