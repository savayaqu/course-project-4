using PicsyncClient.Utils;
using SQLite;
using System.Text.Json.Serialization;

namespace PicsyncClient.Models.Albums;

public class AlbumRemote : AlbumBase
{
    // ��������
  //[JsonPropertyName("path"              )] public string?   Path                { get; set; } // TODO: CLEAN: �� ��������
    [JsonPropertyName("id"                )] public required ulong Id             { get; set; }
    [JsonPropertyName("name"              )] public override string? Name { get; set; }
    [JsonPropertyName("createdAt"         )] public DateTime? CreatedAt           { get; set; }
    [JsonPropertyName("picturesCount"     )] public int       RemotePicturesCount { get; set; } = 0;
    [JsonPropertyName("grantAccessesCount")] public int       GrantAccessesCount  { get; set; } = 0;
    [JsonPropertyName("invitationsCount"  )] public int       InvitationsCount    { get; set; } = 0;
    [JsonPropertyName("owner"             )] public User?     Owner               { get; set; }
    [JsonPropertyName("picturesInfo"      )] public PictureAlbumPreview? Preview  { get; set; }

    // �������
    public override int PicturesCount => RemotePicturesCount;

    private List<string>? _thumbnailPaths = null;
    public override List<string> ThumbnailPaths
    {
        get
        {
            if (_thumbnailPaths != null) 
                return _thumbnailPaths;

            _thumbnailPaths = [];

            // TODO: ���������� API ����� ��������� �������� � ������, ����� ������� ���� ���������

            if (Preview is PictureAlbumPreview preview && preview.PictureIds.Count > 0) 
            {
                // TODO: ������ ����� ������ ������ ������� ��� ��������� �������� ������ ��� ������ ������ ����-�������
                foreach (var pictureId in preview.PictureIds)
                    _thumbnailPaths.Add(URLs.PictureThumbnail(Id, pictureId, preview.Signature).ToString());
            }

            return _thumbnailPaths;
        }
    }
}
