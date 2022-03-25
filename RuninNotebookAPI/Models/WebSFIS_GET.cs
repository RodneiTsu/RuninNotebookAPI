using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RuninNotebookAPI.Models
{
    public class WebSFIS_GET
    {
        public string WorkOrder { get; set; }
        public string ColorCode { get; set; }
        public string CountryCode { get; set; }
        public string CustomerCode { get; set; }
        public string ModelName { get; set; }
        public string DeviceUnderTestSerualNumber { get; set; }
        public string SKU { get; set; }

    }
}