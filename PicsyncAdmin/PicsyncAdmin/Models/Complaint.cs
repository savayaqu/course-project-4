using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicsyncAdmin.Models
{
    public class Complaint
    {
        public ulong Id { get; set; }
        public string Type { get; set; }
        public int Status { get; set; }  
        public string Description { get; set; }
        public User AboutUser { get; set; }
        public User? FromUser { get; set; }
        public object Picture { get; set; }  
        public AlbumComplaint? Album { get; set; }
    }
    public class AlbumComplaint
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }


}
