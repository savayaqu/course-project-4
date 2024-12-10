
using PicsyncAdmin.ViewModels;

namespace PicsyncAdmin.Views.Auth;

public partial class Login : ContentPage
{

    public Login()
    {
        InitializeComponent();
        BindingContext = new LoginViewModel(); // Привязываем ViewModel
    }

}