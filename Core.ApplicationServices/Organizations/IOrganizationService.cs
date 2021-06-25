using System.Collections.Generic;
using System.Linq;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Core.DomainModel.Result;

namespace Core.ApplicationServices.Organizations
{
    public interface IOrganizationService
    {
        //returns a list of organizations that the user is a member of
        IEnumerable<Organization> GetOrganizations(User user);

        //returns the default org unit for that user inside that organization
        //or null if none has been chosen
        OrganizationUnit GetDefaultUnit(Organization organization, User user);

        void SetDefaultOrgUnit(User user, int orgId, int orgUnitId);

        Result<Organization, OperationFailure> RemoveUser(int organizationId, int userId);

        bool CanChangeOrganizationType(Organization organization, OrganizationTypeKeys organizationType);

        Result<Organization, OperationFailure> CreateNewOrganization(Organization newOrg);

        public Result<IQueryable<Organization>, OperationError> GetOrganizations(IEnumerable<int> orgIds);
    }
}
