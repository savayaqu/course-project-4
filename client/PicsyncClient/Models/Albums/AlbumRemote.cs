using PicsyncClient.Utils;
using SQLite;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace PicsyncClient.Models.Albums;

public class AlbumRemote : AlbumBase
{
    // Конструкторы
    [SetsRequiredMembers]
    public AlbumRemote(ulong id, string name)
    {
        Id = id;
        Name = name;
    }

    public AlbumRemote() : base() { }

    // Свойства
  //[JsonPropertyName("path"              )] public string?   Path          { get; set; } // TODO: CLEAN: мб устарело
    [JsonPropertyName("id")]    [PrimaryKey] public ulong Id                { get; set; }
    [JsonPropertyName("name"              )] public override string Name    { get; set; }
    [JsonPropertyName("createdAt"         )] public DateTime? CreatedAt     { get; set; }
    [JsonPropertyName("picturesCount"     )] public int RemotePicturesCount { get; set; } = 0;

    [Ignore]
    [JsonPropertyName("grantAccesses")]
    public List<User>? GrantAccesses { get; set; }

    private int _grantAccessesCount = 0;
    [JsonPropertyName("grantAccessesCount")] 
    public int GrantAccessesCount 
    {
        get => GrantAccesses is null ? _grantAccessesCount : GrantAccesses.Count;
        set => _grantAccessesCount = value; 
    }

    [Ignore]
    [JsonPropertyName("invitations")] 
    public List<Invitation>? Invitations { get; set; }

    private int _invitationsCount = 0;
    [JsonPropertyName("invitationsCount")] 
    public int InvitationsCount
    {
        get => Invitations is null ? _invitationsCount : Invitations.Count;
        set => _invitationsCount = value;
    }

    [Ignore]
    [JsonPropertyName("owner")] 
    public User? Owner { get; set; }

    [Ignore]
    [JsonPropertyName("picturesInfo")] 
    public PictureAlbumPreview? Preview { get; set; }

    // Геттеры
    public override int PicturesCount => RemotePicturesCount;

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

            if (Preview is PictureAlbumPreview preview && preview.PictureIds.Count > 0) 
            {
                // TODO: читать какой сейчас размер колонок для получение удалённых превью без лишних затрат инет-трафика
                foreach (var pictureId in preview.PictureIds)
                    _thumbnailPaths.Add(URLs.PictureThumbnail(Id, pictureId, preview.Signature).ToString());
            }

            return _thumbnailPaths;
        }
    }

    // Функции
    public void Update(AlbumRemote remoteAlbum)
    {
        Id                  = remoteAlbum.Id;
        Name                = remoteAlbum.Name;
        CreatedAt           = remoteAlbum.CreatedAt;
        RemotePicturesCount = remoteAlbum.RemotePicturesCount;
        GrantAccesses       = remoteAlbum.GrantAccesses;
        GrantAccessesCount  = remoteAlbum.GrantAccessesCount;
        Invitations         = remoteAlbum.Invitations;
        InvitationsCount    = remoteAlbum.InvitationsCount;
        Owner               = remoteAlbum.Owner;
        Preview             = remoteAlbum.Preview;
    }
}
