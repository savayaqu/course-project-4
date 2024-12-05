using PicsyncAdmin.Helpers;
using PicsyncAdmin.ViewModels;

namespace PicsyncAdmin
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            if (AuthSession.Token != null)
                GoToAsync("//MainPage");
            else
                GoToAsync("//LoginPage");
        }
    }
}
