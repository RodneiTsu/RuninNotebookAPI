using System;

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
        public string Customer { get; set; }
        public string Status_Code { get; set; }
        public DateTime Dt_Creat { get; set; }
        public string WorkStation { get; set; }

    }
}