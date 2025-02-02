using PicsyncClient.ViewModels.Auth;

namespace PicsyncClient.Views.Auth;

public partial class LoginPage : ContentPage
{
	public LoginPage()
	{
		InitializeComponent();
		//BindingContext = new LoginViewModel();
    }

    private void FocusToPassword(object sender, EventArgs e)
    {
        PasswordEntry.Focus();
    }

    private void TryLogin(object sender, EventArgs e)
    {
        if (sender is Entry entry)
            entry.Unfocus();

        if (BindingContext is LoginViewModel vm)
            vm.TryLoginCommand.Execute(null);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is LoginViewModel vm)
            vm.Update();
    }
}