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
    public class AyudantesController : ChitecApiController
    {
        #region Actions

        /// <summary>
        /// Obtiene la lista de ayudantes.
        /// </summary>
        /// <param name="db" example="db1">ID de la Base de datos</param>        
        /// <remarks>
        /// Se utiliza para obtener una lista de ayudantes, generado con un formato JSON:
        /// 
        ///     Example Result:
        ///     {
        ///       "data": [
        ///         {
        ///             "codigo_unico": "0001",
        ///             "nombre": "nombre",
        ///             "created_at": "06/10/2021 00:00:34",
        ///             "updated_at": "06/10/2021 00:00:34"
        ///             }
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
        public async Task<IHttpActionResult> Index(string db)
        {
            if (string.IsNullOrEmpty(db))
            {
                db = $"{ConfigurationManager.AppSettings["default_db"]}";
            }

            if (ConfigurationManager.ConnectionStrings[db] == null)
            {
                return new CustomJsonActionResult(
                    HttpStatusCode.NotFound,
                    new JsonErrorResponse(1, 400, $"La base de datos {db} no existe."));
            }

            var connectionString = GetConnectionString(db);

            try
            {
                using (var unitOfWork = new AyudanteUnitOfWork(connectionString))
                {
                    var ayudantes = await GetAyudantes(unitOfWork);

                    var data = GetDataJson(ayudantes);

                    return new CustomJsonActionResult(HttpStatusCode.OK, new JsonDataResponse(data));
                }
            }
            catch (Exception exception)
            {
                return new CustomJsonActionResult(
                    HttpStatusCode.NotFound,
                    new JsonErrorResponse(1, 1, exception.GetBaseException().Message));
            }
        }

        [HttpPost]
        public async Task<IHttpActionResult> Guardar([FromBody] List<Ayudante> ayudantes, string db)
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

            foreach (var ayudante in ayudantes)
            {
                if (ayudante?.IsNotInitialized == true)
                {
                    return new CustomJsonActionResult(
                        System.Net.HttpStatusCode.NotFound,
                        new JsonErrorResponse(1, 400, "Faltan parámetros"));
                }

                try
                {
                    if (await IsUniqueAyudante(ayudante.codigo_unico, dbAccess))
                    {
                        await InsertAyudante(ayudante, dbAccess);
                    }
                    else
                    {
                        await EditDuplicatedAyudante(ayudante, dbAccess);
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
                System.Net.HttpStatusCode.NotFound,
                new JsonErrorResponse(0, 0, "0"));
        }

        #endregion

        #region Helper Methods

        private async Task EditDuplicatedAyudante(Ayudante ayudante, SqlServerDataAccess dbAccess)
        {
            var sql = ConfigurationManager.AppSettings["editar_ayudante"];
            var parameters = new
            {
                codigo_unico = ayudante.codigo_unico,
                nombre = ayudante.nombre,
                updated_at = DateTime.Now
            };

            await dbAccess.SaveDataAsync(sql, parameters);
        }

        private async Task InsertAyudante(Ayudante ayudante, SqlServerDataAccess dbAccess)
        {
            var sql = ConfigurationManager.AppSettings["crear_ayudante"];
            var parameters = new
            {
                codigo_unico = ayudante.codigo_unico,
                nombre = ayudante.nombre,
                created_at = DateTime.Now,
                updated_at = DateTime.Now
            };

            await dbAccess.SaveDataAsync(sql, parameters);
        }

        private async Task<bool> IsUniqueAyudante(string uniqueId, SqlServerDataAccess dbAccess)
        {
            var sql = ConfigurationManager.AppSettings["buscar_ayudante_por_codigo_unico"];

            var ayudante =
                await dbAccess.LoadFirstOrDefaultAsync<Ayudante, dynamic>(
                    sql, new { codigo_unico = uniqueId });

            return ayudante == null;
        }

        private async Task<IEnumerable<Ayudante>> GetAyudantes(AyudanteUnitOfWork unitOfWork)
        {
            var sql = ConfigurationManager.AppSettings["buscar_ayudantes"];

            return
                await unitOfWork.AyudanteRepository.LoadAsync(sql, new { });
        }

        #endregion       
    }
}