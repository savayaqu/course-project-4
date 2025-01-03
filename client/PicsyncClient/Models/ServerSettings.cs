using PicsyncClient.Enum;
using System.Text.Json.Serialization;

namespace PicsyncClient.Models;

public class ServerSettings
{
    [JsonPropertyName("allowed_upload_mimes" )] public List<string>         AllowedUploadMimes  { get; set; } = ["png", "jpg", "jpeg"];
    [JsonPropertyName("allowed_preview_sizes")] public List<int>            AllowedPreviewSizes { get; set; } = [144, 240, 360, 480, 720, 1080];
    [JsonPropertyName("warning_limit_for_ban")] public int                  WarningLimitForBan  { get; set; } = 0;
    [JsonPropertyName("free_storage_limit"   )] public ulong?               FreeStorageLimit    { get; set; }
    [JsonPropertyName("complaint_types"      )] public List<ComplaintType>  ComplaintTypes      { get; set; } = [];
    [JsonPropertyName("is_upload_disabled"   )] public bool                 IsUploadDisabled    { get; set; } = false;

    public DateTime GotAt { get; set; }
}