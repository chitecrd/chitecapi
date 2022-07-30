using DataAccess.Models;
using Infrastructure.Repositories;
using System.Data;
using System.Data.SqlClient;

namespace Infrastructure.UnitOfWork
{
    public class PickingUnitOfWork : GenericTransactionalUnitOfWork
    {
        public PickingUnitOfWork(string connectionString) : base(new SqlConnection(connectionString))
        { }

        public PickingUnitOfWork(string connectionString, IDbConnection conection)
          : base(conection)
        { }

        private IGenericRepository<PickingHeader> pickingHeaderRepository;
        public IGenericRepository<PickingHeader> PickingHeaderRepository =>
            pickingHeaderRepository ?? (pickingHeaderRepository = new PickingHeaderRepository(dataBaseAccess));

        private IGenericRepository<PickingDetail> pickingDetailRepository;
        public IGenericRepository<PickingDetail> PickingDetailRepository =>
            pickingDetailRepository ?? (pickingDetailRepository = new PickingDetailRepository(dataBaseAccess));


        private IGenericRepository<PickingDetailPath> pickingDetailRepositoryPatch;
        public IGenericRepository<PickingDetailPath> PickingDetailRepositoryPatch =>
            pickingDetailRepositoryPatch ?? (pickingDetailRepositoryPatch = new PickingDetailRepositoryPatch(dataBaseAccess));

        protected override void ResetRepositories()
        {
            pickingHeaderRepository = null;
            pickingDetailRepository = null;
            pickingDetailRepositoryPatch = null;
        }
    }
}
