using PicsyncClient.Utils;
using PicsyncClient.ViewModels;
using System.Diagnostics;

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

        if (BindingContext is not AlbumsViewModel viewModel)
        {
            Debug.WriteLine("Почему-то не тот BindingContext в AlbumsPage");
#if DEBUG
            throw new Exception("Почему-то не тот BindingContext в AlbumsPage");
#else
            return;
#endif
        }

        var newUserId = AuthData.User?.Id;

        if (userId != newUserId)
        {
            userId = newUserId;
            viewModel.Reset();
            return;
        }

        viewModel.LightUpdate();
    }
}