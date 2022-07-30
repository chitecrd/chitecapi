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
using System.Threading.Tasks;
using System.Web.Http;

namespace chitecapi.Controllers
{
    public class PurchaseOrderController : ChitecApiController
    {
        #region Actions

        // GET api/PurchaseOrder/GetOrderBatchList?db=db1
        /// <summary>
        /// Obtiene una lista de todas las ordenes batch registradas por status.
        ///
        /// </summary>
        /// <param name="status" example="pendiente o completada ">Status de la orden(pendiente o completada)</param>
        /// <param name="db" example="db1">The Database ID</param>
        /// <remarks>
        /// Se utiliza para obtener una lista de los articulos en las ordenes recibidas, generado con un formato JSON:
        /// 
        ///     Example Result:
        ///     {
        ///         "data": [
        ///                       {
        ///                            "DocNum": "abc",
        ///                            "DocDate": "2021-07-22T00:00:00",
        ///                            "DocDueDate": "2021-07-30T00:00:00",
        ///                            "CardName": "cardName",
        ///                            "NumAtCard": "xyz",
        ///                            "DocRate": 1,
        ///                            "JournalMemo": "memo",
        ///                            "TaxDate": "2021-07-30T00:00:00",
        ///                            "DocTotalFc": 1,
        ///                            "DocTotalSys": 1.3,
        ///                            "DocumentStatus": "completada",
        ///                            "U_NCF": "123456789",
        ///                            "TotalLines": 2,
        ///                            "Details": [
        ///                            {
        ///                                "DocNum": "abc",
        ///                                "LineNum": 1,
        ///                                "ItemCode": "code",
        ///                                "ItemDescription": "description",
        ///                               "Quantity": 2,
        ///                                "Costo_Unitario": 20,
        ///                                "BarCode": "code",
        ///                                "UoMCode": "cpde",
        ///                                "LineStatus": "pendiente",
        ///                                "UserName": "user",
        ///                                "WarehouseCode": "cpde",
        ///                                "BinLocation": "location",
        ///                                "Costo_Total": 50,
        ///                                "Itbis": 18.2
        ///                            },
        ///                            {
        ///                                "DocNum": "abc",
        ///                                "LineNum": 2,
        ///                                "ItemCode": "codes",
        ///                                "ItemDescription": "description",
        ///                                "Quantity": 2,
        ///                                "Costo_Unitario": 20,
        ///                                "BarCode": "code",
        ///                                "UoMCode": "cpde",
        ///                                "LineStatus": "pendiente",
        ///                                "UserName": "user",
        ///                                "WarehouseCode": "cpde",
        ///                                "BinLocation": "location",
        ///                                "Costo_Total": 50,
        ///                                "Itbis": 17.2
        ///                            }
        ///                            ]
        ///                        }
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
        public async Task<IHttpActionResult> GetOrderBatchList(string status = "todas", string db = "db1")
        {
            var connectionString = GetConnectionString(db);
            string providerName = ConfigurationManager.ConnectionStrings[db].ProviderName;
            IDbConnection conection = providerName == "Npgsql" ? (IDbConnection)new NpgsqlConnection(connectionString) : (providerName == "Mysql" ? (IDbConnection)new MySqlConnection(connectionString) : (providerName == "System.Data.SqlClient" ? (IDbConnection)new SqlConnection(connectionString) : (IDbConnection)new SqlConnection(connectionString)));


            if (string.IsNullOrEmpty(connectionString))
            {
                return new CustomJsonActionResult(
                    System.Net.HttpStatusCode.NotFound,
                    new JsonErrorResponse(1, 400, $"La base de datos {db} no existe."));
            }

            if (string.IsNullOrEmpty(status))
            {
                return new CustomJsonActionResult(
                    System.Net.HttpStatusCode.NotFound,
                    new JsonErrorResponse(1, 400, "Faltan parámetros."));
            }

            using (var unitOfWork = new PurchaseOrderUnitOfWork(connectionString, conection))
            {
                status = status.ToLower();
                if (!status.Equals("pendiente") && !status.Equals("completada") && !status.Equals("todas"))
                {
                    return new CustomJsonActionResult(
                            System.Net.HttpStatusCode.NotFound,
                            new JsonErrorResponse(1, 400, $"El status de orden de compra ({status}) no existe"));
                }

                var orderHeaders = await GetOrderHeadersBatch(status, unitOfWork);
                var orders = await GetOrdersBatch(orderHeaders, unitOfWork);
                var data = GetDataJson(orders);

                return
                    new CustomJsonActionResult(System.Net.HttpStatusCode.OK, new JsonDataResponse(data));
            }
        }

