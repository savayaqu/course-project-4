using PicsyncClient.Converters.Json;
using PicsyncClient.Models.Albums;
using PicsyncClient.Utils;
using SQLite;
using System.Text.Json.Serialization;

namespace PicsyncClient.Models.Pictures;

public class PictureSynced : PictureRemote, IPictureLocal
{
    // Конструкторы
    public PictureSynced() { }

    // Свойства
    public string LocalPath { get; set; }

    [JsonIgnore]
    [Ignore]
    public new AlbumSynced SpecificAlbum
    {
        get => (AlbumSynced)Album;
        set => Album = value;
    }

    // Геттеры
    public override string OriginalPath  => LocalPath ?? URLs.PictureOriginal (SpecificAlbum.Id, Id, SpecificAlbum?.Preview?.Signature).ToString();
    public override string ThumbnailPath => LocalPath ?? URLs.PictureThumbnail(SpecificAlbum.Id, Id, SpecificAlbum?.Preview?.Signature).ToString();

    // Функции
    public void Update(PictureRemote remotePicture)
    {
        Id          = remotePicture.Id;
        Name        = remotePicture.Name;
        Hash        = remotePicture.Hash;
        Size        = remotePicture.Size;
        Width       = remotePicture.Width;
        Height      = remotePicture.Height;
        UploadedAt  = remotePicture.UploadedAt;
        Date        = remotePicture.Date;
    }
}
