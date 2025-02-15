using System.Text.Json.Serialization;

namespace PicsyncClient.Models.Response;

public class UserResponse
{
    [JsonPropertyName("user" )] public UserWithStats User  { get; set; }
    [JsonPropertyName("quota")] public SpaceQuota    Quota { get; set; }
}

public class UserWithStats : User
{
    [JsonPropertyName("warnings"                   )] public List<Warning> Warnings          { get; set; } = [];
    [JsonPropertyName("complaintsFromCount"        )] public int ComplaintsFromCount         { get; set; } = 0;
    [JsonPropertyName("complaintsFromAcceptedCount")] public int ComplaintsFromAcceptedCount { get; set; } = 0;
    [JsonPropertyName("albumsCount"                )] public int AlbumsOwnCount              { get; set; } = 0;
    [JsonPropertyName("albumsViaAccessCount"       )] public int AlbumsAccessibleCount       { get; set; } = 0;
    [JsonPropertyName("picturesCount"              )] public int PicturesCount               { get; set; } = 0;
    [JsonPropertyName("tagsCount"                  )] public int TagsCount                   { get; set; } = 0;
}

public class SpaceQuota
{
    [JsonPropertyName("total")] public ulong Total { get; set; }
    [JsonPropertyName("used" )] public ulong Used  { get; set; }
    [JsonPropertyName("free" )] public ulong Free  { get; set; }
}