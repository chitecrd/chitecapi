using DataAccess.Models;
using Infrastructure.Repositories;
using System.Data;
using System.Data.SqlClient;

namespace Infrastructure.UnitOfWork
{
    public class InventoryCountingUnitOfWork : GenericTransactionalUnitOfWork
    {
        public InventoryCountingUnitOfWork(string connectionString) : base(new SqlConnection(connectionString))
        { }

        public InventoryCountingUnitOfWork(string connectionString, IDbConnection conection)
          : base(conection)
        { }

        private IGenericRepository<InventoryCountingHeader> inventoryCountingHeaderRepository;
        public IGenericRepository<InventoryCountingHeader> InventoryCountingHeaderRepository =>
            inventoryCountingHeaderRepository ?? (inventoryCountingHeaderRepository = new InventoryCountingHeaderRepository(dataBaseAccess));

        private IGenericRepository<InventoryCountingDetail> inventoryCountingDetailRepository;
        public IGenericRepository<InventoryCountingDetail> InventoryCountingDetailRepository =>
            inventoryCountingDetailRepository ?? (inventoryCountingDetailRepository = new InventoryCountingDetailRepository(dataBaseAccess));

        
        protected override void ResetRepositories()
        {
            inventoryCountingHeaderRepository = null;
            inventoryCountingDetailRepository = null;
        }
    }
}
