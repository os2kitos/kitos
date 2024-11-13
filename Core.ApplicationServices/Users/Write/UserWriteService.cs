using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Authorization.Permissions;
using Core.ApplicationServices.Extensions;
using Core.ApplicationServices.Model.Users;
using Core.ApplicationServices.Model.Users.Write;
using Core.ApplicationServices.Organizations;
using Core.ApplicationServices.Rights;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Core.DomainServices.Generic;
using Infrastructure.Services.DataAccess;

namespace Core.ApplicationServices.Users.Write
{
    public class UserWriteService : IUserWriteService
    {
        private readonly IUserService _userService;
        private readonly IOrganizationRightsService _organizationRightsService;
        private readonly ITransactionManager _transactionManager;
        private readonly IAuthorizationContext _authorizationContext;
        private readonly IOrganizationService _organizationService;
        private readonly IEntityIdentityResolver _entityIdentityResolver;
        private readonly IUserRightsService _userRightsService;
        private readonly IOrganizationalUserContext _organizationalUserContext;

        public UserWriteService(IUserService userService,
            IOrganizationRightsService organizationRightsService,
            ITransactionManager transactionManager,
            IAuthorizationContext authorizationContext,
            IOrganizationService organizationService,
            IEntityIdentityResolver entityIdentityResolver,
            IUserRightsService userRightsService,
            IOrganizationalUserContext organizationalUserContext)
        {
            _userService = userService;
            _organizationRightsService = organizationRightsService;
            _transactionManager = transactionManager;
            _authorizationContext = authorizationContext;
            _organizationService = organizationService;
            _entityIdentityResolver = entityIdentityResolver;
            _userRightsService = userRightsService;
            _organizationalUserContext = organizationalUserContext;
        }

