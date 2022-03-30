using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using RuninNotebookAPI.Models;
using RuninNotebookAPI.DB;
using System.Web.Http;

namespace RuninNotebookAPI.Controllers
{
    public class RuninInsertController : ApiController
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

            Runin_LayOut rl = new Runin_LayOut();

            try
            {
                if (Convert.ToInt32(Columns[0]) > 0 && Columns[1].Contains("10.8.35") && Convert.ToInt32(Columns[2]) > 0 && (Columns[3].Contains("192.168") || Columns[3].Contains("172.168")))
                {
                    ConexaoDB.CRUD_tabela($@"update Runin_LayOut  set  IP_NB='{Columns[3].ToString()}',IP_Address='{Columns[1].ToString()}', Switch_Port='{Columns[2].ToString()}'  where idRunin_Layout={Columns[0]}");

                    return Json("set result=0");
                }
                else
                {
                    return Json("Falta paramenters ");
                }
            }
            catch (Exception)
            {

                return Json("Problema ao gravar Runin_Layout"); ;
            }



        }

    }
}
