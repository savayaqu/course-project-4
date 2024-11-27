using PicsyncAdmin.Methods;
using PicsyncAdmin.Models;
using PicsyncAdmin.ViewModels;
using PicsyncAdmin.Views.Auth;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace PicsyncAdmin.Views;

public partial class Home : ContentPage
{
    public Home()
    {
        InitializeComponent();
       BindingContext = new HomeViewModel();
    }

}
