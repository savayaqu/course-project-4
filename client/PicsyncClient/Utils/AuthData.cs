using PicsyncClient.Models;
using PicsyncClient.Models.Response;
using System.Text.Json;
using static PicsyncClient.Utils.Fetcher;

namespace PicsyncClient.Utils;

public static class AuthData
{
    public static void Save(string? token, User? user)
    {
        Token = token;
        User = user;
    }

    private static string? _token = null;
    public static string? Token
    {
        get => _token ??= SecureStorage.GetAsync("token").Result;
        set
        {
            _token = value;

            if (value == null)
                SecureStorage.Remove("token");
            else
                SecureStorage.SetAsync("token", value);
        }
    }

    private static User? _user = null;
    public static User? User
    {
        get
        {
            if (_user == null)
            {
                var setting = Preferences.Get("user", null);
                if (setting != null)
                    _user = JsonSerializer.Deserialize<User>(setting);
            }
            return _user;
        }
        set
        {
            _user = value;

            if (value == null)
                Preferences.Remove("user");
            else
                Preferences.Set("user", JsonSerializer.Serialize(value));
        }
    }

    private static UserStats? _stats = null;
    public static UserStats? Stats
    {
        get
        {
            if (_stats == null)
            {
                var setting = Preferences.Get("stats", null);
                if (setting != null)
                    _stats = JsonSerializer.Deserialize<UserStats>(setting);
            }
            return _stats;
        }
        set
        {
            _stats = value;

            if (value == null)
                Preferences.Remove("stats");
            else
                Preferences.Set("stats", JsonSerializer.Serialize(value));
        }
    }

    public static void SaveAndNavigate(string token, User user)
    {
        Token = token;
        User = user;
        Shell.Current.GoToAsync("//Main");
    }

    public static async Task TryExitAndNavigate(Action<bool>? setIsFetch = null)
    {
        bool isExit = false;
        try
        {
            var res = await FetchAsync(HttpMethod.Post, "logout", setIsFetch);
            if (!res.IsSuccessStatusCode)
                throw new Exception($"Пришёл плохой код ({(int)res.StatusCode} {res.ReasonPhrase})");

            isExit = true;
        }
        catch (Exception ex)
        {
            isExit = await Shell.Current.DisplayAlert(
                "Ошибка", 
                $"Токен не удалился. Причина:\n{ex.Message}\nВыйти насильно?", 
                "Да", "Отмена"
            );
        }

        if (isExit)
        {
            Token = null;
            User = null;
            await Shell.Current.GoToAsync("//Login");
        }
    }

    public static async Task Update(
        Action<string?>? setError = null,
        CancellationToken token = default
    ) {
        (var res, var body) = await FetchAsync<UserResponse>(
            HttpMethod.Get, 
            URLs.UserSelf,
            setError: setError,
            cancellationToken: token
        );

        if (body == null) return;

        if (User != null)
        {
            User.Nickname = body.User.Nickname;
            User.Login    = body.User.Login;
        }
        else
        {
            User = body.User;
        }

        if (Stats != null)
        {
            Stats.Update(body.User, body.Quota);
            Preferences.Set("stats", JsonSerializer.Serialize(Stats));
        }
        else
        {
            Stats = new(body.User, body.Quota);
        }

    }
}