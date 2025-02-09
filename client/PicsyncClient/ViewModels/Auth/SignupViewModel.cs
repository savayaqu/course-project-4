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
    private string nickname = "";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(TrySignupCommand))]
    private string login = "";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(TrySignupCommand))]
    private string password = "";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(TrySignupCommand))]
    private string passwordConfirm = "";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(TrySignupCommand))]
    private bool isFetch = false;

    [ObservableProperty]
    private string? error = null;

    [ObservableProperty]
    private Dictionary<string, List<string>> badFields = [];

    public string Url => ServerData.Url?.ToString();

    [RelayCommand]
    public void ForgetServer() => ServerData.ForgetAndNavigate();

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
            (var res, var body) = await FetchAsync<AuthResponse>(
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
                Debug.WriteLine("ERRJSON: " + errorJson);
                BadFields = JsonSerializer.Deserialize<ErrorResponse>(errorJson)?.Errors ?? [];
                throw new Exception("Ошибка валидации данных");
            }

            if (!res.IsSuccessStatusCode)
                throw new Exception(Error ?? $"Пришёл плохой код ({(int)res.StatusCode} {res.ReasonPhrase})");

            if (Error != null)
                throw new Exception(Error);

            if (body?.Token == null || body?.User == null)
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

    private bool CanSignup() =>
        Nickname != "" &&
        Login != "" &&
        Password != "" &&
        PasswordConfirm != "" &&
        !IsFetch;

    public void Update()
    {
        OnPropertyChanged(nameof(Url));
    }
}
