using RuninNotebookAPI.ServiceReference1;
using System;
using RuninNotebookAPI.Models;
using RuninNotebookAPI.DB;
using System.Web.Http;
using System.Data;
using System.Reflection;
using System.ServiceModel.Channels;
using System.ServiceModel;

namespace RuninNotebookAPI.Controllers
{    
    public class PretestOutController : ApiController
    {
        public string MSG { get; set; }
        public int ID { get; set; }
        public int IDPM { get; set; }
        public string controller { get; set; }

        [HttpGet]
        public IHttpActionResult OUT_GET(string ssn)
        {
            if (ssn is null)
            {
                NotFound();
            }

            string[] asse = Assembly.GetExecutingAssembly().FullName.ToString().Split(',');
            controller = "PretestOut - " + asse[1];

            string[] Columns = ssn.Split(',');

            if (Columns[0].Length == 15 || Columns[0].Length == 12 || Columns[0].Length == 22)
            {
                if (Columns[0].Substring(4, 2) == "B6" || Columns[0].Substring(9, 3) == "935")
                {
                    MSG = "set result=0";
                }
                else
                {
                    MSG = "set result=It was not possible to define a valid customer";
                    ConexaoDB.CRUDU_ID_tabela($@"insert into logruninnb (log,MSG,controller,SSN) values ('{ssn}','{MSG}','{controller}'),{Columns[0]}");
                    return Ok(MSG);
                }
            }
            else
            {
                MSG = "set result=Serial number length is invalid";
                ConexaoDB.CRUDU_ID_tabela($@"insert into logruninnb (log,MSG,controller,SSN) values ('{ssn}','{MSG}','{controller}'),{Columns[0]}");
                return Ok(MSG);
            }
            Columns[0] = Columns[0].ToUpper().Trim();
            Produto product = new Produto(Columns[0]);
            ID = product.ID;
            if (ID == 0)
            {
                MSG = "set result=Serial number nao existe no cadastro de Produto";
                ConexaoDB.CRUDU_ID_tabela($@"insert into logruninnb (log,MSG,controller) values ('{ssn}','{MSG}','{controller}')");
                return Ok(MSG);
            }
            string slqPM = $@"select if(max(idProduct_Movement)>0,max(idProduct_Movement),0) ";
                  slqPM += $@"from engteste.product p inner ";
                  slqPM += $@"join engteste.product_movement pm on p.idProduct = pm.idProduct ";
                  slqPM += $@"where p.idProduct = {ID} and pm.WorkGroup = 'PRETEST'";
            try
            {
                IDPM = ConexaoDB.CRUDValor_tabela(slqPM);
            }
            catch (Exception)
            {
                int existeMov = 0;
                try
                {
                    existeMov = ConexaoDB.CRUDValor_tabela($@"select if(max(idProduct_Movement)>0,max(idProduct_Movement),0) from product_movement pm where PM.idProduct = {ID} and PM.Status_Code = '1' and PM.WorkGroup = 'PRETEST'");
                    MSG = $@"set result=Existe PRETEST fechado registro-{existeMov}";
                }
                catch (Exception)
                {
                    MSG = "set result=Serial number nao encontrado PRETEST IN";
                }
                ConexaoDB.CRUDU_ID_tabela($@"insert into logruninnb (log,Model,SSN,MSG,controller) values ('{ssn}','{product.Product}','{product.Serial_Number}','{MSG}','{controller}')");
                return Ok(MSG);
            }
            Produto_Movimento product_movement = new Produto_Movimento(IDPM);
            if (product_movement.Fail_Description=="PASS" || product_movement.Fail_Description == "FAIL")
            {
                MSG = $@"set result=ja existe prestet fechado idPM={IDPM}";
                ConexaoDB.CRUDU_ID_tabela($@"insert into logruninnb (log,Model,SSN,MSG,controller) values ('{ssn}','{product.Product}','{product.Serial_Number}','{MSG}','{controller}')");
                return Ok(MSG);
            }
            try
            {
                MSG = "set result=Problema falta de informação SSN-ID-ErrorCode-PASS ou FAIL";
                product_movement.End_Test = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                product_movement.Operator_ID = Columns[1];
                product_movement.Error_Code = Columns[2];
                product_movement.Fail_Description = Columns[3];
                product_movement.Status_Code = "1";
                string SQL = $@"UPDATE product_movement set Operator_ID='{product_movement.Operator_ID}', Error_Code='{product_movement.Error_Code}', Fail_Description='{product_movement.Fail_Description}', End_Test='{product_movement.End_Test}',Status_Code='1' ";
                SQL += $@"WHERE idProduct_Movement = {IDPM};";
                MSG = "set result=Erro ao gravar product_movement";
                ConexaoDB.CRUD_tabela(SQL);

                HttpRequestMessageProperty customerHeader = new HttpRequestMessageProperty();
                WebServiceTestSoapClient client = new WebServiceTestSoapClient("WebServiceTestSoap");
                customerHeader.Headers.Add("X-Type", "L10");
                customerHeader.Headers.Add("X-Customer", product.Customer);

                using (new OperationContextScope(client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = customerHeader;
                    var SFIS_CHECK_STATUS = client.SFIS_LOGOUT(product.Serial_Number,"20692","PL03","PRETEST","PRETEST-01","0");
                    if (SFIS_CHECK_STATUS.StatusCode == "0")
                    {

                    }
                    else
                    {

                    }
                }




                }
            catch (Exception)
            {
                ConexaoDB.CRUDU_ID_tabela($@"insert into logruninnb (log,Model,SSN,MSG,controller) values ('{ssn}','{product.Product}','{product.Serial_Number}','{MSG}','{controller}')");
                return Ok( MSG) ;
            }
            MSG = "set result=0";
            return Ok( MSG) ;
        }
    }
}
