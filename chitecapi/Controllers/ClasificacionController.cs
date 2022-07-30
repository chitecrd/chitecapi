using chitecapi.Responses;
using DataAccess.Models;
using System.Collections.Generic;
using System.Configuration;
using System.Web.Http;

namespace chitecapi.Controllers
{
    public class ClasificacionController : ApiController
    {
        #region Actions

        public IHttpActionResult Guardar([FromBody] List<Clasificacion> clasificaciones, string db)
        {
            if (string.IsNullOrEmpty(db))
            {
                db = $"{ConfigurationManager.AppSettings["default_db"]}";
            }

            return new CustomJsonActionResult(
                    System.Net.HttpStatusCode.OK,
                    new JsonErrorResponse(0, 0, "0"));
        }

        public IHttpActionResult Editar([FromBody] List<Clasificacion> clasificaciones, string db)
        {
            if (string.IsNullOrEmpty(db))
            {
                db = $"{ConfigurationManager.AppSettings["default_db"]}";
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
