using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using RuninNotebookAPI.Models;
using RuninNotebookAPI.DB;

namespace RuninNotebookAPI.Controllers
{
    public class Runin1InController : ApiController
    {
        public object MSG { get; private set; }

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
                    return Json(MSG);
                }
            }
            else
            {
                MSG = "set result=Serial number length is invalid";
                return Json(MSG);
            }

            product_movement.Start_Test = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            ID = ConexaoDB.CRUDValor_tabela($@"SELECT idproduct FROM product WHERE Serial_Number = '{ssn.ToUpper()}'");

            if (ID>0)
            {
                string sqlPM = $@"INSERT INTO product_movement (idProduct,WorkGroup,Position,Start_Test,Status_Code,Next_Station) values ({ID},'RUNIN1','1','{product_movement.Start_Test}','0','0')";
                
                ConexaoDB.CRUD_tabela(sqlPM);

                string sqlPM_pretest = $@"update product_movement set next_Station='1' where idProduct = {ID} and WorkGroup ='PRETEST' and Status_Code ='1'";
                ConexaoDB.CRUD_tabela(sqlPM_pretest);

            }
            else
            {
                MSG = "set result=Não tem entrada deste serial";
                return Json(MSG);
            }
            MSG = "set result=0";
            return Ok(MSG);
        }
    }
}
