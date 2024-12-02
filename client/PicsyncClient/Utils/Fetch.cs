using PicsyncClient.Models.Response;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;

namespace PicsyncClient.Utils;

public static class Fetch
{
    public static Uri API_URL = new("https://sites.kopchan7.keenetic.link/picsync/api/");

    private readonly static HttpClient _httpClient = new() { BaseAddress = API_URL };

    public static async Task<HttpResponseMessage> DoAsync(
        HttpMethod method,
        string path,
        Action<bool>? setIsFetch = null,
        Action<string>? setError = null,
        dynamic? body = null,
        bool serialize = false
    ) {
        setIsFetch?.Invoke(true);
        setError?.Invoke(null);
        var request = new HttpRequestMessage(method, path);
        if (AuthData.Token != null)
            request.Headers.Add("Authorization", $"Bearer {AuthData.Token}");

        if (body != null)
        {
            if (serialize)
            {
                string json = JsonSerializer.Serialize(body);
                Debug.WriteLine("FETCH: REQUEST: JSON: " + json);
                request.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            }
            else if (body is HttpContent)
            {
                request.Content = body;
            }
            else throw new ArgumentException("body should be of the type HttpContent if serialize is false");
        }
        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            var responseJsonErr = await response.Content.ReadAsStringAsync();
            try
            {
                var responseBodyErr = JsonSerializer.Deserialize<ErrorResponse>(responseJsonErr);
                Debug.WriteLine("FETCH: ERROR: Obj: " + JsonSerializer.Serialize(responseBodyErr) + " Json: " + responseJsonErr);
                setError?.Invoke(responseBodyErr?.Message);
            }
            catch (Exception ex)
            {
                setError?.Invoke(ex.Message); // FIXME: удалить
            }
        }
        setIsFetch?.Invoke(false);
        return response;
    }

    public static async Task<(HttpResponseMessage, T?)> DoAsync<T>(
        HttpMethod method,
        string path,
        Action<bool>? setIsFetch = null,
        Action<string>? setError = null,
        dynamic? body = null,
        bool serialize = false
    ) {
        var response = await DoAsync(method, path, setIsFetch, setError, body, serialize);
        try
        {
            var responseJson = await response.Content.ReadAsStringAsync();
            var responseBody = JsonSerializer.Deserialize<T>(responseJson);
            return (response, responseBody);
        }
        catch (Exception) when(!response.IsSuccessStatusCode)
        {
            return (response, default(T));
        }
    }

    /*
    public static object Do(
        HttpMethod method,
        string path,
        out bool isFetch,
        out string? error,
        dynamic? body,
        bool serialize = false
    ) {
        error = null;
        isFetch = true;
        var response = DoAsync(method, path, body, serialize).Result;
        isFetch = false;
        if (!response.IsSuccessStatusCode)
        {
            var responseJson = response.Content.ReadAsStringAsync().Result;
            var responseBody = JsonSerializer.Deserialize(responseJson);
            error = responseBody?.message;
        }
        return response;
    }

    public static object Do<T>(
        HttpMethod method,
        string path,
        out bool isFetch,
        out string? error,
        dynamic? body,
        bool serialize = false
    ) {
        error = null;
        isFetch = true;
        (HttpResponseMessage response, T? responseBody) = DoAsync<T>(method, path, body, serialize).Result;
        isFetch = false;
        if (!response.IsSuccessStatusCode)
        {
            var responseJsonErr = response.Content.ReadAsStringAsync().Result;
            var responseBodyErr = JsonSerializer.Deserialize<dynamic>(responseJsonErr);
            error = responseBodyErr?.message;
        }
        return (response, responseBody);
    }
    */
}
