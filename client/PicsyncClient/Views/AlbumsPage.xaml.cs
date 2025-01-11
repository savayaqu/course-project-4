using PicsyncClient.Utils;
using PicsyncClient.ViewModels;

namespace PicsyncClient.Views;

public partial class AlbumsPage : ContentPage
{
    private ulong? userId = null;

	public AlbumsPage()
	{
		InitializeComponent();
        userId = AuthData.User?.Id;
    }

    private void CollectionView_SizeChanged(object sender, EventArgs e)
    {
        if (sender is not CollectionView collection ||
            BindingContext is not AlbumsViewModel viewModel) return;

        viewModel.CalculateColumnsWidthCommand.Execute(collection.Width);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        var newUserId = AuthData.User?.Id;

        if (userId == newUserId ||
            BindingContext is not AlbumsViewModel viewModel) return;

        userId = newUserId;
        viewModel.Reset();
    }
}