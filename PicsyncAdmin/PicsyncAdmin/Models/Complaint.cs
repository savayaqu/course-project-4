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
        [JsonPropertyName("id")] public ulong Id { get; set; }
        [JsonPropertyName("type")] public string Type { get; set; }
        [JsonPropertyName("status")] public int Status { get; set; }
        [JsonPropertyName("description")] public string Description { get; set; }

        [JsonPropertyName("fromUser")] public ComplaintUser FromUser { get; set; }
        [JsonPropertyName("aboutUser")] public ComplaintUser AboutUser { get; set; }

        [JsonPropertyName("picture")] public Picture? Picture { get; set; }
        [JsonPropertyName("album")] public Album? Album { get; set; }
    }

    public class ComplaintUser
    {
        [JsonPropertyName("id")] public ulong Id { get; set; }
        [JsonPropertyName("name")] public string Name { get; set; }
        [JsonPropertyName("isBanned")] public bool? IsBanned { get; set; }
        [JsonPropertyName("login")] public string? Login { get; set; }
    }


    public class ComplaintList
    {
        [JsonPropertyName("complaints")] public List<Complaint> Complaints { get; set; }
    }

}
