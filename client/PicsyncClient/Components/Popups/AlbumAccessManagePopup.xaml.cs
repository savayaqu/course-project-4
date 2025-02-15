using CommunityToolkit.Maui.Views;
using PicsyncClient.Models.Albums;
using PicsyncClient.ViewModels.Popups;

namespace PicsyncClient.Components.Popups;

public partial class AlbumAccessManagePopup : Popup
{
	public AlbumAccessManagePopup(AlbumRemote album)
	{
		InitializeComponent();
		BindingContext = new AlbumAccessManagePopupViewModel(this, album);
		// TODO: на винде окно скролла слишком малое
    }
}