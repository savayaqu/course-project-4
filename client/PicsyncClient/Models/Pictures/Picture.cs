using PicsyncClient.Converters.Json;
using System.Text.Json.Serialization;
using PicsyncClient.Models.Albums;

namespace PicsyncClient.Models.Pictures;

public class Picture
{
    [JsonPropertyName("id"        )] public ulong?      Id          { get; set; }
    [JsonPropertyName("name"      )] public string?     Name        { get; set; }
    [JsonPropertyName("hash"      )] public string?     Hash        { get; set; }
    [JsonPropertyName("size"      )] public ulong?      Size        { get; set; }
    
    [JsonPropertyName("date")]
    [JsonConverter(typeof(UniversalDateTimeConverter))]
    public DateTime? Date { get; set; }

    //public DateTime? Date { get; set; } = null;
    [JsonPropertyName("width"     )] public int?        Width       { get; set; }
    [JsonPropertyName("height"    )] public int?        Height      { get; set; }
    [JsonPropertyName("uploadedAt")] public DateTime?   UploadedAt  { get; set; }

    public Album Album { get; set; } // TODO: мб попробовать сделать все ссылки от родительского альбома, а не в ручную заполнять?
    
    public string? LocalPath       { get; set; }
    public string? RemotePath      { get; set; }
    public string? RemoteThumbnail { get; set; }

    public string? GetLocalOrRemotePath      => LocalPath ?? RemotePath;
    public string? GetLocalOrRemoteThumbnail => LocalPath ?? RemoteThumbnail;
}