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
    public class ArticulosController : ApiController
    {
        #region Actions

        // GET api/articulos/codigos_barra?db=db1&codigo_barra=2222
        /// <summary>
        /// Obtiene una lista de articulos.
        /// </summary>
        /// <param name="db" example="db1">The Database ID</param>
        /// <param name="no_articulo" example="111">Item ID</param>
        /// <param name="codigo_barra" example="111">Item CodeBar</param>
        /// <param name="alterno1" example="111">Alternative Number1</param>
        /// <param name="alterno2" example="111">Alternative Number2</param>
        /// <param name="alterno3" example="111">Alternative Number3</param>
        /// <param name="descripcion" example="Coca cola">Item Description</param>
        /// <remarks>
        /// Se utiliza para obtener la lista de articulos, generado con un formato JSON:
        /// 
        ///     Example Result:
        ///     {
        ///         "codigos_barras": [
        ///                   {
        ///                      "id": 2,
        ///                      "no_articulo": "2",
        ///                      "codigo_barra": "74641963      ",
        ///                      "created_at": "2021-04-28T20:23:47",
        ///                      "updated_at": "2021-04-28T20:23:47"
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
        public IHttpActionResult codigos_barra([FromUri] string db = "db1", string no_articulo = "0", string codigo_barra = "0", string alterno1 = "0", string alterno2 = "0", string alterno3 = "0", string descripcion = "", string saveresult = "0", string tablename = "codigobarratest")
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


            var sql = ConfigurationManager.AppSettings["getallcodigosbarra"];

            if (sql == null)
            {
                sql = "select id,no_articulo,codigo_barra,created_at,updated_at from articulos";
            }

            dataUtil.PrepareStatement(sql);


            if (!no_articulo.Equals("0") || !codigo_barra.Equals("0") || !alterno1.Equals("0") || !alterno2.Equals("0") || !alterno3.Equals("0") || !descripcion.Equals(""))
            {
                sql = ConfigurationManager.AppSettings["getallcodigosbarraby"];

                dataUtil.PrepareStatement(sql);

                dataUtil.AddParameter("@no_articulo", no_articulo);
                dataUtil.AddParameter("@codigo_barra", codigo_barra);
                dataUtil.AddParameter("@alterno1", alterno1);
                dataUtil.AddParameter("@alterno2", alterno2);
                dataUtil.AddParameter("@alterno3", alterno3);
                dataUtil.AddParameter("@descripcion", descripcion);
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
            jsonvalues.Add("codigos_barras", table);
            jsonvalues.Add("error", 0);
            jsonvalues.Add("error_type", 0);
            jsonvalues.Add("error_message", 0);


            return Json(jsonvalues);
        }

        // GET api/articulos/articulos_ubicaciones?db=db1&codigo_barra=2222
        /// <summary>
        /// Obtiene una lista de articulos.
        /// </summary>
        /// <param name="db" example="db1">The Database ID</param>
        /// <param name="no_articulo" example="111">Item ID</param>
        /// <param name="ubicacion" example="111">Ubicacion</param>
        /// <remarks>
        /// Se utiliza para obtener la lista de articulos dentro de una ubicacion, generado con un formato JSON:
        /// 
        ///     Example Result:
        ///     {
        ///         "data": [
        ///                   {
        ///                      "id": 2,
        ///                      "no_articulo": "2",
        ///                      "descripcion": "testing      ",
        ///                      "ubicacion": "2021-04-28T20:23:47",
        ///                      "codigo barras": "2021-04-28T20:23:47"
        ///                      "Tipo ubicacion": "2021-04-28T20:23:47"
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
        public IHttpActionResult articulos_ubicaciones([FromUri] string db = "db1", string no_articulo = "", string ubicacion = "", string saveresult = "0", string tablename = "articulos_ubicaciones_test")
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



            var sql = ConfigurationManager.AppSettings["getarticulos_ubicacion"];

            if (sql == null)
            {
                sql = "SELECT * FROM articulos_ubicaciones where 1=1";
            }

            if (!no_articulo.Equals(""))
            {
                sql = sql + $" and (No_Articulo = '{no_articulo}' or Codigo_Barras = '{no_articulo}') ";
            }


            if (!ubicacion.Equals(""))
            {
                sql = sql + $" and Ubicacion = '{ubicacion}' ";
            }



            DataTable table = new DataTable();
            dataUtil.FillDatatable(sql, table);

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


        // GET api/articulos/ListaArticulos?db=db1&codigo_barra=2222
        /// <summary>
        /// Obtiene una lista de articulos.
        /// </summary>
        /// <param name="db" example="db1">The Database ID</param>
        /// <param name="no_articulo" example="111">Item ID</param>
        /// <param name="codigo_barra" example="111">Item CodeBar</param>
        /// <param name="alterno1" example="111">Alternative Number1</param>
        /// <param name="alterno2" example="111">Alternative Number2</param>
        /// <param name="alterno3" example="111">Alternative Number3</param>
        /// <param name="descripcion" example="Coca cola">Item Description</param>
        /// <remarks>
        /// Se utiliza para obtener la lista de articulos, generado con un formato JSON:
        /// 
        ///     Example Result:
        ///     {
        ///         "data": [
        ///                   {
        ///                      "id": 2,
        ///                      "no_articulo": "2",
        ///                      "codigo_barra": "74641963      ",
        ///                      "alterno1": "",
        ///                      "alterno2": "",
        ///                      "alterno3": "",
        ///                      "descripcion": "BRUGAL CARTA DORADA 350ML               ",
        ///                      "existencia": 93,
        ///                      "unidad_medida": "1",
        ///                      "costo": 146.47,
        ///                      "precio": 215,
        ///                      "referencia": "1",
        ///                      "marca": "",
        ///                      "familia": "",
        ///                      "created_at": "2021-04-28T20:23:47",
        ///                      "updated_at": "2021-04-28T20:23:47"
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
        public IHttpActionResult ListaArticulos([FromUri] string db = "db1", string no_articulo = "0", string codigo_barra = "0", string alterno1 = "0", string alterno2 = "0", string alterno3 = "0", string descripcion = "", string saveresult = "0",string tablename= "articulos_test")
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


            var sql = ConfigurationManager.AppSettings["getallarticulos"];

            if (sql == null)
            {
                sql = "select * from articulos";
            }

            dataUtil.PrepareStatement(sql);
            

            if (!no_articulo.Equals("0") || !codigo_barra.Equals("0") || !alterno1.Equals("0") || !alterno2.Equals("0") || !alterno3.Equals("0") || !descripcion.Equals(""))
            {
                sql = ConfigurationManager.AppSettings["getallarticulosby"];
                dataUtil.PrepareStatement(sql);

                
                dataUtil.AddParameter("@no_articulo", no_articulo);
                dataUtil.AddParameter("@codigo_barra", codigo_barra);
                dataUtil.AddParameter("@alterno1", alterno1);
                dataUtil.AddParameter("@alterno2", alterno2);
                dataUtil.AddParameter("@alterno3", alterno3);
                dataUtil.AddParameter("@descripcion", descripcion);
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


            return Json(table);
        }


        // GET api/articulos/nro_articulos
        /// <summary>
        /// Obtiene la cantidad total de los articulos.
        /// </summary>
        /// <param name="db" example="db1">The Database ID</param>
        /// <remarks>
        /// Se utiliza para obtener la cantidad total de articulos que existen en el catalogo , generado con un formato JSON:
        /// 
        ///     Example Result:
        ///     {
        ///         "nro_articulos": 2795,
        ///         "error": 0,
        ///         "error_type": 0,
        ///         "error_message": 0
        ///     }
        /// </remarks>
        /// <response code="401">Unauthorized. Error en la configuracion de la base de datos</response>              
        /// <response code="200">OK. Devuelve el objeto solicitado.</response>        
        /// <response code="404">NotFound. No se ha encontrado el objeto solicitado.</response>
        [HttpGet]
        public IHttpActionResult nro_articulos([FromUri] string db = "db1", string saveresult = "0", string tablename = "no_articulos_test")
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


            var sql = ConfigurationManager.AppSettings["getnumeroarticulos"];

            if (sql == null)
            {
                sql = "SELECT COUNT(1) as nro_articulos FROM articulos";
            }


            
            DataTable table = new DataTable();
            dataUtil.FillDatatable(sql, table);
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
            jsonvalues.Add("nro_articulos", table.Rows[0][0]);
            jsonvalues.Add("error", 0);
            jsonvalues.Add("error_type", 0);
            jsonvalues.Add("error_message", 0);


            return Json(jsonvalues);
        }




        // GET api/articulos/costo_total_articulos
        /// <summary>
        /// Obtiene el costo total de los articulos.
        /// </summary>
        /// <param name="db" example="db1">The Database ID</param>
        /// <remarks>
        /// Se utiliza para obtener el costo total de los articulos que existen en el catalogo , generado con un formato JSON:
        /// 
        ///     Example Result:
        ///     {
        ///         "costo_total": 2795,
        ///         "error": 0,
        ///         "error_type": 0,
        ///         "error_message": 0
        ///     }
        /// </remarks>
        /// <response code="401">Unauthorized. Error en la configuracion de la base de datos</response>              
        /// <response code="200">OK. Devuelve el objeto solicitado.</response>        
        /// <response code="404">NotFound. No se ha encontrado el objeto solicitado.</response>
        [HttpGet]
        public IHttpActionResult costo_total_articulos([FromUri] string db = "db1", string saveresult = "0", string tablename = "costo_total_test")
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

            var sql = ConfigurationManager.AppSettings["costo_total_articulos"];

            if (sql == null)
            {
                sql = "SELECT SUM(costo * coalesce(existencia, 0)) as costo_total FROM articulos";
            }


          
            DataTable table = new DataTable();
            dataUtil.FillDatatable(sql,table);

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



        // GET api/articulos/precio_total_articulos
        /// <summary>
        /// Obtiene el precio total de los articulos.
        /// </summary>
        /// <param name="db" example="db1">The Database ID</param>
        /// <remarks>
        /// Se utiliza para obtener el precio total de los articulos que existen en el catalogo , generado con un formato JSON:
        /// 
        ///     Example Result:
        ///     {
        ///         "precio_total": 2795,
        ///         "error": 0,
        ///         "error_type": 0,
        ///         "error_message": 0
        ///     }
        /// </remarks>
        /// <response code="401">Unauthorized. Error en la configuracion de la base de datos</response>              
        /// <response code="200">OK. Devuelve el objeto solicitado.</response>        
        /// <response code="404">NotFound. No se ha encontrado el objeto solicitado.</response>
        [HttpGet]
        public IHttpActionResult precio_total_articulos([FromUri] string db = "db1", string saveresult = "0", string tablename = "precio_total_test")
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


            var sql = ConfigurationManager.AppSettings["precio_total_articulos"];

            if (sql == null)
            {
                sql = "SELECT SUM(precio * coalesce(existencia, 0)) as precio_total FROM articulos";
            }



            DataTable table = new DataTable();
            dataUtil.FillDatatable(sql,table);

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

        [HttpPost]
        public async Task<IHttpActionResult> SimulateSapLogin([FromBody] Saplogin login)
        {

            string result = "N/A";

              

                try
                {

                    if (login.CompanyDB.Equals("DB_") && login.UserName.Equals("PC1") && login.Password.Equals("1234") )
                    {
                        result = "Correct";
                    }
                    else
                    {
                        result = "Invalid User";
                    }
                    


                }
                catch (Exception exception)
                {
                    return new CustomJsonActionResult(
                        System.Net.HttpStatusCode.NotFound,
                        new JsonErrorResponse(1, 1, exception.GetBaseException().Message));
                }
           
            return new CustomJsonActionResult(
                        System.Net.HttpStatusCode.OK,
                        new JsonErrorResponse(0, 0, result));
        }


        /// <summary>
        /// Guardar Articulos.
        /// </summary>
        /// <param name="db" example="db1">The Database ID</param>
        /// <remarks>
        /// Se utiliza para Guardar multiples articulos, Enviando un json con la información del articulo
        /// </remarks>
        /// <response code="401">Unauthorized. Error en la configuracion de la base de datos</response>              
        /// <response code="200">OK. Devuelve el objeto solicitado.</response>        
        /// <response code="404">NotFound. No se ha encontrado el objeto solicitado.</response>
        [HttpPost]
        public async Task<IHttpActionResult> Guardar([FromBody] List<Articulo> articulos, string db)
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

            foreach (var articulo in articulos)
            {
                if (articulo?.IsNotInitialized == true)
                {
                    return new CustomJsonActionResult(
                        System.Net.HttpStatusCode.NotFound,
                        new JsonErrorResponse(1, 400, "Faltan parámetros"));
                }

                try
                {

                    var articulodata = await GetArticulo(articulo.no_articulo, dbAccess);

                    if (articulodata == null)
                    {
                        var sql = ConfigurationManager.AppSettings["crear_articulo"];
                        var parameters = new
                        {
                            articulo.no_articulo,
                            articulo.codigo_barra,
                            articulo.alterno1,
                            articulo.alterno2,
                            articulo.alterno3,
                            articulo.descripcion,
                            articulo.existencia,
                            articulo.unidad_medida,
                            articulo.costo,
                            articulo.precio,
                            referencia = string.IsNullOrEmpty(articulo.referencia) ? string.Empty : articulo.referencia,
                            marca = string.IsNullOrEmpty(articulo.marca) ? string.Empty : articulo.marca,
                            familia = string.IsNullOrEmpty(articulo.familia) ? string.Empty : articulo.familia,
                            created_at = DateTime.Now,
                            updated_at = DateTime.Now
                        };

                        await dbAccess.SaveDataAsync(sql, parameters);
                    }
                    else
                    {
                        var sql = BuildUpdateSql(articulo);

                        var parameters = new
                        {
                            articulo.no_articulo,
                            articulo.codigo_barra,
                            articulo.alterno1,
                            articulo.alterno2,
                            articulo.alterno3,
                            articulo.descripcion,
                            articulo.existencia,
                            articulo.unidad_medida,
                            articulo.costo,
                            articulo.precio,
                            referencia = string.IsNullOrEmpty(articulo.referencia) ? string.Empty : articulo.referencia,
                            marca = string.IsNullOrEmpty(articulo.marca) ? string.Empty : articulo.marca,
                            familia = string.IsNullOrEmpty(articulo.familia) ? string.Empty : articulo.familia,
                            updated_at = DateTime.Now
                        };

                        await dbAccess.SaveDataAsync(sql, parameters);
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

        private async Task<Articulo> GetArticulo(
           string articuloNumber, SqlServerDataAccess dbAccess)
        {
            var sql = ConfigurationManager.AppSettings["buscar_articulo_por_numero"];

            return
                await dbAccess.LoadFirstOrDefaultAsync<Articulo, dynamic>(
                    sql, new { no_articulo = articuloNumber });
        }


        /// <summary>
        /// Editar Articulos.
        /// </summary>
        /// <param name="db" example="db1">The Database ID</param>
        /// <remarks>
        /// Se utiliza para Editar multiples articulos, Enviando un json con la información del articulo
        /// </remarks>
        /// <response code="401">Unauthorized. Error en la configuracion de la base de datos</response>              
        /// <response code="200">OK. Devuelve el objeto solicitado.</response>        
        /// <response code="404">NotFound. No se ha encontrado el objeto solicitado.</response>

        [HttpPost]
        public async Task<IHttpActionResult> Editar([FromBody] List<Articulo> articulos, string db)
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

            foreach (var articulo in articulos)
            {
                if (articulo?.IsNotInitialized == true)
                {
                    return new CustomJsonActionResult(
                        System.Net.HttpStatusCode.NotFound,
                        new JsonErrorResponse(1, 400, "Faltan parámetros"));
                }
                try
                {
                    var sql = BuildUpdateSql(articulo);

                    var parameters = new
                    {
                        articulo.no_articulo,
                        articulo.codigo_barra,
                        articulo.alterno1,
                        articulo.alterno2,
                        articulo.alterno3,
                        articulo.descripcion,
                        articulo.existencia,
                        articulo.unidad_medida,
                        articulo.costo,
                        articulo.precio,
                        referencia = string.IsNullOrEmpty(articulo.referencia) ? string.Empty : articulo.referencia,
                        marca = string.IsNullOrEmpty(articulo.marca) ? string.Empty : articulo.marca,
                        familia = string.IsNullOrEmpty(articulo.familia) ? string.Empty : articulo.familia,
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

        private string BuildUpdateSql(Articulo articulo)
        {
            if (articulo.referencia == null
                && articulo.marca == null
                && articulo.familia == null)
            {
                return ConfigurationManager.AppSettings["editar_articulo"];
            }

            var sql = "UPDATE articulos SET codigo_barra = @codigo_barra, alterno1 = @alterno1, alterno2 = @alterno2, alterno3 = @alterno3, descripcion = @descripcion, existencia = @existencia, unidad_medida = @unidad_medida, costo = @costo, precio = @precio, updated_at = @updated_at ";

            if (articulo.referencia != null)
            {
                sql += ", referencia = @referencia";
            }

            if (articulo.marca != null)
            {
                sql += ", marca = @marca";
            }

            if (articulo.familia != null)
            {
                sql += ", familia = @familia";
            }

            sql += " WHERE no_articulo = @no_articulo";

            return sql;
        }

        #endregion
    }
}
