using DataAccess;
using DataAccess.Models;

namespace Infrastructure.Repositories
{
    public class PickingDetailRepository : GenericRepository<PickingDetail>
    {
        public PickingDetailRepository(DataBaseAccess dataBaseAccess) : base(dataBaseAccess)
        {

        }
    }
}
