using System.Collections.Generic;
using Core.DomainModel;

namespace Core.DomainServices
{
    public interface IOrganizationService
    {
        /* returns a list of <Organization, OrgUnit> of the organizations that a user is member of, together with the 
         * default organization unit of that user in that organization (possibly null) */
        ICollection<KeyValuePair<Organization, OrganizationUnit>> GetByUser(User user);
        void SetDefaultOrgUnit(User user, int orgId, int orgUnitId);

        void SetupDefaultOrganization(Organization org, User objectOwner);
    }
}