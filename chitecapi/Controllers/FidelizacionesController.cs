using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Http;


namespace chitecapi.Controllers
{
    public class FidelizacionesController : ApiController
    {
        [HttpGet]
        public IHttpActionResult Listaclientes(string db = "db1", string no_cedula = "00112975321", string tablename = "Fidelizaciones")
        {
            string connetionString;
            SqlConnection conection;
            string dbconection = "db1";

            if (!db.Equals(""))
            {
                dbconection = db;
            }

            DataUtil dataUtil = new DataUtil(dbconection);

            dataUtil.Connect();

            var sql = ConfigurationManager.AppSettings["ConsultaPuntos"];

            dataUtil.PrepareStatement(sql);

            if (!no_cedula.Equals("00112975321"))
            {
                sql = ConfigurationManager.AppSettings["ConsultaPuntosByCedula"];
                dataUtil.PrepareStatement(sql);

                dataUtil.AddParameter("@no_cedula", no_cedula);
            }


            DataTable table = new DataTable();
            dataUtil.FillDatatable(table);

            dataUtil.CloseConnection();

            dataUtil = new DataUtil("dbconnection");
            dataUtil.Connect();
            string jsonvalue = JsonConvert.SerializeObject(table);
            sql = $"  exec createtablefromjson @tabla='{tablename}', @json='{jsonvalue}'; ";
            dataUtil.ExecuteCommand(sql);
            dataUtil.CloseConnection();

            Dictionary<string, object> jsonvalues = new Dictionary<string, object>();
            jsonvalues.Add("no_cedula", table);
            jsonvalues.Add("error", 0);
            jsonvalues.Add("error_type", 0);
            jsonvalues.Add("error_message", 0);

            return Json(jsonvalues);
        }
    }
}