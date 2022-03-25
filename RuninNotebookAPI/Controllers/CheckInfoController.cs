using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using RuninNotebookAPI.Models;
using RuninNotebookAPI.DB;
using System.Data;

namespace RuninNotebookAPI.Controllers
{
    public class CheckInfoController : ApiController
    {
        public object MSG { get; private set; }

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
            NBMAC nbmac = new NBMAC();

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
                    return Json(MSG);
                }
            }
            else
            {
                MSG = "set result=Serial number length is invalid";
                return Json(MSG);
            }

            product.Serial_Number = Columns[0];
            nbmac.WLANMAC = Columns[1];
            nbmac.BTMAC = Columns[2];
            nbmac.LANMAC = Columns[3];
            nbmac.UUID = Columns[4];

            string sqlmac = "";

            sqlmac = $@"select id_mac,mac,uuid,ssn,datecreate from nbmac where ssn='{product.Serial_Number}' order by ID_MAC";

            if (product.Customer=="ACER")
            {
                if (Columns[0].ToString().Length < 22)
                {
                    product.CustomerSerial = ConexaoDB.CRUDCampo_tabela($@"SELECT CustomerSerial FROM backupengteste.product where Serial_number='{product.Serial_Number}'");
                    sqlmac = $@"select id_mac,mac,uuid,ssn,datecreate from nbmac where ssn='{product.CustomerSerial}' order by ID_MAC";
                }
                else
                {
                    MSG = "Serial deve ser do Produto!!!!";
                    return Json(MSG);
                }
            }
            
            DataTable QueryResult = ConexaoDB.Carrega_Tabela(sqlmac);
            int l = 1;

            try
            {
                int i = QueryResult.Rows.Count;
                foreach(DataRow linha in QueryResult.Rows)
                {
                    if(i==1)
                    {
                        if ((string.IsNullOrEmpty(nbmac.WLANMAC) && (string.IsNullOrEmpty(nbmac.BTMAC))))
                        {
                            if (linha[1].ToString() != nbmac.LANMAC || linha[2].ToString() != nbmac.UUID)
                            {
                                MSG = $@"Diferente LANMAC->{nbmac.LANMAC} ou UUID ->{nbmac.UUID}";
                                return Json(MSG);
                            }
                        }
                        else
                        {
                            MSG = $@"WLAN->{nbmac.WLANMAC} OU BTMAC->{nbmac.BTMAC} não estão vazios!!! {product.Customer} MODELO DESKTOP ";
                            return Json(MSG);
                        }
                        
                    }
                    else if (i==2)
                    {
                        if ((l==1) && (linha[1].ToString()!=nbmac.WLANMAC) || (l == 1) && linha[2].ToString()!= nbmac.UUID)
                        {
                            MSG = $@"Diferente WLANMAC DB->{nbmac.WLANMAC} / WLANMAC->{linha[2].ToString()}  ou UUID DB->{nbmac.UUID} / UUID->{linha[2].ToString()}";
                            return Json(MSG);
                        }
                        else if ((l == 2) && (linha[1].ToString() != nbmac.BTMAC) || (l == 2) && linha[2].ToString() != nbmac.UUID)
                        {
                            MSG = $@"Diferente BTMAC DB->{nbmac.BTMAC} / BTMAC->{linha[1].ToString()}  ou UUID DB->{nbmac.UUID} / UUID->{linha[2].ToString()}";
                            return Json(MSG);
                        }
                    }
                    else if(i==3)
                    {
                        if ((l == 1) && (linha[1].ToString() != nbmac.WLANMAC) || (l == 1) && linha[2].ToString() != nbmac.UUID)
                        {
                            MSG = $@"Diferente WLANMAC DB->{nbmac.WLANMAC} / WLANMAC->{linha[1].ToString()}  ou UUID DB->{nbmac.UUID} / UUID->{linha[2].ToString()}";
                            return Json(MSG);
                        }
                        else if ((l == 2) && (linha[1].ToString() != nbmac.BTMAC) || (l == 2) && linha[2].ToString() != nbmac.UUID)
                        {
                            MSG = $@"Diferente BTMAC DB->{nbmac.BTMAC} / BTMAC->{linha[1].ToString()}  ou UUID DB->{nbmac.UUID} / UUID->{linha[2].ToString()}";
                            return Json(MSG);
                        }
                        else if ((l == 3) && (linha[1].ToString() != nbmac.LANMAC) || (l == 3) && linha[2].ToString() != nbmac.UUID)
                        {
                            MSG = $@"Diferente LANMAC DB->{nbmac.LANMAC} / LANMAC->{linha[1].ToString()}  ou UUID DB->{nbmac.UUID} / UUID->{linha[2].ToString()}";
                            return Json(MSG);
                        }
                    }
                    l++;
                }


               
            }
            catch (Exception)
            {
                return Json(MSG);
            }
            MSG = "set result=0";
            return Json(MSG);
        }

    }
}
