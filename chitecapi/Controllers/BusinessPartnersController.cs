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
    public class BusinessPartnersController : ChitecApiController
    {
        /*[HttpGet]
        public async Task<IHttpActionResult> Listado(string db)
        {
            var connectionString = GetConnectionString(db);

            if (string.IsNullOrEmpty(connectionString))
            {
                return new CustomJsonActionResult(
                    System.Net.HttpStatusCode.NotFound,
                    new JsonErrorResponse(1, 400, $"La base de datos {db} no existe."));
            }

            using (var unitOfWork = new BusinessPartnerUnitOfWork(connectionString))
            {
                var sql = ConfigurationManager.AppSettings["buscar_business_partners"];
                var businessPartners = await unitOfWork.BusinessPartnerRepository.LoadAsync(sql, new { });
                var data = GetDataJson(businessPartners);

                return new CustomJsonActionResult(
                    System.Net.HttpStatusCode.OK,
                   new JsonDataResponse(data));
            }
        }
        */
        [HttpGet]
        public IHttpActionResult Listado(  [FromUri] string db,string saveresult = "0",  string tablename = "proveedores_test")
        {
            string conection = "db1";
            if (!db.Equals(""))
                conection = db;
            DataUtil dataUtil1 = new DataUtil(conection);
            dataUtil1.Connect();
            string sql1 = ConfigurationManager.AppSettings["buscar_business_partners"] ?? "select * from proveedores";
            dataUtil1.PrepareStatement(sql1);
            DataTable table = new DataTable();
            dataUtil1.FillDatatable(table);
            dataUtil1.CloseConnection();
            if (saveresult.Equals("1"))
            {
                DataUtil dataUtil2 = new DataUtil("dbconnection");
                dataUtil2.Connect();
                string str = JsonConvert.SerializeObject((object)table);
                string sql2 = "  exec createtablefromjson @tabla='" + tablename + "', @json='" + str + "'; ";
                dataUtil2.ExecuteCommand(sql2);
                dataUtil2.CloseConnection();
            }
            return (IHttpActionResult)this.Json<Dictionary<string, object>>(new Dictionary<string, object>()
      {
        {
          "data",
          (object) table
        },
        {
          "error",
          (object) 0
        },
        {
          "error_type",
          (object) 0
        },
        {
          "error_message",
          (object) 0
        }
      });
        }


        [HttpPost]
        public async Task<IHttpActionResult> Guardar([FromBody] List<BusinessPartner> businessPartners, string db)
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

            using (var unitOfWork = new BusinessPartnerUnitOfWork(connectionString,conection))
            {
                foreach (var businessPartner in businessPartners)
                {
                    if (businessPartner?.IsNotInitialized == true)
                    {
                        return new CustomJsonActionResult(
                            System.Net.HttpStatusCode.NotFound,
                            new JsonErrorResponse(1, 400, "Faltan parámetros"));
                    }

                    try
                    {
                        var sql = ConfigurationManager.AppSettings["create_business_partner"];
                        var parameters = new
                        {
                            businessPartner.BusinessName,
                            businessPartner.CardCode,
                            businessPartner.CardName,
                            businessPartner.CardType,
                            businessPartner.Notes,
                            businessPartner.FederalTaxId,
                            businessPartner.Currency,
                            businessPartner.VatLiable,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        };

                        await unitOfWork.BusinessPartnerRepository.SaveAsync(sql, parameters);
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
    }
}
