using PicsyncAdmin.Helpers;

namespace PicsyncAdmin
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            AuthSession.LoadSession();

            // Назнаение AppShell в качестве главной страницы
            MainPage = new AppShell();
        }
    }
}
