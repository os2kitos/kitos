using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Authorization.Permissions;
using Core.ApplicationServices.Extensions;
using Core.ApplicationServices.Model.Shared;
using Core.ApplicationServices.Model.Users;
using Core.ApplicationServices.Model.Users.Write;
using Core.ApplicationServices.Organizations;
using Core.ApplicationServices.Rights;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Core.DomainServices;
using Core.DomainServices.Generic;
using Infrastructure.Services.DataAccess;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using Organization = Core.DomainModel.Organization.Organization;

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
        private readonly IUserRepository _userRepository;
        private readonly ILogger _logger;

        public UserWriteService(IUserService userService,
            IOrganizationRightsService organizationRightsService,
            ITransactionManager transactionManager,
            IAuthorizationContext authorizationContext,
            IOrganizationService organizationService,
            IEntityIdentityResolver entityIdentityResolver,
            IUserRightsService userRightsService,
            IOrganizationalUserContext organizationalUserContext,
            IUserRepository userRepository, 
            ILogger logger)
        {
            _userService = userService;
            _organizationRightsService = organizationRightsService;
            _transactionManager = transactionManager;
            _authorizationContext = authorizationContext;
            _organizationService = organizationService;
            _entityIdentityResolver = entityIdentityResolver;
            _userRightsService = userRightsService;
            _organizationalUserContext = organizationalUserContext;
            _userRepository = userRepository;
            _logger = logger;
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

            var getUserResult = _userService.GetUserByUuid(userUuid);
            Result<User, OperationError> updateUserResult;
            if(parameters.HasOnlyRoleChanges())
            {
                updateUserResult = getUserResult
                    .Bind(user => PerformRoleModify(organization, user, parameters));
            }
            else
            {
                updateUserResult = getUserResult.Bind(user => PerformModify(organization, user, parameters));
            }           

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
            var orgIdResult = ResolveUuidToId<Organization>(organizationUuid);
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
        
        public Result<UserCollectionPermissionsResult, OperationError> GetCollectionPermissions(Guid organizationUuid)
        {
            return _organizationService.GetOrganization(organizationUuid)
                .Select(org => UserCollectionPermissionsResult.FromOrganization(org, _authorizationContext, _organizationalUserContext));
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
                orgUuid => ResolveUuidToId<Organization>(orgUuid)
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
            return SetUsersGlobalAdminStatus(userUuid, false).MatchFailure();
        }

        public Result<User, OperationError> AddLocalAdmin(Guid organizationUuid, Guid userUuid)
        {
            return ChangeLocalAdminStatus(organizationUuid, userUuid, (orgId, userId) => _organizationRightsService.AssignRole(orgId, userId, OrganizationRole.LocalAdmin));
        }

        public Maybe<OperationError> RemoveLocalAdmin(Guid organizationUuid, Guid userUuid)
        {
            return ChangeLocalAdminStatus(organizationUuid, userUuid, (orgId, userId) => _organizationRightsService.RemoveRole(orgId, userId, OrganizationRole.LocalAdmin)).MatchFailure();
        }

        public Result<User, OperationError> UpdateSystemIntegrator(Guid userUuid, bool systemIntegratorStatus)
        {
            return UpdateUser(userUuid, user => UpdateSystemIntegrator(user, systemIntegratorStatus));
        }

        public void RequestPasswordReset(string email)
        {
            var userResult = _userRepository.GetByEmail(email).FromNullable();
            if (userResult.IsNone)
            {
                return;
            }
            var user = userResult.Value;
            if (!user.CanAuthenticate())
            {
                return;
            }
            _userService.IssuePasswordReset(user, null, null);
        }

        public Maybe<OperationError> SetDefaultOrgUnit(Guid userUuid, Guid organizationUuid, Guid organizationUnitUuid)
        {
            var orgIdResult = ResolveUuidToId<Organization>(organizationUuid);
            if (orgIdResult.Failed)
            {
                return orgIdResult.Error;
            }
            var unitIdResult = ResolveUuidToId<OrganizationUnit>(organizationUnitUuid);
            if (unitIdResult.Failed)
            {
                return unitIdResult.Error;
            }

            return _userService.GetUserByUuid(userUuid)
                .Match(user =>
                {
                    try
                    {
                        _organizationService.SetDefaultOrgUnit(user, orgIdResult.Value, unitIdResult.Value);
                        return Maybe<OperationError>.None;
                    }
                    catch(Exception ex)
                    {
                        _logger.Error(ex.Message, ex);
                        return new OperationError(ex.Message, OperationFailure.UnknownError);
                    }
                }, error => error);
        }

        private Result<User, OperationError> ChangeLocalAdminStatus<T>(Guid organizationUuid, Guid userUuid, Func<int, int, Result<T, OperationFailure>> changeLocalAdminStatus)
        {
            var transaction = _transactionManager.Begin();
            var user = _userService.GetUserByUuid(userUuid);
            if (user.Failed)
            {
                return user.Error;
            }
            var orgId = ResolveUuidToId<Organization>(organizationUuid);
            if (orgId.Failed)
            {
                return orgId.Error;
            }
            return changeLocalAdminStatus(orgId.Value, user.Value.Id)
                    .Match<Result<User, OperationError>>(
                    _ =>
                    {
                        transaction.Commit();
                        return user.Value;
                    },
                    failure =>
                    {
                        transaction.Rollback();
                        return new OperationError(failure);
                    });
        }

        private Result<User, OperationError> UpdateUser(Guid userUuid, Func<User, Result<User, OperationError>> mutation)
        {
            using var transaction = _transactionManager.Begin();
            var updateResult = _userService.GetUserByUuid(userUuid)
                .Bind(mutation);
            if (updateResult.Failed)
            {
                transaction.Rollback();
                return updateResult.Error;
            }

            var updatedUser = updateResult.Value;
            _userService.UpdateUser(updatedUser, null, null);
            transaction.Commit();
            return updatedUser;
        }

        private Result<User, OperationError> SetUsersGlobalAdminStatus(Guid userUuid, bool status)
        {
            return UpdateUser(userUuid, (user) => UpdateGlobalAdminStatus(user, status));
        }

        private Result<User, OperationError> UpdateSystemIntegrator(User user, bool systemIntegratorStatus)
        {
            var globalAdminWriteAccess = WithGlobalAdminWriteAccess(user);
            if (globalAdminWriteAccess.Failed)
            {
                return globalAdminWriteAccess.Error;
            }
            user.SetSystemIntegratorStatus(systemIntegratorStatus);
            return user;
        }

        private Result<User, OperationError> UpdateGlobalAdminStatus(User user, bool requestedGlobalAdminStatus)
        {
            var globalAdminWriteAccess = WithGlobalAdminWriteAccess(user);
            if (globalAdminWriteAccess.Failed)
            {
                return globalAdminWriteAccess.Error;
            }
            if (!requestedGlobalAdminStatus && _organizationalUserContext.UserId == user.Id)
            {
                return new OperationError("You can not remove yourself as global admin", OperationFailure.Forbidden);
            }
            user.SetGlobalAdminStatus(requestedGlobalAdminStatus);
            return user;
        }

        private Result<User, OperationError> WithGlobalAdminWriteAccess(User user)
        {
            if (!_organizationalUserContext.IsGlobalAdmin())
            {
                return new OperationError("Only global admins can perform this operation", OperationFailure.Forbidden);
            }
            return user;
        }

        private Maybe<OperationError> CollectUsersAndMutateRoles(Func<int, int, int, UserRightsChangeParameters, Maybe<OperationError>> mutateAction,
            Guid organizationUuid, Guid fromUserUuid,
            Guid toUserUuid,
            UserRightsChangeParameters parameters)
        {
            return ResolveUuidToId<Organization>(organizationUuid)
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

        private Result<User, OperationError> PerformModify(Organization organization, User user,
            UpdateUserParameters parameters)
        {
            return CanModifyUser(user)
                .Bind(validatedUser => PerformUpdates(validatedUser, organization, parameters));
        }

        private Result<User, OperationError> PerformRoleModify(Organization organization, User user, UpdateUserParameters parameters)
        {
            return GetCollectionPermissions(organization.Uuid)
                .Bind(permissions =>
                {
                    var editPermissions = permissions.Edit;
                    var (rolesToAdd, rolesToDelete) = CalculateRoleChanges(organization, user, parameters.Roles.HasChange ? parameters.Roles.NewValue : user.GetRolesInOrganization(organization.Uuid));
                    var rolesToAddList = rolesToAdd.ToList();
                    var rolesToDeleteList = rolesToDelete.ToList();
                    if (!editPermissions.EditSystemRole && CheckAnyRoleChangesContainRole(OrganizationRole.SystemModuleAdmin, rolesToAddList, rolesToDeleteList))
                    {
                        return new OperationError(
                            $"User is not allowed to edit {OrganizationRole.SystemModuleAdmin} role",
                            OperationFailure.Forbidden);
                    }
                    if (!editPermissions.EditOrganizationRole && CheckAnyRoleChangesContainRole(OrganizationRole.OrganizationModuleAdmin, rolesToAddList, rolesToDeleteList))
                    {
                        return new OperationError(
                            $"User is not allowed to edit {OrganizationRole.OrganizationModuleAdmin} role",
                            OperationFailure.Forbidden);
                    }
                    if (!editPermissions.EditContractRole && CheckAnyRoleChangesContainRole(OrganizationRole.ContractModuleAdmin, rolesToAddList, rolesToDeleteList))
                    {
                        return new OperationError(
                            $"User is not allowed to edit {OrganizationRole.ContractModuleAdmin} role",
                            OperationFailure.Forbidden);
                    }
                    if (!editPermissions.EditLocalAdminRole &&
                        CheckAnyRoleChangesContainRole(OrganizationRole.LocalAdmin, rolesToAddList, rolesToDeleteList))
                    {

                        return new OperationError(
                            $"User is not allowed to edit {OrganizationRole.LocalAdmin} role",
                            OperationFailure.Forbidden);
                    }
                    if (!_organizationalUserContext.IsGlobalAdmin() &&
                        CheckAnyRoleChangesContainRole(OrganizationRole.GlobalAdmin, rolesToAddList, rolesToDeleteList))
                    {
                        return new OperationError($"User is not allowed to edit {OrganizationRole.GlobalAdmin} role",
                            OperationFailure.Forbidden);
                    }

                    return user.WithOptionalUpdate(parameters.Roles,
                        (userToUpdate, roles) => UpdateRoles(organization, userToUpdate, roles));
                });
        }
        
        private Result<User, OperationError> PerformUpdates(User orgUser, Organization organization, UpdateUserParameters parameters)
        {
            return orgUser.WithOptionalUpdate(parameters.FirstName, (user, firstName) => user.UpdateFirstName(firstName))
                .Bind(user =>
                    user.WithOptionalUpdate(parameters.LastName, (userToUpdate, lastName) => userToUpdate.UpdateLastName(lastName)))
                .Bind(user => user.WithOptionalUpdate(parameters.Email, UpdateEmail))
                .Bind(user => user.WithOptionalUpdate(parameters.PhoneNumber, (userToUpdate, phoneNumber) => userToUpdate.PhoneNumber = phoneNumber))
                .Bind(user => user.WithOptionalUpdate(parameters.HasStakeHolderAccess, UpdateStakeholderAccess))
                .Bind(user => user.WithOptionalUpdate(parameters.HasApiAccess,
                    (userToUpdate, hasApiAccess) => userToUpdate.HasApiAccess = hasApiAccess))
                .Bind(user => user.WithOptionalUpdate(parameters.DefaultUserStartPreference,
                    (userToUpdate, defaultStartPreference) => userToUpdate.DefaultUserStartPreference = defaultStartPreference))
                .Bind(user => user.WithOptionalUpdate(parameters.Roles, (userToUpdate, roles) => UpdateRoles(organization, userToUpdate, roles)))
                .Bind(user => user.WithOptionalUpdate(parameters.DefaultOrganizationUnitUuid, (userToUpdate, organizationUnitUuid) => UpdateOrganizationUnit(userToUpdate, organization.Uuid, organizationUnitUuid)));
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

        private Maybe<OperationError> UpdateEmail(User user, string email)
        {
            return _userService.IsEmailInUse(email)
                ? new OperationError($"Email '{email}' is already in use.", OperationFailure.Conflict)
                : user.UpdateEmail(email);
        }

        private Maybe<OperationError> UpdateOrganizationUnit(User user, Guid orgUuid, Guid unitUuid)
        {
            return ResolveUuidToId<Organization>(orgUuid)
                .Bind(orgId => ResolveUuidToId<OrganizationUnit>(unitUuid)
                    .Select(unitId => (orgId, unitId)))
                .Match(ids =>
                {
                    _organizationService.SetDefaultOrgUnit(user, ids.orgId, ids.unitId);
                    return Maybe<OperationError>.None;
                },
                ex => ex);
        }

        private Result<User, OperationError> UpdateRoles(Organization organization, User user,
            IEnumerable<OrganizationRole> roles)
        {
            var (rolesToAdd, rolesToDelete) = CalculateRoleChanges(organization, user, roles);
            return RemoveRoles(user, organization, rolesToDelete)
                .Match(error => error, () => AssignUserAdministrativeRoles(organization.Id, user.Id, rolesToAdd))
                .Match(error => error, () => Result<User, OperationError>.Success(user));
        }

        private (IEnumerable<OrganizationRole> rolesToAdd, IEnumerable<OrganizationRole> rolesToDelete) CalculateRoleChanges(
            Organization organization, User user,
            IEnumerable<OrganizationRole> roles)
        {
            var oldRoles = user.GetRolesInOrganization(organization.Uuid).ToHashSet();
            var newRoles = roles.ToHashSet();
            var rolesToAdd = newRoles.Except(oldRoles);
            var rolesToDelete = oldRoles.Except(newRoles);
            return (rolesToAdd, rolesToDelete);
        }

        private static bool CheckAnyRoleChangesContainRole(OrganizationRole role, IEnumerable<OrganizationRole> rolesToAdd, IEnumerable<OrganizationRole> rolesToDelete)
        {
            return rolesToAdd.Contains(role) || rolesToDelete.Contains(role);
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

        private Result<Organization, OperationError> ValidateIfCanCreateUser(Guid organizationUuid)
        {
            return _organizationService.GetOrganization(organizationUuid)
                .Bind(organization =>
                {
                    if (_authorizationContext.AllowCreate<User>(organization.Id) == false)
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

        private Result<int, OperationError> ResolveUuidToId<T>(Guid organizationUuid) where T : class, IHasUuid, IHasId
        {
            var idResult
                = _entityIdentityResolver.ResolveDbId<T>(organizationUuid);
            if (idResult.IsNone)
            {
                return new OperationError($"{nameof(T)} with uuid {organizationUuid} was not found",
                    OperationFailure.NotFound);
            }

            return idResult.Value;
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
