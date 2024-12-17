using System;
using System.Diagnostics;

namespace PicsyncAdmin.Helpers
{
    public class API_URL
    {
        private readonly string _path;

        public API_URL(string path)
        {
            _path = path.TrimStart('/');
        }

        public static implicit operator string(API_URL apiUrl)
        {
            // Всегда берём актуальное значение SelectedUrl
            var baseValue = AuthSession.SelectedUrl;

            if (string.IsNullOrWhiteSpace(baseValue))
            {
                throw new InvalidOperationException("Base URL не задан. Проверьте AuthSession.SelectedUrl.");
            }

            var fullUrl = $"{baseValue}/api/{apiUrl._path}";
            Debug.WriteLine($"Сформированный URL: {fullUrl}");
            return fullUrl;
        }
    }
}
