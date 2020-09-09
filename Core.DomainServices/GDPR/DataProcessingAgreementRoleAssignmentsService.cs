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
    public class DataProcessingAgreementRoleAssignmentsService : IDataProcessingAgreementRoleAssignmentsService
    {
        private readonly IOptionsService<DataProcessingAgreementRight, DataProcessingAgreementRole> _localRoleOptionsService;
        private readonly IUserRepository _userRepository;

        public DataProcessingAgreementRoleAssignmentsService(
            IOptionsService<DataProcessingAgreementRight, DataProcessingAgreementRole> localRoleOptionsService,
            IUserRepository userRepository)
        {
            _localRoleOptionsService = localRoleOptionsService;
            _userRepository = userRepository;
        }

        public IQueryable<User> GetUsersWhichCanBeAssignedToRole(DataProcessingAgreement agreement, DataProcessingAgreementRole role, IQueryable<User> candidates)
        {
            if (agreement == null) throw new ArgumentNullException(nameof(agreement));
            if (role == null) throw new ArgumentNullException(nameof(role));
            if (candidates == null) throw new ArgumentNullException(nameof(candidates));

            var usersWithSameRole = GetIdsOfUsersAssignedToRole(agreement, role);
            return candidates.ExceptEntitiesWithIds(usersWithSameRole.ToList());
        }

        private static IEnumerable<int> GetIdsOfUsersAssignedToRole(DataProcessingAgreement agreement, DataProcessingAgreementRole role)
        {
            return agreement.GetRights(role.Id).Select(x => x.UserId).Distinct();
        }

        public IEnumerable<DataProcessingAgreementRole> GetApplicableRoles(DataProcessingAgreement agreement)
        {
            if (agreement == null) throw new ArgumentNullException(nameof(agreement));
            return _localRoleOptionsService.GetAvailableOptions(agreement.OrganizationId);
        }

        public Result<IQueryable<User>, OperationError> GetUsersWhichCanBeAssignedToRole(DataProcessingAgreement agreement, int roleId, Maybe<string> nameEmailQuery)
        {
            if (agreement == null) throw new ArgumentNullException(nameof(agreement));
            var availableRoles = _localRoleOptionsService.GetAvailableOptions(agreement.OrganizationId);

            var targetRole = availableRoles.FirstOrDefault(x => x.Id == roleId).FromNullable();

            if (targetRole.IsNone)
                return new OperationError("Invalid role id", OperationFailure.BadInput);

            var candidates = _userRepository.SearchOrganizationUsers(agreement.OrganizationId, nameEmailQuery);
            var usersWithSameRole = GetIdsOfUsersAssignedToRole(agreement, targetRole.Value);
            return Result<IQueryable<User>, OperationError>.Success(candidates.ExceptEntitiesWithIds(usersWithSameRole.ToList()));
        }

        public Result<DataProcessingAgreementRight, OperationError> AssignRole(DataProcessingAgreement agreement, int roleId, int userId)
        {
            if (agreement == null) throw new ArgumentNullException(nameof(agreement));

            var availableUsers = GetUsersWhichCanBeAssignedToRole(agreement, roleId, Maybe<string>.None);
            if (availableUsers.Failed)
                return availableUsers.Error;

            var user = availableUsers.Value.FirstOrDefault(x => x.Id == userId).FromNullable();
            if (user.IsNone)
            {
                var failure = OperationFailure.BadInput;
                
                if (agreement.GetRights(roleId).Any(x => x.UserId == userId))
                    failure = OperationFailure.Conflict;

                return new OperationError($"User Id {userId} is invalid in the context of assign role {roleId} to dpa with id {agreement.Id} in organization with id '{agreement.OrganizationId}'", failure);
            }

            var role = _localRoleOptionsService.GetOption(agreement.OrganizationId, roleId).Value.option;

            return agreement.AssignRoleToUser(role, user.Value);
        }

        public Result<DataProcessingAgreementRight, OperationError> RemoveRole(DataProcessingAgreement agreement, int roleId, int userId)
        {
            if (agreement == null) throw new ArgumentNullException(nameof(agreement));

            var user = _userRepository.GetById(userId).FromNullable();
            if (user.IsNone)
                return new OperationError($"User Id {userId} is invalid'", OperationFailure.BadInput);

            var role = _localRoleOptionsService.GetOption(agreement.OrganizationId, roleId);

            if (role.IsNone)
                return new OperationError($"Role Id {roleId} is invalid'", OperationFailure.BadInput);

            return agreement.RemoveRole(role.Value.option, user.Value);
        }
    }
}
