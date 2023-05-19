using System;
using System.Web.Http;
using RuninNotebookAPI.Models;
using RuninNotebookAPI.DB;
using System.Data;

namespace RuninNotebookAPI.Controllers
{
    public class Runin1OutController : ApiController
    {
        public string MSG { get; set; }
        public int ID { get; set; }
        public int PMID { get; set; }
        public string controller { get; set; }

        [HttpGet]
        public IHttpActionResult Out_GET(string ssn)
        {
            if (ssn is null)
            {
                NotFound();
            }

            controller = "Runin1Out";
            string[] Columns = ssn.Split(',');
            MSG = "set result=0";

            
            Produto_Movimento product_movement = new Produto_Movimento();

            if (Columns[0].Length == 15 || Columns[0].Length == 12 || Columns[0].Length == 22)
            {
                if (ssn.Substring(4, 2) == "B6" || ssn.Substring(9, 3) == "935")
                {
                    MSG = "set result=0";
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

            Columns[0] = Columns[0].ToUpper();

            Produto product = new Produto(Columns[0]);

            ID = product.ID;

            if (ID == 0)
            {
                MSG = "set result=Serial Number nao encontrado no DB tabela Product";
                ConexaoDB.CRUDU_ID_tabela($@"insert into logruninnb (log,MSG,controller) values ('{ssn}','{MSG}','{controller}')");
                return Ok(MSG);
            }

            product.Serial_Number = Columns[0];
            product_movement.Start_Test = DateTime.Now.AddMinutes(-30).ToString("yyyy-MM-dd HH:mm:ss");
            product_movement.End_Test = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            product_movement.Operator_ID = Columns[1];
            product_movement.Error_Code = Columns[2];
            product_movement.Fail_Description = Columns[3];
            product_movement.Status_Code = "1";

            string sqlPM = $@"select if(max(idProduct_Movement)>0,max(idProduct_Movement),0) from product_movement where idProduct = {ID} AND WorkGroup = 'RUNIN1'";

            try
            {
                PMID = ConexaoDB.CRUDValor_tabela(sqlPM);
            }
            catch (Exception)
            { 
                MSG = "set result=Problema nao existe  RUNIN1 IN ";
                ConexaoDB.CRUDU_ID_tabela($@"insert into logruninnb (log,Model,MSG,SSN,controller) values ('{ssn}','{product.Product}','{MSG}','{ssn}','{controller}')");
                return Ok(MSG);
            }

            if (PMID == 0)
            {
                product_movement.Operator_ID = "OPERADOR-TESTE";
                string sqlPM_Insert = $@"INSERT INTO product_movement (idProduct,WorkGroup,Position,Start_Test,Status_Code,Next_Station) values ({ID},'RUNIN1','1564','{product_movement.Start_Test}','0','0')";
                PMID = ConexaoDB.CRUDU_ID_tabela(sqlPM_Insert);         
                ConexaoDB.CRUD_tabela($@"update product_movement set next_Station='1' where idProduct_Movement = {PMID}");
            }

            if (ConexaoDB.CRUDCampo_tabela($@"Select Fail_Description from product_Movement where idProduct_Movement={PMID}") == "PASS")
            {
                MSG = $@"set result=ja existe RUNIN1 fechado idPM={PMID}";
                ConexaoDB.CRUDU_ID_tabela($@"insert into logruninnb (log,Model,SSN,MSG,controller) values ('{ssn}','{product.Product}','{product.Serial_Number}','{MSG}','{controller}')");
                return Ok(MSG);
            }

            try
            {
                string SQL = $@"UPDATE product_movement set Operator_ID='{product_movement.Operator_ID}', Error_Code='{product_movement.Error_Code}', Fail_Description='{product_movement.Fail_Description}', End_Test='{product_movement.End_Test}',Status_Code='1' ";
                SQL += $@"WHERE idProduct_movement = {PMID}";

                MSG = "set result=Erro ao gravar product_movement";
                ConexaoDB.CRUD_tabela(SQL);
            }
            catch (Exception)
            {
                ConexaoDB.CRUDU_ID_tabela($@"insert into logruninnb (log,MSG,Model,controller) values ('{ssn}','{MSG}','{product.Product}','{controller}')");
                return Ok(MSG);
            }

            MSG = "set result=0";
            return Ok( MSG) ;
        }
    }
}
