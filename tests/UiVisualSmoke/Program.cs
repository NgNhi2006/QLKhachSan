using System.Drawing.Imaging;
using System.Reflection;
using Microsoft.Data.SqlClient;
using QLKhachSan.BLL;
using QLKhachSan.DTO;
using QLKhachSan.GUI;
using QLKhachSan.DAL;

if(args.Length==2 && args[0]=="--payment-preview")
{
    Application.EnableVisualStyles();
    Application.SetCompatibleTextRenderingDefault(false);
    Directory.CreateDirectory(args[1]);
    var previewAssembly=typeof(FormMain).Assembly;
    var previewType=previewAssembly.GetType("QLKhachSan.GUI.InputDialog")!;
    using var paymentDialog=(Form)Activator.CreateInstance(previewType,"Thanh toán P.205",900,760,false)!;
    var previewAdd=previewType.GetMethod("Add")!;
    var surface=(TableLayoutPanel)Activator.CreateInstance(previewAssembly.GetType("QLKhachSan.GUI.RoundedSurface")!)!;
    surface.Height=300;surface.ColumnCount=1;surface.RowCount=3;surface.Padding=new Padding(18);
    surface.RowStyles.Add(new RowStyle(SizeType.Absolute,48));
    surface.RowStyles.Add(new RowStyle(SizeType.Percent,100));
    surface.RowStyles.Add(new RowStyle(SizeType.Absolute,66));
    surface.Controls.Add(new Label {Text="PHIẾU THANH TOÁN  •  P.205",Dock=DockStyle.Fill,
        Font=new Font("Segoe UI",17,FontStyle.Bold)},0,0);
    var list=new ListView {Dock=DockStyle.Fill,View=View.Details,BorderStyle=BorderStyle.None,FullRowSelect=true};
    list.Columns.Add("Hạng mục",300);list.Columns.Add("Thành tiền",160);
    var row=new ListViewItem("Tiền phòng");row.SubItems.Add("1.200.000 đ");list.Items.Add(row);
    surface.Controls.Add(list,0,1);
    surface.Controls.Add(new Label {Text="CẦN THU: 1.200.000 đ",Dock=DockStyle.Fill,
        TextAlign=ContentAlignment.MiddleRight,Font=new Font("Segoe UI",14,FontStyle.Bold)},0,2);
    previewAdd.Invoke(paymentDialog,["",surface,300]);
    var method=new ComboBox {DropDownStyle=ComboBoxStyle.DropDownList};
    method.Items.AddRange(["Tiền mặt","Chuyển khoản","Thẻ POS"]);method.SelectedIndex=0;
    previewAdd.Invoke(paymentDialog,["Phương thức thanh toán",method,0]);
    previewType.GetMethod("Action")!.Invoke(paymentDialog,["HOÀN TẤT THANH TOÁN",(Func<Task>)(()=>Task.CompletedTask),true]);
    paymentDialog.Show();Application.DoEvents();
    using var bitmap=new Bitmap(paymentDialog.Width,paymentDialog.Height);
    paymentDialog.DrawToBitmap(bitmap,new Rectangle(Point.Empty,paymentDialog.Size));
    var path=Path.Combine(args[1],"payment-preview.png");bitmap.Save(path,ImageFormat.Png);
    Console.WriteLine(path);paymentDialog.Close();return;
}

