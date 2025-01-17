using CommunityToolkit.Maui.Views;
using PicsyncClient.Models.Albums;
using PicsyncClient.ViewModels.Popups;

namespace PicsyncClient.Components.Popups;

public partial class InvitationPreviewPopup : Popup
{
	public InvitationPreviewPopup()
	{
		InitializeComponent();
		BindingContext = new InvitationPreviewPopupViewModel(this);
    }
}