using System;
using System.Collections.Generic;
using System.Linq;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Result;
using Core.DomainServices;
using Core.DomainServices.Extensions;
using Core.DomainServices.Options;
using Infrastructure.Services.Types;

namespace Core.ApplicationServices.SystemUsage.Write
{
    public class ItSystemUsageRoleAssignmentService : IItSystemUsageRoleAssignmentService
    {
        private readonly IOptionsService<ItSystemRight, ItSystemRole> _localRoleOptionsService;
        private readonly IUserRepository _userRepository;

        public ItSystemUsageRoleAssignmentService(
            IOptionsService<ItSystemRight, ItSystemRole> localRoleOptionsService, 
            IUserRepository userRepository)
        {
            _localRoleOptionsService = localRoleOptionsService;
            _userRepository = userRepository;
        }

        private Result<IQueryable<User>, OperationError> GetUsersWhichCanBeAssignedToRole(ItSystemUsage systemUsage, int roleId, Maybe<string> nameEmailQuery)
        {
            if (systemUsage == null) throw new ArgumentNullException(nameof(systemUsage));
            var targetRole = _localRoleOptionsService.GetAvailableOption(systemUsage.OrganizationId, roleId);

            if (targetRole.IsNone)
                return new OperationError("Invalid role id", OperationFailure.BadInput);

            var candidates = _userRepository.SearchOrganizationUsers(systemUsage.OrganizationId, nameEmailQuery);
            var usersWithSameRole = GetIdsOfUsersAssignedToRole(systemUsage, targetRole.Value);
            return Result<IQueryable<User>, OperationError>.Success(candidates.ExceptEntitiesWithIds(usersWithSameRole.ToList()));
        }

        private static IEnumerable<int> GetIdsOfUsersAssignedToRole(ItSystemUsage systemUsage, ItSystemRole role)
        {
            return systemUsage.GetRights(role.Id).Select(x => x.UserId).Distinct();
        }

        public Result<ItSystemRight, OperationError> AssignRole(ItSystemUsage systemUsage, int roleId, int userId)
        {
            if (systemUsage == null) throw new ArgumentNullException(nameof(systemUsage));

            var availableUsers = GetUsersWhichCanBeAssignedToRole(systemUsage, roleId, Maybe<string>.None);
            if (availableUsers.Failed)
                return availableUsers.Error;

            var user = availableUsers.Value.FirstOrDefault(x => x.Id == userId).FromNullable();
            if (user.IsNone)
            {
                var failure = OperationFailure.BadInput;

                if (systemUsage.GetRights(roleId).Any(x => x.UserId == userId))
                    failure = OperationFailure.Conflict;

                return new OperationError($"User Id {userId} is invalid in the context of assign role {roleId} to system usage with id {systemUsage.Id} in organization with id '{systemUsage.OrganizationId}'", failure);
            }

            var role = _localRoleOptionsService.GetAvailableOption(systemUsage.OrganizationId, roleId).Value;

            return systemUsage.AssignRole(role, user.Value);
        }

        public Result<ItSystemRight, OperationError> RemoveRole(ItSystemUsage systemUsage, int roleId, int userId)
        {
            throw new NotImplementedException();
        }

        public Result<ItSystemRight, OperationError> AssignRole(ItSystemUsage systemUsage, Guid roleUuid, Guid userUuid)
        {
            var user = _userRepository.GetByUuid(userUuid);
            if (user.IsNone)
            {
                return new OperationError("Could not find user with Uuid: {roleUuid}", OperationFailure.NotFound);
            }

            var roleResult = _localRoleOptionsService.GetOptionByUuid(systemUsage.OrganizationId, roleUuid);
            if (roleResult.IsNone)
            {
                return new OperationError($"Could not find It System Role with Uuid: {roleUuid}", OperationFailure.NotFound);
            }

            var (option, available) = roleResult.Value;
            if (available)
            {
                return AssignRole(systemUsage, option.Id, user.Value.Id);
            }

            return new OperationError($"Role with Uuid: {roleUuid}, is not available in organization with Uuid: {systemUsage.Organization.Uuid}", OperationFailure.BadInput);
        }

        public Result<ItSystemRight, OperationError> RemoveRole(ItSystemUsage systemUsage, Guid roleUuid, Guid userUuid)
        {
            throw new NotImplementedException();
        }
    }
}