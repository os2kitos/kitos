using System.Collections.Generic;
using System.Linq;
using Core.DomainModel;
using Core.DomainServices;

namespace Core.ApplicationServices
{
    public class OrganizationService : IOrganizationService
    {
        private readonly IGenericRepository<OrganizationRight> _orgRightRepository;
        private readonly IGenericRepository<Organization> _orgRepository;
        private readonly IGenericRepository<AdminRight> _admRightRepository;

        public OrganizationService(IGenericRepository<OrganizationRight> orgRightRepository, IGenericRepository<Organization> orgRepository, IGenericRepository<AdminRight> admRightRepository)
        {
            _orgRightRepository = orgRightRepository;
            _orgRepository = orgRepository;
            _admRightRepository = admRightRepository;
        }

        public ICollection<Organization> GetByUser(User user)
        {

            if (user.IsGlobalAdmin) return _orgRepository.Get().ToList();

            var orgs = _orgRightRepository
                .Get(x => x.UserId == user.Id)
                .Select(x => x.Object.Organization).ToList();

            orgs.AddRange(_admRightRepository.Get(x => x.UserId == user.Id).Select(x => x.Object));

            orgs = orgs.Distinct().ToList();

            return orgs;
        }

        public Organization CreateOrganization(string name, OrganizationType organizationType, User objectOwner = null)
        {
            var org = new Organization
            {
                Name = name,
                Config = Config.Default(objectOwner),
                Type = organizationType,
                ObjectOwner = objectOwner
            };

            org.OrgUnits.Add(new OrganizationUnit()
            {
                Name = org.Name,
                ObjectOwner = objectOwner
            });

            return org;
        }

        public bool IsUserMember(User user, Organization organization)
        {
            throw new System.NotImplementedException(); //TODO
        }
    }
}