using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Organizations;
using Core.ApplicationServices.Model.UiCustomization;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Core.DomainServices.Authorization;
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

        public Result<Organization, OperationError> GetOrganization(Guid organizationUuid, OrganizationDataReadAccessLevel? withMinimumAccessLevel = null);
        public Result<IQueryable<Organization>, OperationError> GetAllOrganizations();
        public IQueryable<Organization> SearchAccessibleOrganizations(bool onlyWithMembershipAccess, params IDomainQuery<Organization>[] conditions);
        public IQueryable<Organization> SearchAccessibleOrganizations(params IDomainQuery<Organization>[] conditions);

        public Result<IQueryable<OrganizationUnit>, OperationError> GetOrganizationUnits(Guid organizationUuid, params IDomainQuery<OrganizationUnit>[] criteria);
        public Result<OrganizationUnit, OperationError> GetOrganizationUnit(Guid organizationUnitUuid);
        /// <summary>
        /// Computes the blocking conflicts related to removing an organization
        /// </summary>
        /// <param name="organizationUuid"></param>
        /// <returns></returns>
        Result<OrganizationRemovalConflicts, OperationError> ComputeOrganizationRemovalConflicts(Guid organizationUuid);
        /// <summary>
        /// Removes an organization
        /// </summary>
        /// <param name="organizationUuid"></param>
        /// <param name="enforceDeletion">If set to <c>true</c> any removal conflicts will be ignored and the deletion will proceed</param>
        /// <returns></returns>
        Maybe<OperationError> RemoveOrganization(Guid organizationUuid, bool enforceDeletion);


        Result<IEnumerable<Organization>, OperationError> GetUserOrganizations(int userId);
        Result<bool, OperationError> CanActiveUserModifyCvr(Guid organizationUuid);
        Result<OrganizationPermissionsResult, OperationError> GetPermissions(Guid organizationUuid);

        public GridPermissions GetGridPermissions(int orgId);
        public Maybe<DataResponsible> GetDataResponsible(int organizationId);
        public Maybe<ContactPerson> GetContactPerson(int organizationId);
        public Maybe<DataProtectionAdvisor> GetDataProtectionAdvisor(int organizationId);

        Result<Config, OperationError> GetUIRootConfig(Guid organizationUuid);
    }
}
