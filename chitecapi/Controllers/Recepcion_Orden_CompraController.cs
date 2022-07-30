using chitecapi.Responses;
using DataAccess.Models;
using Infrastructure.UnitOfWork;
using MySql.Data.MySqlClient;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

namespace chitecapi.Controllers
{
    public class Recepcion_Orden_CompraController : ChitecApiController
    {
        #region Actions

        /// <summary>
        /// Obtiene la lista de las recepciones de orden de compra.
        /// </summary>
        /// <param name="id_recepcion_orden_compra" example="16">ID de la recepción de orden de compra</param>
        /// <param name="id_orden_compra" example="ABC">ID de la orden de compra</param>
        /// <param name="fecha_inicial" example="2021-09-01">Fecha inicial del rango de búsqueda para las recepciones</param>
        /// <param name="fecha_final" example="2021-09-05">Fecha final del rango de  búsqueda para las recepciones</param>
        /// <param name="no_articulo" example="xyz">ID del artículo dentro de la recepción de orden de compra</param>
        /// <param name="id_ubicacion" example="A01B">ID de la ubicación del artículo</param>
        /// <param name="db" example="db1">ID de la Base de datos</param>
        /// <remarks>
        /// Se utiliza para obtener una lista de recepciones de orden de compra, generado con un formato JSON:
        /// 
        ///     Example Result:
        ///     {
        ///       "data": [
        ///         {
        ///           "fecha": "2021-09-02T00:00:00",
        ///           "id_orden_compra": "abc",
        ///           "id_contenedor": "id_contenedor",
        ///           "numero_factura": "numero_factura",
        ///           "comentario": "comentario",
        ///           "id_usuario": 1,
        ///           "detalles": [
        ///             {
        ///               "no_articulo": "code",
        ///               "cantidad": 4.0,
        ///               "id_ubicacion": "A01A"
        ///             },
        ///             {
        ///               "no_articulo": "codes",
        ///               "cantidad": 1.0,
        ///               "id_ubicacion": "A01A"
        ///             }
        ///           ]
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
        public async Task<IHttpActionResult> Buscar(string id_recepcion_orden_compra, string id_orden_compra, string fecha_inicial, string fecha_final, string no_articulo, string id_ubicacion, string db = "db1")
        {
            var connectionString = GetConnectionString(db);
            string providerName = ConfigurationManager.ConnectionStrings[db].ProviderName;
            IDbConnection conection = providerName == "Npgsql" ? (IDbConnection)new NpgsqlConnection(connectionString) : (providerName == "Mysql" ? (IDbConnection)new MySqlConnection(connectionString) : (providerName == "System.Data.SqlClient" ? (IDbConnection)new SqlConnection(connectionString) : (IDbConnection)new SqlConnection(connectionString)));


            if (string.IsNullOrEmpty(connectionString))
            {
                return new CustomJsonActionResult(
                    HttpStatusCode.NotFound,
                    new JsonErrorResponse(1, 400, $"La base de datos {db} no existe."));
            }

            using (var unitOfWork = new PurchaseOrderUnitOfWork(connectionString,conection))
            {
                var receptionHeaders =
                    await GetPurchaseOrderReceptionHeaders(id_recepcion_orden_compra, id_orden_compra, fecha_inicial, fecha_final, no_articulo, id_ubicacion, unitOfWork);

                var receptions = await GetPurchaseOrderReceptions(receptionHeaders, unitOfWork);
                var data = GetDataJson(receptions);

                return
                    new CustomJsonActionResult(HttpStatusCode.OK, new JsonDataResponse(data));
            }
        }

