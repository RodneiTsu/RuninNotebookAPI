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
    public class Runin2OutController : ApiController
    {

        public string MSG { get; set; }

        public int ID { get; set; }

        [HttpGet]
        public IHttpActionResult Out_GET(string ssn)
        {
            if (ssn is null)
            {
                NotFound();
            }

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
                    return Ok( MSG) ;
                }
            }
            else
            {
                MSG = "set result=Serial number length is invalid";
                return Ok( MSG) ;
            }

            product.Serial_Number = Columns[0];
            product_movement.End_Test = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            product_movement.Operator_ID = Columns[1];
            product_movement.Error_Code = Columns[2];
            product_movement.Fail_Description = Columns[3];
            product_movement.Status_Code = "1";

            try
            {
                string SQL = "";
                if (product.Customer == "ACER")
                {
                    if (product.Serial_Number.Length > 21)
                    {
                        SQL = $@"UPDATE product_movement set Operator_ID='OPERATOR-TEST', Error_Code='{product_movement.Error_Code}', Fail_Description='{product_movement.Fail_Description}', End_Test='{product_movement.End_Test}',Status_Code='1' ";
                        SQL += $@"WHERE idProduct = (SELECT p1.idProduct FROM product p1 where CustomerSerial = '{product.Serial_Number}') AND WorkGroup = 'RUNIN2' AND Status_Code = '0' order by idProduct_Movement desc limit 1;";
                    }
                    else
                    {
                        MSG = "Serial-> ACER dever conter-> qtd de 22 caracters";
                        return Ok( MSG) ;
                    }
                }
                else
                {
                    SQL  = $@"UPDATE product_movement set Operator_ID='OPERATOR-TEST', Error_Code='{product_movement.Error_Code}', Fail_Description='{product_movement.Fail_Description}', End_Test='{product_movement.End_Test}',Status_Code='1' ";
                    SQL += $@"WHERE idProduct = (SELECT p1.idProduct FROM product p1 where Serial_Number = '{product.Serial_Number}') AND WorkGroup = 'RUNIN2' AND Status_Code = '0' order by idProduct_Movement desc limit 1;";
                }
                MSG = "Erro ao gravar product_movement Runin Out";
                ConexaoDB.CRUD_tabela(SQL);
            }
            catch (Exception)
            {
                return Ok( MSG) ;
            }
            MSG = "set result=0";
            return Ok( MSG) ;
        }

    }
}
