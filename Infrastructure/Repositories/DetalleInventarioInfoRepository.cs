using DataAccess;
using DataAccess.Models;

namespace Infrastructure.Repositories
{
    public class DetalleInventarioInfoRepository : GenericRepository<DetalleInventarioInfo>
    {
        public DetalleInventarioInfoRepository(DataBaseAccess dataBaseAccess) : base(dataBaseAccess)
        { }
    }
}
