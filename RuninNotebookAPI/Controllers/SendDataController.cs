using System;
using RuninNotebookAPI.Models;
using RuninNotebookAPI.DB;
using System.Web.Http;
using System.Data;
using System.Reflection;


namespace RuninNotebookAPI.Controllers
{
    public class SendDataController : ApiController
    {
        public string MSG { get; set; }
        public string registroDB { get; set; }
        public string controller { get; set; }
        public string SerialNumber { get; set; }
        public bool MAC_3 { get; set; }
        public int ID { get; set; }

        [HttpGet]
        public IHttpActionResult Send_GET(string ssn)
        {
            if (ssn is null)
            {
                NotFound();
            }

            string[] asse = Assembly.GetExecutingAssembly().FullName.ToString().Split(',');
            controller = "SendData - " + asse[1];

            string[] Columns = ssn.Split(',');
            WebSFIS_GET wb = new WebSFIS_GET();
            
            NBMAC nbmac = new NBMAC();
            MSG = "set result=0";
            MAC_3 = true;

            Columns[0] = Columns[0].ToUpper();

            string sqlProduct = $@"SELECT idProduct, idProduct_SKU, Serial_Number, CustomerSerial, WorkOrder, UUID, SKU, Color_ID, Product, Model, Customer, Status_Code, Dt_Creat, WorkStation, ";

            if (Columns[0].Length == 15 || Columns[0].Length == 12 || Columns[0].Length == 22)
            {
                if (Columns[0].Substring(4, 2) == "B6")
                {
                    wb.CustomerCode = "ASUS";
                    sqlProduct += $@"Download, Dt_GetIn, Dt_GetOut, idSwitch, S_60, S_MBSN FROM product WHERE Serial_Number = '{Columns[0].ToUpper()}'";
                }
                else if (Columns[0].Substring(9, 3) == "935" )
                {
                    wb.CustomerCode = "ACER";
                    sqlProduct += $@"Download, Dt_GetIn, Dt_GetOut, idSwitch, S_60, S_MBSN FROM product WHERE Serial_Number = '{Columns[0].ToUpper()}'";
                }
                else if ( Columns[0].Length == 22)
                {
                    wb.CustomerCode = "ACER";
                    sqlProduct += $@"Download, Dt_GetIn, Dt_GetOut, idSwitch, S_60, S_MBSN FROM product WHERE CustomerSerial = '{Columns[0].ToUpper()}'";
                }
                else if (Columns[0].Substring(10, 2) == "TL")
                {
                    wb.CustomerCode = "HUAWEI";
                    sqlProduct += $@"Download, Dt_GetIn, Dt_GetOut, idSwitch, S_60, S_MBSN FROM product WHERE Serial_Number = '{Columns[0].ToUpper()}'";
                }
                else
                {
                    MSG = $@"set result=001-Problema Serial Number nao validado pelo customer - {Columns[0]}";
                    ConexaoDB.CRUD_tabela($@"insert into logruninnb (log,MSG) values ('{ssn}','{MSG}')");
                    return Ok(MSG);
                }
            }
            else
            {
                MSG = $@"set result=002-Problema tamanho Serial number invalido - {Columns[0]}";
                ConexaoDB.CRUDU_ID_tabela($@"insert into logruninnb (log,MSG,controller) values ('{ssn}','{MSG}','{controller}')");
                return Ok(MSG);
            }

            if (Columns.Length == 7 || Columns.Length == 9)
            {
            }
            else
            {
                MSG = "set result=003-Problema Deve ter SSN,SERIALNUMBER,WORKWORD,WLANMAC,BTMAC,LANMAC,UUID opcao + S_60,S_MBSN";
                ConexaoDB.CRUD_tabela($@"insert into logruninnb (log,MSG,controller) values ('{ssn}','{MSG}','{controller}')");
                return Ok(MSG);
            }

            Produto product = new Produto(Columns[0]);

            ID = product.ID;

            if (ID <= 0)
            {
                MSG = "set result=004-Problema nao exista registro deste produto - PRODUCT";
                ConexaoDB.CRUDU_ID_tabela($@"insert into logruninnb (log,MSG,controller) values ('{ssn}','{MSG}','{controller}')");
                return Ok(MSG);
            }

            Product_SKU product_SKU = new Product_SKU(product.SKU);

            product.CustomerSerial = Columns[1].ToUpper();

            nbmac.WLANMAC = Columns[3];
            nbmac.BTMAC = Columns[4];
            nbmac.LANMAC = Columns[5];

            nbmac.UUID = Columns[6];

            //============================================================================================================================
            if (wb.CustomerCode == "ACER")
            {
                if (product.CustomerSerial.Substring(0, 1) != "D")
                {
                    if (nbmac.WLANMAC == nbmac.BTMAC)
                        MSG = $@"set result=005-Problema WLANMAC {nbmac.WLANMAC} igual a BTMAC {nbmac.BTMAC}";
                    if (nbmac.WLANMAC == nbmac.LANMAC && (MSG == "set result=0"))
                        MSG = $@"set result=006-Problema WLANMAC {nbmac.WLANMAC} igual a LANMAC {nbmac.LANMAC}";
                    if (nbmac.LANMAC == nbmac.BTMAC && (MSG == "set result=0"))
                        MSG = $@"set result=007-Problema LANMAC {nbmac.LANMAC} igual a BTMAC {nbmac.BTMAC}";
                }
            }
            else
            {
                if (nbmac.WLANMAC == nbmac.BTMAC)
                    MSG = $@"set result=005-Problema WLANMAC {nbmac.WLANMAC} igual a BTMAC {nbmac.BTMAC}";
                if (nbmac.WLANMAC == nbmac.LANMAC && (MSG == "set result=0"))
                    MSG = $@"set result=006-Problema WLANMAC {nbmac.WLANMAC} igual a LANMAC {nbmac.LANMAC}";
                if (nbmac.LANMAC == nbmac.BTMAC && (MSG == "set result=0"))
                    MSG = $@"set result=007-Problema LANMAC {nbmac.LANMAC} igual a BTMAC {nbmac.BTMAC}";
            }

            if (wb.CustomerCode == "ACER")
            {
                if (product_SKU.QtdMAC == 2)
                    MAC_3 = false;

                if (string.IsNullOrWhiteSpace(product.CustomerSerial) && (MSG == "set result=0"))
                {
                    MSG = "set result=008-Problema nao existe Customer Serial gravado..";
                }
                else
                {
                    SerialNumber = Columns[1];
                }

                if (product.CustomerSerial.Substring(0, 1) == "D")
                {
                    if (string.IsNullOrWhiteSpace(nbmac.LANMAC) && (MSG == "set result=0"))
                        MSG = "set result=009-Problema campo vazio LANMAC";
                    if (nbmac.LANMAC.Length != 12 && (MSG == "set result=0"))
                        MSG = "set result=010-Problema campo diferente de 12 caracter LANMAC";
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(nbmac.WLANMAC) && (MSG == "set result=0"))
                        MSG = "set result=011-Problema campo vazio WLANMAC";
                    if (string.IsNullOrWhiteSpace(nbmac.BTMAC) && (MSG == "set result=0"))
                        MSG = "set result=012-Problema campo vazio BTMAC";
                    if (string.IsNullOrWhiteSpace(nbmac.LANMAC) && MAC_3 && (MSG == "set result=0"))
                        MSG = "set result=013-Problema campo vazio LANMAC";
                    if (!string.IsNullOrWhiteSpace(nbmac.LANMAC) && !MAC_3 && (MSG == "set result=0"))
                        MSG = $@"set result=014-Problema - Campo deve estar vazio para este modelo LANMAC {nbmac.LANMAC}";
                    if (nbmac.WLANMAC.Length != 12 && (MSG == "set result=0"))
                        MSG = $@"set result=015-Problema campo diferente de 12 caracter WLANMAC-{nbmac.WLANMAC}";
                    if (nbmac.BTMAC.Length != 12 && (MSG == "set result=0"))
                        MSG = $@"set result=016-Problema campo diferente de 12 caracter BTMAC-{nbmac.BTMAC}";
                    if (MAC_3)
                    {
                        if (nbmac.LANMAC.Length != 12 && (MSG == "set result=0"))
                            MSG = $@"set result=017-Problema campo diferente de 12 caracter LANMAC-{nbmac.LANMAC}";
                    }
                }
            }
            else
            {
                if (product_SKU.QtdMAC == 3)
                { MAC_3 = true; }
                else
                { MAC_3 = false; }

                SerialNumber = Columns[0];

                if (string.IsNullOrWhiteSpace(nbmac.WLANMAC) && (MSG == "set result=0"))
                    MSG = "set result=011-Problema campo vazio WLANMAC";
                if (string.IsNullOrWhiteSpace(nbmac.BTMAC) && (MSG == "set result=0"))
                    MSG = "set result=012-Problema campo vazio BTMAC";
                if (string.IsNullOrWhiteSpace(nbmac.LANMAC) && MAC_3 && (MSG == "set result=0"))
                    MSG = "set result=013-Problema campo vazio LANMAC";
                if (!string.IsNullOrWhiteSpace(nbmac.LANMAC) && !MAC_3 && (MSG == "set result=0"))
                    MSG = $@"set result=014-Problema - Campo deve estar vazio para este modelo LANMAC {nbmac.LANMAC}";
                if (nbmac.WLANMAC.Length != 12 && (MSG == "set result=0"))
                    MSG = $@"set result=015-Problema campo diferente de 12 caracter WLANMAC-{nbmac.WLANMAC}";
                if (nbmac.BTMAC.Length != 12 && (MSG == "set result=0"))
                    MSG = $@"set result=016-Problema campo diferente de 12 caracter BTMAC-{nbmac.BTMAC}";
                if (MAC_3)
                {
                    if (nbmac.LANMAC.Length != 12 && (MSG == "set result=0"))
                        MSG = $@"set result=017-Problema campo diferente de 12 caracter LANMAC-{nbmac.LANMAC}";
                }
            }

