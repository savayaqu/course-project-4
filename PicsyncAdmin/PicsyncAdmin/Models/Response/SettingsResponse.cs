using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PicsyncAdmin.Models.Response
{
    public class ApiResponse
    {
        [JsonPropertyName("settings")] public Settings? Settings { get; set; }
        [JsonPropertyName("space")] public Space? Space { get; set; }
    }

    public class Settings
    {
        [JsonPropertyName("upload_disable_percentage")] public int UploadDisablePercentage { get; set; }
    }

    public class Space
    {
        [JsonPropertyName("total")] public long Total { get; set; }
        [JsonPropertyName("free")] public long Free { get; set; }
        [JsonPropertyName("used")] public long Used { get; set; }
        [JsonPropertyName("usedPercent")] public int UsedPercent { get; set; }
    }
}
