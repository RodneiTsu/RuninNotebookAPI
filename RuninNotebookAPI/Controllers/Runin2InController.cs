using System;
using System.Web.Http;
using RuninNotebookAPI.Models;
using RuninNotebookAPI.DB;
using System.Reflection;

namespace RuninNotebookAPI.Controllers
{
    public class Runin2InController : ApiController
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
            controller = "Runin2In - " + asse[1];
            
            

            if (ssn.Length == 15 || ssn.Length == 12 || ssn.Length == 22)
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
            
            ssn = ssn.ToUpper();

            Produto product = new Produto(ssn);

            ID = product.ID;

            string sqlPM_R1 = $@"select if(max(idProduct_Movement)>0,max(idProduct_Movement),0) from product_movement where idProduct = {ID} and Status_Code = '1' and WorkGroup = 'RUNIN1'";
            

            Int32 PMID_R1 = ConexaoDB.CRUDValor_tabela(sqlPM_R1);

            if (PMID_R1 == 0)
            {
                MSG = "set result=Problema nao exista RUNIN1 fechado";
                ConexaoDB.CRUDU_ID_tabela($@"insert into logruninnb (log,Model,MSG,SSN,controller) values ('{ssn}','{product.Product}','{MSG}','{ssn}','{controller}')");
                return Ok(MSG);
            }

            Produto_Movimento PM_R1 = new Produto_Movimento(PMID_R1);

            PM_R1.Start_Test = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            if (ID > 0)
            {
                
                try
                {
                    string sqlPM = $@"INSERT INTO product_movement (idProduct,WorkGroup,Position,Start_Test,Status_Code,Next_Station) values ({ID},'RUNIN2',{PM_R1.Position},'{PM_R1.Start_Test}','0','0')";
                    MSG = "set result=Inclusao no DB tabela product_Movement RUNIN2 IN";
                    ConexaoDB.CRUD_tabela(sqlPM);

                    string sqlPM_pretest = $@"update product_movement set next_Station='1' where idProduct_movement = {PM_R1.idProduct_Movement}";
                    MSG = $@"set result=UpDate no DB tabela product_Movement RUNIN1 OUT Registro: {PMID_R1}";
                    ConexaoDB.CRUD_tabela(sqlPM_pretest);
                }
                catch (Exception)
                {
                    ConexaoDB.CRUDU_ID_tabela($@"insert into logruninnb (log,MSG,Model,controller) values ('{ssn}','{MSG}','{product.Product}','{controller}')");
                    return Ok(MSG);
                }
            }
            else
            {
                MSG = "set result=Serial Number nao cadastrado no DBR tabela produto";
                ConexaoDB.CRUDU_ID_tabela($@"insert into logruninnb (log,MSG,Model,controller) values ('{ssn}','{MSG}','{product.Product}','{controller}')");
                return Ok(MSG);
            }
            MSG = "set result=0";
            return Ok(MSG);
        }
    }
}
