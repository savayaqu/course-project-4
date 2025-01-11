using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PicsyncClient.Models.Request;
using PicsyncClient.Models.Response;
using PicsyncClient.Utils;
using System.Diagnostics;
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

            if (res.StatusCode == System.Net.HttpStatusCode.UnprocessableEntity)
            {
                try
                {
                    Error = "Ошибка валидации данных";
                    var errorJson = await res.Content.ReadAsStringAsync();
                    Debug.WriteLine("ERRJSON: " + errorJson);
                    BadFields = JsonSerializer.Deserialize<ErrorResponse>(errorJson)?.Errors ?? [];
                }
                catch {}
                return;
            }
            else if (!res.IsSuccessStatusCode)
            {
                Error = "Ошибка сервера: " + Error;
                return;
            }
            else if (body?.Token == null || body?.User == null)
            {
                Error = "Ошибка сервера: " + "Не пришли данные";
                return;
            }
            Password = "";
            PasswordConfirm = "";
            AuthData.SaveAndNavigate(body.Token, body.User);
        }
        catch (HttpRequestException ex)
        {
            Error = ex.Message switch
            {
                "Connection failure" => "Нет соединения с сервером",
                _ => ex.Message,
            };
            return;
        }
    }

    private bool CanSignup() =>
        Nickname != "" &&
        Login != "" &&
        Password != "" &&
        PasswordConfirm != "" &&
        !IsFetch;
}
