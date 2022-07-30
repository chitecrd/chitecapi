using DataAccess.Models;
using Infrastructure.Repositories;
using System.Data;
using System.Data.SqlClient;

namespace Infrastructure.UnitOfWork
{
    public class UsuarioUnitOfWork : GenericTransactionalUnitOfWork
    {
        public UsuarioUnitOfWork(string connectionString) : base(new SqlConnection(connectionString))
        { }

        public UsuarioUnitOfWork(string connectionString, IDbConnection conection)
          : base(conection)
        { }


        private IGenericRepository<Usuario> usuariosRepository;
        public IGenericRepository<Usuario> UsuariosRepository =>
            usuariosRepository ?? (usuariosRepository = new UsuarioRepository(dataBaseAccess));

        protected override void ResetRepositories()
        {
            usuariosRepository = null;
        }
    }
}
