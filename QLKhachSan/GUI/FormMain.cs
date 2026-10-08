using QLKhachSan.DTO;
using Krypton.Toolkit;

namespace QLKhachSan.GUI;

public partial class FormMain : Form
{
    private readonly ucDashboard dashboard;
    private readonly KryptonManager kryptonTheme=new() { GlobalPaletteMode=PaletteMode.Microsoft365White };
    public bool LogoutRequested { get; private set; }
    public FormMain(UserSession user)
    {
        InitializeComponent(); WindowState=FormWindowState.Maximized;MinimumSize=new Size(1100,700);
        BackColor=AppTheme.Canvas;Font=AppTheme.Body;Text="Quản lý khách sạn";
        dashboard=new ucDashboard(user) { Dock=DockStyle.Fill };
        dashboard.LogoutRequested+=(_,_)=> {LogoutRequested=true;Close();};
        Application.Idle+=StyleOpenWindows;
        FormClosing+=(_,e)=>
        {
            if(dashboard.IsBusy){e.Cancel=true;return;}
            dashboard.StopTimers();
            Application.Idle-=StyleOpenWindows;
        };
        Controls.Add(dashboard);dashboard.BringToFront();
    }
    private void StyleOpenWindows(object? sender,EventArgs e)=>WorkspaceSkin.ApplyOpenForms();
}
