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
using Infrastructure.UnitOfWork;
using MySql.Data.MySqlClient;
using Npgsql;


namespace chitecapi.Controllers
{
    public class TransferStockController : ChitecApiController
    {
        #region Actions


        /// <summary>
        /// Guardar Transfer Stock.
        /// </summary>
        /// <param name="db" example="db1">The Database ID</param>
        /// <remarks>
        /// Se utiliza para Guardar multiples Entradas de Picking, Enviando un json con la información de la captura
        /// </remarks>
        /// <response code="401">Unauthorized. Error en la configuracion de la base de datos</response>              
        /// <response code="200">OK. Devuelve el objeto solicitado.</response>        
        /// <response code="404">NotFound. No se ha encontrado el objeto solicitado.</response>
        [HttpPost]
        public async Task<IHttpActionResult> Guardar([FromBody] List<StockTranfersHeader> headers, string db)
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

            using (var unitOfWork = new TransferStockUnitOfWork(connectionString, conection))
            {
                foreach (var transfer in headers)
                {
                    if (transfer?.IsNotInitialized == true)
                    {
                        return new CustomJsonActionResult(
                            System.Net.HttpStatusCode.NotFound,
                            new JsonErrorResponse(1, 400, "Faltan parámetros"));
                    }

                    try
                    {
                        if (!await TransferHeaderDoNotExist(transfer.DocEntry, unitOfWork))
                        {
                            await TransferEditHeader(transfer, unitOfWork);
                        }
                        else
                        {
                            await TransferCreateHeader(transfer, unitOfWork);
                        }


                        foreach (var detail in transfer.DocumentLines)
                        {
                            if (!await TransferDetailDoNotExist(transfer.DocEntry, detail.LineNum, unitOfWork))
                            {
                                await TransferEditDetail(transfer.DocEntry, detail, unitOfWork);
                            }
                            else
                            {
                                await TransferCreateDetail(transfer.DocEntry, detail, unitOfWork);
                            }

                        }
                    }
                    catch (Exception exception)
                    {
                        return new CustomJsonActionResult(
                            System.Net.HttpStatusCode.NotFound,
                            new JsonErrorResponse(1, 1, exception.Message));
                    }
                }

                unitOfWork.Commit();
            }

            return new CustomJsonActionResult(
                System.Net.HttpStatusCode.OK,
                new JsonErrorResponse(0, 0, "0"));
        }



        // GET api/sap/ListaTransferStockSap?db=db1
        /// <summary>
        /// Obtiene una lista de todas los transfer listados por fecha.
        /// </summary>
        /// <param name="date" example="">Fecha en la que se creo el conduce</param>
        /// <param name="db" example="db1">The Database ID</param>
        /// <remarks>
        /// Se utiliza para obtener una lista de los conduces, generado con un formato JSON:
        /// 
        ///     Example Result:
        ///     {
        ///         "data": [
        ///                       {
        ///                            "DocEntry": "abc",
        ///                            "DocDate": "2021-07-22",
        ///                            "DueDate": "2021-07-30",
        ///                            "CardCode": "2233",
        ///                            "CardName": "cardName",
        ///                            "JournalMemo": "memo",
        ///                            "FromWarehouse": "xyz",
        ///                            "ToWarehouse": "1",
        ///                            "CreationDate": "2021-07-30",
        ///                            "UpdateDate": "2021-07-30",
        ///                            "TaxDate": "2021-07-30",
        ///                            "DocumentStatus": "completada",
        ///                            "User": "user1",
        ///                            "DocumentLines": [
        ///                            {
        ///                                "DocEntry": "abc",
        ///                                "LineNum": "1",
        ///                                "ItemCode": "code",
        ///                                "ItemDescription": "description",
        ///                                "Quantity": "2",
        ///                                "Price": "20",
        ///                                "Currency": "RD",
        ///                                "SerialNumber": "cpde",
        ///                                "FromWarehouseCode": "pendiente",
        ///                                "UnitsOfMeasurment": "LB",
        ///                                "WarehouseCode": "cpde",
        ///                                "UoMCode": "22",
        ///                                "InventoryQuantity": "50",
        ///                                "LineStatus": "completada"
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
        public async Task<IHttpActionResult> ListaTransferStock(string db = "db1", string date = "")
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


