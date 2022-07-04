using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.DomainModel;
using Core.DomainModel.Events;
using Core.DomainServices.Extensions;
using Core.DomainServices.Options;
using Infrastructure.Services.DataAccess;


namespace Core.DomainServices.Role
{
    public class RoleAssignmentService<TRight, TRole, TModel> : IRoleAssignmentService<TRight, TRole, TModel>
        where TRight : Entity, IRight<TModel, TRight, TRole>
        where TRole : OptionEntity<TRight>, IRoleEntity, IOptionReference<TRight>
        where TModel : HasRightsEntity<TModel, TRight, TRole>, IOwnedByOrganization
    {
        private readonly IOptionsService<TRight, TRole> _localRoleOptionsService;
        private readonly IUserRepository _userRepository;
        private readonly IGenericRepository<TRight> _rightsRepository;
        private readonly ITransactionManager _transactionManager;
        private readonly IDomainEvents _domainEvents;

        public RoleAssignmentService(
            IOptionsService<TRight, TRole> localRoleOptionsService,
            IUserRepository userRepository,
            IGenericRepository<TRight> rightsRepository,
            ITransactionManager transactionManager,
            IDomainEvents domainEvents)
        {
            _localRoleOptionsService = localRoleOptionsService;
            _userRepository = userRepository;
            _rightsRepository = rightsRepository;
            _transactionManager = transactionManager;
            _domainEvents = domainEvents;
        }

        public IEnumerable<TRole> GetApplicableRoles(TModel model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            return _localRoleOptionsService.GetAvailableOptions(model.OrganizationId);
        }
        public Result<IQueryable<User>, OperationError> GetUsersWhichCanBeAssignedToRole(TModel model, int roleId, Maybe<string> nameEmailQuery)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            var targetRole = _localRoleOptionsService.GetAvailableOption(model.OrganizationId, roleId);

            if (targetRole.IsNone)
                return new OperationError("Invalid role id", OperationFailure.BadInput);

            var candidates = _userRepository.SearchOrganizationUsers(model.OrganizationId, nameEmailQuery);
            var usersWithSameRole = GetIdsOfUsersAssignedToRole(model, targetRole.Value);
            return Result<IQueryable<User>, OperationError>.Success(candidates.ExceptEntitiesWithIds(usersWithSameRole.ToList()));
        }

        private static IEnumerable<int> GetIdsOfUsersAssignedToRole(TModel model, TRole role)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            if (role == null) throw new ArgumentNullException(nameof(role));

