using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicsyncAdmin.Helpers
{
    public class API_URL
    {
        private static readonly string _baseValue = Preferences.Get("selectedApiUrl", string.Empty);
        private readonly string _path;

        public API_URL(string path)
        {
            _path = path.TrimStart('/'); // Убираем лишний слэш в начале
        }

        public static implicit operator string(API_URL apiUrl)
        {
            return $"{_baseValue}/api/{apiUrl._path}";
        }
    }
}
