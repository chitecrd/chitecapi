using DataAccess.Models;
using Infrastructure.Repositories;
using System.Data;
using System.Data.SqlClient;

namespace Infrastructure.UnitOfWork
{
    public class BusinessPartnerUnitOfWork : GenericTransactionalUnitOfWork
    {
        public BusinessPartnerUnitOfWork(string connectionString) : base(new SqlConnection(connectionString))
        { }


        public BusinessPartnerUnitOfWork(string connectionString, IDbConnection conection)
          : base(conection)
        { }


        private IGenericRepository<BusinessPartner> businessPartnerRepository;
        public IGenericRepository<BusinessPartner> BusinessPartnerRepository =>
            businessPartnerRepository ?? (businessPartnerRepository = new BusinessPartnerRepository(dataBaseAccess));

        protected override void ResetRepositories()
        {
            businessPartnerRepository = null;
        }

    }
}
