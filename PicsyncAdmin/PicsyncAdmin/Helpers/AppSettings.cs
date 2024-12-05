using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicsyncAdmin.Helpers
{
    public static class AppSettings
    {
        public static int UploadDisablePercentage { get; set; }
        public static long TotalSpace { get; set; }
        public static long FreeSpace { get; set; }
        public static long UsedSpace { get; set; }
        public static int UsedPercent { get; set; }
    }
}
