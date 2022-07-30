using DataAccess.Models;
using Infrastructure.Repositories;
using System.Data;
using System.Data.SqlClient;

namespace Infrastructure.UnitOfWork
{
    public class AyudanteUnitOfWork : GenericTransactionalUnitOfWork
    {
        public AyudanteUnitOfWork(string connectionString) : base(new SqlConnection(connectionString))
        { }



        public AyudanteUnitOfWork(string connectionString, IDbConnection conection)
          : base(conection)
        { }

        private IGenericRepository<Ayudante> ayudanteRepository;
        public IGenericRepository<Ayudante> AyudanteRepository =>
            ayudanteRepository ?? (ayudanteRepository = new AyudanteRepository(dataBaseAccess));

        protected override void ResetRepositories()
        {
            ayudanteRepository = null;
        }
    }
}
