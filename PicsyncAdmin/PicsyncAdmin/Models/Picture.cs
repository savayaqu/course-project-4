using PicsyncAdmin.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PicsyncAdmin.Models
{
    public class Picture
    {
        [JsonPropertyName("id")] public int Id { get; set; }
        [JsonPropertyName("name")] public string? Name { get; set; }
        [JsonPropertyName("hash")] public string? Hash { get; set; }
        [JsonPropertyName("size")] public int Size { get; set; }

        [JsonPropertyName("date")][System.Text.Json.Serialization.JsonConverter(typeof(CustomDateTimeConverter))] public DateTime Date { get; set; }

        [JsonPropertyName("width")] public int Width { get; set; }
        [JsonPropertyName("height")] public int Height { get; set; }
        [JsonPropertyName("uploadedAt")][System.Text.Json.Serialization.JsonConverter(typeof(CustomDateTimeConverter))] public DateTime UploadedAt { get; set; }

        public string? Path { get; set; }

        public string? OriginalPath { get; set; }
    }
}
