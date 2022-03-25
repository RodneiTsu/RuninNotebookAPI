﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using RuninNotebookAPI.Models;
using RuninNotebookAPI.DB;
using System.Web.Http;
using System.Data;

namespace RuninNotebookAPI.Controllers
{
    public class SendDataController : ApiController
    {
        public object MSG { get; private set; }

        [HttpGet]
        public IHttpActionResult Send_GET(string ssn)
        {
            if (ssn is null)
            {
                NotFound();
            }



            string[] Columns = ssn.Split(',');

            WebSFIS_GET wb = new WebSFIS_GET();
            Produto product = new Produto();
            NBMAC nbmac = new NBMAC();


            if (Columns[0].Length == 15 || Columns[0].Length == 12 || Columns[0].Length == 22)
            {
                if (Columns[0].ToUpper().ToString().ToUpper().Substring(4, 2) == "B6")
                {
                    wb.CustomerCode = "ASUS";
                }
                else if (Columns[0].ToUpper().ToString().ToUpper().Substring(9, 3) == "935" || Columns[0].Length == 22)
                {
                    wb.CustomerCode = "ACER";
                }
                else if (Columns[0].ToUpper().ToString().ToUpper().Substring(10, 2) == "TL")
                {
                    wb.CustomerCode = "HUAWEI";
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

            product.Serial_Number = Columns[0];
            product.CustomerSerial = Columns[1];
            product.WorkOrder = Columns[2];
            nbmac.WLANMAC = Columns[3];
            nbmac.BTMAC = Columns[4];
            nbmac.LANMAC = Columns[5];
            nbmac.UUID = Columns[6];
           
            
            int pos = 0;
            string nome = "";
            foreach(string vazio in Columns)
            {
                if (string.IsNullOrEmpty(vazio))
                {
                    if(pos==0)
                    { nome = "Serial_Number"; }
                    else if(pos==1)
                    { nome = "CustomerSerial"; }
                    else if (pos == 2)
                    { nome = "WorkOrder"; }
                    else if (pos == 6)
                    { nome = "UUID"; }

                    if ( wb.CustomerCode=="ASUS" && pos == 1 )
                    {
                        
                    }
                    else
                    {
                        MSG = $@"Campo vazio {nome}";
                        return Json(MSG);
                    }
                }
                pos++;
            }

            try
            {
                if (wb.CustomerCode == "ASUS")
                {
                    if (nbmac.WLANMAC.Length > 0 && nbmac.BTMAC.Length > 0)
                    {
                        string sqlasus = $@"select count(ID_MAC) from nbmac where SSN='{product.Serial_Number}";
                        string sqltemMAC = $@"select ID_MAC,SSN,mac,uuid,datecreate from nbmac where mac in ('{nbmac.WLANMAC}','{nbmac.BTMAC}','{nbmac.LANMAC}') order by ID_MAC";


                        int qtdSSN = ConexaoDB.CRUDValor_tabela(sqlasus);

                        if (qtdSSN>0)
                        {
                            DataTable ASUSupdate = ConexaoDB.Carrega_Tabela(sqltemMAC);
                            if (ASUSupdate.Rows.Count == 2)
                            {
                                int i = 1;
                                foreach (DataRow linha in ASUSupdate.Rows)
                                {
                                    if (product.Serial_Number == linha[1].ToString())
                                    {

                                        if (i == 1)
                                        { ConexaoDB.CRUD_tabela($@"update nbmac SET MAC ='{nbmac.WLANMAC}',UUID='{nbmac.UUID}' where id_mac=" + linha[0]); }
                                        else
                                        { ConexaoDB.CRUD_tabela($@"update nbmac SET MAC ='{nbmac.BTMAC}' ,UUID='{nbmac.UUID}' where id_mac=" + linha[0]); }
                                    }
                                    else
                                    {
                                        MSG = $@"ssn={linha[1]},MAC={linha[2]},UUID={linha[3]},Dt criação={linha[4]}";
                                        return Json(MSG);
                                    }
                                    i++;
                                }
                            }
                        }
                        else
                        {
                            int temMAC =  ConexaoDB.CRUDValor_tabela(sqltemMAC);
                            if (temMAC>0)
                            {
                                MSG = $@"MAC duplicado WLANMAC->{Columns[3]} ou BTMAC->{Columns[4]}";
                                return Json(MSG);
                            }
                            MSG = $@"Erro ao gravar na tabela de NBMAC";
                            string gravaWLANMAC = $@"INSERT INTO nbmac (MAC,UUID,SSN) VALUES ('{nbmac.WLANMAC}','{nbmac.UUID}','{product.Serial_Number}')";
                            ConexaoDB.CRUD_tabela(gravaWLANMAC);
                            string gravaBTMAC = $@"INSERT INTO nbmac (MAC,UUID,SSN) VALUES ('{nbmac.BTMAC}','{nbmac.UUID}','{product.Serial_Number}')";
                            ConexaoDB.CRUD_tabela(gravaBTMAC);
                        }
                    }
                    else
                    {
                        MSG = $@"WLANMAC->{nbmac.WLANMAC} ou BTMAC->{nbmac.BTMAC} ou QTD de MAC do cliente {wb.CustomerCode} sem valores";
                        return Json(MSG);
                    }
                }
                else if (wb.CustomerCode == "ACER")
                {
                    if (nbmac.WLANMAC.Length > 0 && nbmac.BTMAC.Length > 0 && nbmac.LANMAC.Length > 0)
                    {
                        string sqlacer = $@"select count(ID_MAC) from nbmac where SSN='{product.CustomerSerial}'";
                        string sqltemMAC = $@"select ID_MAC,SSN,mac,uuid,datecreate from nbmac where mac in ('{nbmac.WLANMAC}','{nbmac.BTMAC}','{nbmac.LANMAC}') order by ID_MAC";


                        int qtdSSN = ConexaoDB.CRUDValor_tabela(sqlacer);

                        if (qtdSSN > 0)
                        {
                            DataTable ACERupdate = ConexaoDB.Carrega_Tabela(sqltemMAC);
                            if (ACERupdate.Rows.Count == 3)
                            {
                                int i = 1;
                                foreach (DataRow linha in ACERupdate.Rows)
                                {
                                    if (product.CustomerSerial == linha[1].ToString())
                                    {

                                        if (i == 1)
                                        { ConexaoDB.CRUD_tabela($@"update nbmac SET MAC ='{nbmac.WLANMAC}',UUID='{nbmac.UUID}' where id_mac=" + linha[0]); }
                                        else if(i == 2)
                                        { ConexaoDB.CRUD_tabela($@"update nbmac SET MAC ='{nbmac.BTMAC}' ,UUID='{nbmac.UUID}' where id_mac=" + linha[0]); }
                                        else if  (i == 3)
                                        { ConexaoDB.CRUD_tabela($@"update nbmac SET MAC ='{nbmac.LANMAC}' ,UUID='{nbmac.UUID}' where id_mac=" + linha[0]); }
                                    }
                                    else
                                    {
                                        MSG = $@"ssn={linha[1]},MAC={linha[2]},UUID={linha[3]},Dt criação={linha[4]}";
                                        return Json(MSG);
                                    }
                                    i++;
                                }
                            }
                        }
                        else
                        {
                            sqltemMAC = $@"select count(ID_MAC) from nbmac where mac in ('{nbmac.WLANMAC}','{nbmac.BTMAC}','{nbmac.LANMAC}') order by ID_MAC";
                            int temMAC = ConexaoDB.CRUDValor_tabela(sqltemMAC);
                            if (temMAC > 0)
                            {
                                MSG = $@"MAC duplicado WLANMAC->{Columns[3]} ou BTMAC->{Columns[4]}";
                                return Json(MSG);
                            }
                            MSG = $@"Erro ao gravar na tabela de NBMAC";
                            string gravaWLANMAC = $@"INSERT INTO nbmac (MAC,UUID,SSN) VALUES ('{nbmac.WLANMAC}','{nbmac.UUID}','{product.CustomerSerial}')";
                            ConexaoDB.CRUD_tabela(gravaWLANMAC);
                            string gravaBTMAC = $@"INSERT INTO nbmac (MAC,UUID,SSN) VALUES ('{nbmac.BTMAC}','{nbmac.UUID}','{product.CustomerSerial}')";
                            ConexaoDB.CRUD_tabela(gravaBTMAC);
                            string gravaLANMAC = $@"INSERT INTO nbmac (MAC,UUID,SSN) VALUES ('{nbmac.LANMAC}','{nbmac.UUID}','{product.CustomerSerial}')";
                            ConexaoDB.CRUD_tabela(gravaLANMAC);
                        }

                    }
                    else
                    {
                        MSG = $@"WLANMAC->{nbmac.WLANMAC} , BTMAC->{nbmac.BTMAC} , BTMAC->{nbmac.LANMAC} ou  QTD de MAC do cliente {wb.CustomerCode} sem valores";
                        return Json(MSG);
                    }
                }
                ConexaoDB.CRUD_tabela($@"update product set workOrder='{product.WorkOrder}', CustomerSerial='{product.CustomerSerial}', UUID='{nbmac.UUID}' where Serial_Number='{product.Serial_Number}'");
            }
            catch (Exception)
            { 
                MSG = "erro ao gravar tabela product";
                return Ok(MSG);
            }
            MSG = "set result=0";
            return Ok(MSG);
        }
    }
}
