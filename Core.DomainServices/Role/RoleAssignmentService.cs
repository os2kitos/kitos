using System;
using System.Collections.Generic;
using System.Linq;
using Core.DomainModel;
using Core.DomainModel.Result;
using Core.DomainServices.Extensions;
using Core.DomainServices.Options;
using Infrastructure.Services.Types;

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

        public RoleAssignmentService(
            IOptionsService<TRight, TRole> localRoleOptionsService,
            IUserRepository userRepository,
            IGenericRepository<TRight> rightsRepository)
        {
            _localRoleOptionsService = localRoleOptionsService;
            _userRepository = userRepository;
            _rightsRepository = rightsRepository;
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

            return model.AssignRole(role.Value, user.Value);
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

            return AssignRole(model, roleId.Value, userId.Value);
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

            var removeResult = model.RemoveRole(role.Value.option, user.Value);
            if (removeResult.Failed)
                return removeResult.Error;

            _rightsRepository.Delete(removeResult.Value);
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

        private Result<int, OperationError> GetUserByUuid(Guid userUuid)
        {
            var user = _userRepository.GetByUuid(userUuid);
            if (user.IsNone)
            {
                return new OperationError($"Could not find user with Uuid: {userUuid}", OperationFailure.BadInput);
            }

            return user.Value.Id;
        }

        private Result<int, OperationError> GetRoleByUuid(int organizationId, Guid roleUuid)
        {
            var roleResult = _localRoleOptionsService.GetOptionByUuid(organizationId, roleUuid);
            if (roleResult.IsNone)
            {
                return new OperationError($"Could not find role with Uuid: {roleUuid}", OperationFailure.BadInput);
            }

            return roleResult.Value.option.Id;
        }
    }
}