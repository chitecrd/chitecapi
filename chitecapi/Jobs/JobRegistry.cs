using FluentScheduler;

namespace chitecapi.Jobs
{
    public class JobRegistry : Registry
    {
        public JobRegistry()
        {
            //Schedule an IJob to run at an interval
            Schedule<ConduceJob>().ToRunNow().AndEvery(5).Minutes();
            Schedule<ItemSapJob>().ToRunNow().AndEvery(15).Minutes();
            Schedule<ItemGroupSapJob>().ToRunNow().AndEvery(15).Minutes();
            Schedule<ItemCheckStockSapJob>().ToRunNow().AndEvery(15).Minutes();
            Schedule<WarehouseSapJob>().ToRunNow().AndEvery(15).Minutes();
            Schedule<BusinessPartnersSapJob>().ToRunNow().AndEvery(15).Minutes();
            Schedule<DetalleInventarioJob>().ToRunEvery(5).Seconds();
            //Schedule<ItemSapJob>().ToRunEvery(15).Minutes();

            //// Schedule an IJob to run once, delayed by a specific time interval
            //Schedule<InventoryDetailsJob>().ToRunOnceIn(5).Seconds();

            //// Schedule a simple job to run at a specific time
            //Schedule(() => Console.WriteLine("It's 9:15 PM now.")).ToRunEvery(1).Days().At(21, 15);

            //// Schedule a more complex action to run immediately and on an monthly interval
            //Schedule<InventoryDetailsJob>().ToRunNow().AndEvery(1).Months().OnTheFirst(DayOfWeek.Monday).At(3, 0);

            //// Schedule a job using a factory method and pass parameters to the constructor.
            //Schedule(() => new InventoryDetailsJob("Foo", DateTime.Now)).ToRunNow().AndEvery(2).Seconds();

            //// Schedule multiple jobs to be run in a single schedule
            //Schedule<InventoryDetailsJob>().AndThen<MyOtherJob>().ToRunNow().AndEvery(5).Minutes();
        }
    }
}