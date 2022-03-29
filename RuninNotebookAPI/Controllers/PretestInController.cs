
using RuninNotebookAPI.ServiceReference1;
using RuninNotebookAPI.Models;
using RuninNotebookAPI.DB;
using System.ServiceModel.Channels;
using System.Web.Http;
using System.ServiceModel;
using System;

namespace RuninNotebookAPI.Controllers
{
    public class PretestInController : ApiController
    {
        public object MSG { get; private set; }

        public int ID { get; set; }

        [HttpGet]
        public IHttpActionResult SFIS_GET(string ssn)
        {
            if (ssn is null)
            {
                NotFound();
            }

            WebSFIS_GET wb = new WebSFIS_GET();
            Produto product = new Produto();
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
                    MSG = "set result=It was not possible to define a valid customer";
                    return Json(MSG);
                }
            }
            else
            {
                MSG = "set result=Serial number length is invalid";
                return Json(MSG);
            }


            HttpRequestMessageProperty customerHeader = new HttpRequestMessageProperty();
            WebServiceTestSoapClient client = new WebServiceTestSoapClient("WebServiceTestSoap");
            customerHeader.Headers.Add("X-Type", "L10");
            customerHeader.Headers.Add("X-Customer", wb.CustomerCode);
            using (new OperationContextScope(client.InnerChannel))
            {
                OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = customerHeader;
                var SFIS_CHECK_STATUS = client.SFIS_GET_DATA(ssn.ToString().ToUpper());

                if (SFIS_CHECK_STATUS.StatusCode=="0")
                {
                    wb.ColorCode = SFIS_CHECK_STATUS.Configuration.ColorCode;
                    wb.CountryCode = SFIS_CHECK_STATUS.Configuration.CountryCode;
                    wb.DeviceUnderTestSerualNumber = SFIS_CHECK_STATUS.Configuration.DeviceUnderTestSerialNumber;
                    wb.ModelName = SFIS_CHECK_STATUS.Configuration.ModelName;
                    wb.WorkOrder = SFIS_CHECK_STATUS.Configuration.DeviceDetails[0].Value;
                    wb.SKU = SFIS_CHECK_STATUS.Configuration.Sku;

                    product_movement.Start_Test = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    ID = ConexaoDB.CRUDValor_tabela($@"SELECT idproduct FROM product WHERE Serial_Number = '{ssn.ToUpper()}'");

                    string sqlP = "INSERT INTO product(Serial_Number, SKU, Color_ID, Product, Customer, Status_Code, WorkOrder) VALUES (";
                           sqlP += $@"'{ssn.ToString().Trim()}','{wb.SKU.ToString()}','{wb.ColorCode.Trim()}','{wb.ModelName.Trim()}','{wb.CustomerCode.Trim()}','0','{wb.WorkOrder}')"; 

                    string sqlPM = $@"INSERT INTO product_movement (idProduct,WorkGroup,Position,Start_Test,Status_Code,Next_Station) values ({ID},'PRETEST','1','{product_movement.Start_Test}','0','0')";
                    try
                    {
                        if (ID == 0)
                        {
                            ID = ConexaoDB.CRUDU_ID_tabela(sqlP);
                            
                            sqlPM = $@"INSERT INTO product_movement (idProduct,WorkGroup,Position,Start_Test,Status_Code,Next_Station) values ({ID},'PRETEST','1','{product_movement.Start_Test}','0','0')";

                            ConexaoDB.CRUD_tabela(sqlPM);
                        }
                        else
                        { 
                            ConexaoDB.CRUD_tabela(sqlPM);
                        }
                    }
                    catch (Exception)
                    { 
                        MSG = "Insert DB Product or product_movement is problem!!";
                        return Json(MSG);
                    }
                }
                else
                {
                    MSG = SFIS_CHECK_STATUS.ErrorMessage;
                    return Json(MSG);
                }
            }
            MSG = "set result=0";
            return Ok(MSG);
        }
    }
}
