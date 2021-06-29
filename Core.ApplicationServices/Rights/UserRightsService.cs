using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Organizations;
using Core.DomainModel.Result;
using Core.DomainServices;
using System.Collections.Generic;
using System.Linq;
using Core.ApplicationServices.Model.RightsHolder;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Core.DomainServices.Authorization;
using Core.DomainServices.Extensions;

namespace Core.ApplicationServices.Rights
{
    public class UserRightsService : IUserRightsService
    {
        private readonly IUserService _userService;
        private readonly IOrganizationService _organizationService;
        private readonly IAuthorizationContext _authorizationContext;

        public UserRightsService(IUserService userService, IOrganizationService organizationService, IAuthorizationContext authorizationContext)
        {
            _userService = userService;
            _organizationService = organizationService;
            _authorizationContext = authorizationContext;
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
