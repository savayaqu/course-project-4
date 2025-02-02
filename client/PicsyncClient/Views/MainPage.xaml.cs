using PicsyncClient.ViewModels;

namespace PicsyncClient.Views;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    private void CollectionView_SizeChanged(object sender, EventArgs e)
    {
        if (sender is not CollectionView collection ||
            BindingContext is not MainPageViewModel viewModel) return;

        viewModel.CalculateColumnsWidthCommand.Execute(collection.Width);
    }
}