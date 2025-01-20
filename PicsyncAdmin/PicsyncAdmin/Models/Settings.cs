using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicsyncAdmin.Models
{
    public class Settings
    {
        public ObservableCollection<string> AllowedUploadMimes { get; set; }
        public ObservableCollection<int> AllowedPreviewSizes { get; set; }
        public int WarningLimitForBan { get; set; }
        public long FreeStorageLimit { get; set; }
        public int UploadDisablePercentage { get; set; }
    }

    public class Space
    {
        public long Total { get; set; }
        public long Free { get; set; }
        public long Used { get; set; }
        public int UsedPercent { get; set; }
        public DateTime GotAt { get; set; }
    }

    public class ApiResponse
    {
        public Settings Settings { get; set; }
        public Space Space { get; set; }
    }
}
