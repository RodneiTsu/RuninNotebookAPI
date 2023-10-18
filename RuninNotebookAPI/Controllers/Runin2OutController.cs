using System;
using System.Web.Http;
using RuninNotebookAPI.Models;
using RuninNotebookAPI.DB;
using System.Data;
using System.Reflection;

namespace RuninNotebookAPI.Controllers
{
    public class Runin2OutController : ApiController
    {
        public string MSG { get; set; }
        public int ID { get; set; }
        public int PMID { get; set; }
        public string controller { get; set; }


        [HttpGet]
        public IHttpActionResult Out_GET(string ssn)
        {
            if (ssn is null)
            {
                NotFound();
            }

            string[] asse = Assembly.GetExecutingAssembly().FullName.ToString().Split(',');
            controller = "Runin2Out - " + asse[1];
            string[] Columns = ssn.Split(',');

            
            Produto_Movimento product_movement = new Produto_Movimento();

            DataTable productDT;

            string sqlProduct = $@"SELECT idProduct, idProduct_SKU, Serial_Number, CustomerSerial, WorkOrder, UUID, SKU, Color_ID, Product, Model, Customer, Status_Code, Dt_Creat, WorkStation, ";

            if (Columns[0].Length == 15 || Columns[0].Length == 12 || Columns[0].Length == 22)
            { 
                if (Columns[0].ToUpper().ToString().ToUpper().Substring(4, 2) == "B6")
                {
                    sqlProduct += $@"Download, Dt_GetIn, Dt_GetOut, idSwitch, S_60, S_MBSN FROM product WHERE Serial_Number = '{Columns[0].ToUpper()}'";
                }
                else if (Columns[0].ToUpper().ToString().ToUpper().Substring(9, 3) == "935")
                {
                    sqlProduct += $@"Download, Dt_GetIn, Dt_GetOut, idSwitch, S_60, S_MBSN FROM product WHERE Serial_Number = '{Columns[0].ToUpper()}'";
                }
                else if (Columns[0].Length == 22)
                {
                    sqlProduct += $@"Download, Dt_GetIn, Dt_GetOut, idSwitch, S_60, S_MBSN FROM product WHERE CustomerSerial = '{Columns[0].ToUpper()}'";
                }
                else if (Columns[0].ToUpper().ToString().ToUpper().Substring(10, 2) == "TL")
                { 
                    sqlProduct += $@"Download, Dt_GetIn, Dt_GetOut, idSwitch, S_60, S_MBSN FROM product WHERE Serial_Number = '{Columns[0].ToUpper()}'";
                }
                else
                {
                    MSG = "set result=Validacao de Serial Number incorreto ou tamanho invalido";
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

            Produto product = new Produto();

            productDT = ConexaoDB.Carrega_Tabela(sqlProduct);

            if (productDT.Rows.Count > 0)
            {

                foreach (DataRow lin in productDT.Rows)
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
            }
            else
            {
                MSG = "set result=Serial Number nao encontrado no DB da tabela product";
                ConexaoDB.CRUDU_ID_tabela($@"insert into logruninnb (log,MSG,Model) values ('{ssn}','{MSG}','{product.Product}')");
                return Ok(MSG);
            }

            //product.Dt_GetOut = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            product_movement.End_Test = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            product_movement.Operator_ID = Columns[1];
            product_movement.Error_Code = Columns[2];
            product_movement.Fail_Description = Columns[3];
            product_movement.Status_Code = "1";

            string sqlPM = $@"select if(max(idProduct_Movement)>0,max(idProduct_Movement),0) from product_movement where idProduct = {product.idProduct} AND WorkGroup = 'RUNIN2'";


            PMID = ConexaoDB.CRUDValor_tabela(sqlPM);
            if (PMID == 0)
            {
                MSG = $@"set result=nao foi gravado RUNIN2 IN SN:{product.Serial_Number}";
                ConexaoDB.CRUDU_ID_tabela($@"insert into logruninnb (log,MSG,Model,SSN,controller) values ('{ssn}','{MSG}','{product.Product}','{product.Serial_Number}','{controller}')");
                return Ok(MSG);
            }

            string produmovZERO = ConexaoDB.CRUDCampo_tabela($@"select Status_Code from product_movement where idProduct_Movement={PMID}");

            if (produmovZERO == "0")
            {
                try
                {
                    string SQL = $@"UPDATE product_movement set Operator_ID='OPERATOR-TEST', Error_Code='{product_movement.Error_Code}', Fail_Description='{product_movement.Fail_Description}', End_Test='{product_movement.End_Test}',Status_Code='1' ";
                    SQL += $@"WHERE idProduct_movement = {PMID}";

                    MSG = "set result=UPDATE Error writing to database product_movement";
                    ConexaoDB.CRUD_tabela(SQL);

                    //MSG = "set result=UPDATE Erro UpDate DB tabela product";
                    //ConexaoDB.CRUD_tabela($@"update product set Dt_GetOut='{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}' ,Status_Code='3' where idProduct={product.idProduct}");

                    //MSG = "set result=UPDATE Error writing to database switch_ip_route";
                    //ConexaoDB.CRUD_tabela($@"update switch_ip_route set downloadin=downloadin - 1 where idSwitch_IP_Route = {product.idSwitch}");

                    //MSG = "Problema INSERT LogRuninNB";
                    //ConexaoDB.CRUD_tabela($@"insert into logruninnb (log,MSG,Model,SSN,controller) values ('{ssn}','Status=9 009 DownloadIn-1 idSwitch={product.idSwitch}','{product.Product}','{product.Serial_Number}','{controller}')");

                    MSG = "set result=0";
                }
                catch (Exception)
                {
                    ConexaoDB.CRUDU_ID_tabela($@"insert into logruninnb (log,MSG,Model,SSN,controller) values ('{ssn}','{MSG}','{product.Product}','{product.Serial_Number}','{controller}')");
                    return Ok(MSG);
                }
            }
            MSG = "set result=0";
            return Ok(MSG) ;
        }
    }
}