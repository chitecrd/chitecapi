using chitecapi.Responses;
using DataAccess;
using DataAccess.Models;
using Newtonsoft.Json;
using Infrastructure.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

namespace chitecapi.Controllers
{
    public class Detalles_InventariosController : ChitecApiController
    {
        #region Actions

        // GET api/Detalles_Inventarios/cantidad_tiros_tipo_ubicacion_hora
        /// <summary>
        /// Retorna la lista / cantidad de tiros según tipo de ubicación y hora.
        /// </summary>
        /// <param name="db" example="db1">The Database ID</param>
        /// <remarks>
        /// Se utiliza para obtener la lista / cantidad de tiros según tipo de ubicación y hora, generado con un formato JSON:
        /// 
        ///     Example Result:
        ///     {
        ///         "nro_tiros": 5723,
        ///         "fecha_desde":"2021-05-03 12:11:58",
        ///         "fecha_hasta":"2021-05-03 23:11:58",
        ///         "cantidades_tiros":{"hora":"03\/05\/2021 12:00 PM","tienda":0,"almacen":0},{"hora":"03\/05\/2021 01:00 PM","tienda":0,"almacen":0}, 
        ///         "error": 0,
        ///         "error_type": 0,
        ///         "error_message": 0
        ///     }
        /// </remarks>
        /// <response code="401">Unauthorized. Error en la configuracion de la base de datos</response>              
        /// <response code="200">OK. Devuelve el objeto solicitado.</response>        
        /// <response code="404">NotFound. No se ha encontrado el objeto solicitado.</response>
        [HttpGet]
        public IHttpActionResult cantidad_tiros_tipo_ubicacion_hora([FromUri] string db = "db1",string fecha_desde="",string fecha_hasta="", string saveresult = "0", string tablename = "cantidadtirostest")
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


            fecha_desde = fecha_desde.Replace("%20", " ");
            fecha_desde = fecha_desde.Replace("%3A", ":");
            fecha_hasta = fecha_hasta.Replace("%20", " ");
            fecha_hasta = fecha_hasta.Replace("%3A", ":");

            DateTime inicio =DateTime.Parse(fecha_desde);
            DateTime final = DateTime.Parse(fecha_hasta);
            DateTime iniciocorrecto = inicio.Date;
            iniciocorrecto = iniciocorrecto.AddHours(inicio.Hour);
            DateTime finalcorrecto = final.Date;
            finalcorrecto = finalcorrecto.AddHours(final.Hour);
            inicio = iniciocorrecto;
            final = finalcorrecto;
            DateTime iniciomas = inicio.AddHours(inicio.Hour+1);
            string sql = "";
            Int64 totaltiro = 0;

            Dictionary<string, object> jsonvalues = new Dictionary<string, object>();

            List<Dictionary<string, object>> listacantidadtiros = new List<Dictionary<string, object>>();
            while (inicio<=final)
            {
                Dictionary<string, object> tupla = new Dictionary<string, object>();

                sql = "  select t.descripcion, "+
                      " COALESCE((SELECT COUNT(1) as cantidad from detalle_inventario d " +
                      "  left join ubicaciones u on d.id_ubicacion = u.id " +
                      "  left join tipo_ubicacion tu on u.id_tipo_ubicacion = tu.id " +
                      $"  where tu.id = t.id and d.fecha_registro BETWEEN '{inicio}' and '{iniciomas}' ),0) as total " +
                      " from tipo_ubicacion t";

            dataUtil.PrepareStatement(sql);

                DataTable table = new DataTable();
                dataUtil.FillDatatable(table);

                tupla.Add("hora", inicio.ToString());

                foreach (DataRow row in table.Rows)
                {
                    tupla.Add(row["descripcion"].ToString(), row["total"]);
                    totaltiro +=Int64.Parse(row["total"].ToString());
                }

                inicio = inicio.AddHours(1);
                iniciomas = inicio.AddHours(inicio.Hour + 1);
                listacantidadtiros.Add(tupla);
            }

            jsonvalues.Add("nro_tiros", totaltiro);
            jsonvalues.Add("fecha_desde", fecha_desde);
            jsonvalues.Add("fecha_hasta", fecha_hasta);
            jsonvalues.Add("cantidades_tiros", listacantidadtiros);

           


            

            dataUtil.CloseConnection();


            if (saveresult.Equals("1"))
            {
                dataUtil = new DataUtil("dbconnection");
                dataUtil.Connect();
                string jsonvalue = JsonConvert.SerializeObject(jsonvalues);
                sql = $"  exec createtablefromjson @tabla='{tablename}', @json='{jsonvalue}'; ";
                dataUtil.ExecuteCommand(sql);
                dataUtil.CloseConnection();
            }



            //jsonvalues.Add("nro_tiros", table.Rows[0][0]);
            jsonvalues.Add("error", 0);
            jsonvalues.Add("error_type", 0);
            jsonvalues.Add("error_message", 0);


            return Json(jsonvalues);
        }


        // GET api/Detalles_Inventarios/cantidad_tiros_tipo_ubicacion
        /// <summary>
        /// Retorna la lista / cantidad de tiros según tipo de ubicación.
        /// </summary>
        /// <param name="db" example="db1">The Database ID</param>
        /// <remarks>
        /// Se utiliza para obtener la lista / cantidad de tiros según tipo de ubicación, generado con un formato JSON:
        /// 
        ///     Example Result:
        ///     {
        ///         "tienda": 5723,
        ///         "almacen": 5723,
        ///         "fecha_desde":"2021-05-03 12:11:58",
        ///         "fecha_hasta":"2021-05-03 23:11:58",
        ///         "error": 0,
        ///         "error_type": 0,
        ///         "error_message": 0
        ///     }
        /// </remarks>
        /// <response code="401">Unauthorized. Error en la configuracion de la base de datos</response>              
        /// <response code="200">OK. Devuelve el objeto solicitado.</response>        
        /// <response code="404">NotFound. No se ha encontrado el objeto solicitado.</response>
        [HttpGet]
        public IHttpActionResult cantidad_tiros_tipo_ubicacion([FromUri] string db = "db1", string fecha_desde = "", string fecha_hasta = "", string saveresult = "0", string tablename = "cantidadtiroubitest")
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


            string sql = "";

            fecha_desde = fecha_desde.Replace("%20", " ");
            fecha_desde = fecha_desde.Replace("%3A", ":");
            fecha_hasta = fecha_hasta.Replace("%20", " ");
            fecha_hasta = fecha_hasta.Replace("%3A", ":");
            DateTime inicio = DateTime.Parse(fecha_desde);
            DateTime final = DateTime.Parse(fecha_hasta);

            Dictionary<string, object> jsonvalues = new Dictionary<string, object>();

         
            sql = "  select t.descripcion, " +
                    " COALESCE((SELECT COUNT(1) as cantidad from detalle_inventario d " +
                    "  left join ubicaciones u on d.id_ubicacion = u.id " +
                    "  left join tipo_ubicacion tu on u.id_tipo_ubicacion = tu.id " +
                    $"  where tu.id = t.id and d.fecha_registro BETWEEN '{inicio}' and '{final}' ),0) as total " +
                    " from tipo_ubicacion t";

            dataUtil.PrepareStatement(sql);

            DataTable table = new DataTable();
            dataUtil.FillDatatable(table);
    
            foreach (DataRow row in table.Rows)
            {
                jsonvalues.Add(row["descripcion"].ToString(), row["total"]);
            }


            jsonvalues.Add("fecha_desde", fecha_desde);
            jsonvalues.Add("fecha_hasta", fecha_hasta);






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



            //jsonvalues.Add("nro_tiros", table.Rows[0][0]);
            jsonvalues.Add("error", 0);
            jsonvalues.Add("error_type", 0);
            jsonvalues.Add("error_message", 0);


