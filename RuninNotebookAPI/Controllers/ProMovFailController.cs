using System;
using RuninNotebookAPI.Models;
using RuninNotebookAPI.DB;
using System.Web.Http;
using System.Data;
using System.Reflection;

namespace RuninNotebookAPI.Controllers
{    
    public class ProMovFailController : ApiController
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

            string[] asse = Assembly.GetExecutingAssembly().FullName.ToString().Split(',');

            controller = "ProMovFail - " + asse[1];

            string[] Columns = ssn.Split(',');

            if (Columns[0].Length == 15 || Columns[0].Length == 12 || Columns[0].Length == 22)
            {
                if (Columns[0].Substring(4, 2) == "B6" || Columns[0].Substring(9, 3) == "935")
                {
                    MSG = "set result=0";
                }
                else
                {
                    MSG = "set result=It was not possible to define a valid customer";
                    ConexaoDB.CRUDU_ID_tabela($@"insert into logruninnb (log,MSG,controller,SSN) values ('{ssn}','{MSG}','{controller}'),{Columns[0]}");
                    return Ok(MSG);
                }
            }
            else
            {
                MSG = "set result=Serial number length is invalid";
                ConexaoDB.CRUDU_ID_tabela($@"insert into logruninnb (log,MSG,controller,SSN) values ('{ssn}','{MSG}','{controller}'),{Columns[0]}");
                return Ok(MSG);
            }
            
            Columns[0] = Columns[0].ToUpper().Trim();
            Produto product = new Produto(Columns[0]);
            ID = product.ID;
            
            if (ID == 0)
            {
                MSG = "set result=Serial number nao existe no cadastro de Produto";
                ConexaoDB.CRUDU_ID_tabela($@"insert into logruninnb (log,MSG,controller) values ('{ssn}','{MSG}','{controller}')");
                return Ok(MSG);
            }
            
            string slqPM = $@"select if(max(idProduct_Movement)>0,max(idProduct_Movement),0) ";
                  slqPM += $@"from engteste.product p inner ";
                  slqPM += $@"join engteste.product_movement pm on p.idProduct = pm.idProduct ";
                  slqPM += $@"where p.idProduct = {ID} ";
            
            try
            {
                MSG = $@"Set result=Error ao consultar Product_Movement SSN:{ID}";
                IDPM = ConexaoDB.CRUDValor_tabela(slqPM);
            }
            catch (Exception)
            {
                ConexaoDB.CRUDU_ID_tabela($@"insert into logruninnb (log,Model,SSN,MSG,controller) values ('{ssn}','{product.Product}','{product.Serial_Number}','{MSG}','{controller}')");
                return Ok(MSG);
            }
            
            if (IDPM==0)
            {

            }

            Produto_Movimento product_movement = new Produto_Movimento(IDPM);
            ErrorCode errocode = new ErrorCode(Convert.ToInt32(Columns[2]));

            try
            {
                if (ID==0)
                {
                    MSG = $@"set result=Error Code ñao existe ErrorCode:{Columns[2]}";
                }
                else
                {
                    ConexaoDB.CRUD_tabela($@"INSERT INTO product_errorcode (ProductMovementID, ErrorCodeID) VALUES ({IDPM},{Columns[2]})");
                }
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
