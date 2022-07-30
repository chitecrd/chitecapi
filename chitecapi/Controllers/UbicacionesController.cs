using chitecapi.Responses;
using DataAccess;
using DataAccess.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Web.Http;

namespace chitecapi.Controllers
{
    public class UbicacionesController : ApiController
    {
        #region Actions

        /*
        // GET api/ubicaciones
        public IHttpActionResult Get()
        {
            return Get("db1");
        }

        // GET api/ubicaciones/{value}
        [HttpGet]
        public IHttpActionResult Get(string id)
        {
            return Get(id, "");
        }

        // GET api/ubicaciones/{value}/{value}
        [HttpGet]
        [Route("api/ubicaciones/{id}/{idtipo}")]
        public IHttpActionResult Get(string id, string idtipo)
        {
            string connetionString;
            SqlConnection conection;
            String dbconection = "db1";
            if (!id.Equals(""))
            {
                dbconection = id;
            }
            connetionString = ConfigurationManager.ConnectionStrings[dbconection].ConnectionString;
            conection = new SqlConnection(connetionString);
            conection.Open();

            String sql = "";
            if (idtipo.Equals(""))
            {
                sql = ConfigurationManager.AppSettings["getubicaciones"];
            }
            else
            {
                sql = ConfigurationManager.AppSettings["getubicacionesbytipo"];
            }

            if (sql == null)
            {
                sql = "select * from ubicaciones";
            }

            SqlDataAdapter dataAdapter = new SqlDataAdapter(sql, conection);
            dataAdapter.SelectCommand.Parameters.AddWithValue("idtipoubicacion", idtipo);
            DataTable table = new DataTable();
            dataAdapter.Fill(table);

            conection.Close();
            Dictionary<string, object> jsonvalues = new Dictionary<string, object>();
            jsonvalues.Add("data", table);
            jsonvalues.Add("error", 0);
            jsonvalues.Add("error_type", 0);
            jsonvalues.Add("error_message", 0);


            return Json(jsonvalues);
        }

        */


        // GET api/Ubicaciones/ListadoUbicaciones?db=db1
        /// <summary>
        /// Obtiene una lista de las Ubicaciones.
        /// </summary>
        /// <param name="db" example="db1">The Database ID</param>
        /// <param name="id_tipo_ubicacion" example="1">Tipo Ubicacion</param>
        /// <remarks>
        /// Se utiliza para obtener una lista de las ubicaciones, generado con un formato JSON:
        /// 
        ///     Example Result:
        ///     {
        ///         "data": [
        ///                   {
        ///                       "id": 1,
        ///                       "descripcion": "Punto de Venta 1",
        ///                       "cod_alterno": "TPOS 1 ",
        ///                       "id_tipo_ubicacion": 1,
        ///                       "estado": 1,
        ///                       "created_at": "2020-09-26 02:56:18",
        ///                       "updated_at": "2020-09-26 02:56:18",
        ///                       "nro_tiros": 161,
        ///                       "suma_articulos": 113,
        ///                       "costo_total": 106050.06479999995,
        ///                       "precio_total": 135241
        ///                    }
        ///                 ],
        ///         "error": 0,
        ///         "error_type": 0,
        ///         "error_message": 0
        ///     }
        /// </remarks>
        /// <response code="401">Unauthorized. Error en la configuracion de la base de datos</response>              
        /// <response code="200">OK. Devuelve el objeto solicitado.</response>        
        /// <response code="404">NotFound. No se ha encontrado el objeto solicitado.</response>
        [HttpGet]
        public IHttpActionResult ListadoUbicaciones([FromUri] string db = "db1", string id_tipo_ubicacion = "", string saveresult = "0", string tablename = "ubicacionestest")
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




            var sql = ConfigurationManager.AppSettings["getubicaciones"];

            if (sql == null)
            {
                sql = " select u.*, COALESCE(r.nro_tiros,0) as nro_tiros , COALESCE(r.suma_articulos,0) as suma_articulos,                  COALESCE(r.costo_total,0) as costo_total ,COALESCE(r.precio_total,0) as precio_total                   from ubicaciones u left join v_resumebyubicacion r on u.id=r.id_ubicacion where 1=1";
            }

            if (!id_tipo_ubicacion.Equals(""))
                sql = sql + " AND u.id_tipo_ubicacion = " + id_tipo_ubicacion;




            dataUtil.PrepareStatement(sql);


            DataTable table = new DataTable();
            dataUtil.FillDatatable(table);

            dataUtil.CloseConnection();


