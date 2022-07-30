using chitecapi.Responses;
using DataAccess;
using DataAccess.Models;
using Infrastructure.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

namespace chitecapi.Controllers
{
    public class AsignacionesController : ChitecApiController
    {
        #region Actions

        /// <summary>
        /// Obtiene la lista de las asiganciones por usuario.
        /// </summary>
        /// <param name="db" example="db1">ID de la Base de datos</param>
        /// <param name="id_usuario" example="8">ID de usuario</param>
        /// <remarks>
        /// Se utiliza para obtener una lista de asignaciones por usuario, generado con un formato JSON:
        /// 
        ///     Example Result:
        ///     {
        ///       "data": [
        ///         {
        ///             "codigo_unico": "codigo_unico",
        ///             "cod_alterno": "cod",
        ///             "descripcion": "descripcion",
        ///             "id_usuario": "1",
        ///             "id_ayudante": "1",
        ///             "nombre_usuario": "admin",
        ///             "nombre_ayudante": "nombre",
        ///             "created_at": "06/10/2021 20:49:21",
        ///             "updated_at": "06/10/2021 20:49:21"
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
        public async Task<IHttpActionResult> Index(string db, string id_usuario)
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

            var connectionString = GetConnectionString(db);


            if (!int.TryParse(id_usuario, out var userId))
            {
                userId = 0;
            }

            try
            {
                using (var unitOfWork = new AsignacionesUnitOfWork(connectionString))
                {
                    var asignaciones =
                        await GetAsignaciones(userId, unitOfWork);

                    var data = GetDataJson(asignaciones);

                    return
                        new CustomJsonActionResult(HttpStatusCode.OK, new JsonDataResponse(data));
                }
            }
            catch (Exception exception)
            {
                return new CustomJsonActionResult(
                    HttpStatusCode.NotFound,
                    new JsonErrorResponse(1, 1, exception.GetBaseException().Message));
            }
        }

        private async Task<IEnumerable<Asignacion>> GetAsignaciones(int id_usuario, AsignacionesUnitOfWork unitOfWork)
        {
            var sql = ConfigurationManager.AppSettings["buscar_asignaciones_por_usuario"];

            if (id_usuario > 0)
            {
                sql += $" AND id_usuario = {id_usuario}";
            }
            return await unitOfWork.AsignacionesRepository.LoadAsync(sql, new { });
        }

        //// GET api/asignaciones/{value}/{value}
        //[HttpGet]
        //[Route("api/asignaciones/{id}/{idusuario}")]
        //public IHttpActionResult Get(string id, string idusuario)
        //{
        //    string connetionString;
        //    SqlConnection conection;
        //    String dbconection = "db1";
        //    if (!id.Equals(""))
        //    {
        //        dbconection = id;
        //    }
        //    connetionString = ConfigurationManager.ConnectionStrings[dbconection].ConnectionString;
        //    conection = new SqlConnection(connetionString);
        //    conection.Open();

        //    String sql = "";
        //    if (idusuario.Equals(""))
        //    {
        //        sql = ConfigurationManager.AppSettings["getasignaciones"];
        //    }
        //    else
        //    {
        //        sql = ConfigurationManager.AppSettings["getasignacionesbyusuario"];
        //    }

        //    if (sql == null)
        //    {
        //        sql = "select * from asignaciones";
        //    }

        //    SqlDataAdapter dataAdapter = new SqlDataAdapter(sql, conection);
        //    dataAdapter.SelectCommand.Parameters.AddWithValue("usuario", idusuario);
        //    DataTable table = new DataTable();
        //    dataAdapter.Fill(table);

        //    conection.Close();
        //    Dictionary<String, Object> jsonvalues = new Dictionary<string, object>();
        //    jsonvalues.Add("data", table);
        //    jsonvalues.Add("error", 0);
        //    jsonvalues.Add("error_type", 0);
        //    jsonvalues.Add("error_message", 0);


        //    return Json(jsonvalues);
        //}

        [HttpPost]
        public async Task<IHttpActionResult> Guardar([FromBody] List<Asignacion> partialAsignaciones, string db)
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

