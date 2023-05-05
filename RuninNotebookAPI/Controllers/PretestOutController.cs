using System;
using RuninNotebookAPI.Models;
using RuninNotebookAPI.DB;
using System.Web.Http;
using System.Data;

namespace RuninNotebookAPI.Controllers
{    
    public class PretestOutController : ApiController
    {
        public string MSG { get; set; }
        public int ID { get; set; }
        public int IDPM { get; set; }
        public string controller { get; set; }


        [HttpGet]
        public IHttpActionResult OUT_GET(string ssn)
        {
            if (ssn is null)
            {
                NotFound();
            }

            controller = "PretestOut";
            string[] Columns = ssn.Split(',');

            Produto product = new Produto();
            Produto_Movimento product_movement = new Produto_Movimento();

            if (Columns[0].Length == 15 || Columns[0].Length == 12 || Columns[0].Length == 22)
            {
                if (Columns[0].ToUpper().ToString().ToUpper().Substring(4, 2) == "B6")
                {
                    product.Customer = "ASUS";
                }
                else if (Columns[0].ToUpper().ToString().ToUpper().Substring(9, 3) == "935" || Columns[0].Length == 22)
                {
                    product.Customer = "ACER";
                }
                else if (Columns[0].ToUpper().ToString().ToUpper().Substring(10, 2) == "TL")
                {
                    product.Customer = "HUAWEI";
                }
                else
                {
                    MSG = "set result=It was not possible to define a valid customer";
                    ConexaoDB.CRUDU_ID_tabela($@"insert into logruninnb (log,MSG,controller) values ('{ssn}','{MSG}','{controller}')");
                    return Ok( MSG) ;
                }
            }
            else
            {
                MSG = "set result=Serial number length is invalid";
                ConexaoDB.CRUDU_ID_tabela($@"insert into logruninnb (log,MSG,controller) values ('{ssn}','{MSG}','{controller}')");
                return Ok( MSG) ;
            }

            DataTable dtProduct =  ConexaoDB.Carrega_Tabela($@"SELECT * FROM product WHERE Serial_Number = '{Columns[0]}'");
 
            if (dtProduct.Rows.Count == 0)
            {
                MSG = "set result=Serial number Not found";
                ConexaoDB.CRUDU_ID_tabela($@"insert into logruninnb (log,MSG,controller) values ('{ssn}','{MSG}','{controller}')");
                return Ok(MSG);
            }

            foreach (DataRow lin in dtProduct.Rows)
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
            }

            ID = product.idProduct;

            string slqPM = $@"select max(pm.idProduct_Movement) ";
                  slqPM += $@"from engteste.product p inner ";
                  slqPM += $@"join engteste.product_movement pm on p.idProduct = pm.idProduct ";
                  slqPM += $@"where p.idProduct = {ID} and pm.WorkGroup = 'PRETEST'";
            try
            {
                IDPM = ConexaoDB.CRUDValor_tabela(slqPM);
            }
            catch (Exception)
            {
                int existeMov = 0;
                try
                {
                    existeMov = ConexaoDB.CRUDValor_tabela($@"select MAX(PM.idProduct_Movement) from product_movement pm where PM.idProduct = {ID} and PM.Status_Code = '1' and PM.WorkGroup = 'PRETEST'");
                    MSG = $@"set result=Existe PRETEST fechado registro-{existeMov}";
                }
                catch (Exception)
                {
                    MSG = "set result=Serial number Not found PRETEST IN";
                }
                ConexaoDB.CRUDU_ID_tabela($@"insert into logruninnb (log,Model,SSN,MSG,controller) values ('{ssn}','{product.Product}','{product.Serial_Number}','{MSG}','{controller}')");
                return Ok(MSG);
            }
            
            if (ConexaoDB.CRUDCampo_tabela($@"Select Fail_Description from product_Movement where idProduct_Movement={IDPM}")=="PASS")
            {
                MSG = $@"set result=ja existe prestet fechado idPM={IDPM}";
                ConexaoDB.CRUDU_ID_tabela($@"insert into logruninnb (log,Model,SSN,MSG,controller) values ('{ssn}','{product.Product}','{product.Serial_Number}','{MSG}','{controller}')");
                return Ok(MSG);
            }

            try
            {
                MSG = "set result=Problema falta de informação SSN-ID-ErrorCode-PASS ou FAIL";
                product.Serial_Number = Columns[0];
                product_movement.End_Test = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                product_movement.Operator_ID = Columns[1];
                product_movement.Error_Code = Columns[2];
                product_movement.Fail_Description = Columns[3];
                product_movement.Status_Code = "1";

                string SQL = $@"UPDATE product_movement set Operator_ID='{product_movement.Operator_ID}', Error_Code='{product_movement.Error_Code}', Fail_Description='{product_movement.Fail_Description}', End_Test='{product_movement.End_Test}',Status_Code='1' ";
                SQL += $@"WHERE idProduct_Movement = {IDPM};";

                MSG = "set result=Erro ao gravar product_movement";
                ConexaoDB.CRUD_tabela(SQL);
            }
            catch (Exception)
            {
                ConexaoDB.CRUDU_ID_tabela($@"insert into logruninnb (log,Model,SSN,MSG,controller) values ('{ssn}','{product.Product}','{product.Serial_Number}','{MSG}','{controller}')");
                return Ok( MSG) ;
            }
            MSG = "set result=0";
            return Ok( MSG) ;
        }
    }
}
