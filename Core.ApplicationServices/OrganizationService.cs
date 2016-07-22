using System.Collections.Generic;
using System.Linq;
using Core.DomainModel;
using Core.DomainServices;

namespace Core.ApplicationServices
{
    public class OrganizationService : IOrganizationService
    {
        private readonly IGenericRepository<Organization> _orgRepository;
        private readonly IGenericRepository<OrganizationRight> _orgRightRepository;

        public OrganizationService(IGenericRepository<Organization> orgRepository, IGenericRepository<OrganizationRight> orgRightRepository)
        {
            _orgRepository = orgRepository;
            _orgRightRepository = orgRightRepository;
        }

        //returns a list of organizations that the user is a member of
        public IEnumerable<Organization> GetOrganizations(User user)
        {
            if (user.IsGlobalAdmin) return _orgRepository.Get();
            return _orgRepository.Get(o => o.Rights.Count(r => r.OrganizationId == o.Id && r.UserId == user.Id) > 0);
        }

        //returns the default org unit for that user inside that organization
        //or null if none has been chosen
        public OrganizationUnit GetDefaultUnit(Organization organization, User user)
        {
            return _orgRightRepository.Get(r => r.OrganizationId == organization.Id && r.UserId == user.Id).Select(r => r.DefaultOrgUnit).FirstOrDefault();
        }

        public void SetDefaultOrgUnit(User user, int orgId, int orgUnitId)
        {
            //TODO this should probably be Single() ?
            var right = _orgRightRepository.Get(r => r.UserId == user.Id && r.OrganizationId == orgId).First();
            right.DefaultOrgUnitId = orgUnitId;

            _orgRightRepository.Update(right);
            _orgRightRepository.Save();
        }

        public void SetupDefaultOrganization(Organization org, User objectOwner)
        {
            org.Config = Config.Default(objectOwner);
            org.OrgUnits.Add(new OrganizationUnit()
                {
                    Name = org.Name,
                    ObjectOwner = org.ObjectOwner,
                    LastChangedByUser = objectOwner
                });
        }
    }
}
