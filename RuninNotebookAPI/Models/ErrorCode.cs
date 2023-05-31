using System;
using System.Data;
using RuninNotebookAPI.DB;
using System.ComponentModel.DataAnnotations;

namespace RuninNotebookAPI.Models
{
    public class ErrorCode
    {
        public int idErrorCode { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public string Description_US { get; set; }
        public int Status { get; set; }
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime dtCreat { get; set; }
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime dtAlter { get; set; }
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime dtDelet { get; set; }
        public string Customer { get; set; }
        public string Model { get; set; }
        public int ID { get; set; }

        public ErrorCode(){
        }

        public ErrorCode(int id)
        {
            DataTable dtErrorCode = ConexaoDB.Carrega_Tabela($@"select * from ErrorCode where idErrorCode={id}");
            if (dtErrorCode.Rows.Count > 0)
            {
                foreach (DataRow lin in dtErrorCode.Rows)
                {
                    idErrorCode = Convert.ToInt32(lin[0]);
                    Code = lin[1].ToString();
                    Description = lin[2].ToString();
                    Description_US = lin[3].ToString();
                    Status = Convert.ToInt32(lin[4]);
                    if (lin[5].ToString() != "")
                    {
                        dtCreat = Convert.ToDateTime(lin[5]);
                    }
                    if (lin[6].ToString() != "")
                    {
                        dtAlter = Convert.ToDateTime(lin[6]);
                    }
                    if (lin[7].ToString() != "")
                    {
                        dtDelet = Convert.ToDateTime(lin[7]);
                    }
                    Customer = lin[8].ToString();
                    Model = lin[9].ToString();
                    ID = Convert.ToInt32(lin[0]);
                }

            }
            else
            {
                ID = 0;
            }
        }
    }
}