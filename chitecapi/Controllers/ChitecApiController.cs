using Newtonsoft.Json;
using System.Collections.Generic;
using System.Configuration;
using System.Web.Http;

namespace chitecapi.Controllers
{
    public class ChitecApiController : ApiController
    {
        protected string GetConnectionString(string db)
        {
            if (string.IsNullOrEmpty(db))
            {
                db = $"{ConfigurationManager.AppSettings["default_db"]}";
            }

            if (ConfigurationManager.ConnectionStrings[db] == null)
            {
                return null;
            }

            return ConfigurationManager.ConnectionStrings[db].ConnectionString;
        }

        protected string GetDataJson<T>(IEnumerable<T> model)
        {
            return JsonConvert.SerializeObject(model);
        }

        protected string GetDataJson<T>(T model)
        {
            return JsonConvert.SerializeObject(model);
        }
    }
}
