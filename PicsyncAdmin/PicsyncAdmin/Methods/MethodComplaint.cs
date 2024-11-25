using PicsyncAdmin.Models;
using PicsyncAdmin.Resources;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PicsyncAdmin.Methods
{
    public static class MethodComplaint
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        // Метод для загрузки списка жалоб с сервера
        public static async Task<List<Complaint>> LoadComplaints(string token)
        {
            try
            {
                // Добавление токена авторизации
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                // Отправка GET-запроса на сервер
                var response = await _httpClient.GetAsync(new API_URL("complaints"));
                Debug.WriteLine($"Response Status: {response.StatusCode}");

                // Чтение ответа от сервера
                var responseContent = await response.Content.ReadAsStringAsync();
                Debug.WriteLine("Response content: " + responseContent); // Выводим для отладки

                // Десериализация ответа в объект ComplaintList
                var complaintsList = JsonSerializer.Deserialize<ComplaintList>(responseContent);

                // Проверка успешного получения данных
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    // Возвращаем список жалоб
                    Debug.WriteLine("Это список жалоб",complaintsList?.Complaints ?? new List<Complaint>());
                    return complaintsList?.Complaints ?? new List<Complaint>();
                }
                else
                {
                    Debug.WriteLine("Failed to fetch complaints. Status Code: " + response.StatusCode);
                    return new List<Complaint>();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error: {ex.Message}");
                return new List<Complaint>();
            }
        }
    }
}
