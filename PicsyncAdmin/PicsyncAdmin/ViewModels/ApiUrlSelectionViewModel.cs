using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PicsyncAdmin.Helpers;

namespace PicsyncAdmin.ViewModels
{
    public partial class ApiUrlSelectionViewModel : ObservableObject
    {
        // Список сохранённых URL
        [ObservableProperty]
        private ObservableCollection<string> savedApiUrls = new ObservableCollection<string>();
        // Поле для ввода URL
        [ObservableProperty] public string apiUrlEntry;
        // Выбранный URL
        [ObservableProperty] public string selectedApiUrl;

        // Конструктор
        public ApiUrlSelectionViewModel()
        {
            LoadSavedApiUrls();
        }
        [RelayCommand]
        public async Task ShowApiUrlSelection()
        {
            SelectedApiUrl = await Shell.Current.DisplayActionSheet("Выберите API URL", "Отмена", null, SavedApiUrls.ToArray());
            if (!string.IsNullOrEmpty(SelectedApiUrl) && SelectedApiUrl != "Отмена")
            {
                if (IsValidUrl(SelectedApiUrl))
                {
                    AuthSession.SaveUrl(SelectedApiUrl);
                    Debug.Write(SelectedApiUrl ,"Во время выбора и сохранения токена на странице с выбором апи");
                    await TestUriAp();
                }
                else
                {
                    await Shell.Current.DisplayAlert("Ошибка", "Некорректный URL. Выберите другой.", "OK");
                }
            }
        }
        private void LoadSavedApiUrls()
        {
            var savedUrls = Preferences.Get("savedApiUrls", string.Empty);
            if (!string.IsNullOrEmpty(savedUrls))
            {
                var urls = savedUrls.Split(';').ToList();
                SavedApiUrls = new ObservableCollection<string>(urls);
            }
        }

        [RelayCommand]
        public async Task SaveApiUrl()
        {
            ApiUrlEntry = await Shell.Current.DisplayPromptAsync("Сервер", "Введите URL", "OK", "Отмена", "https://example.com");
            if (!string.IsNullOrEmpty(ApiUrlEntry))
            {
                if (IsValidUrl(ApiUrlEntry))
                {
                    var result = await Shell.Current.DisplayActionSheet(
                        "Выберите действие", // Заголовок
                        "Отмена",            // Кнопка "Отмена"
                        null,                 // Нет текста для cancel кнопки
                        "Сохранить и выбрать", // Первый вариант
                        "Сохранить"           // Второй вариант
                    );
                    if (result == "Сохранить и выбрать")
                    {
                        // Сохраняем URL и выбираем его
                        await SaveUrlAndSelect();
                    }
                    else if (result == "Сохранить")
                    {
                        // Только сохраняем URL
                        SaveUrlOnly();
                    }
                }
                else
                {
                    // Если URL некорректный, показываем сообщение об ошибке
                    await Shell.Current.DisplayAlert("Ошибка", "URL должен иметь стандартный формат и не оканчиваться на /", "OK");
                }
            }
        }
        // Функция для проверки правильности URL
        private static bool IsValidUrl(string url)
        {
            // Регулярное выражение для проверки URL без api и up
            var regex = @"^https?://(?!.*(/$|/api$|/up$)).*$";
            return System.Text.RegularExpressions.Regex.IsMatch(url, regex);
        }
        // Функция для сохранения и одновременного выбора Url
        [RelayCommand]
        private async Task SaveUrlAndSelect()
        {
            SavedApiUrls.Add(ApiUrlEntry);
            var urls = SavedApiUrls.ToList();
            Preferences.Set("savedApiUrls", string.Join(";", urls));
            AuthSession.SaveUrl(ApiUrlEntry);
            await TestUriAp();
        }
        // Функция для сохранения Url
        private void SaveUrlOnly()
        {
            SavedApiUrls.Add(ApiUrlEntry);
            var urls = SavedApiUrls.ToList();
            Preferences.Set("savedApiUrls", string.Join(";", urls));
        }
        // Функция для проверки работоспособности Url
        [RelayCommand]
        public static async Task TestUriAp()
        {
            var url = AuthSession.SelectedUrl;

            if (string.IsNullOrEmpty(url))
            {
                Debug.WriteLine("SelectedUrl пустой или не задан.");
                await Shell.Current.DisplayAlert("Ошибка", "Базовый URL не задан. Выберите сервер снова.", "OK");
                await Shell.Current.GoToAsync("//ApiUrlSelectionPage");
                return;
            }

            var timeout = TimeSpan.FromSeconds(5);
            var isUp = await ApiHelper.ExecuteRequestAsync(async () =>
            {
                using var cts = new CancellationTokenSource(timeout);
                var client = new HttpClient();
                var response = await client.GetAsync($"{url}/up", cts.Token);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                if ((int)response.StatusCode == 502 || (int)response.StatusCode == 504)
                {
                    throw new Exception("Сервер временно недоступен (502/504).");
                }
                throw new Exception($"Ошибка: {response.StatusCode}");
            });

            if (isUp)
            {
                await Shell.Current.GoToAsync("///LoginPage");
            }
            else
            {
                var retryChoice = await Shell.Current.DisplayActionSheet(
                    "Сервер недоступен. Попробовать снова или выбрать другой?",
                    "Отмена",
                    null,
                    "Попробовать снова",
                    "Выбрать другой сервер"
                );

                if (retryChoice == "Попробовать снова")
                {
                    await TestUriAp(); // Рекурсия для повторной проверки
                }
                else if (retryChoice == "Выбрать другой сервер")
                {
                    AuthSession.SelectedUrl = null;
                    await Shell.Current.GoToAsync("//ApiUrlSelectionPage");
                }
            }
        }
    }
}
