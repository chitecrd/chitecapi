using DataAccess.Models;
using Infrastructure.Repositories;
using System.Data.SqlClient;

namespace Infrastructure.UnitOfWork
{
    public class DetalleInventarioUnitOfWork : GenericTransactionalUnitOfWork
    {
        public DetalleInventarioUnitOfWork(string connectionString) : base(new SqlConnection(connectionString))
        { }

        private IGenericRepository<DetalleInventario> detalleInventarioRepository;
        public IGenericRepository<DetalleInventario> DetalleInventarioRepository =>
            detalleInventarioRepository ?? (detalleInventarioRepository = new DetalleInventarioRepository(dataBaseAccess));

        private IGenericRepository<DetalleInventarioInfo> detalleInventarioInfoRepository;
        public IGenericRepository<DetalleInventarioInfo> DetalleInventarioInfoRepository =>
            detalleInventarioInfoRepository ?? (detalleInventarioInfoRepository = new DetalleInventarioInfoRepository(dataBaseAccess));

        protected override void ResetRepositories()
        {
            detalleInventarioRepository = null;
            detalleInventarioInfoRepository = null;
        }
    }
}
