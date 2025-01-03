using System.Text.Json.Serialization;

namespace PicsyncClient.Models;

public class ComplaintType
{
    [JsonPropertyName("id"  )] public ulong  Id   { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; }
}