using DataAccess.Models;
using Infrastructure.Repositories;
using System.Data;
using System.Data.SqlClient;

namespace Infrastructure.UnitOfWork
{
    public class AsignacionesUnitOfWork : GenericTransactionalUnitOfWork
    {
        public AsignacionesUnitOfWork(string connectionString) : base(new SqlConnection(connectionString))
        { }


        public AsignacionesUnitOfWork(string connectionString, IDbConnection conection)
          : base(conection)
        { }

        private IGenericRepository<Asignacion> asignacionesRepository;
        public IGenericRepository<Asignacion> AsignacionesRepository =>
            asignacionesRepository ?? (asignacionesRepository = new AsignacionesRepository(dataBaseAccess));

        protected override void ResetRepositories()
        {
            asignacionesRepository = null;
        }
    }
}
