using DataAccess;
using DataAccess.Models;

namespace Infrastructure.Repositories
{
    public class UsuarioRepository : GenericRepository<Usuario>
    {
        public UsuarioRepository(DataBaseAccess dataBaseAccess) : base(dataBaseAccess)
        { }

    }
}