        // GET api/PurchaseOrder/GetOrderList?db=db1
        /// <summary>
        /// Obtiene una lista de todas las ordenes  registradas por status.
        /// </summary>
        /// <param name="status" example="pendiente o completada ">Status de la orden(pendiente o completada)</param>
        /// <param name="db" example="db1">The Database ID</param>
        /// <remarks>
        ///  WebConfig Items: get_purchase_order_headers_all,get_purchase_order_headers,
        /// get_purchase_order_by_doc,get_purchase_order_details,get_purchase_order_details_by_line_number
        /// 
        /// Se utiliza para obtener una lista de los articulos en las ordenes, generado con un formato JSON:
        /// 
        ///     Example Result:
        ///     {
        ///         "data": [
        ///                       {
        ///                            "DocNum": "abc",
        ///                            "DocDate": "2021-07-22T00:00:00",
        ///                            "DocDueDate": "2021-07-30T00:00:00",
        ///                            "CardName": "cardName",
        ///                            "NumAtCard": "xyz",
        ///                            "DocRate": 1,
        ///                            "JournalMemo": "memo",
        ///                            "TaxDate": "2021-07-30T00:00:00",
        ///                            "DocTotalFc": 1,
        ///                            "DocTotalSys": 1.3,
        ///                            "DocumentStatus": "completada",
        ///                            "U_NCF": "123456789",
        ///                            "TotalLines": 2,
        ///                            "Details": [
        ///                            {
        ///                                "DocNum": "abc",
        ///                                "LineNum": 1,
        ///                                "ItemCode": "code",
        ///                                "ItemDescription": "description",
        ///                               "Quantity": 2,
        ///                                "Costo_Unitario": 20,
        ///                                "BarCode": "code",
        ///                                "UoMCode": "cpde",
        ///                                "LineStatus": "pendiente",
        ///                                "UserName": "user",
        ///                                "WarehouseCode": "cpde",
        ///                                "BinLocation": "location",
        ///                                "Costo_Total": 50,
        ///                                "QuantityConfirm": 50,
        ///                                "Itbis": 18.2
        ///                            },
        ///                            {
        ///                                "DocNum": "abc",
        ///                                "LineNum": 2,
        ///                                "ItemCode": "codes",
        ///                                "ItemDescription": "description",
        ///                                "Quantity": 2,
        ///                                "Costo_Unitario": 20,
        ///                                "BarCode": "code",
        ///                                "UoMCode": "cpde",
        ///                                "LineStatus": "pendiente",
        ///                                "UserName": "user",
        ///                                "WarehouseCode": "cpde",
        ///                                "BinLocation": "location",
        ///                                "Costo_Total": 50,
        ///                                "QuantityConfirm": 50,
        ///                                "Itbis": 17.2
        ///                            }
        ///                            ]
        ///                        }
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
        public async Task<IHttpActionResult> GetOrderList(string status = "todas", string db = "db1")
        {
            var connectionString = GetConnectionString(db);
            string providerName = ConfigurationManager.ConnectionStrings[db].ProviderName;
            IDbConnection conection = providerName == "Npgsql" ? (IDbConnection)new NpgsqlConnection(connectionString) : (providerName == "Mysql" ? (IDbConnection)new MySqlConnection(connectionString) : (providerName == "System.Data.SqlClient" ? (IDbConnection)new SqlConnection(connectionString) : (IDbConnection)new SqlConnection(connectionString)));


            if (string.IsNullOrEmpty(connectionString))
            {
                return new CustomJsonActionResult(
                    System.Net.HttpStatusCode.NotFound,
                    new JsonErrorResponse(1, 400, $"La base de datos {db} no existe."));
            }

            if (string.IsNullOrEmpty(status))
            {
                return new CustomJsonActionResult(
                    System.Net.HttpStatusCode.NotFound,
                    new JsonErrorResponse(1, 400, "Faltan parámetros."));
            }

            using (var unitOfWork = new PurchaseOrderUnitOfWork(connectionString,conection))
            {
                status = status.ToLower();
                if (!status.Equals("pendiente") && !status.Equals("completada") && !status.Equals("todas"))
                {
                    return new CustomJsonActionResult(
                            System.Net.HttpStatusCode.NotFound,
                            new JsonErrorResponse(1, 400, $"El status de orden de compra ({status}) no existe"));
                }

                var orderHeaders = await GetOrderHeaders(status, unitOfWork);
                var orders = await GetOrders(orderHeaders, unitOfWork);
                var data = GetDataJson(orders);

                return
                    new CustomJsonActionResult(System.Net.HttpStatusCode.OK, new JsonDataResponse(data));
            }
        }

