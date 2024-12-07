using Newtonsoft.Json;

namespace PicsyncAdmin.Models.Response
{
    public class PicturesResponse
    {
        public string Sign { get; set; }
        public int Page { get; set; }
        public int Limit { get; set; }
        public int Total { get; set; }

        [JsonProperty("pictures")]
        public List<Picture> Pictures { get; set; }
    }
}
