using DataAccess;
using DataAccess.Models;

namespace Infrastructure.Repositories
{
    public class TransferStockDetailRepository : GenericRepository<StockTranfersDetail>
    {
        public TransferStockDetailRepository(DataBaseAccess dataBaseAccess) : base(dataBaseAccess)
        {

        }
    }
}
