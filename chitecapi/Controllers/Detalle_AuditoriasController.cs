using chitecapi.Responses;
using DataAccess;
using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using System.Web.Http;
using System.Data;
using System.Data.SqlClient;

namespace chitecapi.Controllers
{
    public class Detalle_AuditoriasController : ApiController
    {
        #region Actions

        // GET api/Detalle_Auditorias/ListadoDetalleAuditorias?db=db1
        /// <summary>
        /// Obtiene una lista del detalle de las auditorias.
        /// </summary>
        /// <param name="db" example="db1">The Database ID</param>
        /// <remarks>
        /// Se utiliza para obtener una lista de los articulos en el detalle de las auditoria, generado con un formato JSON:
        /// 
        ///     Example Result:
        ///     {
        ///         "data": [
        ///                   {
        ///                       "id": 1,
        ///                       "id_terminal": "B-4CTTPC2-573D2323",
        ///                       "no_detalleInv": "1",
        ///                       "no_articulo": "25787",
        ///                       "descripcion": "AZUCAR CREMA 1 LB",
        ///                       "id_tipo_ubicacion": 0,
        ///                       "cod_alterno": "G02",
        ///                       "pre_conteo": 2,
        ///                       "cantidad_auditada": 3,
        ///                       "diferencia": 1,
        ///                       "porcentaje_diferencia": 33.33333333,
        ///                       "id_tipo_error": 0,
        ///                       "notas": "",
        ///                       "codigo_barra": "123",
        ///                       "id_usuario_registro": 1,
        ///                       "secuencia_tiro": 1,
        ///                       "alterno1": "",
        ///                       "alterno2": "",
        ///                       "alterno3": "",
        ///                       "cantidad": 0,
        ///                       "costo": 0,
        ///                       "costo_total": 0,
        ///                       "precio": 0,
        ///                       "precio_total": 0,
        ///                       "id_ubicacion": 0,
        ///                       "fecha_registro": "0000-00-00 00:00:00",
        ///                       "fecha_modificacion": "0000-00-00 00:00:00",
        ///                       "id_usuario_modificacion": 0,
        ///                       "id_auditor": 0,
        ///                       "id_tipo_auditoria": 0,
        ///                       "estado": 0,
        ///                       "created_at": "0000-00-00 00:00:00",
        ///                       "updated_at": "0000-00-00 00:00:00",
        ///                       "es_cantidad_igual_tiros": 0
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
        public IHttpActionResult ListadoDetalleAuditorias([FromUri] string db = "db1")
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


            var sql = ConfigurationManager.AppSettings["getdetalleauditorias"];

            if (sql == null)
            {
                sql = " SELECT *, case  when d.cantidad=d.no_detalleInv then 1 else 0 end as es_cantidad_igual_tiros FROM detalle_auditorias d ";
            }



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




        [HttpPost]
        public async Task<IHttpActionResult> Guardar([FromBody] List<DetalleAuditoria> partialDetallesAuditoria, string db)
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

            var dbAccess = new SqlServerDataAccess(
                ConfigurationManager.ConnectionStrings[db].ConnectionString, ConfigurationManager.ConnectionStrings[db].ProviderName
                );

            foreach (var partialDetalleAuditoria in partialDetallesAuditoria)
            {
                if (partialDetalleAuditoria?.IsNotInitialized == true)
                {
                    return new CustomJsonActionResult(
                        System.Net.HttpStatusCode.NotFound,
                        new JsonErrorResponse(1, 400, "Faltan parámetros"));
                }

                try
                {
                    var articulo = await GetArticulo(partialDetalleAuditoria.no_articulo, dbAccess);

                    if (articulo == null)
                    {
                        return new CustomJsonActionResult(
                            System.Net.HttpStatusCode.NotFound,
                            new JsonErrorResponse(1, 1, $"El artículo con el código  {partialDetalleAuditoria.no_articulo} no existe en la base de datos."));
                    }

                    var fullDetalleAuditoria =
                        SetFullDetalleAuditoria(partialDetalleAuditoria, articulo);

                    var sql = ConfigurationManager.AppSettings["crear_detalle_auditoria"];

                    var parameters = new
                    {
                        id_terminal = fullDetalleAuditoria.id_terminal,
                        no_detalleInv = fullDetalleAuditoria.no_detalleInv,
                        no_articulo = fullDetalleAuditoria.no_articulo,
                        descripcion = fullDetalleAuditoria.descripcion,
                        id_tipo_ubicacion = fullDetalleAuditoria.id_tipo_ubicacion,
                        cod_alterno = fullDetalleAuditoria.cod_alterno,
                        pre_conteo = fullDetalleAuditoria.pre_conteo,
                        cantidad_auditada = fullDetalleAuditoria.cantidad_auditada,
                        diferencia = fullDetalleAuditoria.diferencia,
                        porcentaje_diferencia = fullDetalleAuditoria.porcentaje_diferencia,
                        id_tipo_error = fullDetalleAuditoria.id_tipo_error,
                        notas = fullDetalleAuditoria.notas,
                        codigo_barra = fullDetalleAuditoria.codigo_barra,
                        id_usuario_registro = fullDetalleAuditoria.id_usuario_registro,
                        secuencia_tiro = fullDetalleAuditoria.secuencia_tiro,
                        alterno1 = fullDetalleAuditoria.alterno1,
                        alterno2 = fullDetalleAuditoria.alterno2,
                        alterno3 = fullDetalleAuditoria.alterno3,
                        cantidad = fullDetalleAuditoria.cantidad,
                        costo = fullDetalleAuditoria.costo,
                        costo_total = fullDetalleAuditoria.costo_total,
                        precio = fullDetalleAuditoria.precio,
                        precio_total = fullDetalleAuditoria.precio_total,
                        id_ubicacion = fullDetalleAuditoria.id_ubicacion,
                        fecha_registro = fullDetalleAuditoria.fecha_registro,
                        fecha_modificacion = fullDetalleAuditoria.fecha_modificacion,
                        id_usuario_modificacion = fullDetalleAuditoria.id_usuario_modificacion,
                        id_auditor = fullDetalleAuditoria.id_auditor,
                        id_tipo_auditoria = fullDetalleAuditoria.id_tipo_auditoria,
                        estado = 1,
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

        #endregion

        #region Helper methods

        private DetalleAuditoria SetFullDetalleAuditoria(DetalleAuditoria partialDetalleAuditoria, Articulo articulo)
        {
            float.TryParse(articulo.costo, out var articuloCosto);
            float.TryParse(articulo.precio, out var articuloPrecio);
            float.TryParse(partialDetalleAuditoria.cantidad, out var articuloCantidad);

            partialDetalleAuditoria.costo_total = (articuloCosto * articuloCantidad).ToString();
            partialDetalleAuditoria.precio_total = (articuloPrecio * articuloCantidad).ToString();
            partialDetalleAuditoria.alterno1 = articulo.alterno1;
            partialDetalleAuditoria.alterno2 = articulo.alterno2;
            partialDetalleAuditoria.alterno3 = articulo.alterno3;
            partialDetalleAuditoria.costo = articulo.costo;
            partialDetalleAuditoria.precio = articulo.precio;

            return partialDetalleAuditoria;
        }

        private async Task<Articulo> GetArticulo(string articuloNumber, SqlServerDataAccess dbAccess)
        {
            var sql = ConfigurationManager.AppSettings["buscar_articulo_por_numero"];

            return
                await dbAccess.LoadFirstOrDefaultAsync<Articulo, dynamic>(
                    sql, new { no_articulo = articuloNumber });
        }

        #endregion
    }
}
