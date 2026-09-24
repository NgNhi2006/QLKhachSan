using QLKhachSan.DTO;

namespace QLKhachSan.GUI;

public partial class FormMain : Form
{
    private readonly ucDashboard dashboard;
    public bool LogoutRequested { get; private set; }
    public FormMain(UserSession user)
    {
        InitializeComponent(); WindowState=FormWindowState.Maximized;MinimumSize=new Size(1100,700);
        dashboard=new ucDashboard(user) { Dock=DockStyle.Fill };
        dashboard.LogoutRequested+=(_,_)=> {LogoutRequested=true;Close();};
        FormClosing+=(_,e)=>
        {
            if(dashboard.IsBusy){e.Cancel=true;return;}
            dashboard.StopTimers();
        };
        Controls.Add(dashboard);dashboard.BringToFront();
    }
}
