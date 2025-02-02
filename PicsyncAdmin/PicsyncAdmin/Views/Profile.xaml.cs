using PicsyncAdmin.Helpers;
using PicsyncAdmin.Models;
using PicsyncAdmin.ViewModels;

namespace PicsyncAdmin.Views;

public partial class Profile : ContentPage
{
    private User user;
	public Profile()
	{
		InitializeComponent();
        BindingContext = new ProfileViewModel();

    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is not ProfileViewModel viewmodel)
            return;
        if(user != AuthSession.User)
        {
            BindingContext = new ProfileViewModel();
        }
    }
}