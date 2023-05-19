using System;
using System.ComponentModel.DataAnnotations;
using System.Data;
using RuninNotebookAPI.DB;

namespace RuninNotebookAPI.Models
{
    public class Produto_Movimento
    {
        public int idProduct_Movement { get; set; }
        public int idProduct { get; set; }
        public string WorkGroup { get; set; }
        public string Position { get; set; }
        public string Operator_ID { get; set; }
        public string Error_Code { get; set; }
        public string Fail_Description { get; set; }
        public string Start_Test { get; set; }
        public string End_Test { get; set; }
        public string Status_Code { get; set; }
        public string Next_Station { get; set; }
        public string Station { get; set; }
        public string Station_Date { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime Station_Date_PACK_QA { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime Station_Date_Palletization { get; set; }

        public int IDPM { get; set; }

        public string ProductLine { get; set; }

        public Produto_Movimento()
        { }

        public Produto_Movimento(int idpm)
        {
            DataTable dtProduct_Movement = ConexaoDB.Carrega_Tabela($@"select * from Product_Movement where idProduct_Movement='{idpm}'");
            if (dtProduct_Movement.Rows.Count > 0)
            {
                foreach (DataRow lin in dtProduct_Movement.Rows)
                {
                    IDPM = Convert.ToInt32(lin[0]);
                    idProduct_Movement = Convert.ToInt32(lin[0]);
                    idProduct = Convert.ToInt32(lin[1]);
                    WorkGroup = lin[2].ToString();
                    Position = lin[3].ToString();
                    Operator_ID = lin[4].ToString();
                    Error_Code = lin[5].ToString();
                    Fail_Description = lin[6].ToString();
                    Start_Test = lin[7].ToString();
                    End_Test = lin[8].ToString();
                    Start_Test = lin[9].ToString();
                    Next_Station = lin[10].ToString();
                    Station = lin[11].ToString();
                    if (lin[12].ToString() != "")
                        Station_Date = lin[12].ToString();
                    if (lin[13].ToString() != "")
                        Station_Date_PACK_QA = Convert.ToDateTime(lin[13]);
                    if (lin[14].ToString() != "")
                        Station_Date_Palletization = Convert.ToDateTime(lin[14]);
                    ProductLine = lin[15].ToString();
                }
            }
            else
            {
                IDPM = 0;
            }
        }
    }
}