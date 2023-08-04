using System;
using System.Web.Http;
using RuninNotebookAPI.Models;
using RuninNotebookAPI.DB;
using System.Data;
using System.Reflection;

namespace RuninNotebookAPI.Controllers
{
    public class AssemblyOutController : ApiController
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
            controller = "AssemblyOut - " + asse[1];

            string[] Columns = ssn.Split(',');
            Columns[0] = Columns[0].ToUpper();

            Produto_Movimento product_movement = new Produto_Movimento();

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

            Produto product = new Produto(Columns[0]);

            ID = product.ID;

            if (ID == 0)
            {
                MSG = "set result=Serial number Not found";
                ConexaoDB.CRUDU_ID_tabela($@"insert into logruninnb (log,MSG,controller,SSN) values ('{ssn}','{MSG}','{controller}'),{Columns[0]}");
                return Ok(MSG);
            }

            product_movement.Start_Test = DateTime.Now.AddMinutes(-30).ToString("yyyy-MM-dd HH:mm:ss");
            product_movement.End_Test = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            product_movement.Operator_ID = Columns[1];
            product_movement.Error_Code = Columns[2];
            product_movement.Fail_Description = Columns[3];
            product_movement.Status_Code = "1";

            try
            {
                string sqlPM = $@"select if(max(idProduct_Movement)>0,max(idProduct_Movement),0) from product_movement where idProduct = {ID} AND WorkGroup = 'ASSEMBLY1';";
                MSG = $@"set result=Problema na consulta IDProduct_Moment na tabela Product_Movement ";
                PMID = ConexaoDB.CRUDValor_tabela(sqlPM);

                if (PMID == 0)
                {
                    product_movement.Operator_ID = "OPERADOR-TESTE";
                    string sqlPM_Insert = $@"INSERT INTO product_movement (idProduct,WorkGroup,Position,Start_Test,Status_Code,Next_Station) values ({ID},'ASSEMBLY1','1567','{product_movement.Start_Test}','0','0')";
                    MSG = $@"set result=Problema na inclusao de ASSEMBLY1 IN - idPM={PMID}";
                    PMID = ConexaoDB.CRUDU_ID_tabela(sqlPM_Insert);
                }

                string SQL = $@"UPDATE product_movement set Operator_ID='{product_movement.Operator_ID}', Error_Code='{product_movement.Error_Code}', Fail_Description='{product_movement.Fail_Description}', End_Test='{product_movement.End_Test}',Status_Code='1' ";
                SQL += $@"WHERE idProduct_movement = {PMID}";
                MSG = "set result=Erro ao Update na tabela Product_Movement";
                ConexaoDB.CRUD_tabela(SQL);
            }
            catch (Exception)
            {
                ConexaoDB.CRUDU_ID_tabela($@"insert into logruninnb (log,Model,MSG,SSN,controller) values ('{ssn}','{product.Product}','{MSG}','{ssn}','{controller}')");
                return Ok(MSG);
            }   
            MSG = "set result=0";
            return Ok(MSG);
        }
    }
}