            foreach (var partialAsignacion in partialAsignaciones)
            {
                if (partialAsignacion?.IsNotInitialized == true)
                {
                    return new CustomJsonActionResult(
                        System.Net.HttpStatusCode.NotFound,
                        new JsonErrorResponse(1, 400, "Faltan parámetros"));
                }

                try
                {
                    var ayudanteName = await GetAyudanteName(partialAsignacion.id_ayudante, dbAccess);
                    var usuarioName = await GetUsuarioName(partialAsignacion.id_usuario, dbAccess);
                    var fullAsignacion = SetAsignacion(ayudanteName, usuarioName, partialAsignacion);

                    if (await IsUniqueAsignacion(fullAsignacion.codigo_unico, dbAccess))
                    {
                        await InsertAsignacion(fullAsignacion, dbAccess);
                    }
                    else
                    {
                        await EditDuplicatedAsignacion(fullAsignacion, dbAccess);
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

        #endregion

        #region Helper methods

        private async Task EditDuplicatedAsignacion(Asignacion asignacion, SqlServerDataAccess dbAccess)
        {
            var parameters = new
            {
                codigo_unico = asignacion.codigo_unico,
                cod_alterno = asignacion.cod_alterno,
                descripcion = asignacion.descripcion,
                id_usuario = asignacion.id_usuario,
                id_ayudante = asignacion.id_ayudante,
                nombre_usuario = asignacion.nombre_usuario,
                nombre_ayudante = asignacion.nombre_ayudante,
                updated_at = DateTime.Now
            };
            var sql = ConfigurationManager.AppSettings["editar_duplicado_asignacion"];

            await dbAccess.SaveDataAsync(sql, parameters);
        }

        private async Task InsertAsignacion(Asignacion asignacion, SqlServerDataAccess dbAccess)
        {
            var parameters = new
            {
                codigo_unico = asignacion.codigo_unico,
                cod_alterno = asignacion.cod_alterno,
                descripcion = asignacion.descripcion,
                id_usuario = asignacion.id_usuario,
                id_ayudante = asignacion.id_ayudante,
                nombre_usuario = asignacion.nombre_usuario,
                nombre_ayudante = asignacion.nombre_ayudante,
                created_at = DateTime.Now,
                updated_at = DateTime.Now
            };
            var sql = ConfigurationManager.AppSettings["crear_asignacion"];

            await dbAccess.SaveDataAsync(sql, parameters);
        }

        private async Task<bool> IsUniqueAsignacion(string uniqueId, SqlServerDataAccess dbAccess)
        {
            var sql = ConfigurationManager.AppSettings["buscar_asignacion_por_codigo_unico"];

            var duplicateData =
                await dbAccess.LoadFirstOrDefaultAsync<Asignacion, dynamic>(
                    sql, new { codigo_unico = uniqueId });

            return duplicateData == null;
        }

        private Asignacion SetAsignacion(string ayudanteName, string usuarioName, Asignacion partialAsignacion)
        {
            partialAsignacion.nombre_ayudante = ayudanteName;
            partialAsignacion.nombre_usuario = usuarioName;

            return partialAsignacion;
        }

        private async Task<string> GetUsuarioName(string usuarioId, SqlServerDataAccess dbAccess)
        {
            var sql = ConfigurationManager.AppSettings["buscar_usuario_por_id"];
            var usuario =
                await dbAccess.LoadFirstOrDefaultAsync<Usuario, dynamic>(sql, new { id = usuarioId });

            return usuario == null ?
                string.Empty : string.IsNullOrEmpty(usuario.nombre) ?
                string.Empty : usuario.nombre;
        }

        private async Task<string> GetAyudanteName(string ayudanteId, SqlServerDataAccess dbAccess)
        {
            var sql = ConfigurationManager.AppSettings["buscar_ayudante_por_id"];
            var ayudante =
                await dbAccess.LoadFirstOrDefaultAsync<Ayudante, dynamic>(
                    sql, new { id = ayudanteId });

            return ayudante == null ?
                string.Empty : string.IsNullOrEmpty(ayudante.nombre) ?
                string.Empty : ayudante.nombre;
        }

        #endregion       
    }
}
