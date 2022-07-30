using System;
using System.Collections.Generic;
using System.Configuration;
using Newtonsoft.Json;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using DataAccess.Models;
using System.Threading.Tasks;
using chitecapi.Responses;
using DataAccess;
using System.Data.Common;

namespace chitecapi.Controllers
{
    public class detalle_inventario_ciclicoController : ApiController
    {
        #region Actions
        // GET api/detalle_inventario_ciclico?db=db1
        /// <summary>
        /// Obtiene una lista del detalle de inventario ciclico.
        /// </summary>
        /// <param name="db" example="db1">The Database ID</param>
        /// <param name="no_conteo" example="no_conteo">Numero de Conteo</param>
        /// <param name="no_articulo" example="no_articulo">Numero de articulo</param>
        /// <param name="saveresult" example="0">0 Para guardar los datos en una tabla en sqlserver </param>
        /// <param name="tablename" example="nombretabla">El nombre de la tabla para guardar</param>
        /// <remarks>
        /// Se utiliza para obtener la lista del detalle de inventario cicliclo, generado con un formato JSON:
        /// 
        ///     Example Result:
        ///     {
        ///         "data": [
        ///                    {
        ///                        "id": 9,
        ///                        "no_articulo": "122",
        ///                        "codigo_barra": "33333",
        ///                        "alterno1": "444",
        ///                        "alterno2": "222",
        ///                        "alterno3": "333",
        ///                        "descripcion": "sssss",
        ///                        "existencia": 5,
        ///                        "unidad_medida": "LB",
        ///                        "costo": 5,
        ///                        "precio": 5,
        ///                        "referencia": "2222",
        ///                        "marca": "sss",
        ///                        "id_familia_productos": 3,
        ///                        "id_clasificacion": 1,
        ///                        "fecha_asignada_ciclico": "2021-10-18T00:00:00",
        ///                        "estado": 1,
        ///                        "no_detalleinv_ciclico": "1",
        ///                        "id_terminal": "2",
        ///                        "cantidad": 5,
        ///                        "diferencia_valor_absoluto": 3,
        ///                        "acierto": 2,
        ///                        "falla": 3,
        ///                       "id_usuario_registro": 1,
        ///                        "fecha_registro": "2021-10-18T00:00:00",
        ///                        "created_at": "2021-10-18T00:00:00",
        ///                        "updated_at": "2021-10-20T00:00:00"
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
        public IHttpActionResult Index([FromUri] string db = "db1", string no_conteo="", string no_articulo = "", string saveresult = "0", string tablename = "detalleciclicotest")
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

            var sql = ConfigurationManager.AppSettings["getdetalleinventariociclico"];

            if (sql == null)
            {
                sql = "select * from detalle_inventario_ciclico";
            }

            if (!no_conteo.Equals(""))
                sql = sql + " AND no_conteo = " + no_conteo;



            if (!no_articulo.Equals(""))
                sql = sql + " AND no_articulo = " + no_articulo;



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


        // GET api/detalle_inventario_ciclico/porcentaje_precision?db=db1
        /// <summary>
        /// Obtiene el porcentaje de precision del detalle de inventario ciclico.
        /// </summary>
        /// <param name="db" example="db1">The Database ID</param>
        /// <remarks>
        /// Se utiliza para obtener el porcentaje de precision del detalle de inventario cicliclo, generado con un formato JSON:
        /// 
        ///     Example Result:
        ///     {
        ///         "porcentaje_precision": 100,
        ///         "error": 0,
        ///         "error_type": 0,
        ///         "error_message": 0
        ///     }
        /// </remarks>
        /// <response code="401">Unauthorized. Error en la configuracion de la base de datos</response>              
        /// <response code="200">OK. Devuelve el objeto solicitado.</response>        
        /// <response code="404">NotFound. No se ha encontrado el objeto solicitado.</response>
        [HttpGet]
        public IHttpActionResult porcentaje_precision([FromUri] string db = "db1", string fecha="", string saveresult = "0", string tablename = "porcentajeprecisiontest")
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



            string condicionfecha = "";
            if (!fecha.Equals(""))
                condicionfecha = " AND fecha_asignada_ciclico = '" + fecha+"'";


