using PicsyncClient.Enum;
using PicsyncClient.Models.Albums;
using PicsyncClient.Models.Pictures;
using static PicsyncClient.Utils.LocalDB;
using System.Diagnostics;
using System.Text;
using System.Collections.ObjectModel;
#if ANDROID
using Android;
using Android.OS;
using Android.Content.PM;
using AndroidX.Core.App;
using AndroidX.Core.Content;
#endif

namespace PicsyncClient.Utils;

public static class LocalData
{
    public static async Task<bool> CheckPermissions()
    {
#if ANDROID
        return ContextCompat.CheckSelfPermission(
            Platform.CurrentActivity, 
            (int)Build.VERSION.SdkInt >= 33 
                ? Manifest.Permission.ReadMediaImages
                : Manifest.Permission.ReadExternalStorage
            ) == Permission.Granted;
#else
        throw new PlatformNotSupportedException("К сожалению доступно только для Android");
#endif
    }

    public static async Task<bool> RequestPermissions()
    {
#if ANDROID

        bool granted = await CheckPermissions();
        if (granted) return true;
        ActivityCompat.RequestPermissions(
            Platform.CurrentActivity,
            (int)Build.VERSION.SdkInt >= 33
                ? [Manifest.Permission.ReadMediaImages]
                : [Manifest.Permission.ReadExternalStorage],
            requestCode: 0
        );

        // TODO: костыль, мб по CurrentActivity(requestCode: ) что-то есть для ожидания
        DateTime startTime = DateTime.UtcNow;

        while (DateTime.UtcNow - startTime < TimeSpan.FromSeconds(10))
        {
            granted = await CheckPermissions();
            if (granted) return true;

            await Task.Delay(500);
        }

        return false;
#else
        throw new PlatformNotSupportedException("К сожалению доступно только для Android");
#endif
    }

    public static LocalLoadStatus Status { get; set; } = LocalLoadStatus.NotLoad;
    public static ObservableCollection<IPictureLocal> Pictures { get; } = [];
    public static ObservableCollection<IAlbumLocal>   Albums   { get; } = [];

