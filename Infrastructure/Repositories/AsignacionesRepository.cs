using DataAccess;
using DataAccess.Models;

namespace Infrastructure.Repositories
{
    public class AsignacionesRepository : GenericRepository<Asignacion>
    {
        public AsignacionesRepository(DataBaseAccess dataBaseAccess) : base(dataBaseAccess)
        { }
    }
}
