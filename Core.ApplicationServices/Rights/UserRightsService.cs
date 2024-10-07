using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Organizations;
using System.Collections.Generic;
using System;
using System.Linq;
using Core.Abstractions.Types;
using Core.ApplicationServices.Model.RightsHolder;
using Core.ApplicationServices.Model.Users;
using Core.DomainModel;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Organization;
using Core.DomainServices.Authorization;
using Core.DomainServices.Extensions;
using Core.DomainServices.Generic;
using Core.DomainServices.Queries;
using Core.DomainServices.Role;
using Infrastructure.Services.DataAccess;
using Serilog;

namespace Core.ApplicationServices.Rights
{
    public class UserRightsService : IUserRightsService
    {
        private readonly IUserService _userService;
        private readonly IOrganizationService _organizationService;
        private readonly IAuthorizationContext _authorizationContext;
        private readonly IEntityIdentityResolver _identityResolver;
        private readonly IOrganizationRightsService _organizationRightsService;
        private readonly IRoleAssignmentService<ItContractRight, ItContractRole, ItContract> _itContractRightService;
        private readonly IRoleAssignmentService<ItSystemRight, ItSystemRole, ItSystemUsage> _itSystemRightService;
        private readonly IRoleAssignmentService<OrganizationUnitRight, OrganizationUnitRole, OrganizationUnit> _organizationUnitRightService;
        private readonly IRoleAssignmentService<DataProcessingRegistrationRight, DataProcessingRegistrationRole, DataProcessingRegistration> _dprRoleAssignmentsService;
        private readonly ITransactionManager _transactionManager;
        private readonly IDatabaseControl _databaseControl;
        private readonly ILogger _logger;

        public UserRightsService(
            IUserService userService,
            IOrganizationService organizationService,
            IAuthorizationContext authorizationContext,
            IEntityIdentityResolver identityResolver,
            IOrganizationRightsService organizationRightsService,
            IRoleAssignmentService<ItContractRight, ItContractRole, ItContract> itContractRightService,
            IRoleAssignmentService<ItSystemRight, ItSystemRole, ItSystemUsage> itSystemRightService,
            IRoleAssignmentService<OrganizationUnitRight, OrganizationUnitRole, OrganizationUnit> organizationUnitRightService,
            IRoleAssignmentService<DataProcessingRegistrationRight, DataProcessingRegistrationRole, DataProcessingRegistration> dprRoleAssignmentsService,
            ITransactionManager transactionManager,
            IDatabaseControl databaseControl,
            ILogger logger)
        {
            _userService = userService;
            _organizationService = organizationService;
            _authorizationContext = authorizationContext;
            _identityResolver = identityResolver;
            _organizationRightsService = organizationRightsService;
            _itContractRightService = itContractRightService;
            _itSystemRightService = itSystemRightService;
            _organizationUnitRightService = organizationUnitRightService;
            _dprRoleAssignmentsService = dprRoleAssignmentsService;
            _transactionManager = transactionManager;
            _databaseControl = databaseControl;
            _logger = logger;
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

        public Result<UserRightsAssignments, OperationError> GetUserRights(int userId, int organizationId)
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
                .Select(user => ExtractAllRightsInOrganization(user, orgUuid.Value))
                .Select(rights => new UserRightsAssignments
                    (
                        rights.rolesInOrganization.Where(x => x != OrganizationRole.User && x != OrganizationRole.GlobalAdmin).ToList(),
                        rights.dprRights.ToList(),
                        rights.systemRights.ToList(),
                        rights.contractRights.ToList(),
                        rights.organizationUnitRights.ToList()
                    )
                );
        }

        public Maybe<OperationError> RemoveAllRights(int userId, int organizationId)
        {
            return MutateUserRights(
                userId,
                organizationId,
                context =>
                    RemoveRights
                    (
                        context.user,
                        context.organization,
                        context.dprRights,
                        context.contractRights,
                        context.systemRights,
                        context.organizationUnitRights,
                        context.rolesInOrganization
                    )
            );
        }

