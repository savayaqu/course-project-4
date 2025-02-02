using CommunityToolkit.Maui.Views;
using PicsyncClient.Models.Albums;
using PicsyncClient.ViewModels.Popups;

namespace PicsyncClient.Components.Popups;

public partial class GeneralSyncManagePopup : Popup
{
	public GeneralSyncManagePopup()
	{
		InitializeComponent();
		BindingContext = new GeneralSyncManagePopupViewModel(this);
    }
}