        public Result<User, OperationError> Create(Guid organizationUuid, CreateUserParameters parameters)
        {
            return ValidateUserCanBeCreated(parameters)
                .Match(
                error => error,
                () =>
                    ValidateIfCanCreateUser(organizationUuid)
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
                        }
                    )
                );
        }

        public Result<User, OperationError> Update(Guid organizationUuid, Guid userUuid, UpdateUserParameters parameters)
        {
            using var transactionManager = _transactionManager.Begin();

            var orgResult = _organizationService.GetOrganization(organizationUuid);
            if (orgResult.Failed)
            {
                return orgResult.Error;
            }
            var organization = orgResult.Value;

            var updateUserResult =
                _userService.GetUserByUuid(userUuid)
                    .Bind(CanModifyUser)
                    .Bind(user => PerformUpdates(user, organization, parameters));

            if (updateUserResult.Failed)
            {
                transactionManager.Rollback();
                return updateUserResult.Error;
            }

            var user = updateUserResult.Value;
            _userService.UpdateUser(user, parameters.SendMailOnUpdate, organization.Id);
            transactionManager.Commit();
            return user;
        }

        public Maybe<OperationError> SendNotification(Guid organizationUuid, Guid userUuid)
        {
            var orgIdResult = ResolveOrganizationUuidToId(organizationUuid);
            if (orgIdResult.Failed)
            {
                return orgIdResult.Error;
            }

            var user = _userService.GetUserInOrganization(organizationUuid, userUuid);
            if (user.Failed)
            {
                return user.Error;
            }
            _userService.IssueAdvisMail(user.Value, false, orgIdResult.Value);
            return Maybe<OperationError>.None;
        }

        public Result<UserCollectionPermissionsResult, OperationError> GetCollectionPermissions(
            Guid organizationUuid)
        {
            return _organizationService.GetOrganization(organizationUuid)
                .Select(org => UserCollectionPermissionsResult.FromOrganization(org, _authorizationContext));
        }

        public Maybe<OperationError> CopyUserRights(Guid organizationUuid, Guid fromUserUuid, Guid toUserUuid,
            UserRightsChangeParameters parameters)
        {
            return CollectUsersAndMutateRoles(_userRightsService.CopyRights, organizationUuid, fromUserUuid, toUserUuid, parameters);
        }

        public Maybe<OperationError> TransferUserRights(Guid organizationUuid, Guid fromUserUuid, Guid toUserUuid,
            UserRightsChangeParameters parameters)
        {
            return CollectUsersAndMutateRoles(_userRightsService.TransferRights, organizationUuid, fromUserUuid, toUserUuid, parameters);
        }

        public Maybe<OperationError> DeleteUser(Guid userUuid, Maybe<Guid> scopedToOrganizationUuid)
        {
            return scopedToOrganizationUuid
                .Match(
                orgUuid => ResolveOrganizationUuidToId(orgUuid)
                    .Match(
                        id => Result<int?, OperationError>.Success(id),
                        ex => ex
                        ),
                () => Result<int?, OperationError>.Success(null)
                )
                .Match(
                    orgDbId => _userService.DeleteUser(userUuid, orgDbId),
                    error => error
                    );
        }

        public Result<User, OperationError> AddGlobalAdmin(Guid userUuid)
        {
            return SetUsersGlobalAdminStatus(userUuid, true);
        }

        public Maybe<OperationError> RemoveGlobalAdmin(Guid userUuid)
        {
            return SetUsersGlobalAdminStatus(userUuid, false)
                .Match(_ => Maybe<OperationError>.None, Maybe<OperationError>.Some);
        }

        private Result<User, OperationError> SetUsersGlobalAdminStatus(Guid userUuid, bool status)
        {
            using var transaction = _transactionManager.Begin();
            return _userService.GetUserByUuid(userUuid)
                .Bind(user => UpdateGlobalAdminStatus(user, status))
                .Match<Result<User, OperationError>>(
                    user =>
                    {
                        transaction.Commit();
                        _userService.UpdateUser(user, null, null);
                        return user;
                    }, 
                    error =>
                    {
                        transaction.Rollback();
                        return error;
                    });
        }

        private Result<User, OperationError> UpdateGlobalAdminStatus(User user, bool requestedGlobalAdminStatus)
        {
            if (!_organizationalUserContext.IsGlobalAdmin())
            {
                return new OperationError("Only global admins can add or remove global admins", OperationFailure.Forbidden);
            }
            if (!requestedGlobalAdminStatus && _organizationalUserContext.UserId == user.Id)
            {
                return new OperationError("You can not remove yourself as global admin", OperationFailure.Forbidden);
            }
            user.IsGlobalAdmin = requestedGlobalAdminStatus;
            return user;
        }

        private Maybe<OperationError> CollectUsersAndMutateRoles(Func<int, int, int, UserRightsChangeParameters, Maybe<OperationError>> mutateAction,
            Guid organizationUuid, Guid fromUserUuid,
            Guid toUserUuid,
            UserRightsChangeParameters parameters)
        {
            return ResolveOrganizationUuidToId(organizationUuid)
                .Match(orgDbId =>
                    {
                        var fromUserResult = _userService.GetUserInOrganization(organizationUuid, fromUserUuid);
                        if (fromUserResult.Failed)
                        {
                            return fromUserResult.Error;
                        }

                        var fromUser = fromUserResult.Value;

                        return _userService.GetUserInOrganization(organizationUuid, toUserUuid)
                            .Bind(CanModifyUser)
                            .Match(toUser => mutateAction(fromUser.Id, toUser.Id, orgDbId, parameters), error => error);
                    },
                    error => error
                );
        }

        private Result<User, OperationError> PerformUpdates(User orgUser, Organization organization, UpdateUserParameters parameters)
        {
            return orgUser.WithOptionalUpdate(parameters.FirstName, (user, firstName) => user.UpdateFirstName(user, firstName))
                .Bind(user =>
                    user.WithOptionalUpdate(parameters.LastName, (userToUpdate, lastName) => userToUpdate.UpdateLastName(user, lastName)))
                .Bind(user => user.WithOptionalUpdate(parameters.Email, (userToUpdate, email) => UpdateEmail(userToUpdate, email)))
                .Bind(user => user.WithOptionalUpdate(parameters.PhoneNumber, (userToUpdate, phoneNumber) => userToUpdate.PhoneNumber = phoneNumber))
                .Bind(user => user.WithOptionalUpdate(parameters.HasStakeHolderAccess, UpdateStakeholderAccess))
                .Bind(user => user.WithOptionalUpdate(parameters.HasApiAccess,
                    (userToUpdate, hasApiAccess) => userToUpdate.HasApiAccess = hasApiAccess))
                .Bind(user => user.WithOptionalUpdate(parameters.DefaultUserStartPreference,
                    (userToUpdate, defaultStartPreference) => userToUpdate.DefaultUserStartPreference = defaultStartPreference))
                .Bind(user => user.WithOptionalUpdate(parameters.Roles, (userToUpdate, roles) => UpdateRoles(organization, userToUpdate, roles)));
        }

        private Result<User, OperationError> UpdateStakeholderAccess(User user, bool stakeholderAccess)
        {
            if (stakeholderAccess &&
                !_authorizationContext.HasPermission(
                    new AdministerGlobalPermission(GlobalPermission.StakeHolderAccess)))
            {
                return new OperationError("You don't have permission to issue stakeholder access.", OperationFailure.Forbidden);
            }

            user.HasStakeHolderAccess = stakeholderAccess;
            return user;
        }

        private Result<User, OperationError> UpdateEmail(User user, string email)
        {
            if (_userService.IsEmailInUse(email))
            {
                return new OperationError($"Email '{email}' is already in use.", OperationFailure.Conflict);
            }

            return user.UpdateEmail(user, email);
        }

        private Result<User, OperationError> UpdateRoles(Organization organization, User user,
            IEnumerable<OrganizationRole> roles)
        {
            var oldRoles = user.GetRolesInOrganization(organization.Uuid).ToHashSet();
            var newRoles = roles.ToHashSet();
            var rolesToAdd = newRoles.Except(oldRoles);
            var rolesToDelete = oldRoles.Except(newRoles);
            return RemoveRoles(user, organization, rolesToDelete)
                .Match(error => error, () => AssignUserAdministrativeRoles(organization.Id, user.Id, rolesToAdd))
                .Match(error => error, () => Result<User, OperationError>.Success(user));

        }

        private Maybe<OperationError> RemoveRoles(User user, Organization organization,
            IEnumerable<OrganizationRole> roles)
        {
            foreach (var role in roles)
            {
                var result = _organizationRightsService.RemoveRole(organization.Id, user.Id, role);
                if (result.Failed)
                {
                    return new OperationError($"Failed to remove role {role}", result.Error);

                }
            }
            return Maybe<OperationError>.None;
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
                    if (permissionsResult.Value.Create == false)
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

        private Result<int, OperationError> ResolveOrganizationUuidToId(Guid organizationUuid)
        {
            var orgIdResult
                = _entityIdentityResolver.ResolveDbId<Organization>(organizationUuid);
            if (orgIdResult.IsNone)
            {
                return new OperationError($"Organization with uuid {organizationUuid} was not found",
                    OperationFailure.NotFound);
            }

            return orgIdResult.Value;
        }

        private Maybe<OperationError> ValidateUserCanBeCreated(CreateUserParameters parameters)
        {
            var user = parameters.User;
            if (user.Email != null && _userService.IsEmailInUse(user.Email))
            {
                return new OperationError("Email is already in use.", OperationFailure.BadInput);
            }

            // user is being created as global admin
            if (user.IsGlobalAdmin)
            {
                // only other global admins can create global admin users
                if (!_authorizationContext.HasPermission(new AdministerGlobalPermission(GlobalPermission.GlobalAdmin)))
                {
                    return new OperationError("You don't have permission to create a global admin user.", OperationFailure.Forbidden);
                }
            }

            if (user.HasStakeHolderAccess)
            {
                // only global admins can create stakeholder access
                if (!_authorizationContext.HasPermission(new AdministerGlobalPermission(GlobalPermission.StakeHolderAccess)))
                {
                    return new OperationError("You don't have permission to issue stakeholder access.", OperationFailure.Forbidden);
                }
            }

            return Maybe<OperationError>.None;

        }

        private Result<User, OperationError> CanModifyUser(User user)
        {
            if (!_authorizationContext.AllowModify(user))
            {
                return new OperationError($"Not allowed to modify user with uuid {user.Uuid}",
                    OperationFailure.Forbidden);
            }

            return user;
        }

    }
}
