using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui;
using PicsyncClient.Models.Albums;
using PicsyncClient.Models.Response;
using PicsyncClient.Models.Request;
using PicsyncClient.Utils;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.Json;
using static PicsyncClient.Utils.Fetcher;
using static PicsyncClient.Utils.LocalDB;
using PicsyncClient.Models.Pictures;
using PicsyncClient.Components.Popups;
using System.Net;

namespace PicsyncClient.ViewModels.Popups;

public partial class EditSelfPopupViewModel : ObservableObject
{
    [ObservableProperty] private bool    isBusy = false;
    [ObservableProperty] private string? error;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConfirmCommand))]
    private string? nickname = AuthData.User?.Nickname;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConfirmCommand))]
    private string? login = AuthData.User?.Login;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConfirmCommand))]
    private string? password;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConfirmCommand))]
    private string? passwordConfirm;

    [ObservableProperty]
    private Dictionary<string, List<string>> badFields = [];

    private readonly Popup _popup;

    public EditSelfPopupViewModel(Popup popup)
    {
        _popup = popup;
    }

    public bool CanConfirm => Login    != AuthData.User?.Login
                           || Nickname != AuthData.User?.Nickname
                           || !String.IsNullOrEmpty(Password);

    [RelayCommand(CanExecute = nameof(CanConfirm))]
    public async Task Confirm()
    {
        if (Login    == AuthData.User?.Login   ) Login    = null; 
        if (Nickname == AuthData.User?.Nickname) Nickname = null;

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

        MultipartFormDataContent formData = [];
        if (!string.IsNullOrEmpty(Login   )) formData.Add(new StringContent(Login   ), "login");
        if (!string.IsNullOrEmpty(Password)) formData.Add(new StringContent(Password), "password");
        if (!string.IsNullOrEmpty(Nickname)) formData.Add(new StringContent(Nickname), "name");

        (var res, var body) = await FetchAsync<UserResponse>(
            HttpMethod.Post,
            URLs.UserSelf,
            body: formData,
            setError: e => Error = e
        );

        if (res == null) return;

        if (res.StatusCode == HttpStatusCode.UnprocessableEntity)
        {
            var errorJson = await res.Content.ReadAsStringAsync();
            Debug.WriteLine("ERRJSON: " + errorJson);
            BadFields = JsonSerializer.Deserialize<ErrorResponse>(errorJson)?.Errors ?? [];
            Error = "Ошибка валидации данных";
        }

        if (body == null) return;
            
        AuthData.User = body.User;
        _popup.Close(true);
    }

    [RelayCommand]
    public void Cancel()
    {
        _popup.Close();
    }
}
