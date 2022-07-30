using DataAccess;
using System;
using System.Configuration;

namespace chitecapi.Jobs
{
    public class GenericJob
    {
        internal bool IsDisabled(string enabledConfigurationName)
        {
            if (bool.TryParse(enabledConfigurationName, out var isDisabled))
            {
                return isDisabled;
            }
            return true;
        }

        internal bool VerifyIsEnable(string enabledConfigurationName)
        {
            if (bool.TryParse(enabledConfigurationName, out var value))
            {
                return value;
            }
            return false;
        }

        internal void RegisterJobSuccess(
            bool isSuccessful, string errorMessage = "")
        {
            var db = $"{ConfigurationManager.AppSettings["default_db"]}";
            var dbAccess = new SqlServerDataAccess(ConfigurationManager.ConnectionStrings[db].ConnectionString);

            try
            {
                var parameters = new
                {
                    job = GetType().Name,
                    fue_completado = isSuccessful,
                    mensaje_error = errorMessage,
                    fecha = DateTime.Now
                };

                var sql = ConfigurationManager.AppSettings["guardar_historial_de_jobs"];

                dbAccess.SaveData(sql, parameters);
            }
            catch
            {
                // ignore
                // todo: log this exceptions with an email based logger or something similar.
            }
        }
    }
}