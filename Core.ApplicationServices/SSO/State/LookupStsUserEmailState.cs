using System;
using Core.DomainServices.SSO;

namespace Core.ApplicationServices.SSO.State
{
    public class LookupStsUserEmailState : AbstractState
    {
        private readonly Guid _userUuid;
        private readonly IStsBrugerInfoService _stsBrugerInfoService;

        public LookupStsUserEmailState(Guid userUuid, IStsBrugerInfoService stsBrugerInfoService)
        {
            _userUuid = userUuid;
            _stsBrugerInfoService = stsBrugerInfoService;
        }

        public override void Handle(FlowEvent @event, FlowContext context)
        {
            if (@event.Equals(FlowEvent.UserHasValidAccessRole))
            {
                var stsBrugerInfo = _stsBrugerInfoService.GetStsBrugerInfo(_userUuid);
                // TODO: Lookup stsBrugerEmails in Kitos User database
                context.TransitionTo(new UserSignedInState());
            }
        }
    }
}