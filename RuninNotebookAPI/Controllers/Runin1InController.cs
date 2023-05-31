using System;
using System.Web.Http;
using RuninNotebookAPI.Models;
using RuninNotebookAPI.DB;


namespace RuninNotebookAPI.Controllers
{
    public class Runin1InController : ApiController
    {
        public string MSG { get; set; }
        public int ID { get; set; }
        public int PMID { get; set; }
        public string controller { get; set; }

        [HttpGet]
        public IHttpActionResult IN_GET(string ssn)
        {
            if (ssn is null)
            {
                NotFound();
            }

            controller = "Runin1In";

            ssn = ssn.ToUpper();
            Produto_Movimento product_movement = new Produto_Movimento();

            if (ssn.Length == 15 || ssn.Length == 12 || ssn.Length == 22)
            {
                if (ssn.Substring(4, 2) == "B6" || ssn.Substring(9, 3) == "935")
                {
                    MSG = "set result=0";
                }
                else
                {
                    MSG = "set result=Serial Number nao e reconhecido";
                    ConexaoDB.CRUDU_ID_tabela($@"insert into logruninnb (log,MSG,controller) values ('{ssn}','{MSG}','{controller}')");
                    return Ok( MSG) ;
                }
            }
            else
            {
                MSG = "set result=Serial number tamanho nao valido";
                ConexaoDB.CRUDU_ID_tabela($@"insert into logruninnb (log,MSG,controller) values ('{ssn}','{MSG}','{controller}')");
                return Ok( MSG) ;
            }

            Produto product = new Produto(ssn);

            ID = product.ID;

            if (ID == 0)
            {
                MSG = "set result=Serial Number nao encontrado no Banco de Dados";
                ConexaoDB.CRUDU_ID_tabela($@"insert into logruninnb (log,MSG,controller) values ('{ssn}','{MSG}','{controller}')");
                return Ok(MSG);
            }
            product_movement.Start_Test = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string sqlPM = $@"select if(max(idProduct_Movement)>0,max(idProduct_Movement),0) from product_movement where idProduct = {ID} and Status_Code = '1' and WorkGroup = 'PRETEST'";
            try
            {
                MSG = "set result=Problema nao existe registro no PRETEST";
                PMID = ConexaoDB.CRUDValor_tabela(sqlPM);
                sqlPM = $@"INSERT INTO product_movement (idProduct,WorkGroup,Position,Start_Test,Status_Code,Next_Station) values ({ID},'RUNIN1','1564','{product_movement.Start_Test}','0','0')";

                MSG = $@"set result=Problema na inclusao do registro RUNIN1 idProduct {ID}";
                ConexaoDB.CRUD_tabela(sqlPM);

                string sqlPM_pretest = $@"update product_movement set next_Station='1' where idProduct_Movement = {PMID}";
                ConexaoDB.CRUD_tabela(sqlPM_pretest);
            }
            catch (Exception)
            {
                ConexaoDB.CRUDU_ID_tabela($@"insert into logruninnb (log,Model,MSG,SSN,controller) values ('{ssn}','{product.Product}','{MSG}','{ssn}','{controller}')");
                return Ok(MSG);
            }
            MSG = "set result=0";
            return Ok(MSG);
        }
    }
}
