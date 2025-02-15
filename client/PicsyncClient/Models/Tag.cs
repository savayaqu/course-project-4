using PicsyncClient.Converters.Json;
using System.Text.Json.Serialization;

namespace PicsyncClient.Models;

public class Tag
{
    [JsonPropertyName("id"   )] public ulong  Id   { get; set; }
    [JsonPropertyName("value")] public string Name { get; set; }
}