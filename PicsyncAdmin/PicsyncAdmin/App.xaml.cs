using PicsyncAdmin.Helpers;

namespace PicsyncAdmin
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            // Назнаение AppShell в качестве главной страницы
            MainPage = new AppShell();
            // Затем асинхронно загружаем сессию
            LoadAuthSession();
        }
        private async void LoadAuthSession()
        {
            await AuthSession.LoadSession();
        }
    }
}
