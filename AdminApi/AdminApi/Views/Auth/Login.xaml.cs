using AdminApi.Models;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Net.Http.Headers;

namespace AdminApi.Views.Auth;

public partial class Login : ContentPage
{
    // ������������� HTTP �������
    private readonly HttpClient _httpClient = new HttpClient();

    public Login()
    {
        InitializeComponent();
    }

    // ������� �� �������� �����������
    private async void OnRegisterTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new Register());
    }

    // ��������������
    private async void OnLoginButtonClicked(object sender, EventArgs e)
    {
        // ��������� ������ � ������ �� �����
        string login = LoginEntry.Text;
        string password = PasswordEntry.Text;

        if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
        {
            await DisplayAlert("������", "������� ����� � ������", "OK");
            return;
        }

        var loginResponse = await AuthenticateUserAsync(login, password);
        if (loginResponse != null)
        {
            // ���������� ������ � SecureStorage
            await SecureStorage.SetAsync("auth_token", loginResponse.Token);

            // ��������� ��������� Authorization
            await SetAuthorizationHeader();

            // ������� �� �������� Home � ������� ������������
            await Navigation.PushAsync(new Home(loginResponse.User, loginResponse.Token));
        }
    }

    // �������������� ������������
    private async Task<AuthResponse> AuthenticateUserAsync(string login, string password)
    {
        // ������������ ���� ��� ��������
        var loginData = new { login, password };
        var jsonContent = new StringContent(JsonSerializer.Serialize(loginData), Encoding.UTF8, "application/json");

        try
        {
            // �������� POST-������� �� ������
            HttpResponseMessage response = await _httpClient.PostAsync("http://savayaqu.ddns.net/picsync/api/login", jsonContent);

            // �������� ���� ������
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<AuthResponse>(content);
                if (result?.Token != null && result.User.Role == "admin")
                {
                    return result; // ���������� ������ ������������ � �����
                }
                else
                {
                    await DisplayAlert("������ �����", "�� �� ��������� ���������������", "OK");
                }
        
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                await DisplayAlert("������ �����", "������������ ����� ��� ������", "OK");
            }
            else
            {
                await DisplayAlert("������", "��������� ������ �� �������", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("������ ����", ex.Message, "OK");
        }
        return null;
    }

    // ��������� ��������� Authorization
    private async Task SetAuthorizationHeader()
    {
        var token = await SecureStorage.GetAsync("auth_token");
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }
}