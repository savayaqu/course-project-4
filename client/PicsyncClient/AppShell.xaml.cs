using PicsyncClient.Utils;

namespace PicsyncClient;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        if (ServerData.Url == null)
            GoToAsync("//ServerSelector");
        else if (AuthData.Token == null)
            GoToAsync("//Login");
        else
            GoToAsync("//Main");
    }
}
