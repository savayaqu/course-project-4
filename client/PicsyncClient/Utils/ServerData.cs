using PicsyncClient.Models;
using PicsyncClient.Models.Response;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Text.Json;
using static PicsyncClient.Utils.Fetcher;

namespace PicsyncClient.Utils;

public static class ServerData
{
    private static Uri? _url = null;
    public static Uri? Url
    {
        get
        {
            if (_url == null)
            {
                var url = Preferences.Get("url", null);
                if (url != null)
                    _url = new Uri(url);
            }
            return _url;
        }
        set
        {
            _url = value;

            if (value == null)
                Preferences.Remove("url");
            else
                Preferences.Set("url", value.ToString());
        }
    }

    private static ServerSettings? _settings = null;
    public static ServerSettings? Settings
    {
        get
        {
            if (_settings == null)
            {
                var setting = Preferences.Get("server_settings", null);
                if (setting != null)
                    _settings = JsonSerializer.Deserialize<ServerSettings>(setting);
            }
            return _settings;
        }
        set
        {
            _settings = value;

            if (value == null)
                Preferences.Remove("server_settings");
            else
                Preferences.Set("server_settings", JsonSerializer.Serialize(value));
        }
    }

    private static ObservableCollection<string>? _pastUrls = null;
    public static ObservableCollection<string> PastUrls
    {
        get
        {
            if (_pastUrls == null)
            {
                var pastUrls = Preferences.Get("past_urls", null);
                Debug.WriteLine("pastUrls: " + pastUrls);
                _pastUrls = new(pastUrls?.Split(';') ?? []);
                _pastUrls.CollectionChanged += OnPastUrlsChanged;
            }
            return _pastUrls;
        }
    }
    private static void OnPastUrlsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        Debug.WriteLine("OnPastUrlsChanged: " + JsonSerializer.Serialize(PastUrls));
        Preferences.Set("past_urls", string.Join(";", PastUrls));
    }

    public static async Task<bool> TrySaveAndNavigate(
        string rawUrl, 
        Action<bool>? setIsFetch = null,
        Action<string>? setError = null,
        CancellationToken cancellationToken = default
    ) {
        Debug.WriteLine("SRV-SETT: SaveStart");
        string? error = null;
        setIsFetch?.Invoke(false);
        setError?.Invoke(null);
        try
        {
            if (!rawUrl.StartsWith("http://") && 
                !rawUrl.StartsWith("https://"))
                rawUrl = "https://" + rawUrl;

            rawUrl = rawUrl.TrimEnd('/');

            if (rawUrl.EndsWith("/api"))
                rawUrl += rawUrl[.. ^4];

            Uri url = new(rawUrl);

            setIsFetch?.Invoke(true);
            (var res, var body) = await FetchAsync<ServerSettingsResponse>(
                HttpMethod.Get,
                new Uri(url + "/api"), 
                setIsFetch,
                mes => error = mes,
                cancellationToken: cancellationToken
            );
            if (res == null)
                throw new Exception(error ?? "Сервер не ответил");

            if (!res.IsSuccessStatusCode)
                throw new Exception(error ?? $"Пришёл плохой код ({(int)res.StatusCode} {res.ReasonPhrase})");

            if (error != null)
                throw new Exception(error);

            if (body == null)
                throw new Exception($"Пришёл не тот ответ, возможно сервер не является экземпляром PicSync");

            Url = url;
            Settings = body.Settings;

            var cleanUrl = rawUrl;
            if (cleanUrl.StartsWith("https://"))
                cleanUrl = cleanUrl.Substring(8);

            if (!PastUrls.Contains(cleanUrl))
                PastUrls.Add(cleanUrl);

            _ = Shell.Current.GoToAsync("//Login");
            return true;
        }
        catch (Exception ex)
        {
            setError.Invoke(ex.Message);
        }
        setIsFetch?.Invoke(false);
        return false;
    }

    public static void ForgetAndNavigate()
    {
        Url = null;
        Settings = null;
        Shell.Current.GoToAsync("//ServerSelector");
    }

    public static async Task Update(
        Action<string?>? setError = null,
        CancellationToken token = default
    ) {
        (var res, var body) = await FetchAsync<ServerSettingsResponse>(
            HttpMethod.Get,
            URLs.Settings,
            setError: setError,
            cancellationToken: token
        );

        if (body == null) return;
        Settings = body.Settings;
    }
}