    public static async Task FillPictures()
    {
        Pictures.Clear();
        Albums.Clear();
        System.Diagnostics.Debug.WriteLine("=========== START FillPictures ============");
#if ANDROID
        Status = LocalLoadStatus.InLoad;

        bool granted = await CheckPermissions();
        if (!granted)
        {
            Status = LocalLoadStatus.NoPermissions;
        }

        var contentResolver = Android.App.Application.Context.ContentResolver;

        string[] projection = 
        [
            Android.Provider.MediaStore.Images.Media.InterfaceConsts.Data,
            Android.Provider.MediaStore.Images.Media.InterfaceConsts.Size,
            Android.Provider.MediaStore.Images.Media.InterfaceConsts.Width,
            Android.Provider.MediaStore.Images.Media.InterfaceConsts.Height,
            Android.Provider.MediaStore.Images.Media.InterfaceConsts.DateModified,
        ];
        var sortOrder = $"{projection[4]} DESC";

        var uri = Android.Provider.MediaStore.Images.Media.ExternalContentUri;
        if (uri == null)
        {
#if DEBUG
            _ = Shell.Current.DisplayAlert("DEBUG ERROR", $"Ошибка получение картинок\nuri пустой", "OK");
#endif
            return;
        }
        System.Diagnostics.Debug.WriteLine($"URI: " + uri.ToString());

        var cursor = contentResolver?.Query(uri, projection, null, null, sortOrder);
        if (cursor == null)
        {
#if DEBUG
            _ = Shell.Current.DisplayAlert("DEBUG ERROR", $"Ошибка получение картинок\ncursor пустой", "OK");
#endif
            return;
        }

        try
        {
            int   dataColumn = cursor.GetColumnIndexOrThrow(projection[0]);
            int   sizeColumn = cursor.GetColumnIndexOrThrow(projection[1]);
            int  widthColumn = cursor.GetColumnIndexOrThrow(projection[2]);
            int heightColumn = cursor.GetColumnIndexOrThrow(projection[3]);
            int   dateColumn = cursor.GetColumnIndexOrThrow(projection[4]);

            /*
            await Task.Run(() =>
            {
                // Проход по картинкам
                while (cursor.MoveToNext())
                {
                    var picturePath = cursor.GetString(dataColumn);
                    if (picturePath == null) continue;

                    var albumPath = Path.GetDirectoryName(picturePath);
                    if (albumPath == null) continue;

                    // Альбом
                    IAlbumLocal? album;
                    bool isInSync = false;

                    // Ищем в списке-переменной есть ли альбом уже
                    album = Albums.FirstOrDefault(a => a.LocalPath == albumPath);
                    if (album == null)
                    {
                        // Ищем в локальной БД есть ли альбом как синхронизирующийся
                        album = DB.Table<AlbumSynced>().FirstOrDefault(a => a.LocalPath == albumPath);

                        isInSync = album != null;

                        // Создаём новый объект альбома если нигде нет ещё
                        if (!isInSync)
                            album = new AlbumLocal(albumPath);

                        // TODO: помечать скрытыми альбомы
                        // TODO: задавать установленные алиасы имени

                        // Ищем дубликат по имени
                        var duplica = Albums
                            .Where(d => d.Name == album.Name)
                            .OrderByDescending(d => d.NameDuplicaIndex)
                            .FirstOrDefault();

                        // Назначаем индекс если новый альбом это дубликат
                        if (duplica != null)
                            album.NameDuplicaIndex = (duplica.NameDuplicaIndex ??= 1) + 1;

                        // Добавляем альбом к общему списку локальных альбомов
                        Albums.Add(album);
                    }
                    else
                    {
                        isInSync = album is AlbumSynced;
                    }

                    // Картинка
                    IPictureLocal? picture = null;

                    // Ищем в локальной БД есть ли картинка как синхронизированная
                    if (isInSync)
                        picture = DB.Table<PictureSynced>().FirstOrDefault(p => p.LocalPath == picturePath);

                    // Создаём новый объект альбома если в БД нет, т.е. ещё не синхронизирована
                    picture ??= new PictureLocal()
                    {
                        LocalPath = picturePath,
                        Size = (ulong)cursor.GetLong(sizeColumn),
                        Width = cursor.GetInt(widthColumn),
                        Height = cursor.GetInt(heightColumn),
                        Date = DateTimeOffset.FromUnixTimeSeconds(cursor.GetLong(dateColumn)).DateTime,
                        Album = album,
                    };

                    // Добавляем картинку в список локальных картинок альбома
                    album.LocalPictures.Add((PictureLocal)picture);

                    // Добавляем картинку к общему списку локальных картинок
                    Pictures.Add(picture);
                }
            });
            */
            await Task.Run(() =>
            {
                // Создаем список для хранения времени выполнения
                var executionTimes = new List<Dictionary<string, long>>();

                int iterationIndex = 0; // Индекс итерации

                // Проход по картинкам
                while (true)
                {
                    // Словарь для хранения времени выполнения функций в текущей итерации
                    var iterationTimes = new Dictionary<string, long>
                    {
                        { "IterationIndex", iterationIndex } // Добавляем индекс итерации
                    };

                    // Замер времени для целой итерации
                    var iterationStopwatch = Stopwatch.StartNew();

                    var stopwatch = Stopwatch.StartNew();
                    bool hasNext = cursor.MoveToNext();
                    iterationTimes["cursor.MoveToNext()"] = stopwatch.ElapsedTicks * (1_000_000_000 / Stopwatch.Frequency); 

                    if (!hasNext) break; // Выход из цикла, если больше нет элементов

                    var picturePath = cursor.GetString(dataColumn);
                    if (picturePath == null) continue;

                    var albumPath = Path.GetDirectoryName(picturePath);
                    if (albumPath == null) continue;

                    // Альбом
                    IAlbumLocal? album;
                    bool isInSync = false;

                    // Ищем в списке-переменной есть ли альбом уже
                    stopwatch.Restart();
                    album = Albums.FirstOrDefault(a => a.LocalPath == albumPath);
                    iterationTimes["Albums.FirstOrDefault"] = stopwatch.ElapsedTicks * (1_000_000_000 / Stopwatch.Frequency);

                    if (album == null)
                    {
                        // Ищем в локальной БД есть ли альбом как синхронизирующийся
                        stopwatch.Restart();
                        album = DB.Table<AlbumSynced>().FirstOrDefault(a => a.LocalPath == albumPath);
                        System.Diagnostics.Debug.WriteLine($"FillPictures: DB.Table<AlbumSynced>().FirstOrDefault: {((album is AlbumSynced synced) ? synced.Id : string.Empty)}");
                        iterationTimes["DB.Table<AlbumSynced>.FirstOrDefault"] = stopwatch.ElapsedTicks * (1_000_000_000 / Stopwatch.Frequency);

                        isInSync = album != null;

                        // Создаём новый объект альбома если нигде нет ещё
                        if (!isInSync)
                            album = new AlbumLocal(albumPath);

                        // Ищем дубликат по имени
                        stopwatch.Restart();
                        var duplica = Albums
                            .Where(d => d.Name == album.Name)
                            .OrderByDescending(d => d.NameDuplicaIndex)
                            .FirstOrDefault();
                        iterationTimes["Albums.Where.OrderByDescending.FirstOrDefault"] = stopwatch.ElapsedTicks * (1_000_000_000 / Stopwatch.Frequency);

                        // Назначаем индекс если новый альбом это дубликат
                        if (duplica != null)
                            album.NameDuplicaIndex = (duplica.NameDuplicaIndex ??= 1) + 1;

                        // Добавляем альбом к общему списку локальных альбомов
                        stopwatch.Restart();
                        Albums.Add(album);
                        iterationTimes["Albums.Add"] = stopwatch.ElapsedTicks * (1_000_000_000 / Stopwatch.Frequency);
                    }
                    else
                    {
                        isInSync = album is AlbumSynced;
                    }

                    // Картинка
                    IPictureLocal? picture = null;

                    // Ищем в локальной БД есть ли картинка как синхронизированная
                    if (isInSync)
                    {
                        stopwatch.Restart();
                        picture = DB.Table<PictureSynced>().FirstOrDefault(p => p.LocalPath == picturePath);
                        iterationTimes["DB.Table<PictureSynced>.FirstOrDefault"] = stopwatch.ElapsedTicks * (1_000_000_000 / Stopwatch.Frequency);
                    }

                    // Создаём новый объект альбома если в БД нет, т.е. ещё не синхронизирована
                    if (picture == null)
                    {
                        stopwatch.Restart();
                        picture = new PictureLocal()
                        {
                            LocalPath = picturePath,
                            Size = (ulong)cursor.GetLong(sizeColumn),
                            Width = cursor.GetInt(widthColumn),
                            Height = cursor.GetInt(heightColumn),
                            Date = DateTimeOffset.FromUnixTimeSeconds(cursor.GetLong(dateColumn)).DateTime,
                            Album = album,
                        };
                        iterationTimes["PictureLocal Constructor"] = stopwatch.ElapsedTicks * (1_000_000_000 / Stopwatch.Frequency);
                    }

                    // Добавляем картинку в список локальных картинок альбома
                    stopwatch.Restart();
                    album.LocalPictures.Add((PictureLocal)picture);
                    iterationTimes["album.LocalPictures.Add"] = stopwatch.ElapsedTicks * (1_000_000_000 / Stopwatch.Frequency);

                    // Добавляем картинку к общему списку локальных картинок
                    stopwatch.Restart();
                    Pictures.Add(picture);
                    iterationTimes["Pictures.Add"] = stopwatch.ElapsedTicks * (1_000_000_000 / Stopwatch.Frequency);

                    // Завершаем замер времени для целой итерации
                    iterationStopwatch.Stop();
                    iterationTimes["Whole Iteration"] = iterationStopwatch.ElapsedTicks * (1_000_000_000 / Stopwatch.Frequency);

                    // Добавляем данные текущей итерации в общий список
                    executionTimes.Add(iterationTimes);

                    // Увеличиваем индекс итерации
                    iterationIndex++;
                }

                var csvContent = new StringBuilder();

                // Заголовок CSV
                var headers = new List<string> { "IterationIndex" };
                headers.AddRange(executionTimes.SelectMany(x => x.Keys).Distinct().Where(k => k != "IterationIndex"));
                csvContent.AppendLine(string.Join(';', headers));

                // Контент CSV
                foreach (var iteration in executionTimes)
                {
                    var row = new List<string> { iteration["IterationIndex"].ToString() };
                    foreach (var header in headers.Skip(1))
                    {
                        row.Add(iteration.ContainsKey(header) ? iteration[header].ToString() : "0");
                    }
                    csvContent.AppendLine(string.Join(';', row));
                }

                System.Diagnostics.Debug.WriteLine("Execution times:\n" + csvContent.ToString());
            });
        }
        catch (Exception ex)
        {
#if DEBUG
            _ = Shell.Current.DisplayAlert("DEBUG ERROR", $"Ошибка получение картинок\n{ex.Message}", "OK");
            throw ex;
#endif
        }
        finally
        {
            cursor.Close();
        }

        Status = LocalLoadStatus.Loaded;
        System.Diagnostics.Debug.WriteLine("=========== END FillPictures ============");
#else
        throw new PlatformNotSupportedException("К сожалению доступно только для Android");
#endif
    }
}