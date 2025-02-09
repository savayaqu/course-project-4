using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PicsyncClient.Models.Request;
using PicsyncClient.Models.Response;
using PicsyncClient.Utils;
using System.Net;
using static PicsyncClient.Utils.Fetcher;

namespace PicsyncClient.ViewModels.Auth;

public partial class LoginViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(TryLoginCommand))]
    private string login = "";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(TryLoginCommand))]
    private string password = "";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(TryLoginCommand))]
    private bool isFetch = false;

    [ObservableProperty]
    private string? error = null;

    public string Url => ServerData.Url?.ToString();

    [RelayCommand]
    public void ForgetServer() => ServerData.ForgetAndNavigate();

    [RelayCommand]
    private void GoToSignup() => Shell.Current.GoToAsync("//Signup");

    private bool CanLogin() =>
        Login    != "" &&
        Password != "" &&
        !IsFetch;

    [RelayCommand(CanExecute = nameof(CanLogin))]
    private async Task TryLogin()
    {
        var credentials = new CredentialsRequest(Login, Password);
        try
        {
            (var res, var body) = await FetchAsync<AuthResponse>(
                HttpMethod.Post, "login",
                isFetch => IsFetch = isFetch,
                error => Error = error,
                credentials,
                serialize: true
            );

            if (res == null)
                throw new Exception(Error ?? "Сервер не ответил");

            if (res.StatusCode == HttpStatusCode.Unauthorized)
                throw new Exception("Неправильный логин и/или пароль");

            if (!res.IsSuccessStatusCode)
                throw new Exception(Error ?? $"Пришёл плохой код ({(int)res.StatusCode} {res.ReasonPhrase})");

            if (Error != null)
                throw new Exception(Error);

            if (body?.Token == null || body?.User == null)
                throw new Exception("Ошибка сервера: не пришли нужные данные\n" 
                    + await res.Content.ReadAsStringAsync());

            Password = "";
            AuthData.SaveAndNavigate(body.Token, body.User);
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }

        /*
        Debug.WriteLine("ERROR: " + Error);
        Debug.WriteLine("BODY: " + JsonSerializer.Serialize(body));
        Debug.WriteLine("RESPONSE: " + JsonSerializer.Serialize(res));
        Debug.WriteLine("CONTENT: " + JsonSerializer.Serialize(await res.Content.ReadAsStringAsync()));
        */
    }
    public void Update()
    {
        OnPropertyChanged(nameof(Url));
    }
}
