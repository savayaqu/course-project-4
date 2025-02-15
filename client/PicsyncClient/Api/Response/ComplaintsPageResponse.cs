using PicsyncClient.Models.Albums;
using PicsyncClient.Models.Pictures;
using System.Text.Json.Serialization;

namespace PicsyncClient.Models.Response;

public class ComplaintsPageResponse
{
    [JsonPropertyName("page"  )] public int? Page      { get; set; }
    [JsonPropertyName("limit" )] public int? Limit     { get; set; }
    [JsonPropertyName("total" )] public int  Total     { get; set; }
    [JsonPropertyName("albums")] public List<AlbumRemoteWithComplaints> Albums { get; set; } = [];
}

public class AlbumRemoteWithComplaints : AlbumRemote
{
    
}