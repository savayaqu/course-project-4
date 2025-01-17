using CommunityToolkit.Maui.Views;
using PicsyncClient.Models.Albums;
using PicsyncClient.ViewModels.Popups;

namespace PicsyncClient.Components.Popups;

public partial class AlbumSyncManagePopup : Popup
{
	public AlbumSyncManagePopup(AlbumSynced album)
	{
		InitializeComponent();
		BindingContext = new AlbumSyncManagePopupViewModel(this, album);
    }
}