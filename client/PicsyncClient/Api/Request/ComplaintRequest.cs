using System.Text.Json.Serialization;

namespace PicsyncClient.Models.Request;

public class ComplaintRequest(ComplaintType type, string description)
{
    [JsonPropertyName("typeId"     )] public ulong  TypeId      { get; set; } = type.Id;
    [JsonPropertyName("description")] public string Description { get; set; } = description;
}