            return Json(jsonvalues);
        }


        /// <summary>
        /// Obtiene las estadísticas y articulos en almacén que, no están en tienda.
        /// </summary>
        /// <param name="db" example="db1">ID de la Base de datos</param>        
        /// <remarks>
        /// Se utiliza para obtener las estadísticas y artículos en almacén que, no están en tienda, generado con un formato JSON:
        /// 
        ///     Example Result:
        ///     {
        ///       "cantidad_articulos": 1088,
        ///       "cantidad_articulos_no_existen_tienda": 217,
        ///       "porciento_cantidad_articulos": 19.944853,
        ///       "costo_total": 7058721.0637,
        ///       "costo_total_no_existe_tienda": 888268.17,
        ///       "porciento_costo_total": 12.583982,
        ///       "precio_total": 11029851.93,
        ///       "precio_total_no_existe_tienda": 1390555.42,
        ///       "porciento_precio_total": 12.607199,
        ///       "data": [
        ///         {
        ///           "id": "2",
        ///           "id_terminal": "03-87C3312B890F2CC6-FFFFFFFF9B2812E4",
        ///           "no_detalleInv": "2",
        ///           "no_articulo": "853",
        ///           "codigo_barra": "7412100073243 ",
        ///           "alterno1": "",
        ///           "alterno2": "",
        ///           "alterno3": "",
        ///           "descripcion": "ZERO VODKA 700ML                        ",
        ///           "cantidad": "4",
        ///           "costo": "416.9449",
        ///           "costo_total": "1667.7796",
        ///           "precio": "640",
        ///           "precio_total": "2560",
        ///           "id_ubicacion": "8",
        ///           "cod_alterno": "A03B",
        ///           "fecha_registro": "05/02/2021 22:07:13",
        ///           "fecha_modificacion": "05/02/2021 22:07:13",
        ///           "id_usuario_registro": "3",
        ///           "id_usuario_modificacion": "3",
        ///           "id_auditor": "0",
        ///           "id_tipo_auditoria": "0",
        ///           "pre_conteo": "0",
        ///           "cantidad_auditada": "0",
        ///           "diferencia": "0",
        ///           "porcentaje_diferencia": "0",
        ///           "id_tipo_error": "0",
        ///           "notas": "",
        ///           "estado": "1",
        ///           "codigo_capturado": ""
        ///         }
        ///       ],
        ///       "error": 0,
        ///       "error_type": 0,
        ///       "error_message": 0
        ///     }
        /// </remarks>
        /// <response code="401">Unauthorized. Error en la configuracion de la base de datos</response>              
        /// <response code="200">OK. Devuelve el objeto solicitado.</response>        
        /// <response code="404">NotFound. No se ha encontrado el objeto solicitado.</response>
        [HttpGet]
        public async Task<IHttpActionResult> get_almacen_tienda(string db)
        {
            if (string.IsNullOrEmpty(db))
            {
                db = $"{ConfigurationManager.AppSettings["default_db"]}";
            }

            if (ConfigurationManager.ConnectionStrings[db] == null)
            {
                return new CustomJsonActionResult(
                    HttpStatusCode.NotFound,
                    new JsonErrorResponse(1, 400, $"La base de datos {db} no existe."));
            }

            var connectionString = GetConnectionString(db);
            try
            {
                using (var unitOfWork = new DetalleInventarioUnitOfWork(connectionString))
                {
                    var itemsNotInStore = await GetItemsNotInStore(unitOfWork);
                    var inventoryStatistics = await GetInventoryStatistics(unitOfWork, itemsNotInStore);
                    var prefixData = GetDataJson(inventoryStatistics);
                    var data = GetDataJson(itemsNotInStore);
                    return new CustomJsonActionResult(HttpStatusCode.OK, new JsonDataResponse(prefixData, data));
                }
            }
            catch (Exception exception)
            {
                return new CustomJsonActionResult(
                    HttpStatusCode.NotFound,
                    new JsonErrorResponse(1, 1, exception.GetBaseException().Message));
            }
        }

        // GET api/Detalles_Inventarios/nro_tiros
        /// <summary>
        /// Obtiene la cantidad total de tiros.
        /// </summary>
        /// <param name="db" example="db1">The Database ID</param>
        /// <remarks>
        /// Se utiliza para obtener la cantidad total de tiros realizados en un registro de inventario, generado con un formato JSON:
        /// 
        ///     Example Result:
        ///     {
        ///         "nro_tiros": 5723,
        ///         "error": 0,
        ///         "error_type": 0,
        ///         "error_message": 0
        ///     }
        /// </remarks>
        /// <response code="401">Unauthorized. Error en la configuracion de la base de datos</response>              
        /// <response code="200">OK. Devuelve el objeto solicitado.</response>        
        /// <response code="404">NotFound. No se ha encontrado el objeto solicitado.</response>
        [HttpGet]
        public IHttpActionResult nro_tiros([FromUri] string db = "db1", string saveresult = "0", string tablename = "notirostest")
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

            var sql = ConfigurationManager.AppSettings["getnumero_tiros"];

            if (sql == null)
            {
                sql = "SELECT COUNT(1) as nro_tiros FROM detalle_inventario";
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
            jsonvalues.Add("nro_tiros", table.Rows[0][0]);
            jsonvalues.Add("error", 0);
            jsonvalues.Add("error_type", 0);
            jsonvalues.Add("error_message", 0);


            return Json(jsonvalues);
        }


        // GET api/Detalles_Inventarios/get_nro_tiros_desde_hasta
        /// <summary>
        /// Obtiene la cantidad total de tiros desde un rango de fecha, este puede ser agrupado por usuario.
        /// </summary>
        /// <param name="db" example="db1">The Database ID</param>
        /// <remarks>
        /// Se utiliza para obtener la cantidad total de tiros desde un rango de fecha, este puede ser agrupado por usuario, generado con un formato JSON:
        /// 
        ///     Example Result:
        ///     {
        ///         "nro_tiros": 5723,
        ///         "error": 0,
        ///         "error_type": 0,
        ///         "error_message": 0
        ///     }
        /// </remarks>
        /// <response code="401">Unauthorized. Error en la configuracion de la base de datos</response>              
        /// <response code="200">OK. Devuelve el objeto solicitado.</response>        
        /// <response code="404">NotFound. No se ha encontrado el objeto solicitado.</response>
        [HttpGet]
        public IHttpActionResult get_nro_tiros_desde_hasta([FromUri] string db = "db1",string fecha_desde="",string fecha_hasta = "",string es_agrupar_por_usuario="0",string id_tipo_ubicacion="", string saveresult = "0", string tablename = "notirosdesdetest")
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

            string columna_usuario="";
            string condicion_id_tipo_ubicacion = "";
            string condicion_fechadesde = "";
            string condicion_fechahasta = "";
            string condicion_group_by_usuario = "";


            if (!fecha_desde.Equals(""))
                condicion_fechadesde = " AND d.fecha_registro >= '" + fecha_desde + "'";

            if (!fecha_hasta.Equals(""))
                condicion_fechahasta = " AND d.fecha_registro <= '" + fecha_hasta + "'";


            if (!id_tipo_ubicacion.Equals(""))
                condicion_id_tipo_ubicacion = " AND u.id_tipo_ubicacion = " + id_tipo_ubicacion;

            if (es_agrupar_por_usuario.Equals("1"))
            {
                columna_usuario = ",d.id_usuario_registro ";
                condicion_group_by_usuario = " GROUP BY d.id_usuario_registro";

            }


            var sql = $"SELECT COUNT(1) as nro_tiros {columna_usuario} "+
                      " FROM detalle_inventario d " +
                      " left join ubicaciones u " +
                      " on(d.id_ubicacion = u.id) " +
                      $" WHERE 1=1 {condicion_fechadesde} {condicion_fechahasta} {condicion_id_tipo_ubicacion} {condicion_group_by_usuario} ";



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

            if (es_agrupar_por_usuario.Equals("1"))
            {
                jsonvalues.Add("datos", table);
                jsonvalues.Add("error", 0);
                jsonvalues.Add("error_type", 0);
                jsonvalues.Add("error_message", 0);

            }
            else
            {
                Int64 valor = 0;
                if (table.Rows[0][0] != null)
                {
                    valor = Int64.Parse(table.Rows[0][0].ToString());
                }
                jsonvalues.Add("nro_tiros", valor);
                jsonvalues.Add("error", 0);
                jsonvalues.Add("error_type", 0);
                jsonvalues.Add("error_message", 0);
            }

            


            return Json(jsonvalues);
        }

        // GET api/Detalles_Inventarios/costo_total_inventario
        /// <summary>
        /// Obtiene el costo total del inventario.
        /// </summary>
        /// <param name="db" example="db1">The Database ID</param>
        /// <remarks>
        /// Se utiliza para obtener el Costo total del inventario registrado, generado con un formato JSON:
        /// 
        ///     Example Result:
        ///     {
        ///         "costo_total": 5723,
        ///         "error": 0,
        ///         "error_type": 0,
        ///         "error_message": 0
        ///     }
        /// </remarks>
        /// <response code="401">Unauthorized. Error en la configuracion de la base de datos</response>              
        /// <response code="200">OK. Devuelve el objeto solicitado.</response>        
        /// <response code="404">NotFound. No se ha encontrado el objeto solicitado.</response>
        [HttpGet]
        public IHttpActionResult costo_total_inventario([FromUri] string db = "db1", string saveresult = "0", string tablename = "costototaltest")
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

            var sql = ConfigurationManager.AppSettings["getcostototal_inventario"];

            if (sql == null)
            {
                sql = "SELECT SUM(costo_total) as costo_total FROM detalle_inventario";
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
            jsonvalues.Add("costo_total", table.Rows[0][0]);
            jsonvalues.Add("error", 0);
            jsonvalues.Add("error_type", 0);
            jsonvalues.Add("error_message", 0);


            return Json(jsonvalues);
        }

        // GET api/Detalles_Inventarios/precio_total_inventario
        /// <summary>
        /// Obtiene el precio total del inventario.
        /// </summary>
        /// <param name="db" example="db1">The Database ID</param>
        /// <remarks>
        /// Se utiliza para obtener el precio total del inventario registrado, generado con un formato JSON:
        /// 
        ///     Example Result:
        ///     {
        ///         "precio_total": 5723,
        ///         "error": 0,
        ///         "error_type": 0,
        ///         "error_message": 0
        ///     }
        /// </remarks>
        /// <response code="401">Unauthorized. Error en la configuracion de la base de datos</response>              
        /// <response code="200">OK. Devuelve el objeto solicitado.</response>        
        /// <response code="404">NotFound. No se ha encontrado el objeto solicitado.</response>
        [HttpGet]
        public IHttpActionResult precio_total_inventario([FromUri] string db = "db1", string saveresult = "0", string tablename = "preciototaltest")
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

            var sql = ConfigurationManager.AppSettings["getpreciototal_inventario"];

            if (sql == null)
            {
                sql = "SELECT SUM(precio_total) as precio_total FROM detalle_inventario";
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
            jsonvalues.Add("precio_total", table.Rows[0][0]);
            jsonvalues.Add("error", 0);
            jsonvalues.Add("error_type", 0);
            jsonvalues.Add("error_message", 0);


            return Json(jsonvalues);
        }

        // GET api/Detalles_Inventarios/ListaDetalleInventario?db=db1
        /// <summary>
        /// Obtiene una lista del detalle de inventario con cada articulo.
        /// </summary>
        /// <param name="db" example="db1">The Database ID</param>
        /// <param name="es_cantidad_igual_tiros" example="111">Cantidad igual a Tiros</param>
        /// <param name="es_cantidad_igual_no_articulo" example="111">Cantidad igual No Articulo</param>
        /// <param name="precio_total_desde" example="111">Precio Total Desde</param>
        /// <param name="precio_total_hasta" example="111">Precio Total Hasta</param>
        /// <param name="costo_total_desde" example="111">Costo total Desde</param>
        /// <param name="costo_total_hasta" example="Coca cola">Costo total hasta</param>
        /// <param name="fecha_desde" example="Coca cola">Fecha Desde</param>
        /// <param name="fecha_hasta" example="Coca cola">fecha Hasta</param>
        /// <param name="id_tipo_ubicacion" example="Coca cola">Tipo Ubicacion</param>
        /// <remarks>
        /// Se utiliza para obtener una lista de los articulos en el detalle de inventario, generado con un formato JSON:
        /// 
        ///     Example Result:
        ///     {
        ///         "data": [
        ///                   {
        ///                       id": 1,
        ///                       "id_terminal": "0344970B167E4B43B7FFFFFFFFB7F55748",
        ///                       "no_detalleInv": "4",
        ///                       "no_articulo": "13523",
        ///                       "codigo_barra": "74673643",
        ///                       "alterno1": "01010010",
        ///                       "alterno2": "",
        ///                       "alterno3": "",
        ///                       "descripcion": "BRUGAL BLANCO 350 CC",
        ///                       "cantidad": 100,
        ///                       "costo": 169.16,
        ///                       "costo_total": 16916,
        ///                       "precio": 279.95,
        ///                       "precio_total": 27995,
        ///                       "id_ubicacion": 1,
        ///                       "cod_alterno": "G01",
        ///                       "fecha_registro": "2020-07-06 20:09:26",
        ///                       "fecha_modificacion": "2020-07-06 20:09:26",
        ///                       "id_usuario_registro": 1,
        ///                       "id_usuario_modificacion": 1,
        ///                       "id_auditor": 1,
        ///                       "id_tipo_auditoria": 0,
        ///                       "pre_conteo": 0,
        ///                       "cantidad_auditada": 0,
        ///                       "diferencia": 0,
        ///                       "porcentaje_diferencia": 0,
        ///                       "id_tipo_error": 0,
        ///                       "notas": "",
        ///                       "estado": 1,
        ///                       "codigo_capturado": "",
        ///                       "created_at": "2020-07-06 20:09:39",
        ///                       "updated_at": "2020-07-06 20:09:39",
        ///                       "es_cantidad_igual_tiros": 0,
        ///                       "tipo_ubicacion": "tienda"
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
        public IHttpActionResult ListaDetalleInventario([FromUri] string db = "db1", string es_cantidad_igual_tiros = "", string es_cantidad_igual_no_articulo = "", string precio_total_desde = "",
            string precio_total_hasta = "", string costo_total_desde = "", string costo_total_hasta = "", string fecha_desde = "", string fecha_hasta = "", string id_tipo_ubicacion = "", string saveresult = "0", string tablename = "codigobarratest")
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

            var sql = ConfigurationManager.AppSettings["getdetalleinventario"];

            if (sql == null)
            {
                sql = "SELECT    d.*, 0 as es_cantidad_igual_tiros, coalesce(tu.descripcion, '') as tipo_ubicacion    FROM detalle_inventario d " +
                               " left join ubicaciones u " +
                               " on(d.id_ubicacion = u.id) " +
                               " left join tipo_ubicacion tu " +
                               " on(u.id_tipo_ubicacion = tu.id) " +
                               " WHERE 1 = 1";
            }


            if (es_cantidad_igual_tiros.Equals("1"))
                sql = ConfigurationManager.AppSettings["getdetalleinventariocantidadigualatiro"]; ;


            if (es_cantidad_igual_no_articulo.Equals("1"))
                sql = sql + " AND d.no_articulo = d.cantidad ";


            if (!precio_total_desde.Equals(""))
                sql = sql + " AND d.precio_total >= " + precio_total_desde;

            if (!precio_total_hasta.Equals(""))
                sql = sql + " AND d.precio_total <= " + precio_total_hasta;

            if (!costo_total_desde.Equals(""))
                sql = sql + " AND d.costo_total >= " + costo_total_desde;

            if (!costo_total_hasta.Equals(""))
                sql = sql + " AND d.costo_total <= " + costo_total_hasta;

            if (!fecha_desde.Equals(""))
                sql = sql + " AND d.fecha_registro >= '" + fecha_desde + "'";

            if (!fecha_hasta.Equals(""))
                sql = sql + " AND d.fecha_registro <= '" + fecha_hasta + "'";

            if (!id_tipo_ubicacion.Equals(""))
                sql = sql + " AND u.id_tipo_ubicacion = " + id_tipo_ubicacion;

            sql = sql + "      order by d.id";

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

        // GET api/Detalles_Inventarios/ListaDetalleInventarioLog?db=db1
        /// <summary>
        /// Obtiene una lista del detalle de inventario que esta pendiente de registrarse con cada articulo.
        /// </summary>
        /// <param name="db" example="db1">The Database ID</param>
        /// <param name="es_cantidad_igual_tiros" example="111">Cantidad igual a Tiros</param>
        /// <param name="es_cantidad_igual_no_articulo" example="111">Cantidad igual No Articulo</param>
        /// <param name="precio_total_desde" example="111">Precio Total Desde</param>
        /// <param name="precio_total_hasta" example="111">Precio Total Hasta</param>
        /// <param name="costo_total_desde" example="111">Costo total Desde</param>
        /// <param name="costo_total_hasta" example="Coca cola">Costo total hasta</param>
        /// <param name="fecha_desde" example="Coca cola">Fecha Desde</param>
        /// <param name="fecha_hasta" example="Coca cola">fecha Hasta</param>
        /// <param name="id_tipo_ubicacion" example="Coca cola">Tipo Ubicacion</param>
        /// <remarks>
        /// Se utiliza para obtener una lista de los articulos en el detalle de inventario, generado con un formato JSON:
        /// 
        ///     Example Result:
        ///     {
        ///         "data": [
        ///                   {
        ///                       id": 1,
        ///                       "id_terminal": "0344970B167E4B43B7FFFFFFFFB7F55748",
        ///                       "no_detalleInv": "4",
        ///                       "no_articulo": "13523",
        ///                       "codigo_barra": "74673643",
        ///                       "alterno1": "01010010",
        ///                       "alterno2": "",
        ///                       "alterno3": "",
        ///                       "descripcion": "BRUGAL BLANCO 350 CC",
        ///                       "cantidad": 100,
        ///                       "costo": 169.16,
        ///                       "costo_total": 16916,
        ///                       "precio": 279.95,
        ///                       "precio_total": 27995,
        ///                       "id_ubicacion": 1,
        ///                       "cod_alterno": "G01",
        ///                       "fecha_registro": "2020-07-06 20:09:26",
        ///                       "fecha_modificacion": "2020-07-06 20:09:26",
        ///                       "id_usuario_registro": 1,
        ///                       "id_usuario_modificacion": 1,
        ///                       "id_auditor": 1,
        ///                       "id_tipo_auditoria": 0,
        ///                       "pre_conteo": 0,
        ///                       "cantidad_auditada": 0,
        ///                       "diferencia": 0,
        ///                       "porcentaje_diferencia": 0,
        ///                       "id_tipo_error": 0,
        ///                       "notas": "",
        ///                       "estado": 1,
        ///                       "codigo_capturado": "",
        ///                       "created_at": "2020-07-06 20:09:39",
        ///                       "updated_at": "2020-07-06 20:09:39",
        ///                       "es_cantidad_igual_tiros": 0,
        ///                       "tipo_ubicacion": "tienda"
        ///                       "generado": "false"
        ///                       "ultimoerror": "--"
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
        public IHttpActionResult ListaDetalleInventarioLog([FromUri] string db = "db1", string es_cantidad_igual_no_articulo = "", string precio_total_desde = "",
            string precio_total_hasta = "", string costo_total_desde = "", string costo_total_hasta = "", string fecha_desde = "", string fecha_hasta = "", string id_tipo_ubicacion = "", string saveresult = "0", string tablename = "codigobarratest")
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

            var sql = ConfigurationManager.AppSettings["buscar_detalle_inventario_transitorio"];

            if (sql == null)
            {
                sql = "SELECT    d.*, 0 as es_cantidad_igual_tiros, coalesce(tu.descripcion, '') as tipo_ubicacion    FROM detalle_inventario d " +
                               " left join ubicaciones u " +
                               " on(d.id_ubicacion = u.id) " +
                               " left join tipo_ubicacion tu " +
                               " on(u.id_tipo_ubicacion = tu.id) " +
                               " WHERE 1 = 1";
            }


            if (es_cantidad_igual_no_articulo.Equals("1"))
                sql = sql + " AND d.no_articulo = d.cantidad ";


            if (!precio_total_desde.Equals(""))
                sql = sql + " AND d.precio_total >= " + precio_total_desde;

            if (!precio_total_hasta.Equals(""))
                sql = sql + " AND d.precio_total <= " + precio_total_hasta;

            if (!costo_total_desde.Equals(""))
                sql = sql + " AND d.costo_total >= " + costo_total_desde;

            if (!costo_total_hasta.Equals(""))
                sql = sql + " AND d.costo_total <= " + costo_total_hasta;

            if (!fecha_desde.Equals(""))
                sql = sql + " AND d.fecha_registro >= '" + fecha_desde + "'";

            if (!fecha_hasta.Equals(""))
                sql = sql + " AND d.fecha_registro <= '" + fecha_hasta + "'";

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

        // GET api/Detalles_Inventarios/duplicados_nro_articulos?db=db1
        /// <summary>
        /// Obtiene una lista del detalle de inventario con los articulos duplicados.
        /// </summary>
        /// <param name="db" example="db1">The Database ID</param>
        /// <param name="id_tipo_ubicacion" example="Coca cola">Tipo Ubicacion</param>
        /// <remarks>
        /// Se utiliza para obtener una lista de los articulos duplicados en el detalle de inventario, generado con un formato JSON:
        /// 
        ///     Example Result:
        ///     {
        ///         "data": [
        ///                   {
        ///                       id": 1,
        ///                       "id_terminal": "0344970B167E4B43B7FFFFFFFFB7F55748",
        ///                       "no_detalleInv": "4",
        ///                       "no_articulo": "13523",
        ///                       "codigo_barra": "74673643",
        ///                       "alterno1": "01010010",
        ///                       "alterno2": "",
        ///                       "alterno3": "",
        ///                       "descripcion": "BRUGAL BLANCO 350 CC",
        ///                       "cantidad": 100,
        ///                       "costo": 169.16,
        ///                       "costo_total": 16916,
        ///                       "precio": 279.95,
        ///                       "precio_total": 27995,
        ///                       "id_ubicacion": 1,
        ///                       "cod_alterno": "G01",
        ///                       "fecha_registro": "2020-07-06 20:09:26",
        ///                       "fecha_modificacion": "2020-07-06 20:09:26",
        ///                       "id_usuario_registro": 1,
        ///                       "id_usuario_modificacion": 1,
        ///                       "id_auditor": 1,
        ///                       "id_tipo_auditoria": 0,
        ///                       "pre_conteo": 0,
        ///                       "cantidad_auditada": 0,
        ///                       "diferencia": 0,
        ///                       "porcentaje_diferencia": 0,
        ///                       "id_tipo_error": 0,
        ///                       "notas": "",
        ///                       "estado": 1,
        ///                       "codigo_capturado": "",
        ///                       "created_at": "2020-07-06 20:09:39",
        ///                       "updated_at": "2020-07-06 20:09:39",
        ///                       "es_cantidad_igual_tiros": 0,
        ///                       "tipo_ubicacion": "tienda"
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
        public IHttpActionResult duplicados_nro_articulos([FromUri] string db = "db1", string id_tipo_ubicacion = "", string saveresult = "0", string tablename = "codigobarratest")
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

            var sql = ConfigurationManager.AppSettings["getdetalleinventarionoarticuloduplicado"];

            if (sql == null)
            {
                sql = "SELECT    d.*, 0 as es_cantidad_igual_tiros, coalesce(tu.descripcion, '') as tipo_ubicacion    FROM detalle_inventario d " +
                               " left join ubicaciones u " +
                               " on(d.id_ubicacion = u.id) " +
                               " left join tipo_ubicacion tu " +
                               " on(u.id_tipo_ubicacion = tu.id) " +
                               " WHERE d.no_articulo IN (SELECT d2.no_articulo FROM detalle_inventario d2 GROUP BY d2.no_articulo, d2.cantidad " +
                               " having count(*) >= 2) ";
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

        // GET api/Detalles_Inventarios/auditar?db=db1
        /// <summary>
        /// Obtiene una lista del detalle de inventario con filtros de auditoria.
        /// </summary>
        /// <param name="db" example="db1">The Database ID</param>
        /// <param name="id_tipo_ubicacion" example="Coca cola">Tipo Ubicacion</param>
        /// <param name="id_auditor" example="Coca cola">Id Auditor</param>
        /// <param name="id_tipo_auditoria" example="Coca cola">Tipo Auditoria</param>
        /// <remarks>
        /// Se utiliza para obtener una lista de los articulos en el detalle de inventario mediante filtros de auditoria, generado con un formato JSON:
        /// 
        ///     Example Result:
        ///     {
        ///         "data": [
        ///                   {
        ///                       id": 1,
        ///                       "id_terminal": "0344970B167E4B43B7FFFFFFFFB7F55748",
        ///                       "no_detalleInv": "4",
        ///                       "no_articulo": "13523",
        ///                       "codigo_barra": "74673643",
        ///                       "alterno1": "01010010",
        ///                       "alterno2": "",
        ///                       "alterno3": "",
        ///                       "descripcion": "BRUGAL BLANCO 350 CC",
        ///                       "cantidad": 100,
        ///                       "costo": 169.16,
        ///                       "costo_total": 16916,
        ///                       "precio": 279.95,
        ///                       "precio_total": 27995,
        ///                       "id_ubicacion": 1,
        ///                       "cod_alterno": "G01",
        ///                       "fecha_registro": "2020-07-06 20:09:26",
        ///                       "fecha_modificacion": "2020-07-06 20:09:26",
        ///                       "id_usuario_registro": 1,
        ///                       "id_usuario_modificacion": 1,
        ///                       "id_auditor": 1,
        ///                       "id_tipo_auditoria": 0,
        ///                       "pre_conteo": 0,
        ///                       "cantidad_auditada": 0,
        ///                       "diferencia": 0,
        ///                       "porcentaje_diferencia": 0,
        ///                       "id_tipo_error": 0,
        ///                       "notas": "",
        ///                       "estado": 1,
        ///                       "codigo_capturado": "",
        ///                       "created_at": "2020-07-06 20:09:39",
        ///                       "updated_at": "2020-07-06 20:09:39",
        ///                       "es_cantidad_igual_tiros": 0,
        ///                       "tipo_ubicacion": "tienda"
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
        public IHttpActionResult auditar([FromUri] string db = "db1", string id_tipo_ubicacion = "", string id_auditor = "", string id_tipo_auditoria = "", string saveresult = "0", string tablename = "codigobarratest")
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

            var sql = ConfigurationManager.AppSettings["getdetalleinventario"];

            if (sql == null)
            {
                sql = "SELECT    d.*, 0 as es_cantidad_igual_tiros, coalesce(tu.descripcion, '') as tipo_ubicacion    FROM detalle_inventario d " +
                               " left join ubicaciones u " +
                               " on(d.id_ubicacion = u.id) " +
                               " left join tipo_ubicacion tu " +
                               " on(u.id_tipo_ubicacion = tu.id) " +
                               " WHERE 1 = 1";
            }



            if (!id_tipo_ubicacion.Equals(""))
                sql = sql + " AND u.id_tipo_ubicacion = " + id_tipo_ubicacion;



            if (!id_auditor.Equals(""))
                sql = sql + " AND d.id_auditor = " + id_auditor;



            if (!id_tipo_auditoria.Equals(""))
                sql = sql + " AND d.id_tipo_auditoria = " + id_tipo_auditoria;



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

        /// <summary>
        /// Guardar Detalle Inventario.
        /// </summary>
        /// <param name="db" example="db1">The Database ID</param>
        /// <remarks>
        /// Se utiliza para Guardar multiples Entradas de inventario, Enviando un json con la información de la captura
        /// </remarks>
        /// <response code="401">Unauthorized. Error en la configuracion de la base de datos</response>              
        /// <response code="200">OK. Devuelve el objeto solicitado.</response>        
        /// <response code="404">NotFound. No se ha encontrado el objeto solicitado.</response>
        [HttpPost]
        public async Task<IHttpActionResult> Guardar([FromBody] List<DetalleInventario> detallesInventario, string db, string saveresult = "0", string tablename = "codigobarratest")
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

            foreach (var partialDetalleInventario in detallesInventario)
            {
                if (partialDetalleInventario?.IsNotInitialized == true)
                {
                    return new CustomJsonActionResult(
                        System.Net.HttpStatusCode.NotFound,
                        new JsonErrorResponse(1, 400, "Faltan parámetros"));
                }

                try
                {
                    var articulo = await GetArticulo(partialDetalleInventario.no_articulo, dbAccess);

                    if (articulo == null)
                    {
                        await InsertDetalleInventarioTransitorioSinArticulo(partialDetalleInventario, dbAccess);
                        return new CustomJsonActionResult(
                            System.Net.HttpStatusCode.NotFound,
                            new JsonErrorResponse(1, 1, $"El artículo con el código  {partialDetalleInventario.no_articulo} no existe en la base de datos."));
                    }

                    var codigoAlterno =
                        await GetCodigoAlterno(partialDetalleInventario.id_ubicacion, dbAccess);



                    await InsertDetalleInventarioTransitorio(partialDetalleInventario, dbAccess);

                    var fullDetalleInventario = SetFullDetalleInventario(partialDetalleInventario, articulo, codigoAlterno);


                    if (await IsUniqueDetalleInventario(
                        fullDetalleInventario.id_terminal,
                        fullDetalleInventario.no_detalleInv,
                        dbAccess))
                    {
                        await InsertDetalleInventario(fullDetalleInventario, dbAccess);
                    }
                    else
                    {
                        await EditDuplicatedDetalleInventario(fullDetalleInventario, dbAccess);
                    }
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

        /// <summary>
        /// Editar Detalle Inventario.
        /// </summary>
        /// <param name="db" example="db1">The Database ID</param>
        /// <remarks>
        /// Se utiliza para Editar multiples Entradas de inventario, Enviando un json con la información de la captura
        /// </remarks>
        /// <response code="401">Unauthorized. Error en la configuracion de la base de datos</response>              
        /// <response code="200">OK. Devuelve el objeto solicitado.</response>        
        /// <response code="404">NotFound. No se ha encontrado el objeto solicitado.</response>
        [HttpPost]
        public async Task<IHttpActionResult> Editar([FromBody] List<DetalleInventario> detallesInventario, string db)
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

            foreach (var partialDetalleInventario in detallesInventario)
            {
                if (partialDetalleInventario?.IsNotInitialized == true)
                {
                    return new CustomJsonActionResult(
                        System.Net.HttpStatusCode.NotFound,
                        new JsonErrorResponse(1, 400, "Faltan parámetros"));
                }

                try
                {
                    var articulo = await GetArticulo(partialDetalleInventario.no_articulo, dbAccess);

                    if (articulo == null)
                    {
                        return new CustomJsonActionResult(
                            System.Net.HttpStatusCode.NotFound,
                            new JsonErrorResponse(1, 1, $"El artículo con el código  {partialDetalleInventario.no_articulo} no existe en la base de datos."));
                    }

                    var codigoAlterno =
                        await GetCodigoAlterno(partialDetalleInventario.id_ubicacion, dbAccess);

                    var fullDetalleInventario = SetFullDetalleInventario(partialDetalleInventario, articulo, codigoAlterno);

                    var parameters = new
                    {
                        id_terminal = fullDetalleInventario.id_terminal,
                        no_detalleInv = fullDetalleInventario.no_detalleInv,
                        no_articulo = fullDetalleInventario.no_articulo,
                        codigo_barra = fullDetalleInventario.codigo_barra,
                        alterno1 = fullDetalleInventario.alterno1,
                        alterno2 = fullDetalleInventario.alterno2,
                        alterno3 = fullDetalleInventario.alterno3,
                        descripcion = fullDetalleInventario.descripcion,
                        cantidad = fullDetalleInventario.cantidad,
                        costo = fullDetalleInventario.costo,
                        costo_total = fullDetalleInventario.costo_total,
                        precio = fullDetalleInventario.precio,
                        precio_total = fullDetalleInventario.precio_total,
                        id_ubicacion = fullDetalleInventario.id_ubicacion,
                        id_almacen = fullDetalleInventario.id_almacen,
                        cod_alterno = fullDetalleInventario.cod_alterno,
                        fecha_registro = fullDetalleInventario.fecha_registro,
                        fecha_modificacion = fullDetalleInventario.fecha_modificacion,
                        id_usuario_registro = fullDetalleInventario.id_usuario_registro,
                        id_usuario_modificacion = fullDetalleInventario.id_usuario_modificacion,
                        estado = 1,
                        updated_at = DateTime.Now
                    };
                    var sql = ConfigurationManager.AppSettings["editar_detalle_inventario"];

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

        /// <summary>
        /// Editar Cantidad en detalle de Inventario.
        /// </summary>
        /// <param name="db" example="db1">The Database ID</param>
        /// <remarks>
        /// Se utiliza para Editar la cantidad de las multiples Entradas de inventario, Enviando un json con la información de la captura
        /// </remarks>
        /// <response code="401">Unauthorized. Error en la configuracion de la base de datos</response>              
        /// <response code="200">OK. Devuelve el objeto solicitado.</response>        
        /// <response code="404">NotFound. No se ha encontrado el objeto solicitado.</response>
        [HttpPost]
        public async Task<IHttpActionResult> Editar_Cantidad([FromBody] List<DetalleInventario> detallesInventario, string db)
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

            foreach (var partialDetalleInventario in detallesInventario)
            {
                if (partialDetalleInventario?.IsNotInitialized == true)
                {
                    return new CustomJsonActionResult(
                        System.Net.HttpStatusCode.NotFound,
                        new JsonErrorResponse(1, 400, "Faltan parámetros"));
                }

                if (partialDetalleInventario.id_terminal == null
                    || partialDetalleInventario.no_detalleInv == null
                    || partialDetalleInventario.cantidad == null)
                {
                    return new CustomJsonActionResult(
                        System.Net.HttpStatusCode.OK,
                        new JsonErrorResponse(1, 400, $"Faltan parámetros (id_terminal, no_detalleInv, cantidad)"));
                }

                try
                {
                    var detalleInventarioParameters = new
                    {
                        id_terminal = partialDetalleInventario.id_terminal,
                        no_detalleInv = partialDetalleInventario.no_detalleInv
                    };
                    var sql = ConfigurationManager.AppSettings["buscar_detalle_inventario_por_terminal_y_numero"];
                    var articuloNumber =
                        await GetArticuloNumber(sql, detalleInventarioParameters, dbAccess);

                    var articulo = await GetArticulo(articuloNumber, dbAccess);
                    if (articulo == null)
                    {
                        return new CustomJsonActionResult(
                            System.Net.HttpStatusCode.NotFound,
                            new JsonErrorResponse(1, 400, $"No existe un artículo relacionado al número de detalle de inventario: {partialDetalleInventario.no_detalleInv}."));
                    }

                    var detalleInventarioForEditCantidad =
                        SetDetalleInventarioForEdit(partialDetalleInventario, articulo);

                    if (!string.IsNullOrEmpty(partialDetalleInventario.id_ubicacion))
                    {
                        detalleInventarioForEditCantidad.cod_alterno = await GetCodigoAlterno(detalleInventarioForEditCantidad.id_ubicacion, dbAccess);

                        await EditCantidadWithUbicacion(detalleInventarioForEditCantidad, dbAccess);
                    }
                    else
                    {
                        await EditCantidadWithoutUbicacion(detalleInventarioForEditCantidad, dbAccess);
                    }
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

        /// <summary>
        /// Editar Informacion Auditoria en detalle de Inventario.
        /// </summary>
        /// <param name="db" example="db1">The Database ID</param>
        /// <remarks>
        /// Se utiliza para Editar la informacion de la auditoria en las multiples Entradas de inventario, Enviando un json con la información de la captura
        /// </remarks>
        /// <response code="401">Unauthorized. Error en la configuracion de la base de datos</response>              
        /// <response code="200">OK. Devuelve el objeto solicitado.</response>        
        /// <response code="404">NotFound. No se ha encontrado el objeto solicitado.</response>
        [HttpPost]
        public async Task<IHttpActionResult> Editar_Info_Auditoria([FromBody] List<DetalleInventario> detallesInventario, string db)
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

            foreach (var partialDetalleInventario in detallesInventario)
            {
                if (partialDetalleInventario?.IsNotInitialized == true)
                {
                    return new CustomJsonActionResult(
                        System.Net.HttpStatusCode.NotFound,
                        new JsonErrorResponse(1, 400, "Faltan parámetros"));
                }

                if (partialDetalleInventario.id == null
                    || partialDetalleInventario.pre_conteo == null
                    || partialDetalleInventario.diferencia == null
                    || partialDetalleInventario.cantidad_auditada == null
                    || partialDetalleInventario.porcentaje_diferencia == null
                    || partialDetalleInventario.id_tipo_error == null
                    || partialDetalleInventario.notas == null
                    || partialDetalleInventario.id_tipo_auditoria == null)
                {
                    return new CustomJsonActionResult(
                        System.Net.HttpStatusCode.OK,
                        new JsonErrorResponse(1, 400, $"Faltan parámetros (id, pre_conteo, cantidad_auditada, diferencia, porcentaje_diferencia, id_tipo_error, notas)"));
                }

                try
                {
                    var sql = ConfigurationManager.AppSettings["buscar_detalle_inventario_por_id"];
                    var articuloNumber =
                        await GetArticuloNumber(
                            sql, new { id = partialDetalleInventario.id }, dbAccess);

                    var articulo = await GetArticulo(articuloNumber, dbAccess);
                    if (articulo == null)
                    {
                        return new CustomJsonActionResult(
                            System.Net.HttpStatusCode.NotFound,
                            new JsonErrorResponse(1, 400, $"No existe un artículo relacionado al número de detalle de inventario: {partialDetalleInventario.no_detalleInv}."));
                    }

                    const bool isAudit = true;
                    var detalleInventarioForEdit =
                        SetDetalleInventarioForEdit(partialDetalleInventario, articulo, isAudit);

                    await EditAuditData(detalleInventarioForEdit, dbAccess);
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

        /// <summary>
        /// Editar Informacion Auditoria en detalle de Inventario (Manual).
        /// </summary>
        /// <param name="db" example="db1">The Database ID</param>
        /// <remarks>
        /// Se utiliza para Editar la informacion de la auditoria en las multiples Entradas de inventario (Manual), Enviando un json con la información de la captura
        /// </remarks>
        /// <response code="401">Unauthorized. Error en la configuracion de la base de datos</response>              
        /// <response code="200">OK. Devuelve el objeto solicitado.</response>        
        /// <response code="404">NotFound. No se ha encontrado el objeto solicitado.</response>
        [HttpPost]
        public async Task<IHttpActionResult> Editar_Info_Auditoria_Manual([FromBody] List<DetalleInventario> detallesInventario, string db)
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

            foreach (var partialDetalleInventario in detallesInventario)
            {
                if (partialDetalleInventario?.IsNotInitialized == true)
                {
                    return new CustomJsonActionResult(
                        System.Net.HttpStatusCode.NotFound,
                        new JsonErrorResponse(1, 400, "Faltan parámetros"));
                }

                if (partialDetalleInventario.id == null
                    || partialDetalleInventario.id_auditor == null
                    || partialDetalleInventario.id_tipo_auditoria == null)
                {
                    return new CustomJsonActionResult(
                        System.Net.HttpStatusCode.OK,
                        new JsonErrorResponse(1, 400, $"Faltan parámetros (id, id_auditor, id_tipo_auditoria)"));
                }

                try
                {
                    var sql = ConfigurationManager.AppSettings["editar_detalle_inventario_auditoria_manual"];
                    var parameters = new
                    {
                        id = partialDetalleInventario.id,
                        id_auditor = partialDetalleInventario.id_auditor,
                        id_tipo_auditoria = partialDetalleInventario.id_tipo_auditoria
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

        private decimal RoundDecimalPlaces(decimal value, int decimalPlaces)
        {
            return decimal.Round(value, decimalPlaces, MidpointRounding.AwayFromZero);
        }

        private async Task<bool> IsUniqueDetalleInventario(
            string terminalId, string detalleInventarioNumber, SqlServerDataAccess dbAccess)
        {
            var parameters = new { id_terminal = terminalId, no_detalleInv = detalleInventarioNumber };
            var sql =
                ConfigurationManager.AppSettings["buscar_detalle_inventario_por_terminal_y_numero"];

            var duplicateData =
                await dbAccess.LoadFirstOrDefaultAsync<DetalleInventario, dynamic>(sql, parameters);

            return duplicateData == null;
        }

        private async Task EditAuditData(DetalleInventario detalleInventarioForEdit, SqlServerDataAccess dbAccess)
        {
            var sql = ConfigurationManager.AppSettings["editar_detalle_inventario_auditoria"];
            var parameters = new
            {
                id = detalleInventarioForEdit.id,
                pre_conteo = detalleInventarioForEdit.pre_conteo,
                cantidad = detalleInventarioForEdit.cantidad_auditada,
                cantidad_auditada = detalleInventarioForEdit.cantidad_auditada,
                diferencia = detalleInventarioForEdit.diferencia,
                porcentaje_diferencia = detalleInventarioForEdit.porcentaje_diferencia,
                id_tipo_error = detalleInventarioForEdit.id_tipo_error,
                notas = detalleInventarioForEdit.notas,
                id_tipo_auditoria = detalleInventarioForEdit.id_tipo_auditoria,
                costo_total = detalleInventarioForEdit.costo_total,
                precio_total = detalleInventarioForEdit.precio_total,
                costo = detalleInventarioForEdit.costo,
                precio = detalleInventarioForEdit.precio,
            };

            await dbAccess.SaveDataAsync(sql, parameters);
        }

        private async Task EditCantidadWithoutUbicacion(DetalleInventario detalleInventario, SqlServerDataAccess dbAccess)
        {
            var sql =
                ConfigurationManager.AppSettings["editar_cantidad_detalle_inventario_sin_ubicacion"];
            var parameters = new
            {
                id_terminal = detalleInventario.id_terminal,
                no_detalleInv = detalleInventario.no_detalleInv,
                cantidad = detalleInventario.cantidad,
                costo = detalleInventario.costo,
                costo_total = detalleInventario.costo_total,
                precio = detalleInventario.precio,
                id_almacen = detalleInventario.id_almacen,
                precio_total = detalleInventario.precio_total
            };

            await dbAccess.SaveDataAsync(sql, parameters);
        }

        private async Task EditCantidadWithUbicacion(DetalleInventario detalleInventario, SqlServerDataAccess dbAccess)
        {
            var parameters = new
            {
                id_terminal = detalleInventario.id_terminal,
                no_detalleInv = detalleInventario.no_detalleInv,
                cantidad = detalleInventario.cantidad,
                costo = detalleInventario.costo,
                costo_total = detalleInventario.costo_total,
                precio = detalleInventario.precio,
                precio_total = detalleInventario.precio_total,
                id_ubicacion = detalleInventario.id_ubicacion,
                id_almacen = detalleInventario.id_almacen,
                cod_alterno = detalleInventario.cod_alterno
            };

            var sql =
                ConfigurationManager.AppSettings["editar_cantidad_detalle_inventario_con_ubicacion"];

            await dbAccess.SaveDataAsync(sql, parameters);
        }

        private async Task EditDuplicatedDetalleInventario(DetalleInventario detalleInventario, SqlServerDataAccess dbAccess)
        {
            var parameters = new
            {
                id_terminal = detalleInventario.id_terminal,
                no_detalleInv = detalleInventario.no_detalleInv,
                no_articulo = detalleInventario.no_articulo,
                codigo_barra = detalleInventario.codigo_barra,
                cantidad = detalleInventario.cantidad,
                costo = detalleInventario.costo,
                costo_total = detalleInventario.costo_total,
                precio = detalleInventario.precio,
                precio_total = detalleInventario.precio_total,
                id_ubicacion = detalleInventario.id_ubicacion,
                fecha_registro = detalleInventario.fecha_registro,
                fecha_modificacion = detalleInventario.fecha_modificacion,
                id_usuario_registro = detalleInventario.id_usuario_registro,
                id_usuario_modificacion = detalleInventario.id_usuario_modificacion,
                codigo_capturado = detalleInventario.codigo_capturado,
                id_almacen = detalleInventario.id_almacen,
                estado = 1,
                updated_at = DateTime.Now
            };
            var sql = ConfigurationManager.AppSettings["editar_duplicado_detalle_inventario"];

            await dbAccess.SaveDataAsync(sql, parameters);
        }

        private async Task InsertDetalleInventario(DetalleInventario detalleInventario, SqlServerDataAccess dbAccess)
        {
            var parameters = new
            {
                id_terminal = detalleInventario.id_terminal,
                no_detalleInv = detalleInventario.no_detalleInv,
                no_articulo = detalleInventario.no_articulo,
                codigo_barra = detalleInventario.codigo_barra,
                alterno1 = detalleInventario.alterno1,
                alterno2 = detalleInventario.alterno2,
                alterno3 = detalleInventario.alterno3,
                descripcion = detalleInventario.descripcion,
                cantidad = detalleInventario.cantidad,
                costo = detalleInventario.costo,
                costo_total = detalleInventario.costo_total,
                precio = detalleInventario.precio,
                precio_total = detalleInventario.precio_total,
                id_ubicacion = detalleInventario.id_ubicacion,
                cod_alterno = detalleInventario.cod_alterno,
                fecha_registro = detalleInventario.fecha_registro,
                fecha_modificacion = detalleInventario.fecha_modificacion,
                id_usuario_registro = detalleInventario.id_usuario_registro,
                id_usuario_modificacion = detalleInventario.id_usuario_modificacion,
                codigo_capturado = detalleInventario.codigo_capturado,
                id_auditor = detalleInventario.id_auditor,
                id_tipo_auditoria = detalleInventario.id_tipo_auditoria,
                pre_conteo = detalleInventario.pre_conteo,
                cantidad_auditada = detalleInventario.cantidad_auditada,
                diferencia = detalleInventario.diferencia,
                porcentaje_diferencia = detalleInventario.porcentaje_diferencia,
                id_tipo_error = detalleInventario.id_tipo_error,
                notas = detalleInventario.notas,
                id_almacen = detalleInventario.id_almacen,
                estado = 1,
                created_at = DateTime.Now,
                updated_at = DateTime.Now,
                generado = 0,
                ultimo_mensaje_error = string.Empty
            };
            var sql = ConfigurationManager.AppSettings["crear_detalle_inventario"];

            await dbAccess.SaveDataAsync(sql, parameters);
        }


        private async Task InsertDetalleInventarioTransitorio(DetalleInventario detalleInventario, SqlServerDataAccess dbAccess)
        {
            var parameters = new
            {
                id_terminal = detalleInventario.id_terminal,
                no_detalleInv = detalleInventario.no_detalleInv,
                no_articulo = detalleInventario.no_articulo,
                codigo_barra = detalleInventario.codigo_barra,
                alterno1 = detalleInventario.alterno1,
                alterno2 = detalleInventario.alterno2,
                alterno3 = detalleInventario.alterno3,
                descripcion = detalleInventario.descripcion,
                cantidad = detalleInventario.cantidad,
                costo = detalleInventario.costo,
                costo_total = detalleInventario.costo_total,
                precio = detalleInventario.precio,
                precio_total = detalleInventario.precio_total,
                id_ubicacion = detalleInventario.id_ubicacion,
                cod_alterno = detalleInventario.cod_alterno,
                fecha_registro = detalleInventario.fecha_registro,
                fecha_modificacion = detalleInventario.fecha_modificacion,
                id_usuario_registro = detalleInventario.id_usuario_registro,
                id_usuario_modificacion = detalleInventario.id_usuario_modificacion,
                codigo_capturado = detalleInventario.codigo_capturado,
                id_auditor = detalleInventario.id_auditor,
                id_tipo_auditoria = detalleInventario.id_tipo_auditoria,
                pre_conteo = detalleInventario.pre_conteo,
                cantidad_auditada = detalleInventario.cantidad_auditada,
                diferencia = detalleInventario.diferencia,
                porcentaje_diferencia = detalleInventario.porcentaje_diferencia,
                id_tipo_error = detalleInventario.id_tipo_error,
                notas = detalleInventario.notas,
                id_almacen = detalleInventario.id_almacen,
                estado = 1,
                created_at = DateTime.Now,
                updated_at = DateTime.Now,
                generado = 0,
                ultimo_mensaje_error = string.Empty
            };
            var sql = ConfigurationManager.AppSettings["crear_detalle_inventario_transitorio"];

            await dbAccess.SaveDataAsync(sql, parameters);
        }

        private async Task InsertDetalleInventarioTransitorioSinArticulo(DetalleInventario detalleInventario, SqlServerDataAccess dbAccess)
        {
            var parameters = new
            {
                id_terminal = detalleInventario.id_terminal,
                no_detalleInv = detalleInventario.no_detalleInv,
                no_articulo = detalleInventario.no_articulo,
                codigo_barra = detalleInventario.codigo_barra,
                alterno1 = detalleInventario.alterno1,
                alterno2 = detalleInventario.alterno2,
                alterno3 = detalleInventario.alterno3,
                descripcion = detalleInventario.descripcion,
                cantidad = detalleInventario.cantidad,
                costo = detalleInventario.costo,
                costo_total = detalleInventario.costo_total,
                precio = detalleInventario.precio,
                precio_total = detalleInventario.precio_total,
                id_ubicacion = detalleInventario.id_ubicacion,
                cod_alterno = detalleInventario.cod_alterno,
                fecha_registro = detalleInventario.fecha_registro,
                fecha_modificacion = detalleInventario.fecha_modificacion,
                id_usuario_registro = detalleInventario.id_usuario_registro,
                id_usuario_modificacion = detalleInventario.id_usuario_modificacion,
                codigo_capturado = detalleInventario.codigo_capturado,
                id_auditor = detalleInventario.id_auditor,
                id_tipo_auditoria = detalleInventario.id_tipo_auditoria,
                pre_conteo = detalleInventario.pre_conteo,
                cantidad_auditada = detalleInventario.cantidad_auditada,
                diferencia = detalleInventario.diferencia,
                porcentaje_diferencia = detalleInventario.porcentaje_diferencia,
                id_tipo_error = detalleInventario.id_tipo_error,
                notas = detalleInventario.notas,
                id_almacen = detalleInventario.id_almacen,
                estado = 1,
                created_at = DateTime.Now,
                updated_at = DateTime.Now,
                generado = 0,
                ultimo_mensaje_error = "El código del artículo no existe en la base de datos"
            };
            var sql = ConfigurationManager.AppSettings["crear_detalle_inventario_transitorio"];

            await dbAccess.SaveDataAsync(sql, parameters);
        }


        private DetalleInventario SetFullDetalleInventario(
            DetalleInventario partialDetalleInventario, Articulo articulo, string codigoAlterno)
        {
            float.TryParse(articulo.costo, out var articuloCosto);
            float.TryParse(articulo.precio, out var articuloPrecio);
            float.TryParse(partialDetalleInventario.cantidad, out var articuloCantidad);

            partialDetalleInventario.costo_total = (articuloCosto * articuloCantidad).ToString();

            partialDetalleInventario.precio_total = (articuloPrecio * articuloCantidad).ToString();

            partialDetalleInventario.codigo_barra = string.IsNullOrEmpty(partialDetalleInventario.codigo_barra)
                ? articulo.codigo_barra : partialDetalleInventario.codigo_barra;

            partialDetalleInventario.id_auditor =
                (string.IsNullOrEmpty(partialDetalleInventario.id_auditor)
                ? 0 : int.Parse(partialDetalleInventario.id_auditor)).ToString();

            partialDetalleInventario.id_tipo_auditoria =
                (string.IsNullOrEmpty(partialDetalleInventario.id_tipo_auditoria)
                ? 0 : int.Parse(partialDetalleInventario.id_tipo_auditoria)).ToString();

            partialDetalleInventario.pre_conteo =
                (string.IsNullOrEmpty(partialDetalleInventario.pre_conteo)
                ? 0 : int.Parse(partialDetalleInventario.pre_conteo)).ToString();

            partialDetalleInventario.cantidad_auditada =
                (string.IsNullOrEmpty(partialDetalleInventario.cantidad_auditada)
                    ? 0 : int.Parse(partialDetalleInventario.cantidad_auditada)).ToString();

            partialDetalleInventario.diferencia =
                (string.IsNullOrEmpty(partialDetalleInventario.diferencia)
                    ? 0 : int.Parse(partialDetalleInventario.diferencia)).ToString();

            partialDetalleInventario.porcentaje_diferencia =
                (string.IsNullOrEmpty(partialDetalleInventario.porcentaje_diferencia)
                    ? 0 : int.Parse(partialDetalleInventario.porcentaje_diferencia)).ToString();

            partialDetalleInventario.id_tipo_error =
                (string.IsNullOrEmpty(partialDetalleInventario.id_tipo_error)
                    ? 0 : int.Parse(partialDetalleInventario.id_tipo_error)).ToString();

            partialDetalleInventario.notas = string.IsNullOrEmpty(partialDetalleInventario.notas)
                ? string.Empty : partialDetalleInventario.notas;

            partialDetalleInventario.codigo_capturado =
                string.IsNullOrEmpty(partialDetalleInventario.codigo_capturado)
                ? string.Empty : partialDetalleInventario.codigo_capturado;

            partialDetalleInventario.alterno1 = articulo.alterno1;
            partialDetalleInventario.alterno2 = articulo.alterno2;
            partialDetalleInventario.alterno3 = articulo.alterno3;
            partialDetalleInventario.descripcion = articulo.descripcion;
            partialDetalleInventario.costo = articulo.costo;
            partialDetalleInventario.precio = articulo.precio;
            partialDetalleInventario.cod_alterno = codigoAlterno;

            return partialDetalleInventario;
        }

        private DetalleInventario SetDetalleInventarioForEdit(DetalleInventario detalleInventario, Articulo articulo, bool isAuditing = false)
        {
            var cantidad = isAuditing ?
                detalleInventario.cantidad_auditada : detalleInventario.cantidad;

            float.TryParse(articulo.costo, out var articuloCosto);
            float.TryParse(articulo.precio, out var articuloPrecio);
            float.TryParse(cantidad, out var articuloCantidad);
            detalleInventario.costo_total = (articuloCosto * articuloCantidad).ToString();
            detalleInventario.precio_total = (articuloPrecio * articuloCantidad).ToString();
            detalleInventario.costo = articulo.costo;
            detalleInventario.precio = articulo.precio;

            return detalleInventario;
        }

        private async Task<string> GetCodigoAlterno(string ubicacionId, SqlServerDataAccess dbAccess)
        {
            var sql = ConfigurationManager.AppSettings["buscar_ubicacion_por_id"];

            var ubicacion =
                await dbAccess.LoadFirstOrDefaultAsync<Ubicacion, dynamic>(
                    sql, new { id = ubicacionId });

            return ubicacion == null ?
                string.Empty
                : string.IsNullOrEmpty(ubicacion.cod_alterno) ?
                    string.Empty
                    : ubicacion.cod_alterno;
        }

        private async Task<string> GetArticuloNumber(string sql, object parameters, SqlServerDataAccess dbAccess)
        {
            var targetDetalleInventario =
                await dbAccess.LoadFirstOrDefaultAsync<DetalleInventario, dynamic>(sql, parameters);

            if (targetDetalleInventario == null
                || string.IsNullOrEmpty(targetDetalleInventario.no_articulo))
            {
                return "0";
            }

            return int.TryParse(targetDetalleInventario.no_articulo, out var articuloNumber) ?
                articuloNumber.ToString() : "0";
        }

        private async Task<Articulo> GetArticulo(
            string articuloNumber, SqlServerDataAccess dbAccess)
        {
            var sql = ConfigurationManager.AppSettings["buscar_articulo_por_numero"];

            return
                await dbAccess.LoadFirstOrDefaultAsync<Articulo, dynamic>(
                    sql, new { no_articulo = articuloNumber });
        }

        private async Task<InventoryStatistic> GetInventoryStatistics(DetalleInventarioUnitOfWork unitOfWork, IEnumerable<DetalleInventario> itemsNotInStore)
        {
            var itemsNotInStoreCount = itemsNotInStore.Count();
            decimal itemsNotInStoreCost = 0;
            decimal itemsNotInStorePrice = 0;

            foreach (var item in itemsNotInStore)
            {
                if (decimal.TryParse(item.costo_total, out var itemCost))
                {
                    itemsNotInStoreCost += itemCost;
                }

                if (decimal.TryParse(item.precio_total, out var itemPrice))
                {
                    itemsNotInStorePrice += itemPrice;
                }
            }
            itemsNotInStoreCost = RoundDecimalPlaces(itemsNotInStoreCost, 2);
            itemsNotInStorePrice = RoundDecimalPlaces(itemsNotInStorePrice, 2);

            var sql = ConfigurationManager.AppSettings["buscar_estadisticas_inventario"];

            var inventoryInfo = await unitOfWork.DetalleInventarioInfoRepository.LoadFirstAsync(sql, new { });

            var inventoryCount = 0;
            decimal inventoryCost = 0;
            decimal inventoryPrice = 0;

            if (inventoryInfo != null)
            {
                inventoryCount = inventoryInfo.cantidad_articulos;
                inventoryCost = inventoryInfo.costo_total;
                inventoryPrice = inventoryInfo.precio_total;
            }

            var inventoryNotInStorePercentage = inventoryCount > 0 ? (decimal)itemsNotInStoreCount / inventoryCount * 100 : 0;
            var inventoryNotInStoreCostPercentage = inventoryCost > 0 ? itemsNotInStoreCost / inventoryCost * 100 : 0;
            var inventoryNotInStorePricePercentage = inventoryPrice > 0 ? itemsNotInStorePrice / inventoryPrice * 100 : 0;

            inventoryNotInStorePercentage = RoundDecimalPlaces(inventoryNotInStorePercentage, 6);
            inventoryNotInStoreCostPercentage = RoundDecimalPlaces(inventoryNotInStoreCostPercentage, 6);
            inventoryNotInStorePricePercentage = RoundDecimalPlaces(inventoryNotInStorePricePercentage, 6);

            return new InventoryStatistic
            {
                cantidad_articulos = inventoryCount,
                cantidad_articulos_no_existen_tienda = itemsNotInStoreCount,
                porciento_cantidad_articulos = inventoryNotInStorePercentage,
                costo_total = inventoryCost,
                costo_total_no_existe_tienda = itemsNotInStoreCost,
                porciento_costo_total = inventoryNotInStoreCostPercentage,
                precio_total = inventoryPrice,
                precio_total_no_existe_tienda = itemsNotInStorePrice,
                porciento_precio_total = inventoryNotInStorePricePercentage
            };
        }

        private async Task<IEnumerable<DetalleInventario>> GetItemsNotInStore(DetalleInventarioUnitOfWork unitOfWork)
        {
            var sql = ConfigurationManager.AppSettings["buscar_articulos_almacen_no_existentes__en_tienda"];
            return await unitOfWork.DetalleInventarioRepository.LoadAsync(sql, new { });
        }

        #endregion
    }
}
