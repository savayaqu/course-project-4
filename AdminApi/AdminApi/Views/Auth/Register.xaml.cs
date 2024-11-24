using AdminApi.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace AdminApi.Views.Auth;

public partial class Register : ContentPage
{
    // Инициализация HTTP клиента
    private readonly HttpClient _httpClient = new HttpClient();

    public Register()
	{
		InitializeComponent();
	}

	// Путь к изображению
	private string? avatarFilePath;

	// Обработчик аватара
	private async void OnSelectAvatarClicked(object sender, EventArgs e)
	{
		var result = await FilePicker.PickAsync(new PickOptions
		{
			FileTypes = FilePickerFileType.Images,
			PickerTitle = "Выберите изображение"
		});

		if (result != null)
		{
			avatarFilePath = result.FullPath;
			await DisplayAlert("Аватар выбран", $"Выбран файл: {result.FileName}", "OK");
		}
	}

    // Регистрация
    private async void OnRegisterButtonClicked(object sender, EventArgs e)
	{
		if (PasswordEntry.Text != ConfirmPasswordEntry.Text) { 
			await DisplayAlert("Ошибка", "Пароли не совпадают", "OK");
			return;
		}

		// Сбор данных из формы
		string surname = SurnameEntry.Text;
		string name = NameEntry.Text;
		string? patronymic = PatronymicEntry.Text;
		string sex = MaleRadioButton.IsChecked ? "1" : "0";
		string birthday = BirthdayPicker.Date.ToString("yyyy-MM-dd");
		string email = EmailEntry.Text;
		string password = PasswordEntry.Text;
		string nickname = NicknameEntry.Text;
		string? phone = PhoneEntry.Text;

		// Формирование тела запроса
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

		// Добавление аватара, если он был выбран
		if (!string.IsNullOrEmpty(avatarFilePath))
		{
			var fileContent = new StreamContent(File.OpenRead(avatarFilePath));
			registerData.Add(fileContent, "avatar", Path.GetFileName(avatarFilePath));
        }

		try
		{
			// Отправляем запрос и записываем ответ в response
			HttpResponseMessage response = await _httpClient.PostAsync("http://bububu.ru/api/register", registerData);

			if (response.StatusCode == System.Net.HttpStatusCode.Created)
			{
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<AuthResponse>(content);

                if (result?.Token != null)
                {
					await DisplayAlert("Успешная регистрация", $"Ваш токен: {result.Token}", "ОК");
                    await Navigation.PushAsync(new Home(result.User, result.Token));
                }
            } else if (response.StatusCode == System.Net.HttpStatusCode.UnprocessableContent)
			{
                await DisplayAlert("Ошибка валидалии данных", "ППЦ", "ОК");
            }
            else
            {
                await DisplayAlert("Ошибка", "Произошла ошибка на сервере", "OK");
            }
        } 
		catch (Exception ex)
        {
            await DisplayAlert("Ошибка сети", ex.Message, "ОК");
        }



    }

}