        [HttpPost]
        public async Task<IHttpActionResult> GuardarOrderBath([FromBody] List<PurchaseOrder> orders, string db = "db1")
        {
            var connectionString = GetConnectionString(db);
            string providerName = ConfigurationManager.ConnectionStrings[db].ProviderName;
            IDbConnection conection = providerName == "Npgsql" ? (IDbConnection)new NpgsqlConnection(connectionString) : (providerName == "Mysql" ? (IDbConnection)new MySqlConnection(connectionString) : (providerName == "System.Data.SqlClient" ? (IDbConnection)new SqlConnection(connectionString) : (IDbConnection)new SqlConnection(connectionString)));


            if (string.IsNullOrEmpty(connectionString))
            {
                return new CustomJsonActionResult(
                    System.Net.HttpStatusCode.NotFound,
                    new JsonErrorResponse(1, 400, $"La base de datos {db} no existe."));
            }

            using (var unitOfWork = new PurchaseOrderUnitOfWork(connectionString, conection))
            {
                foreach (var order in orders)
                {
                    if (order?.IsNotInitialized == true)
                    {
                        return new CustomJsonActionResult(
                            System.Net.HttpStatusCode.NotFound,
                            new JsonErrorResponse(1, 400, "Faltan parámetros"));
                    }

                    try
                    {
                        if (!await OrderBatchDoNotExist(order.DocNum, unitOfWork))
                        {
                            return new CustomJsonActionResult(
                            System.Net.HttpStatusCode.NotFound,
                            new JsonErrorResponse(1, 400, "Order ya existe"));
                        }

                        await CreateHeaderBatch(order, unitOfWork);

                        foreach (var detail in order.Details)
                        {
                            if (!await DetailBatchDoNotExist(order.DocNum, detail.LineNum, unitOfWork))
                            {
                                continue;
                            }

                            await CreateDetailBatch(order.DocNum, detail, unitOfWork);
                        }
                    }
                    catch (Exception exception)
                    {
                        return new CustomJsonActionResult(
                            System.Net.HttpStatusCode.NotFound,
                            new JsonErrorResponse(1, 1, exception.GetBaseException().Message));
                    }
                }

                unitOfWork.Commit();
            }

            return new CustomJsonActionResult(
                System.Net.HttpStatusCode.OK,
                new JsonErrorResponse(0, 0, "0"));
        }