            if (saveresult.Equals("1"))
            {
                dataUtil = new DataUtil("dbconnection");
                dataUtil.Connect();
                string jsonvalue = JsonConvert.SerializeObject(table);
                sql = $"  exec createtablefromjson @tabla='{tablename}', @json='{jsonvalue}'; ";
                dataUtil.ExecuteCommand(sql);
                dataUtil.CloseConnection();
            }



            Dictionary<string, object> jsonvalues = new Dictionary<string, object>();
            jsonvalues.Add("data", table);
            jsonvalues.Add("error", 0);
            jsonvalues.Add("error_type", 0);
            jsonvalues.Add("error_message", 0);


            return Json(jsonvalues);
        }


        [HttpPost]
        public async Task<IHttpActionResult> Guardar([FromBody] List<Ubicacion> ubicaciones, string db)
        {
            if (string.IsNullOrEmpty(db))
            {
                db = $"{ConfigurationManager.AppSettings["default_db"]}";
            }

            if (ConfigurationManager.ConnectionStrings[db] == null)
            {
                return new CustomJsonActionResult(
                    System.Net.HttpStatusCode.NotFound,
                    new JsonErrorResponse(1, 400, $"La base de datos {db} no existe."));
            }

            var dbAccess = new SqlServerDataAccess(ConfigurationManager.ConnectionStrings[db].ConnectionString, ConfigurationManager.ConnectionStrings[db].ProviderName);

            foreach (var ubicacion in ubicaciones)
            {
                if (ubicacion?.IsNotInitialized == true)
                {
                    return new CustomJsonActionResult(
                        System.Net.HttpStatusCode.NotFound,
                        new JsonErrorResponse(1, 400, "Faltan parámetros"));
                }

                try
                {
                    var sql = ConfigurationManager.AppSettings["crear_ubicacion"];
                    var parameters = new
                    {
                        descripcion = ubicacion.descripcion,
                        cod_alterno = ubicacion.cod_alterno,
                        estado = ubicacion.estado,
                        id_tipo_ubicacion = ubicacion.id_tipo_ubicacion,
                        created_at = DateTime.Now,
                        updated_at = DateTime.Now
                    };

                    await dbAccess.SaveDataAsync(sql, parameters);
                }
                catch (Exception exception)
                {
                    return new CustomJsonActionResult(
                        System.Net.HttpStatusCode.NotFound,
                        new JsonErrorResponse(1, 1, exception.GetBaseException().Message));
                }
            }

            return new CustomJsonActionResult(
                System.Net.HttpStatusCode.OK,
                new JsonErrorResponse(0, 0, "0"));
        }

        [HttpPost]
        public async Task<IHttpActionResult> Editar([FromBody] List<Ubicacion> ubicaciones, string db)
        {
            if (string.IsNullOrEmpty(db))
            {
                db = $"{ConfigurationManager.AppSettings["default_db"]}";
            }

            if (ConfigurationManager.ConnectionStrings[db] == null)
            {
                return new CustomJsonActionResult(
                    System.Net.HttpStatusCode.NotFound,
                    new JsonErrorResponse(1, 400, $"La base de datos {db} no existe."));
            }

            var dbAccess = new SqlServerDataAccess(ConfigurationManager.ConnectionStrings[db].ConnectionString, ConfigurationManager.ConnectionStrings[db].ProviderName);

            foreach (var ubicacion in ubicaciones)
            {
                if (ubicacion?.IsNotInitialized == true)
                {
                    return new CustomJsonActionResult(
                        System.Net.HttpStatusCode.NotFound,
                        new JsonErrorResponse(1, 400, "Faltan parámetros"));
                }

                try
                {
                    var sql = ConfigurationManager.AppSettings["editar_ubicacion"];
                    var parameters = new
                    {
                        id = ubicacion.id,
                        descripcion = ubicacion.descripcion,
                        cod_alterno = ubicacion.cod_alterno,
                        estado = ubicacion.estado,
                        id_tipo_ubicacion = ubicacion.id_tipo_ubicacion,
                        updated_at = DateTime.Now
                    };
                    await dbAccess.SaveDataAsync(sql, parameters);
                }
                catch (Exception exception)
                {
                    return new CustomJsonActionResult(
                        System.Net.HttpStatusCode.NotFound,
                        new JsonErrorResponse(1, 1, exception.GetBaseException().Message));
                }
            }

            return new CustomJsonActionResult(
                System.Net.HttpStatusCode.OK,
                new JsonErrorResponse(0, 0, "0"));
        }

        #endregion

        #region Helper methods



        #endregion        
    }


}
