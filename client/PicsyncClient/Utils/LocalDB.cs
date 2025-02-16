using PicsyncClient.Models;
using PicsyncClient.Models.Albums;
using PicsyncClient.Models.Pictures;
using SQLite;
using System.Diagnostics;
using static PicsyncClient.Utils.Helpers;

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
            string hashname;
            try
            {
                string host = ServerData.Url?.Host.ToLower() ?? "nohost";
                string path = ServerData.Url?.AbsolutePath.ToLower() ?? "nopath";
                string login = AuthData.User?.Login?.ToLower() ?? "nologin";
                string data = $"{host}{path}_{login}";
#if DEBUG
                Debug.WriteLine("LocalDB: Init: data: " + data);
#endif
                hashname = HashUTF8toMD5HEX(data);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("LocalDB: Init: Exception: " + ex.Message);
                hashname = "default";
            }
            _db = new(Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
                $"local-{hashname}.db"
            ));
            _db.CreateTable<AlbumSynced>();
            _db.CreateTable<PictureSynced>();
            _db.CreateTable<PictureDuplica>();
            return _db;
        }
    }
    public static void Reset()
    {
        _db = null;
    }
}