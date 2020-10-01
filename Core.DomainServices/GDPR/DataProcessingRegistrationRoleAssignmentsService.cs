using System;
using System.Collections.Generic;
using System.Linq;
using Core.DomainModel;
using Core.DomainModel.GDPR;
using Core.DomainModel.Result;
using Core.DomainServices.Extensions;
using Core.DomainServices.Options;
using Infrastructure.Services.Types;

namespace Core.DomainServices.GDPR
{
    public class DataProcessingRegistrationRoleAssignmentsService : IDataProcessingRegistrationRoleAssignmentsService
    {
        private readonly IOptionsService<DataProcessingRegistrationRight, DataProcessingRegistrationRole> _localRoleOptionsService;
        private readonly IUserRepository _userRepository;

        public DataProcessingRegistrationRoleAssignmentsService(
            IOptionsService<DataProcessingRegistrationRight, DataProcessingRegistrationRole> localRoleOptionsService,
            IUserRepository userRepository)
        {
            _localRoleOptionsService = localRoleOptionsService;
            _userRepository = userRepository;
        }

        private static IEnumerable<int> GetIdsOfUsersAssignedToRole(DataProcessingRegistration registration, DataProcessingRegistrationRole role)
        {
            return registration.GetRights(role.Id).Select(x => x.UserId).Distinct();
        }

        public IEnumerable<DataProcessingRegistrationRole> GetApplicableRoles(DataProcessingRegistration registration)
        {
            if (registration == null) throw new ArgumentNullException(nameof(registration));
            return _localRoleOptionsService.GetAvailableOptions(registration.OrganizationId);
        }

        public Result<IQueryable<User>, OperationError> GetUsersWhichCanBeAssignedToRole(DataProcessingRegistration registration, int roleId, Maybe<string> nameEmailQuery)
        {
            if (registration == null) throw new ArgumentNullException(nameof(registration));
            var targetRole = _localRoleOptionsService.GetAvailableOption(registration.OrganizationId, roleId);

            if (targetRole.IsNone)
                return new OperationError("Invalid role id", OperationFailure.BadInput);

            var candidates = _userRepository.SearchOrganizationUsers(registration.OrganizationId, nameEmailQuery);
            var usersWithSameRole = GetIdsOfUsersAssignedToRole(registration, targetRole.Value);
            return Result<IQueryable<User>, OperationError>.Success(candidates.ExceptEntitiesWithIds(usersWithSameRole.ToList()));
        }

        public Result<DataProcessingRegistrationRight, OperationError> AssignRole(DataProcessingRegistration registration, int roleId, int userId)
        {
            if (registration == null) throw new ArgumentNullException(nameof(registration));

            var availableUsers = GetUsersWhichCanBeAssignedToRole(registration, roleId, Maybe<string>.None);
            if (availableUsers.Failed)
                return availableUsers.Error;

            var user = availableUsers.Value.FirstOrDefault(x => x.Id == userId).FromNullable();
            if (user.IsNone)
            {
                var failure = OperationFailure.BadInput;

                if (registration.GetRights(roleId).Any(x => x.UserId == userId))
                    failure = OperationFailure.Conflict;

                return new OperationError($"User Id {userId} is invalid in the context of assign role {roleId} to dpa with id {registration.Id} in organization with id '{registration.OrganizationId}'", failure);
            }

            var role = _localRoleOptionsService.GetAvailableOption(registration.OrganizationId, roleId).Value;

            return registration.AssignRoleToUser(role, user.Value);
        }

        public Result<DataProcessingRegistrationRight, OperationError> RemoveRole(DataProcessingRegistration registration, int roleId, int userId)
        {
            if (registration == null) throw new ArgumentNullException(nameof(registration));

            var user = _userRepository.GetById(userId).FromNullable();
            if (user.IsNone)
                return new OperationError($"User Id {userId} is invalid'", OperationFailure.BadInput);

            var role = _localRoleOptionsService.GetOption(registration.OrganizationId, roleId);

            if (role.IsNone)
                return new OperationError($"Role Id {roleId} is invalid'", OperationFailure.BadInput);

            return registration.RemoveRole(role.Value.option, user.Value);
        }
    }
}
