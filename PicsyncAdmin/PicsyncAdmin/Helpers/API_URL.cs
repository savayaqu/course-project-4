using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicsyncAdmin.Helpers
{
    public class API_URL
    {
        private static readonly string? _baseValue = AuthSession.SelectedUrl;
        private readonly string _path;

        public API_URL(string path)
        {
            _path = path.TrimStart('/');
        }

        public static implicit operator string(API_URL apiUrl)
        {
            if (string.IsNullOrWhiteSpace(_baseValue))
            {
                throw new InvalidOperationException("Base URL не задан. Проверьте AuthSession.SelectedUrl.");
            }

            var fullUrl = $"{_baseValue}/api/{apiUrl._path}";
            return fullUrl;
        }
    }

}
