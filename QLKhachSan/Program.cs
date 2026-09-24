using QLKhachSan.GUI;

namespace QLKhachSan;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        try
        {
            while(true)
            {
                using var login=new FormLogin();
                if(login.ShowDialog()!=DialogResult.OK || login.Session is null)break;
                using var main=new FormMain(login.Session);
                Application.Run(main);
                if(!main.LogoutRequested)break;
            }
        }
        catch(Exception ex){Ui.Error(null,ex);}
    }
}
