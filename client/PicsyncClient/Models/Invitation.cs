using PicsyncClient.Converters.Json;
using System.Text.Json.Serialization;

namespace PicsyncClient.Models;

public class Invitation
{
    [JsonPropertyName("code")] 
    public required string Code { get; set; }

    [JsonPropertyName("expiresAt")]
    [JsonConverter(typeof(UniversalDateTimeConverter))]
    public DateTime? ExpiresAt { get; set; }

    [JsonPropertyName("joinLimit")] 
    public int? JoinLimit { get; set; }
}