using Core.ApplicationServices.Rights;
using Core.DomainModel;
using Core.DomainModel.Events;
using Core.Abstractions.Extensions;

namespace Core.ApplicationServices.Users.Handlers
{
    public class HandleUserBeingRemovedFromOrganization : IDomainEventHandler<UserBeingRemovedFromOrganizationEvent<User>>
    {
        private readonly IUserRightsService _userRightsService;

        public HandleUserBeingRemovedFromOrganization(IUserRightsService userRightsService)
        {
            _userRightsService = userRightsService;
        }

        public void Handle(UserBeingRemovedFromOrganizationEvent<User> domainFromOrganizationEvent)
        {
            var user = domainFromOrganizationEvent.Entity;
            _userRightsService.RemoveAllRights(user.Id, domainFromOrganizationEvent.OrganizationId).ThrowOnValue();
        }
    }
}
