using PicsyncClient.Converters.Json;
using System.Text.Json.Serialization;

namespace PicsyncClient.Models;

public class Warning
{
    [JsonPropertyName("id"     )] public ulong Id { get; set; }
    [JsonPropertyName("comment")] public string Comment { get; set; }
}