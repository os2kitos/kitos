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

            if(user.CreatedIn != null) orgs.Add(user.CreatedIn);

            orgs.AddRange(_admRightRepository.Get(x => x.UserId == user.Id).Select(x => x.Object));

            orgs = orgs.Distinct().ToList();

            return orgs;
        }

        public void SetupDefaultOrganization(Organization org, User objectOwner)
        {
            org.Config = Config.Default(objectOwner);
            org.OrgUnits.Add(new OrganizationUnit()
                {
                    Name = org.Name,
                    ObjectOwner = org.ObjectOwner
                });
        }
    }
}