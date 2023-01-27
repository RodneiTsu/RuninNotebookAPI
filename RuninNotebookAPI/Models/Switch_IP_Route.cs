using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RuninNotebookAPI.Models
{
    public class Switch_IP_Route
    {
        public int idSwitch_IP_Route { get; set; }
        public string switch_ip_route { get; set; }
        public string workStation  { get; set; }
        public int DownloadQtd  { get; set; }
        public int DownloadIn { get; set; }
        public string switch_ip_routecol { get; set; }
    }
}