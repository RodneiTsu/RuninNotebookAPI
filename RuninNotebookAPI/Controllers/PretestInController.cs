﻿using RuninNotebookAPI.ServiceReference1;
using RuninNotebookAPI.Models;
using RuninNotebookAPI.DB;
using System.ServiceModel.Channels;
using System.Web.Http;
using System.ServiceModel;
using System;
using System.Reflection;

namespace RuninNotebookAPI.Controllers
{
    public class PretestInController : ApiController
    {
        public string MSG { get; set; }
        public int ID { get; set; }
        public int SKUID { get; set; }
        public int LOGID { get; set; }
        public string controller { get; set; }

        [HttpGet]
        public dynamic GET(string ssn)
        {
            if (ssn is null)
            {
                NotFound();
            }

            string[] asse = Assembly.GetExecutingAssembly().FullName.ToString().Split(',');
            controller = "PretestIn - " + asse[1];

            WebSFIS_GET wb = new WebSFIS_GET();
            Produto_Movimento product_movement = new Produto_Movimento();
            

            if (ssn.Length == 15 || ssn.Length == 12 || ssn.Length == 22)
            {
                if (ssn.ToUpper().ToString().ToUpper().Substring(4, 2) == "B6")
                {
                    wb.CustomerCode = "ASUS";
                }
                else if (ssn.ToUpper().ToString().ToUpper().Substring(9, 3) == "935" || ssn.Length == 22)
                {
                    wb.CustomerCode = "ACER";
                }
                else if (ssn.ToUpper().ToString().ToUpper().Substring(10, 2) == "TL")
                {
                    wb.CustomerCode = "HUAWEI";
                }
                else
                {
                    MSG = "set result=Nao e possivel validar serial number com customer";
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

            ssn = ssn.ToString().ToUpper();

            HttpRequestMessageProperty customerHeader = new HttpRequestMessageProperty();
            WebServiceTestSoapClient client = new WebServiceTestSoapClient("WebServiceTestSoap");
            customerHeader.Headers.Add("X-Type", "L10");
            customerHeader.Headers.Add("X-Customer", wb.CustomerCode);
            using (new OperationContextScope(client.InnerChannel))
            {
                OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = customerHeader;
                var SFIS_CHECK_STATUS = client.SFIS_GET_DATA(ssn);

                if (SFIS_CHECK_STATUS.StatusCode=="0")
                {
                    wb.ColorCode = SFIS_CHECK_STATUS.Configuration.ColorCode;
                    wb.CountryCode = SFIS_CHECK_STATUS.Configuration.CountryCode;
                    wb.DeviceUnderTestSerialNumber = SFIS_CHECK_STATUS.Configuration.DeviceUnderTestSerialNumber;
                    wb.ModelName = SFIS_CHECK_STATUS.Configuration.ModelName;
                    wb.WorkOrder = SFIS_CHECK_STATUS.Configuration.DeviceDetails[0].Value;
                    wb.SKU = SFIS_CHECK_STATUS.Configuration.Sku;

                    //wb.ColorCode = SFIS_CHECK_STATUS.Configuration.ColorCode;
                    //wb.CountryCode = SFIS_CHECK_STATUS.Configuration.CountryCode;
                    //wb.DeviceUnderTestSerialNumber = SFIS_CHECK_STATUS.Configuration.DeviceUnderTestSerialNumber;
                    //wb.ModelName = "M515DA";
                    //wb.WorkOrder = "000080238245";
                    //wb.SKU = "90NB0T41-M00B49";

                    product_movement.Start_Test = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    Produto product = new Produto(ssn);
                    ID = product.ID;

                    SKUID = ConexaoDB.CRUDValor_tabela($@"select id from product_sku where sku='{wb.SKU}'");

                    string sqlP  = "INSERT INTO product(Serial_Number, SKU, Color_ID, Product, Customer, Status_Code, WorkOrder) VALUES (";
                           sqlP += $@"'{ssn}','{wb.SKU}','{wb.ColorCode}','{wb.ModelName}','{wb.CustomerCode}','0','{wb.WorkOrder}')";

                    string sqlPM = $@"INSERT INTO product_movement (idProduct,WorkGroup,Position,Start_Test,Status_Code,Next_Station) values ({ID},'PRETEST','1565','{product_movement.Start_Test}','0','0')";

                    try
                    {
                        if (ID == 0)
                        {
                            ID = ConexaoDB.CRUDU_ID_tabela(sqlP);        
                            sqlPM = $@"INSERT INTO product_movement (idProduct,WorkGroup,Position,Start_Test,Status_Code,Next_Station) values ({ID},'PRETEST','1565','{product_movement.Start_Test}','0','0')";
                            ConexaoDB.CRUD_tabela(sqlPM);
                            if (SKUID == 0)
                            {
                                string sqlSKU = $@"INSERT INTO engteste.product_sku ";
                                sqlSKU += $@"(Product,SKU,Customer,UPH,Display,OSVersion,DtCreate)";
                                sqlSKU += $@" VALUES ('{wb.ModelName}','{wb.SKU}','{wb.CustomerCode}',9,0,NULL,'{product_movement.Start_Test}')";
                                ConexaoDB.CRUD_tabela(sqlSKU);
                            }
                        }
                        else
                        {
                            if (SKUID == 0)
                            {
                                MSG = "set result=Problema ao criar DB Product_SKU";
                                string sqlSKU = $@"INSERT INTO engteste.product_sku ";
                                sqlSKU += $@"(Product,SKU,Customer,UPH,Display,OSVersion,DtCreate)";
                                sqlSKU += $@" VALUES ('{wb.ModelName}','{wb.SKU}','{wb.CustomerCode}',9,0,NULL,'{product_movement.Start_Test}')";
                                ConexaoDB.CRUD_tabela(sqlSKU);
                            }
                            MSG = "set result=Problema ao criar DB Product_Movement";
                            ConexaoDB.CRUD_tabela(sqlPM);
                        }
                    }
                    catch (Exception)
                    { 
                        MSG = "set result=Problema ao criar DB LogRuninNb";
                        ConexaoDB.CRUDU_ID_tabela($@"insert into logruninnb (log,Model,MSG,SSN,controller) values ('{ssn}','{wb.ModelName}','{MSG}','{ssn}','{controller}')");
                        return Ok(MSG) ;
                    }
                    
                    string sqlPM_S = $@"select if(max(idProduct_Movement)>0,max(idProduct_Movement),0) from product_movement where idProduct = {ID} and Status_Code = '1' and WorkGroup = 'ASSEMBLY'";
                    try
                    {
                        MSG = "set result=Problema nao existe registro no ASSEMBLY";
                        int PMID_S = ConexaoDB.CRUDValor_tabela(sqlPM_S);
                        if (PMID_S == 0)
                        {
                            ConexaoDB.CRUDU_ID_tabela($@"insert into logruninnb (log,Model,MSG,SSN,controller) values ('{ssn}','{product.Product}','{MSG}','{ssn}','{controller}')");
                           // return Ok(MSG);
                        }
                        else
                        {
                            string sqlPM_pretest = $@"update product_movement set next_Station='1' where idProduct_Movement = {PMID_S}";
                            ConexaoDB.CRUD_tabela(sqlPM_pretest);
                        }
                    }
                    catch (Exception)
                    {
                        ConexaoDB.CRUDU_ID_tabela($@"insert into logruninnb (log,Model,MSG,SSN,controller) values ('{ssn}','{product.Product}','{MSG}','{ssn}','{controller}')");
                        return Ok(MSG);
                    }

                }
                else
                {
                    MSG = "set result=Error WebService" + SFIS_CHECK_STATUS.ErrorMessage;
                    ConexaoDB.CRUDU_ID_tabela($@"insert into logruninnb (log,Model,MSG,SSN,controller) values ('{ssn}','{wb.ModelName}','{MSG}','{ssn}','{controller}')");
                    return Ok(MSG) ;
                }
            }
            MSG = "set result=0";
            return Ok(MSG); 
        }
    }
}