if(args.Length==2 && args[0]=="--migrate" &&
    new SqlConnectionStringBuilder(args[1]).InitialCatalog.Contains("_Verify_",StringComparison.Ordinal))
{
    await SchemaMigrator.EnsureAsync(args[1]);
    Console.WriteLine("Verify schema migration passed.");
    return;
}
if(args.Length==2 && args[0]=="--profile-smoke" &&
    new SqlConnectionStringBuilder(args[1]).InitialCatalog.Contains("_Verify_",StringComparison.Ordinal))
{
    var service=new AuthService(new HotelRepository(args[1]));
    var actor=new UserSession(3,"admin","Admin")
    {
        SecurityVersion=2,GrantedFunctions=FunctionPolicy.Defaults("Admin")
    };
    var original=await service.ProfileAsync(actor,4);
    using var bitmap=new Bitmap(80,80);
    using(var g=Graphics.FromImage(bitmap))g.Clear(Color.CornflowerBlue);
    using var imageBytes=new MemoryStream();bitmap.Save(imageBytes,ImageFormat.Png);
    try
    {
        await service.SaveProfileAsync(actor,original,"Nguyễn Văn A",imageBytes.ToArray());
        var saved=await service.ProfileAsync(actor,4);
        if(saved.DisplayName!="Nguyễn Văn A" || saved.AvatarPng is not {Length:>0})
            throw new InvalidOperationException("Staff profile was not persisted.");
        Console.WriteLine("Staff name and avatar persistence passed.");
    }
    finally {await service.SaveProfileAsync(actor,original,original.DisplayName,original.AvatarPng);}
    return;
}
if(args.Length==3 && args[0]=="--profile-header" &&
    new SqlConnectionStringBuilder(args[1]).InitialCatalog.Contains("_Verify_",StringComparison.Ordinal))
{
    Application.EnableVisualStyles();
    Application.SetCompatibleTextRenderingDefault(false);
    Directory.CreateDirectory(args[2]);
    var actorProfile=new UserSession(3,"admin","Admin")
    {
        SecurityVersion=2,GrantedFunctions=FunctionPolicy.Defaults("Admin")
    };
    var profileService=new AuthService(new HotelRepository(args[1]));
    var previous=await profileService.ProfileAsync(actorProfile,actorProfile.Id);
    using var portraitTest=new Bitmap(100,100);
    using(var paint=Graphics.FromImage(portraitTest))
    {
        paint.Clear(Color.FromArgb(49,100,203));
        paint.FillEllipse(Brushes.White,25,15,50,50);
    }
    using var portraitBytes=new MemoryStream();portraitTest.Save(portraitBytes,ImageFormat.Png);
    try
    {
        await profileService.SaveProfileAsync(actorProfile,previous,"Nguyễn Văn A",portraitBytes.ToArray());
        using var headerForm=new FormMain(actorProfile);
        var headerDashboard=headerForm.Controls.OfType<ucDashboard>().Single();
        typeof(ucDashboard).GetField("loaded",BindingFlags.Instance|BindingFlags.NonPublic)!
            .SetValue(headerDashboard,true);
        headerForm.Show();headerForm.WindowState=FormWindowState.Normal;
        headerForm.SetBounds(0,0,1100,700);Application.DoEvents();
        using var headerBitmap=new Bitmap(headerForm.Width,headerForm.Height);
        headerForm.DrawToBitmap(headerBitmap,new Rectangle(Point.Empty,headerForm.Size));
        var headerPath=Path.Combine(args[2],"named-avatar-header.png");
        headerBitmap.Save(headerPath,ImageFormat.Png);Console.WriteLine(headerPath);
        headerForm.Close();
    }
    finally {await profileService.SaveProfileAsync(actorProfile,previous,previous.DisplayName,previous.AvatarPng);}
    return;
}

if(args.Length==2 && args[0]=="--offline")
{
    Application.EnableVisualStyles();
    Application.SetCompatibleTextRenderingDefault(false);
    Directory.CreateDirectory(args[1]);
    var previewUser=new UserSession(1,"admin","Admin") {GrantedFunctions=FunctionPolicy.Defaults("Admin")};
    using var preview=new FormMain(previewUser);
    var previewDashboard=preview.Controls.OfType<ucDashboard>().Single();
    typeof(ucDashboard).GetField("loaded",BindingFlags.Instance|BindingFlags.NonPublic)!.SetValue(previewDashboard,true);
    preview.Show();
    foreach(var (width,height) in new[]{(1366,768),(1920,1080)})
    {
        preview.WindowState=FormWindowState.Normal;
        preview.SetBounds(0,0,width,height);
        foreach(var (name,method,argument) in new[]{("menu","ShowMainMenu",""),("rooms-menu","ShowSubmenu","rooms"),("room-map","ShowEmbeddedFunction","room.map")})
        {
            typeof(ucDashboard).GetMethod(method,BindingFlags.Instance|BindingFlags.NonPublic)!
                .Invoke(previewDashboard,method=="ShowMainMenu"?[]:[argument]);
            Cursor.Position=Point.Empty;
            Application.DoEvents();
            using var image=new Bitmap(preview.Width,preview.Height);
            preview.DrawToBitmap(image,new Rectangle(Point.Empty,preview.Size));
            var file=Path.Combine(args[1],$"{name}-offline-{width}x{height}.png");
            image.Save(file,ImageFormat.Png);
            Console.WriteLine(file);
        }
    }
    preview.Close();
    return;
}
if(args.Length is not (4 or 6 or 7) || !new SqlConnectionStringBuilder(args[0]).InitialCatalog.Contains("_Verify_",StringComparison.Ordinal))
    throw new InvalidOperationException("Pass a dedicated _Verify_ connection string, account ID, security version, output directory, and optionally role and username.");
