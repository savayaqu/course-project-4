using CommunityToolkit.Maui.Views;
using PicsyncClient.Models.Albums;
using PicsyncClient.ViewModels.Popups;

namespace PicsyncClient.Components.Popups;

public partial class PictureInfoPopup : Popup
{
	public PictureInfoPopup(Models.Pictures.IPicture picture)
	{
		InitializeComponent();
		BindingContext = new PictureInfoPopupViewModel(this, picture);
    }
}