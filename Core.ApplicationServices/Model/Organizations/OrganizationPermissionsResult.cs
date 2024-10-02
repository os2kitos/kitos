using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.DomainModel.Organization;

namespace Core.ApplicationServices.Model.Organizations
{
    public class OrganizationPermissionsResult: ResourcePermissionsResult
    {
        public bool ModifyCvr { get; }

        public OrganizationPermissionsResult(ResourcePermissionsResult permissions, bool modifyCvr) : base(permissions.Read, permissions.Modify, permissions.Delete)
        {
            ModifyCvr = modifyCvr;
        }

        public static Result<OrganizationPermissionsResult, OperationError> FromResolutionResult(
            Result<Organization, OperationError> getOrganizationResult,
            IAuthorizationContext authorizationContext, bool modifyCvr)
        {
            var basePermissionsResult = getOrganizationResult
                .Select(organization => FromEntity(organization, authorizationContext))
                .Match
                (
                    result => result,
                    error => error.FailureType == OperationFailure.Forbidden ? Empty : error
                );

            if (basePermissionsResult.Failed) return basePermissionsResult.Error;
            return new OrganizationPermissionsResult(basePermissionsResult.Value, modifyCvr);
        }
    }
}
