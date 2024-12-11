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
        public List<AlbumComplaintData> Complaints { get; set; }
        [JsonProperty("limit")] public int Limit { get; set; }
        [JsonProperty("total")] public int Total { get; set; }
        [JsonProperty("page")] public int Page { get; set; }
    }

    public class AlbumComplaintData
    {
        [JsonProperty("album")] public Album Album { get; set; }
        [JsonProperty("complaintsCount")] public int ComplaintsCount { get; set; }
        [JsonProperty("complaints")] public List<Complaint> Complaints { get; set; }
    }
    public class AlbumViewModel
    {
        public string AlbumName { get; set; }
        public ulong Id { get; set; }
        public int ComplaintsCount { get; set; }
        public ObservableCollection<Picture> Pictures { get; set; }
        public Complaint RepresentativeComplaint { get; set; }
        public List<Complaint> AllComplaints { get; set; } // Хранение всех жалоб
    }


}
