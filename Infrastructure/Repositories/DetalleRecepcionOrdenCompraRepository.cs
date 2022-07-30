using DataAccess;
using DataAccess.Models;

namespace Infrastructure.Repositories
{
    public class DetalleRecepcionOrdenCompraRepository : GenericRepository<DetalleRecepcionOrdenCompra>
    {
        public DetalleRecepcionOrdenCompraRepository(DataBaseAccess dataBaseAccess) : base(dataBaseAccess)
        { }
    }
}
