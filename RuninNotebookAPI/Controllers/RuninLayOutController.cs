using RuninNotebookAPI.DB;
using System.Web.Http;
using System;
using System.Linq;

namespace RuninNotebookAPI.Controllers
{
    public class RuninLayOutController : ApiController
    {

        public object MSG { get; set; }

        [HttpGet]
        public IHttpActionResult OUT_GET(string idRunin)
        {
            if (idRunin is null)
            {
                NotFound();
            }

            string[] Columns = idRunin.Split(',');

            try
            {
                if (Convert.ToInt32(Columns[0]) > 0 && Columns[1].Contains("10.8.35") && Convert.ToInt32(Columns[2]) > 0 && (Columns[3].Contains("192.168") || Columns[3].Contains("172.168")))
                {
                    if (Columns.Count() == 5)
                    {
                        ConexaoDB.CRUD_tabela($@"update Runin_LayOut  set  IP_NB='{Columns[3].ToString()}',IP_Address='{Columns[1].ToString()}',IP_Gateway='{Columns[4].ToString()}', Switch_Port='{Columns[2].ToString()}'  where idRunin_Layout={Columns[0]}");
                    }
                    else
                    {
                        ConexaoDB.CRUD_tabela($@"update Runin_LayOut  set  IP_NB='{Columns[3].ToString()}',IP_Address='{Columns[1].ToString()}', Switch_Port='{Columns[2].ToString()}'  where idRunin_Layout={Columns[0]}");
                    }
                    return Json("set result=0");
                }
                else
                {
                    return Json("set result=Falta paramenters ");
                }
            }
            catch (Exception)
            {

                return Json("set result=Problema ao gravar Runin_Layout"); ;
            }
        }
    }
}
