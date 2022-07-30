using DataAccess;
using DataAccess.Models;

namespace Infrastructure.Repositories
{
    public class AyudanteRepository : GenericRepository<Ayudante>
    {
        public AyudanteRepository(DataBaseAccess dataBaseAccess) : base(dataBaseAccess)
        { }
    }
}