        [HttpPost]
        public async Task<IHttpActionResult> GuardarOrder([FromBody] List<PurchaseOrder> orders, string db = "db1")
        {
            var connectionString = GetConnectionString(db);
            string providerName = ConfigurationManager.ConnectionStrings[db].ProviderName;
            IDbConnection conection = providerName == "Npgsql" ? (IDbConnection)new NpgsqlConnection(connectionString) : (providerName == "Mysql" ? (IDbConnection)new MySqlConnection(connectionString) : (providerName == "System.Data.SqlClient" ? (IDbConnection)new SqlConnection(connectionString) : (IDbConnection)new SqlConnection(connectionString)));


            if (string.IsNullOrEmpty(connectionString))
            {
                return new CustomJsonActionResult(
                    System.Net.HttpStatusCode.NotFound,
                    new JsonErrorResponse(1, 400, $"La base de datos {db} no existe."));
            }

            using (var unitOfWork = new PurchaseOrderUnitOfWork(connectionString, conection))
            {
                foreach (var order in orders)
                {
                    if (order?.IsNotInitialized == true)
                    {
                        return new CustomJsonActionResult(
                            System.Net.HttpStatusCode.NotFound,
                            new JsonErrorResponse(1, 400, "Faltan parámetros"));
                    }

                    try
                    {
                        if (!await OrderDoNotExist(order.DocNum, unitOfWork))
                        {
                            return new CustomJsonActionResult(
                            System.Net.HttpStatusCode.NotFound,
                            new JsonErrorResponse(1, 400, "Order ya existe"));
                        }

                        await CreateHeader(order, unitOfWork);

                        foreach (var detail in order.Details)
                        {
                            if (!await DetailDoNotExist(order.DocNum, detail.LineNum, unitOfWork))
                            {
                                continue;
                            }

                            await CreateDetail(order.DocNum, detail, unitOfWork);
                        }
                    }
                    catch (Exception exception)
                    {
                        return new CustomJsonActionResult(
                            System.Net.HttpStatusCode.NotFound,
                            new JsonErrorResponse(1, 1, exception.GetBaseException().Message));
                    }
                }

                unitOfWork.Commit();
            }

            return new CustomJsonActionResult(
                System.Net.HttpStatusCode.OK,
                new JsonErrorResponse(0, 0, "0"));
        }

        [HttpPost]
        public async Task<IHttpActionResult> EditarOrderBathPendiente([FromBody] List<PurchaseOrder> orders, string db = "db1")
        {
            var connectionString = GetConnectionString(db);
            string providerName = ConfigurationManager.ConnectionStrings[db].ProviderName;
            IDbConnection conection = providerName == "Npgsql" ? (IDbConnection)new NpgsqlConnection(connectionString) : (providerName == "Mysql" ? (IDbConnection)new MySqlConnection(connectionString) : (providerName == "System.Data.SqlClient" ? (IDbConnection)new SqlConnection(connectionString) : (IDbConnection)new SqlConnection(connectionString)));


            if (string.IsNullOrEmpty(connectionString))
            {
                return new CustomJsonActionResult(
                    System.Net.HttpStatusCode.NotFound,
                    new JsonErrorResponse(1, 400, $"La base de datos {db} no existe."));
            }

            using (var unitOfWork = new PurchaseOrderUnitOfWork(connectionString, conection))
            {
                foreach (var order in orders)
                {
                    if (order?.IsNotInitialized == true)
                    {
                        return new CustomJsonActionResult(
                            System.Net.HttpStatusCode.NotFound,
                            new JsonErrorResponse(1, 400, "Faltan parámetros"));
                    }

                    try
                    {
                        var pendingHeader = await GetPendingHeaderBatch(order.DocNum, unitOfWork);

                        if (pendingHeader == null)
                        {
                            continue;
                        }

                        await UpdateHeaderBatch(order, unitOfWork);

                        foreach (var detail in order.Details)
                        {
                            if (await DetailBatchDoNotExist(order.DocNum, detail.LineNum, unitOfWork))
                            {
                                continue;
                            }

                            await UpdateDetailBatch(order.DocNum, detail, unitOfWork);
                        }
                    }
                    catch (Exception exception)
                    {
                        return new CustomJsonActionResult(
                            System.Net.HttpStatusCode.NotFound,
                            new JsonErrorResponse(1, 1, exception.GetBaseException().Message));
                    }
                }

                unitOfWork.Commit();
            }

            return new CustomJsonActionResult(
                System.Net.HttpStatusCode.OK,
                new JsonErrorResponse(0, 0, "0"));
        }