        /// <summary>
        /// Crea una recepción de orden de compra.
        /// </summary>
        /// <param name="recepciones">Lista de objetos que representan recepciones de orden de compra</param>
        /// <param name="db" example="db1">ID de la Base de datos</param>
        /// <remarks>
        /// Las recepciones se reciben en formato JSON:
        /// [
        ///   {
        ///     "fecha": "2021-09-02",
        ///     "id_orden_compra": "abc",
        ///     "id_contenedor": "id_contenedor",
        ///     "numero_factura": "numero_factura",
        ///     "comentario": "comentario",
        ///     "id_usuario": 1,
        ///     "detalles": [
        ///       {
        ///         "no_articulo": "code",
        ///         "cantidad": 4.0,
        ///         "id_ubicacion": "A01A"
        ///       },
        ///       {
        ///         "no_articulo": "codes",
        ///         "cantidad": 1.0,
        ///         "id_ubicacion": "A01A"
        ///       }
        ///     ]
        ///   }
        /// ]
        /// </remarks>
        /// <response code="401">Unauthorized. Error en la configuracion de la base de datos</response>              
        /// <response code="200">OK. Objetos guardados correctamente.</response>                
        [HttpPost]
        public async Task<IHttpActionResult> Guardar([FromBody] List<RecepcionOrdenCompra> recepciones, string db = "db1")
        {
            var connectionString = GetConnectionString(db);
            string providerName = ConfigurationManager.ConnectionStrings[db].ProviderName;
            IDbConnection conection = providerName == "Npgsql" ? (IDbConnection)new NpgsqlConnection(connectionString) : (providerName == "Mysql" ? (IDbConnection)new MySqlConnection(connectionString) : (providerName == "System.Data.SqlClient" ? (IDbConnection)new SqlConnection(connectionString) : (IDbConnection)new SqlConnection(connectionString)));


            if (string.IsNullOrEmpty(connectionString))
            {
                return new CustomJsonActionResult(
                    HttpStatusCode.NotFound,
                    new JsonErrorResponse(1, 400, $"La base de datos {db} no existe."));
            }

            using (var unitOfWork = new PurchaseOrderUnitOfWork(connectionString,conection))
            {
                foreach (var recepcion in recepciones)
                {
                    if (recepcion?.IsNotInitialized == true)
                    {
                        return new CustomJsonActionResult(
                            HttpStatusCode.NotFound,
                            new JsonErrorResponse(1, 400, "Faltan parámetros"));
                    }

                    try
                    {
                        if (await ReceptionExists(recepcion.id_orden_compra, unitOfWork))
                        {
                            return new CustomJsonActionResult(
                            HttpStatusCode.NotFound,
                            new JsonErrorResponse(1, 400, "La recepción ya existe"));
                        }

                        var receptionHeaderId = await CreateHeader(recepcion, unitOfWork);
                        var isOrderComplete = true;

                        foreach (var detalle in recepcion.detalles)
                        {
                            if (await DetailExists(receptionHeaderId, detalle.no_articulo, unitOfWork))
                            {
                                continue;
                            }

                            if (!await IsItemStatusComplete(detalle, recepcion.id_orden_compra, unitOfWork))
                            {
                                isOrderComplete = false;
                            }

                            await CreateDetail(receptionHeaderId, detalle, unitOfWork);
                        }

                        await UpdatePurchaseOrderStatus(isOrderComplete, recepcion.id_orden_compra, unitOfWork);
                    }
                    catch (Exception exception)
                    {
                        return new CustomJsonActionResult(
                            HttpStatusCode.NotFound,
                            new JsonErrorResponse(1, 1, exception.GetBaseException().Message));
                    }
                }

                unitOfWork.Commit();
            }

            return new CustomJsonActionResult(
                HttpStatusCode.OK,
                new JsonErrorResponse(0, 0, "0"));
        }

        #endregion

        #region Helper methods

        private async Task<IEnumerable<EncabezadoRecepcionOrdenCompra>> GetPurchaseOrderReceptionHeaders(
            string receptionHeaderId,
            string purchaseOrderHeaderId,
            string startDate,
            string endDate,
            string itemId,
            string locationId,
            PurchaseOrderUnitOfWork unitOfWork)
        {
            var sql = ConfigurationManager.AppSettings["buscar_encabezado_recepcion_orden_compra"];

            if (!string.IsNullOrEmpty(receptionHeaderId))
            {
                sql += $" AND encabezado.id = {receptionHeaderId}";
            }

            if (!string.IsNullOrEmpty(purchaseOrderHeaderId))
            {
                sql += $" AND encabezado.id_orden_compra = '{purchaseOrderHeaderId}'";
            }

            if (!string.IsNullOrEmpty(startDate))
            {
                sql += $" AND encabezado.fecha >= '{startDate}'";
            }

            if (!string.IsNullOrEmpty(endDate))
            {
                sql += $" AND encabezado.fecha <= '{endDate}'";
            }

            if (!string.IsNullOrEmpty(itemId))
            {
                sql += $" AND detalle.no_articulo = '{itemId}'";
            }

            if (!string.IsNullOrEmpty(locationId))
            {
                sql += $" AND detalle.id_ubicacion = '{locationId}'";
            }

            return await unitOfWork.EncabezadoRecepcionOrdenCompraRepository
                .LoadAsync(sql, new { });
        }

        private async Task<IEnumerable<RecepcionOrdenCompra>> GetPurchaseOrderReceptions(IEnumerable<EncabezadoRecepcionOrdenCompra> receptionHeaders, PurchaseOrderUnitOfWork unitOfWork)
        {
            var receptions = new List<RecepcionOrdenCompra>();
            foreach (var receptionHeader in receptionHeaders)
            {
                var sql = ConfigurationManager.AppSettings["buscar_recepciones_orden_compra"];
                var parameters = new
                {
                    id_recepcion_orden_compra = receptionHeader.id
                };

                var receptionDetails = await unitOfWork.DetalleRecepcionOrdenCompraRepository.LoadAsync(sql, parameters);
                var reception = new RecepcionOrdenCompra
                {
                    fecha = receptionHeader.fecha,
                    id_orden_compra = receptionHeader.id_orden_compra,
                    id_contenedor = receptionHeader.id_contenedor,
                    fecha_registro = receptionHeader.fecha_registro,
                    numero_factura = receptionHeader.numero_factura,
                    comentario = receptionHeader.comentario,
                    id_usuario = receptionHeader.id_usuario,
                    detalles = new List<DetalleRecepcionOrdenCompra>(receptionDetails)
                };

                receptions.Add(reception);
            }

            return receptions;
        }

