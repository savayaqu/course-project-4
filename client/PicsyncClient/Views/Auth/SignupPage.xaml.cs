using PicsyncClient.ViewModels.Auth;

namespace PicsyncClient.Views.Auth;

public partial class SignupPage : ContentPage
{
	public SignupPage()
	{
		InitializeComponent();
        //BindingContext = new SignupViewModel();
    }

    private void FocusToEntry2(object sender, EventArgs e) => Entry2.Focus();
    private void FocusToEntry3(object sender, EventArgs e) => Entry3.Focus();
    private void FocusToEntry4(object sender, EventArgs e) => Entry4.Focus();

    private void TrySignup(object sender, EventArgs e)
    {
        if (sender is Entry entry)
            entry.Unfocus();

        if (BindingContext is SignupViewModel vm)
            vm.TrySignupCommand.Execute(null);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is SignupViewModel vm)
            vm.Update();
    }
}