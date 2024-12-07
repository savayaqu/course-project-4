using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicsyncAdmin.Models
{
    public class Album
    {
        public ulong Id { get; set; }
        public required string Name { get; set; }
    }
}
