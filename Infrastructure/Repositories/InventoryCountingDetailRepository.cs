using DataAccess;
using DataAccess.Models;

namespace Infrastructure.Repositories
{
    public class InventoryCountingDetailRepository : GenericRepository<InventoryCountingDetail>
    {
        public InventoryCountingDetailRepository(DataBaseAccess dataBaseAccess) : base(dataBaseAccess)
        {

        }
    }
}
