using System.Collections.Generic;
using System.Linq;
using Core.DomainModel;
using Core.DomainServices;

namespace Core.ApplicationServices
{
    public class OrgService : IOrgService
    {
        private readonly IGenericRepository<OrganizationRight> _orgRightRepository;

        public OrgService(IGenericRepository<OrganizationRight> orgRightRepository)
        {
            _orgRightRepository = orgRightRepository;
        }

        public IEnumerable<Organization> GetByUserId(int userId)
        {
            var orgs = _orgRightRepository.Get(x => x.UserId == userId).Select(x => x.Object.Organization).Distinct();
            return orgs;
        }
    }
}