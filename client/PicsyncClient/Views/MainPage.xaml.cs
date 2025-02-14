using PicsyncClient.Utils;
using PicsyncClient.ViewModels;

namespace PicsyncClient.Views;

public partial class MainPage : ContentPage
{
    private MainPageViewModel _viewModel;
    private ulong? userId = null;

    public MainPage()
    {
        InitializeComponent();
        BindingContext = _viewModel = new MainPageViewModel(this);
        userId = AuthData.User?.Id;
    }

    private void CollectionView_SizeChanged(object sender, EventArgs e)
    {
        if (sender is not CollectionView collection) return;

        _viewModel.CalculateColumnsWidthCommand.Execute(collection.Width);
    }

    public void ScrollTo(Models.Pictures.IPicture picture)
    {
        PicturesCollectionView.ScrollTo(
            picture, 
            position: ScrollToPosition.Center, 
            animate: true
        );
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        var newUserId = AuthData.User?.Id;

        if (userId != newUserId)
        {
            userId = newUserId;
            _ = _viewModel.Refresh();
            return;
        }

        _viewModel.LightUpdate();
    }
}