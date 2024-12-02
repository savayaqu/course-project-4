using PicsyncClient.Utils;
using PicsyncClient.ViewModels;

namespace PicsyncClient.Views;

public partial class SettingsPage : ContentPage
{
	public SettingsPage()
	{
		InitializeComponent();
		BindingContext = new SettingsViewModel();
	}
}