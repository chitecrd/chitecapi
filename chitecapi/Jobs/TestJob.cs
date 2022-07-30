using FluentScheduler;
using System;
using System.Web.Hosting;

namespace chitecapi.Jobs
{
    public class TestJob : GenericJob, IJob, IRegisteredObject
    {
        private readonly object _lock = new object();
        private bool shuttingDown;

        public void Execute()
        {
            try
            {
                lock (_lock)
                {
                    if (shuttingDown)
                        return;

                    // Do stuff

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