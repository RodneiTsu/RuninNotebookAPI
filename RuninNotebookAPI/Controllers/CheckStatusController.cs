
using RuninNotebookAPI.ServiceReference1;
using RuninNotebookAPI.Models;
using RuninNotebookAPI.DB;
using System.ServiceModel.Channels;
using System.Web.Http;
using System.ServiceModel;
using System;

namespace RuninNotebookAPI.Controllers
{
    public class CheckStatusController : ApiController
    {
        public string MSG { get; set; }
        public int ID { get; set; }
        public int SKUID { get; set; }
        public int LOGID { get; set; }
        public string Station { get; set; }
        public string controller { get; set; }

        [HttpGet]
        public dynamic GET(string ssn)
        {
            if (ssn is null)
            {
                NotFound();
            }
            controller = "CheckStatus";

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
                    ConexaoDB.CRUD_tabela($@"insert into logruninnb (log,MSG,controller) values ('{ssn}','{MSG}','{controller}')");
                    return Ok( MSG) ;
                }
            }
            else
            {
                MSG = "set result=Serial number length is invalid";
                ConexaoDB.CRUD_tabela($@"insert into logruninnb (log,MSG,controller) values ('{ssn}','{MSG}','{controller}')");
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
                var SFIS_CHECK_STATUS = client.SFIS_CHECK_STATUS(ssn,"PRETEST");

                if (SFIS_CHECK_STATUS.StatusCode=="0")
                {

                    MSG ="set checkstatus=PRETEST";

                }
                else
                {
                    MSG = SFIS_CHECK_STATUS.ErrorMessage;


                    if (SFIS_CHECK_STATUS.ErrorMessage.Contains("SHIPPACK")) {  MSG ="SHIPPACK"; }
                    else if (SFIS_CHECK_STATUS.ErrorMessage.Contains("SHIPPED")) {  MSG ="set checkstatus=SHIPPED"; }
                    else if (SFIS_CHECK_STATUS.ErrorMessage.Contains("PACKING1")) { MSG ="set checkstatus=PACKING1"; }
                    else if (SFIS_CHECK_STATUS.ErrorMessage.Contains("PACKING2")) {  MSG ="set checkstatus=PACKING2"; }
                    else if (SFIS_CHECK_STATUS.ErrorMessage.Contains("ASSIGNOOBA_SELECTED")) {  MSG ="set checkstatus=ASSIGNOOBA_SELECTED"; }
                    else if (SFIS_CHECK_STATUS.ErrorMessage.Contains("PACKING3")) {  MSG ="set checkstatus=PACKING3"; }
                    else if (SFIS_CHECK_STATUS.ErrorMessage.Contains("OOBA_SELECTED")) {  MSG ="set checkstatus=OOBA_SELECTED"; }
                    else if (SFIS_CHECK_STATUS.ErrorMessage.Contains("ASSIGNOBE")) {  MSG ="set checkstatus=ASSIGNOBE"; }
                    else if (SFIS_CHECK_STATUS.ErrorMessage.Contains("OBA")) {  MSG ="set checkstatus=OBA"; }
                    else if (SFIS_CHECK_STATUS.ErrorMessage.Contains("OBE")) {  MSG ="set checkstatus=OBE"; }
                    else if (SFIS_CHECK_STATUS.ErrorMessage.Contains("PALLETIZATION")) {  MSG ="set checkstatus=PALLETIZATION"; }
                    else if (SFIS_CHECK_STATUS.ErrorMessage.Contains("PRETEST")) {  MSG ="set checkstatus=PRETEST"; }
                    else if (SFIS_CHECK_STATUS.ErrorMessage.Contains("RUN-IN")) {  MSG ="set checkstatus=RUN-IN"; }
                    else { MSG = "set result=" + SFIS_CHECK_STATUS.ErrorMessage; }



                    
                    return Ok(MSG) ;
                }
            }
            //MSG = "set result=0";
            return Ok(MSG); 
        }
    }
}
