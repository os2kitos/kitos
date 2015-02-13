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


        /* returns a list of <Organization, OrgUnit> of the organizations that a user is member of, together with the 
         * default organization unit of that user in that organization (possibly null) */
        public ICollection<KeyValuePair<Organization, OrganizationUnit>> GetByUser(User user)
        {
            //select from admin rights
            var result = _admRightRepository.Get(x => x.UserId == user.Id)
                .Select(x => new KeyValuePair<Organization, OrganizationUnit>(x.Object, x.DefaultOrgUnit)).ToList();

            //if the user is global admin, the list should include all organization, even if the user doesn't have an 
            //explicit role on the organization
            if (user.IsGlobalAdmin)
            {
                var remainingOrgs =
                    _orgRepository.Get()
                    .Where(org => result.All(kvp => kvp.Key != org))
                    .Select(org => new KeyValuePair<Organization, OrganizationUnit>(org, null));

                result.AddRange(remainingOrgs);

            }

            return result;
        }

        public void SetDefaultOrgUnit(User user, int orgId, int orgUnitId)
        {
            //TODO this should probably be Single() ?
            var right = _admRightRepository.Get(r => r.UserId == user.Id && r.ObjectId == orgId).First();
            right.DefaultOrgUnitId = orgUnitId;

            _admRightRepository.Update(right);
            _admRightRepository.Save();
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