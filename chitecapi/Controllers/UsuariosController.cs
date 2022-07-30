using chitecapi.Responses;
using DataAccess.Models;
using Infrastructure.UnitOfWork;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
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
    public class UsuariosController : ChitecApiController
    {
        #region Actions

        // GET api/Usuarios/ListadoUsuarios?db=db1
        /// <summary>
        /// Obtiene una lista de los usuarios.
        /// </summary>
        /// <param name="db" example="db1">The Database ID</param>
        /// <remarks>
        /// Se utiliza para obtener una lista de los usuarios, generado con un formato JSON:
        /// 
        ///     Example Result:
        ///     {
        ///         "data": [
        ///                   {
        ///                       "id": 1,
        ///                       "nombre": "us01",
        ///                       "apellido": "us01",
        ///                       "usuario": "us01",
        ///                       "password": "123",
        ///                       "estado": 1,
        ///                       "tipo_usuario": 1,
        ///                       "created_at": "0000-00-00 00:00:00",
        ///                       "updated_at": "0000-00-00 00:00:00",
        ///                       "nro_tiros": 0
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
        public IHttpActionResult ListadoUsuarios([FromUri] string db = "db1", string saveresult = "0", string tablename = "usuariostest")
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





            var sql = ConfigurationManager.AppSettings["getusuarios"];

            if (sql == null)
            {
                sql = " SELECT u.*,  (SELECT COUNT(1) FROM detalle_inventario d WHERE d.id_usuario_registro = u.id) as nro_tiros FROM usuarios u";
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
        public async Task<IHttpActionResult> Guardar([FromBody] List<Usuario> usuarios, string db)
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

            using (var unitOfWork = new UsuarioUnitOfWork(connectionString,conection))
            {
                foreach (var usuario in usuarios)
                {
                    if (usuario?.IsNotInitialized == true)
                    {
                        return new CustomJsonActionResult(
                            System.Net.HttpStatusCode.NotFound,
                            new JsonErrorResponse(1, 400, "Faltan parámetros"));
                    }

                    try
                    {
                        var sql = ConfigurationManager.AppSettings["crear_usuario"];
                        var parameters = new
                        {
                            nombre = usuario.nombre,
                            apellido = usuario.apellido,
                            usuario = usuario.usuario,
                            password = usuario.password,
                            estado = usuario.estado,
                            created_at = DateTime.Now,
                            updated_at = DateTime.Now
                        };

                        await unitOfWork.UsuariosRepository.SaveAsync(sql, parameters);
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
        public async Task<IHttpActionResult> Editar([FromBody] List<Usuario> usuarios, string db)
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

            using (var unitOfWork = new UsuarioUnitOfWork(connectionString,conection))
            {
                foreach (var usuario in usuarios)
                {
                    if (usuario?.IsNotInitialized == true)
                    {
                        return new CustomJsonActionResult(
                            System.Net.HttpStatusCode.NotFound,
                            new JsonErrorResponse(1, 400, "Faltan parámetros"));
                    }

                    try
                    {
                        var sql = ConfigurationManager.AppSettings["editar_usuario"];
                        var parameters = new
                        {
                            id = usuario.id,
                            nombre = usuario.nombre,
                            apellido = usuario.apellido,
                            usuario = usuario.usuario,
                            password = usuario.password,
                            estado = usuario.estado,
                            updated_at = DateTime.Now
                        };

                        await unitOfWork.UsuariosRepository.SaveAsync(sql, parameters);
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


        #endregion        
    }
}
