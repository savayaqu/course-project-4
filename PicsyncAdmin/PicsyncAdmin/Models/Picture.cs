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
        [JsonPropertyName("id")] public ulong Id { get; set; }

    }
}
