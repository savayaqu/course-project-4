using PicsyncClient.ViewModels;

namespace PicsyncClient.Views;

public partial class AlbumsPage : ContentPage
{
	public AlbumsPage()
	{
		InitializeComponent();
    }

    private void CollectionView_SizeChanged(object sender, EventArgs e)
    {
        if (sender is not CollectionView collection ||
            BindingContext is not AlbumsViewModel viewModel) return;

        viewModel.CalculateColumnsWidthCommand.Execute(collection.Width);
    }
}