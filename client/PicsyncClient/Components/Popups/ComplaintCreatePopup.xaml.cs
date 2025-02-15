using CommunityToolkit.Maui.Views;
using PicsyncClient.Models.Albums;
using PicsyncClient.Models.Pictures;
using PicsyncClient.ViewModels.Popups;

namespace PicsyncClient.Components.Popups;

public partial class ComplaintCreatePopup : Popup
{
	public ComplaintCreatePopup(AlbumRemote album, PictureRemote? picture = null)
	{
		InitializeComponent();
		BindingContext = new ComplaintCreatePopupViewModel(this, album, picture);
    }
}