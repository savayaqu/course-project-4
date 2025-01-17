using CommunityToolkit.Maui.Views;
using PicsyncClient.Models.Albums;
using PicsyncClient.ViewModels.Popups;

namespace PicsyncClient.Components.Popups;

public partial class AlbumUnjoinPopup : Popup
{
	public AlbumUnjoinPopup(AlbumRemote album)
	{
		InitializeComponent();
		BindingContext = new AlbumUnjoinPopupViewModel(this, album);
    }
}