        [HttpPost]
        public async Task<IHttpActionResult> EditarOrderPendiente([FromBody] List<PurchaseOrder> orders, string db = "db1")
        {
            var connectionString = GetConnectionString(db);
            string providerName = ConfigurationManager.ConnectionStrings[db].ProviderName;
            IDbConnection conection = providerName == "Npgsql" ? (IDbConnection)new NpgsqlConnection(connectionString) : (providerName == "Mysql" ? (IDbConnection)new MySqlConnection(connectionString) : (providerName == "System.Data.SqlClient" ? (IDbConnection)new SqlConnection(connectionString) : (IDbConnection)new SqlConnection(connectionString)));


            if (string.IsNullOrEmpty(connectionString))
            {
                return new CustomJsonActionResult(
                    System.Net.HttpStatusCode.NotFound,
                    new JsonErrorResponse(1, 400, $"La base de datos {db} no existe."));
            }

            using (var unitOfWork = new PurchaseOrderUnitOfWork(connectionString, conection))
            {
                foreach (var order in orders)
                {
                    if (order?.IsNotInitialized == true)
                    {
                        return new CustomJsonActionResult(
                            System.Net.HttpStatusCode.NotFound,
                            new JsonErrorResponse(1, 400, "Faltan parámetros"));
                    }

                    try
                    {
                        var pendingHeader = await GetPendingHeader(order.DocNum, unitOfWork);

                        if (pendingHeader == null)
                        {
                            continue;
                        }

                        await UpdateHeader(order, unitOfWork);

                        foreach (var detail in order.Details)
                        {
                            if (await DetailDoNotExist(order.DocNum, detail.LineNum, unitOfWork))
                            {
                                continue;
                            }

                            await UpdateDetail(order.DocNum, detail, unitOfWork);
                        }
                    }
                    catch (Exception exception)
                    {
                        return new CustomJsonActionResult(
                            System.Net.HttpStatusCode.NotFound,
                            new JsonErrorResponse(1, 1, exception.GetBaseException().Message));
                    }
                }

                unitOfWork.Commit();
            }

            return new CustomJsonActionResult(
                System.Net.HttpStatusCode.OK,
                new JsonErrorResponse(0, 0, "0"));
        }



        #endregion

        #region Helper methods

        private async Task<IEnumerable<PurchaseOrder>> GetOrdersBatch(IEnumerable<PurchaseOrderHeader> orderHeaders, PurchaseOrderUnitOfWork unitOfWork)
        {
            var orders = new List<PurchaseOrder>();
            foreach (var header in orderHeaders)
            {
                var sql = ConfigurationManager.AppSettings["get_purchase_order_details_batch"];
                var orderDetails = await unitOfWork.PurchaseOrderDetailRepository.LoadAsync(sql, new { header.DocNum });
                var order = new PurchaseOrder
                {
                    DocNum = header.DocNum,
                    DocDate = header.DocDate,
                    DocDueDate = header.DocDueDate,
                    DocRate = header.DocRate,
                    DocTotalFc = header.DocTotalFc,
                    DocTotalSys = header.DocTotalSys,
                    DocumentStatus = header.DocumentStatus,
                    TaxDate = header.TaxDate,
                    TotalLines = header.TotalLines,
                    CardName = header.CardName,
                    CardCode = header.CardCode,
                    NumAtCard = header.NumAtCard,
                    JournalMemo = header.JournalMemo,
                    U_NCF = header.U_NCF,
                    Details = new List<PurchaseOrderDetail>(orderDetails)
                };

                orders.Add(order);
            }

            return orders;
        }

