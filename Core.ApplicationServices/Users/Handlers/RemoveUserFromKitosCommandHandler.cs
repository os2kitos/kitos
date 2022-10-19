using System;
using System.Linq;
using Core.Abstractions.Types;
using Core.DomainModel;
using Core.DomainModel.Commands;
using Core.DomainModel.Events;
using Core.DomainServices.Repositories.SSO;

namespace Core.ApplicationServices.Users.Handlers
{
    public class RemoveUserFromKitosCommandHandler : ICommandHandler<RemoveUserFromKitosCommand, Maybe<OperationError>>
    {
        private readonly ISsoUserIdentityRepository _ssoUserIdentityRepository;
        private readonly ICommandBus _commandBus;
        private readonly IDomainEvents _domainEvents;

        public RemoveUserFromKitosCommandHandler(
            ISsoUserIdentityRepository ssoUserIdentityRepository,
            ICommandBus commandBus,
            IDomainEvents domainEvents)
        {
            _ssoUserIdentityRepository = ssoUserIdentityRepository;
            _commandBus = commandBus;
            _domainEvents = domainEvents;
        }

        private void ClearSsoIdentities(User user)
        {
            var roles = user.SsoIdentities;
            if (roles == null)
                return;

            _ssoUserIdentityRepository.DeleteIdentitiesForUser(user);
            roles.Clear();
        }

        public Maybe<OperationError> Execute(RemoveUserFromKitosCommand command)
        {
            var user = command.User;

            _domainEvents.Raise(new EntityBeingDeletedEvent<User>(user));

            var organizationIds = user.GetOrganizationIds().ToList();

            foreach (var organizationId in organizationIds)
            {
                var error = _commandBus.Execute<RemoveUserFromOrganizationCommand, Maybe<OperationError>>(new RemoveUserFromOrganizationCommand(user, organizationId));
                if (error.HasValue)
                    return error;
            }

            ClearSsoIdentities(user);

            Anonymize(user);

            return Maybe<OperationError>.None;
        }

        private static void Anonymize(User user)
        {
            user.LockedOutDate = DateTime.Now;
            user.Name = "Slettet bruger";
            user.Email = $"{Guid.NewGuid()}_deleted_user@kitos.dk";
            user.PhoneNumber = null;
            user.LastName = "";
            user.DeletedDate = DateTime.Now;
            user.Deleted = true;
            user.IsGlobalAdmin = false;
            user.HasApiAccess = false;
            user.HasStakeHolderAccess = false;
        }
    }
}