            using (var unitOfWork = new TransferStockUnitOfWork(connectionString, conection))
            {

                var orderHeaders = await GetTransferHeaders(unitOfWork, date);
                var orders = await GetTransfer(orderHeaders, unitOfWork);
                var data = GetDataJson(orders);

                return
                    new CustomJsonActionResult(System.Net.HttpStatusCode.OK, new JsonDataResponse(data));
            }
        }

        
        #endregion

        #region Helper methods

        //Transfer Section

        private async Task<IEnumerable<StockTranfersHeader>> GetTransfer(IEnumerable<StockTranfersHeader> Headers, TransferStockUnitOfWork unitOfWork)
        {
            var orders = new List<StockTranfersHeader>();
            foreach (var header in Headers)
            {
                var sql = ConfigurationManager.AppSettings["get_transfer_details"];
                var orderDetails = await unitOfWork.TransferStockDetailRepository.LoadAsync(sql, new { header.DocEntry });
                var order = new StockTranfersHeader
                {
                    DocEntry = header.DocEntry,
                    DueDate = header.DueDate,
                    DocDate = header.DocDate,
                    CardCode = header.CardCode,
                    CardName = header.CardName,
                    JournalMemo = header.JournalMemo,
                    FromWarehouse = header.FromWarehouse,
                    ToWarehouse = header.ToWarehouse,
                    CreationDate = header.CreationDate,
                    UpdateDate = header.UpdateDate,
                    TaxDate = header.TaxDate,
                    DocumentStatus = header.DocumentStatus,
                    User = header.User,
                    M_SYN = header.M_SYN,
                    DocumentLines = new List<StockTranfersDetail>(orderDetails)
                };


                orders.Add(order);
            }

            return orders;
        }


        private async Task<IEnumerable<StockTranfersHeader>> GetTransferHeaders(TransferStockUnitOfWork unitOfWork, string date = "")
        {
            var sql = ConfigurationManager.AppSettings["get_transfer_headers"];

            if (!date.Equals(""))
            {
                sql = sql + $" and  cast(DocDate as varchar)='{date}'";
            }

            return await unitOfWork.TransferStockHeaderRepository
                .LoadAsync(sql, new { });
        }




        private async Task<bool> TransferDetailDoNotExist(string docNum, string lineNum, TransferStockUnitOfWork unitOfWork)
        {
            var sql = ConfigurationManager.AppSettings["get_transfer_details_by_line_number"];
            var detail = await unitOfWork.TransferStockDetailRepository
                .LoadFirstAsync(sql, new { DocEntry = docNum, LineNum = lineNum });

            return detail == null;
        }


        private async Task<bool> TransferHeaderDoNotExist(string docNum, TransferStockUnitOfWork unitOfWork)
        {
            var sql = ConfigurationManager.AppSettings["get_transfer_by_doc"];
            var detail = await unitOfWork.TransferStockHeaderRepository
                .LoadFirstAsync(sql, new { DocEntry = docNum });

            return detail == null;
        }




        private async Task TransferEditHeader(StockTranfersHeader header, TransferStockUnitOfWork unitOfWork)
        {
            var headerParameters = new
            {
                DocEntry = header.DocEntry,
                DueDate = header.DueDate,
                DocDate = header.DocDate,
                CardCode = header.CardCode,
                CardName = header.CardName,
                JournalMemo = header.JournalMemo,
                FromWarehouse = header.FromWarehouse,
                ToWarehouse = header.ToWarehouse,
                CreationDate = header.CreationDate,
                UpdateDate = header.UpdateDate,
                TaxDate = header.TaxDate,
                DocumentStatus = header.DocumentStatus,
                User = header.User
            };

            var sql = ConfigurationManager.AppSettings["update_transfer_header"];
            await unitOfWork.TransferStockHeaderRepository
                    .SaveAsync(sql, headerParameters);
        }


        private async Task TransferCreateHeader(StockTranfersHeader header, TransferStockUnitOfWork unitOfWork)
        {
            var headerParameters = new
            {
                DocEntry = header.DocEntry,
                DueDate = header.DueDate,
                DocDate = header.DocDate,
                CardCode = header.CardCode,
                CardName = header.CardName,
                JournalMemo = header.JournalMemo,
                FromWarehouse = header.FromWarehouse,
                ToWarehouse = header.ToWarehouse,
                CreationDate = header.CreationDate,
                UpdateDate = header.UpdateDate,
                TaxDate = header.TaxDate,
                DocumentStatus = header.DocumentStatus,
                User = header.User
            };

            var sql = ConfigurationManager.AppSettings["insert_transfer_header"];
            await unitOfWork.TransferStockHeaderRepository
                    .SaveAsync(sql, headerParameters);
        }


        private async Task TransferCreateDetail(string docNum, StockTranfersDetail detail, TransferStockUnitOfWork unitOfWork)
        {
            var detailParameters = new
            {
                DocEntry = docNum,
                LineNum = detail.LineNum,
                ItemCode = detail.ItemCode,
                ItemDescription = detail.ItemDescription,
                Quantity = detail.Quantity,
                Price = detail.Price,
                SerialNumber = detail.SerialNumber,
                Currency = detail.Currency,
                WarehouseCode = detail.WarehouseCode,
                FromWarehouseCode = detail.FromWarehouseCode,
                UnitsOfMeasurment = detail.UnitsOfMeasurment,
                LineStatus = detail.LineStatus,
                UoMCode = detail.UoMCode,
                InventoryQuantity = detail.InventoryQuantity
            };

            var sql = ConfigurationManager.AppSettings["insert_transfer_detail"];
            await unitOfWork.TransferStockDetailRepository
                .SaveAsync(sql, detailParameters);
        }



        private async Task TransferEditDetail(string docNum, StockTranfersDetail detail, TransferStockUnitOfWork unitOfWork)
        {
            var detailParameters = new
            {
                DocEntry = docNum,
                LineNum = detail.LineNum,
                ItemCode = detail.ItemCode,
                ItemDescription = detail.ItemDescription,
                Quantity = detail.Quantity,
                Price = detail.Price,
                SerialNumber = detail.SerialNumber,
                Currency = detail.Currency,
                WarehouseCode = detail.WarehouseCode,
                FromWarehouseCode = detail.FromWarehouseCode,
                UnitsOfMeasurment = detail.UnitsOfMeasurment,
                LineStatus = detail.LineStatus,
                UoMCode = detail.UoMCode,
                InventoryQuantity = detail.InventoryQuantity
            };

            var sql = ConfigurationManager.AppSettings["update_transfer_detail"];
            await unitOfWork.TransferStockDetailRepository
                .SaveAsync(sql, detailParameters);
        }


        #endregion        
    }


}
