using chitecapi.Responses;
using DataAccess;
using DataAccess.Models;
using Infrastructure.UnitOfWork;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql;
using RestSharp;
using RestSharp.Serializers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace chitecapi.Controllers
{
    public class SapController : ChitecApiController
    {
        public SapController()
        {

        }
        #region Actions

        // GET api/SapInvoices/GetSapConduces?db=db1
        /// <summary>
        /// Obtiene una lista de Conduces.
        /// </summary>
        /// <param name="db" example="db1">The Database ID</param>
        /// <remarks>
        /// Se utiliza para obtener la lista de Conduces, generado con un formato JSON:
        /// 
        ///     Example Result:
        ///     {
        ///         "data": [
        ///                   {
        ///                     
        ///                   }
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
        public async Task<IHttpActionResult> GetSapConduces([FromUri] string db = "db1",string date="", string saveresult = "1")
        {
            string connetionString;
            SqlConnection conection;
            string dbconection = "db1";

            if (!db.Equals(""))
            {
                dbconection = db;
            }
            connetionString = ConfigurationManager.ConnectionStrings[dbconection].ConnectionString;
            string companydb = ConfigurationManager.AppSettings["sapcompanydb"];
            string username = ConfigurationManager.AppSettings["sapusername"];
            string password = ConfigurationManager.AppSettings["sappassword"];
            string urllogin = ConfigurationManager.AppSettings["saplogin"];
            string executionurl = ConfigurationManager.AppSettings["sapconduce"];
            conection = new SqlConnection(connetionString);

            //conection.Open();

            DataUtil dataUtil = new DataUtil(dbconection);

            dataUtil.Connect();

            string sql = "";

            String table = "";
            string filterdate = "";
            if (!date.Equals(""))
            {
                filterdate = $" & $filter=DocDate eq '{date}'";
            }
            else
            {
                filterdate = $" & $filter=DocDate eq '{DateTime.Now.ToString("yyyy-MM-dd")}'";
            }


            try
            {
                string data = "{\"CompanyDB\": \"" + companydb + "\",    \"UserName\": \""+username+"\",    \"Password\": \""+password+"\"}";

                var WebReq = (HttpWebRequest)WebRequest.Create(urllogin);
                WebReq.ContentType = "application/json;odata=minimalmetadata;charset=utf8";
                WebReq.Method = "POST";
                WebReq.KeepAlive = true;
                WebReq.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
                WebReq.Accept = "application/json;odata=minimalmetadata";
                WebReq.ServicePoint.Expect100Continue = false;

                WebReq.AllowAutoRedirect = true;
                WebReq.Timeout = 10000000;

                using (var streamWriter = new StreamWriter(WebReq.GetRequestStream()))
                { streamWriter.Write(data); }

                var httpResponse = (HttpWebResponse)WebReq.GetResponse();
                //ConexionSAP obj = null;

                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                    Console.WriteLine(result);
                    dynamic value = JObject.Parse(result);
                    string sessionid = value.SessionId.ToString();

                    //MessageBox.Show(sessionid);

                    //With the SessionID we can get all the Items

                    var WebReq2 = (HttpWebRequest)WebRequest.Create($"{executionurl} {filterdate}");
                    WebReq2.ContentType = "application/json;odata=minimalmetadata;charset=utf8";
                    WebReq2.Method = "GET";
                    WebReq2.KeepAlive = true;
                    WebReq2.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
                    WebReq2.Accept = "application/json;odata=minimalmetadata";
                    WebReq2.ServicePoint.Expect100Continue = false;
                    //WebReq2.Headers.Add("B1SESSION", sessionid);
                    WebReq2.Headers.Add("Cookie", $"B1SESSION={sessionid}");
                    WebReq2.Headers.Add("Prefer", "odata.maxpagesize=0");


                    WebReq2.AllowAutoRedirect = true;
                    WebReq2.Timeout = 10000000;


                    var httpResponse2 = (HttpWebResponse)WebReq2.GetResponse();
                    //ConexionSAP obj = null;

                    using (var streamReader2 = new StreamReader(httpResponse2.GetResponseStream()))
                    {

                        var result2 = streamReader2.ReadToEnd();
                        dynamic value2 = JObject.Parse(result2);
                        // Dictionary<string, object> jsonvalues = new Dictionary<string, object>();
                        // jsonvalues.Add("data", value2);
                        // jsonvalues.Add("error", 0);
                        // jsonvalues.Add("error_type", 0);
                        // jsonvalues.Add("error_message", 0);


                        if (saveresult.Equals("1"))
                        {
                            //conection = new DataUtil("dbconnection");
                            //conection.Connect();
                            string jsonvalue = JsonConvert.SerializeObject(value2);
                            sql = $"  exec insertpickingordersfromjson  @json='{jsonvalue.Replace("'", "")}'; ";
                            dataUtil.ExecuteCommand(sql);

                           /* sql = "select "+
                                  "  (SELECT header.*, " +
                                  "      ((SELECT detail.* FROM PickingOrderDetail detail WHERE detail.DocEntry = header.DocEntry FOR JSON AUTO) " +
                                  "      ) AS DocumentLines " +
                                  "  FROM PickingOrdersheader  header " +
                                  "  FOR JSON AUTO " +
                                  "  ) as value";
                            DataTable records = new DataTable();
                            dataUtil.FillDatatable(sql, records);
                            //Dictionary<string, object> jsonvalues = new Dictionary<string, object>();
                            //jsonvalues.Add("value", records);
                           */
                            dataUtil.CloseConnection();
                            //return Json(records);

                        }


                        return Json(value2);
                        //table = result2;


                    }


                    //obj = JsonConvert.DeserializeObject<ConexionSAP>(result);

                }
            }
            catch (WebException ex)
            {
                Console.WriteLine($"Exception occurred: {ex.Message}");
                Console.WriteLine($"Response: {new StreamReader(ex.Response.GetResponseStream()).ReadToEnd()}");
                table = $"Exception occurred: {ex.Message}" + "\n" + $"Response: {ex.StackTrace}";

                // Handle error
            }






            return Json(table);
        }

        // PATH api/SapInvoices/PatchSapConduces?db=db1
        /// <summary>
        /// Obtiene una lista de Conduces.
        /// </summary>
        /// <param name="db" example="db1">The Database ID</param>
        /// <remarks>
        /// WebConfig Keys: update_picking_detail_patch
        /// Se utiliza para obtener la lista de Conduces, generado con un formato JSON:
        /// 
        ///     Example Result:
        ///     {
        ///         "data": [
        ///                   {
        ///                     
        ///                   }
        ///                 ],
        ///         "error": 0,
        ///         "error_type": 0,
        ///         "error_message": 0
        ///     }
        /// </remarks>
        /// <response code="401">Unauthorized. Error en la configuracion de la base de datos</response>              
        /// <response code="200">OK. Devuelve el objeto solicitado.</response>        
        /// <response code="404">NotFound. No se ha encontrado el objeto solicitado.</response>
        [HttpPatch]
        public async Task<IHttpActionResult> PatchSapConduces([FromBody] PickingHeaderPatch pickingheader, [FromUri] string db = "db1", string lbl_NumeroDocEntry = "",string updateonsap="0")
        {
            string companydb = ConfigurationManager.AppSettings["sapcompanydb"];
            string username = ConfigurationManager.AppSettings["sapusername"];
            string password = ConfigurationManager.AppSettings["sappassword"];
            string urllogin = ConfigurationManager.AppSettings["saplogin"];
            string executionurl = ConfigurationManager.AppSettings["sappatchconduce"];
           
            string sql = "";
            String table = "";            
            
            if (updateonsap.Equals("1"))
            {
                try
                {
                    string data = "{\"CompanyDB\": \"" + companydb + "\",    \"UserName\": \"" + username + "\",    \"Password\": \"" + password + "\"}";

                    var WebReq = (HttpWebRequest)WebRequest.Create(urllogin);
                    WebReq.ContentType = "application/json;odata=minimalmetadata;charset=utf8";
                    WebReq.Method = "POST";
                    WebReq.KeepAlive = true;
                    WebReq.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
                    WebReq.Accept = "application/json;odata=minimalmetadata";
                    WebReq.ServicePoint.Expect100Continue = false;

                    WebReq.AllowAutoRedirect = true;
                    WebReq.Timeout = 10000000;

                    using (var streamWriter = new StreamWriter(WebReq.GetRequestStream()))
                    { streamWriter.Write(data); }

                    var httpResponse = (HttpWebResponse)WebReq.GetResponse();
                    //ConexionSAP obj = null;

                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        var result = streamReader.ReadToEnd();
                        Console.WriteLine(result);
                        dynamic value = JObject.Parse(result);
                        string sessionid = value.SessionId.ToString();

                        //MessageBox.Show(sessionid);

                        //With the SessionID we can get all the Items

                        var WebReq2 = (HttpWebRequest)WebRequest.Create($"{executionurl}({lbl_NumeroDocEntry})");
                        WebReq2.ContentType = "application/json;odata=minimalmetadata;charset=utf8";
                        WebReq2.Method = "PATCH";
                        WebReq2.KeepAlive = true;
                        WebReq2.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
                        WebReq2.Accept = "application/json;odata=minimalmetadata";
                        WebReq2.ServicePoint.Expect100Continue = false;
                        //WebReq2.Headers.Add("B1SESSION", sessionid);
                        WebReq2.Headers.Add("Cookie", $"B1SESSION={sessionid}");
                        WebReq2.Headers.Add("Prefer", "odata.maxpagesize=0");


                        WebReq2.AllowAutoRedirect = true;
                        WebReq2.Timeout = 10000000;

                        string datadetail = "";
                        datadetail = JsonConvert.SerializeObject(pickingheader);

                        using (var streamWriter = new StreamWriter(WebReq2.GetRequestStream()))
                        { streamWriter.Write(datadetail); }

                        var httpResponse2 = (HttpWebResponse)WebReq2.GetResponse();
                        //ConexionSAP obj = null;

                        using (var streamReader2 = new StreamReader(httpResponse2.GetResponseStream()))
                        {

                            //var result2 = streamReader2.ReadToEnd();
                            //dynamic value2 = JObject.Parse(result2);
                            Dictionary<string, object> jsonvalues = new Dictionary<string, object>();
                            jsonvalues.Add("data", "Updated Completed");
                            jsonvalues.Add("error", 0);
                            jsonvalues.Add("error_type", 0);
                            jsonvalues.Add("error_message", 0);


                            return Json(jsonvalues);
                            //table = result2;


                        }


                        //obj = JsonConvert.DeserializeObject<ConexionSAP>(result);

                    }
                }
                catch (WebException ex)
                {
                    Console.WriteLine($"Exception occurred: {ex.Message}");
                    //Console.WriteLine($"Response: {new StreamReader(ex.Response.GetResponseStream()).ReadToEnd()}");
                    table = $"Exception occurred: {ex.Message}" + "\n" + $"Response: {ex.StackTrace}";
                    return Json(table);
                    // Handle error
                }

            }

            // updating the sql server picking order
           
            string connetionString;
            SqlConnection conection;
            string dbconection = "db1";

            if (!db.Equals(""))
            {
                dbconection = db;
            }

            connetionString = ConfigurationManager.ConnectionStrings[dbconection].ConnectionString;

            DataUtil dataUtil = new DataUtil(dbconection);
            dataUtil.Connect();
            foreach (var detail in pickingheader.DocumentLines)
            {
                sql = ConfigurationManager.AppSettings["update_picking_detail_patch"];
                dataUtil.PrepareStatement(sql);
                dataUtil.AddParameter("DocEntry", lbl_NumeroDocEntry);
                dataUtil.AddParameter("LineNum", detail.LineNum);
                dataUtil.AddParameter("ItemCode", detail.ItemCode);
                dataUtil.AddParameter("FreeText", detail.FreeText);
                dataUtil.ExecuteStatement();
            }
           

            dataUtil.CloseConnection();
           
            Dictionary<string, object> jsonvalue = new Dictionary<string, object>();
            jsonvalue.Add("data", "Updated Completed");
            jsonvalue.Add("error", 0);
            jsonvalue.Add("error_type", 0);
            jsonvalue.Add("error_message", 0);


            return Json(jsonvalue);

            
        }

        
        /// <summary>
        /// Guardar Picking.
        /// </summary>
        /// <param name="db" example="db1">The Database ID</param>
        /// <remarks>
        /// Se utiliza para Guardar multiples Entradas de Picking, Enviando un json con la información de la captura
        /// </remarks>
        /// <response code="401">Unauthorized. Error en la configuracion de la base de datos</response>              
        /// <response code="200">OK. Devuelve el objeto solicitado.</response>        
        /// <response code="404">NotFound. No se ha encontrado el objeto solicitado.</response>
        [HttpPost]
        public async Task<IHttpActionResult> GuardarSapConduces([FromBody] List<PickingHeader> pickingheaders, string db="db1")
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

            using (var unitOfWork = new PickingUnitOfWork(connectionString, conection))
            {
                foreach (var picking in pickingheaders)
                {
                    if (picking?.IsNotInitialized == true)
                    {
                        return new CustomJsonActionResult(
                            System.Net.HttpStatusCode.NotFound,
                            new JsonErrorResponse(1, 400, "Faltan parámetros"));
                    }

                    try
                    {
                        if (!await HeaderDoNotExist(picking.DocEntry, unitOfWork))
                        {
                            await EditHeader(picking, unitOfWork);
                        }
                        else
                        {
                            await CreateHeader(picking, unitOfWork);
                        }


                        foreach (var detail in picking.DocumentLines)
                        {
                            if (!await DetailDoNotExist(picking.DocEntry, detail.LineNum, unitOfWork))
                            {
                                await EditDetail(picking.DocEntry, detail, unitOfWork);
                            }
                            else
                            {
                                await CreateDetail(picking.DocEntry, detail, unitOfWork);
                            }

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

       

        // GET api/sap/ListaConduceSap?db=db1
        /// <summary>
        /// Obtiene una lista de todas los conduces listados por fecha.
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
        public async Task<IHttpActionResult> ListaConduceSap(string db = "db1", string date = "")
        {
            var connectionString = GetConnectionString(db);

            if (string.IsNullOrEmpty(connectionString))
            {
                return new CustomJsonActionResult(
                    System.Net.HttpStatusCode.NotFound,
                    new JsonErrorResponse(1, 400, $"La base de datos {db} no existe."));
            }


            using (var unitOfWork = new PickingUnitOfWork(connectionString))
            {
                
                var orderHeaders = await GetConduceHeaders(unitOfWork, date);
                var orders = await GetConduces(orderHeaders, unitOfWork);
                var data = GetDataJson(orders);

                return
                    new CustomJsonActionResult(System.Net.HttpStatusCode.OK, new JsonDataResponse(data));
            }
        }

        // GET api/Sap/GetSapItems?db=db1
        /// <summary>
        /// Obtiene una lista de Items.
        /// </summary>
        /// <param name="db" example="db1">The Database ID</param>
        /// <remarks>
        /// Se utiliza para obtener la lista de Items, generado con un formato JSON:
        /// 
        ///     Example Result:
        ///     {
        ///         "data": [
        ///                   {
        ///                     
        ///                   }
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
        public async Task<IHttpActionResult> GetSapItems([FromUri] string db = "db1", string saveresult = "1")
        {
            string connetionString;
            SqlConnection conection;
            string dbconection = "db1";

            if (!db.Equals(""))
            {
                dbconection = db;
            }

            connetionString = ConfigurationManager.ConnectionStrings[dbconection].ConnectionString;
            string companydb = ConfigurationManager.AppSettings["sapcompanydb"];
            string username = ConfigurationManager.AppSettings["sapusername"];
            string password = ConfigurationManager.AppSettings["sappassword"];
            string urllogin = ConfigurationManager.AppSettings["saplogin"];
            string executionurl = ConfigurationManager.AppSettings["sapitems"];
            string rutafile = ConfigurationManager.AppSettings["getrutaarticulossap"];

            DataUtil dataUtil = new DataUtil(dbconection);

            dataUtil.Connect();

            String table = "";
            string sql = "";
            string filterdate = "";
            //if (!date.Equals(""))
            //{
            //    filterdate = $" & $filter=DocDate eq '{date}'";
            //}


            try
            {
                string data = "{\"CompanyDB\": \"" + companydb + "\",    \"UserName\": \"" + username + "\",    \"Password\": \"" + password + "\"}";

                var WebReq = (HttpWebRequest)WebRequest.Create(urllogin);
                WebReq.ContentType = "application/json;odata=minimalmetadata;charset=utf8";
                WebReq.Method = "POST";
                WebReq.KeepAlive = true;
                WebReq.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
                WebReq.Accept = "application/json;odata=minimalmetadata";
                WebReq.ServicePoint.Expect100Continue = false;

                WebReq.AllowAutoRedirect = true;
                WebReq.Timeout = 10000000;

                using (var streamWriter = new StreamWriter(WebReq.GetRequestStream()))
                { streamWriter.Write(data); }

                var httpResponse = (HttpWebResponse)WebReq.GetResponse();
                //ConexionSAP obj = null;

                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                    Console.WriteLine(result);
                    dynamic value = JObject.Parse(result);
                    string sessionid = value.SessionId.ToString();

                    //MessageBox.Show(sessionid);

                    //With the SessionID we can get all the Items

                    var WebReq2 = (HttpWebRequest)WebRequest.Create($"{executionurl} {filterdate}");
                    WebReq2.ContentType = "application/json;odata=minimalmetadata;charset=utf8";
                    WebReq2.Method = "GET";
                    WebReq2.KeepAlive = true;
                    WebReq2.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
                    WebReq2.Accept = "application/json;odata=minimalmetadata";
                    WebReq2.ServicePoint.Expect100Continue = false;
                    //WebReq2.Headers.Add("B1SESSION", sessionid);
                    WebReq2.Headers.Add("Cookie", $"B1SESSION={sessionid}");
                    WebReq2.Headers.Add("Prefer", "odata.maxpagesize=0");


                    WebReq2.AllowAutoRedirect = true;
                    WebReq2.Timeout = 10000000;


                    var httpResponse2 = (HttpWebResponse)WebReq2.GetResponse();
                    //ConexionSAP obj = null;

                    using (var streamReader2 = new StreamReader(httpResponse2.GetResponseStream()))
                    {
                        var result2 = streamReader2.ReadToEnd();
                        dynamic value2 = JObject.Parse(result2);
                        // Dictionary<string, object> jsonvalues = new Dictionary<string, object>();
                        // jsonvalues.Add("data", value2);
                        // jsonvalues.Add("error", 0);
                        // jsonvalues.Add("error_type", 0);
                        // jsonvalues.Add("error_message", 0);

                        if (!rutafile.Equals(""))
                        {
                            using (StreamWriter file = File.CreateText($@"{rutafile}\articulossap.json"))
                            {
                                JsonSerializer serializer = new JsonSerializer();
                                //serialize object directly into file stream
                                serializer.Serialize(file, value2);
                            }
                        }

                        if (saveresult.Equals("1"))
                        {
                            //conection = new DataUtil("dbconnection");
                            //conection.Connect();
                            string jsonvalue = JsonConvert.SerializeObject(value2);
                            sql = $"  exec insertitemssb1fromjson  @json='{jsonvalue.Replace("'","")}'; ";
                            dataUtil.ExecuteCommand(sql);

                            sql = "select * from ArticulosSB1";
                            DataTable records = new DataTable();
                            dataUtil.FillDatatable(sql,records);
                            Dictionary<string, object> jsonvalues = new Dictionary<string, object>();
                            jsonvalues.Add("value", records);



                            dataUtil.CloseConnection();
                            return Json(jsonvalues);

                        }


                        return Json(value2);
                        //table = result2;


                    }


                    //obj = JsonConvert.DeserializeObject<ConexionSAP>(result);

                }
            }
            catch (WebException ex)
            {
                //Console.WriteLine($"Exception occurred: {ex.Message}");
                //Console.WriteLine($"Response: {new StreamReader(ex.Response.GetResponseStream()).ReadToEnd()}");
                table = $"Exception occurred: {ex.Message}" + "\n" + $"Response: {ex.StackTrace}";

                // Handle error
            }






            return Json(table);
        }

        // GET api/Sap/ListaArticulosSap?db=db1&codigo_barra=2222
        /// <summary>
        /// Obtiene una lista de articulos.
        /// </summary>
        /// <param name="db" example="db1">The Database ID</param>
        /// <param name="ItemCode" example="111">Item ID</param>
        /// <param name="BarCode" example="111">Item CodeBar</param>
        /// <param name="alterno1" example="111">Alternative Number1</param>
        /// <param name="alterno2" example="111">Alternative Number2</param>
        /// <param name="alterno3" example="111">Alternative Number3</param>
        /// <param name="ItemName" example="Coca cola">Item Description</param>
        /// <remarks>
        /// Se utiliza para obtener la lista de articulos, generado con un formato JSON:
        /// 
        ///     Example Result:
        ///     {
        ///         "data": [
        ///                   {
        ///                   "ItemCode": "00-3910",
        ///                   "ItemName": "G.DIVA CREAM BR CERAMICA ESPAÑOLA 45X45 (7PZ Y 64CJ)",
        ///                   "ForeignName": null,
        ///                   "ItemsGroupCode": "102",
        ///                   "CustomsGroupCode": null,
        ///                   "SalesVATGroup": null,
        ///                   "BarCode": "00-3910",
        ///                   "VatLiable": null,
        ///                   "PurchaseItem": null,
        ///                   "SalesItem": null,
        ///                   "InventoryItem": null,
        ///                   "IncomeAccount": null,
        ///                   "Price": null,
        ///                   "Binlocations": null,
        ///                   "WarehouseCode": null,
        ///                   "U_MasterCode": null,
        ///                   "U_MasterQty": null,
        ///                   "U_InnerCode": null,
        ///                   "U_InnerQty": null,
        ///                   "U_AddCode": null,
        ///                   "U_AddQty": null,
        ///                   "U_Pasillo": null,
        ///                   "U_Pasillo2": null,
        ///                   "U_Tramo": null,
        ///                   "U_Tramo2": null,
        ///                   "Commited": null,
        ///                   "Existencia": null,
        ///                   "M_SYN": null
        ///                       }
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
        public IHttpActionResult ListaArticulosSap([FromUri] string db = "db1", string ItemCode = "0", string BarCode = "", string alterno1 = "0", string alterno2 = "0", string alterno3 = "0", string ItemName = "", string saveresult = "0", string tablename = "articulos_test")
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


            var sql = ConfigurationManager.AppSettings["getallarticulossap"];

            if (sql == null)
            {
                sql = "select * from ArticulosSB1";
            }

            dataUtil.PrepareStatement(sql);


            if (!ItemCode.Equals("0") || !BarCode.Equals("") || !alterno1.Equals("0") || !alterno2.Equals("0") || !alterno3.Equals("0") || !ItemName.Equals(""))
            {
                sql = ConfigurationManager.AppSettings["getallarticulossapby"];
                dataUtil.PrepareStatement(sql);


                dataUtil.AddParameter("@ItemCode", ItemCode);
                dataUtil.AddParameter("@BarCode", BarCode);
                dataUtil.AddParameter("@alterno1", alterno1);
                dataUtil.AddParameter("@alterno2", alterno2);
                dataUtil.AddParameter("@alterno3", alterno3);
                dataUtil.AddParameter("@ItemName", ItemName);
            }





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


          /*  string rutafile = ConfigurationManager.AppSettings["getrutaarticulossap"];
            if (!rutafile.Equals(""))
            {
                using (StreamWriter file = File.CreateText($@"{rutafile}\articulossap.json"))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    //serialize object directly into file stream
                    serializer.Serialize(file, table);
                }
            }
          */
            return Json(table);
        }



        // GET api/Sap/GetSapItemsGroup?db=db1
        /// <summary>
        /// Obtiene una lista de ItemsGroup.
        /// </summary>
        /// <param name="db" example="db1">The Database ID</param>
        /// <remarks>
        /// Se utiliza para obtener la lista de ItemsGroup, generado con un formato JSON:
        /// 
        ///     Example Result:
        ///     {
        ///         "data": [
        ///                   {
        ///                     
        ///                   }
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
        public async Task<IHttpActionResult> GetSapItemsGroup([FromUri] string db = "db1", string saveresult = "1")
        {
            string connetionString;
            SqlConnection conection;
            string dbconection = "db1";

            if (!db.Equals(""))
            {
                dbconection = db;
            }

            connetionString = ConfigurationManager.ConnectionStrings[dbconection].ConnectionString;
            string companydb = ConfigurationManager.AppSettings["sapcompanydb"];
            string username = ConfigurationManager.AppSettings["sapusername"];
            string password = ConfigurationManager.AppSettings["sappassword"];
            string urllogin = ConfigurationManager.AppSettings["saplogin"];
            string executionurl = ConfigurationManager.AppSettings["sapitemsgroup"];

            DataUtil dataUtil = new DataUtil(dbconection);

            dataUtil.Connect();

            String table = "";
            string sql = "";
            string filterdate = "";
            //if (!date.Equals(""))
            //{
            //    filterdate = $" & $filter=DocDate eq '{date}'";
            //}


            try
            {
                string data = "{\"CompanyDB\": \"" + companydb + "\",    \"UserName\": \"" + username + "\",    \"Password\": \"" + password + "\"}";

                var WebReq = (HttpWebRequest)WebRequest.Create(urllogin);
                WebReq.ContentType = "application/json;odata=minimalmetadata;charset=utf8";
                WebReq.Method = "POST";
                WebReq.KeepAlive = true;
                WebReq.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
                WebReq.Accept = "application/json;odata=minimalmetadata";
                WebReq.ServicePoint.Expect100Continue = false;

                WebReq.AllowAutoRedirect = true;
                WebReq.Timeout = 10000000;

                using (var streamWriter = new StreamWriter(WebReq.GetRequestStream()))
                { streamWriter.Write(data); }

                var httpResponse = (HttpWebResponse)WebReq.GetResponse();
                //ConexionSAP obj = null;

                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                    Console.WriteLine(result);
                    dynamic value = JObject.Parse(result);
                    string sessionid = value.SessionId.ToString();

                    //MessageBox.Show(sessionid);

                    //With the SessionID we can get all the Items

                    var WebReq2 = (HttpWebRequest)WebRequest.Create($"{executionurl} {filterdate}");
                    WebReq2.ContentType = "application/json;odata=minimalmetadata;charset=utf8";
                    WebReq2.Method = "GET";
                    WebReq2.KeepAlive = true;
                    WebReq2.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
                    WebReq2.Accept = "application/json;odata=minimalmetadata";
                    WebReq2.ServicePoint.Expect100Continue = false;
                    //WebReq2.Headers.Add("B1SESSION", sessionid);
                    WebReq2.Headers.Add("Cookie", $"B1SESSION={sessionid}");
                    WebReq2.Headers.Add("Prefer", "odata.maxpagesize=0");


                    WebReq2.AllowAutoRedirect = true;
                    WebReq2.Timeout = 10000000;


                    var httpResponse2 = (HttpWebResponse)WebReq2.GetResponse();
                    //ConexionSAP obj = null;

                    using (var streamReader2 = new StreamReader(httpResponse2.GetResponseStream()))
                    {
                        var result2 = streamReader2.ReadToEnd();
                        dynamic value2 = JObject.Parse(result2);
                        // Dictionary<string, object> jsonvalues = new Dictionary<string, object>();
                        // jsonvalues.Add("data", value2);
                        // jsonvalues.Add("error", 0);
                        // jsonvalues.Add("error_type", 0);
                        // jsonvalues.Add("error_message", 0);


                        if (saveresult.Equals("1"))
                        {
                            //conection = new DataUtil("dbconnection");
                            //conection.Connect();
                            string jsonvalue = JsonConvert.SerializeObject(value2);
                            sql = $"  exec insertItemsGroupfromjson  @json='{jsonvalue.Replace("'", "")}'; ";
                            dataUtil.ExecuteCommand(sql);

                            sql = "select * from ItemsGroup";
                            DataTable records = new DataTable();
                            dataUtil.FillDatatable(sql, records);
                            Dictionary<string, object> jsonvalues = new Dictionary<string, object>();
                            jsonvalues.Add("value", records);

                            dataUtil.CloseConnection();
                            return Json(jsonvalues);

                        }


                        return Json(value2);
                        //table = result2;


                    }


                    //obj = JsonConvert.DeserializeObject<ConexionSAP>(result);

                }
            }
            catch (WebException ex)
            {
                //Console.WriteLine($"Exception occurred: {ex.Message}");
                //Console.WriteLine($"Response: {new StreamReader(ex.Response.GetResponseStream()).ReadToEnd()}");
                table = $"Exception occurred: {ex.Message}" + "\n" + $"Response: {ex.StackTrace}";

                // Handle error
            }






            return Json(table);
        }

        // GET api/Sap/ListaItemGroupSap
        /// <summary>
        /// Obtiene una lista de ItemsGroup.
        /// </summary>
        /// <param name="db" example="db1">The Database ID</param>
        /// <remarks>
        /// Se utiliza para obtener la lista de Items Group, generado con un formato JSON:
        /// 
        ///     Example Result:
        ///     {
        ///         "data": [
        ///                   {
        ///                   "Number": "01",
        ///                   "GroupName": "test",
        ///                   "M_SYN": null
        ///                       }
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
        public IHttpActionResult ListaItemGroupSap([FromUri] string db = "db1", string saveresult = "0", string tablename = "itemgroup_test")
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


            var sql = ConfigurationManager.AppSettings["getallitemgroupsap"];

            if (sql == null)
            {
                sql = "select * from ItemsGroup";
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


            return Json(table);
        }


        // GET api/Sap/GetSapCheckItemsStock?db=db1
        /// <summary>
        /// Obtiene una lista de inventario de items por almacen.
        /// </summary>
        /// <param name="db" example="db1">The Database ID</param>
        /// <remarks>
        /// Se utiliza para obtener la lista de CheckItemsStock, generado con un formato JSON:
        /// 
        ///     Example Result:
        ///     {
        ///         "data": [
        ///                   {
        ///                     
        ///                   }
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
        public async Task<IHttpActionResult> GetSapCheckItemsStock([FromUri] string db = "db1", string saveresult = "1")
        {
            string connetionString;
            SqlConnection conection;
            string dbconection = "db1";

            if (!db.Equals(""))
            {
                dbconection = db;
            }

            connetionString = ConfigurationManager.ConnectionStrings[dbconection].ConnectionString;
            string companydb = ConfigurationManager.AppSettings["sapcompanydb"];
            string username = ConfigurationManager.AppSettings["sapusername"];
            string password = ConfigurationManager.AppSettings["sappassword"];
            string urllogin = ConfigurationManager.AppSettings["saplogin"];
            string executionurl = ConfigurationManager.AppSettings["sapcheckitemstock"];

            DataUtil dataUtil = new DataUtil(dbconection);

            dataUtil.Connect();

            String table = "";
            string sql = "";
            string filterdate = "";
            //if (!date.Equals(""))
            //{
            //    filterdate = $" & $filter=DocDate eq '{date}'";
            //}


            try
            {
                string data = "{\"CompanyDB\": \"" + companydb + "\",    \"UserName\": \"" + username + "\",    \"Password\": \"" + password + "\"}";

                var WebReq = (HttpWebRequest)WebRequest.Create(urllogin);
                WebReq.ContentType = "application/json;odata=minimalmetadata;charset=utf8";
                WebReq.Method = "POST";
                WebReq.KeepAlive = true;
                WebReq.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
                WebReq.Accept = "application/json;odata=minimalmetadata";
                WebReq.ServicePoint.Expect100Continue = false;

                WebReq.AllowAutoRedirect = true;
                WebReq.Timeout = 10000000;

                using (var streamWriter = new StreamWriter(WebReq.GetRequestStream()))
                { streamWriter.Write(data); }

                var httpResponse = (HttpWebResponse)WebReq.GetResponse();
                //ConexionSAP obj = null;

                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                    Console.WriteLine(result);
                    dynamic value = JObject.Parse(result);
                    string sessionid = value.SessionId.ToString();

                    //MessageBox.Show(sessionid);

                    //With the SessionID we can get all the Items

                    var WebReq2 = (HttpWebRequest)WebRequest.Create($"{executionurl} {filterdate}");
                    WebReq2.ContentType = "application/json;odata=minimalmetadata;charset=utf8";
                    WebReq2.Method = "GET";
                    WebReq2.KeepAlive = true;
                    WebReq2.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
                    WebReq2.Accept = "application/json;odata=minimalmetadata";
                    WebReq2.ServicePoint.Expect100Continue = false;
                    //WebReq2.Headers.Add("B1SESSION", sessionid);
                    WebReq2.Headers.Add("Cookie", $"B1SESSION={sessionid}");
                    WebReq2.Headers.Add("Prefer", "odata.maxpagesize=0");


                    WebReq2.AllowAutoRedirect = true;
                    WebReq2.Timeout = 10000000;


                    var httpResponse2 = (HttpWebResponse)WebReq2.GetResponse();
                    //ConexionSAP obj = null;

                    using (var streamReader2 = new StreamReader(httpResponse2.GetResponseStream()))
                    {
                        var result2 = streamReader2.ReadToEnd();
                        dynamic value2 = JObject.Parse(result2);
                        // Dictionary<string, object> jsonvalues = new Dictionary<string, object>();
                        // jsonvalues.Add("data", value2);
                        // jsonvalues.Add("error", 0);
                        // jsonvalues.Add("error_type", 0);
                        // jsonvalues.Add("error_message", 0);


                        if (saveresult.Equals("1"))
                        {
                            //conection = new DataUtil("dbconnection");
                            //conection.Connect();
                            string jsonvalue = JsonConvert.SerializeObject(value2);
                            sql = $"  exec insertCheckItemsStockfromjson  @json='{jsonvalue.Replace("'", "").Replace("/","")}'; ";
                            dataUtil.ExecuteCommand(sql);

                            sql = "select * from CheckItemStock";
                            DataTable records = new DataTable();
                            dataUtil.FillDatatable(sql, records);
                            Dictionary<string, object> jsonvalues = new Dictionary<string, object>();
                            jsonvalues.Add("value", records);

                            dataUtil.CloseConnection();
                            return Json(jsonvalues);

                        }


                        return Json(value2);
                        //table = result2;


                    }


                    //obj = JsonConvert.DeserializeObject<ConexionSAP>(result);

                }
            }
            catch (WebException ex)
            {
                //Console.WriteLine($"Exception occurred: {ex.Message}");
                //Console.WriteLine($"Response: {new StreamReader(ex.Response.GetResponseStream()).ReadToEnd()}");
                table = $"Exception occurred: {ex.Message}" + "\n" + $"Response: {ex.StackTrace}";

                // Handle error
            }






            return Json(table);
        }

        // GET api/Sap/ListaCheckItemsStock
        /// <summary>
        /// Obtiene una lista de ItemsGroup.
        /// </summary>
        /// <param name="db" example="db1">The Database ID</param>
        /// <remarks>
        /// Se utiliza para obtener la lista de Items Group, generado con un formato JSON:
        /// 
        ///     Example Result:
        ///     {
        ///         "data": [
        ///                   {
        ///                   "Number": "01",
        ///                   "GroupName": "test",
        ///                   "M_SYN": null
        ///                       }
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
        public IHttpActionResult ListaCheckItemStockSap([FromUri] string db = "db1", string saveresult = "0", string tablename = "itemgroup_test")
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


            var sql = ConfigurationManager.AppSettings["getallcheckitemstocksap"];

            if (sql == null)
            {
                sql = "select * from CheckItemStock";
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


            return Json(table);
        }

        // GET api/Sap/GetSapBusinessPartners?db=db1
        /// <summary>
        /// Obtiene una lista de Business Partners.
        /// </summary>
        /// <param name="db" example="db1">The Database ID</param>
        /// <remarks>
        /// Se utiliza para obtener la lista de Business Partners, generado con un formato JSON:
        /// 
        ///     Example Result:
        ///     {
        ///         "data": [
        ///                   {
        ///                     
        ///                   }
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
        public async Task<IHttpActionResult> GetSapBusinessPartners([FromUri] string db = "db1", string saveresult = "1")
        {
            string connetionString;
            SqlConnection conection;
            string dbconection = "db1";

            if (!db.Equals(""))
            {
                dbconection = db;
            }

            connetionString = ConfigurationManager.ConnectionStrings[dbconection].ConnectionString;
            string companydb = ConfigurationManager.AppSettings["sapcompanydb"];
            string username = ConfigurationManager.AppSettings["sapusername"];
            string password = ConfigurationManager.AppSettings["sappassword"];
            string urllogin = ConfigurationManager.AppSettings["saplogin"];
            string executionurl = ConfigurationManager.AppSettings["sapbusinesspartners"];

            DataUtil dataUtil = new DataUtil(dbconection);

            dataUtil.Connect();

            String table = "";
            string sql = "";
            string filterdate = "";
            //if (!date.Equals(""))
            //{
            //    filterdate = $" & $filter=DocDate eq '{date}'";
            //}


            try
            {
                string data = "{\"CompanyDB\": \"" + companydb + "\",    \"UserName\": \"" + username + "\",    \"Password\": \"" + password + "\"}";

                var WebReq = (HttpWebRequest)WebRequest.Create(urllogin);
                WebReq.ContentType = "application/json;odata=minimalmetadata;charset=utf8";
                WebReq.Method = "POST";
                WebReq.KeepAlive = true;
                WebReq.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
                WebReq.Accept = "application/json;odata=minimalmetadata";
                WebReq.ServicePoint.Expect100Continue = false;

                WebReq.AllowAutoRedirect = true;
                WebReq.Timeout = 10000000;

                using (var streamWriter = new StreamWriter(WebReq.GetRequestStream()))
                { streamWriter.Write(data); }

                var httpResponse = (HttpWebResponse)WebReq.GetResponse();
                //ConexionSAP obj = null;

                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                    Console.WriteLine(result);
                    dynamic value = JObject.Parse(result);
                    string sessionid = value.SessionId.ToString();

                    //MessageBox.Show(sessionid);

                    //With the SessionID we can get all the Items

                    var WebReq2 = (HttpWebRequest)WebRequest.Create($"{executionurl} {filterdate}");
                    WebReq2.ContentType = "application/json;odata=minimalmetadata;charset=utf8";
                    WebReq2.Method = "GET";
                    WebReq2.KeepAlive = true;
                    WebReq2.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
                    WebReq2.Accept = "application/json;odata=minimalmetadata";
                    WebReq2.ServicePoint.Expect100Continue = false;
                    //WebReq2.Headers.Add("B1SESSION", sessionid);
                    WebReq2.Headers.Add("Cookie", $"B1SESSION={sessionid}");
                    WebReq2.Headers.Add("Prefer", "odata.maxpagesize=0");


                    WebReq2.AllowAutoRedirect = true;
                    WebReq2.Timeout = 10000000;


                    var httpResponse2 = (HttpWebResponse)WebReq2.GetResponse();
                    //ConexionSAP obj = null;

                    using (var streamReader2 = new StreamReader(httpResponse2.GetResponseStream()))
                    {
                        var result2 = streamReader2.ReadToEnd();
                        dynamic value2 = JObject.Parse(result2);
                        // Dictionary<string, object> jsonvalues = new Dictionary<string, object>();
                        // jsonvalues.Add("data", value2);
                        // jsonvalues.Add("error", 0);
                        // jsonvalues.Add("error_type", 0);
                        // jsonvalues.Add("error_message", 0);


                        if (saveresult.Equals("1"))
                        {
                            //conection = new DataUtil("dbconnection");
                            //conection.Connect();
                            string jsonvalue = JsonConvert.SerializeObject(value2);
                            sql = $"  exec insertBusinessPartnersfromjson  @json='{jsonvalue.Replace("'", "")}'; ";
                            dataUtil.ExecuteCommand(sql);

                            sql = "select * from BusinessPartners";
                            DataTable records = new DataTable();
                            dataUtil.FillDatatable(sql, records);
                            Dictionary<string, object> jsonvalues = new Dictionary<string, object>();
                            jsonvalues.Add("value", records);

                            dataUtil.CloseConnection();
                            return Json(jsonvalues);

                        }


                        return Json(value2);
                        //table = result2;


                    }


                    //obj = JsonConvert.DeserializeObject<ConexionSAP>(result);

                }
            }
            catch (WebException ex)
            {
                //Console.WriteLine($"Exception occurred: {ex.Message}");
                //Console.WriteLine($"Response: {new StreamReader(ex.Response.GetResponseStream()).ReadToEnd()}");
                table = $"Exception occurred: {ex.Message}" + "\n" + $"Response: {ex.StackTrace}";

                // Handle error
            }






            return Json(table);
        }


        // GET api/Sap/GetSapWarehouse?db=db1
        /// <summary>
        /// Obtiene una lista de ItemsGroup.
        /// </summary>
        /// <param name="db" example="db1">The Database ID</param>
        /// <remarks>
        /// Se utiliza para obtener la lista de ItemsGroup, generado con un formato JSON:
        /// 
        ///     Example Result:
        ///     {
        ///         "data": [
        ///                   {
        ///                     
        ///                   }
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
        public async Task<IHttpActionResult> GetSapWarehouse([FromUri] string db = "db1", string saveresult = "1")
        {
            string connetionString;
            SqlConnection conection;
            string dbconection = "db1";

            if (!db.Equals(""))
            {
                dbconection = db;
            }

            connetionString = ConfigurationManager.ConnectionStrings[dbconection].ConnectionString;
            string companydb = ConfigurationManager.AppSettings["sapcompanydb"];
            string username = ConfigurationManager.AppSettings["sapusername"];
            string password = ConfigurationManager.AppSettings["sappassword"];
            string urllogin = ConfigurationManager.AppSettings["saplogin"];
            string executionurl = ConfigurationManager.AppSettings["sapwarehouse"];

            DataUtil dataUtil = new DataUtil(dbconection);

            dataUtil.Connect();

            String table = "";
            string sql = "";
            string filterdate = "";
            //if (!date.Equals(""))
            //{
            //    filterdate = $" & $filter=DocDate eq '{date}'";
            //}


            try
            {
                string data = "{\"CompanyDB\": \"" + companydb + "\",    \"UserName\": \"" + username + "\",    \"Password\": \"" + password + "\"}";

                var WebReq = (HttpWebRequest)WebRequest.Create(urllogin);
                WebReq.ContentType = "application/json;odata=minimalmetadata;charset=utf8";
                WebReq.Method = "POST";
                WebReq.KeepAlive = true;
                WebReq.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
                WebReq.Accept = "application/json;odata=minimalmetadata";
                WebReq.ServicePoint.Expect100Continue = false;

                WebReq.AllowAutoRedirect = true;
                WebReq.Timeout = 10000000;

                using (var streamWriter = new StreamWriter(WebReq.GetRequestStream()))
                { streamWriter.Write(data); }

                var httpResponse = (HttpWebResponse)WebReq.GetResponse();
                //ConexionSAP obj = null;

                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                    Console.WriteLine(result);
                    dynamic value = JObject.Parse(result);
                    string sessionid = value.SessionId.ToString();

                    //MessageBox.Show(sessionid);

                    //With the SessionID we can get all the Items

                    var WebReq2 = (HttpWebRequest)WebRequest.Create($"{executionurl} {filterdate}");
                    WebReq2.ContentType = "application/json;odata=minimalmetadata;charset=utf8";
                    WebReq2.Method = "GET";
                    WebReq2.KeepAlive = true;
                    WebReq2.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
                    WebReq2.Accept = "application/json;odata=minimalmetadata";
                    WebReq2.ServicePoint.Expect100Continue = false;
                    //WebReq2.Headers.Add("B1SESSION", sessionid);
                    WebReq2.Headers.Add("Cookie", $"B1SESSION={sessionid}");
                    WebReq2.Headers.Add("Prefer", "odata.maxpagesize=0");


                    WebReq2.AllowAutoRedirect = true;
                    WebReq2.Timeout = 10000000;


                    var httpResponse2 = (HttpWebResponse)WebReq2.GetResponse();
                    //ConexionSAP obj = null;

                    using (var streamReader2 = new StreamReader(httpResponse2.GetResponseStream()))
                    {
                        var result2 = streamReader2.ReadToEnd();
                        dynamic value2 = JObject.Parse(result2);
                        // Dictionary<string, object> jsonvalues = new Dictionary<string, object>();
                        // jsonvalues.Add("data", value2);
                        // jsonvalues.Add("error", 0);
                        // jsonvalues.Add("error_type", 0);
                        // jsonvalues.Add("error_message", 0);


                        if (saveresult.Equals("1"))
                        {
                            //conection = new DataUtil("dbconnection");
                            //conection.Connect();
                            string jsonvalue = JsonConvert.SerializeObject(value2);
                            sql = $"  exec insertWarehousefromjson  @json='{jsonvalue.Replace("'", "")}'; ";
                            dataUtil.ExecuteCommand(sql);

                            sql = "select * from Warehouses";
                            DataTable records = new DataTable();
                            dataUtil.FillDatatable(sql, records);
                            Dictionary<string, object> jsonvalues = new Dictionary<string, object>();
                            jsonvalues.Add("value", records);

                            dataUtil.CloseConnection();
                            return Json(jsonvalues);

                        }


                        return Json(value2);
                        //table = result2;


                    }


                    //obj = JsonConvert.DeserializeObject<ConexionSAP>(result);

                }
            }
            catch (WebException ex)
            {
                //Console.WriteLine($"Exception occurred: {ex.Message}");
                //Console.WriteLine($"Response: {new StreamReader(ex.Response.GetResponseStream()).ReadToEnd()}");
                table = $"Exception occurred: {ex.Message}" + "\n" + $"Response: {ex.StackTrace}";

                // Handle error
            }






            return Json(table);
        }

        // GET api/Sap/ListaWarehouseSap
        /// <summary>
        /// Obtiene una lista de Warehouse.
        /// </summary>
        /// <param name="db" example="db1">The Database ID</param>
        /// <remarks>
        /// Se utiliza para obtener la lista de Items Group, generado con un formato JSON:
        /// 
        ///     Example Result:
        ///     {
        ///         "data": [
        ///                   {
        ///                   "Number": "01",
        ///                   "GroupName": "test",
        ///                   "M_SYN": null
        ///                       }
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
        public IHttpActionResult ListaWarehouseSap([FromUri] string db = "db1", string saveresult = "0", string tablename = "warehouses_test")
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


            var sql = ConfigurationManager.AppSettings["getallwarehousesap"];

            if (sql == null)
            {
                sql = "select * from Warehouses";
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


            return Json(table);
        }



        /// <summary>
        /// Remotes the certificate validate.
        /// </summary>
        private static bool RemoteCertificateValidate(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors error)
        {
            // trust any certificate!!!
            System.Console.WriteLine("Warning, trust any certificate");
            return true;
        }

        /// <summary>
        /// Sets the cert policy.
        /// </summary>
        public static void SetCertificatePolicy()
        {
            ServicePointManager.ServerCertificateValidationCallback += RemoteCertificateValidate;
        }



        /// <summary>
        /// Guardar InventoryCount.
        /// </summary>
        /// <param name="db" example="db1">The Database ID</param>
        /// <remarks>
        /// Se utiliza para Guardar multiples Entradas de Picking, Enviando un json con la información de la captura
        /// </remarks>
        /// <response code="401">Unauthorized. Error en la configuracion de la base de datos</response>              
        /// <response code="200">OK. Devuelve el objeto solicitado.</response>        
        /// <response code="404">NotFound. No se ha encontrado el objeto solicitado.</response>
        [HttpPost]
        public async Task<IHttpActionResult> GuardarSapInventoryCount([FromBody] List<InventoryCountingHeader> headers, string db = "db1")
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

            using (var unitOfWork = new InventoryCountingUnitOfWork(connectionString, conection))
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
                        if (!await CountingHeaderDoNotExist(transfer.DocumentEntry, unitOfWork))
                        {
                            await CountingEditHeader(transfer, unitOfWork);
                        }
                        else
                        {
                            await CountingCreateHeader(transfer, unitOfWork);
                        }


                        foreach (var detail in transfer.InventoryCountingLines)
                        {
                            if (!await CountingDetailDoNotExist(transfer.DocumentEntry, detail.LineNumber, unitOfWork))
                            {
                                await CountingEditDetail(transfer.DocumentEntry, detail, unitOfWork);
                            }
                            else
                            {
                                await CountingCreateDetail(transfer.DocumentEntry, detail, unitOfWork);
                            }

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



        // GET api/sap/ListaInventoryCountSap?db=db1
        /// <summary>
        /// Obtiene una lista de todas los Inventory Counts listados por fecha.
        /// </summary>
        /// <param name="date" example="">Fecha en la que se creo el Inventario</param>
        /// <param name="db" example="db1">The Database ID</param>
        /// <remarks>
        /// Se utiliza para obtener una lista de los inventarios, generado con un formato JSON:
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
        public async Task<IHttpActionResult> ListaInventoryCountSap(string db = "db1", string date = "")
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


            using (var unitOfWork = new InventoryCountingUnitOfWork(connectionString, conection))
            {

                var orderHeaders = await GetCountingHeaders(unitOfWork, date);
                var orders = await GetCounting(orderHeaders, unitOfWork);
                var data = GetDataJson(orders);

                return
                    new CustomJsonActionResult(System.Net.HttpStatusCode.OK, new JsonDataResponse(data));
            }
        }

        #endregion

        #region Helper methods

        private async Task<IEnumerable<PickingHeader>> GetConduces(IEnumerable<PickingHeader> pickingHeaders, PickingUnitOfWork unitOfWork)
        {
            var orders = new List<PickingHeader>();
            foreach (var header in pickingHeaders)
            {
                var sql = ConfigurationManager.AppSettings["get_picking_details"];
                var orderDetails = await unitOfWork.PickingDetailRepository.LoadAsync(sql, new { header.DocEntry });
                var order = new PickingHeader
                {
                    DocEntry = header.DocEntry,
                    DocNum = header.DocNum,
                    DocDate = header.DocDate,
                    CardCode = header.CardCode,
                    CardName = header.CardName,
                    DocTotal = header.DocTotal,
                    DocCurrency = header.DocCurrency,
                    JournalMemo = header.JournalMemo,
                    DocTime = header.DocTime,
                    SalesPersonCode = header.SalesPersonCode,
                    TransNum = header.TransNum,
                    VatSum = header.VatSum,
                    VatSumSys = header.VatSumSys,
                    DocTotalSys = header.DocTotalSys,
                    PickStatus = header.PickStatus,
                    DocumentStatus = header.DocumentStatus,
                    DownPaymentType = header.DownPaymentType,
                    U_NCF = header.U_NCF,
                    U_Usuario = header.U_Usuario,
                    U_claveVendedor = header.U_claveVendedor,
                    Comments = header.Comments,
                    DocumentLines = new List<PickingDetail>(orderDetails)
                };

                orders.Add(order);
            }

            return orders;
        }


        private async Task<IEnumerable<PickingHeader>> GetConduceHeaders(PickingUnitOfWork unitOfWork,string date="")
        {
            var sql = ConfigurationManager.AppSettings["get_picking_headers"];

            if (!date.Equals(""))
            {
                sql = sql + $" and  cast(DocDate as varchar)='{date}'";
            }

            return await unitOfWork.PickingHeaderRepository
                .LoadAsync(sql, new { });
        }




        private async Task<bool> DetailDoNotExist(string docNum, string lineNum, PickingUnitOfWork unitOfWork)
        {
            var sql = ConfigurationManager.AppSettings["get_picking_details_by_line_number"];
            var detail = await unitOfWork.PickingDetailRepository
                .LoadFirstAsync(sql, new { DocEntry = docNum, LineNum = lineNum });

            return detail == null;
        }


        private async Task<bool> HeaderDoNotExist(string docNum, PickingUnitOfWork unitOfWork)
        {
            var sql = ConfigurationManager.AppSettings["get_picking_by_doc"];
            var detail = await unitOfWork.PickingHeaderRepository
                .LoadFirstAsync(sql, new { DocEntry = docNum });

            return detail == null;
        }




        private async Task EditHeader(PickingHeader header, PickingUnitOfWork unitOfWork)
        {
            var headerParameters = new
            {
                DocEntry = header.DocEntry,
                DocNum = header.DocNum,
                DocDate = header.DocDate,
                CardCode = header.CardCode,
                CardName = header.CardName,
                DocTotal = header.DocTotal,
                DocCurrency = header.DocCurrency,
                JournalMemo = header.JournalMemo,
                DocTime = header.DocTime,
                SalesPersonCode = header.SalesPersonCode,
                TransNum = header.TransNum,
                VatSum = header.VatSum,
                VatSumSys = header.VatSumSys,
                DocTotalSys = header.DocTotalSys,
                PickStatus = header.PickStatus,
                DocumentStatus = header.DocumentStatus,
                DownPaymentType = header.DownPaymentType,
                U_NCF = header.U_NCF,
                U_Usuario = header.U_Usuario,
                U_claveVendedor = header.U_claveVendedor,
                Comments = header.Comments,
            };

            var sql = ConfigurationManager.AppSettings["update_picking_header"];
            await unitOfWork.PickingHeaderRepository
                    .SaveAsync(sql, headerParameters);
        }


        private async Task CreateHeader(PickingHeader header, PickingUnitOfWork unitOfWork)
        {
            var headerParameters = new
            {
                DocEntry = header.DocEntry,
                DocNum = header.DocNum,
                DocDate = header.DocDate,
                CardCode = header.CardCode,
                CardName = header.CardName,
                DocTotal = header.DocTotal,
                DocCurrency = header.DocCurrency,
                JournalMemo = header.JournalMemo,
                DocTime = header.DocTime,
                SalesPersonCode = header.SalesPersonCode,
                TransNum = header.TransNum,
                VatSum = header.VatSum,
                VatSumSys = header.VatSumSys,
                DocTotalSys = header.DocTotalSys,
                PickStatus = header.PickStatus,
                DocumentStatus = header.DocumentStatus,
                DownPaymentType = header.DownPaymentType,
                U_NCF = header.U_NCF,
                U_Usuario = header.U_Usuario,
                U_claveVendedor = header.U_claveVendedor,
                Comments = header.Comments,
            };

            var sql = ConfigurationManager.AppSettings["insert_picking_header"];
            await unitOfWork.PickingHeaderRepository
                    .SaveAsync(sql, headerParameters);
        }


        private async Task CreateDetail(string docNum, PickingDetail detail, PickingUnitOfWork unitOfWork)
        {
            var detailParameters = new
            {

                 DocEntry = docNum,
                 LineNum = detail.LineNum,
                 ItemCode = detail.ItemCode,
                 ItemDescription = detail.ItemDescription,
                 Quantity = detail.Quantity,
                 Price = detail.Price,
                 PriceAfterVAT = detail.PriceAfterVAT,
                 Currency = detail.Currency,
                 WarehouseCode = detail.WarehouseCode,
                 BarCode = detail.BarCode,
                 PickQuantity = detail.PickQuantity,
                 FreeText = detail.FreeText,
                 ShippingMethod = detail.ShippingMethod,
                 NetTaxAmount = detail.NetTaxAmount,
                 LineStatus = detail.LineStatus,
                 PackageQuantity    = detail.PackageQuantity,
                 ActualDeliveryDate = detail.ActualDeliveryDate,
                 UoMCode = detail.UoMCode,
                 InventoryQuantity = detail.InventoryQuantity,
                 U_metrosxpiezas = detail.U_metrosxpiezas,
                 U_articulosTienda = detail.U_articulosTienda,
                 U_Mayor = detail.U_Mayor,
                 U_metrosCliente = detail.U_metrosCliente
            };

            var sql = ConfigurationManager.AppSettings["insert_picking_detail"];
            await unitOfWork.PickingDetailRepository
                .SaveAsync(sql, detailParameters);
        }



        private async Task EditDetail(string docNum, PickingDetail detail, PickingUnitOfWork unitOfWork)
        {
            var detailParameters = new
            {
                DocEntry = docNum,
                LineNum = detail.LineNum,
                ItemCode = detail.ItemCode,
                ItemDescription = detail.ItemDescription,
                Quantity = detail.Quantity,
                Price = detail.Price,
                PriceAfterVAT = detail.PriceAfterVAT,
                Currency = detail.Currency,
                WarehouseCode = detail.WarehouseCode,
                BarCode = detail.BarCode,
                PickQuantity = detail.PickQuantity,
                FreeText = detail.FreeText,
                ShippingMethod = detail.ShippingMethod,
                NetTaxAmount = detail.NetTaxAmount,
                LineStatus = detail.LineStatus,
                PackageQuantity = detail.PackageQuantity,
                ActualDeliveryDate = detail.ActualDeliveryDate,
                UoMCode = detail.UoMCode,
                InventoryQuantity = detail.InventoryQuantity,
                U_metrosxpiezas = detail.U_metrosxpiezas,
                U_articulosTienda = detail.U_articulosTienda,
                U_Mayor = detail.U_Mayor,
                U_metrosCliente = detail.U_metrosCliente
            };

            var sql = ConfigurationManager.AppSettings["update_picking_detail"];
            await unitOfWork.PickingDetailRepository
                .SaveAsync(sql, detailParameters);
        }

        private async Task EditDetailPath(string docNum, PickingDetailPath detail, PickingUnitOfWork unitOfWork)
        {
            var detailParameters = new
            {
                DocEntry = docNum,
                LineNum = detail.LineNum,
                ItemCode = detail.ItemCode,
                FreeText = detail.FreeText,
            };

            var sql = ConfigurationManager.AppSettings["update_picking_detail_patch"];
 

            await unitOfWork.PickingDetailRepositoryPatch
                .SaveAsync(sql, detailParameters);
        }


        //Inventory Counting Section

        private async Task<IEnumerable<InventoryCountingHeader>> GetCounting(IEnumerable<InventoryCountingHeader> Headers, InventoryCountingUnitOfWork unitOfWork)
        {
            var orders = new List<InventoryCountingHeader>();
            foreach (var header in Headers)
            {
                var sql = ConfigurationManager.AppSettings["get_counting_details"];
                var orderDetails = await unitOfWork.InventoryCountingDetailRepository.LoadAsync(sql, new { header.DocumentEntry });
                var order = new InventoryCountingHeader
                {
                    DocumentEntry = header.DocumentEntry,
                    DocumentNumber = header.DocumentNumber,
                    Series = header.Series,
                    CountDate = header.CountDate,
                    CountTime = header.CountTime,
                    SingleCounterType = header.SingleCounterType,
                    SingleCounterID = header.SingleCounterID,
                    DocumentStatus = header.DocumentStatus,
                    Remarks = header.Remarks,
                    DocObjectCodeEx = header.DocObjectCodeEx,
                    FinancialPeriod = header.FinancialPeriod,
                    PeriodIndicator = header.PeriodIndicator,
                    CountingType = header.CountingType,
                    InventoryCountingLines = new List<InventoryCountingDetail>(orderDetails)
                };


                orders.Add(order);
            }

            return orders;
        }


        private async Task<IEnumerable<InventoryCountingHeader>> GetCountingHeaders(InventoryCountingUnitOfWork unitOfWork, string date = "")
        {
            var sql = ConfigurationManager.AppSettings["get_counting_headers"];

            if (!date.Equals(""))
            {
                sql = sql + $" and  cast(CountDate as varchar)='{date}'";
            }

            return await unitOfWork.InventoryCountingHeaderRepository
                .LoadAsync(sql, new { });
        }




        private async Task<bool> CountingDetailDoNotExist(string docNum, string lineNum, InventoryCountingUnitOfWork unitOfWork)
        {
            var sql = ConfigurationManager.AppSettings["get_counting_details_by_line_number"];
            var detail = await unitOfWork.InventoryCountingDetailRepository
                .LoadFirstAsync(sql, new { DocumentEntry = docNum, LineNumber = lineNum });

            return detail == null;
        }


        private async Task<bool> CountingHeaderDoNotExist(string docNum, InventoryCountingUnitOfWork unitOfWork)
        {
            var sql = ConfigurationManager.AppSettings["get_counting_by_doc"];
            var detail = await unitOfWork.InventoryCountingHeaderRepository
                .LoadFirstAsync(sql, new { DocumentEntry = docNum });

            return detail == null;
        }




        private async Task CountingEditHeader(InventoryCountingHeader header, InventoryCountingUnitOfWork unitOfWork)
        {
            var headerParameters = new
            {
                DocumentEntry = header.DocumentEntry,
                DocumentNumber = header.DocumentNumber,
                Series = header.Series,
                CountDate = header.CountDate,
                CountTime = header.CountTime,
                SingleCounterType = header.SingleCounterType,
                SingleCounterID = header.SingleCounterID,
                DocumentStatus = header.DocumentStatus,
                Remarks = header.Remarks,
                DocObjectCodeEx = header.DocObjectCodeEx,
                FinancialPeriod = header.FinancialPeriod,
                PeriodIndicator = header.PeriodIndicator,
                CountingType = header.CountingType
            };

            var sql = ConfigurationManager.AppSettings["update_counting_header"];
            await unitOfWork.InventoryCountingHeaderRepository
                    .SaveAsync(sql, headerParameters);
        }


        private async Task CountingCreateHeader(InventoryCountingHeader header, InventoryCountingUnitOfWork unitOfWork)
        {
            var headerParameters = new
            {
                DocumentEntry = header.DocumentEntry,
                DocumentNumber = header.DocumentNumber,
                Series = header.Series,
                CountDate = header.CountDate,
                CountTime = header.CountTime,
                SingleCounterType = header.SingleCounterType,
                SingleCounterID = header.SingleCounterID,
                DocumentStatus = header.DocumentStatus,
                Remarks = header.Remarks,
                DocObjectCodeEx = header.DocObjectCodeEx,
                FinancialPeriod = header.FinancialPeriod,
                PeriodIndicator = header.PeriodIndicator,
                CountingType = header.CountingType
            };

            var sql = ConfigurationManager.AppSettings["insert_counting_header"];
            await unitOfWork.InventoryCountingHeaderRepository
                    .SaveAsync(sql, headerParameters);
        }


        private async Task CountingCreateDetail(string docNum, InventoryCountingDetail detail, InventoryCountingUnitOfWork unitOfWork)
        {
            var detailParameters = new
            {
                DocumentEntry = docNum,
                LineNumber = detail.LineNumber,
                ItemCode = detail.ItemCode,
                ItemDescription = detail.ItemDescription,
                Freeze = detail.Freeze,
                InWarehouseQuantity = detail.InWarehouseQuantity,
                Counted = detail.Counted,
                UoMCode = detail.UoMCode,
                WarehouseCode = detail.WarehouseCode,
                CountedQuantity = detail.CountedQuantity,
                Variance = detail.Variance,
                VariancePercentage = detail.VariancePercentage,
                VisualOrder = detail.VisualOrder,
                TargetEntry = detail.TargetEntry,
                TargetLine = detail.TargetLine,
                TargetType = detail.TargetType,
                TargetReference = detail.TargetReference,
                ProjectCode = detail.ProjectCode,
                Manufacturer = detail.Manufacturer,
                LineStatus = detail.LineStatus,
                CounterType = detail.CounterType,
                CounterID = detail.CounterID,
                MultipleCounterRole = detail.MultipleCounterRole
            };

            var sql = ConfigurationManager.AppSettings["insert_counting_detail"];
            await unitOfWork.InventoryCountingDetailRepository
                .SaveAsync(sql, detailParameters);
        }



        private async Task CountingEditDetail(string docNum, InventoryCountingDetail detail, InventoryCountingUnitOfWork unitOfWork)
        {
            var detailParameters = new
            {
                DocumentEntry = docNum,
                LineNumber = detail.LineNumber,
                ItemCode = detail.ItemCode,
                ItemDescription = detail.ItemDescription,
                Freeze = detail.Freeze,
                InWarehouseQuantity = detail.InWarehouseQuantity,
                Counted = detail.Counted,
                UoMCode = detail.UoMCode,
                WarehouseCode = detail.WarehouseCode,
                CountedQuantity = detail.CountedQuantity,
                Variance = detail.Variance,
                VariancePercentage = detail.VariancePercentage,
                VisualOrder = detail.VisualOrder,
                TargetEntry = detail.TargetEntry,
                TargetLine = detail.TargetLine,
                TargetType = detail.TargetType,
                TargetReference = detail.TargetReference,
                ProjectCode = detail.ProjectCode,
                Manufacturer = detail.Manufacturer,
                LineStatus = detail.LineStatus,
                CounterType = detail.CounterType,
                CounterID = detail.CounterID,
                MultipleCounterRole = detail.MultipleCounterRole
            };

            var sql = ConfigurationManager.AppSettings["update_counting_detail"];
            await unitOfWork.InventoryCountingDetailRepository
                .SaveAsync(sql, detailParameters);
        }
        #endregion        

    }
}
