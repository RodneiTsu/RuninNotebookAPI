using System;
using RuninNotebookAPI.Models;
using RuninNotebookAPI.DB;
using System.Web.Http;
using System.Data;
using System.Collections.Generic;
using System.Linq;

namespace RuninNotebookAPI.Controllers
{
    public class SendDataController : ApiController
    {
        public string MSG { get; set; }

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
            MSG = "set result=0";

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
                    return Ok(MSG);
                }
            }
            else
            {
                MSG = "set result=Serial number length is invalid";
                return Ok(MSG);
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
                    if (pos == 0)
                    { nome = "Serial_Number"; }
                    else if (pos == 1)
                    { nome = "CustomerSerial"; }
                    else if (pos == 2)
                    { nome = "WorkOrder"; }
                    else if (pos == 3)
                    { nome = "WLANMAC"; }
                    else if (pos == 4)
                    { nome = "BTMAC"; }
                    else if (pos == 5)
                    { nome = "LANMAC"; }
                    else if (pos == 6)
                    { nome = "UUID"; }


                    if (wb.CustomerCode == "ASUS")
                    {
                        if (pos == 0 || pos == 2 || pos == 3 || pos == 4 || pos == 6)
                        {
                            MSG = $@"set result=Notebook {wb.CustomerCode} {nome} vazio!!!! ";
                            return Ok(MSG);
                        }
                    }
                    else
                    {
                        if (product.CustomerSerial.Substring(0, 1) == "N" && pos == 3)
                        {
                            MSG = $@"set result=Notebook {wb.CustomerCode} WLANMAC->{nbmac.WLANMAC} vazio!!!! ";
                            return Ok(MSG);
                        }
                        else if (product.CustomerSerial.Substring(0, 1) == "N" && pos == 4)
                        {
                            MSG = $@"set result=Notebook {wb.CustomerCode} BTMAC->{nbmac.BTMAC} vazio!!!! ";
                            return Ok(MSG);
                        }
                        else if (product.CustomerSerial.Substring(0, 1) == "N" && pos == 5)
                        {
                            MSG = $@"set result=Notebook {wb.CustomerCode} LANMAC->{nbmac.BTMAC} vazio!!!! ";
                            return Ok(MSG);
                        }
                        else if (product.CustomerSerial.Substring(0, 1) == "D" && pos == 5)
                        {
                            MSG = $@"set result=Desktop {wb.CustomerCode} lanmac vazio!!!! ";
                            return Ok(MSG);
                        }
                        else if (pos == 0 || pos == 1 || pos == 2 || pos == 6)
                        {
                            MSG = $@"set result={nome} vazio!!!! ";
                            return Ok(MSG);
                        }
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
                        string sqltemMAC = $@"select ID_MAC,SSN,mac,uuid,datecreate from nbmac where SSN='{product.Serial_Number}' order by ID_MAC";

                        DataTable ASUSupdate = ConexaoDB.Carrega_Tabela(sqltemMAC);
                        if (ASUSupdate.Rows.Count == 2)
                        {
                            int i = 1;
                            foreach (DataRow linha in ASUSupdate.Rows)
                            {
                                try
                                {
                                    if (i == 1)
                                    { ConexaoDB.CRUD_tabela($@"update nbmac SET MAC ='{nbmac.WLANMAC}',UUID='{nbmac.UUID}' where id_mac=" + linha[0]); }
                                    else
                                    { ConexaoDB.CRUD_tabela($@"update nbmac SET MAC ='{nbmac.BTMAC}' ,UUID='{nbmac.UUID}' where id_mac=" + linha[0]); }
                                }
                                catch (Exception e)
                                {

                                   return Ok("set result=" + e.ToString()) ;
                                }
                                
                                i++;
                            }
                        }
                        else if (ASUSupdate.Rows.Count == 3)
                        {
                            int i = 1;
                            foreach (DataRow linha in ASUSupdate.Rows)
                            {
                                try
                                {
                                    if (i == 1)
                                    { ConexaoDB.CRUD_tabela($@"update nbmac SET MAC ='{nbmac.WLANMAC}',UUID='{nbmac.UUID}' where id_mac=" + linha[0]); }
                                    else if (i == 2)
                                    { ConexaoDB.CRUD_tabela($@"update nbmac SET MAC ='{nbmac.BTMAC}' ,UUID='{nbmac.UUID}' where id_mac=" + linha[0]); }
                                    else if (i == 3)
                                    { ConexaoDB.CRUD_tabela($@"update nbmac SET MAC ='{nbmac.LANMAC}' ,UUID='{nbmac.UUID}' where id_mac=" + linha[0]); }
                                }
                                catch (Exception e)
                                {

                                    return Ok("set result ="+ e.ToString());
                                }
                                i++;
                            }
                        }
                        else if (ASUSupdate.Rows.Count == 0)
                        {
                            sqltemMAC = $@"select count(ID_MAC) from nbmac where mac in ('{nbmac.WLANMAC}','{nbmac.BTMAC}','{nbmac.LANMAC}') order by ID_MAC";
                            int temMAC =  ConexaoDB.CRUDValor_tabela(sqltemMAC);
                            if (temMAC>0)
                            {
                                MSG = $@"set result=MAC duplicado WLANMAC->{Columns[3]} ou BTMAC->{Columns[4]}";
                                return Ok( MSG) ;
                            }
                            

                            string gravaWLANMAC = $@"INSERT INTO nbmac (MAC,UUID,SSN) VALUES ('{nbmac.WLANMAC}','{nbmac.UUID}','{product.Serial_Number}')";
                            ConexaoDB.CRUD_tabela(gravaWLANMAC);
                            string gravaBTMAC = $@"INSERT INTO nbmac (MAC,UUID,SSN) VALUES ('{nbmac.BTMAC}','{nbmac.UUID}','{product.Serial_Number}')";
                            ConexaoDB.CRUD_tabela(gravaBTMAC);

                            if (!string.IsNullOrEmpty(nbmac.LANMAC))
                            {
                                string gravaLANMAC = $@"INSERT INTO nbmac (MAC,UUID,SSN) VALUES ('{nbmac.LANMAC}','{nbmac.UUID}','{product.Serial_Number}')";
                                ConexaoDB.CRUD_tabela(gravaLANMAC);
                            }

                        }
                    }
                    else
                    {
                        MSG = $@"set result=WLANMAC->{nbmac.WLANMAC} ou BTMAC->{nbmac.BTMAC} ou QTD de MAC do cliente {wb.CustomerCode} sem valores";
                        return Ok(MSG) ;
                    }
                }
                else if (wb.CustomerCode == "ACER")
                {
                    if (nbmac.WLANMAC.Length > 0 && nbmac.BTMAC.Length > 0 && nbmac.LANMAC.Length > 0)
                    {
                        
                        DataTable MACOLD = ConexaoDB.Carrega_Tabela($@"select ID_MAC,SSN,mac,uuid,datecreate from nbmac where SSN='{product.CustomerSerial}' order by ID_MAC");

                        if (MACOLD.Rows.Count == 3)
                        {
                            int i = 1;
                            foreach (DataRow linha in MACOLD.Rows)
                            {
                                try
                                {
                                    if (i == 1)
                                    { ConexaoDB.CRUD_tabela($@"update nbmac SET MAC ='{nbmac.WLANMAC}',UUID='{nbmac.UUID}' where id_mac=" + linha[0]); }
                                    else if (i == 2)
                                    { ConexaoDB.CRUD_tabela($@"update nbmac SET MAC ='{nbmac.BTMAC}' ,UUID='{nbmac.UUID}' where id_mac=" + linha[0]); }
                                    else if (i == 3)
                                    { ConexaoDB.CRUD_tabela($@"update nbmac SET MAC ='{nbmac.LANMAC}' ,UUID='{nbmac.UUID}' where id_mac=" + linha[0]); }
                                }
                                catch (Exception)
                                {
                                    MSG = $@"set result=MAC duplicado WLANMAC/BTMAC/LANMAC ou UUID->{nbmac.WLANMAC} / {nbmac.BTMAC} / {nbmac.LANMAC} ou UUID={nbmac.UUID}";
                                    break;
                                }
                                

                                i++;
                            }
                        }
                        else
                        {
                            string sqltemMAC = $@"select count(ID_MAC) from nbmac where mac in ('{nbmac.WLANMAC}','{nbmac.BTMAC}','{nbmac.LANMAC}') order by ID_MAC";
                            int temMAC = ConexaoDB.CRUDValor_tabela(sqltemMAC);
                            if (temMAC > 0)
                            {
                                MSG = $@"set result=MAC duplicado WLANMAC->{Columns[3]} ou BTMAC->{Columns[4]}";
                                return Ok(MSG) ;
                            }

                            

                            string gravaWLANMAC = $@"INSERT INTO nbmac (MAC,UUID,SSN) VALUES ('{nbmac.WLANMAC}','{nbmac.UUID}','{product.CustomerSerial}')";
                            ConexaoDB.CRUD_tabela(gravaWLANMAC);

                            string gravaBTMAC = $@"INSERT INTO nbmac (MAC,UUID,SSN) VALUES ('{nbmac.BTMAC}','{nbmac.UUID}','{product.CustomerSerial}')";
                            ConexaoDB.CRUD_tabela(gravaBTMAC);

                            string gravaLANMAC = $@"INSERT INTO nbmac (MAC,UUID,SSN) VALUES ('{nbmac.LANMAC}','{nbmac.UUID}','{product.CustomerSerial}')";
                            ConexaoDB.CRUD_tabela(gravaLANMAC);
                        }
                    }
                    else if (nbmac.LANMAC.Length > 0)
                    {
                        string sqltemMAC = $@"select ID_MAC,SSN,mac,uuid,datecreate from nbmac where SSN='{product.CustomerSerial}' order by ID_MAC";

                        DataTable ACERupdate = ConexaoDB.Carrega_Tabela(sqltemMAC);
                        if (ACERupdate.Rows.Count == 1)
                        {
                            foreach (DataRow linha in ACERupdate.Rows)
                            {
                                try
                                {
                                    ConexaoDB.CRUD_tabela($@"update nbmac SET MAC ='{nbmac.LANMAC}' ,UUID='{nbmac.UUID}' where id_mac=" + linha[0]);
                                }
                                catch (Exception e)
                                {

                                    return Json("set result="+e.ToString());
                                }
                               
                            }
                        }
                        else
                        {
                            sqltemMAC = $@"select count(ID_MAC) from nbmac where mac in ('{nbmac.LANMAC}') order by ID_MAC";
                            int temMAC = ConexaoDB.CRUDValor_tabela(sqltemMAC);
                            if (temMAC > 0)
                            {
                                MSG = $@"set result=MAC duplicado LANMAC->{Columns[5]}";
                                return Ok(MSG) ;
                            }

                            

                            string gravaLANMAC = $@"INSERT INTO nbmac (MAC,UUID,SSN) VALUES ('{nbmac.LANMAC}','{nbmac.UUID}','{product.CustomerSerial}')";
                            try
                            {
                                ConexaoDB.CRUD_tabela(gravaLANMAC);
                            }
                            catch (Exception e)
                            {

                                return Json("set result=" + e.ToString());
                            }
                            
                        }
                    }
                    else
                    {
                        MSG = $@"set result=WLANMAC->{nbmac.WLANMAC} , BTMAC->{nbmac.BTMAC} , BTMAC->{nbmac.LANMAC} ou  QTD de MAC do cliente {wb.CustomerCode} sem valores";
                        return Ok(MSG) ;
                    }
                }
                ConexaoDB.CRUD_tabela($@"update product set workOrder='{product.WorkOrder}', CustomerSerial='{product.CustomerSerial}', UUID='{nbmac.UUID}' where Serial_Number='{product.Serial_Number}'");
            }
            catch (Exception)
            { 
                return Ok(MSG) ;
            }
            return Ok(MSG) ;
        }
    }
}
