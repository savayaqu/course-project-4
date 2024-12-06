
using PicsyncAdmin.ViewModels;

namespace PicsyncAdmin.Views;

public partial class Home : ContentPage
{
    private readonly HomeViewModel _viewModel;
    public Home()
    {
        InitializeComponent();
        _viewModel = new HomeViewModel();
        BindingContext = _viewModel;

    }

}