        public Maybe<OperationError> RemoveRights(int userId, int organizationId, UserRightsChangeParameters parameters)
        {
            return MutateUserRights(
                userId,
                organizationId,
                context =>
                    RemoveRights
                    (
                        context.user,
                        context.organization,
                        context.dprRights.Where(right => parameters.DataProcessingRegistrationRightIds.Contains(right.Id)).ToList(),
                        context.contractRights.Where(right => parameters.ContractRightIds.Contains(right.Id)).ToList(),
                        context.systemRights.Where(right => parameters.SystemRightIds.Contains(right.Id)).ToList(),
                        context.organizationUnitRights.Where(right => parameters.OrganizationUnitRightsIds.Contains(right.Id)).ToList(),
                        context.rolesInOrganization.Where(role => parameters.AdministrativeAccessRoles.Contains(role)).ToList()
                    )
            );
        }

        public Maybe<OperationError> TransferRights(int fromUserId, int toUserId, int organizationId, UserRightsChangeParameters parameters)
        {
            if (fromUserId == toUserId)
            {
                return Maybe<OperationError>.None;
            }
            return MutateUserRights(
                fromUserId,
                organizationId,
                context =>
                    TransferRights
                    (
                        context.user,
                        context.organization,
                        toUserId,
                        context.dprRights.Where(right => parameters.DataProcessingRegistrationRightIds.Contains(right.Id)).ToList(),
                        context.contractRights.Where(right => parameters.ContractRightIds.Contains(right.Id)).ToList(),
                        context.systemRights.Where(right => parameters.SystemRightIds.Contains(right.Id)).ToList(),
                        context.organizationUnitRights.Where(right => parameters.OrganizationUnitRightsIds.Contains(right.Id)).ToList(),
                        context.rolesInOrganization.Where(role => parameters.AdministrativeAccessRoles.Contains(role)).ToList()
                    )
            );
        }

