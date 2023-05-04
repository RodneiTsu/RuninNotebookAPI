﻿using System;
using System.Web.Http;
using RuninNotebookAPI.Models;
using RuninNotebookAPI.DB;
using System.Data;

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

            DataTable dtProduct = ConexaoDB.Carrega_Tabela($@"SELECT * FROM product WHERE Serial_Number = '{ssn}'");

            if (dtProduct.Rows.Count == 0)
            {
                MSG = "set result=Serial number Not found";
                ConexaoDB.CRUDU_ID_tabela($@"insert into logruninnb (log,MSG,controller) values ('{ssn}','{MSG}','{controller}')");
                return Ok(MSG);
            }

            foreach (DataRow lin in dtProduct.Rows)
            {
                product.idProduct = Convert.ToInt32(lin[0]);
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

            product_movement.Start_Test = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            string sqlPM = $@"select max(idProduct_Movement) from product_movement where idProduct = {ID} and Status_Code = '1' and WorkGroup = 'PRETEST'";

            if (ID<=0)
            {
                MSG = "set result=Is was not record in DB - PRODUCT";
                ConexaoDB.CRUDU_ID_tabela($@"insert into logruninnb (log,MSG,Model,controller) values ('{ssn}','{MSG}','{product.Product}','{controller}')");
                return Ok(MSG);
            }

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
                ConexaoDB.CRUDU_ID_tabela($@"insert into logruninnb (log,MSG,Model,controller) values ('{ssn}','{MSG}','{product.Product}','{controller}')");
                return Ok(MSG);
            }
            MSG = "set result=0";
            return Ok(MSG);
        }
    }
}
