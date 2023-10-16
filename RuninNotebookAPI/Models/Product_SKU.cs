using RuninNotebookAPI.DB;
using System;
using System.Data;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;


namespace RuninNotebookAPI.Models
{
    public class Product_SKU
    {
        public Int32 ID { get; set; }
        public string Product { get; set; }
        public string SKU { get; set; }
        public string Customer { get; set; }
        public Int32 UPH { get; set; }
        public bool Display { get; set; }
        public string OSVersion { get; set; }
        [DisplayName("Data de Criação")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime DtCreate { get; set; }

        public int StatusCode { get; set; }
        public int QtdMAC { get; set; }
        public bool WO { get; set; }
        public string OrderNumber { get; set; }
        public bool SerialNumber { get; set; }
        public string ListSSN { get; set; }
        public bool CtrRun12 { get; set; }
        public bool CtrRun48 { get; set; }

        public Product_SKU()
        { }

        public Product_SKU(string _sku)
        {
            DataTable product_sku = ConexaoDB.Carrega_Tabela($@"select * from Product_SKU where SKU='{_sku}'");
            if (product_sku.Rows.Count > 0)
            {
                foreach (DataRow lin in product_sku.Rows)
                {
                    ID = Convert.ToInt32(lin[0]);
                    Product = lin[1].ToString();
                    SKU = lin[2].ToString();
                    Customer = lin[3].ToString();
                    UPH = Convert.ToInt32(lin[4]);
                    Display = Convert.ToBoolean(lin[5]);
                    OSVersion = lin[6].ToString();
                    DtCreate = Convert.ToDateTime(lin[7]);
                    StatusCode = Convert.ToInt32(lin[8]);
                    QtdMAC = Convert.ToInt32(lin[9]);
                    WO = Convert.ToBoolean(lin[10]);
                    OrderNumber = lin[11].ToString();
                    SerialNumber = Convert.ToBoolean(lin[12]);
                    ListSSN = lin[13].ToString();
                    CtrRun12 = Convert.ToBoolean(lin[14]);
                    CtrRun48 = Convert.ToBoolean(lin[15]);
                }
            }
            else
            {
                ID = 0;
            }
        }

    }
}