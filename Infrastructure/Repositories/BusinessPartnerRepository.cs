using DataAccess;
using DataAccess.Models;

namespace Infrastructure.Repositories
{
    public class BusinessPartnerRepository : GenericRepository<BusinessPartner>
    {
        public BusinessPartnerRepository(DataBaseAccess dataBaseAccess) : base(dataBaseAccess)
        { }
    }
}
