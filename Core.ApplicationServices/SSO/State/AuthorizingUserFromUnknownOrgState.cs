using System;
using Core.DomainModel;
using Core.DomainServices.SSO;

namespace Core.ApplicationServices.SSO.State
{
    public class AuthorizingUserFromUnknownOrgState : AbstractState
    {
        private readonly User _user;
        private readonly StsBrugerInfo _externalUser;

        public AuthorizingUserFromUnknownOrgState(User user, StsBrugerInfo externalUser)
        {
            _user = user;
            _externalUser = externalUser;
        }

        public override void Handle(FlowEvent @event, FlowContext context)
        {
            if (@event.Equals(FlowEvent.OrganizationNotFound))
            {
                // TODO
                //TODO: Transition to "find by cvr" (extend user info and pass that along the states in stead of small bits)
                //TODO: If find by cvr fails then go to unknown state. If it succeeds, assign the relationship and then transition to authorizing state.
            }
        }
    }
}