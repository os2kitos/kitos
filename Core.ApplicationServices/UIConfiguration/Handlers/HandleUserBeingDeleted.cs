using System.Linq;
using Core.Abstractions.Extensions;
using Core.ApplicationServices.Rights;
using Core.DomainModel;
using Core.DomainModel.Events;
using Core.DomainServices.Repositories.SSO;

namespace Core.ApplicationServices.UIConfiguration.Handlers
{
    public class HandleUserBeingDeleted : IDomainEventHandler<EntityBeingDeletedEvent<User>>
    {
        private readonly ISsoUserIdentityRepository _ssoUserIdentityRepository;
        private readonly IUserRightsService _userRightsService;


        public HandleUserBeingDeleted(ISsoUserIdentityRepository ssoUserIdentityRepository, IUserRightsService userRightsService)
        {
            _ssoUserIdentityRepository = ssoUserIdentityRepository;
            _userRightsService = userRightsService;
        }

        public void Handle(EntityBeingDeletedEvent<User> domainEvent)
        {
            var user = domainEvent.Entity;

            var organizations = user.GetOrganizations().ToList();
            foreach (var organization in organizations)
            {
                _userRightsService.RemoveAllRights(user.Id, organization.Id).ThrowOnValue();
            }
            
            ClearSsoIdentities(user);
        }

        private void ClearSsoIdentities(User user)
        {
            var roles = user.SsoIdentities;
            if (roles == null)
                return;

            _ssoUserIdentityRepository.DeleteIdentitiesForUser(user);
            roles.Clear();
        }

    }
}