using System;
using DataAccess;
using DataAccess.Models;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using chitecapi.Responses;
using Infrastructure.UnitOfWork;
using System.Threading.Tasks;

namespace chitecapi.Controllers
{
    public class LogcodigosinexistentesController : ChitecApiController
    {
        #region Actions
        // GET api/Logcodigosinexistentes?db=db1
        /// <summary>
        /// Obtiene una lista los codigos inexistentes de cada ubicacion.
        /// </summary>
        /// <param name="db" example="db1">The Database ID</param>
        /// <param name="id_ubicacion" example="111">ID Ubicacion</param>
        /// <param name="id_almacen" example="111">ID Almacen</param>        /// <param name="fecha_desde" example="Coca cola">Fecha Desde</param>
        /// <param name="fecha_hasta" example="Coca cola">fecha Hasta</param>
        /// <remarks>
        /// Se utiliza para obtener una lista los codigos inexistentes de cada ubicacion, generado con un formato JSON:
        /// 
        ///     Example Result:
        ///     {
        ///         "data": [
        ///                   {
        ///                      "Id": 1,
        ///                      "Id_Terminal": "2222",
        ///                      "Barcode": "123",
        ///                      "id_almacen": 1,
        ///                      "Id_Ubicacion": 1,
        ///                      "Id_usuario": 1, 
        ///                      "Fecha": "2021-10-18T00:00:00",
        ///                      "descripcionubicacion": "Cervezas",
        ///                      "almacen": "",
        ///                      "cod_alterno_almacen": ""
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
        public IHttpActionResult Index([FromUri] string db = "db1", string fecha_desde = "", string fecha_hasta = "", string id_ubicacion = "", string id_almacen = "")
        {

            string connetionString;
            SqlConnection conection;
            string dbconection = "db1";

            if (!db.Equals(""))
            {
                dbconection = db;
            }
            connetionString = ConfigurationManager.ConnectionStrings[dbconection].ConnectionString;
            conection = new SqlConnection(connetionString);

            conection.Open();


            var sql = ConfigurationManager.AppSettings["getlogcodigosinexistentes"];

            if (sql == null)
            {
                sql = "SELECT d.*, " +
                      "  COALESCE(u.descripcion, '') as descripcionubicacion,COALESCE(a.descripcion, '') as almacen, " +
                      "  COALESCE(a.cod_alterno, '') as cod_alterno_almacen " +
                      "  FROM log_codigos_inexistentes d " +
                      "  left join ubicaciones u " +
                      "      on(d.id_ubicacion = u.id) " +
                      "  left join almacen a " +
                      "      on(a.id = d.id_almacen) " +
                      "  WHERE 1 = 1  ";
            }


            
            

            if (!fecha_desde.Equals(""))
                sql = sql + " AND d.fecha_registro >= '" + fecha_desde + "'";

            if (!fecha_hasta.Equals(""))
                sql = sql + " AND d.fecha_registro <= '" + fecha_hasta + "'";

            if (!id_ubicacion.Equals(""))
                sql = sql + " AND d.id_ubicacion = " + id_ubicacion;

            if (!id_almacen.Equals(""))
                sql = sql + " AND d.id_almacen = " + id_almacen;



            SqlDataAdapter dataAdapter = new SqlDataAdapter(sql, conection);


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


        /// <summary>
        /// Guardar Logs Impresiones.
        /// </summary>
        /// <param name="db" example="db1">The Database ID</param>
        /// <remarks>
        /// Se utiliza para Guardar multiples Logs Impresiones, Enviando un json con la información de la captura
        /// </remarks>
        /// <response code="401">Unauthorized. Error en la configuracion de la base de datos</response>              
        /// <response code="200">OK. Devuelve el objeto solicitado.</response>        
        /// <response code="404">NotFound. No se ha encontrado el objeto solicitado.</response>
        [HttpPost]
        public async Task<IHttpActionResult> Guardar([FromBody] List<LogCodigosInexistentes> logCodigosInexistentes, string db)
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

            foreach (var logs in logCodigosInexistentes)
            {
                if (logs?.IsNotInitialized == true)
                {
                    return new CustomJsonActionResult(
                        System.Net.HttpStatusCode.NotFound,
                        new JsonErrorResponse(1, 400, "Faltan parámetros"));
                }

                try
                {
                    await InsertRecord(logs, dbAccess);

                }
                catch (Exception exception)
                {
                    return new CustomJsonActionResult(
                        System.Net.HttpStatusCode.NotFound,
                        new JsonErrorResponse(1, 1, exception.GetBaseException().Message));
                }
            }

            return new CustomJsonActionResult(
                System.Net.HttpStatusCode.NotFound,
                new JsonErrorResponse(0, 0, "0"));
        }

        /// <summary>
        /// Editar Log Impresiones.
        /// </summary>
        /// <param name="db" example="db1">The Database ID</param>
        /// <remarks>
        /// Se utiliza para Editar multiples Entradas de Log Impresiones, Enviando un json con la información de la captura
        /// </remarks>
        /// <response code="401">Unauthorized. Error en la configuracion de la base de datos</response>              
        /// <response code="200">OK. Devuelve el objeto solicitado.</response>        
        /// <response code="404">NotFound. No se ha encontrado el objeto solicitado.</response>
        [HttpPost]
        public async Task<IHttpActionResult> Editar([FromBody] List<LogCodigosInexistentes> logCodigosInexistentes, string db)
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

            foreach (var logs in logCodigosInexistentes)
            {
                if (logs?.IsNotInitialized == true)
                {
                    return new CustomJsonActionResult(
                        System.Net.HttpStatusCode.NotFound,
                        new JsonErrorResponse(1, 400, "Faltan parámetros"));
                }

                try
                {
                    await EditRecord(logs, dbAccess);

                }
                catch (Exception exception)
                {
                    return new CustomJsonActionResult(
                        System.Net.HttpStatusCode.NotFound,
                        new JsonErrorResponse(1, 1, exception.GetBaseException().Message));
                }
            }

            return new CustomJsonActionResult(
                System.Net.HttpStatusCode.NotFound,
                new JsonErrorResponse(0, 0, "0"));
        }

        #endregion

        #region Helper Methods

        private async Task EditRecord(LogCodigosInexistentes logs, SqlServerDataAccess dbAccess)
        {
            var sql = ConfigurationManager.AppSettings["editar_logcodigosinexistentes"];
            var parameters = new
            {
                id_Terminal = logs.id_Terminal,
                Barcode = logs.Barcode,
                id_almacen = logs.id_almacen,
                id_ubicacion = logs.id_ubicacion,
                Id_usuario = logs.id_usuario,
                Fecha = logs.Fecha
            };

            await dbAccess.SaveDataAsync(sql, parameters);
        }

        private async Task InsertRecord(LogCodigosInexistentes logs, SqlServerDataAccess dbAccess)
        {
            var sql = ConfigurationManager.AppSettings["crear_logcodigosinexistentes"];
            var parameters = new
            {
                id_Terminal = logs.id_Terminal,
                Barcode = logs.Barcode,
                id_almacen = logs.id_almacen,
                id_ubicacion = logs.id_ubicacion,
                Id_usuario = logs.id_usuario,
                Fecha = logs.Fecha
            };

            await dbAccess.SaveDataAsync(sql, parameters);
        }

        #endregion


    }
}
