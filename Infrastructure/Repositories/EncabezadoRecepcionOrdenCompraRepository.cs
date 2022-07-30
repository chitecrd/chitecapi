using DataAccess;
using DataAccess.Models;

namespace Infrastructure.Repositories
{
    public class EncabezadoRecepcionOrdenCompraRepository : GenericRepository<EncabezadoRecepcionOrdenCompra>
    {
        public EncabezadoRecepcionOrdenCompraRepository(DataBaseAccess dataBaseAccess) : base(dataBaseAccess)
        { }
    }
}
