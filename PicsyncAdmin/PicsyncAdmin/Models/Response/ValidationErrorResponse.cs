using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PicsyncAdmin.Models.Response
{
    public class ValidationErrorResponse
    {
        [JsonPropertyName("code")]      public int? Code { get; set; }
        [JsonPropertyName("message")]   public string? Message { get; set; }
        [JsonPropertyName("errors")]    [JsonProperty("errors")] public Dictionary<string, List<string>>? Errors { get; set; }
    }

}
