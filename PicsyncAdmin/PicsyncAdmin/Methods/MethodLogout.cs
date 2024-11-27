using PicsyncAdmin.Helpers;
using PicsyncAdmin.Models;
using PicsyncAdmin.Resources;
using PicsyncAdmin.Views.Auth;

using System.Net.Http.Headers;

namespace PicsyncAdmin.Methods;

public static class MethodLogout
{
    private static readonly HttpClient _httpClient = new HttpClient();

    public static async Task<bool> Logout(User user, string token)
    {
 
            // ��������� ����� � ��������� Authorization
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // �������� POST-������� �� ������
            HttpResponseMessage response = await _httpClient.PostAsync(new API_URL("logout"), null);

            if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
            {
            _httpClient.DefaultRequestHeaders.Authorization = null;
            // ������� ������ (���� � �����)
            AuthSession.ClearSession();
            return true; // ���������, ��� ����� �������
            }
            // ���� ��������� ������
            return false;
    }
}
