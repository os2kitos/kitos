using System;
using System.Linq;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Core.DomainModel.Result;
using Core.DomainServices.Queries;

namespace Core.ApplicationServices.Organizations
{
    public interface IOrganizationService
    {
        //returns the default org unit for that user inside that organization
        //or null if none has been chosen
        OrganizationUnit GetDefaultUnit(Organization organization, User user);

        void SetDefaultOrgUnit(User user, int orgId, int orgUnitId);

        Result<Organization, OperationFailure> RemoveUser(int organizationId, int userId);

        bool CanChangeOrganizationType(Organization organization, OrganizationTypeKeys organizationType);

        Result<Organization, OperationFailure> CreateNewOrganization(Organization newOrg);

        public Result<Organization, OperationError> GetOrganization(Guid organizationUuid);
        public Result<IQueryable<Organization>, OperationError> GetAllOrganizations();
        public IQueryable<Organization> SearchAccessibleOrganizations(params IDomainQuery<Organization>[] conditions);
    }
}
