using Core.ApplicationServices.Rights;
using Core.DomainModel;
using Core.DomainModel.Events;
using Core.Abstractions.Extensions;

namespace Core.ApplicationServices.Users.Handlers
{
    public class HandleUserBeingRemoved : IDomainEventHandler<EntityBeingRemovedEvent<User>>
    {
        private readonly IUserRightsService _userRightsService;

        public HandleUserBeingRemoved(IUserRightsService userRightsService)
        {
            _userRightsService = userRightsService;
        }

        public void Handle(EntityBeingRemovedEvent<User> domainEvent)
        {
            var user = domainEvent.Entity;
            _userRightsService.RemoveAllRights(user.Id, domainEvent.OrganizationId).ThrowOnValue();
        }
    }
}
