using PicsyncClient.Utils;
using System.Text.Json.Serialization;

namespace PicsyncClient.Models;

public class Album
{
    [JsonPropertyName("id"           )] public required ulong Id             { get; set; }
    [JsonPropertyName("name"         )] public string?        Name           { get; set; }
    [JsonPropertyName("path"         )] public string?        Path           { get; set; }
    [JsonPropertyName("createdAt"    )] public DateTime?      CreatedAt       { get; set; }
    [JsonPropertyName("picturesCount")] public ulong?         PicturesCount  { get; set; }
    [JsonPropertyName("picturesInfo" )] public AlbumPreview?  Preview        { get; set; }

    public void FillPreview()
    {
        foreach (var pictureId in Preview?.PictureIds ?? [])
        {
            Preview.Pictures.Add(new() 
            { 
                Id = pictureId, 
                Thumbnail = URLs.Thumbnail(Id, pictureId, Preview.Signature) 
            });
        }
    }
}

public class AlbumPreview
{
    [JsonPropertyName("sign")] public required string Signature   { get; set; }
    [JsonPropertyName("ids" )] public List<ulong>     PictureIds  { get; set; } = [];

    public List<Picture> Pictures { get; set; } = [];
}
