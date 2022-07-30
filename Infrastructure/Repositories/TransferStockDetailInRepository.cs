using DataAccess;
using DataAccess.Models;

namespace Infrastructure.Repositories
{
    public class TransferStockDetailInRepository : GenericRepository<StockTranfersDetailIn>
    {
        public TransferStockDetailInRepository(DataBaseAccess dataBaseAccess) : base(dataBaseAccess)
        {

        }
    }
}
