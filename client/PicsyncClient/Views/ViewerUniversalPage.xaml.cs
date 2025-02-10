using PicsyncClient.Models.Pictures;
using PicsyncClient.ViewModels;

namespace PicsyncClient.Views;

public partial class ViewerUniversalPage : ContentPage
{
	public ViewerUniversalPage(
        Models.Pictures.IPicture picture, 
		IList<Models.Pictures.IPicture>? listPictures = null, 
		Func<int, Task<IList<Models.Pictures.IPicture>>>? loadMore = null
    ) {
		InitializeComponent();
        BindingContext = new ViewerUniversalViewModel(picture, listPictures, loadMore);
    }

    public ViewerUniversalPage(
        IList<Models.Pictures.IPicture> listPictures,
        int position = 0,
        Func<int, Task<IList<Models.Pictures.IPicture>>>? loadMore = null
    ) {
        InitializeComponent();
        BindingContext = new ViewerUniversalViewModel(listPictures, position, loadMore);
    }
}