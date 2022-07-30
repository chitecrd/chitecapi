using DataAccess;
using DataAccess.Models;

namespace Infrastructure.Repositories
{
    public class TransferStockHeaderInRepository : GenericRepository<StockTranfersHeaderIn>
    {
        public TransferStockHeaderInRepository(DataBaseAccess dataBaseAccess) : base(dataBaseAccess)
        {

        }
    }
}
