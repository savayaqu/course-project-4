using System.Text.Json.Serialization;

namespace PicsyncClient.Models.Request;

public class InvationCreateRequest
{
    public InvationCreateRequest(int? joinLimit, int? timeLimit)
    {
        JoinLimit = joinLimit;
        TimeLimit = timeLimit;
    }

    public InvationCreateRequest(int? joinLimit, DateTime? expiresAt)
    {
        JoinLimit = joinLimit;
        ExpiresAt = expiresAt;
    }

    [JsonPropertyName("joinLimit")] public int?      JoinLimit { get; set; }
    [JsonPropertyName("timeLimit")] public int?      TimeLimit { get; set; }
    [JsonPropertyName("expiresAt")] public DateTime? ExpiresAt { get; set; }
}
