using System.Text.Json.Serialization;

namespace PicsyncClient.Models.Response;

public class PageResponse<T>
{
    [JsonPropertyName("page" )] public int     Page  { get; set; }
    [JsonPropertyName("limit")] public int     Limit { get; set; }
    [JsonPropertyName("total")] public int     Total { get; set; }
    [JsonPropertyName("data" )] public List<T> Data  { get; set; } = [];
}