Environment.SetEnvironmentVariable("QLKHACHSAN_CONNECTION_STRING",args[0]);
Application.EnableVisualStyles();
Application.SetCompatibleTextRenderingDefault(false);
var role=args.Length>=6?args[4]:"Admin";
var username=args.Length>=6?args[5]:"ui_preview";
var user=new UserSession(int.Parse(args[1]),username,role)
{
    SecurityVersion=long.Parse(args[2]),GrantedFunctions=FunctionPolicy.Defaults(role)
};
Directory.CreateDirectory(args[3]);
using var form=new FormMain(user);
var theme=typeof(FormMain).Assembly.GetType("QLKhachSan.GUI.AppTheme")!;
var font=(Font)theme.GetField("Body",BindingFlags.Static|BindingFlags.Public)!.GetValue(null)!;
Console.WriteLine($"UI font family: {font.FontFamily.Name}");
var dashboard=form.Controls.OfType<ucDashboard>().Single();
typeof(ucDashboard).GetField("loaded",BindingFlags.Instance|BindingFlags.NonPublic)!.SetValue(dashboard,true);
form.Show();
var reload=(Task)typeof(ucDashboard).GetMethod("Reload",BindingFlags.Instance|BindingFlags.NonPublic)!
    .Invoke(dashboard,[])!;
