using DataAccess;
using DataAccess.Models;

namespace Infrastructure.Repositories
{
    public class PurchaseOrderHeaderRepository : GenericRepository<PurchaseOrderHeader>
    {
        public PurchaseOrderHeaderRepository(DataBaseAccess dataBaseAccess) : base(dataBaseAccess)
        { }
    }
}
