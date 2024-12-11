using PicsyncAdmin.Models;
using PicsyncAdmin.Models.Response;
using PicsyncAdmin.ViewModels;

namespace PicsyncAdmin.Views;


public partial class UserContentPage : ContentPage
{
    private UserContentViewModel ViewModel => (UserContentViewModel)BindingContext;


    public UserContentPage(AlbumViewModel album)
    {
        InitializeComponent();
        BindingContext = new UserContentViewModel(album); 
        SizeChanged += OnSizeChanged;
    }
    private void OnSizeChanged(object sender, EventArgs e)
    {
        if (AdaptiveGridLayout != null && Width > 0)
        {
            int columnCount = (int)(Width / 150); 
            columnCount = Math.Max(columnCount, 1); 
            AdaptiveGridLayout.Span = columnCount;
        }
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await ViewModel.LoadData();
    }
}