            var sql = "select case when subquery.nro_inventarios >0 then (subquery.nro_aciertos/subquery.nro_inventarios)*100 else  0 end as porcentaje_precision from "+
                      "  (select "+ 
                      $" COALESCE((SELECT SUM(acierto) as nro_aciertos FROM detalle_inventario_ciclico  WHERE 1 = 1 {condicionfecha}), 0) as nro_aciertos, "+
                      $" COALESCE( (SELECT SUM(1) as nro_inventarios FROM detalle_inventario_ciclico  WHERE 1 = 1 {condicionfecha} ),0) as nro_inventarios "+
                      "  ) subquery ";




            dataUtil.PrepareStatement(sql);


            DataTable table = new DataTable();
            dataUtil.FillDatatable(table);



            Double porcentaje = 0.0;

            foreach (DataRow row in table.Rows)
            {
                porcentaje = double.Parse(row["porcentaje_precision"].ToString());
            }

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
            jsonvalues.Add("porcentaje_precision", porcentaje);
            jsonvalues.Add("error", 0);
            jsonvalues.Add("error_type", 0);
            jsonvalues.Add("error_message", 0);


            return Json(jsonvalues);
        }



       

        /// <summary>
        /// Editar Detalle Inventario Ciclico.
        /// </summary>
        /// <param name="db" example="db1">The Database ID</param>
        /// <remarks>
        /// Se utiliza para Editar multiples Entradas de inventario, Enviando un json con la información de la captura
        /// </remarks>
        /// <response code="401">Unauthorized. Error en la configuracion de la base de datos</response>              
        /// <response code="200">OK. Devuelve el objeto solicitado.</response>        
        /// <response code="404">NotFound. No se ha encontrado el objeto solicitado.</response>
        [HttpPost]
        public async Task<IHttpActionResult> Editar([FromBody] List<DetalleInventariociclico> detallesInventariociclico, string db="db1")
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

            foreach (var partialDetalleInventario in detallesInventariociclico)
            {
                if (partialDetalleInventario?.IsNotInitialized == true)
                {
                    return new CustomJsonActionResult(
                        System.Net.HttpStatusCode.NotFound,
                        new JsonErrorResponse(1, 400, "Faltan parámetros"));
                }

                try
                {
                    var articulo = await GetArticulo(partialDetalleInventario.No_Articulo, dbAccess);

                    if (articulo == null)
                    {
                        return new CustomJsonActionResult(
                            System.Net.HttpStatusCode.NotFound,
                            new JsonErrorResponse(1, 1, $"El artículo con el código  {partialDetalleInventario.No_Articulo} no existe en la base de datos."));
                    }


                    var fullDetalleInventario = SetFullDetalleInventario(partialDetalleInventario, articulo);

                    var parameters = new
                    {
                        No_interno = fullDetalleInventario.No_interno,
                        No_Conteo = fullDetalleInventario.No_Conteo,
                        Nombre_Conteo = fullDetalleInventario.Nombre_Conteo,
                        No_detalleinv_ciclico = fullDetalleInventario.No_detalleinv_ciclico,
                        Tipo_Conteo = fullDetalleInventario.Tipo_Conteo,
                        No_Articulo = fullDetalleInventario.No_Articulo,
                        Codigo_Barra = fullDetalleInventario.Codigo_Barra,
                        Alterno1 = fullDetalleInventario.Alterno1,
                        Alterno2 = fullDetalleInventario.Alterno2,
                        Alterno3 = fullDetalleInventario.Alterno3,
                        Descripcion = fullDetalleInventario.Descripcion,
                        Existencia = fullDetalleInventario.Existencia,
                        Cantidad_Transito = fullDetalleInventario.Cantidad_Transito,
                        Unidad_Medida = fullDetalleInventario.Unidad_Medida,
                        Costo = fullDetalleInventario.Costo,
                        Precio = fullDetalleInventario.Precio,
                        Marca = fullDetalleInventario.Marca,
                        id_Familia_Productos = fullDetalleInventario.id_Familia_Productos,
                        id_Clasificacion_inventario = fullDetalleInventario.id_Clasificacion_inventario,
                        Fecha_asignada_ciclico = fullDetalleInventario.Fecha_asignada_ciclico,
                        estado = fullDetalleInventario.estado,
                        Id_Terminal = fullDetalleInventario.Id_Terminal,
                        Wharehouse_Existencia = fullDetalleInventario.Wharehouse_Existencia,
                        Diferencia = fullDetalleInventario.Diferencia,
                        Diferencia_porcentual = fullDetalleInventario.Diferencia_porcentual,
                        Cantidad = fullDetalleInventario.Cantidad,
                        Id_Usuario_Registro = fullDetalleInventario.Id_Usuario_Registro,
                        Fecha_Registro = fullDetalleInventario.Fecha_Registro,
                        Cantidad_Contada = fullDetalleInventario.Cantidad_Contada,
                        Observaciones = fullDetalleInventario.Observaciones,
                        Costing_Code = fullDetalleInventario.Costing_Code,
                        Wharehouse = fullDetalleInventario.Wharehouse,
                        ubicacion = fullDetalleInventario.ubicacion,
                        Fecha_Conteo = fullDetalleInventario.Fecha_Conteo,
                        Hora_Conteo = fullDetalleInventario.Hora_Conteo,
                        updated_at = DateTime.Now
                    };
                    //Verificamos el no_conteo si existe lo editamos y si no entonces lo guardamos

                    var sql = "";
                    var record = await GetdetalleinventarioCiclico(partialDetalleInventario.No_interno,partialDetalleInventario.Id_Terminal, dbAccess);

                    if (record == null)
                    {
                        sql = ConfigurationManager.AppSettings["guardar_detalle_inventario_ciclico"];
                    }
                    else
                    {
                        sql = ConfigurationManager.AppSettings["editar_detalle_inventario_ciclico"];
                    }

                    

                   

                    await dbAccess.SaveDataAsync(sql, parameters);
                }
                catch (DbException exception)
                {
                    return new CustomJsonActionResult(
                        System.Net.HttpStatusCode.NotFound,
                        new JsonErrorResponse(1, 1, exception.Message));
                }
            }

