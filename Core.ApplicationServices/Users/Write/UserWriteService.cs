using System;
using System.Collections.Generic;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
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

        public UserWriteService(IUserService userService,
            IEntityIdentityResolver entityIdentityResolver,
            IOrganizationRightsService organizationRightsService,
            ITransactionManager transactionManager)
        {
            _userService = userService;
            _entityIdentityResolver = entityIdentityResolver;
            _organizationRightsService = organizationRightsService;
            _transactionManager = transactionManager;
        }

        public Result<User, OperationError> Create(Guid organizationUuid, CreateUserParameters parameters)
        {
            using var transaction = _transactionManager.Begin();

            var organizationIdResult = _entityIdentityResolver.ResolveDbId<Organization>(organizationUuid);
            if (organizationIdResult.IsNone)
            {
                return new OperationError($"Organization with uuid {organizationUuid} was not found",
                    OperationFailure.NotFound);
            }

            var organizationId = organizationIdResult.Value;
            var user = _userService.AddUser(parameters.User, parameters.SendMailOnCreation, organizationId);

            var roleAssignmentError = AssignUserAdministrativeRoles(organizationId, user.Id, parameters.Roles);

            if (roleAssignmentError.HasValue)
            {
                transaction.Rollback();
                return roleAssignmentError.Value;
            }

            transaction.Commit();
            return  user;
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
