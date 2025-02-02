using PicsyncClient.Models.Pictures;
using PicsyncClient.ViewModels;

namespace PicsyncClient.Views;

public partial class ViewerPage : ContentPage
{
	public ViewerPage(Models.Pictures.IPicture picture, AlbumViewModel? albumViewModel = null)
	{
		InitializeComponent();
        BindingContext = new ViewerViewModel(picture, albumViewModel);
    }
}