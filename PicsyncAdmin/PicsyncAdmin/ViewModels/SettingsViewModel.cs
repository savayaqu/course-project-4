using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PicsyncAdmin.Helpers;
using PicsyncAdmin.Models.Response.PicsyncAdmin.Models.Response;
using Settings = PicsyncAdmin.Models.Settings;
using Space = PicsyncAdmin.Models.Space;

namespace PicsyncAdmin.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        [ObservableProperty] private Settings settings;
        [ObservableProperty] private Space space;
        [ObservableProperty] private bool isLoading;
        [ObservableProperty] private int uploadDisablePercentage;
        [ObservableProperty] private int warningLimitForBan;
        [ObservableProperty] private double usedPercent;
        [ObservableProperty] private string usedSpaceHumanReadable;
        [ObservableProperty] private string totalSpaceHumanReadable;
        [ObservableProperty] private string freeSpaceHumanReadable;
        [ObservableProperty] private double usedPercentDisplay;
        [ObservableProperty] private string? validationMessage;
        [ObservableProperty] private Color? validationMessageColor;
        public SettingsViewModel()
        {
            _ = LoadSettingsAsync();
            // Инициализация по умолчанию
        }
        [RelayCommand]
        public async Task Tap(object item)
        {
            var result = await Shell.Current.DisplayActionSheet(
                        "Выберите действие", // Заголовок
                        "Отмена",            // Кнопка "Отмена"
                        null,                 // Нет текста для cancel кнопки
                        "Редактирование", // Первый вариант
                        "Удаление"           // Второй вариант
                    );
            if (result == "Редактирование")
            {
                await Edit(item);
            }
            else if (result == "Удаление")
            {
                await Del(item);
            }
        }
        [RelayCommand]
        public async Task Del(object item)
        {
            string value;
            string key;

            if (item is string)
            {
                // Логика для AllowedUploadMimes
                Settings.AllowedUploadMimes.Remove((string)item);
                value = string.Join(",", Settings.AllowedUploadMimes);
                key = "allowed_upload_mimes";
            }
            else if (item is int)
            {
                // Логика для AllowedPreviewSizes
                Settings.AllowedPreviewSizes.Remove((int)item);
                value = string.Join(",", Settings.AllowedPreviewSizes);
                key = "allowed_preview_sizes";
            }
            else
            {
                // Обработка случая, если item не является string или int
                throw new ArgumentException("Неподдерживаемый тип элемента", nameof(item));
            }
            await Save(key, value);
        }
        [RelayCommand]
        public async Task Edit(object item)
        {
            string prompt = await Shell.Current.DisplayPromptAsync("Редактирование", "Введите новое значение", initialValue: item.ToString());
            if (!string.IsNullOrEmpty(prompt))
            {
                if (item is string)
                {
                    // Редактирование элемента в AllowedUploadMimes
                    if(Regex.IsMatch(prompt, @"^[a-z]{3,5}$"))
                    {
                        int index = Settings.AllowedUploadMimes.IndexOf((string)item);
                        if (index != -1)
                        {
                            Settings.AllowedUploadMimes[index] = prompt;
                            await Save("allowed_upload_mimes", prompt);
                        }
                    }
                    else
                    {
                       ValidationMessage = "MIME должен быть из 5 букв нижнего регистра American Language";
                       ValidationMessageColor = Colors.Red;
                    }
                }
                else if (item is int)
                {
                    // Редактирование элемента в AllowedPreviewSizes
                    if (Regex.IsMatch(prompt, @"^(10000|[0-9]{1,4})$"))
                    {
                        int index = Settings.AllowedPreviewSizes.IndexOf((int)item);
                        if (index != -1)
                        {
                            if (int.TryParse(prompt, out int newIntValue))
                            {
                                Settings.AllowedPreviewSizes[index] = newIntValue;

                                await Save("allowed_preview_sizes", string.Join(",", Settings.AllowedPreviewSizes));
                            }
                        }
                    }
                    else
                    {
                        ValidationMessage = "Размер превью должен быть числом и не превышать 10000";
                        ValidationMessageColor = Colors.Red;
                    }
                }
            }
        }
       
        [RelayCommand]
        public async Task SaveBanSpace()
        {
            if(WarningLimitForBan != Settings.WarningLimitForBan)
            {
                await Save("warning_limit_for_ban", Convert.ToString(WarningLimitForBan));
            }
            if(UploadDisablePercentage != Settings.UploadDisablePercentage)
            {
                await Save("upload_disable_percentage", Convert.ToString(UploadDisablePercentage));
            }
        }
        public async Task Save(string key, object value)
        {
            var payload = new { value, key };
            try
            {
                var response = await Fetch.DoAsync(HttpMethod.Post, "/settings", setError: msg => ValidationMessage = msg, body: payload, serialize: true);
                if (response.IsSuccessStatusCode)
                {
                    ValidationMessage = "Изменения сохранены успешно.";
                    ValidationMessageColor = Colors.Green; // Успешное сообщение зелёным
                                                           // Очищаем сообщение через 3 секунды
                    await Task.Delay(2000);
                    ValidationMessage = null;
                }
                else
                {
                    ValidationMessageColor = Colors.Red;
                    ValidationMessage ??= "Не удалось сохранить изменения.";
                }
            }
            catch (Exception ex)
            {
                ValidationMessage = $"Ошибка: {ex.Message}";
            }
        }
        [RelayCommand]
        public async Task AddMime()
        {
            string prompt = await Shell.Current.DisplayPromptAsync("MIME", "Введите новый MIME", "OK", "Отмена");
            if (!string.IsNullOrEmpty(prompt) && Regex.IsMatch(prompt, @"^[a-z]{3,5}$"))
            {
                Settings.AllowedUploadMimes.Add(prompt);
                var value = string.Join(",", Settings.AllowedUploadMimes);
                await Save("allowed_upload_mimes", value);
            }
            else
            {
                ValidationMessage = "MIME должен быть из 5 букв нижнего регистра American Language";
                ValidationMessageColor = Colors.Red;
            }
        }
        [RelayCommand]
        public async Task AddSize()
        {
            string prompt = await Shell.Current.DisplayPromptAsync("MIME", "Введите новый MIME", "OK", "Отмена");
            if (!string.IsNullOrEmpty(prompt) && Regex.IsMatch(prompt, @"^(10000|[0-9]{1,4})$"))
            {
                if (int.TryParse(prompt, out int newIntValue))
                {
                    Settings.AllowedPreviewSizes.Add(newIntValue);
                    await Save("allowed_preview_sizes", string.Join(",", Settings.AllowedPreviewSizes));
                }
            }
            else
            {
                ValidationMessage = "Размер превью должен быть числом и не превышать 10000";
                ValidationMessageColor = Colors.Red;
            }
        }
        [RelayCommand]
        public async Task LoadSettingsAsync()
        {
            IsLoading = true;
            try
            {
                var response = await Fetch.DoAsync(HttpMethod.Get, "/settings", setError: msg => Debug.WriteLine(msg));
                var responseString = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(responseString))
                {
                    var settingsResponse = JsonSerializer.Deserialize<SettingsResponse>(responseString);
                    if (settingsResponse != null)
                    {
                        Settings = new Settings
                        {
                            AllowedUploadMimes = new ObservableCollection<string>(settingsResponse.Settings.AllowedUploadMimes),
                            AllowedPreviewSizes = new ObservableCollection<int>(settingsResponse.Settings.AllowedPreviewSizes),
                            WarningLimitForBan = settingsResponse.Settings.WarningLimitForBan,
                            FreeStorageLimit = settingsResponse.Settings.FreeStorageLimit,
                            UploadDisablePercentage = settingsResponse.Settings.UploadDisablePercentage
                        };
                        UploadDisablePercentage = settingsResponse.Settings.UploadDisablePercentage;
                        WarningLimitForBan = settingsResponse.Settings.WarningLimitForBan;
                        Space = new Space
                        {
                            Total = settingsResponse.Space.Total,
                            Free = settingsResponse.Space.Free,
                            Used = settingsResponse.Space.Used,
                            UsedPercent = settingsResponse.Space.UsedPercent,
                            GotAt = settingsResponse.Space.GotAt
                        };
                        UsedPercentDisplay = AppSettings.UsedPercent;
                        UsedPercent = (double)AppSettings.UsedPercent / 100;
                        UsedSpaceHumanReadable = AppSettings.BytesToHuman(AppSettings.UsedSpace);
                        TotalSpaceHumanReadable = AppSettings.BytesToHuman(AppSettings.TotalSpace);
                        FreeSpaceHumanReadable = AppSettings.BytesToHuman(AppSettings.FreeSpace);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading settings: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
