using System;
using System.ComponentModel.DataAnnotations;


namespace RuninNotebookAPI.Models
{
    public class Product_SKU
    {
        public int ID { get; set; }

        public string Product { get; set;}
        public string SKU { get; set; }
        public string Customer { get; set; }
        public int UPH { get; set; }
        public int Display { get; set; }
        public string OSVersio { get; set; }
        public string OSVersion_OLD { get; set; }
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime DtCreate { get; set; }
        public int WLAMAC { get; set; }
        public int BTMAC { get; set; }
        public int LANNMAC { get; set; }

    }
}