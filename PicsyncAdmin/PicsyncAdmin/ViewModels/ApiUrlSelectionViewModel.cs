using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PicsyncAdmin.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicsyncAdmin.ViewModels
{
    public partial class ApiUrlSelectionViewModel : ObservableObject
    {
        // Список сохранённых URL
        [ObservableProperty]
        private ObservableCollection<string> savedApiUrls = new ObservableCollection<string>();
        // Поле для ввода URL
        [ObservableProperty]
        public string apiUrlEntry;

        // Выбранный URL
        [ObservableProperty]
        public string selectedApiUrl;

        // Конструктор
        public ApiUrlSelectionViewModel()
        {
            // Очистить коллекцию
            //SavedApiUrls.Clear();
            // Записать пустой список в локальное хранилище
            //Preferences.Set("savedApiUrls", string.Empty);

            LoadSavedApiUrls();
        }
        [RelayCommand]
        public async Task ShowApiUrlSelection()
        {
            var result = await Shell.Current.DisplayActionSheet("Выберите API URL", "Отмена", null, SavedApiUrls.ToArray());

            if (!string.IsNullOrEmpty(result) && result != "Отмена")
            {
                Preferences.Set("SelectedUrl", result);  // Сохраняем выбранный URL
                AuthSession.SelectedUrl = result;
                await TestUriAp();
            }
        }

        // Загружаем сохранённые URL
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
                // Проверяем, что введённый URL соответствует формату
                if (IsValidUrl(ApiUrlEntry))
                {
                    // Показываем ActionSheet с 3 вариантами
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
        private bool IsValidUrl(string url)
        {
            // Регулярное выражение для проверки URL без api и up
            var regex = @"^https?://(?!.*(/$|/api$|/up$)).*$";
            return System.Text.RegularExpressions.Regex.IsMatch(url, regex);
        }
        [RelayCommand]
        private async Task SaveUrlAndSelect()
        {
            // Добавляем новый URL в коллекцию
            SavedApiUrls.Add(ApiUrlEntry);

            // Сохраняем коллекцию URL в локальное хранилище
            var urls = SavedApiUrls.ToList();
            Preferences.Set("savedApiUrls", string.Join(";", urls));

            // Сохраняем выбранный URL

            Preferences.Set("SelectedUrl", ApiUrlEntry);
            AuthSession.SelectedUrl = ApiUrlEntry;

            await TestUriAp();
        }
        private void SaveUrlOnly()
        {
            // Добавляем новый URL в коллекцию
            SavedApiUrls.Add(ApiUrlEntry);

            // Сохраняем коллекцию URL в локальное хранилище
            var urls = SavedApiUrls.ToList();
            Preferences.Set("savedApiUrls", string.Join(";", urls));
        }
        [RelayCommand]
        public async Task TestUriAp()
        {
            var url = AuthSession.SelectedUrl;
            try
            {
                HttpResponseMessage response = await new HttpClient().GetAsync($"{url}/up");
                if (response.IsSuccessStatusCode)
                {
                    await Shell.Current.GoToAsync("///LoginPage");
                    return;
                }
                throw new Exception($"{response.StatusCode} , {response.Content}");
            }
            catch(Exception ex) { await Shell.Current.DisplayAlert("Ошибка", $"{ex.Message} ", "OK"); }
                await Shell.Current.DisplayAlert("Ошибка", $"{url} сервер не отвечает, выберите другой", "OK");
            AuthSession.SelectedUrl = null;
        }


    }

}
