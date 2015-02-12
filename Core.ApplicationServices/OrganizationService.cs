using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls.WebParts;
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
            //if a user is global admin, return a list of all organizations, and no default organization unit.
            //this might not be optimal, because the global admin might also have selected some default org unit, but it'll work for now
            //(fake it till you make it)
            var result = 

            if (user.IsGlobalAdmin)
                return _orgRepository.Get().Select(org => new KeyValuePair<Organization, OrganizationUnit>(org, null)).ToList();

            //otherwise, select from admin rights
            return
                _admRightRepository.Get(x => x.UserId == user.Id)
                    .Select(x => new KeyValuePair<Organization, OrganizationUnit>(x.Object, x.DefaultOrgUnit))
                    .ToList();
        }

        public void SetDefaultOrgUnit(User user, int orgId, int orgUnitId)
        {
            var right = _admRightRepository.Get(r => r.UserId == user.Id && r.ObjectId == orgId).Single();
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