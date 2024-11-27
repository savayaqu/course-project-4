using PicsyncAdmin.Methods;
using PicsyncAdmin.Models;
using PicsyncAdmin.ViewModels;
using PicsyncAdmin.Views.Auth;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace PicsyncAdmin.Views;

public partial class Home : ContentPage
{
    public Home(User user, string token)
    {
        InitializeComponent();
        BindingContext = new HomeViewModel(user, token);
        Debug.WriteLine(user.Name);
        HelloHome.Text = $"Приветствуем, {user.Name}";
    }

}
