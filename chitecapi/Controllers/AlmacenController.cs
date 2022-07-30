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
    public class AlmacenController : ApiController
    {
        #region Actions


        // GET api/Almacen/ListadoAlmacenes?db=db1
        /// <summary>
        /// Obtiene una lista de los almacenes.
        /// </summary>
        /// <param name="db" example="db1">The Database ID</param>
        /// <param name="id_tipo_ubicacion" example="1">Tipo Ubicacion</param>
        /// <remarks>
        /// Se utiliza para obtener una lista de los almacenes, generado con un formato JSON:
        /// 
        ///     Example Result:
        ///     {
        ///         "data": [
        ///                   {
        ///                       "id": 1,
        ///                       "descripcion": "Almacen1",
        ///                       "cod_alterno": "ALM1",
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
        public IHttpActionResult ListadoAlmacenes([FromUri] string db = "db1", string saveresult = "0", string tablename = "almacentest")
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





            var sql = ConfigurationManager.AppSettings["getalmacen"];

            if (sql == null)
            {
                sql = " select * from almacen";
            }




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
        public async Task<IHttpActionResult> Guardar([FromBody] List<Almacen> almacenes, string db)
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

            var dbAccess = new SqlServerDataAccess(ConfigurationManager.ConnectionStrings[db].ConnectionString);

            foreach (var almacen in almacenes)
            {
                if (almacen?.IsNotInitialized == true)
                {
                    return new CustomJsonActionResult(
                        System.Net.HttpStatusCode.NotFound,
                        new JsonErrorResponse(1, 400, "Faltan parámetros"));
                }

                try
                {
                    var sql = ConfigurationManager.AppSettings["crear_almacen"];
                    var parameters = new
                    {
                        descripcion = almacen.descripcion,
                        cod_alterno = almacen.cod_alterno,
                        estado = almacen.estado,
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
        public async Task<IHttpActionResult> Editar([FromBody] List<Almacen> almacenes, string db)
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

            var dbAccess = new SqlServerDataAccess(ConfigurationManager.ConnectionStrings[db].ConnectionString);

            foreach (var almacen in almacenes)
            {
                if (almacen?.IsNotInitialized == true)
                {
                    return new CustomJsonActionResult(
                        System.Net.HttpStatusCode.NotFound,
                        new JsonErrorResponse(1, 400, "Faltan parámetros"));
                }

                try
                {
                    var sql = ConfigurationManager.AppSettings["editar_almacen"];
                    var parameters = new
                    {
                        id = almacen.id,
                        descripcion = almacen.descripcion,
                        cod_alterno = almacen.cod_alterno,
                        estado = almacen.estado,
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
