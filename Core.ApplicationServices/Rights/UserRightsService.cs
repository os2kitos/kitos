using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Organizations;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;
using Core.ApplicationServices.Helpers;
using Core.ApplicationServices.Model.RightsHolder;
using Core.ApplicationServices.Model.Users;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Core.DomainServices.Authorization;
using Core.DomainServices.Extensions;
using Core.DomainServices.Generic;

namespace Core.ApplicationServices.Rights
{
    public class UserRightsService : IUserRightsService
    {
        private readonly IUserService _userService;
        private readonly IOrganizationService _organizationService;
        private readonly IAuthorizationContext _authorizationContext;
        private readonly IOrganizationalUserContext _organizationalUserContext;
        private readonly IEntityIdentityResolver _identityResolver;

        public UserRightsService(
            IUserService userService,
            IOrganizationService organizationService,
            IAuthorizationContext authorizationContext,
            IOrganizationalUserContext organizationalUserContext,
            IEntityIdentityResolver identityResolver)
        {
            _userService = userService;
            _organizationService = organizationService;
            _authorizationContext = authorizationContext;
            _organizationalUserContext = organizationalUserContext;
            _identityResolver = identityResolver;
        }

        public Result<IEnumerable<UserRoleAssociationDTO>, OperationError> GetUsersWithRoleAssignment(OrganizationRole role)
        {
            if (_authorizationContext.GetCrossOrganizationReadAccess() < CrossOrganizationDataReadAccessLevel.All)
            {
                return new OperationError(OperationFailure.Forbidden);
            }

            return _userService
                .GetUsersWithRoleAssignedInAnyOrganization(role)
                .Bind(users => MapOrganizationalRightsHolderRelation(users, role));
        }

        public Result<UserRoleAssignments, OperationError> GetUserRoles(int userId, int organizationId)
        {
            if (_authorizationContext.GetOrganizationReadAccessLevel(organizationId) < OrganizationDataReadAccessLevel.All)
            {
                return new OperationError(OperationFailure.Forbidden);
            }
            var orgUuid = _identityResolver.ResolveUuid<Organization>(organizationId);
            if (orgUuid.IsNone)
            {
                return new OperationError("Organization id is invalid", OperationFailure.BadInput);
            }
            var userUuid = _identityResolver.ResolveUuid<User>(userId);
            if (userUuid.IsNone)
            {
                return new OperationError("User id is invalid", OperationFailure.BadInput);
            }

            return _userService
                .GetUserInOrganization(orgUuid.Value, userUuid.Value)
                .Select(user => new UserRoleAssignments
                    (
                        user.GetRolesInOrganization(orgUuid.Value).Where(x => x != OrganizationRole.User),
                        user.DataProcessingRegistrationRights.Where(right => right.Object.OrganizationId == organizationId).ToList(),
                        user.ItSystemRights.Where(right => right.Object.OrganizationId == organizationId).ToList(),
                        user.ItContractRights.Where(right => right.Object.OrganizationId == organizationId).ToList(),
                        user.ItProjectRights.Where(right => right.Object.OrganizationId == organizationId).ToList()
                    )
                );
        }

        private Result<IEnumerable<UserRoleAssociationDTO>, OperationError> MapOrganizationalRightsHolderRelation(IQueryable<User> users, OrganizationRole role)
        {
            var result = new List<UserRoleAssociationDTO>();

            foreach (var user in users.ToList())
            {
                var organizationIds = user.GetOrganizationIdsWhereRoleIsAssigned(role).ToList();

                var rightsHolderRelations = _organizationService.GetAllOrganizations()
                    .Select(organizations => organizations.ByIds(organizationIds))
                    .Select(organizations =>
                        organizations.ToList().Select(org => new UserRoleAssociationDTO(role, user, org)).ToList());

                if (rightsHolderRelations.Failed)
                {
                    return rightsHolderRelations.Error;
                }

                result.AddRange(rightsHolderRelations.Value);
            }

            return result;
        }
    }
}
