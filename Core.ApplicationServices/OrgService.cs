using System.Collections.Generic;
using System.Linq;
using Core.DomainModel;
using Core.DomainServices;

namespace Core.ApplicationServices
{
    public class OrgService : IOrgService
    {
        private readonly IGenericRepository<OrganizationRight> _orgRightRepository;
        private readonly IGenericRepository<Organization> _orgRepository;

        public OrgService(IGenericRepository<OrganizationRight> orgRightRepository, IGenericRepository<Organization> orgRepository)
        {
            _orgRightRepository = orgRightRepository;
            _orgRepository = orgRepository;
        }

        public ICollection<Organization> GetByUser(User user)
        {

            if (user.IsGlobalAdmin) return _orgRepository.Get().ToList();

            var orgs = _orgRightRepository
                .Get(x => x.UserId == user.Id)
                .Select(x => x.Object.Organization)
                .Distinct()
                .ToList();

            return orgs;
        }
    }
}