        private async Task<bool> ReceptionExists(string purchaseOrderId, PurchaseOrderUnitOfWork unitOfWork)
        {
            var sql = ConfigurationManager.AppSettings["buscar_recepcion_orden_compra_por_id"];
            var parameters = new { id_orden_compra = purchaseOrderId };

            var detail = await unitOfWork.EncabezadoRecepcionOrdenCompraRepository
                .LoadFirstAsync(sql, parameters);

            return detail != null;
        }

        private async Task<int> CreateHeader(RecepcionOrdenCompra recepcion, PurchaseOrderUnitOfWork unitOfWork)
        {
            var sql = ConfigurationManager.AppSettings["insertar_encabezado_orden_compra"];
            var headerParameters = new
            {
                recepcion.fecha,
                recepcion.id_orden_compra,
                recepcion.id_contenedor,
                recepcion.numero_factura,
                recepcion.comentario,
                recepcion.id_usuario
            };

            return await unitOfWork.EncabezadoRecepcionOrdenCompraRepository
                    .SaveAndReturnAsync<int>(sql, headerParameters);
        }

        private async Task<bool> DetailExists(int headerId, string itemNumber, PurchaseOrderUnitOfWork unitOfWork)
        {
            var sql = ConfigurationManager.AppSettings["buscar_detalle_orden_compra_por_encabezado_y_articulo"];
            var detail = await unitOfWork.EncabezadoRecepcionOrdenCompraRepository
                .LoadFirstAsync(sql, new { id_recepcion_orden_compra = headerId, no_articulo = itemNumber });

            return detail != null;
        }

        private async Task CreateDetail(int headerId, DetalleRecepcionOrdenCompra detail, PurchaseOrderUnitOfWork unitOfWork)
        {
            var sql = ConfigurationManager.AppSettings["insertar_detalle_recepcion_oder_compra"];
            var detailParameters = new
            {
                id_recepcion_orden_compra = headerId,
                detail.no_articulo,
                detail.cantidad,
                detail.id_ubicacion
            };

            await unitOfWork.DetalleRecepcionOrdenCompraRepository
                .SaveAsync(sql, detailParameters);
        }

        private async Task<bool> IsItemStatusComplete(DetalleRecepcionOrdenCompra detail, string purchaseOrderId, PurchaseOrderUnitOfWork unitOfWork)
        {
            var isLineItemComplete = true;

            if (!await ValidateItemStatusCompleteness(detail, purchaseOrderId, unitOfWork))
            {
                isLineItemComplete = false;
            }

            return isLineItemComplete;
        }

        private async Task<bool> ValidateItemStatusCompleteness(DetalleRecepcionOrdenCompra detail, string purchaseOrderId, PurchaseOrderUnitOfWork unitOfWork)
        {
            var sql = ConfigurationManager.AppSettings["buscar_detalle_orden_compra_por_id_y_articulo"];
            var parameters = new { DocNum = purchaseOrderId, ItemCode = detail.no_articulo };

            var item = await unitOfWork.PurchaseOrderDetailRepository.LoadFirstAsync(sql, parameters);

            if (item == null)
            {
                throw new Exception($"El artículo: {detail.no_articulo}, no existe en la orden de compra: {purchaseOrderId}.");
            }

            var itemStatus = "completada";
            var isItemStatusComplete = false;

            if (item.Quantity < detail.cantidad)
            {
                itemStatus = "excedente";
                item.LineStatus = itemStatus;
            }
            else if (item.Quantity > detail.cantidad)
            {
                itemStatus = "backorder";
                item.LineStatus = itemStatus;
            }
            else
            {
                item.LineStatus = itemStatus;
                isItemStatusComplete = true;
            }

            await UpdatePurchaseOrderDetailStatus(itemStatus, purchaseOrderId, detail.no_articulo, unitOfWork);

            return isItemStatusComplete;
        }

        private async Task UpdatePurchaseOrderDetailStatus(string itemStatus, string purchaseOrderId, string itemId, PurchaseOrderUnitOfWork unitOfWork)
        {
            var sql = ConfigurationManager.AppSettings["actualizar_estado_articulo_detalle_orden_compra"];
            var parameters = new
            {
                LineStatus = itemStatus,
                DocNum = purchaseOrderId,
                ItemCode = itemId
            };

            await unitOfWork.PurchaseOrderDetailRepository.SaveAsync(sql, parameters);
        }

        private async Task UpdatePurchaseOrderStatus(bool isOrderComplete, string id, PurchaseOrderUnitOfWork unitOfWork)
        {
            var sql = ConfigurationManager.AppSettings["actualizar_estado_orden_compra"];
            var parameters = new
            {
                DocumentStatus = isOrderComplete ? "completada" : "pendiente",
                DocNum = id
            };

            await unitOfWork.PurchaseOrderHeaderRepository.SaveAsync(sql, parameters);
        }

        #endregion
    }
}
