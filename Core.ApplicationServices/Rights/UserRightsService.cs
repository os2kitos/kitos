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
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Organization;
using Core.DomainServices.Authorization;
using Core.DomainServices.Extensions;
using Core.DomainServices.Generic;
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
        private readonly IOrganizationalUserContext _organizationalUserContext;
        private readonly IEntityIdentityResolver _identityResolver;
        private readonly IOrganizationRightsService _organizationRightsService;
        private readonly IRoleAssignmentService<ItContractRight, ItContractRole, ItContract> _itContractRightService;
        private readonly IRoleAssignmentService<ItSystemRight, ItSystemRole, ItSystemUsage> _itSystemRightService;
        private readonly IRoleAssignmentService<ItProjectRight, ItProjectRole, ItProject> _itProjectRightService;
        private readonly IRoleAssignmentService<OrganizationUnitRight, OrganizationUnitRole, OrganizationUnit> _organizationUnitRightService;
        private readonly IRoleAssignmentService<DataProcessingRegistrationRight, DataProcessingRegistrationRole, DataProcessingRegistration> _dprRoleAssignmentsService;
        private readonly ITransactionManager _transactionManager;
        private readonly IDatabaseControl _databaseControl;
        private readonly ILogger _logger;

        public UserRightsService(
            IUserService userService,
            IOrganizationService organizationService,
            IAuthorizationContext authorizationContext,
            IOrganizationalUserContext organizationalUserContext,
            IEntityIdentityResolver identityResolver,
            IOrganizationRightsService organizationRightsService,
            IRoleAssignmentService<ItContractRight, ItContractRole, ItContract> itContractRightService,
            IRoleAssignmentService<ItSystemRight, ItSystemRole, ItSystemUsage> itSystemRightService,
            IRoleAssignmentService<ItProjectRight, ItProjectRole, ItProject> itProjectRightService,
            IRoleAssignmentService<OrganizationUnitRight, OrganizationUnitRole, OrganizationUnit> organizationUnitRightService,
            IRoleAssignmentService<DataProcessingRegistrationRight, DataProcessingRegistrationRole, DataProcessingRegistration> dprRoleAssignmentsService,
            ITransactionManager transactionManager,
            IDatabaseControl databaseControl,
            ILogger logger)
        {
            _userService = userService;
            _organizationService = organizationService;
            _authorizationContext = authorizationContext;
            _organizationalUserContext = organizationalUserContext;
            _identityResolver = identityResolver;
            _organizationRightsService = organizationRightsService;
            _itContractRightService = itContractRightService;
            _itSystemRightService = itSystemRightService;
            _itProjectRightService = itProjectRightService;
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
                .Select(user => new UserRightsAssignments
                    (
                        user.GetRolesInOrganization(orgUuid.Value).Where(x => x != OrganizationRole.User),
                        user.GetDataProcessingRegistrationRights(organizationId).ToList(),
                        user.GetItSystemRights(organizationId).ToList(),
                        user.GetItContractRights(organizationId).ToList(),
                        user.GetItProjectRights(organizationId).ToList(),
                        user.GetOrganizationUnitRights(organizationId).ToList()
                    )
                );
        }

        public Maybe<OperationError> RemoveAllRights(int userId, int organizationId)
        {
            var transaction = _transactionManager.Begin();
            var uuidResult = _identityResolver.ResolveUuid<Organization>(organizationId);

            if (uuidResult.IsNone)
            {
                return new OperationError(nameof(organizationId), OperationFailure.BadInput);
            }

            var removeAllRightsError = _organizationService
                .GetOrganization(uuidResult.Value, OrganizationDataReadAccessLevel.All)
                .Bind(WithWriteAccess)
                .Match(organization => RemoveAllRights(userId, organization), error => error);

            if (removeAllRightsError.IsNone)
            {
                _databaseControl.SaveChanges();
                transaction.Commit();
            }

            return removeAllRightsError;
        }

        public Maybe<OperationError> RemoveRights(int userId, int organizationId, UserRightsChangeParameters parameters)
        {
            throw new NotImplementedException();
        }

        public Maybe<OperationError> TransferRights(int fromUserId, int toUserId, int organizationId, UserRightsChangeParameters parameters)
        {
            throw new NotImplementedException();
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

        private Maybe<OperationError> RemoveAllRights(int userId, Organization organization)
        {
            return _userService
                .GetUsersInOrganization(organization.Uuid).Select(users => users.ById(userId))
                .Match(user => RemoveAllRights(user, organization), error => error);
        }

        private Maybe<OperationError> RemoveAllRights(User user, Organization organization)
        {
            var dprRights = user.GetDataProcessingRegistrationRights(organization.Id).ToList();
            var contractRights = user.GetItContractRights(organization.Id).ToList();
            var projectRights = user.GetItProjectRights(organization.Id).ToList();
            var systemRights = user.GetItSystemRights(organization.Id).ToList();
            var organizationUnitRights = user.GetOrganizationUnitRights(organization.Id).ToList();
            var rolesInOrganization = user.GetRolesInOrganization(organization.Uuid);

            return RemoveBusinessRights(user, organization, dprRights, _dprRoleAssignmentsService)
                .Match
                (
                    error => error,
                    () => RemoveBusinessRights(user, organization, contractRights, _itContractRightService)
                )
                .Match
                (
                    error => error,
                    () => RemoveBusinessRights(user, organization, projectRights, _itProjectRightService)
                )
                .Match
                (
                    error => error,
                    () => RemoveBusinessRights(user, organization, systemRights, _itSystemRightService)
                )
                .Match
                (
                    error => error,
                    () => RemoveBusinessRights(user, organization, organizationUnitRights,
                        _organizationUnitRightService)
                )
                .Match
                (
                    error => error,
                    () => RemoveAdministrativeRoles(user, organization, rolesInOrganization)
                );
        }

        private Maybe<OperationError> RemoveAdministrativeRoles(User user, Organization organization, IEnumerable<OrganizationRole> rolesInOrganization)
        {
            foreach (var organizationRole in rolesInOrganization)
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

        private Maybe<OperationError> RemoveBusinessRights<TRight, TRole, TModel>(
            User user,
            Organization organization,
            IEnumerable<TRight> rights,
            IRoleAssignmentService<TRight, TRole, TModel> assignmentService)
            where TRight : Entity, IRight<TModel, TRight, TRole>
            where TRole : OptionEntity<TRight>, IRoleEntity, IOptionReference<TRight>
            where TModel : HasRightsEntity<TModel, TRight, TRole>, IOwnedByOrganization
        {
            foreach (var right in rights)
            {
                var removeRoleResult = assignmentService.RemoveRole(right.Object, right.RoleId, right.UserId);
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
