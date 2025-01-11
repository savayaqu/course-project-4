using CommunityToolkit.Maui.Views;
using PicsyncClient.Models.Albums;
using PicsyncClient.ViewModels.Popups;

namespace PicsyncClient.Components.Popups;

public partial class AlbumSyncPopup : Popup
{
	public AlbumSyncPopup(AlbumLocal album)
	{
		InitializeComponent();
		BindingContext = new AlbumSyncPopupViewModel(this, album);
    }
}