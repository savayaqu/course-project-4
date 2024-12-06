using PicsyncAdmin.Helpers;
using PicsyncAdmin.ViewModels;
using PicsyncAdmin.Views;

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

            Routing.RegisterRoute("UserContentPage", typeof(UserContentPage));

        }
    }
}