            if (MSG != "set result=0")
            {
                ConexaoDB.CRUD_tabela($@"insert into logruninnb (log,Model,MSG,SSN,controller) values ('{ssn}','{product.Product}','{MSG}','{Columns[0]}','{controller}')");
                return Ok(MSG);
            }
            //==========================================================================================================================================================

            string SQLnbmac = string.Empty;

            SQLnbmac = $@"SELECT * from NBMAC where ProductID='{product.idProduct}'";

            if (Columns[3].ToString() != "")
            {
                DataTable WLANMAC_DB = ConexaoDB.Carrega_Tabela($@"select ID_MAC, MAC, SSN, DATECREATE,ProductID from nbmac where MAC='{Columns[3]}'");
                if (WLANMAC_DB.Rows.Count >= 0)
                {
                    string MACSSN = Columns[0].ToString();
                    Int32 MACSSNID = product.idProduct;
                    foreach (DataRow linha in WLANMAC_DB.Rows)
                    {

                        if (Convert.ToInt32(linha[4]) == 100)
                        {
                            if (product.Customer == "ACER")
                                MACSSN = Columns[1].ToString();
                            if (linha[2].ToString() != MACSSN)
                            {
                                MSG = $@"set result=018-Problema Duplicado WLAMAC {Columns[3]} entrada SSN {MACSSN} gravado DB ID {linha[0]} SSN {linha[2]} DATACREATE {linha[3]}";
                                ConexaoDB.CRUDU_ID_tabela($@"insert into logruninnb (log,MSG,controller) values ('{ssn}','{MSG}','{controller}')");
                                return Ok(MSG);
                            }
                        }
                        else
                        {
                            if (Convert.ToInt32(linha[4]) != MACSSNID)
                            {
                                MSG = $@"set result=018-Problema Duplicado WLAMAC {Columns[3]} entrada SSN {MACSSN} gravado DB ID {linha[0]} SSN {linha[2]} DATACREATE {linha[3]}";
                                ConexaoDB.CRUDU_ID_tabela($@"insert into logruninnb (log,MSG,controller) values ('{ssn}','{MSG}','{controller}')");
                                return Ok(MSG);
                            }
                        }
                    }
                }
            }

