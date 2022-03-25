using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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
    }
}