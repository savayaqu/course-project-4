using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicsyncAdmin.Models
{
    public class ComplaintResponse
    {
        
        public List<Complaint> Complaints { get; set; }
        [JsonProperty("limit")] public int Limit { get; set; }
        [JsonProperty("total")] public int Total { get; set; }
        [JsonProperty("page")]  public int Page { get; set; }
    }

}