           if (Columns[4].ToString() != "")
            {
                DataTable WLANMAC_DB = ConexaoDB.Carrega_Tabela($@"select ID_MAC, MAC, SSN, DATECREATE,ProductID from nbmac where MAC='{Columns[4]}'");
                if (WLANMAC_DB.Rows.Count >= 0)
                {
                    string MACSSN = Columns[0].ToString();
                    Int32 MACSSNID = product.idProduct;
                    foreach (DataRow linha in WLANMAC_DB.Rows)
                    {

                        if (Convert.ToInt32(linha[4]) == 100)
                        {
                            if (product.Customer == "ACER")
                                MACSSN = Columns[1].ToString();
                            if (linha[2].ToString() != MACSSN)
                            {
                                MSG = $@"set result=018-Problema Duplicado BTMAC {Columns[4]} entrada SSN {MACSSN} gravado DB ID {linha[0]} SSN {linha[2]} DATACREATE {linha[3]}";
                                ConexaoDB.CRUDU_ID_tabela($@"insert into logruninnb (log,MSG,controller) values ('{ssn}','{MSG}','{controller}')");
                                return Ok(MSG);
                            }
                        }
                        else
                        {
                            if (Convert.ToInt32(linha[4]) != MACSSNID)
                            {
                                MSG = $@"set result=018-Problema Duplicado BTMAC {Columns[4]} entrada SSN {MACSSN} gravado DB ID {linha[0]} SSN {linha[2]} DATACREATE {linha[3]}";
                                ConexaoDB.CRUDU_ID_tabela($@"insert into logruninnb (log,MSG,controller) values ('{ssn}','{MSG}','{controller}')");
                                return Ok(MSG);
                            }
                        }
                    }
                }
            }

