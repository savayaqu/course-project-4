using PicsyncAdmin.Models;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Net.Http.Headers;
using PicsyncAdmin.Resources;
using Microsoft.Win32;
using System.Net.Http.Json;
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