            return new CustomJsonActionResult(
                System.Net.HttpStatusCode.OK,
                new JsonErrorResponse(0, 0, "0"));
        }



        // GET api/detalle_inventario_ciclico/CrearInventarioCiclicobyItems?db=db1&codigo_barra=2222
        /// <summary>
        /// Obtiene una lista de articulos y con esto crea un conjunto de detalle inventario ciclico para ser contados.
        /// </summary>
        /// <param name="db" example="db1">The Database ID</param>
        /// <param name="no_articulo" example="111">Item ID</param>
        /// <param name="codigo_barra" example="111">Item CodeBar</param>
        /// <param name="alterno1" example="111">Alternative Number1</param>
        /// <param name="alterno2" example="111">Alternative Number2</param>
        /// <param name="alterno3" example="111">Alternative Number3</param>
        /// <param name="descripcion" example="Coca cola">Item Description</param>
        /// <remarks>
        /// Se utiliza para obtener la lista de articulos y con esto proceder a crear los detalles de inventario ciclico
        /// </remarks>
        /// <response code="401">Unauthorized. Error en la configuracion de la base de datos</response>              
        /// <response code="200">OK. Devuelve el objeto solicitado.</response>        
        /// <response code="404">NotFound. No se ha encontrado el objeto solicitado.</response>
        [HttpPost]
        public async Task<IHttpActionResult> CrearInventarioCiclicobyItems([FromUri] string Id_Terminal, string db = "db1", string no_articulo = "0", string codigo_barra = "0", string alterno1 = "0", string alterno2 = "0", string alterno3 = "0", string descripcion = "",string familia="", string marca = "", string costodesde="0", string costohasta = "0", string preciodesde="0", string preciohasta = "0", int cantidadoperadores = 1 )
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
            var dbAccess = new SqlServerDataAccess(ConfigurationManager.ConnectionStrings[db].ConnectionString, ConfigurationManager.ConnectionStrings[db].ProviderName);



            var sql = ConfigurationManager.AppSettings["getallarticulosbydetalleinventariociclico"];

            if (sql == null)
            {
                sql = "select * from articulos";
            }


