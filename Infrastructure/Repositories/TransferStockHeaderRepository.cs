using DataAccess;
using DataAccess.Models;

namespace Infrastructure.Repositories
{
    public class TransferStockHeaderRepository : GenericRepository<StockTranfersHeader>
    {
        public TransferStockHeaderRepository(DataBaseAccess dataBaseAccess) : base(dataBaseAccess)
        {

        }
    }
}
