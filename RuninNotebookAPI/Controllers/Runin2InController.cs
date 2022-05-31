using System;
using System.Web.Http;
using RuninNotebookAPI.Models;
using RuninNotebookAPI.DB;

namespace RuninNotebookAPI.Controllers
{
    public class Runin2InController : ApiController
    {
        public string MSG { get; set; }

        public int ID { get; set; }

        [HttpGet]
        public IHttpActionResult IN_GET(string ssn)
        {
            if (ssn is null)
            {
                NotFound();
            }

            Produto product = new Produto();
            Produto_Movimento product_movement = new Produto_Movimento();

            if (ssn.Length == 15 || ssn.Length == 12 || ssn.Length == 22)
            {
                if (ssn.ToUpper().ToString().ToUpper().Substring(4, 2) == "B6")
                {
                    product.Customer = "ASUS";
                }
                else if (ssn.ToUpper().ToString().ToUpper().Substring(9, 3) == "935" || ssn.Length == 22)
                {
                    product.Customer = "ACER";
                }
                else if (ssn.ToUpper().ToString().ToUpper().Substring(10, 2) == "TL")
                {
                    product.Customer = "HUAWEI";
                }
                else
                {
                    MSG = "set result=It was not possible to define a valid customer";
                    return Ok( MSG) ;
                }
            }
            else
            {
                MSG = "set result=Serial number length is invalid";
                return Ok( MSG) ;
            }

            product_movement.Start_Test = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            ID = ConexaoDB.CRUDValor_tabela($@"SELECT idproduct FROM product WHERE Serial_Number = '{ssn.ToUpper()}'");

            if (ID > 0)
            {

                string  sqlposition = $@"select position from product_movement where idProduct = {ID} and WorkGroup ='RUNIN1' and Status_Code ='1'";
                try
                {
                    MSG = "set result=Erro ao gravar banco Runin2 In";
                    string position = ConexaoDB.CRUDCampo_tabela(sqlposition);
                    string sqlPM = $@"INSERT INTO product_movement (idProduct,WorkGroup,Position,Start_Test,Status_Code,Next_Station) values ({ID},'RUNIN2',{position},'{product_movement.Start_Test}','0','0')";
                    ConexaoDB.CRUD_tabela(sqlPM);
                    string sqlPM_pretest = $@"update product_movement set next_Station='1' where idProduct = {ID} and WorkGroup ='RUNIN1' and Status_Code ='1'";
                    ConexaoDB.CRUD_tabela(sqlPM_pretest);
                }
                catch (Exception)
                {
                    return Ok(MSG);
                }
            }
            else
            {
                MSG = "set result=Nao tem entrada deste serial";
                return Ok( MSG) ;
            }
            MSG = "set result=0";
            return Ok(MSG);
        }
    }
}
