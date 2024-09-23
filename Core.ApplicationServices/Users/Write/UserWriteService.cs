using System;
using System.Collections.Generic;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Model.Users;
using Core.ApplicationServices.Model.Users.Write;
using Core.ApplicationServices.Organizations;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Core.DomainServices.Generic;
using Infrastructure.Services.DataAccess;

namespace Core.ApplicationServices.Users.Write
{
    public class UserWriteService : IUserWriteService
    {
        private readonly IUserService _userService;
        private readonly IEntityIdentityResolver _entityIdentityResolver;
        private readonly IOrganizationRightsService _organizationRightsService;
        private readonly ITransactionManager _transactionManager;
        private readonly IAuthorizationContext _authorizationContext;
        private readonly IOrganizationService _organizationService;

        public UserWriteService(IUserService userService,
            IEntityIdentityResolver entityIdentityResolver,
            IOrganizationRightsService organizationRightsService,
            ITransactionManager transactionManager, 
            IAuthorizationContext authorizationContext,
            IOrganizationService organizationService)
        {
            _userService = userService;
            _entityIdentityResolver = entityIdentityResolver;
            _organizationRightsService = organizationRightsService;
            _transactionManager = transactionManager;
            _authorizationContext = authorizationContext;
            _organizationService = organizationService;
        }

        public Result<User, OperationError> Create(Guid organizationUuid, CreateUserParameters parameters)
        {
            return ValidateIfCanCreateUser(organizationUuid)
                .Bind(organization =>
                {
                    using var transaction = _transactionManager.Begin();

                    var user = _userService.AddUser(parameters.User, parameters.SendMailOnCreation, organization.Id);

                    var roleAssignmentError = AssignUserAdministrativeRoles(organization.Id, user.Id, parameters.Roles);

                    if (roleAssignmentError.HasValue)
                    {
                        transaction.Rollback();
                        return roleAssignmentError.Value;
                    }

                    transaction.Commit();
                    return Result<User, OperationError>.Success(user);
                });
        }

        public Result<UserCollectionPermissionsResult, OperationError> GetCollectionPermissions(
            Guid organizationUuid)
        {
            return _organizationService.GetOrganization(organizationUuid)
                .Select(org => UserCollectionPermissionsResult.FromOrganization(org, _authorizationContext));
        }

        private Result<UserCollectionPermissionsResult, OperationError> GetCollectionPermissions(Organization organization)
        {
            return UserCollectionPermissionsResult.FromOrganization(organization, _authorizationContext);
        }

        private Result<Organization, OperationError> ValidateIfCanCreateUser(Guid organizationUuid)
        {
            return _organizationService.GetOrganization(organizationUuid)
                .Bind(organization =>
                {
                    var permissionsResult = GetCollectionPermissions(organization);
                    if (permissionsResult.Failed)
                        return permissionsResult.Error;
                    if(permissionsResult.Value.Create == false)
                        return new OperationError(
                            $"User is not allowed to create users for organization with uuid: {organizationUuid}",
                            OperationFailure.Forbidden);

                    return Result<Organization, OperationError>.Success(organization);
                });
        }
        
        private Maybe<OperationError> AssignUserAdministrativeRoles(int organizationId, int userId,
            IEnumerable<OrganizationRole> roles)
        {
            foreach (var role in roles)
            {
                var result = _organizationRightsService.AssignRole(organizationId, userId, role);
                if (result.Failed)
                    return new OperationError($"Failed to assign role: {role}", result.Error);
            }

            return Maybe<OperationError>.None;
        }
    }
}
