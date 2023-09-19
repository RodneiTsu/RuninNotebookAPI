 using System;
using System.Web.Http;
using RuninNotebookAPI.Models;
using RuninNotebookAPI.DB;
using System.Data;
using System.Reflection;

namespace RuninNotebookAPI.Controllers
{
    public class CheckInfoController : ApiController
    {
        public string MSG { get; set; }
        public string controller { get; set; }

        public bool MAC_3 { get; set; }

        public string SerialNumber { get; set; }

        [HttpGet]
        public IHttpActionResult Out_GET(string ssn)
        {
            if (ssn is null)
            {
                NotFound();
            }

            string[] asse = Assembly.GetExecutingAssembly().FullName.ToString().Split(',');
            controller = "CheckInfo - " + asse[1];
            
            MSG = "set result=0";
            string[] Columns = ssn.Split(',');
            MAC_3 = true;
            Produto product = new Produto();
            Produto_Movimento product_movement = new Produto_Movimento();
            NBMAC nbmac = new NBMAC();

            string SqlProduct = $@"SELECT * FROM product where ";

            
            if (Columns[0].Length == 15 || Columns[0].Length == 12 || Columns[0].Length == 22)
            {
                if (Columns[0].ToUpper().ToString().ToUpper().Substring(4, 2) == "B6")
                {
                    product.Customer = "ASUS";
                    SqlProduct += $@"Serial_Number='{Columns[0].ToString().ToUpper()}'";
                }
                else if (Columns[0].ToUpper().ToString().ToUpper().Substring(9, 3) == "935" || Columns[0].Length == 22)
                {
                    product.Customer = "ACER";
                    if (Columns[0].Length == 22)
                    { SqlProduct += $@"CustomerSerial='{Columns[0].ToString().ToUpper()}'"; }
                    else { SqlProduct += $@"Serial_Number='{Columns[0].ToString().ToUpper()}'"; }
                }
                else if (Columns[0].ToUpper().ToString().ToUpper().Substring(10, 2) == "TL")
                {
                    product.Customer = "HUAWEI";
                }
                else
                {
                    MSG = $@"set result=050-Problema Serial Number nao validado pelo customer - {Columns[0]}";
                    ConexaoDB.CRUD_tabela($@"insert into logruninnb (log,MSG,controller) values ('{ssn}','{MSG}','{controller}')");
                    return Ok(MSG);
                }
            }
            else
            {
                MSG = $@"set result=051-Problema tamanho Serial number invalido - {Columns[0]}";
                ConexaoDB.CRUD_tabela($@"insert into logruninnb (log,MSG,controller) values ('{ssn}','{MSG}','{controller}')");
                return Ok(MSG);
            }

            if(Columns.Length!=5)
            {
                MSG = "set result=052-Problema Falta informacao ou  informacao maior - deve ter SSN,WLANMAC,BTMAC,LANMAC,UUID";
                ConexaoDB.CRUD_tabela($@"insert into logruninnb (log,MSG,controller) values ('{ssn}','{MSG}','{controller}')");
                return Ok(MSG);
            }

            DataTable dtProduct = ConexaoDB.Carrega_Tabela(SqlProduct);

            if (dtProduct.Rows.Count <= 0)
            {
                MSG = "set result=053-Problema nao exista registro deste produto - PRODUCT";
                ConexaoDB.CRUDU_ID_tabela($@"insert into logruninnb (log,MSG,controller) values ('{ssn}','{MSG}','{controller}')");
                return Ok(MSG);
            }

            foreach (DataRow lin in dtProduct.Rows)
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

            nbmac.WLANMAC = Columns[1];
            nbmac.BTMAC = Columns[2];
            nbmac.LANMAC = Columns[3];
            nbmac.UUID = Columns[4];
            //============================================================================================================================



            if (product.Customer == "ACER")
            {
                if (product.Product == "SF314-511")
                    MAC_3 = false;

                if (string.IsNullOrWhiteSpace(product.CustomerSerial))
                {
                    MSG = "set result=057-Problema nao existe Customer Serial gravado..";
                }
                else
                {
                    SerialNumber = product.CustomerSerial;
                }
                if (product.CustomerSerial.Substring(0, 1) == "D")
                {
                    if (string.IsNullOrWhiteSpace(nbmac.LANMAC))
                        MSG = "set result=058-Problema campo vazio LANMAC";

                    if (nbmac.LANMAC.Length != 12)
                        MSG = "set result=059-Problema campo diferente de 12 caracter LANMAC";

                    if (!string.IsNullOrWhiteSpace(nbmac.LANMAC) && !MAC_3 && (MSG == "set result=0"))
                        MSG = $@"set result=063-Problema - Campo deve estar vazio para este modelo LANMAC {nbmac.LANMAC}";
                }
                else
                {
                    if (nbmac.WLANMAC == nbmac.BTMAC)
                        MSG = $@"set result=054-Problema WLANMAC {nbmac.WLANMAC} igual a BTMAC {nbmac.BTMAC}";
                    if (nbmac.WLANMAC == nbmac.LANMAC && (MSG == "set result=0"))
                        MSG = $@"set result=055-Problema WLANMAC {nbmac.WLANMAC} igual a LANMAC {nbmac.LANMAC}";
                    if (nbmac.LANMAC == nbmac.BTMAC && (MSG == "set result=0"))
                        MSG = $@"set result=056-Problema LANMAC {nbmac.LANMAC} igual a BTMAC {nbmac.BTMAC}";

                    if (string.IsNullOrWhiteSpace(nbmac.WLANMAC) && (MSG == "set result=0"))
                        MSG = "set result=060-Problema campo vazio WLANMAC";
                    if (string.IsNullOrWhiteSpace(nbmac.BTMAC) && (MSG == "set result=0"))
                        MSG = "set result=061-Problema campo vazio BTMAC";
                    if (string.IsNullOrWhiteSpace(nbmac.LANMAC) && MAC_3 && (MSG == "set result=0"))
                        MSG = "set result=062-Problema campo vazio LANMAC";
                    if (!string.IsNullOrWhiteSpace(nbmac.LANMAC) && !MAC_3 && (MSG == "set result=0"))
                        MSG = $@"set result=063-Problema - Campo deve estar vazio para este modelo LANMAC {nbmac.LANMAC}";


                    if (nbmac.WLANMAC.Length != 12 && (MSG == "set result=0"))
                        MSG = $@"set result=064-Problema campo diferente de 12 caracter WLANMAC-{nbmac.WLANMAC}";
                    if (nbmac.BTMAC.Length != 12 && (MSG == "set result=0"))
                        MSG = $@"set result=065-Problema campo diferente de 12 caracter BTMAC-{nbmac.BTMAC}";
                    if (MAC_3)
                    {
                        if (nbmac.LANMAC.Length != 12 && (MSG == "set result=0"))
                            MSG = $@"set result=066-Problema campo diferente de 12 caracter LANMAC-{nbmac.LANMAC}";
                    }
                }
            }
            else
            {
                if (product.Product == "K6502ZC" || product.Product == "K6502HC" || product.Product == "G614JV" || product.Product == "FX507ZC4" || product.Product == "A315-59" || product.Product == "AN515-57" || product.Product == "A515-57")
                { MAC_3 = true; }
                else
                { MAC_3 = false; }

                SerialNumber = product.Serial_Number;

                if (nbmac.WLANMAC == nbmac.BTMAC)
                    MSG = $@"set result=054-Problema WLANMAC {nbmac.WLANMAC} igual a BTMAC {nbmac.BTMAC}";
                if (nbmac.WLANMAC == nbmac.LANMAC && (MSG == "set result=0"))
                    MSG = $@"set result=055-Problema WLANMAC {nbmac.WLANMAC} igual a LANMAC {nbmac.LANMAC}";
                if (nbmac.LANMAC == nbmac.BTMAC && (MSG == "set result=0"))
                    MSG = $@"set result=056-Problema LANMAC {nbmac.LANMAC} igual a BTMAC {nbmac.BTMAC}";

                if (string.IsNullOrWhiteSpace(nbmac.WLANMAC) && (MSG == "set result=0"))
                    MSG = "set result=060-Problema campo vazio WLANMAC";
                if (string.IsNullOrWhiteSpace(nbmac.BTMAC) && (MSG == "set result=0"))
                    MSG = "set result=061-Problema campo vazio BTMAC";
                if (string.IsNullOrWhiteSpace(nbmac.LANMAC) && MAC_3 && (MSG == "set result=0"))
                    MSG = "set result=062-Problema campo vazio LANMAC";
                if (!string.IsNullOrWhiteSpace(nbmac.LANMAC) && !MAC_3 && (MSG == "set result=0"))
                    MSG = $@"set result=063-Problema - Campo deve estar vazio para este modelo LANMAC {nbmac.LANMAC}";

                if (nbmac.WLANMAC.Length != 12 && (MSG == "set result=0"))
                    MSG = $@"set result=064-Problema campo diferente de 12 caracter WLANMAC-{nbmac.WLANMAC}";
                if (nbmac.BTMAC.Length != 12 && (MSG == "set result=0"))
                    MSG = $@"set result=065-Problema campo diferente de 12 caracter BTMAC-{nbmac.BTMAC}";
                if (MAC_3)
                {
                    if (nbmac.LANMAC.Length != 12 && (MSG == "set result=0"))
                        MSG = $@"set result=066-Problema campo diferente de 12 caracter LANMAC-{nbmac.LANMAC}";
                }
            }



            if (Columns[4] != product.UUID)
            {
                MSG = $@"set result=067-Problema UUID diferente do registrado DB-{product.UUID} entrada-{Columns[4]}";
            }

            if (MSG != "set result=0")
            {
                ConexaoDB.CRUD_tabela($@"insert into logruninnb (log,Model,MSG,SSN,controller) values ('{ssn}','{product.Product}','{MSG}','{Columns[0]}','{controller}')");
                return Ok(MSG);
            }
            //==========================================================================================================================================================

            DataTable QueryResult = new DataTable();
          
            QueryResult = ConexaoDB.Carrega_Tabela($@"select * from nbmac where ProductID='{product.idProduct}'");

            DataView ver = new DataView(QueryResult);
            ver.Sort = "ID_MAC ASC";

            int l = 1;

            if (QueryResult.Rows.Count == 0)
            {
                MSG = "set result=068-Problema Serial nao existe do NBMAC... ";
                ConexaoDB.CRUD_tabela($@"insert into logruninnb (log,Model,MSG,SSN,controller) values ('{ssn}','{product.Product}','{MSG}','{Columns[0]}','{controller}')");
                return Ok(MSG);
            }

            try
            {
                int i = QueryResult.Rows.Count;
                foreach (DataRowView linha in ver)
                {
                    if (i == 1)
                    {
                        if (linha["MAC"].ToString() != nbmac.LANMAC)
                        {
                            MSG = $@"set result=069-Problema Diferente LANMAC {nbmac.LANMAC}";
                        }
                    }
                    else if (i == 2)
                    {
                        if ((l == 1) && (linha["MAC"].ToString() != nbmac.WLANMAC))
                        {
                            MSG = $@"set result=071-Problema Diferente WLANMAC DB {nbmac.WLANMAC} - WLANMAC-{linha[1].ToString()} ";
                        }
                        else if ((l == 2) && (linha["MAC"].ToString() != nbmac.BTMAC))
                        {
                            MSG = $@"set result=072-Problema Diferente BTMAC DB {nbmac.BTMAC} - BTMAC-{linha[1].ToString()}";
                        }
                    }
                    else if (i == 3)
                    {
                        if ((l == 1) && (linha["MAC"].ToString() != nbmac.WLANMAC))
                        {
                            MSG = $@"set result=073-Problema Diferente WLANMAC DB {nbmac.WLANMAC} - WLANMAC-{linha[1].ToString()}";
                        }
                        else if ((l == 2) && (linha["MAC"].ToString() != nbmac.BTMAC))
                        {
                            MSG = $@"set result=074-Problema Diferente BTMAC DB {nbmac.BTMAC} - BTMAC-{linha[1].ToString()}";
                        }
                        else if ((l == 3) && (linha["MAC"].ToString() != nbmac.LANMAC))
                        {
                            MSG = $@"set result=075-Problema Diferente LANMAC DB {nbmac.LANMAC} - LANMAC-{linha[1].ToString()}";
                        }
                    }
                    l++;
                }
                if (MSG != "set result=0")
                {
                    ConexaoDB.CRUD_tabela($@"insert into logruninnb (log,Model,MSG,SSN,controller) values ('{ssn}','{product.Product}','{MSG}','{Columns[0]}','{controller}')");
                    return Ok(MSG);
                }
            }
            catch (Exception)
            {
                ConexaoDB.CRUD_tabela($@"insert into logruninnb (log,Model,MSG,SSN,controller) values ('{ssn}','{product.Product}','{MSG}','{Columns[0]}','{controller}')");
                return Ok(MSG);
            }
            MSG = "set result=0";
            return Ok( MSG) ;
        }

    }
}
