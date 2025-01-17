using PicsyncClient.Models.Albums;
using System.Text.Json.Serialization;

namespace PicsyncClient.Models.Response;

public class InvitationResponse
{
    [JsonPropertyName("invitation")] public Invitation Invitation { get; set; }
}