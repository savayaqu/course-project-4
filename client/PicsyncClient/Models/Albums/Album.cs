using PicsyncClient.Utils;
using SQLite;
using System.Text.Json.Serialization;
using PicsyncClient.Models.Pictures;

namespace PicsyncClient.Models.Albums;

public class Album
{
    // Свойства, которые могут быть получены с API
    // TODO: отдельный класс с наследованием?
    [JsonPropertyName("id")]    [PrimaryKey] public ulong?         Id                   { get; set; }
    [JsonPropertyName("name"              )] public string?        Name                 { get; set; }
  //[JsonPropertyName("path"              )] public string?        Path                 { get; set; } // TODO: CLEAN: мб устарело
    [JsonPropertyName("createdAt"         )] public DateTime?      CreatedAt            { get; set; }
    [JsonPropertyName("picturesCount"     )] public int            RemotePicturesCount  { get; set; } = 0;
    [JsonPropertyName("grantAccessesCount")] public int            GrantAccessesCount   { get; set; } = 0;
    [JsonPropertyName("invitationsCount"  )] public int            InvitationsCount     { get; set; } = 0;
    [JsonPropertyName("owner"             )] public User?          Owner                { get; set; }
    [JsonPropertyName("picturesInfo"      )] public PictureAlbumPreview?  Preview              { get; set; }

    // Свойства, которые могут получены локально
    // TODO: отдельный класс с наследованием?
    [Ignore] public int? NameDuplicaIndex { get; set; } = null;
    public List<Picture> LocalPictures    { get; set; } = [];
    public string?       LocalPath        { get; set; }


    // Геттеры
    public bool IsRemote => Id        != null;
    public bool IsLocal  => LocalPath != null;
    public bool IsSynced => IsRemote && IsLocal;

    public int TotalPicturesCount => (LocalPictures?.Count ?? 0) + RemotePicturesCount; // TODO: бред

    private List<string>? _thumbnailPaths = null;
    public List<string> ThumbnailPaths
    {
        get
        {
            if (_thumbnailPaths != null) 
                return _thumbnailPaths;

            _thumbnailPaths = [];

            // TODO: переделать API чтобы возвращал картинки с датами, чтобы вернуть труъ последние

            if (Id is ulong albumId && 
                Preview is PictureAlbumPreview preview && 
                preview.PictureIds.Count > 0
            ) {
                // TODO: читать какой сейчас размер колонок для получение удалённых превью без лишних затрат инет-трафика
                foreach (var pictureId in preview.PictureIds)
                    _thumbnailPaths.Add(URLs.PictureThumbnail(albumId, pictureId, preview.Signature).ToString());
            } else {
                byte i = 0;
                foreach (var picture in LocalPictures)
                {
                    if (i++ > 2) break; // TODO: FIXME: 2 -> 4

                    if (picture.LocalPath is string path)
                        _thumbnailPaths.Add(path);
                }
            }

            return _thumbnailPaths;
        }
    }
}

//public class AlbumPreview
//{
//    [JsonPropertyName("sign")] public required string Signature   { get; set; }
//    [JsonPropertyName("ids" )] public List<ulong>     PictureIds  { get; set; } = [];

//    public List<Picture> Pictures { get; set; } = [];
//}