/*
            if (!no_articulo.Equals("0") || !codigo_barra.Equals("0") || !alterno1.Equals("0") || !alterno2.Equals("0") || !alterno3.Equals("0") || !descripcion.Equals("") || !costo.Equals("0") || !familia.Equals("") || !precio.Equals("0") )
            {
                sql = ConfigurationManager.AppSettings["getallarticulosbydetalleinventariociclico"];
                dataUtil.PrepareStatement(sql);


                dataUtil.AddParameter("@no_articulo", no_articulo);
                dataUtil.AddParameter("@codigo_barra", codigo_barra);
                dataUtil.AddParameter("@alterno1", alterno1);
                dataUtil.AddParameter("@alterno2", alterno2);
                dataUtil.AddParameter("@alterno3", alterno3);
                dataUtil.AddParameter("@descripcion", descripcion);
                dataUtil.AddParameter("@familia", familia);
                dataUtil.AddParameter("@costo", costo);
                dataUtil.AddParameter("@precio", precio);
            }
*/

            if (!preciodesde.Equals("0"))
                sql = sql + " AND precio >= " + preciodesde;

            if (!preciohasta.Equals("0"))
                sql = sql + " AND precio >= " + preciohasta;


            if (!costodesde.Equals("0"))
                sql = sql + " AND costo >= " + costodesde;


            if (!costohasta.Equals("0"))
                sql = sql + " AND costo >= " + costohasta;

            if (!familia.Equals(""))
                sql = sql + " AND familia = " + familia;

            if (!marca.Equals(""))
                sql = sql + " AND marca = " + marca;

            if (!alterno3.Equals("0"))
                sql = sql + " AND alterno3 = " + alterno3;

            if (!descripcion.Equals(""))
                sql = sql + " AND descripcion = " + descripcion;

            if (!no_articulo.Equals("0"))
                sql = sql + " AND no_articulo = " + no_articulo;

            if (!codigo_barra.Equals("0"))
                sql = sql + " AND codigo_barra = " + codigo_barra;

            if (!alterno1.Equals("0"))
                sql = sql + " AND alterno1 = " + alterno1;

            if (!alterno2.Equals("0"))
                sql = sql + " AND alterno2 = " + alterno2;


            dataUtil.PrepareStatement(sql);


            DataTable table = new DataTable();
            dataUtil.FillDatatable(table);
            var no_conteo = await GetLastDetalleInventarioCiclico(dbAccess);

            //Distribuir por usuarios


            dataUtil.CloseConnection();


            foreach (DataRow row in table.Rows)
            {
                try
                {
                    var articulo = await GetArticulo(row["no_articulo"].ToString(), dbAccess);

                    if (articulo == null)
                    {
                        return new CustomJsonActionResult(
                            System.Net.HttpStatusCode.NotFound,
                            new JsonErrorResponse(1, 1, $"El artículo con el código  {row["no_articulo"].ToString()} no existe en la base de datos."));
                    }

                    DetalleInventariociclico partialDetalleInventario = new DetalleInventariociclico();
                    var no_id_detalle = await GetLastidDetalleInventarioCiclico(dbAccess);


                    var fullDetalleInventario = SetFullDetalleInventario(partialDetalleInventario, articulo);
                    var parameters = new
                    {
                        No_interno = no_conteo,
                        No_Conteo = no_conteo,
                        Nombre_Conteo = fullDetalleInventario.Nombre_Conteo,
                        No_detalleinv_ciclico = no_id_detalle,
                        Tipo_Conteo = fullDetalleInventario.Tipo_Conteo,
                        No_Articulo = fullDetalleInventario.No_Articulo,
                        Codigo_Barra = fullDetalleInventario.Codigo_Barra,
                        Alterno1 = fullDetalleInventario.Alterno1,
                        Alterno2 = fullDetalleInventario.Alterno2,
                        Alterno3 = fullDetalleInventario.Alterno3,
                        Descripcion = fullDetalleInventario.Descripcion,
                        Existencia = fullDetalleInventario.Existencia,
                        Cantidad_Transito = fullDetalleInventario.Cantidad_Transito,
                        Unidad_Medida = fullDetalleInventario.Unidad_Medida,
                        Costo = fullDetalleInventario.Costo,
                        Precio = fullDetalleInventario.Precio,
                        Marca = fullDetalleInventario.Marca,
                        id_Familia_Productos = fullDetalleInventario.id_Familia_Productos,
                        id_Clasificacion_inventario = fullDetalleInventario.id_Clasificacion_inventario,
                        Fecha_asignada_ciclico = fullDetalleInventario.Fecha_asignada_ciclico,
                        estado = fullDetalleInventario.estado,
                        Id_Terminal = Id_Terminal,
                        Wharehouse_Existencia = fullDetalleInventario.Wharehouse_Existencia,
                        Diferencia = fullDetalleInventario.Diferencia,
                        Diferencia_porcentual = fullDetalleInventario.Diferencia_porcentual,
                        Cantidad = fullDetalleInventario.Cantidad,
                        Id_Usuario_Registro = fullDetalleInventario.Id_Usuario_Registro,
                        Fecha_Registro = fullDetalleInventario.Fecha_Registro,
                        Cantidad_Contada = fullDetalleInventario.Cantidad_Contada,
                        Observaciones = fullDetalleInventario.Observaciones,
                        Costing_Code = fullDetalleInventario.Costing_Code,
                        Wharehouse = fullDetalleInventario.Wharehouse,
                        ubicacion = fullDetalleInventario.ubicacion,
                        Fecha_Conteo = fullDetalleInventario.Fecha_Conteo,
                        Hora_Conteo = fullDetalleInventario.Hora_Conteo,
                        updated_at = DateTime.Now
                    };
                    //Verificamos el no_conteo si existe lo editamos y si no entonces lo guardamos

                    sql = "";
                    var record = await GetdetalleinventarioCiclico(partialDetalleInventario.No_interno, Id_Terminal, dbAccess);

                    if (record == null)
                    {
                        sql = ConfigurationManager.AppSettings["guardar_detalle_inventario_ciclico"];
                    }
                    else
                    {
                        sql = ConfigurationManager.AppSettings["editar_detalle_inventario_ciclico"];
                    }





                    await dbAccess.SaveDataAsync(sql, parameters);
                }
                catch (DbException exception)
                {
                    return new CustomJsonActionResult(
                        System.Net.HttpStatusCode.NotFound,
                        new JsonErrorResponse(1, 1, exception.Message));
                }
            }


           


            Dictionary<string, object> jsonvalues = new Dictionary<string, object>();
            jsonvalues.Add("data", table);
            jsonvalues.Add("error", 0);
            jsonvalues.Add("error_type", 0);
            jsonvalues.Add("error_message", 0);


            return new CustomJsonActionResult(
                System.Net.HttpStatusCode.OK,
                new JsonErrorResponse(0, 0, "0"));
        }

        #endregion



        #region Helper methods

        private async Task<DetalleInventariociclico> GetdetalleinventarioCiclico(
            string No_interno, string Id_Terminal, SqlServerDataAccess dbAccess)
        {
            var sql = ConfigurationManager.AppSettings["buscar_detalle_inventario_ciclico_por_no_conteo"];

            return
                await dbAccess.LoadFirstOrDefaultAsync<DetalleInventariociclico, dynamic>(
                    sql, new { No_interno = No_interno, Id_Terminal= Id_Terminal });
        }

        private async Task<int> GetLastDetalleInventarioCiclico(
           SqlServerDataAccess dbAccess)
        {
            var sql = ConfigurationManager.AppSettings["buscar_ultimo_inventario_ciclico"];

            return
                await dbAccess.LoadFirstOrDefaultAsync<int, dynamic>(
                    sql, new {  });
        }

        private async Task<int> GetLastidDetalleInventarioCiclico(
         SqlServerDataAccess dbAccess)
        {
            var sql = ConfigurationManager.AppSettings["buscar_ultimo_id_inventario_ciclico"];

            return
                await dbAccess.LoadFirstOrDefaultAsync<int, dynamic>(
                    sql, new { });
        }


        private async Task<Articulo> GetArticulo(
        string articuloNumber, SqlServerDataAccess dbAccess)
        {
            var sql = ConfigurationManager.AppSettings["buscar_articulo_por_numero"];

            return
                await dbAccess.LoadFirstOrDefaultAsync<Articulo, dynamic>(
                    sql, new { no_articulo = articuloNumber });
        }


        private DetalleInventariociclico SetFullDetalleInventario(
            DetalleInventariociclico partialDetalleInventario, Articulo articulo)
        {
            float.TryParse(articulo.costo, out var articuloCosto);
            float.TryParse(articulo.precio, out var articuloPrecio);



            partialDetalleInventario.Diferencia =
                (string.IsNullOrEmpty(partialDetalleInventario.Diferencia)
                    ? 0 : int.Parse(partialDetalleInventario.Diferencia)).ToString();

                        
            partialDetalleInventario.Alterno1 = articulo.alterno1;
            partialDetalleInventario.Alterno2 = articulo.alterno2;
            partialDetalleInventario.Alterno3 = articulo.alterno3;
            partialDetalleInventario.No_Articulo = articulo.no_articulo;
            partialDetalleInventario.Descripcion = articulo.descripcion;
            partialDetalleInventario.Codigo_Barra = articulo.codigo_barra;
            partialDetalleInventario.Costo = articulo.costo;
            partialDetalleInventario.Precio = articulo.precio;


            return partialDetalleInventario;
        }




        #endregion
    }
}
