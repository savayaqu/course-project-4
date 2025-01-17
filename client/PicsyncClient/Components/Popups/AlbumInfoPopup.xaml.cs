using CommunityToolkit.Maui.Views;
using PicsyncClient.Models.Albums;
using PicsyncClient.ViewModels.Popups;

namespace PicsyncClient.Components.Popups;

public partial class AlbumInfoPopup : Popup
{
	public AlbumInfoPopup(IAlbum album)
	{
		InitializeComponent();
		BindingContext = new AlbumInfoPopupViewModel(this, album);
    }
}