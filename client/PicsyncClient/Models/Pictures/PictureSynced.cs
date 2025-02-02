using PicsyncClient.Converters.Json;
using PicsyncClient.Models.Albums;
using PicsyncClient.Utils;
using SQLite;
using System.Text.Json.Serialization;

namespace PicsyncClient.Models.Pictures;

public class PictureSynced : PictureRemote, IPictureLocal
{
    // Конструкторы
    public PictureSynced(IPictureLocal localPicture, PictureRemote remotePicture, AlbumSynced syncedAlbum)
    {
        LocalPath   = localPicture.LocalPath;

        Id          = remotePicture.Id;
        Name        = remotePicture.Name;
        Hash        = remotePicture.Hash;
        Size        = remotePicture.Size;
        Width       = remotePicture.Width;
        Height      = remotePicture.Height;
        UploadedAt  = remotePicture.UploadedAt;
        Date        = remotePicture.Date;

        Album = syncedAlbum;
    }

    public PictureSynced() { }

    // Свойства
    public string LocalPath { get; set; }

    [Ignore]
    [JsonIgnore]
    public new AlbumSynced SpecificAlbum
    {
        get => (AlbumSynced)Album;
        set => Album = value;
    }

    // Геттеры
    public override string OriginalPath  => LocalPath ?? URLs.PictureOriginal (SpecificAlbum.Id, Id, SpecificAlbum?.Preview?.Signature).ToString();
    public override string ThumbnailPath => LocalPath ?? URLs.PictureThumbnail(SpecificAlbum.Id, Id, SpecificAlbum?.Preview?.Signature).ToString();

    // Функции
    public void Update(PictureRemote remote)
    {
        Id          = remote.Id;
        Name        = remote.Name;
        Hash        = remote.Hash;
        Size        = remote.Size;
        Width       = remote.Width;
        Height      = remote.Height;
        UploadedAt  = remote.UploadedAt;
        Date        = remote.Date;
    }
    public void Update(IPictureLocal local)
    {
        LocalPath = local.LocalPath;
    }
}
