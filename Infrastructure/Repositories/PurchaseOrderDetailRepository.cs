using DataAccess;
using DataAccess.Models;

namespace Infrastructure.Repositories
{
    public class PurchaseOrderDetailRepository : GenericRepository<PurchaseOrderDetail>
    {
        public PurchaseOrderDetailRepository(DataBaseAccess dataBaseAccess) : base(dataBaseAccess)
        {

        }
    }
}