            if (Columns[5].ToString() != "")
            {
                DataTable WLANMAC_DB = ConexaoDB.Carrega_Tabela($@"select ID_MAC, MAC, SSN, DATECREATE,ProductID from nbmac where MAC='{Columns[5]}'");
                if (WLANMAC_DB.Rows.Count >= 0)
                {
                    string MACSSN = Columns[0].ToString();
                    Int32 MACSSNID = product.idProduct;
                    foreach (DataRow linha in WLANMAC_DB.Rows)
                    {

                        if (Convert.ToInt32(linha[4]) == 100)
                        {
                            if (product.Customer == "ACER")
                                MACSSN = Columns[1].ToString();
                            if (linha[2].ToString() != MACSSN)
                            {
                                MSG = $@"set result=018-Problema Duplicado LAMAC {Columns[5]} entrada SSN {MACSSN} gravado DB ID {linha[0]} SSN {linha[2]} DATACREATE {linha[3]}";
                                ConexaoDB.CRUDU_ID_tabela($@"insert into logruninnb (log,MSG,controller) values ('{ssn}','{MSG}','{controller}')");
                                return Ok(MSG);
                            }
                        }
                        else
                        {
                            if (Convert.ToInt32(linha[4]) != MACSSNID)
                            {
                                MSG = $@"set result=018-Problema Duplicado LAMAC {Columns[5]} entrada SSN {MACSSN} gravado DB ID {linha[0]} SSN {linha[2]} DATACREATE {linha[3]}";
                                ConexaoDB.CRUDU_ID_tabela($@"insert into logruninnb (log,MSG,controller) values ('{ssn}','{MSG}','{controller}')");
                                return Ok(MSG);
                            }
                        }
                    }
                }
            }

            DataTable dtnbmac = ConexaoDB.Carrega_Tabela(SQLnbmac);
            
            DataView ver = new DataView(dtnbmac);
            ver.Sort = "ID_MAC ASC";

            int looping = 1;
            Int32 reg1 = 0;
            Int32 reg2 = 0;
            Int32 reg3 = 0;

            foreach(DataRowView row in ver)
            {
                if (looping == 1)
                {
                    reg1 = Convert.ToInt32(row["ID_MAC"]);
                    registroDB += row["MAC"].ToString() + " ";
                }
                if (looping == 2)
                {
                    reg2 = Convert.ToInt32(row["ID_MAC"]);
                    registroDB += row["MAC"].ToString() + " ";
                }
                if (looping == 3)
                {
                    reg3 = Convert.ToInt32(row["ID_MAC"]);
                    registroDB += row["MAC"].ToString() + " ";
                }
                looping++;
            }

