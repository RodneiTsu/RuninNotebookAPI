using RuninNotebookAPI.ServiceReference1;
using System.ServiceModel.Channels;
using System.Web.Http;
using System.ServiceModel;


namespace RuninNotebookAPI.Controllers
{
    public class RuninLayOutController : ApiController
    {

        public object MSG { get; set; }

        [HttpGet]
        public IHttpActionResult OUT_GET(string SSN)
        {
            if (SSN is null)
            {
                NotFound();
            }

            string[] Columns = SSN.Split(',');

            HttpRequestMessageProperty customerHeader = new HttpRequestMessageProperty();
            WebServiceTestSoapClient client = new WebServiceTestSoapClient("WebServiceTestSoap");
            customerHeader.Headers.Add("X-Type", "L06");
            customerHeader.Headers.Add("X-Customer", "ACER");
            using (new OperationContextScope(client.InnerChannel))
            {
                OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = customerHeader;
                var SFIS_CHECK_STATUS = client.SFIS_GET_DATA(Columns[0]);

                if (SFIS_CHECK_STATUS.StatusCode == "0")
                {
                    string ColorCode = SFIS_CHECK_STATUS.Configuration.ColorCode;
                    string CountryCode = SFIS_CHECK_STATUS.Configuration.CountryCode;
                    string DeviceUnderTestSerialNumber = SFIS_CHECK_STATUS.Configuration.DeviceUnderTestSerialNumber;
                    string ModelName = SFIS_CHECK_STATUS.Configuration.ModelName;
                    string WorkOrder = SFIS_CHECK_STATUS.Configuration.DeviceDetails[0].Value;
                    string SKU = SFIS_CHECK_STATUS.Configuration.Sku;
                }
            }
            return Ok("OK");
        }
    }
}
