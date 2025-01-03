using PicsyncClient.Models;
using PicsyncClient.Models.Albums;
using PicsyncClient.Models.Pictures;
using SQLite;

namespace PicsyncClient.Utils;

public static class LocalDB
{
    private static SQLiteConnection? _db = null;
    public static SQLiteConnection DB
    {
        get
        {
            if (_db != null) 
                return _db;

            // Инициализация
            _db = new(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "local.db"));
            _db.CreateTable<Album>();
            _db.CreateTable<Picture>();
            return _db;
        }
    }
}