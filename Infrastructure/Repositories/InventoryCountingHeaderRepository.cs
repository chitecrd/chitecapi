using DataAccess;
using DataAccess.Models;

namespace Infrastructure.Repositories
{
    public class InventoryCountingHeaderRepository : GenericRepository<InventoryCountingHeader>
    {
        public InventoryCountingHeaderRepository(DataBaseAccess dataBaseAccess) : base(dataBaseAccess)
        {

        }
    }
}
