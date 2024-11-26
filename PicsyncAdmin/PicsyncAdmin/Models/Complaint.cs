using PicsyncAdmin.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PicsyncAdmin.Models
{
    public class Complaint
    {
        public ulong Id { get; set; }
        public string Type { get; set; }
        public int? Status { get; set; }
        public string Description { get; set; }
        public User AboutUser { get; set; }
        public User? FromUser { get; set; }
        public PictureComplaint? Picture { get; set; }
        public AlbumComplaint? Album { get; set; }

        // Свойство для получения описания статуса
        public string StatusDescription
        {
            get
            {
                return Status switch
                {
                    1 => "Рассмотрена",
                    0 => "Отклонена",
                    _ => "Не рассмотрена" // Для null (или других значений, если status не инициализирован)
                };
            }
        }
    }
    public class AlbumComplaint
    {
        public ulong Id { get; set; }
        public string Name { get; set; }
    }
    public class PictureComplaint
    {
        [JsonPropertyName("id")] public ulong Id { get; set; }
        [JsonPropertyName("name")] public string Name { get; set; }
        [JsonPropertyName("hash")] public string Hash { get; set; }
        [JsonPropertyName("size")] public int Size { get; set; }

        [JsonPropertyName("date")]
        [JsonConverter(typeof(CustomDateTimeConverter))]  // Apply the custom converter
        public DateTime Date { get; set; }

        [JsonPropertyName("width")] public int Width { get; set; }
        [JsonPropertyName("height")] public int Height { get; set; }
        [JsonPropertyName("uploadedAt")] public DateTime UploadedAt { get; set; }

        public string Path { get; set; }
    }




}
