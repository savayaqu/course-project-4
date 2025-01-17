using CommunityToolkit.Maui.Views;
using PicsyncClient.Models.Albums;
using PicsyncClient.ViewModels.Popups;

namespace PicsyncClient.Components.Popups;

public partial class AlbumDesyncPopup : Popup
{
	public AlbumDesyncPopup(AlbumSynced album)
	{
		InitializeComponent();
		BindingContext = new AlbumDesyncPopupViewModel(this, album);
    }
}