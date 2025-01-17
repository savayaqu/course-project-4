using PicsyncAdmin.Models;
using PicsyncAdmin.Models.Response;
using PicsyncAdmin.ViewModels;

namespace PicsyncAdmin.Views;


public partial class UserContentPage : ContentPage
{
    public UserContentPage(Album album)
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
}

