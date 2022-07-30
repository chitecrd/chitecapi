using DataAccess.Models;
using Infrastructure.Repositories;
using System.Data;
using System.Data.SqlClient;

namespace Infrastructure.UnitOfWork
{
    public class TransferStockUnitOfWork : GenericTransactionalUnitOfWork
    {
        public TransferStockUnitOfWork(string connectionString) : base(new SqlConnection(connectionString))
        { }

        public TransferStockUnitOfWork(string connectionString, IDbConnection conection)
          : base(conection)
        { }

        private IGenericRepository<StockTranfersHeader> transferStockHeaderRepository;
        public IGenericRepository<StockTranfersHeader> TransferStockHeaderRepository =>
            transferStockHeaderRepository ?? (transferStockHeaderRepository = new TransferStockHeaderRepository(dataBaseAccess));

        private IGenericRepository<StockTranfersDetail> transferStockDetailRepository;
        public IGenericRepository<StockTranfersDetail> TransferStockDetailRepository =>
            transferStockDetailRepository ?? (transferStockDetailRepository = new TransferStockDetailRepository(dataBaseAccess));

        
        protected override void ResetRepositories()
        {
            transferStockHeaderRepository = null;
            transferStockDetailRepository = null;
        }
    }
}
