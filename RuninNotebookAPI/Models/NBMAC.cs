using System;
using System.Collections.Generic;

namespace RuninNotebookAPI.Models
{
    public class NBMAC
    {
        public int ID_MAC { get; set; }
        public string MAC { get; set; }
        public string UUID { get; set; }
        public string SSN { get; set; }
        public string DATECREATE { get; set; }
        public string WLANMAC { get; set; }
        public string BTMAC { get; set; }
        public string LANMAC { get; set; }
    }
}