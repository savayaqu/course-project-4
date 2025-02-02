using PicsyncClient.Models.Pictures;
using PicsyncClient.Utils;
using SQLite;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace PicsyncClient.Models.Albums;

public class AlbumSynced : AlbumRemote, IAlbumLocal
{
    // Конструкторы
    [SetsRequiredMembers]
    public AlbumSynced(IAlbumLocal local, AlbumRemote remote)
    {
        Id        = remote.Id;
        Name      = remote.Name;
        CreatedAt = remote.CreatedAt;

        LocalPath        = local.LocalPath;
        LocalPictures    = local.LocalPictures;
        NameDuplicaIndex = local.NameDuplicaIndex;
    }

    public AlbumSynced() : base() { }

    // Свойства
    [Ignore]
    public int? NameDuplicaIndex { get; set; } = null;

    [Ignore]
    public List<IPictureLocal> LocalPictures { get; set; } = [];

    public string LocalPath { get; set; }

    // Геттеры
    public int TrueRemotePicturesCount => RemotePicturesCount - SyncedPicturesCount;
    public int  TrueLocalPicturesCount => LocalPictures.OfType<PictureLocal> ().Count();
    public int     SyncedPicturesCount => LocalPictures.OfType<PictureSynced>().Count();
    public override int  PicturesCount => TrueLocalPicturesCount + RemotePicturesCount;

    private List<string>? _thumbnailPaths = null;
    public override List<string> ThumbnailPaths
    {
        // Ленивое получение
        get
        {
            if (_thumbnailPaths != null)
                return _thumbnailPaths;

            // Инициализация
            _thumbnailPaths = [];

            // TODO: переделать API чтобы возвращал картинки с датами, чтобы вернуть труъ последние
            if (Preview is PictureAlbumPreview preview && preview.PictureIds.Count > 0)
            {
                foreach (var pictureId in preview.PictureIds)
                    _thumbnailPaths.Add(URLs.PictureThumbnail(Id, pictureId, preview.Signature).ToString());
            }


            byte i = 0;
            foreach (var picture in LocalPictures)
            {
                if (i > 4) break;

                i++;
                if (picture.LocalPath is string path)
                    _thumbnailPaths.Add(path);
            }
            return _thumbnailPaths;
        }
    }
}

