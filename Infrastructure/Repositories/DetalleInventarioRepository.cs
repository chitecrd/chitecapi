using DataAccess;
using DataAccess.Models;

namespace Infrastructure.Repositories
{
    public class DetalleInventarioRepository : GenericRepository<DetalleInventario>
    {
        public DetalleInventarioRepository(DataBaseAccess dataBaseAccess) : base(dataBaseAccess)
        { }
    }
}
