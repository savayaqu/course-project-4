using PicsyncClient.ViewModels;

namespace PicsyncClient.Views;

public partial class MainPage : ContentPage
{
    private MainPageViewModel _viewModel;

    public MainPage()
    {
        InitializeComponent();
        BindingContext = _viewModel = new MainPageViewModel(this);
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
}