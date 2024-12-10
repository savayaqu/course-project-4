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
            GoToAsync("//ApiUrlSelectionPage");
            Routing.RegisterRoute("ApiUrlSelectionPage", typeof(ApiUrlSelectionPage));
            //if (AuthSession.Token != null)
            //  GoToAsync("//MainPage");
            //else
            //  GoToAsync("//LoginPage");

            Routing.RegisterRoute("UserContentPage", typeof(UserContentPage));
            Routing.RegisterRoute("LoginPage", typeof(Login));

        }
    }
}