        private async Task<IEnumerable<PurchaseOrder>> GetOrders(IEnumerable<PurchaseOrderHeader> orderHeaders, PurchaseOrderUnitOfWork unitOfWork)
        {
            var orders = new List<PurchaseOrder>();
            foreach (var header in orderHeaders)
            {
                var sql = ConfigurationManager.AppSettings["get_purchase_order_details"];
                var orderDetails = await unitOfWork.PurchaseOrderDetailRepository.LoadAsync(sql, new { header.DocNum });
                var order = new PurchaseOrder
                {
                    DocNum = header.DocNum,
                    DocDate = header.DocDate,
                    DocDueDate = header.DocDueDate,
                    DocRate = header.DocRate,
                    DocTotalFc = header.DocTotalFc,
                    DocTotalSys = header.DocTotalSys,
                    DocumentStatus = header.DocumentStatus,
                    TaxDate = header.TaxDate,
                    TotalLines = header.TotalLines,
                    CardName = header.CardName,
                    CardCode = header.CardCode,
                    NumAtCard = header.NumAtCard,
                    JournalMemo = header.JournalMemo,
                    U_NCF = header.U_NCF,
                    Details = new List<PurchaseOrderDetail>(orderDetails)
                };

                orders.Add(order);
            }

            return orders;
        }


        private async Task<IEnumerable<PurchaseOrderHeader>> GetOrderHeadersBatch(string status, PurchaseOrderUnitOfWork unitOfWork)
        {
            var sql = ConfigurationManager.AppSettings["get_purchase_order_headers_batch"];

            if (status.Equals("todas"))
            {
                sql = ConfigurationManager.AppSettings["get_purchase_order_headers_all_batch"];
                return await unitOfWork.PurchaseOrderHeaderRepository
               .LoadAsync(sql, new { });
            }

            return await unitOfWork.PurchaseOrderHeaderRepository
                .LoadAsync(sql, new { DocumentStatus = status });
        }

        private async Task<IEnumerable<PurchaseOrderHeader>> GetOrderHeaders(string status, PurchaseOrderUnitOfWork unitOfWork)
        {
            var sql = ConfigurationManager.AppSettings["get_purchase_order_headers"];

            if (status.Equals("todas"))
            {
                sql = ConfigurationManager.AppSettings["get_purchase_order_headers_all"];
                return await unitOfWork.PurchaseOrderHeaderRepository
               .LoadAsync(sql, new { });
            }

            return await unitOfWork.PurchaseOrderHeaderRepository
                .LoadAsync(sql, new { DocumentStatus = status });
        }


        private async Task<bool> DetailBatchDoNotExist(string docNum, int lineNum, PurchaseOrderUnitOfWork unitOfWork)
        {
            var sql = ConfigurationManager.AppSettings["get_purchase_order_details_by_line_number_batch"];
            var detail = await unitOfWork.PurchaseOrderDetailRepository
                .LoadFirstAsync(sql, new { DocNum = docNum, LineNum = lineNum });

            return detail == null;
        }


        private async Task<bool> DetailDoNotExist(string docNum, int lineNum, PurchaseOrderUnitOfWork unitOfWork)
        {
            var sql = ConfigurationManager.AppSettings["get_purchase_order_details_by_line_number"];
            var detail = await unitOfWork.PurchaseOrderDetailRepository
                .LoadFirstAsync(sql, new { DocNum = docNum, LineNum = lineNum });

            return detail == null;
        }


        private async Task<bool> OrderBatchDoNotExist(string docNum, PurchaseOrderUnitOfWork unitOfWork)
        {
            var sql = ConfigurationManager.AppSettings["get_purchase_order_by_doc_batch"];
            var detail = await unitOfWork.PurchaseOrderHeaderRepository
                .LoadFirstAsync(sql, new { DocNum = docNum });

            return detail == null;
        }

        private async Task<bool> OrderDoNotExist(string docNum, PurchaseOrderUnitOfWork unitOfWork)
        {
            var sql = ConfigurationManager.AppSettings["get_purchase_order_by_doc"];
            var detail = await unitOfWork.PurchaseOrderHeaderRepository
                .LoadFirstAsync(sql, new { DocNum = docNum });

            return detail == null;
        }


