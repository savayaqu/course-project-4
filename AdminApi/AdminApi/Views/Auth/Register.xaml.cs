using AdminApi.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace AdminApi.Views.Auth;

public partial class Register : ContentPage
{
    // ������������� HTTP �������
    private readonly HttpClient _httpClient = new HttpClient();

    public Register()
	{
		InitializeComponent();
	}

	// ���� � �����������
	private string? avatarFilePath;

	// ���������� �������
	private async void OnSelectAvatarClicked(object sender, EventArgs e)
	{
		var result = await FilePicker.PickAsync(new PickOptions
		{
			FileTypes = FilePickerFileType.Images,
			PickerTitle = "�������� �����������"
		});

		if (result != null)
		{
			avatarFilePath = result.FullPath;
			await DisplayAlert("������ ������", $"������ ����: {result.FileName}", "OK");
		}
	}

    // �����������
    private async void OnRegisterButtonClicked(object sender, EventArgs e)
	{
		if (PasswordEntry.Text != ConfirmPasswordEntry.Text) { 
			await DisplayAlert("������", "������ �� ���������", "OK");
			return;
		}

		// ���� ������ �� �����
		string surname = SurnameEntry.Text;
		string name = NameEntry.Text;
		string? patronymic = PatronymicEntry.Text;
		string sex = MaleRadioButton.IsChecked ? "1" : "0";
		string birthday = BirthdayPicker.Date.ToString("yyyy-MM-dd");
		string email = EmailEntry.Text;
		string password = PasswordEntry.Text;
		string nickname = NicknameEntry.Text;
		string? phone = PhoneEntry.Text;

		// ������������ ���� �������
		var registerData = new MultipartFormDataContent
		{
			{new StringContent(surname), "surname" },
			{new StringContent(name), "name" },
			{new StringContent(patronymic ?? string.Empty), "patronymic" },
			{new StringContent(sex), "sex" },
			{new StringContent(birthday), "birthday" },
			{new StringContent(email), "email" },
			{new StringContent(password), "password" },
			{new StringContent(nickname), "nickname" },
			{new StringContent(phone ?? string.Empty), "phone" },
		};

		// ���������� �������, ���� �� ��� ������
		if (!string.IsNullOrEmpty(avatarFilePath))
		{
			var fileContent = new StreamContent(File.OpenRead(avatarFilePath));
			registerData.Add(fileContent, "avatar", Path.GetFileName(avatarFilePath));
        }

		try
		{
			// ���������� ������ � ���������� ����� � response
			HttpResponseMessage response = await _httpClient.PostAsync("http://bububu.ru/api/register", registerData);

			if (response.StatusCode == System.Net.HttpStatusCode.Created)
			{
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<AuthResponse>(content);

                if (result?.Token != null)
                {
					await DisplayAlert("�������� �����������", $"��� �����: {result.Token}", "��");
                    await Navigation.PushAsync(new Home(result.User, result.Token));
                }
            } else if (response.StatusCode == System.Net.HttpStatusCode.UnprocessableContent)
			{
                await DisplayAlert("������ ��������� ������", "���", "��");
            }
            else
            {
                await DisplayAlert("������", "��������� ������ �� �������", "OK");
            }
        } 
		catch (Exception ex)
        {
            await DisplayAlert("������ ����", ex.Message, "��");
        }



    }

}