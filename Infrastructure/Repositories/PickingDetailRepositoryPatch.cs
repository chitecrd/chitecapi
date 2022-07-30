using DataAccess;
using DataAccess.Models;

namespace Infrastructure.Repositories
{
    public class PickingDetailRepositoryPatch : GenericRepository<PickingDetailPath>
    {
        public PickingDetailRepositoryPatch(DataBaseAccess dataBaseAccess) : base(dataBaseAccess)
        {

        }
    }
}