            return model.GetRights(role.Id).Select(x => x.UserId).Distinct();
        }

        public Result<TRight, OperationError> AssignRole(TModel model, int roleId, int userId)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            var availableUsers = GetUsersWhichCanBeAssignedToRole(model, roleId, Maybe<string>.None);
            if (availableUsers.Failed)
                return availableUsers.Error;

            var user = availableUsers.Value.FirstOrDefault(x => x.Id == userId).FromNullable();
            if (user.IsNone)
            {
                var failure = OperationFailure.BadInput;

                if (model.GetRights(roleId).Any(x => x.UserId == userId))
                    failure = OperationFailure.Conflict;

                return new OperationError($"User Id {userId} is invalid in the context of assign role {roleId} to {typeof(TModel)} with id {model.Id} in organization with id '{model.OrganizationId}'", failure);
            }

            var role = _localRoleOptionsService.GetAvailableOption(model.OrganizationId, roleId);

            if (role.IsNone)
                return new OperationError("Invalid role id", OperationFailure.BadInput);

            var assignRoleResult = model.AssignRole(role.Value, user.Value);
            if (assignRoleResult.Ok)
            {
                _domainEvents.Raise(new EntityUpdatedEvent<TModel>(model));
            }
            return assignRoleResult;
        }

        public Result<TRight, OperationError> AssignRole(TModel model, Guid roleUuid, Guid userUuid)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            var userId = GetUserByUuid(userUuid);
            if (userId.Failed)
                return userId.Error;

            var roleId = GetRoleByUuid(model.OrganizationId, roleUuid);
            if (roleId.Failed)
                return roleId.Error;

            return AssignRole(model, roleId.Value.Id, userId.Value.Id);
        }

        public Result<TRight, OperationError> RemoveRole(TModel model, int roleId, int userId)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            var user = _userRepository.GetById(userId).FromNullable();
            if (user.IsNone)
                return new OperationError($"User Id {userId} is invalid'", OperationFailure.BadInput);

            var role = _localRoleOptionsService.GetOption(model.OrganizationId, roleId);

            if (role.IsNone)
                return new OperationError($"Role Id {roleId} is invalid'", OperationFailure.BadInput);

            return RemoveRole(model, role.Value.option, user.Value);
        }

        private Result<TRight, OperationError> RemoveRole(TModel model, TRole role, User user)
        {
            var removeResult = model.RemoveRole(role, user);
            if (removeResult.Failed)
                return removeResult.Error;

            _rightsRepository.Delete(removeResult.Value);
            _domainEvents.Raise(new EntityUpdatedEvent<TModel>(model));
            _rightsRepository.Save();
            return removeResult.Value;
        }

        public Result<TRight, OperationError> RemoveRole(TModel model, Guid roleUuid, Guid userUuid)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            var userId = GetUserByUuid(userUuid);
            if (userId.Failed)
                return userId.Error;

            var roleId = GetRoleByUuid(model.OrganizationId, roleUuid);
            if (roleId.Failed)
                return roleId.Error;

            return RemoveRole(model, roleId.Value, userId.Value);
        }

        public Maybe<OperationError> BatchUpdateRoles(TModel model, IEnumerable<(Guid roleUuid, Guid userUuid)> roleAssignments)
        {
            using var transaction = _transactionManager.Begin();

            if (model == null) throw new ArgumentNullException(nameof(model));
            if (roleAssignments == null) throw new ArgumentNullException(nameof(roleAssignments));

            var existingRights = model.Rights.ToDictionary(x => (x.Role.Uuid, x.User.Uuid));
            List<(Guid roleUuid, Guid userUuid)> existingKeys = model.Rights.Select(x => (x.Role.Uuid, x.User.Uuid)).ToList();
            var nextStateKeys = roleAssignments.ToList();

            var toRemove = existingKeys.Except(nextStateKeys).ToList();
            var toAdd = nextStateKeys.Except(existingKeys).ToList();

            foreach (var ruleUserPair in toRemove)
            {
                var existingRight = existingRights[ruleUserPair];
                var removeResult = RemoveRole(model, existingRight.Role, existingRight.User);

                if (removeResult.Failed)
                    return new OperationError($"Failed to remove role with Uuid: {ruleUserPair.roleUuid} from user with Uuid: {ruleUserPair.userUuid}, with following error message: {removeResult.Error.Message.GetValueOrEmptyString()}", removeResult.Error.FailureType);
            }

            foreach (var userRolePair in toAdd)
            {
                var assignmentResult = AssignRole(model, userRolePair.roleUuid, userRolePair.userUuid);

                if (assignmentResult.Failed)
                    return new OperationError($"Failed to assign role with Uuid: {userRolePair.roleUuid} from user with Uuid: {userRolePair.userUuid}, with following error message: {assignmentResult.Error.Message.GetValueOrEmptyString()}", assignmentResult.Error.FailureType);
            }

            transaction.Commit();
            return Maybe<OperationError>.None;
        }

        private Result<User, OperationError> GetUserByUuid(Guid userUuid)
        {
            var user = _userRepository.GetByUuid(userUuid);
            if (user.IsNone)
            {
                return new OperationError($"Could not find user with Uuid: {userUuid}", OperationFailure.BadInput);
            }

            return user.Value;
        }

        private Result<TRole, OperationError> GetRoleByUuid(int organizationId, Guid roleUuid)
        {
            var roleResult = _localRoleOptionsService.GetOptionByUuid(organizationId, roleUuid);
            if (roleResult.IsNone)
            {
                return new OperationError($"Could not find role with Uuid: {roleUuid}", OperationFailure.BadInput);
            }

            return roleResult.Value.option;
        }
    }
}