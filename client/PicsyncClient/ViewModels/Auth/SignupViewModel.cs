using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PicsyncClient.Models.Request;
using PicsyncClient.Models.Response;
using PicsyncClient.Utils;
using System.Diagnostics;
using System.Net;
using System.Text.Json;
using static PicsyncClient.Utils.Fetcher;

namespace PicsyncClient.ViewModels.Auth;

public partial class SignupViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(TrySignupCommand))]
    private string _nickname = "";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(TrySignupCommand))]
    private string _login = "";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(TrySignupCommand))]
    private string _password = "";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(TrySignupCommand))]
    private string _passwordConfirm = "";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(TrySignupCommand))]
    private bool _isFetch;

    [ObservableProperty]
    private string? _error;

    [ObservableProperty]
    private Dictionary<string, List<string>> _badFields = [];

    public string Url => ServerData.Url?.ToString() ?? "";

    [RelayCommand]
    private void ForgetServer() => ServerData.ForgetAndNavigate();

    [RelayCommand]
    private void GoToLogin()
    {
        Shell.Current.GoToAsync("//Login");
    }

    [RelayCommand(CanExecute = nameof(CanSignup))]
    private async Task TrySignup()
    {
        Error = null;
        BadFields = [];

        if (Password != PasswordConfirm)
        {
            Error = "Ошибка валидации данных";
            BadFields = new()
            {
                { "passwordConfirm", ["Пароли не совпадают"] },
            };
            return;
        }
        var regData = new RegisterRequest(Login, Password, Nickname);
        try
        {
            var (res, body) = await FetchAsync<AuthResponse>(
                HttpMethod.Post, "register",
                isFetch => IsFetch = isFetch,
                error   => Error   = error,
                regData,
                serialize: true
            );

            if (res == null)
                throw new Exception(Error ?? "Сервер не ответил");

            if (res.StatusCode == HttpStatusCode.UnprocessableEntity)
            {
                var errorJson = await res.Content.ReadAsStringAsync();
                Debug.WriteLine("ERR_JSON: " + errorJson);
                BadFields = JsonSerializer.Deserialize<ErrorResponse>(errorJson)?.Errors ?? [];
                throw new Exception("Ошибка валидации данных");
            }

            if (!res.IsSuccessStatusCode)
                throw new Exception(Error ?? $"Пришёл плохой код ({(int)res.StatusCode} {res.ReasonPhrase})");

            if (Error != null)
                throw new Exception(Error);

            if (body?.Token == null || body.User == null)
                throw new Exception("Ошибка сервера: не пришли нужные данные\n"
                    + await res.Content.ReadAsStringAsync());

            Password = "";
            PasswordConfirm = "";
            AuthData.SaveAndNavigate(body.Token, body.User);
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }
    }

    private bool CanSignup() => Nickname        != "" 
                             && Login           != "" 
                             && Password        != "" 
                             && PasswordConfirm != "" 
                             && !IsFetch;

    public void Update()
    {
        OnPropertyChanged(nameof(Url));
    }
}
