
using RuninNotebookAPI.ServiceReference1;
using RuninNotebookAPI.Models;
using RuninNotebookAPI.DB;
using System.ServiceModel.Channels;
using System.Web.Http;
using System.ServiceModel;
using System.Reflection;

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

            string[] asse = Assembly.GetExecutingAssembly().FullName.ToString().Split(',');
            controller = "CheckStatus - " + asse[1];

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

                    //"Should In : RUN-IN"

                    if (SFIS_CHECK_STATUS.ErrorMessage.Contains("Should In : SHIPPACK")) {  MSG = "set checkstatus=SHIPPACK"; }
                    else if (SFIS_CHECK_STATUS.ErrorMessage.Contains("Should In : SHIPPED")) {  MSG ="set checkstatus=SHIPPED"; }
                    else if (SFIS_CHECK_STATUS.ErrorMessage.Contains("Should In : PACKING1")) { MSG ="set checkstatus=PACKING1"; }
                    else if (SFIS_CHECK_STATUS.ErrorMessage.Contains("Should In : PACKING2")) {  MSG ="set checkstatus=PACKING2"; }
                    else if (SFIS_CHECK_STATUS.ErrorMessage.Contains("Should In : ASSIGNOOBA_SELECTED")) {  MSG ="set checkstatus=ASSIGNOOBA_SELECTED"; }
                    else if (SFIS_CHECK_STATUS.ErrorMessage.Contains("Should In : PACKING3")) {  MSG ="set checkstatus=PACKING3"; }
                    else if (SFIS_CHECK_STATUS.ErrorMessage.Contains("Should In : OOBA_SELECTED")) {  MSG ="set checkstatus=OOBA_SELECTED"; }
                    else if (SFIS_CHECK_STATUS.ErrorMessage.Contains("Should In : ASSIGNOBE")) {  MSG ="set checkstatus=ASSIGNOBE"; }
                    else if (SFIS_CHECK_STATUS.ErrorMessage.Contains("Should In : OBA")) {  MSG ="set checkstatus=OBA"; }
                    else if (SFIS_CHECK_STATUS.ErrorMessage.Contains("Should In : OBE")) {  MSG ="set checkstatus=OBE"; }
                    else if (SFIS_CHECK_STATUS.ErrorMessage.Contains("Should In : PALLETIZATION")) {  MSG ="set checkstatus=PALLETIZATION"; }
                    else if (SFIS_CHECK_STATUS.ErrorMessage.Contains("Should In : RUN-IN")) { MSG = "set checkstatus=RUN-IN"; }
                    else { MSG = "set result=" + SFIS_CHECK_STATUS.ErrorMessage; }

                    return Ok(MSG) ;
                }
            }
            //MSG = "set result=0";
            return Ok(MSG); 
        }
    }
}
