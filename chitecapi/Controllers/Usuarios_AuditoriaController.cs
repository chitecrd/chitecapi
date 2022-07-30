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
    public class Usuarios_AuditoriaController : ApiController
    {
        #region Actions

        // GET api/Usuarios_Auditoria/ListadoUsuariosAuditoria?db=db1
        /// <summary>
        /// Obtiene una lista de los usuarios de auditoria.
        /// </summary>
        /// <param name="db" example="db1">The Database ID</param>
        /// <remarks>
        /// Se utiliza para obtener una lista de los usuarios de auditoria, generado con un formato JSON:
        /// 
        ///     Example Result:
        ///     {
        ///         "data": [
        ///                   {
        ///                       "id": 1,
        ///                       "nombre": "au01",
        ///                       "apellido": "au01",
        ///                       "usuario": "au01",
        ///                       "password": "123",
        ///                       "estado": 1,
        ///                       "tipo_usuario": 1,
        ///                       "created_at": "0000-00-00 00:00:00",
        ///                       "updated_at": "0000-00-00 00:00:00",
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
        public IHttpActionResult ListadoUsuariosAuditoria([FromUri] string db = "db1", string saveresult = "0", string tablename = "usuariosauditoriatest")
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




            var sql = ConfigurationManager.AppSettings["getusuariosauditoria"];

            if (sql == null)
            {
                sql = " SELECT * from usuarios_auditoria";
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
        public async Task<IHttpActionResult> Guardar([FromBody] List<UsuarioAuditoria> usuariosAuditoria, string db)
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

            foreach (var usuarioAuditoria in usuariosAuditoria)
            {
                if (usuarioAuditoria?.IsNotInitialized == true)
                {
                    return new CustomJsonActionResult(
                        System.Net.HttpStatusCode.NotFound,
                        new JsonErrorResponse(1, 400, "Faltan parámetros"));
                }

                try
                {
                    var sql = ConfigurationManager.AppSettings["crear_usuario_auditoria"];
                    var parameters = new
                    {
                        nombre = usuarioAuditoria.nombre,
                        apellido = usuarioAuditoria.apellido,
                        usuario = usuarioAuditoria.usuario,
                        password = usuarioAuditoria.password,
                        estado = usuarioAuditoria.estado,
                        tipo_usuario = usuarioAuditoria.tipo_usuario,
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

        #region HelperMethods



        #endregion
    }
}
