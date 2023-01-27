using System;
using RuninNotebookAPI.Models;
using RuninNotebookAPI.DB;
using System.Web.Http;
using System.Data;
using System.Collections.Generic;
using System.Linq;

namespace RuninNotebookAPI.Controllers
{
    public class CheckOSVersionController : ApiController
    {
        public string MSG { get; set; }

        public string controller { get; set; }

        [HttpGet]
        public IHttpActionResult Send_GET(string ssn)
        {
            if (ssn is null)
            {
                NotFound();
            }

            controller = "CheckOSVersion";

            string[] Columns = ssn.Split(',');

            WebSFIS_GET wb = new WebSFIS_GET();
            Produto product = new Produto();
            NBMAC nbmac = new NBMAC();
            Switch_IP_Route IPSwitch = new Switch_IP_Route();


            MSG = "set result=0";

            string sqlProduct = $@"SELECT idProduct, idProduct_SKU, Serial_Number, CustomerSerial, WorkOrder, UUID, SKU, Color_ID, Product, Model, Customer, Status_Code, Dt_Creat, WorkStation, ";

            if (Columns[0].Length == 15 || Columns[0].Length == 12 || Columns[0].Length == 22)
            {
                if (Columns[0].ToUpper().ToString().ToUpper().Substring(4, 2) == "B6")
                {
                    wb.CustomerCode = "ASUS";
                    sqlProduct += $@"Download, Dt_GetIn, Dt_GetOut, idSwitch, S_60, S_MBSN FROM product WHERE Serial_Number = '{Columns[0].ToUpper()}'";
                }
                else if (Columns[0].ToUpper().ToString().ToUpper().Substring(9, 3) == "935" )
                {
                    wb.CustomerCode = "ACER";
                    sqlProduct += $@"Download, Dt_GetIn, Dt_GetOut, idSwitch, S_60, S_MBSN FROM product WHERE Serial_Number = '{Columns[0].ToUpper()}'";
                }
                else if ( Columns[0].Length == 22)
                {
                    wb.CustomerCode = "ACER";
                    sqlProduct += $@"Download, Dt_GetIn, Dt_GetOut, idSwitch, S_60, S_MBSN FROM product WHERE CustomerSerial = '{Columns[0].ToUpper()}'";
                }
                else if (Columns[0].ToUpper().ToString().ToUpper().Substring(10, 2) == "TL")
                {
                    wb.CustomerCode = "HUAWEI";
                    sqlProduct += $@"Download, Dt_GetIn, Dt_GetOut, idSwitch, S_60, S_MBSN FROM product WHERE Serial_Number = '{Columns[0].ToUpper()}'";
                }
                else
                {
                    MSG = "set result=It was not possible to define a valid customer";
                    ConexaoDB.CRUD_tabela($@"insert into logruninnb (log,MSG,controller) values ('{ssn}','{MSG}','{controller}')");
                    return Ok(MSG);
                }
            }
            else
            {
                MSG = "set result=Serial number length is invalid";
                ConexaoDB.CRUD_tabela($@"insert into logruninnb (log,MSG,controller) values ('{ssn}','{MSG}','{controller}')");
                return Ok(MSG);
            }

            DataTable dtProduct = ConexaoDB.Carrega_Tabela(sqlProduct);

            if (dtProduct.Rows.Count <= 0)
            {
                MSG = "set result=Serial nao encontrado em PRODUTO";
                ConexaoDB.CRUD_tabela($@"insert into logruninnb (log,MSG,controller) values ('{ssn}','{MSG}','{controller}')");
                return Ok(MSG);
            }

            foreach(DataRow lin in dtProduct.Rows)
            {
                product.idProduct = Convert.ToInt32(lin[0]);
                //product.idProduct_SKU = Convert.ToInt32(lin[1]);
                product.Serial_Number = lin[2].ToString();
                product.CustomerSerial = lin[3].ToString();
                product.WorkOrder = lin[4].ToString();
                product.UUID = lin[5].ToString();
                product.SKU = lin[6].ToString();
                product.Color_ID = lin[7].ToString();
                product.Product = lin[8].ToString();
                product.Model = lin[9].ToString();
                product.Customer = lin[10].ToString();
                product.Status_Code = lin[11].ToString();
                product.Dt_Creat = Convert.ToDateTime(lin[12]);
                product.WorkOrder = lin[13].ToString();
                product.Download = lin[14].ToString();
                if (lin[15].ToString() != "")
                {
                    product.Dt_GetIn = Convert.ToDateTime(lin[15]);
                }
                if (lin[16].ToString() != "")
                {
                    product.Dt_GetOut = Convert.ToDateTime(lin[16]);
                }
                if (Convert.ToInt32(lin[17]) > 0)
                {
                    product.idSwitch = Convert.ToInt32(lin[17]);
                }
                product.S_60 = lin[18].ToString();
                product.S_MBSN = lin[19].ToString();
            }
         
            int pos = 0;
            string nome = "";

            //======================================================================================================================================

            string _CheckOsVersion = ConexaoDB.CRUDCampo_tabela($@"Select OSVersion from Product_SKU where SKU='{product.SKU}'");

            MSG = "set CheckOsVersion=" + _CheckOsVersion;

            //======================================================================================================================================

            return Ok(MSG);
        }
    }
}
