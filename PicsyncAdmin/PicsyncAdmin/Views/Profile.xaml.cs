namespace PicsyncAdmin.Views;

public partial class Profile : ContentPage
{
	public Profile()
	{
		InitializeComponent();
	}
    // Обработчик для переключения темы
    private void OnToggleThemeClicked(object sender, EventArgs e)
    {
        var app = Application.Current;
        if (app.RequestedTheme == AppTheme.Light)
        {
            SwitchToDarkTheme();
        }
        else
        {
            SwitchToLightTheme();
        }
    }

    public void SwitchToLightTheme()
    {
        Application.Current.Resources["BackgroundColorLight"] = Color.FromArgb("#FFFFFF");
        Application.Current.Resources["TextColorLight"] = Color.FromArgb("#000000");
        Application.Current.Resources["ButtonBackgroundColorLight"] = Color.FromArgb("#808080");
    }

    public void SwitchToDarkTheme()
    {
        Application.Current.Resources["BackgroundColorDark"] = Color.FromArgb("#000000");
        Application.Current.Resources["TextColorDark"] = Color.FromArgb("#FFFFFF");
        Application.Current.Resources["ButtonBackgroundColorDark"] = Color.FromArgb("#A9A9A9");
    }
}