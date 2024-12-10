using PicsyncAdmin.Helpers;
using PicsyncAdmin.ViewModels;
using PicsyncAdmin.Views;
using PicsyncAdmin.Views.Auth;

namespace PicsyncAdmin
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
             if (AuthSession.SelectedUrl == null)
                GoToAsync("//ApiUrlSelectionPage");
            else if (AuthSession.Token == null)
              GoToAsync("//LoginPage");
            else
                GoToAsync("//MainPage");

        }
    }
}
