namespace AdminApi
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            // Назнаение AppShell в качестве главной страницы
            MainPage = new AppShell();
        }
    }
}
