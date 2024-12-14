using System.Diagnostics;
using System.Text.Json;
using PicsyncAdmin.Models.Response;

namespace PicsyncAdmin.Helpers
{
    public static class Fetch
    {
        // Используем ваш класс API_URL для формирования базового URL
        private readonly static HttpClient _httpClient = new();

        public static async Task<HttpResponseMessage> DoAsync(
            HttpMethod method,
            string path,
            Action<bool>? setIsFetch = null,
            Action<string>? setError = null,
            dynamic? body = null,
            bool serialize = false
        )
        {
            setIsFetch?.Invoke(true);
            setError?.Invoke(null);

            try
            {
                // Формируем запрос
                var request = new HttpRequestMessage(method, new API_URL(path));

                if (AuthSession.Token != null)
                    request.Headers.Add("Authorization", $"Bearer {AuthSession.Token}");

                if (body != null)
                {
                    if (serialize)
                    {
                        string json = JsonSerializer.Serialize(body);
                        Debug.WriteLine($"FETCH: REQUEST: JSON: {json}");
                        request.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                    }
                    else if (body is HttpContent)
                    {
                        request.Content = body;
                    }
                    else
                    {
                        throw new ArgumentException("Body should be of the type HttpContent if serialize is false");
                    }
                }

                // Выполняем запрос
                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var responseJsonErr = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"FETCH: ERROR: Json: {responseJsonErr}");

                    try
                    {
                        var responseBodyErr = JsonSerializer.Deserialize<ValidationErrorResponse>(responseJsonErr);
                        setError?.Invoke(responseBodyErr?.Message ?? "Произошла ошибка.");
                    }
                    catch (JsonException)
                    {
                        setError?.Invoke("Ошибка обработки ответа от сервера.");
                    }
                }

                return response;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FETCH: EXCEPTION: {ex.Message}");
                setError?.Invoke(ex.Message);
                return new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError);
            }
            finally
            {
                setIsFetch?.Invoke(false);
            }
        }

        public static async Task<(HttpResponseMessage, T?)> DoAsync<T>(
            HttpMethod method,
            string path,
            Action<bool>? setIsFetch = null,
            Action<string>? setError = null,
            dynamic? body = null,
            bool serialize = false
        )
        {
            var response = await DoAsync(method, path, setIsFetch, setError, body, serialize);

            try
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var responseBody = JsonSerializer.Deserialize<T>(responseJson);
                return (response, responseBody);
            }
            catch (JsonException ex)
            {
                Debug.WriteLine($"FETCH: JSON DESERIALIZATION ERROR: {ex.Message}");
                setError?.Invoke("Ошибка обработки данных от сервера.");
                return (response, default);
            }
        }
    }
}
