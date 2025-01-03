using PicsyncClient.Enum;
using PicsyncClient.Models;
using PicsyncClient.Models.Albums;
using PicsyncClient.Models.Pictures;
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
        if (await CheckPermissions()) return true;
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
            if (await CheckPermissions()) return true;

            await Task.Delay(500);
        }

        return false;
#else
        throw new PlatformNotSupportedException("К сожалению доступно только для Android");
#endif
    }

    public static LocalLoadStatus    Status   { get; set; } = LocalLoadStatus.NotLoad;
    public static List<PictureLocal> Pictures { get; set; } = [];
    public static List<AlbumLocal>   Albums   { get; set; } = [];

    public static async Task FillPictures()
    {
        System.Diagnostics.Debug.WriteLine("=========== START FillPictures ============");
#if ANDROID
        Status = LocalLoadStatus.InLoad;

        if (!await CheckPermissions())
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

        var cursor = contentResolver?.Query(uri, projection, null, null, sortOrder);

        if (cursor == null) return;

        try
        {
            int   dataColumn = cursor.GetColumnIndexOrThrow(projection[0]);
            int   sizeColumn = cursor.GetColumnIndexOrThrow(projection[1]);
            int  widthColumn = cursor.GetColumnIndexOrThrow(projection[2]);
            int heightColumn = cursor.GetColumnIndexOrThrow(projection[3]);
            int   dateColumn = cursor.GetColumnIndexOrThrow(projection[4]);

            while (cursor.MoveToNext())
            {
                string picturePath = cursor.GetString(dataColumn);
                if (picturePath == null) continue;

                var albumPath = Path.GetDirectoryName(picturePath);
                if (albumPath == null) continue;

                PictureLocal picture;
                var album = Albums.Find(a => a.LocalPath == albumPath);
                if (album != null)
                {
                    picture = new()
                    {
                        LocalPath = picturePath,
                        Size = (ulong)cursor.GetLong(sizeColumn),
                        Width = cursor.GetInt(widthColumn),
                        Height = cursor.GetInt(heightColumn),
                        Date = DateTimeOffset.FromUnixTimeSeconds(cursor.GetLong(dateColumn)).DateTime,
                        Album = album,
                    };
                }
                else
                {
                    album = new()
                    {
                        Name = Path.GetFileName(albumPath),
                        LocalPath = albumPath,
                    };

                    picture = new()
                    {
                        LocalPath = picturePath,
                        Size = (ulong)cursor.GetLong(sizeColumn),
                        Width = cursor.GetInt(widthColumn),
                        Height = cursor.GetInt(heightColumn),
                        Date = DateTimeOffset.FromUnixTimeSeconds(cursor.GetLong(dateColumn)).DateTime,
                        Album = album,
                    };

                    var duplica = Albums
                        .Where(d => d.Name == album.Name)
                        .OrderByDescending(d => d.NameDuplicaIndex)
                        .FirstOrDefault();

                    if (duplica != null)
                        album.NameDuplicaIndex = (duplica.NameDuplicaIndex ??= 1) + 1;

                    Albums.Add(album);
                }
                // TODO: помечать скрытыми альбомы
                // TODO: задавать установленные алиасы имени

                album.LocalPictures.Add(picture);

                Pictures.Add(picture);
            }
        }
        catch (Exception ex)
        {
#if DEBUG
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