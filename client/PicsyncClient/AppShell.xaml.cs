using PicsyncClient.Utils;

namespace PicsyncClient;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        if (AuthData.Token != null)
            GoToAsync("//Main");
        else
            GoToAsync("//Login");
    }
}
