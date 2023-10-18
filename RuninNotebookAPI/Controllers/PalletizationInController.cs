using System;
using System.Web.Http;
using RuninNotebookAPI.Models;
using RuninNotebookAPI.DB;
using System.Data;
using System.Reflection;

namespace RuninNotebookAPI.Controllers
{
    public class PalletizationInController : ApiController
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

            string[] asse = Assembly.GetExecutingAssembly().FullName.ToString().Split(',');
            controller = "PalletizationIn - " + asse[1];
            string[] Columns = ssn.Split(',');
            Columns[0] = Columns[0].ToUpper();
            Columns[1] = Columns[1].ToUpper();

            ssn = ssn.ToUpper();
            Produto_Movimento product_movement = new Produto_Movimento();

            if (Columns[0].Length == 15 || Columns[0].Length == 12 || Columns[0].Length == 22)
            {
                if (Columns[0].Substring(4, 2) == "B6" || Columns[0].Substring(9, 3) == "935")
                {
                    MSG = "set result=0";
                }
                else
                {
                    MSG = "set result=Validacao de Serial Number incorreto ou tamanho invalido";
                    ConexaoDB.CRUDU_ID_tabela($@"insert into logruninnb (log,MSG,controller) values ('{ssn}','{MSG}','{controller}')");
                    return Ok( MSG) ;
                }
            }
            else
            {
                MSG = "set result=Validacao de Serial Number incorreto ou tamanho invalido";
                ConexaoDB.CRUDU_ID_tabela($@"insert into logruninnb (log,MSG,controller) values ('{ssn}','{MSG}','{controller}')");
                return Ok( MSG) ;
            }

            Produto product = new Produto(Columns[0]);

            ID = product.ID;

            if (ID == 0)
            {
                MSG = "set result=Serial Number nao encontrado no Banco de Dados";
                ConexaoDB.CRUDU_ID_tabela($@"insert into logruninnb (log,MSG,controller) values ('{ssn}','{MSG}','{controller}')");
                return Ok(MSG);
            }
            product_movement.Start_Test = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string sqlPM = $@"select max(idProduct_Movement) from product_movement where idProduct = {ID} and Status_Code = '1' and WorkGroup = 'RUNIN2'";
            try
            {
                MSG = "set result=Problema nao existe registro no RUNIN2 OUT";
                PMID = ConexaoDB.CRUDValor_tabela(sqlPM);
                sqlPM = $@"INSERT INTO product_movement (idProduct,WorkGroup,Position,Start_Test,Status_Code,Next_Station,ProductLine) values ({ID},'PALLETIZATION','1568','{product_movement.Start_Test}','0','0','{Columns[1]}')";

                MSG = $@"set result=Problema na inclusao do registro PALLETIZATTION idProduct {ID}";
                ConexaoDB.CRUD_tabela(sqlPM);

                string sqlPM_Runin2 = $@"update product_movement set next_Station='1' where idProduct_Movement = {PMID}";
                ConexaoDB.CRUD_tabela(sqlPM_Runin2);
            }
            catch (Exception)
            {
                ConexaoDB.CRUDU_ID_tabela($@"insert into logruninnb (log,Model,MSG,SSN,controller) values ('{ssn}','{product.Product}','{MSG}','{Columns[1]}','{controller}')");
                return Ok(MSG);
            }
            MSG = "set result=0";
            return Ok(MSG);
        }
    }
}
