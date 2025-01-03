using PicsyncClient.Models;
using PicsyncClient.Models.Albums;
using PicsyncClient.ViewModels;

namespace PicsyncClient.Views;

public partial class AlbumPage : ContentPage
{
	public AlbumPage(IAlbum album)
	{
		InitializeComponent();
        BindingContext = new AlbumViewModel(album);
    }

    private void CollectionView_SizeChanged(object sender, EventArgs e)
    {
        if (sender is not CollectionView collection ||
            BindingContext is not AlbumViewModel viewModel) return;

        viewModel.CalculateColumnsWidthCommand.Execute(collection.Width);
    }
}