using PicsyncAdmin.Helpers;
using PicsyncAdmin.Methods;
using PicsyncAdmin.Models;
using PicsyncAdmin.ViewModels;
using PicsyncAdmin.Views.Auth;
using System.Collections.ObjectModel;
using System.Diagnostics;

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
