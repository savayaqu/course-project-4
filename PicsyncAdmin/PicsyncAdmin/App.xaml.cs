using PicsyncAdmin.Models;
using PicsyncAdmin.Resources;
using PicsyncAdmin.ViewModels;
using PicsyncAdmin.Views;
using PicsyncAdmin.Views.Auth;
using System.Diagnostics;
using System.Text.Json;

namespace PicsyncAdmin
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