        public Maybe<OperationError> CopyRights(int fromUserId, int toUserId, int organizationId, UserRightsChangeParameters parameters)
        {
            if (fromUserId == toUserId)
            {
                return new OperationError("Tried to copy roles from the user to itself", OperationFailure.Conflict);
            }
            return MutateUserRights(
                fromUserId,
                organizationId,
                context =>
                    CopyRights
                    (
                        context.organization,
                        toUserId,
                        context.dprRights.Where(right => parameters.DataProcessingRegistrationRightIds.Contains(right.Id)).ToList(),
                        context.contractRights.Where(right => parameters.ContractRightIds.Contains(right.Id)).ToList(),
                        context.systemRights.Where(right => parameters.SystemRightIds.Contains(right.Id)).ToList(),
                        context.organizationUnitRights.Where(right => parameters.OrganizationUnitRightsIds.Contains(right.Id)).ToList(),
                        context.rolesInOrganization.Where(role => parameters.AdministrativeAccessRoles.Contains(role)).ToList()
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

        private Maybe<OperationError> MutateUserRights(
          int userId,
          int organizationId,
          Func<(Organization organization, User user, IEnumerable<DataProcessingRegistrationRight> dprRights, IEnumerable<ItContractRight> contractRights, IEnumerable<ItSystemRight> systemRights, IEnumerable<OrganizationUnitRight> organizationUnitRights, IEnumerable<OrganizationRole> rolesInOrganization), Maybe<OperationError>> mutation)
        {
            using var transaction = _transactionManager.Begin();
            var uuidResult = _identityResolver.ResolveUuid<Organization>(organizationId);

            if (uuidResult.IsNone)
            {
                return new OperationError(nameof(organizationId), OperationFailure.BadInput);
            }

            var error = _organizationService
                .GetOrganization(uuidResult.Value, OrganizationDataReadAccessLevel.All)
                .Bind(WithWriteAccess)
                .Bind<(Organization organization, User user)>(organization =>
                {
                    var userResult = _userService.GetUsersInOrganization(organization.Uuid,new QueryById<User>(userId));
                    if (userResult.Failed)
                    {
                        return userResult.Error;
                    }

                    var user = userResult.Value.FirstOrDefault();
                    if (user == null)
                    {
                        return new OperationError($"User with id: {userId} not found in the organization", OperationFailure.NotFound);
                    }

                    return (organization, user);
                })
                .Select(orgAndUser =>
                {
                    var (organization, user) = orgAndUser;
                    var (dprRights, contractRights, systemRights, organizationUnitRights, rolesInOrganization) = ExtractAllRightsInOrganization(user, organization.Uuid);
                    return
                    (
                        organization,
                        user,
                        dprRights,
                        contractRights,
                        systemRights,
                        organizationUnitRights,
                        rolesInOrganization
                    );
                })
                .Match
                (
                    context => mutation(context),
                    error => error
                );

            if (error.IsNone)
            {
                _databaseControl.SaveChanges();
                transaction.Commit();
            }
            else
            {
                transaction.Rollback();
            }

            return error;
        }

        private static (List<DataProcessingRegistrationRight> dprRights, List<ItContractRight> contractRights, List<ItSystemRight> systemRights, List<OrganizationUnitRight> organizationUnitRights, List<OrganizationRole> rolesInOrganization) ExtractAllRightsInOrganization(User user, Guid organizationUuid)
        {
            var dprRights = user.GetDataProcessingRegistrationRights(organizationUuid).ToList();
            var contractRights = user.GetItContractRights(organizationUuid).ToList();
            var systemRights = user.GetItSystemRights(organizationUuid).ToList();
            var organizationUnitRights = user.GetOrganizationUnitRights(organizationUuid).ToList();
            var rolesInOrganization = user.GetRolesInOrganization(organizationUuid).ToList();
            return (dprRights, contractRights, systemRights, organizationUnitRights, rolesInOrganization);
        }

        private Maybe<OperationError> RemoveRights(
            User user,
            Organization organization,
            IEnumerable<DataProcessingRegistrationRight> dprRights,
            IEnumerable<ItContractRight> contractRights,
            IEnumerable<ItSystemRight> systemRights,
            IEnumerable<OrganizationUnitRight> organizationUnitRights,
            IEnumerable<OrganizationRole> rolesInOrganization)
        {
            return RemoveBusinessRights(user, organization, dprRights, _dprRoleAssignmentsService)
                .Match
                (
                    error => error,
                    () => RemoveBusinessRights(user, organization, contractRights, _itContractRightService)
                )
                .Match
                (
                    error => error,
                    () => RemoveBusinessRights(user, organization, systemRights, _itSystemRightService)
                )
                .Match
                (
                    error => error,
                    () => RemoveBusinessRights(user, organization, organizationUnitRights, _organizationUnitRightService)
                )
                .Match
                (
                    error => error,
                    () => RemoveAdministrativeRoles(user, organization, rolesInOrganization)
                );
        }

        private Maybe<OperationError> TransferRights(
            User fromUser,
            Organization organization,
            int toUserId,
            IEnumerable<DataProcessingRegistrationRight> dprRights,
            IEnumerable<ItContractRight> contractRights,
            IEnumerable<ItSystemRight> systemRights,
            IEnumerable<OrganizationUnitRight> organizationUnitRights,
            IEnumerable<OrganizationRole> rolesInOrganization)
        {
            return TransferBusinessRights(fromUser, organization, toUserId, dprRights, _dprRoleAssignmentsService)
                .Match
                (
                    error => error,
                    () => TransferBusinessRights(fromUser, organization, toUserId, contractRights, _itContractRightService)
                )
                .Match
                (
                    error => error,
                    () => TransferBusinessRights(fromUser, organization, toUserId, systemRights, _itSystemRightService)
                )
                .Match
                (
                    error => error,
                    () => TransferBusinessRights(fromUser, organization, toUserId, organizationUnitRights, _organizationUnitRightService)
                )
                .Match
                (
                    error => error,
                    () => TransferAdministrativeRoles(fromUser, organization, toUserId, rolesInOrganization)
                );
        }

        private Maybe<OperationError> CopyRights(
            Organization organization,
            int toUserId,
            IEnumerable<DataProcessingRegistrationRight> dprRights,
            IEnumerable<ItContractRight> contractRights,
            IEnumerable<ItSystemRight> systemRights,
            IEnumerable<OrganizationUnitRight> organizationUnitRights,
            IEnumerable<OrganizationRole> rolesInOrganization)
        {
            return CopyBusinessRights(organization, toUserId, dprRights, _dprRoleAssignmentsService)
                .Match
                (
                    error => error,
                    () => CopyBusinessRights(organization, toUserId, contractRights, _itContractRightService)
                )
                .Match
                (
                    error => error,
                    () => CopyBusinessRights(organization, toUserId, systemRights, _itSystemRightService)
                )
                .Match
                (
                    error => error,
                    () => CopyBusinessRights(organization, toUserId, organizationUnitRights, _organizationUnitRightService)
                )
                .Match
                (
                    error => error,
                    () => CopyAdministrativeRoles(organization, toUserId, rolesInOrganization)
                );
        }

        private Maybe<OperationError> RemoveAdministrativeRoles(User user, Organization organization, IEnumerable<OrganizationRole> rolesInOrganization)
        {
            foreach (var organizationRole in rolesInOrganization.ToList())
            {
                var removeRoleResult = _organizationRightsService.RemoveRole(organization.Id, user.Id, organizationRole);
                if (removeRoleResult.Failed)
                {
                    var operationFailure = removeRoleResult.Error;
                    _logger.Error(
                        "Failed to remove role {role} from user {userId} in organization {organizationId}. Failed with {errorCode}",
                        organizationRole, user.Id, organization.Id, operationFailure);
                    {
                        return new OperationError("Failed removing organization role:" + organizationRole.ToString("G"), operationFailure);
                    }
                }
            }

            return Maybe<OperationError>.None;
        }

        private Maybe<OperationError> TransferAdministrativeRoles(User user, Organization organization, int toUserId, IEnumerable<OrganizationRole> rolesInOrganization)
        {
            var organizationRoles = rolesInOrganization.ToList();

            //Start by removing the old assignments
            var removeFailure = RemoveAdministrativeRoles(user, organization, organizationRoles);
            if (removeFailure.HasValue)
            {
                return removeFailure;
            }

            // Re-assign the roles to the specified user
            foreach (var role in organizationRoles)
            {
                var assignResult = _organizationRightsService.AssignRole(organization.Id, toUserId, role);
                if (assignResult.Failed)
                {
                    _logger.Error("Failed to assign role of type {roleType} user {userId} in organization {organizationId}. Failed with {error}", role, user.Id, organization.Id, assignResult.Error.ToString());
                    {
                        return new OperationError($"Failed to assign role of type {role:G}:{assignResult.Error}", assignResult.Error);
                    }
                }
            }

            return Maybe<OperationError>.None;
        }

        private Maybe<OperationError> CopyAdministrativeRoles(Organization organization, int toUserId, IEnumerable<OrganizationRole> rolesInOrganization)
        {
            var organizationRoles = rolesInOrganization.ToList();
            
            // Re-assign the roles to the specified user
            foreach (var role in organizationRoles)
            {
                var assignResult = _organizationRightsService.AssignRole(organization.Id, toUserId, role);
                if (assignResult.Failed)
                {
                    _logger.Error("Failed to assign role of type {roleType} user {userId} in organization {organizationId}. Failed with {error}", role, toUserId, organization.Id, assignResult.Error.ToString());
                    {
                        return new OperationError($"Failed to assign role of type {role:G}:{assignResult.Error}", assignResult.Error);
                    }
                }
            }

            return Maybe<OperationError>.None;
        }

        private Maybe<OperationError> RemoveBusinessRights<TRight, TRole, TModel>(
            User user,
            Organization organization,
            IEnumerable<TRight> rights,
            IRoleAssignmentService<TRight, TRole, TModel> assignmentService)
            where TRight : Entity, IRight<TModel, TRight, TRole>
            where TRole : OptionEntity<TRight>, IRoleEntity, IOptionReference<TRight>
            where TModel : HasRightsEntity<TModel, TRight, TRole>, IOwnedByOrganization
        {
            foreach (var right in rights.ToList())
            {
                var removeRoleResult = assignmentService.RemoveRole(right.Object, right.RoleId, user.Id);
                if (removeRoleResult.Failed)
                {
                    _logger.Error(
                        "Failed to remove right {rightType}:{rightId} located on object: {objectType}:{objectId} from user {userId} in organization {organizationId}. Failed with {error}",
                        right.GetType().Name, right.Id, right.Object.GetType().Name, right.ObjectId, user.Id, organization.Id, removeRoleResult.Error.ToString()
                        );
                    {
                        return new OperationError($"Failed to remove right of type {right.GetType().Name} with rightId {right.Id}:{removeRoleResult.Error}", removeRoleResult.Error.FailureType);
                    }
                }
            }

            return Maybe<OperationError>.None;
        }

        private Maybe<OperationError> TransferBusinessRights<TRight, TRole, TModel>(
            User user,
            Organization organization,
            int toUserId,
            IEnumerable<TRight> rights,
            IRoleAssignmentService<TRight, TRole, TModel> assignmentService)
            where TRight : Entity, IRight<TModel, TRight, TRole>
            where TRole : OptionEntity<TRight>, IRoleEntity, IOptionReference<TRight>
            where TModel : HasRightsEntity<TModel, TRight, TRole>, IOwnedByOrganization
        {
            var businessRights = rights.ToList();

            //Take a snapshot of the rights info before we remove all relations
            var rightsInfoSnapshot = businessRights.Select(r => (r.Object, r.RoleId)).ToList();

            //Start by removing the original assignments
            var removeFailure = RemoveBusinessRights(user, organization, businessRights, assignmentService);

            if (removeFailure.HasValue)
                return removeFailure.Value;

            // Re-assign the roles to the specified user
            foreach (var right in rightsInfoSnapshot)
            {
                var assignResult = assignmentService.AssignRole(right.Object, right.RoleId, toUserId);
                if (assignResult.Failed)
                {
                    _logger.Error(
                        "Failed to assign right of type {rightType} with role {roleId} on object: {objectType}:{objectId} to user {userId} in organization {organizationId}. Failed with {error}",
                        typeof(TRight).Name, right.RoleId, right.Object.GetType().Name, right.Object.Id, toUserId, organization.Id, assignResult.Error.ToString()
                    );
                    {
                        return new OperationError($"Failed to assign role of type {typeof(TRight).Name} with role {right.RoleId}:{assignResult.Error}", assignResult.Error.FailureType);
                    }
                }
            }

            return Maybe<OperationError>.None;
        }

        private Maybe<OperationError> CopyBusinessRights<TRight, TRole, TModel>(
            Organization organization,
            int toUserId,
            IEnumerable<TRight> rights,
            IRoleAssignmentService<TRight, TRole, TModel> assignmentService)
            where TRight : Entity, IRight<TModel, TRight, TRole>
            where TRole : OptionEntity<TRight>, IRoleEntity, IOptionReference<TRight>
            where TModel : HasRightsEntity<TModel, TRight, TRole>, IOwnedByOrganization
        {
            var businessRights = rights.ToList();

            //Take a snapshot of the rights info before we remove all relations
            var rightsInfoSnapshot = businessRights.Select(r => (r.Object, r.RoleId)).ToList();

            // Re-assign the roles to the specified user
            foreach (var right in rightsInfoSnapshot)
            {
                var assignResult = assignmentService.AssignRole(right.Object, right.RoleId, toUserId);
                if (assignResult.Failed)
                {
                    _logger.Error(
                        "Failed to assign right of type {rightType} with role {roleId} on object: {objectType}:{objectId} to user {userId} in organization {organizationId}. Failed with {error}",
                        typeof(TRight).Name, right.RoleId, right.Object.GetType().Name, right.Object.Id, toUserId, organization.Id, assignResult.Error.ToString()
                    );
                    {
                        return assignResult.Error;
                    }
                }
            }

            return Maybe<OperationError>.None;
        }

        private Result<Organization, OperationError> WithWriteAccess(Organization organization)
        {
            if (_authorizationContext.AllowModify(organization))
            {
                return organization;
            }

            return new OperationError(OperationFailure.Forbidden);
        }
    }
}
