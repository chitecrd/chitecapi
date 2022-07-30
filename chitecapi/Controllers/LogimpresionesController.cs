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
    public class LogimpresionesController : ChitecApiController
    {
        #region Actions
        // GET api/Logimpresiones?db=db1
        /// <summary>
        /// Obtiene una lista del los logs de cada impresion.
        /// </summary>
        /// <param name="db" example="db1">The Database ID</param>
        /// <param name="Barcode" example="111">Codigo Barra</param>
        /// <param name="No_articulo" example="111">Numero Articulo</param>
        /// <param name="Id_usuario" example="111">Id Usuario</param>
        /// <param name="fecha_desde" example="Coca cola">Fecha Desde</param>
        /// <param name="fecha_hasta" example="Coca cola">fecha Hasta</param>
        /// <remarks>
        /// Se utiliza para obtener una lista del los logs de cada impresion, generado con un formato JSON:
        /// 
        ///     Example Result:
        ///     {
        ///         "data": [
        ///                   {
        ///                       "Id": 1,
        ///                       "Id_Terminal": "122",
        ///                       "Barcode": "74685110      ",
        ///                       "descripcion": "BRUGAL CARTA BLANCA 350ML               ",
        ///                       "cantidad_impresiones": 5,
        ///                       "Id_usuario": 1,
        ///                       "No_articulo": "iswar123",
        ///                       "Fecha": "2021-10-18T00:00:00"
        ///                    }
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
        public IHttpActionResult Index([FromUri] string db = "db1", string fecha_desde = "", string fecha_hasta = "", string Barcode = "", string No_articulo = "", string Id_usuario = "")
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


            var sql = ConfigurationManager.AppSettings["getlogimpresiones"];

            if (sql == null)
            {
                 sql = "select "+
                        " l.Id, "+
                        " l.Id_Terminal, " +
                        " l.Barcode, " +
                        " a.descripcion, " +
                        " l.cantidad_impresiones, " +
                        " l.Id_usuario, " +
                        " l.No_articulo, " +
                        " l.Fecha " +
                        " from log_impresiones l " +
                        " left join articulos a on a.no_articulo = l.No_articulo and a.codigo_barra = l.Barcode " +
                        " where 1=1 ";
            }





            if (!fecha_desde.Equals(""))
                sql = sql + " AND l.Fecha >= '" + fecha_desde + "'";

            if (!fecha_hasta.Equals(""))
                sql = sql + " AND l.Fecha <= '" + fecha_hasta + "'";

            if (!Barcode.Equals(""))
                sql = sql + " AND l.Barcode = '" + Barcode + "'";

            if (!No_articulo.Equals(""))
                sql = sql + " AND l.No_articulo = '" + No_articulo + "'";


            if (!Id_usuario.Equals(""))
                sql = sql + " AND l.Id_usuario = " + Id_usuario;


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
        public async Task<IHttpActionResult> Guardar([FromBody] List<LogImpresiones> logimpresiones, string db)
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

            foreach (var logimpresion in logimpresiones)
            {
                if (logimpresion?.IsNotInitialized == true)
                {
                    return new CustomJsonActionResult(
                        System.Net.HttpStatusCode.NotFound,
                        new JsonErrorResponse(1, 400, "Faltan parámetros"));
                }

                try
                {
                        await InsertRecord(logimpresion, dbAccess);
                  
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
        public async Task<IHttpActionResult> Editar([FromBody] List<LogImpresiones> logimpresiones, string db)
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

            foreach (var logimpresion in logimpresiones)
            {
                if (logimpresion?.IsNotInitialized == true)
                {
                    return new CustomJsonActionResult(
                        System.Net.HttpStatusCode.NotFound,
                        new JsonErrorResponse(1, 400, "Faltan parámetros"));
                }

                try
                {
                    await EditRecord(logimpresion, dbAccess);

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

        private async Task EditRecord(LogImpresiones logImpresiones, SqlServerDataAccess dbAccess)
        {
            var sql = ConfigurationManager.AppSettings["editar_logimpresion"];
            var parameters = new
            {
                id_Terminal = logImpresiones.id_Terminal,
                Barcode = logImpresiones.Barcode,
                cantidad_impresiones = logImpresiones.cantidad_impresiones,
                No_articulo = logImpresiones.No_articulo,
                Id_usuario = logImpresiones.id_usuario,
                Fecha = logImpresiones.Fecha
            };

            await dbAccess.SaveDataAsync(sql, parameters);
        }

        private async Task InsertRecord(LogImpresiones logImpresiones, SqlServerDataAccess dbAccess)
        {
            var sql = ConfigurationManager.AppSettings["crear_logimpresion"];
            var parameters = new
            {
                id_Terminal = logImpresiones.id_Terminal,
                Barcode = logImpresiones.Barcode,
                cantidad_impresiones = logImpresiones.cantidad_impresiones,
                No_articulo = logImpresiones.No_articulo,
                Id_usuario = logImpresiones.id_usuario,
                Fecha = logImpresiones.Fecha
            };

            await dbAccess.SaveDataAsync(sql, parameters);
        }

        #endregion

    }
}