for(var until=DateTime.UtcNow.AddSeconds(20);!reload.IsCompleted && DateTime.UtcNow<until;)
{
    Application.DoEvents();
    Thread.Sleep(25);
}
if(!reload.IsCompleted)throw new TimeoutException("Dashboard reload did not complete within 20 seconds.");
reload.GetAwaiter().GetResult();
var screens=new (string Name,string? Method,string? Argument)[]
{
    ("menu","ShowMainMenu",null),("rooms-menu","ShowSubmenu","rooms"),
    ("room-map","ShowEmbeddedFunction","room.map"),
    ("booking","ShowBookingScreen","true"),
    ("finance-menu","ShowSubmenu","finance")
};
if(role!="Admin")screens=screens.Where(x=>x.Name is "menu" or "rooms-menu").ToArray();
if(args.Length==7)screens=screens.Where(x=>x.Name is "menu" or "rooms-menu" or "room-map" or "finance-menu").ToArray();
foreach(var (width,height) in args.Length==7?new[]{(1100,700),(1366,768),(1920,1080)}:new[]{(1366,768),(1920,1080)})
{
    form.WindowState=FormWindowState.Normal;
    form.SetBounds(0,0,width,height);
    foreach(var (name,method,argument) in screens)
    {
        if(method is not null)
        {
            var target=typeof(ucDashboard).GetMethod(method,BindingFlags.Instance|BindingFlags.NonPublic)
                ?? throw new MissingMethodException(method);
            target.Invoke(dashboard,method=="ShowMainMenu"?[]:
                new object[]{method=="ShowBookingScreen"?bool.Parse(argument!):argument!});
        }
        Application.DoEvents();
        if(name=="rooms-menu" && width==1100)
        {
            var page=(Control)typeof(ucDashboard).GetField("modulePage",BindingFlags.Instance|BindingFlags.NonPublic)!.GetValue(dashboard)!;
            var scroll=page.Controls.OfType<Panel>().Single(x=>x.AutoScroll);
            if(scroll.HorizontalScroll.Visible)
                throw new InvalidOperationException("Module menu has an unwanted horizontal scrollbar at 1100 px.");
        }
        using var bitmap=new Bitmap(form.Width,form.Height);
        form.DrawToBitmap(bitmap,new Rectangle(Point.Empty,form.Size));
        var path=Path.Combine(args[3],$"{name}-{width}x{height}.png");
        bitmap.Save(path,ImageFormat.Png);
        Console.WriteLine(path);
    }
}
if(args.Length==7 && args[6]=="profile-preview")
{
    var captured=false;
    using var watch=new System.Windows.Forms.Timer {Interval=100};
    watch.Tick+=(_,_)=>
    {
        var opened=Application.OpenForms.Cast<Form>().FirstOrDefault(x=>x.Text=="Tài khoản cá nhân và phân quyền");
        if(opened is null)return;
        watch.Stop();
        var pages=opened.Controls.OfType<TableLayoutPanel>().Single().Controls.OfType<TabControl>().Single();
        foreach(var index in new[]{0,pages.TabPages.Count-1})
        {
            pages.SelectedIndex=index;
            Application.DoEvents();
            using var bitmap=new Bitmap(opened.Width,opened.Height);
            opened.DrawToBitmap(bitmap,new Rectangle(Point.Empty,opened.Size));
            var path=Path.Combine(args[3],$"account-tab-{index}.png");
            bitmap.Save(path,ImageFormat.Png);Console.WriteLine(path);
        }
        captured=true;opened.Close();
    };
    watch.Start();
    var task=(Task)typeof(ucDashboard).GetMethod("ShowAccounts",BindingFlags.Instance|BindingFlags.NonPublic)!
        .Invoke(dashboard,[])!;
    var deadline=DateTime.UtcNow.AddSeconds(12);
    while(!task.IsCompleted && DateTime.UtcNow<deadline){Application.DoEvents();Thread.Sleep(25);}
    watch.Stop();
    if(!captured)throw new TimeoutException("Account screen did not open.");
    task.GetAwaiter().GetResult();
    captured=false;
    var current=new AuthService(new HotelRepository(args[0])).ProfileAsync(user,user.Id)
        .GetAwaiter().GetResult();
    using var editorWatch=new System.Windows.Forms.Timer {Interval=100};
    editorWatch.Tick+=(_,_)=>
    {
        var opened=Application.OpenForms.Cast<Form>().FirstOrDefault(x=>x.Text.StartsWith("Hồ sơ nhân viên ·",StringComparison.Ordinal));
        if(opened is null)return;
        editorWatch.Stop();Application.DoEvents();
        using var bitmap=new Bitmap(opened.Width,opened.Height);
        opened.DrawToBitmap(bitmap,new Rectangle(Point.Empty,opened.Size));
        var path=Path.Combine(args[3],"profile-editor.png");
        bitmap.Save(path,ImageFormat.Png);Console.WriteLine(path);
        captured=true;opened.Close();
    };
    editorWatch.Start();
    var editTask=(Task)typeof(ucDashboard).GetMethod("EditStaffProfile",BindingFlags.Instance|BindingFlags.NonPublic)!
        .Invoke(dashboard,[current,form])!;
    deadline=DateTime.UtcNow.AddSeconds(12);
    while(!editTask.IsCompleted && DateTime.UtcNow<deadline){Application.DoEvents();Thread.Sleep(25);}
    editorWatch.Stop();
    if(!captured)throw new TimeoutException("Profile editor did not open.");
    editTask.GetAwaiter().GetResult();form.Close();return;
}
if(args.Length==7)
{
    var functionPage=(Control)typeof(ucDashboard).GetField("functionPage",BindingFlags.Instance|BindingFlags.NonPublic)!
        .GetValue(dashboard)!;
    IEnumerable<Control> Walk(Control parent)
    {
        foreach(Control child in parent.Controls)
        {
            yield return child;
            foreach(var nested in Walk(child))yield return nested;
        }
    }
    var search=Walk(functionPage).OfType<TextBox>().Single(x=>x.PlaceholderText=="Tìm số phòng...");
    search.Text="205";
    Application.DoEvents();
    var matches=Walk(functionPage).Count(x=>x.GetType().Name=="RoomTile");
    if(matches!=1)throw new InvalidOperationException($"Expected one room tile after filtering 205, got {matches}.");
    Console.WriteLine("Room filter smoke check passed.");
    form.Close();return;
}
if(role!="Admin") {form.Close();return;}
var assembly=typeof(FormMain).Assembly;
var accountingType=assembly.GetType("QLKhachSan.GUI.AccountingForm")!;
using var accounting=(Form)Activator.CreateInstance(accountingType,user,"Sổ quỹ")!;
accounting.Show(form);
for(var until=DateTime.UtcNow.AddSeconds(3);DateTime.UtcNow<until;)
{
    Application.DoEvents();Thread.Sleep(25);
}
foreach(var (width,height) in new[]{(1366,768),(1920,1080)})
{
    accounting.MaximumSize=Size.Empty;
    accounting.MinimumSize=Size.Empty;
    accounting.SetBounds(0,0,width,height);
    Application.DoEvents();
    using var bitmap=new Bitmap(accounting.Width,accounting.Height);
    accounting.DrawToBitmap(bitmap,new Rectangle(Point.Empty,accounting.Size));
    var path=Path.Combine(args[3],$"accounting-{width}x{height}.png");
    bitmap.Save(path,ImageFormat.Png);
    Console.WriteLine(path);
}
accounting.Close();
void CaptureBusinessForm(string methodName,string windowTitle,object?[]? methodArgs=null)
{
    var foundAt=DateTime.MinValue;
    var captured=false;
    using var watch=new System.Windows.Forms.Timer {Interval=100};
    watch.Tick+=(_,_)=>
    {
        var opened=Application.OpenForms.Cast<Form>().FirstOrDefault(x=>x.Text==windowTitle);
        if(opened is null)return;
        if(foundAt==DateTime.MinValue){foundAt=DateTime.UtcNow;return;}
        if(DateTime.UtcNow-foundAt<TimeSpan.FromMilliseconds(800))return;
        watch.Stop();
        foreach(var (width,height) in new[]{(1366,768),(1920,1080)})
        {
            opened.MaximumSize=Size.Empty;opened.MinimumSize=Size.Empty;
            opened.SetBounds(0,0,width,height);
            Application.DoEvents();
            using var bitmap=new Bitmap(opened.Width,opened.Height);
            opened.DrawToBitmap(bitmap,new Rectangle(Point.Empty,opened.Size));
            var path=Path.Combine(args[3],$"{methodName}-{width}x{height}.png");
            bitmap.Save(path,ImageFormat.Png);
            Console.WriteLine(path);
        }
        captured=true;
        opened.Close();
    };
    watch.Start();
    typeof(ucDashboard).GetMethod(methodName,BindingFlags.Instance|BindingFlags.NonPublic)!
        .Invoke(dashboard,methodArgs);
    var deadline=DateTime.UtcNow.AddSeconds(18);
    while(!captured && DateTime.UtcNow<deadline)
    {
        Application.DoEvents();Thread.Sleep(25);
    }
    watch.Stop();
    if(!captured)Console.WriteLine($"UNVERIFIED {methodName}: modal did not appear in 18 seconds");
}
CaptureBusinessForm("ShowRoomCatalog","Quản lý phòng");
CaptureBusinessForm("ShowPricing","Bảng giá phòng");
CaptureBusinessForm("ShowServiceCatalog","Quản lý danh mục dịch vụ");
CaptureBusinessForm("ShowStayHistory","Lịch đặt và lịch sử lưu trú");
CaptureBusinessForm("btnQuanLyKhach_Click","Hồ sơ khách hàng",[null,EventArgs.Empty]);
CaptureBusinessForm("ShowInvoices","Hóa đơn, doanh thu và thu chi");
CaptureBusinessForm("ShowAccounts","Tài khoản cá nhân và phân quyền");
CaptureBusinessForm("ShowMenuConfiguration","Tùy chỉnh menu và chức năng");
var inputType=assembly.GetType("QLKhachSan.GUI.InputDialog")!;
using var dialog=(Form)Activator.CreateInstance(inputType,"Kiểm tra hộp thoại",620,650,false)!;
var add=inputType.GetMethod("Add")!;
foreach(var caption in new[]{"Phòng","Họ tên","Số điện thoại","Ngày đến","Số tiền","Ghi chú"})
    add.Invoke(dialog,[caption,new TextBox{Text=caption},0]);
inputType.GetMethod("Action")!.Invoke(dialog,["XÁC NHẬN",(Func<Task>)(()=>Task.CompletedTask),true]);
dialog.Show(form);
Application.DoEvents();
foreach(var (width,height) in new[]{(620,650),(620,768)})
{
    dialog.MaximumSize=Size.Empty;
    dialog.MinimumSize=Size.Empty;
    dialog.SetBounds(0,0,width,height);
    Application.DoEvents();
    using var bitmap=new Bitmap(dialog.Width,dialog.Height);
    dialog.DrawToBitmap(bitmap,new Rectangle(Point.Empty,dialog.Size));
    var path=Path.Combine(args[3],$"dialog-{width}x{height}.png");
    bitmap.Save(path,ImageFormat.Png);
    Console.WriteLine(path);
}
dialog.Close();
form.Close();
