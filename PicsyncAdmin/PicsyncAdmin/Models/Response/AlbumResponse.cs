using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PicsyncAdmin.Models.Response
{
    public class AlbumResponse
    {
        [JsonProperty("album")] public required AlbumDetails Album { get; set; }
    }

    public class AlbumDetails
    {
        [JsonProperty("id")] public int Id { get; set; }

        [JsonProperty("name")] public required string Name { get; set; }
        [JsonProperty("owner")] public required User Owner { get; set; }

        [JsonProperty("picturesCount")] public int PicturesCount { get; set; }

        [JsonProperty("complaintsCount")] public int ComplaintsCount { get; set; }
    }

}