        private async Task<PurchaseOrderHeader> GetPendingHeaderBatch(string docNum, PurchaseOrderUnitOfWork unitOfWork)
        {
            var sql = ConfigurationManager.AppSettings["get_pending_purchase_order_header_batch"];
            return await unitOfWork.PurchaseOrderHeaderRepository
                .LoadFirstAsync(sql, new { DocNum = docNum });
        }



        private async Task<PurchaseOrderHeader> GetPendingHeader(string docNum, PurchaseOrderUnitOfWork unitOfWork)
        {
            var sql = ConfigurationManager.AppSettings["get_pending_purchase_order_header"];
            return await unitOfWork.PurchaseOrderHeaderRepository
                .LoadFirstAsync(sql, new { DocNum = docNum });
        }


        private async Task UpdateDetailBatch(string docNum, PurchaseOrderDetail detail, PurchaseOrderUnitOfWork unitOfWork)
        {
            var detailParameters = new
            {
                DocNum = docNum,
                detail.LineNum,
                detail.ItemCode,
                detail.ItemDescription,
                detail.Quantity,
                detail.Costo_Unitario,
                detail.BarCode,
                detail.UoMCode,
                detail.LineStatus,
                detail.UserName,
                detail.WarehouseCode,
                detail.BinLocation,
                detail.Costo_Total,
                detail.Itbis
            };

            var sql = ConfigurationManager.AppSettings["update_purchase_order_batch_detail"];
            await unitOfWork.PurchaseOrderDetailRepository
                .SaveAsync(sql, detailParameters);
        }



        private async Task UpdateDetail(string docNum, PurchaseOrderDetail detail, PurchaseOrderUnitOfWork unitOfWork)
        {
            var detailParameters = new
            {
                DocNum = docNum,
                detail.LineNum,
                detail.ItemCode,
                detail.ItemDescription,
                detail.Quantity,
                detail.Costo_Unitario,
                detail.BarCode,
                detail.UoMCode,
                detail.LineStatus,
                detail.UserName,
                detail.WarehouseCode,
                detail.BinLocation,
                detail.Costo_Total,
                detail.QuantityConfirm,
                detail.Itbis
            };

            var sql = ConfigurationManager.AppSettings["update_purchase_order_detail"];
            await unitOfWork.PurchaseOrderDetailRepository
                .SaveAsync(sql, detailParameters);
        }


        private async Task UpdateHeaderBatch(PurchaseOrder purchaseOrder, PurchaseOrderUnitOfWork unitOfWork)
        {
            var headerParameters = new
            {
                purchaseOrder.DocNum,
                purchaseOrder.DocDate,
                purchaseOrder.DocDueDate,
                purchaseOrder.CardName,
                purchaseOrder.CardCode,
                purchaseOrder.NumAtCard,
                purchaseOrder.DocRate,
                purchaseOrder.JournalMemo,
                purchaseOrder.TaxDate,
                purchaseOrder.DocTotalFc,
                purchaseOrder.DocTotalSys,
                purchaseOrder.DocumentStatus,
                purchaseOrder.U_NCF,
                purchaseOrder.TotalLines
            };

            var sql = ConfigurationManager.AppSettings["update_purchase_order_batch_header"];
            await unitOfWork.PurchaseOrderHeaderRepository
                    .SaveAsync(sql, headerParameters);
        }

        private async Task UpdateHeader(PurchaseOrder purchaseOrder, PurchaseOrderUnitOfWork unitOfWork)
        {
            var headerParameters = new
            {
                purchaseOrder.DocNum,
                purchaseOrder.DocDate,
                purchaseOrder.DocDueDate,
                purchaseOrder.CardName,
                purchaseOrder.CardCode,
                purchaseOrder.NumAtCard,
                purchaseOrder.DocRate,
                purchaseOrder.JournalMemo,
                purchaseOrder.TaxDate,
                purchaseOrder.DocTotalFc,
                purchaseOrder.DocTotalSys,
                purchaseOrder.DocumentStatus,
                purchaseOrder.U_NCF,
                purchaseOrder.TotalLines
            };

            var sql = ConfigurationManager.AppSettings["update_purchase_order_header"];
            await unitOfWork.PurchaseOrderHeaderRepository
                    .SaveAsync(sql, headerParameters);
        }


