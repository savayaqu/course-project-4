using PicsyncClient.Utils;

namespace PicsyncClient.Views.Auth;

public partial class SignupPage : ContentPage
{
	public SignupPage()
	{
		InitializeComponent();
	}

    private void OnLoginLinkTapped(object sender, TappedEventArgs e)
    {
		Shell.Current.GoToAsync("//Login");
    }

    private void OnSignupButtonClicked(object sender, EventArgs e)
    {
        // TODO: рег и вход
        throw new NotImplementedException();
    }
}