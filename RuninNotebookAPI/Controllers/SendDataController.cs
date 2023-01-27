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

        public string registroDB { get; set; }
        public string controller { get; set; }

        [HttpGet]
        public IHttpActionResult Send_GET(string ssn)
        {
            if (ssn is null)
            {
                NotFound();
            }

            controller = "SendData";
            string[] Columns = ssn.Split(',');

            WebSFIS_GET wb = new WebSFIS_GET();
            Produto product = new Produto();
            NBMAC nbmac = new NBMAC();
            MSG = "set result=0";

            string sqlProduct = $@"SELECT idProduct, idProduct_SKU, Serial_Number, CustomerSerial, WorkOrder, UUID, SKU, Color_ID, Product, Model, Customer, Status_Code, Dt_Creat, WorkStation, ";

            if (Columns[0].Length == 15 || Columns[0].Length == 12 || Columns[0].Length == 22)
            {
                if (Columns[0].ToUpper().ToString().ToUpper().Substring(4, 2) == "B6")
                {
                    wb.CustomerCode = "ASUS";
                    sqlProduct += $@"Download, Dt_GetIn, Dt_GetOut, idSwitch, S_60, S_MBSN FROM product WHERE Serial_Number = '{Columns[0].ToUpper()}'";
                }
                else if (Columns[0].ToUpper().ToString().ToUpper().Substring(9, 3) == "935" )
                {
                    wb.CustomerCode = "ACER";
                    sqlProduct += $@"Download, Dt_GetIn, Dt_GetOut, idSwitch, S_60, S_MBSN FROM product WHERE Serial_Number = '{Columns[0].ToUpper()}'";
                }
                else if ( Columns[0].Length == 22)
                {
                    wb.CustomerCode = "ACER";
                    sqlProduct += $@"Download, Dt_GetIn, Dt_GetOut, idSwitch, S_60, S_MBSN FROM product WHERE CustomerSerial = '{Columns[0].ToUpper()}'";
                }
                else if (Columns[0].ToUpper().ToString().ToUpper().Substring(10, 2) == "TL")
                {
                    wb.CustomerCode = "HUAWEI";
                    sqlProduct += $@"Download, Dt_GetIn, Dt_GetOut, idSwitch, S_60, S_MBSN FROM product WHERE Serial_Number = '{Columns[0].ToUpper()}'";
                }
                else
                {
                    MSG = "set result=It was not possible to define a valid customer";
                    ConexaoDB.CRUD_tabela($@"insert into logruninnb (log,MSG) values ('{ssn}','{MSG}')");
                    return Ok(MSG);
                }
            }
            else
            {
                MSG = "set result=Serial number length is invalid";
                ConexaoDB.CRUDU_ID_tabela($@"insert into logruninnb (log,MSG,controller) values ('{ssn}','{MSG}','{controller}')");
                return Ok(MSG);
            }

            DataTable dtProduct = ConexaoDB.Carrega_Tabela(sqlProduct);

            if (dtProduct.Rows.Count <= 0)
            {
                MSG = "set result=Is was not record in DB - PRODUCT";
                ConexaoDB.CRUDU_ID_tabela($@"insert into logruninnb (log,MSG,controller) values ('{ssn}','{MSG}','{controller}')");
                return Ok(MSG);
            }

            foreach(DataRow lin in dtProduct.Rows)
            {
                product.idProduct = Convert.ToInt32(lin[0]);
                //product.idProduct_SKU = Convert.ToInt32(lin[1]);
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
                if (lin[15].ToString() != "")
                {
                    product.Dt_GetIn = Convert.ToDateTime(lin[15]);
                }
                if (lin[16].ToString() != "")
                {
                    product.Dt_GetOut = Convert.ToDateTime(lin[16]);
                }
                if (Convert.ToInt32(lin[17]) > 0)
                {
                    product.idSwitch = Convert.ToInt32(lin[17]);
                }
                product.S_60 = lin[18].ToString();
                product.S_MBSN = lin[19].ToString();
            }

            string SQLnbmac = string.Empty;


            if (wb.CustomerCode == "ACER")
            {
                SQLnbmac = $@"SELECT ID_MAC, MAC, UUID, SSN, DATECREATE from NBMAC where SSN='{Columns[1]}' order by ID_MAC asc";
            }
            else
            {
                SQLnbmac = $@"SELECT ID_MAC, MAC, UUID, SSN, DATECREATE from NBMAC where SSN='{Columns[0]}' order by ID_MAC asc";
            }

            if (Columns[3].ToString() != "")
            {
                DataTable WLANMAC_DB = ConexaoDB.Carrega_Tabela($@"select ID_MAC, MAC, UUID, SSN, DATECREATE from nbmac where MAC='{Columns[3]}'");
                if (WLANMAC_DB.Rows.Count >= 0)
                {
                    string MACSSN = Columns[0].ToString();

                    foreach (DataRow linha in WLANMAC_DB.Rows)
                    {
                        if (product.Customer == "ACER")
                            MACSSN = Columns[1].ToString();

                        if (linha[3].ToString() != MACSSN)
                        {
                            MSG = $@"set result=DUPLICADO WLAMAC {Columns[3]} entrada SSN {MACSSN} gravado DB ID {linha[0]} SSN {linha[3]} DATACREATE {linha[4]}";
                            ConexaoDB.CRUDU_ID_tabela($@"insert into logruninnb (log,MSG,controller) values ('{ssn}','{MSG}','{controller}')");
                            return Ok(MSG);
                        }
                    }
                }
            }

            if (Columns[4].ToString() != "")
            {
                DataTable WLANMAC_DB = ConexaoDB.Carrega_Tabela($@"select ID_MAC, MAC, UUID, SSN, DATECREATE from nbmac where MAC='{Columns[4]}'");
                if (WLANMAC_DB.Rows.Count >= 0)
                {
                    string MACSSN = Columns[0].ToString();

                    foreach (DataRow linha in WLANMAC_DB.Rows)
                    {
                        if (product.Customer == "ACER")
                            MACSSN = Columns[1].ToString();

                        if (linha[3].ToString() != MACSSN)
                        {
                            MSG = $@"set result=DUPLICADO WLAMAC {Columns[4]} entrada SSN {MACSSN} gravado DB ID {linha[0]} SSN {linha[3]} DATACREATE {linha[4]}";
                            ConexaoDB.CRUDU_ID_tabela($@"insert into logruninnb (log,MSG,controller) values ('{ssn}','{MSG}','{controller}')");
                            return Ok(MSG);
                        }
                    }
                }
            }

            if (Columns[5].ToString() != "")
            {
                DataTable WLANMAC_DB = ConexaoDB.Carrega_Tabela($@"select ID_MAC, MAC, UUID, SSN, DATECREATE from nbmac where MAC='{Columns[5]}'");
                if (WLANMAC_DB.Rows.Count >= 0)
                {
                    string MACSSN = Columns[0].ToString();

                    foreach (DataRow linha in WLANMAC_DB.Rows)
                    {
                        if (product.Customer == "ACER")
                            MACSSN = Columns[1].ToString();

                        if (linha[3].ToString() != MACSSN)
                        {
                            MSG = $@"set result=DUPLICADO WLAMAC {Columns[5]} entrada SSN {MACSSN} gravado DB ID {linha[0]} SSN {linha[3]} DATACREATE {linha[4]}";
                            ConexaoDB.CRUDU_ID_tabela($@"insert into logruninnb (log,MSG,controller) values ('{ssn}','{MSG}','{controller}')");
                            return Ok(MSG);
                        }
                    }
                }
            }

            DataTable dtnbmac = ConexaoDB.Carrega_Tabela(SQLnbmac);

            //dtnbmac.DefaultView.Sort = "ID_MAC asc";

            int reg = 1;

            foreach (DataRow lin in dtnbmac.Rows)
            {
               if (reg==1) 
                    nbmac.DATECREATE = lin[4].ToString();

                registroDB += lin[0].ToString() + " " + lin[1].ToString() + " " + lin[3].ToString() + " - ";
                reg++;
            }

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
                            MSG = $@"set result=Notebook {wb.CustomerCode} {nome} vazio";
                            ConexaoDB.CRUDU_ID_tabela($@"insert into logruninnb (log,MSG,controller) values ('{ssn}','{MSG}','{controller}')");
                            return Ok(MSG);
                        }
                    }
                    else
                    {
                        if (product.CustomerSerial.Substring(0, 1) == "N" && pos == 3)
                        {
                            MSG = $@"set result=Notebook {wb.CustomerCode} WLANMAC {nbmac.WLANMAC} vazio";
                            ConexaoDB.CRUDU_ID_tabela($@"insert into logruninnb (log,MSG,controller) values ('{ssn}','{MSG}','{controller}')");
                            return Ok(MSG);
                        }
                        else if (product.CustomerSerial.Substring(0, 1) == "N" && pos == 4)
                        {
                            MSG = $@"set result=Notebook {wb.CustomerCode} BTMAC {nbmac.BTMAC} vazio ";
                            ConexaoDB.CRUDU_ID_tabela($@"insert into logruninnb (log,MSG,controller) values ('{ssn}','{MSG}','{controller}')");
                            return Ok(MSG);
                        }
                        else if (product.CustomerSerial.Substring(0, 1) == "N" && pos == 5)
                        {
                            if (product.Product != "SF314-511")
                            {
                                MSG = $@"set result=Notebook {wb.CustomerCode} LANMAC {nbmac.BTMAC} vazio ";
                                ConexaoDB.CRUDU_ID_tabela($@"insert into logruninnb (log,MSG,controller) values ('{ssn}','{MSG}','{controller}')");
                                return Ok(MSG);
                            }
                        }
                        else if (product.CustomerSerial.Substring(0, 1) == "D" && pos == 5)
                        {
                            MSG = $@"set result=Desktop {wb.CustomerCode} lanmac vazio ";
                            ConexaoDB.CRUDU_ID_tabela($@"insert into logruninnb (log,MSG,controller) values ('{ssn}','{MSG}','{controller}')");
                            return Ok(MSG);
                        }
                        else if (pos == 0 || pos == 1 || pos == 2 || pos == 6)
                        {
                            MSG = $@"set result={nome} vazio ";
                            ConexaoDB.CRUDU_ID_tabela($@"insert into logruninnb (log,MSG,controller) values ('{ssn}','{MSG}','{controller}')");
                            return Ok(MSG);
                        }
                    }
                }
                pos++;
            }
            try
            {
                //==============================================================================================================================================================================
                if (wb.CustomerCode == "ASUS")
                {
                    if (nbmac.WLANMAC.Length > 0 && nbmac.BTMAC.Length > 0 && nbmac.LANMAC.Length > 0)
                    {
                        if (dtnbmac.Rows.Count == 0)
                        {
                            MSG = $@"set result=INSERT id {product.idProduct} SSN {product.Serial_Number} WLAMAC {nbmac.WLANMAC} BTMAC {nbmac.BTMAC} LANMAC {nbmac.LANMAC}";
                            string gravaWLANMAC = $@"INSERT INTO nbmac (MAC,UUID,SSN) VALUES ('{nbmac.WLANMAC}','{nbmac.UUID}','{product.Serial_Number}')";
                            ConexaoDB.CRUD_tabela(gravaWLANMAC);
                            string gravaBTMAC = $@"INSERT INTO nbmac (MAC,UUID,SSN) VALUES ('{nbmac.BTMAC}','{nbmac.UUID}','{product.Serial_Number}')";
                            ConexaoDB.CRUD_tabela(gravaBTMAC);
                            string gravaLANMAC = $@"INSERT INTO nbmac (MAC,UUID,SSN) VALUES ('{nbmac.LANMAC}','{nbmac.UUID}','{product.Serial_Number}')";
                            ConexaoDB.CRUD_tabela(gravaBTMAC);
                            MSG = $@"set result=0";
                        }
                        else if (dtnbmac.Rows.Count == 3)
                        {
                            int i = 1;
                            foreach (DataRow linha in dtnbmac.Rows)
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
                                    MSG = $@"set result=UPDATE id {product.idProduct} SSN {product.Serial_Number} WLAMAC {nbmac.WLANMAC} BTMAC {nbmac.BTMAC} LANMAC {nbmac.LANMAC}";
                                    ConexaoDB.CRUDU_ID_tabela($@"insert into logruninnb (log,MSG,controller) values ('{ssn}','{MSG}','{controller}')");
                                    return Ok("set result =" + MSG);
                                }
                                i++;
                            }
                        }
                    }
                    else if (nbmac.WLANMAC.Length > 0 && nbmac.BTMAC.Length > 0)
                    {
                        if (dtnbmac.Rows.Count == 0)
                        {
                            MSG = $@"set result=INSERT id {product.idProduct} SSN {product.Serial_Number} WLAMAC {nbmac.WLANMAC} BTMAC {nbmac.BTMAC}";
                            string gravaWLANMAC = $@"INSERT INTO nbmac (MAC,UUID,SSN) VALUES ('{nbmac.WLANMAC}','{nbmac.UUID}','{product.Serial_Number}')";
                            ConexaoDB.CRUD_tabela(gravaWLANMAC);
                            string gravaBTMAC = $@"INSERT INTO nbmac (MAC,UUID,SSN) VALUES ('{nbmac.BTMAC}','{nbmac.UUID}','{product.Serial_Number}')";
                            ConexaoDB.CRUD_tabela(gravaBTMAC);
                            MSG = "set result=0";
                        }
                        else if (dtnbmac.Rows.Count == 2)
                        {
                            int i = 1;
                            foreach (DataRow linha in dtnbmac.Rows)
                            {
                                try
                                {
                                    if (i == 1)
                                    { ConexaoDB.CRUD_tabela($@"update nbmac SET MAC ='{nbmac.WLANMAC}',UUID='{nbmac.UUID}' where id_mac=" + linha[0]); }
                                    else
                                    { ConexaoDB.CRUD_tabela($@"update nbmac SET MAC ='{nbmac.BTMAC}' ,UUID='{nbmac.UUID}' where id_mac=" + linha[0]); }
                                }
                                catch (Exception)
                                {
                                    MSG = $@"set result=UPDATE id {product.idProduct} SSN {product.Serial_Number} WLAMAC {nbmac.WLANMAC} BTMAC {nbmac.BTMAC}";
                                    ConexaoDB.CRUDU_ID_tabela($@"insert into logruninnb (log,MSG,controller) values ('{ssn}','{MSG}','{controller}')");
                                    return Ok("set result=" + MSG) ;
                                }
                                i++;
                            }
                        }
                    }
                    else
                    {
                        MSG = $@"set result=WLANMAC{nbmac.WLANMAC} ou BTMAC{nbmac.BTMAC} ou QTD de MAC do cliente {wb.CustomerCode} sem valores";
                        ConexaoDB.CRUDU_ID_tabela($@"insert into logruninnb (log,MSG,controller) values ('{ssn}','{MSG}','{controller}')");
                        return Ok(MSG) ;
                    }
                }
                //==============================================================================================================================================================================
                else if (wb.CustomerCode == "ACER")
                {
                    if (nbmac.WLANMAC.Length > 0 && nbmac.BTMAC.Length > 0 && nbmac.LANMAC.Length > 0)
                    {
                        if (dtnbmac.Rows.Count == 0)
                        {
                            MSG = $@"set result=Problema INSERT MAC {nbmac.WLANMAC} {nbmac.BTMAC} {nbmac.LANMAC} - DB {registroDB}";
                            string gravaWLANMAC = $@"INSERT INTO nbmac (MAC,UUID,SSN) VALUES ('{nbmac.WLANMAC}','{nbmac.UUID}','{product.CustomerSerial}')";
                            ConexaoDB.CRUD_tabela(gravaWLANMAC);

                            string gravaBTMAC = $@"INSERT INTO nbmac (MAC,UUID,SSN) VALUES ('{nbmac.BTMAC}','{nbmac.UUID}','{product.CustomerSerial}')";
                            ConexaoDB.CRUD_tabela(gravaBTMAC);

                            string gravaLANMAC = $@"INSERT INTO nbmac (MAC,UUID,SSN) VALUES ('{nbmac.LANMAC}','{nbmac.UUID}','{product.CustomerSerial}')";
                            ConexaoDB.CRUD_tabela(gravaLANMAC);
   
                        }
                        else if (dtnbmac.Rows.Count == 3)
                        {
                            int i = 1;
                            MSG = $@"set result=Problema UPDATE MAC {nbmac.WLANMAC} {nbmac.BTMAC} {nbmac.LANMAC} - DB {registroDB}";
                            foreach (DataRow linha in dtnbmac.Rows)
                            {
                                if (i == 1)
                                { ConexaoDB.CRUD_tabela($@"update nbmac SET MAC ='{nbmac.WLANMAC}',UUID='{nbmac.UUID}' where id_mac=" + linha[0]); }
                                else if (i == 2)
                                { ConexaoDB.CRUD_tabela($@"update nbmac SET MAC ='{nbmac.BTMAC}' ,UUID='{nbmac.UUID}' where id_mac=" + linha[0]); }
                                else if (i == 3)
                                { ConexaoDB.CRUD_tabela($@"update nbmac SET MAC ='{nbmac.LANMAC}' ,UUID='{nbmac.UUID}' where id_mac=" + linha[0]); }
                                i++;
                            }
                        }
                        else
                        {
                            MSG = $@"set result=Problema MAC {nbmac.WLANMAC} {nbmac.BTMAC} {nbmac.LANMAC} - DB {registroDB}";
                            ConexaoDB.CRUDU_ID_tabela($@"insert into logruninnb (log,MSG,controller) values ('{ssn}','{MSG}','{controller}')");
                            return Ok(MSG);
                        }
                        MSG = "set result=0";
                    }
                    else if (nbmac.WLANMAC.Length > 0 && nbmac.BTMAC.Length > 0)
                    {
                        if (dtnbmac.Rows.Count == 0)
                        {
                            MSG = $@"set result=Problema INSERT MAC {nbmac.WLANMAC} {nbmac.BTMAC} - DB {registroDB}";
                            string gravaWLANMAC = $@"INSERT INTO nbmac (MAC,UUID,SSN) VALUES ('{nbmac.WLANMAC}','{nbmac.UUID}','{product.CustomerSerial}')";
                            ConexaoDB.CRUD_tabela(gravaWLANMAC);
                            string gravaBTMAC = $@"INSERT INTO nbmac (MAC,UUID,SSN) VALUES ('{nbmac.BTMAC}','{nbmac.UUID}','{product.CustomerSerial}')";
                            ConexaoDB.CRUD_tabela(gravaBTMAC);
                            
                        }
                        else if (dtnbmac.Rows.Count == 2)
                        {
                            MSG = $@"set result=Problema UPDATE MAC {nbmac.WLANMAC} {nbmac.BTMAC} - DB {registroDB}";
                            int i = 1;
                            foreach (DataRow linha in dtnbmac.Rows)
                            {
                                    if (i == 1)
                                    { ConexaoDB.CRUD_tabela($@"update nbmac SET MAC ='{nbmac.WLANMAC}',UUID='{nbmac.UUID}' where id_mac=" + linha[0]); }
                                    else if (i == 2)
                                    { ConexaoDB.CRUD_tabela($@"update nbmac SET MAC ='{nbmac.BTMAC}' ,UUID='{nbmac.UUID}' where id_mac=" + linha[0]); }  
                                i++;
                            }
                        }
                        else
                        {
                            MSG = $@"set result=Problema MAC {nbmac.WLANMAC} {nbmac.BTMAC} - DB {registroDB}";
                            ConexaoDB.CRUDU_ID_tabela($@"insert into logruninnb (log,MSG,controller) values ('{ssn}','{MSG}','{controller}')");
                            return Ok(MSG);
                        }
                        MSG = "set result=0";
                    }
                    else if (nbmac.LANMAC.Length > 0)
                    {
                        if (dtnbmac.Rows.Count == 0)
                        {
                            MSG = $@"set result=Problema INSERT MAC DESKTOP {nbmac.LANMAC} - DB {registroDB}";
                            ConexaoDB.CRUD_tabela($@"INSERT INTO nbmac (MAC,UUID,SSN) VALUES ('{nbmac.LANMAC}','{nbmac.UUID}','{product.CustomerSerial}')");
                        }
                        else if (dtnbmac.Rows.Count == 1)
                        {
                            MSG = $@"set result=Problema UPDATE MAC DESKTO {nbmac.LANMAC} - DB {registroDB}";
                            foreach (DataRow linha in dtnbmac.Rows)
                            {
                                ConexaoDB.CRUD_tabela($@"update nbmac SET MAC ='{nbmac.LANMAC}' ,UUID='{nbmac.UUID}' where id_mac=" + linha[0]);
                            }
                        }
                        else
                        {
                            MSG = $@"set result=Problema INSERT MAC DESKTOP {nbmac.LANMAC}  - DB {registroDB}";
                            ConexaoDB.CRUDU_ID_tabela($@"insert into logruninnb (log,MSG,controller) values ('{ssn}','{MSG}','{controller}')");
                            return Ok(MSG);
                        }
                        MSG = "set result=0";
                    }
                    else
                    {
                        MSG = $@"set result=Problema MAC DESKTOP {nbmac.LANMAC} - DB {registroDB}";
                        ConexaoDB.CRUDU_ID_tabela($@"insert into logruninnb (log,MSG,controller) values ('{ssn}','{MSG}','{controller}')");
                        return Ok(MSG) ;
                    }
                }
                MSG = $@"set result=Problema update em Product - saida";
                if (Columns.Length > 7)
                {
                    ConexaoDB.CRUD_tabela($@"update product set workOrder='{Columns[2]}', CustomerSerial='{Columns[1]}', UUID='{Columns[6]}', S_60='{Columns[7]}', S_MBSN='{Columns[8]}' where idProduct='{product.idProduct}'");
                }
                else
                {
                    ConexaoDB.CRUD_tabela($@"update product set workOrder='{Columns[2]}', CustomerSerial='{Columns[1]}', UUID='{Columns[6]}' where idProduct='{product.idProduct}'");
                }
                MSG = "set result=0";
            }
            catch (Exception)
            {
                ConexaoDB.CRUDU_ID_tabela($@"insert into logruninnb (log,MSG,controller) values ('{ssn}','{MSG}','{controller}')");
                return Ok(MSG) ;
            }
            return Ok(MSG) ;
        }
    }
}
