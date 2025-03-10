using PicsyncClient.Models;
using PicsyncClient.Utils;
using PicsyncClient.Views;
using System.Diagnostics;

namespace PicsyncClient;

public partial class AppShell : Shell
{
    //private readonly List<PageRoute> _navigationItems = [
    //    new() { Title = "Недавнее" , Icon = "house.png"      , PageType = typeof(MainPage)    , Route = "Main" },
    //    new() { Title = "Альбомы"  , Icon = "library_big.png", PageType = typeof(AlbumsPage)  , Route = "Albums" },
    //    new() { Title = "Настройки", Icon = "settings.png"   , PageType = typeof(SettingsPage), Route = "Settings" }
    //];

    public AppShell()
    {
        InitializeComponent();

        Debug.WriteLine($"ServerData.Url {ServerData.Url}, AuthData.Token {AuthData.Token}");

        if (ServerData.Url == null)
            GoToAsync("//ServerSelector");
        else if (AuthData.Token == null)
            GoToAsync("//Login");
        else
            GoToAsync("//Main");

        /*
        bool isPhone = DeviceInfo.Idiom == DeviceIdiom.Phone;

        if (isPhone)
            FlyoutBehavior = FlyoutBehavior.Disabled;

        if (isPhone)
        {
            TabBar tabBar = new();
            foreach (var item in _navigationItems)
            {
                tabBar.Items.Add(new Tab
                {
                    Title = item.Title,
                    Icon = item.Icon,
                    Items =
                    {
                        new ShellContent
                        {
                            ContentTemplate = new DataTemplate(item.PageType),
                            Route = item.Route
                        }
                    }
                });
            }
            Items.Add(tabBar);
        }
        else
        {
            foreach (var item in _navigationItems)
            {
                var flyoutItem = new FlyoutItem
                {
                    Title = item.Title,
                    Icon = item.Icon,
                    Items =
                    {
                        new ShellContent
                        {
                            Title = item.Title,
                            ContentTemplate = new DataTemplate(item.PageType),
                            Route = item.Route
                        }
                    }
                };
                Items.Add(flyoutItem);
            }
        }*/
    }
}