        private async Task CreateHeaderBatch(PurchaseOrder purchaseOrder, PurchaseOrderUnitOfWork unitOfWork)
        {
            var headerParameters = new
            {
                purchaseOrder.DocNum,
                purchaseOrder.DocDate,
                purchaseOrder.DocDueDate,
                purchaseOrder.CardName,
                purchaseOrder.CardCode,
                purchaseOrder.NumAtCard,
                purchaseOrder.DocRate,
                purchaseOrder.JournalMemo,
                purchaseOrder.TaxDate,
                purchaseOrder.DocTotalFc,
                purchaseOrder.DocTotalSys,
                purchaseOrder.DocumentStatus,
                purchaseOrder.U_NCF,
                purchaseOrder.TotalLines
            };

            var sql = ConfigurationManager.AppSettings["insert_purchase_order_batch_header"];
            await unitOfWork.PurchaseOrderHeaderRepository
                    .SaveAsync(sql, headerParameters);
        }


        private async Task CreateHeader(PurchaseOrder purchaseOrder, PurchaseOrderUnitOfWork unitOfWork)
        {
            var headerParameters = new
            {
                purchaseOrder.DocNum,
                purchaseOrder.DocDate,
                purchaseOrder.DocDueDate,
                purchaseOrder.CardName,
                purchaseOrder.CardCode,
                purchaseOrder.NumAtCard,
                purchaseOrder.DocRate,
                purchaseOrder.JournalMemo,
                purchaseOrder.TaxDate,
                purchaseOrder.DocTotalFc,
                purchaseOrder.DocTotalSys,
                purchaseOrder.DocumentStatus,
                purchaseOrder.U_NCF,
                purchaseOrder.TotalLines
            };

            var sql = ConfigurationManager.AppSettings["insert_purchase_order_header"];
            await unitOfWork.PurchaseOrderHeaderRepository
                    .SaveAsync(sql, headerParameters);
        }

        private async Task CreateDetailBatch(string docNum, PurchaseOrderDetail detail, PurchaseOrderUnitOfWork unitOfWork)
        {
            var detailParameters = new
            {
                DocNum = docNum,
                detail.LineNum,
                detail.ItemCode,
                detail.ItemDescription,
                detail.Quantity,
                detail.Costo_Unitario,
                detail.BarCode,
                detail.UoMCode,
                detail.LineStatus,
                detail.UserName,
                detail.WarehouseCode,
                detail.BinLocation,
                detail.Costo_Total,
                detail.Itbis
            };

            var sql = ConfigurationManager.AppSettings["insert_purchase_order_batch_detail"];
            await unitOfWork.PurchaseOrderDetailRepository
                .SaveAsync(sql, detailParameters);
        }


        private async Task CreateDetail(string docNum, PurchaseOrderDetail detail, PurchaseOrderUnitOfWork unitOfWork)
        {
            var detailParameters = new
            {
                DocNum = docNum,
                detail.LineNum,
                detail.ItemCode,
                detail.ItemDescription,
                detail.Quantity,
                detail.Costo_Unitario,
                detail.BarCode,
                detail.UoMCode,
                detail.LineStatus,
                detail.UserName,
                detail.WarehouseCode,
                detail.BinLocation,
                detail.Costo_Total,
                detail.QuantityConfirm,
                detail.Itbis
            };

            var sql = ConfigurationManager.AppSettings["insert_purchase_order_detail"];
            await unitOfWork.PurchaseOrderDetailRepository
                .SaveAsync(sql, detailParameters);
        }

        #endregion        
    }
}
