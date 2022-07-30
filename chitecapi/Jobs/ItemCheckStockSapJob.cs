using chitecapi.Controllers;
using DataAccess;
using DataAccess.Models;
using FluentScheduler;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Web.Http;

namespace chitecapi.Jobs
{
    public class ItemCheckStockSapJob : GenericJob, IJob, IRegisteredObject
    {
        private readonly object _lock = new object();
        private bool shuttingDown;

        public void Execute()
        {
            try
            {
                lock (_lock)
                {
                    if (shuttingDown || (!VerifyIsEnable(ConfigurationManager.AppSettings["habilitar_itemcheckstocksapjob"])))
                    {
                        return;
                    }

                    var db = $"{ConfigurationManager.AppSettings["default_db"]}";

                    if (ConfigurationManager.ConnectionStrings[db] == null)
                    {
                        RegisterJobSuccess(false, $"La base de datos {db} no existe.");
                    }

                    SapController sapController = new SapController();
                    Task<IHttpActionResult> result = sapController.GetSapCheckItemsStock();


                    RegisterJobSuccess(true);
                }
            }
            catch (Exception exception)
            {
                RegisterJobSuccess(false, exception.GetBaseException().Message);
            }
            finally
            {
                HostingEnvironment.UnregisterObject(this);
            }
        }

        public void Stop(bool immediate)
        {
            lock (_lock)
            {
                shuttingDown = true;
            }

            HostingEnvironment.UnregisterObject(this);
        }

 
    }
}