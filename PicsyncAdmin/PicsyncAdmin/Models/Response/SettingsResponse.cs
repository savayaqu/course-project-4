using System.Text.Json.Serialization;

namespace PicsyncAdmin.Models.Response
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    namespace PicsyncAdmin.Models.Response
    {
        public class SettingsResponse
        {
            [JsonPropertyName("settings")]
            public Settings Settings { get; set; } = new();

            [JsonPropertyName("space")]
            public Space Space { get; set; } = new();
        }

        public class Settings
        {
            [JsonPropertyName("allowed_upload_mimes")]
            public List<string> AllowedUploadMimes { get; set; } = new();

            [JsonPropertyName("allowed_preview_sizes")]
            public List<int> AllowedPreviewSizes { get; set; } = new();

            [JsonPropertyName("warning_limit_for_ban")]
            public int WarningLimitForBan { get; set; }

            [JsonPropertyName("free_storage_limit")]
            public long FreeStorageLimit { get; set; }

            [JsonPropertyName("upload_disable_percentage")]
            public int UploadDisablePercentage { get; set; }
        }

        public class Space
        {
            [JsonPropertyName("total")]
            public long Total { get; set; }

            [JsonPropertyName("free")]
            public long Free { get; set; }

            [JsonPropertyName("used")]
            public long Used { get; set; }

            [JsonPropertyName("usedPercent")]
            public int UsedPercent { get; set; }

            [JsonPropertyName("gotAt")]
            public DateTime GotAt { get; set; }
        }
    }

}
