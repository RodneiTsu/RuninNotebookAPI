using System;
using System.Collections.Generic;

namespace RuninNotebookAPI.Models
{
    public class Runin_LayOut
    {
        public int idRunin_Layout { get; set; }
        public string Descricao { get; set; }
        public string Lado { get; set; }
        public int Linha { get; set; }
        public int Coluna { get; set; }
        public int Position { get; set; }
        public int Status_Code { get; set; }
        public int idproduct_movement { get; set; }
        public string Chamado_IT { get; set; }
        public string Data_Parada { get; set; }
        public string IP_Address { get; set; }
        public string Switch_Port { get; set; }
        public string Note { get; set; }
        public string IP_NB { get; set; }
    }
}