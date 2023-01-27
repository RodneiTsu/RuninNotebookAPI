using System;
using System.ComponentModel.DataAnnotations;

namespace RuninNotebookAPI.Models
{
    public class Produto
    {
        public int idProduct { get; set; }
        public int idProduct_SKU { get; set; }
        public string Serial_Number { get; set; }
        public string CustomerSerial { get; set; }
        public string WorkOrder { get; set; }
        public string UUID { get; set; }
        public string SKU { get; set; }
        public string Color_ID { get; set; }
        public string Product { get; set; }
        public string Model { get; set; }
        public string Customer { get; set; }
        public string Status_Code { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime Dt_Creat { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime Dt_GetIn { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime Dt_GetOut { get; set; }

        public string WorkStation { get; set; }
        public string Download { get; set; }
        public int idSwitch { get; set; }
        public string S_60 { get; set; }
        public string S_MBSN { get; set; }

        public bool WLAMAC { get; set; }
        public bool BTMAC { get; set; }
        public bool LANMAC { get; set; }
        

    }
}