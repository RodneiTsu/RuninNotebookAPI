using System;
using System.ComponentModel.DataAnnotations;
using System.Data;
using RuninNotebookAPI.DB;


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
        
        public int ID { get; set; }
        public Produto()
        {}

        public  Produto(string SSN)
        {
            DataTable dtProduct =  ConexaoDB.Carrega_Tabela($@"select * from Product where Serial_Number='{SSN}'");
            if (dtProduct.Rows.Count > 0)
            {
                foreach (DataRow lin in dtProduct.Rows)
                {
                    ID= Convert.ToInt32(lin[0]);
                    idProduct = Convert.ToInt32(lin[0]);
                    //product.idProduct_SKU = Convert.ToInt32(lin[1]);
                    Serial_Number = lin[2].ToString();
                    CustomerSerial = lin[3].ToString();
                    WorkOrder = lin[4].ToString();
                    UUID = lin[5].ToString();
                    SKU = lin[6].ToString();
                    Color_ID = lin[7].ToString();
                    Product = lin[8].ToString();
                    Model = lin[9].ToString();
                    Customer = lin[10].ToString();
                    Status_Code = lin[11].ToString();
                    Dt_Creat = Convert.ToDateTime(lin[12]);
                    WorkOrder = lin[13].ToString();
                    Download = lin[14].ToString();
                    if (lin[15].ToString() != "")
                    {
                        Dt_GetIn = Convert.ToDateTime(lin[15]);
                    }
                    if (lin[16].ToString() != "")
                    {
                        Dt_GetOut = Convert.ToDateTime(lin[16]);
                    }
                    if (Convert.ToInt32(lin[17]) > 0)
                    {
                        idSwitch = Convert.ToInt32(lin[17]);
                    }
                    S_60 = lin[18].ToString();
                    S_MBSN = lin[19].ToString();
                }
            }
            else
            {
                ID = 0;
            }
        
        }

    }
}