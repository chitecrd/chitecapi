using chitecapi.Responses;
using DataAccess;
using System;
using System.Configuration;
using System.Threading.Tasks;
using System.Web.Http;

namespace chitecapi.Controllers
{
    public class Base_DatosController : ApiController
    {
        #region Actions

        [HttpPost]
        public async Task<IHttpActionResult> Crear(string nombre_base_datos)
        {
            if (string.IsNullOrEmpty(nombre_base_datos))
            {
                return new CustomJsonActionResult(
                    System.Net.HttpStatusCode.NotFound,
                    new JsonErrorResponse(1, 400, "Faltan parámetros"));
            }

            var db = $"{ConfigurationManager.AppSettings["default_db"]}";

            try
            {
                var dbAccess =
                    new SqlServerDataAccess(ConfigurationManager.ConnectionStrings[db].ConnectionString);

                if (await DataBaseExists(dbAccess, nombre_base_datos))
                {
                    return new CustomJsonActionResult(
                        System.Net.HttpStatusCode.OK,
                        new JsonErrorResponse(1, 1, $"La base de datos {nombre_base_datos} ya existe"));
                }

                await CreateDb(dbAccess, nombre_base_datos);
                await CreateTables(dbAccess, nombre_base_datos);
                await CreateViews(dbAccess, nombre_base_datos);
                await PopulateTables(dbAccess, nombre_base_datos);

                return new CustomJsonActionResult(
                        System.Net.HttpStatusCode.OK,
                        new JsonErrorResponse(0, 0, "0"));
            }
            catch (Exception exception)
            {
                return new CustomJsonActionResult(
                    System.Net.HttpStatusCode.NotFound,
                    new JsonErrorResponse(1, 1, exception.GetBaseException().Message));
            }
        }

        #endregion

        #region Helper methods

        private async Task<bool> DataBaseExists(SqlServerDataAccess dbAccess, string dbName)
        {
            var sql = $"{ConfigurationManager.AppSettings["verificar_existencia_base_datos"]}";
            var parameters = new
            {
                nombre_base_datos = dbName
            };
            return await dbAccess.ValidateAsync<dynamic>(sql, parameters);
        }

        private async Task CreateDb(SqlServerDataAccess dbAccess, string dbName)
        {
            var sql = $"{ConfigurationManager.AppSettings["crear_base_datos"]} {dbName}";

            await dbAccess.SaveDataAsync(sql, new { });
        }

        private async Task CreateTables(SqlServerDataAccess dbAccess, string dbName)
        {
            string[] tableCreationStatements = {
                ConfigurationManager.AppSettings["crear_tabla_detalle_inventario"],
                ConfigurationManager.AppSettings["crear_tabla_articulos"],
                ConfigurationManager.AppSettings["crear_tabla_articulos_temp"],
                ConfigurationManager.AppSettings["crear_tabla_tokens"],
                ConfigurationManager.AppSettings["crear_tabla_ubicaciones"],
                ConfigurationManager.AppSettings["crear_tabla_ubicaciones_temp"],
                ConfigurationManager.AppSettings["crear_tabla_tipo_ubicacion"],
                ConfigurationManager.AppSettings["crear_tabla_cuenta"],
                ConfigurationManager.AppSettings["crear_tabla_detalle_auditorias"],
                ConfigurationManager.AppSettings["crear_tabla_tipo_error"],
                ConfigurationManager.AppSettings["crear_tabla_tipo_auditorias"],
                ConfigurationManager.AppSettings["crear_tabla_usuarios_auditoria"],
                ConfigurationManager.AppSettings["crear_tabla_usuarios"],
                ConfigurationManager.AppSettings["crear_tabla_usuarios_temp"],
                ConfigurationManager.AppSettings["crear_tabla_ayudantes"],
                ConfigurationManager.AppSettings["crear_tabla_ayudantes_temp"],
                ConfigurationManager.AppSettings["crear_tabla_asignaciones"],
                ConfigurationManager.AppSettings["crear_tabla_asignaciones_temp"],
                ConfigurationManager.AppSettings["crear_tabla_detalle_inventario_ciclico"],
                ConfigurationManager.AppSettings["crear_tabla_clasificacion"],
                ConfigurationManager.AppSettings["crear_tabla_familias_productos"],
                ConfigurationManager.AppSettings["crear_tabla_codigos_barras"],
                ConfigurationManager.AppSettings["crear_tabla_codigos_barras_temp"],
                ConfigurationManager.AppSettings["crear_tabla_detalle_inventario_transitorio"],
                ConfigurationManager.AppSettings["crear_tabla_historial_de_jobs"],
                ConfigurationManager.AppSettings["crear_tabla_purchase_order_header_batch"],
                ConfigurationManager.AppSettings["crear_tabla_purchase_order_detail_batch"],
                ConfigurationManager.AppSettings["crear_tabla_purchase_order_header"],
                ConfigurationManager.AppSettings["crear_tabla_purchase_order_detail"],
                ConfigurationManager.AppSettings["crear_tabla_business_partners"],
                ConfigurationManager.AppSettings["crear_tabla_articulos_ubicaciones"],
                ConfigurationManager.AppSettings["crear_tabla_articulos_ubicaciones_temp"],
                ConfigurationManager.AppSettings["crear_tabla_codigos_inexistentes"],
                ConfigurationManager.AppSettings["crear_tabla_logs_impresion"]
            };

            await ExecuteStatements(dbName, tableCreationStatements, new { }, dbAccess);
        }


        private async Task CreateViews(SqlServerDataAccess dbAccess, string dbName)
        {
            string[] tableCreationStatements = {
                ConfigurationManager.AppSettings["crear_vista_ubicaciones"]
            };

            await ExecuteStatementsViews(dbName, tableCreationStatements, new { }, dbAccess);
        }

        private async Task PopulateTables(SqlServerDataAccess dbAccess, string dbName)
        {
            string[] insertStatements = {
                ConfigurationManager.AppSettings["insertar_datos_usuarios"],
                ConfigurationManager.AppSettings["insertar_datos_tipo_ubicacion1"],
                ConfigurationManager.AppSettings["insertar_datos_tipo_ubicacion2"],
                ConfigurationManager.AppSettings["insertar_datos_tipo_auditorias1"],
                ConfigurationManager.AppSettings["insertar_datos_tipo_auditorias2"],
                ConfigurationManager.AppSettings["insertar_datos_tipo_auditorias3"],
                ConfigurationManager.AppSettings["insertar_datos_tipo_auditorias4"],
                ConfigurationManager.AppSettings["insertar_datos_usuarios_auditoria"],
                ConfigurationManager.AppSettings["insertar_datos_clasificacion1"],
                ConfigurationManager.AppSettings["insertar_datos_clasificacion2"],
                ConfigurationManager.AppSettings["insertar_datos_clasificacion3"]
            };

            await ExecuteStatements(dbName, insertStatements, new { TodayDate = DateTime.Now }, dbAccess);
        }



        private async Task ExecuteStatements(string dbName, string[] statements, dynamic parameters, SqlServerDataAccess dbAccess)
        {
            foreach (var statement in statements)
            {
                string sql = $"USE {dbName} {statement}";
                await dbAccess.SaveDataAsync(sql, parameters);
            }
        }

        private async Task ExecuteStatementsViews(string dbName, string[] statements, dynamic parameters, SqlServerDataAccess dbAccess)
        {
            foreach (var statement in statements)
            {
                string sql = $"{statement} ";
                await dbAccess.SaveDataAsync(sql, parameters,dbName);
            }
        }

        #endregion
    }
}
