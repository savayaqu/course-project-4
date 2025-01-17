using CommunityToolkit.Maui.Views;
using PicsyncClient.Models.Albums;
using PicsyncClient.ViewModels.Popups;

namespace PicsyncClient.Components.Popups;

public partial class InvitationCreatePopup : Popup
{
	public InvitationCreatePopup(AlbumRemote album)
	{
		InitializeComponent();
		BindingContext = new InvitationCreatePopupViewModel(this, album);
    }
}