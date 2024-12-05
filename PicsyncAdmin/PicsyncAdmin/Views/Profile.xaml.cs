using PicsyncAdmin.ViewModels;

namespace PicsyncAdmin.Views;

public partial class Profile : ContentPage
{
	public Profile()
	{
		InitializeComponent();
        BindingContext = new ProfileViewModel();
    }
    
}