            try
            {
                if (Columns.Length > 7)
                {
                    ConexaoDB.CRUD_tabela($@"update product set workOrder='{Columns[2]}', CustomerSerial='{Columns[1]}', UUID='{Columns[6]}', S_60='{Columns[7]}', S_MBSN='{Columns[8]}' where idProduct='{product.idProduct}'");
                }
                else
                {
                    ConexaoDB.CRUD_tabela($@"update product set workOrder='{Columns[2]}', CustomerSerial='{Columns[1]}', UUID='{Columns[6]}' where idProduct='{product.idProduct}'");
                }
            }
            catch (Exception x)
            {
                int pos = x.ToString().IndexOf("Duplicate");
                int pos1 = x.ToString().IndexOf("\r\n")-pos;
                string y = x.ToString().Substring(pos, pos1);
                MSG = $@"set result=021-Problema {y}";
                ConexaoDB.CRUDU_ID_tabela($@"insert into logruninnb (log,Model,MSG,SSN,controller) values ('{ssn}',{product.Product},'{MSG}',{product.Serial_Number},'{controller}')");
                return Ok(MSG);
            }
            string hoje =  DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            try
            {
                if (wb.CustomerCode == "ASUS")
                {
                    if (nbmac.WLANMAC.Length > 0 && nbmac.BTMAC.Length > 0 && nbmac.LANMAC.Length > 0)
                    {
                        if (dtnbmac.Rows.Count == 0)
                        {
                            MSG = $@"set result=022-Problema INSERT NBMAC id {product.idProduct} SSN {product.Serial_Number} WLAMAC {nbmac.WLANMAC} BTMAC {nbmac.BTMAC} LANMAC {nbmac.LANMAC} DB {registroDB}";
                            string gravaWLANMAC = $@"INSERT INTO nbmac (MAC,SSN,productID) VALUES ('{nbmac.WLANMAC}','{product.Serial_Number}',{product.idProduct})";
                            ConexaoDB.CRUD_tabela(gravaWLANMAC);
                            string gravaBTMAC = $@"INSERT INTO nbmac (MAC,SSN,productID) VALUES ('{nbmac.BTMAC}','{product.Serial_Number}',{product.idProduct})";
                            ConexaoDB.CRUD_tabela(gravaBTMAC);
                            string gravaLANMAC = $@"INSERT INTO nbmac (MAC,SSN,productID) VALUES ('{nbmac.LANMAC}','{product.Serial_Number}',{product.idProduct})";
                            ConexaoDB.CRUD_tabela(gravaLANMAC);
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
                                    { ConexaoDB.CRUD_tabela($@"update nbmac SET MAC ='{nbmac.WLANMAC}' ,DateAlter='{hoje}' where id_mac={reg1}"); }
                                    else if (i == 2)
                                    { ConexaoDB.CRUD_tabela($@"update nbmac SET MAC ='{nbmac.BTMAC}' ,DateAlter='{hoje}'  where id_mac={reg2}"); }
                                    else if (i == 3)
                                    { ConexaoDB.CRUD_tabela($@"update nbmac SET MAC ='{nbmac.LANMAC}' ,DateAlter='{hoje}'  where id_mac={reg3}"); }
                                }
                                catch (Exception)
                                {
                                    MSG = $@"set result=023-Problema UPDATE id {product.idProduct} SSN {product.Serial_Number} WLAMAC {nbmac.WLANMAC} BTMAC {nbmac.BTMAC} LANMAC {nbmac.LANMAC} DB {registroDB}";
                                    ConexaoDB.CRUDU_ID_tabela($@"insert into logruninnb (log,MSG,controller) values ('{ssn}','{MSG}','{controller}')");
                                    return Ok("set result=" + MSG);
                                }
                                i++;
                            }
                        }
                        else
                        {
                            MSG = $@"set result=024-Problema UPDATE id {product.idProduct} SSN {product.Serial_Number} WLAMAC {nbmac.WLANMAC} BTMAC {nbmac.BTMAC} LANMAC {nbmac.LANMAC}  DB {registroDB}";
                            ConexaoDB.CRUDU_ID_tabela($@"insert into logruninnb (log,MSG,controller) values ('{ssn}','{MSG}','{controller}')");
                            return Ok("set result=" + MSG);
                        }
                    }
                    else if (nbmac.WLANMAC.Length > 0 && nbmac.BTMAC.Length > 0)
                    {
                        if (dtnbmac.Rows.Count == 0)
                        {
                            MSG = $@"set result=025-Problema INSERT id {product.idProduct} SSN {product.Serial_Number} WLAMAC {nbmac.WLANMAC} BTMAC {nbmac.BTMAC} DB {registroDB}";
                            string gravaWLANMAC = $@"INSERT INTO nbmac (MAC,SSN,productID) VALUES ('{nbmac.WLANMAC}','{product.Serial_Number}',{product.idProduct})";
                            ConexaoDB.CRUD_tabela(gravaWLANMAC);
                            string gravaBTMAC = $@"INSERT INTO nbmac (MAC,SSN,productID) VALUES ('{nbmac.BTMAC}','{product.Serial_Number}',{product.idProduct})";
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
                                    { ConexaoDB.CRUD_tabela($@"update nbmac SET MAC ='{nbmac.WLANMAC}' ,DateAlter='{hoje}' where id_mac={reg1}"); }
                                    else
                                    { ConexaoDB.CRUD_tabela($@"update nbmac SET MAC ='{nbmac.BTMAC}' ,DateAlter='{hoje}' where id_mac={reg2}"); }
                                }
                                catch (Exception)
                                {
                                    MSG = $@"set result=026-Problema UPDATE id {product.idProduct} SSN {product.Serial_Number} WLAMAC {nbmac.WLANMAC} BTMAC {nbmac.BTMAC}  DB {registroDB}";
                                    ConexaoDB.CRUDU_ID_tabela($@"insert into logruninnb (log,MSG,controller) values ('{ssn}','{MSG}','{controller}')");
                                    return Ok("set result=" + MSG) ;
                                }
                                i++;
                            }
                        }
                        else
                        {
                            MSG = $@"set result=027-Problema UPDATE id {product.idProduct} SSN {product.Serial_Number} WLAMAC {nbmac.WLANMAC} BTMAC {nbmac.BTMAC} LANMAC {nbmac.LANMAC} DB {registroDB}";
                            ConexaoDB.CRUDU_ID_tabela($@"insert into logruninnb (log,MSG,controller) values ('{ssn}','{MSG}','{controller}')");
                            return Ok("set result=" + MSG);
                        }
                    }
                    else
                    {
                        MSG = $@"set result=028-Problema WLANMAC{nbmac.WLANMAC} ou BTMAC{nbmac.BTMAC} ou QTD de MAC do cliente {wb.CustomerCode} sem valores";
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
                            MSG = $@"set result=029-Problema INSERT MAC {nbmac.WLANMAC} {nbmac.BTMAC} {nbmac.LANMAC} ";
                            string gravaWLANMAC = $@"INSERT INTO nbmac (MAC,SSN,productID) VALUES ('{nbmac.WLANMAC}','{product.Serial_Number}',{product.idProduct})";
                            ConexaoDB.CRUD_tabela(gravaWLANMAC);
                            string gravaBTMAC = $@"INSERT INTO nbmac (MAC,SSN,productID) VALUES ('{nbmac.BTMAC}','{product.Serial_Number}',{product.idProduct})";
                            ConexaoDB.CRUD_tabela(gravaBTMAC);
                            string gravaLANMAC = $@"INSERT INTO nbmac (MAC,SSN,productID) VALUES ('{nbmac.LANMAC}','{product.Serial_Number}',{product.idProduct})";
                            ConexaoDB.CRUD_tabela(gravaLANMAC);
                        }
                        else if (dtnbmac.Rows.Count == 3)
                        {
                            int i = 1;
                            MSG = $@"set result=030-Problema UPDATE MAC {nbmac.WLANMAC} {nbmac.BTMAC} {nbmac.LANMAC} DB {registroDB}";
                            foreach (DataRow linha in dtnbmac.Rows)
                            {
                                if (i == 1)
                                { ConexaoDB.CRUD_tabela($@"update nbmac SET MAC ='{nbmac.WLANMAC}' ,DateAlter='{hoje}' where id_mac={reg1}"); }
                                else if (i == 2)
                                { ConexaoDB.CRUD_tabela($@"update nbmac SET MAC ='{nbmac.BTMAC}' ,DateAlter='{hoje}' where id_mac={reg2}"); }
                                else if (i == 3)
                                { ConexaoDB.CRUD_tabela($@"update nbmac SET MAC ='{nbmac.LANMAC}' ,DateAlter='{hoje}' where id_mac={reg3}"); }
                                i++;
                            }
                        }
                        else
                        {
                            MSG = $@"set result=031-Problema MAC {nbmac.WLANMAC} {nbmac.BTMAC} {nbmac.LANMAC} DB {registroDB}";
                            ConexaoDB.CRUDU_ID_tabela($@"insert into logruninnb (log,MSG,controller) values ('{ssn}','{MSG}','{controller}')");
                            return Ok(MSG);
                        }
                        MSG = "set result=0";
                    }
                    else if (nbmac.WLANMAC.Length > 0 && nbmac.BTMAC.Length > 0)
                    {
                        if (dtnbmac.Rows.Count == 0)
                        {
                            MSG = $@"set result=032-Problema INSERT MAC {nbmac.WLANMAC} {nbmac.BTMAC} ";
                            string gravaWLANMAC = $@"INSERT INTO nbmac (MAC,SSN,productID) VALUES ('{nbmac.WLANMAC}','{product.Serial_Number}',{product.idProduct})";
                            ConexaoDB.CRUD_tabela(gravaWLANMAC);
                            string gravaBTMAC = $@"INSERT INTO nbmac (MAC,SSN,productID) VALUES ('{nbmac.BTMAC}','{product.Serial_Number}',{product.idProduct})";
                            ConexaoDB.CRUD_tabela(gravaBTMAC);
                            
                        }
                        else if (dtnbmac.Rows.Count == 2)
                        {
                            MSG = $@"set result=033-Problema UPDATE MAC {nbmac.WLANMAC} {nbmac.BTMAC}  DB {registroDB}";
                            int i = 1;
                            foreach (DataRow linha in dtnbmac.Rows)
                            {
                                    if (i == 1)
                                    { ConexaoDB.CRUD_tabela($@"update nbmac SET MAC ='{nbmac.WLANMAC}' ,DateAlter='{hoje}' where id_mac={reg1}"); }
                                    else if (i == 2)
                                    { ConexaoDB.CRUD_tabela($@"update nbmac SET MAC ='{nbmac.BTMAC}' ,DateAlter='{hoje}' where id_mac={reg2}"); }  
                                i++;
                            }
                        }
                        else
                        {
                            MSG = $@"set result=034-Problema MAC {nbmac.WLANMAC} {nbmac.BTMAC} DB {registroDB}";
                            ConexaoDB.CRUDU_ID_tabela($@"insert into logruninnb (log,MSG,controller) values ('{ssn}','{MSG}','{controller}')");
                            return Ok(MSG);
                        }
                        MSG = "set result=0";
                    }
                    else if (nbmac.LANMAC.Length > 0)
                    {
                        if (dtnbmac.Rows.Count == 0)
                        {
                            MSG = $@"set result=035-Problema INSERT MAC DESKTOP {nbmac.LANMAC} DB {registroDB}";
                            ConexaoDB.CRUD_tabela($@"INSERT INTO nbmac (MAC,SSN,productID) VALUES ('{nbmac.LANMAC}','{product.CustomerSerial}',{product.idProduct})");
                        }
                        else if (dtnbmac.Rows.Count == 1)
                        {
                            MSG = $@"set result=036-Problema UPDATE MAC DESKTO {nbmac.LANMAC}";
                            foreach (DataRow linha in dtnbmac.Rows)
                            {
                                ConexaoDB.CRUD_tabela($@"update nbmac SET MAC ='{nbmac.LANMAC}' ,DateAlter='{hoje}' where id_mac=" + linha[0]);
                            }
                        }
                        else
                        {
                            MSG = $@"set result=037-Problema INSERT MAC DESKTOP {nbmac.LANMAC} DB {registroDB}";
                            ConexaoDB.CRUDU_ID_tabela($@"insert into logruninnb (log,MSG,controller) values ('{ssn}','{MSG}','{controller}')");
                            return Ok(MSG);
                        }
                        MSG = "set result=0";
                    }
                    else
                    {
                        MSG = $@"set result=038-Problema MAC DESKTOP {nbmac.LANMAC} DB {registroDB}";
                        ConexaoDB.CRUDU_ID_tabela($@"insert into logruninnb (log,MSG,controller) values ('{ssn}','{MSG}','{controller}')");
                        return Ok(MSG) ;
                    }
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
