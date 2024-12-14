using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicsyncAdmin.Models.Response
{
    public class ComplaintResponse
    {
        public List<AlbumComplaintData>? Albums { get; set; }
        [JsonProperty("limit")] public int Limit { get; set; }
        [JsonProperty("total")] public int Total { get; set; }
        [JsonProperty("page")] public int Page { get; set; }
    }

    public class AlbumComplaintData
    {
        [JsonProperty("id")] public ulong Id { get; set; }

        [JsonProperty("name")] public string? Name { get; set; }
        [JsonProperty("album")] public Album? Album { get; set; }
        [JsonProperty("complaintsCount")] public int ComplaintsCount { get; set; }
        [JsonProperty("complaints")] public List<Complaint>? Complaints { get; set; }
    }
}
