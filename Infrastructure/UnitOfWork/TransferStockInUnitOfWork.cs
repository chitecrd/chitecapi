using DataAccess.Models;
using Infrastructure.Repositories;
using System.Data;
using System.Data.SqlClient;

namespace Infrastructure.UnitOfWork
{
    public class TransferStockInUnitOfWork : GenericTransactionalUnitOfWork
    {
        public TransferStockInUnitOfWork(string connectionString) : base(new SqlConnection(connectionString))
        { }

        public TransferStockInUnitOfWork(string connectionString, IDbConnection conection)
          : base(conection)
        { }

        private IGenericRepository<StockTranfersHeaderIn> transferStockHeaderInRepository;
        public IGenericRepository<StockTranfersHeaderIn> TransferStockHeaderInRepository =>
            transferStockHeaderInRepository ?? (transferStockHeaderInRepository = new TransferStockHeaderInRepository(dataBaseAccess));

        private IGenericRepository<StockTranfersDetailIn> transferStockDetailInRepository;
        public IGenericRepository<StockTranfersDetailIn> TransferStockDetailInRepository =>
            transferStockDetailInRepository ?? (transferStockDetailInRepository = new TransferStockDetailInRepository(dataBaseAccess));

        
        protected override void ResetRepositories()
        {
            transferStockHeaderInRepository = null;
            transferStockDetailInRepository = null;
        }
    }
}
