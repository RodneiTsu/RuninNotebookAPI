using RuninNotebookAPI.ServiceReference1;
using RuninNotebookAPI.Models;
using RuninNotebookAPI.DB;
using System.ServiceModel.Channels;
using System.Web.Http;
using System.ServiceModel;
using System;

namespace RuninNotebookAPI.Controllers
{
    public class DownPreInController : ApiController
    {
        public string MSG { get; set; }
        public int ID { get; set; }
        public int IDPM { get; set; }
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
            controller = "DownPreIn";

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
                    MSG = "set result=Validacao do serial esta incorreto com Customer";
                    ConexaoDB.CRUDU_ID_tabela($@"insert into logruninnb (log,MSG,controller) values ('{ssn}','{MSG}','{controller}')");
                    return Ok( MSG) ;
                }
            }
            else
            {
                MSG = "set result=Serializacao nao esta correto provalvel tamanho";
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

                    product_movement.Start_Test = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    Produto product = new Produto(ssn);

                    ID = product.ID;

                    SKUID = ConexaoDB.CRUDValor_tabela($@"select id from product_sku where sku='{wb.SKU}'");

                    string sqlP  = "INSERT INTO product(Serial_Number, SKU, Color_ID, Product, Customer, Status_Code, WorkOrder) VALUES (";
                           sqlP += $@"'{ssn}','{wb.SKU}','{wb.ColorCode}','{wb.ModelName}','{wb.CustomerCode}','0','{wb.WorkOrder}')";

                    try
                    {
                        if (ID == 0)
                        {
                            MSG = "Ok Gravacao de Product";
                            ID = ConexaoDB.CRUDU_ID_tabela(sqlP);

                            string sqlPM = $@"INSERT INTO product_movement (idProduct,WorkGroup,Position,Start_Test,Status_Code,Next_Station) values ({ID},'DOWNLOADPRE','1565','{product_movement.Start_Test}','0','0')";
                            ConexaoDB.CRUD_tabela(sqlPM);

                            ConexaoDB.CRUDU_ID_tabela($@"insert into logruninnb (log,Model,SSN,MSG,controller) values ('{ssn}','{wb.ModelName}','{ssn}','{MSG}','{controller}')");

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
                            string slqPM = $@"select if(max(idProduct_Movement)>0,max(idProduct_Movement),0) ";
                                  slqPM += $@"from engteste.product p inner ";
                                  slqPM += $@"join engteste.product_movement pm on p.idProduct = pm.idProduct ";
                                  slqPM += $@"where p.idProduct = {ID} and pm.WorkGroup = 'DOWNLOADPRE'";

                            IDPM = ConexaoDB.CRUDValor_tabela(slqPM);

                            if (IDPM == 0)
                            {
                                string sqlPM = $@"INSERT INTO product_movement (idProduct,WorkGroup,Position,Start_Test,Status_Code,Next_Station) values ({ID},'DOWNLOADPRE','1565','{product_movement.Start_Test}','0','0')";
                                ConexaoDB.CRUD_tabela(sqlPM);
                            }
                        }
                    }
                    catch (Exception)
                    { 
                        MSG = "set result=Problema ao gravar DB LogRuninNb";
                        ConexaoDB.CRUDU_ID_tabela($@"insert into logruninnb (log,Model,SSN,MSG,controller) values ('{ssn}','{wb.ModelName}','{ssn}','{MSG}','{controller}')");
                        return Ok(MSG) ;
                    }
                }
                else
                {
                    MSG = "set result=Erro WebService" + SFIS_CHECK_STATUS.ErrorMessage;
                    ConexaoDB.CRUDU_ID_tabela($@"insert into logruninnb (log,Model,SSN,MSG,controller) values ('{ssn}','{wb.ModelName}','{ssn}','{MSG}','{controller}')");
                    return Ok(MSG) ;
                }
            }
            MSG = "set result=0";
            return Ok(MSG); 
        }
    }
}