using PicsyncAdmin.Models;
using PicsyncAdmin.Resources;
using PicsyncAdmin.Views.Auth;
using System.Net;
using System.Net.Http;

namespace PicsyncAdmin.Views;

public partial class Home : ContentPage
{
	private User _user;
	private string _token;
    private readonly HttpClient _httpClient = new HttpClient();

    public Home(User user, string token)
	{
		InitializeComponent();
		_user = user;
		_token = token;

		//������������� ������ � �������
		LoginHome.Text = _user.Login;

    }

    private async void LogoutButton_Clicked(object sender, EventArgs e)
    {
        await Logout();

    }
    private async Task Logout()
    {

        // �������� ������ ����� ��������� �������
        string token = _token;
        if (string.IsNullOrEmpty(token))
        {
            await DisplayAlert("������", "����� �� ������. ��������, �� ��� ����� �� �������.", "OK");
            return;
        }
        // ��������� ����� � ��������� Authorization
        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        await DisplayAlert("dff", "sd", "dfg");

        // �������� POST-������� �� ������
        HttpResponseMessage response = await _httpClient.PostAsync(new API_URL("logout"), null);

        // �������� ���� ������
        if (response.IsSuccessStatusCode)
        {

            await DisplayAlert("�����", "�� ������� ����� �� �������", "OK");
            _httpClient.DefaultRequestHeaders.Authorization = null;
            // ������� �� �������� ��������
            await Navigation.PushAsync(new Login());
        }
        else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            await DisplayAlert("������", "������ ���������������. ����������, ������� �����.", "OK");
        }
        else
        {
            await DisplayAlert("������", $"��������� ������ �� �������: {response.StatusCode}", "OK");
        }
    }           
}
