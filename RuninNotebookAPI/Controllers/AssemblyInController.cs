using RuninNotebookAPI.ServiceReference1;
using RuninNotebookAPI.Models;
using RuninNotebookAPI.DB;
using System.ServiceModel.Channels;
using System.Web.Http;
using System.ServiceModel;
using System;
using System.Reflection;

namespace RuninNotebookAPI.Controllers
{
    public class AssemblyInController : ApiController
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

            WebSFIS_GET wb = new WebSFIS_GET();
           
            Produto_Movimento product_movement = new Produto_Movimento();

            string[] asse = Assembly.GetExecutingAssembly().FullName.ToString().Split(',');
            controller = "ASSEMBLY1 - " + asse[1];

            string[] Columns = ssn.Split(',');

            Columns[0] = Columns[0].ToString().ToUpper();

            if (Columns[0].Length == 15 || Columns[0].Length == 12 || Columns[0].Length == 22)
            {
                if (Columns[0].Substring(4, 2) == "B6")
                {
                    wb.CustomerCode = "ASUS";
                }
                else if (Columns[0].Substring(9, 3) == "935" || Columns[0].Length == 22)
                {
                    wb.CustomerCode = "ACER";
                }
                else if (Columns[0].Substring(10, 2) == "TL")
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

           
            Produto product = new Produto(Columns[0]);

            HttpRequestMessageProperty customerHeader = new HttpRequestMessageProperty();
            WebServiceTestSoapClient client = new WebServiceTestSoapClient("WebServiceTestSoap");
            customerHeader.Headers.Add("X-Type", "L10");
            customerHeader.Headers.Add("X-Customer", wb.CustomerCode);
            using (new OperationContextScope(client.InnerChannel))
            {
                OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = customerHeader;
                var SFIS_CHECK_STATUS = client.SFIS_GET_DATA(Columns[0]);

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


                    ID = product.ID;
                    
                    SKUID = ConexaoDB.CRUDValor_tabela($@"select id from product_sku where sku='{wb.SKU}'");

                    string sqlP = "INSERT INTO product(Serial_Number, SKU, Color_ID, Product, Customer, Status_Code, WorkOrder) VALUES (";
                           sqlP += $@"'{Columns[0]}','{wb.SKU}','{wb.ColorCode}','{wb.ModelName}','{wb.CustomerCode}','0','{wb.WorkOrder}')";

                    string sqlPM = $@"INSERT INTO product_movement (idProduct,WorkGroup,Position,Start_Test,Status_Code,Next_Station,ProductLine) values ({ID},'ASSEMBLY1','1567','{product_movement.Start_Test}','0','0','{Columns[1]}')";

                    try
                    {
                        if (ID == 0)
                        {
                            ID = ConexaoDB.CRUDU_ID_tabela(sqlP);        
                            sqlPM = $@"INSERT INTO product_movement (idProduct,WorkGroup,Position,Start_Test,Status_Code,Next_Station,ProductLine) values ({ID},'ASSEMBLY1','1567','{product_movement.Start_Test}','0','0','{Columns[1]}')";
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
                            ConexaoDB.CRUD_tabela(sqlPM);
                        }
                    }
                    catch (Exception)
                    { 
                        MSG = "set result=Problema ao criar DB LogRuninNb";
                        ConexaoDB.CRUDU_ID_tabela($@"insert into logruninnb (log,Model,MSG,SSN,controller) values ('{ssn}','{wb.ModelName}','{MSG}','{Columns[0]}','{controller}')");
                        return Ok(MSG) ;
                    }
                }
                else
                {
                    MSG = "set result=Error WebService" + SFIS_CHECK_STATUS.ErrorMessage;
                    ConexaoDB.CRUDU_ID_tabela($@"insert into logruninnb (log,Model,MSG,SSN,controller) values ('{ssn}','{wb.ModelName}','{MSG}','{Columns[0]}','{controller}')");
                    return Ok(MSG) ;
                }
            }
            MSG = "set result=0";
            return Ok(MSG); 
        }
    }
}
