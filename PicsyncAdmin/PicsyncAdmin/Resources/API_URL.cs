using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicsyncAdmin.Resources
{
    public class API_URL
    {
        private static readonly string _baseValue = "https://savayaqu.duckdns.org/picsync/api";
        private readonly string _path;

        public API_URL(string path)
        {
            _path = path.TrimStart('/'); // Убираем лишний слэш в начале
        }

        public static implicit operator string(API_URL apiUrl)
        {
            return $"{_baseValue}/{apiUrl._path}";
        }
    }


}
