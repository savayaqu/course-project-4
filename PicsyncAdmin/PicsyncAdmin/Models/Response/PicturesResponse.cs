using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicsyncAdmin.Models.Response
{
    public class PictureComplaint
    {
        public ulong Id { get; set; }
        public string Name { get; set; }
        public string Hash { get; set; }
        public int Size { get; set; }
        public DateTime Date { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public DateTime UploadedAt { get; set; }
        public string? Path { get; set; }
    }

    public class PicturesResponse
    {
        public string Sign { get; set; }
        public int Page { get; set; }
        public int Limit { get; set; }
        public int Total { get; set; }

        [JsonProperty("pictures")]
        public List<PictureComplaint> Pictures { get; set; }
    }
}
