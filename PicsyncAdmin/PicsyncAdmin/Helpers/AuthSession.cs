using PicsyncAdmin.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicsyncAdmin.Helpers
{
    public static class AuthSession
    {
        public static User? User { get; set; }
        public static string? Token { get; set; }

        // Метод для очистки данных при выходе
        public static void ClearSession()
        {
            User = null;
            Token = null;
        }
    }
}
