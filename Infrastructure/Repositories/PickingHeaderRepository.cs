using DataAccess;
using DataAccess.Models;

namespace Infrastructure.Repositories
{
    public class PickingHeaderRepository : GenericRepository<PickingHeader>
    {
        public PickingHeaderRepository(DataBaseAccess dataBaseAccess) : base(dataBaseAccess)
        {

        }
    }
}
