using System.Collections.Generic;
using Core.DomainModel;
using Core.DomainModel.Organization;

namespace Core.DomainServices
{
    public interface IOrganizationService
    {
        //returns a list of organizations that the user is a member of
        IEnumerable<Organization> GetOrganizations(User user);

        //returns the default org unit for that user inside that organization
        //or null if none has been chosen
        OrganizationUnit GetDefaultUnit(Organization organization, User user);

        void SetDefaultOrgUnit(User user, int orgId, int orgUnitId);

        void SetupDefaultOrganization(Organization org, User objectOwner);

        void RemoveUser(int organizationId, int userId);
        void addContactPerson(int organizationId, int contactId);
    }
}
