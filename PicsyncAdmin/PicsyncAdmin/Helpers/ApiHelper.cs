using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicsyncAdmin.Helpers
{
    public static class ApiHelper
    {
        public static async Task<TResult?> ExecuteRequestAsync<TResult>(Func<Task<TResult>> requestFunc)
        {
            try
            {
                return await requestFunc();
            }
            catch (HttpRequestException httpEx)
            {
                await Shell.Current.DisplayAlert("Ошибка сети", $"Ошибка HTTP: {httpEx.Message}", "OK");
            }
            catch (TaskCanceledException)
            {
                await Shell.Current.DisplayAlert("Ошибка сети", "Превышено время ожидания запроса.", "OK");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Ошибка", $"Ошибка выполнения запроса: {ex.Message}", "OK");
            }
            return default; // Возвращаем null или значение по умолчанию
        }
    }

}
