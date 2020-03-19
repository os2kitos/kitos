using System;
using Core.DomainServices.Repositories.SSO;
using Core.DomainServices.SSO;

namespace Core.ApplicationServices.SSO.State
{
    public class PrivilegeVerifiedState : AbstractState
    {
        private readonly Guid _userUuid;
        private readonly IStsBrugerInfoService _stsBrugerInfoService;
        private readonly ISsoUserIdentityRepository _ssoUserIdentityRepository;

        public PrivilegeVerifiedState(Guid userUuid, IStsBrugerInfoService stsBrugerInfoService, ISsoUserIdentityRepository ssoUserIdentityRepository)
        {
            _userUuid = userUuid;
            _stsBrugerInfoService = stsBrugerInfoService;
            _ssoUserIdentityRepository = ssoUserIdentityRepository;
        }

        public override void Handle(FlowEvent @event, FlowContext context)
        {
            if (@event.Equals(FlowEvent.UserPrivilegeVerified))
            {
                var stsBrugerInfo = _stsBrugerInfoService.GetStsBrugerInfo(_userUuid);
                if (_ssoUserIdentityRepository.GetByExternalUuid(stsBrugerInfo.Value.Uuid).HasValue)
                {
                    // TODO: Fetch and set current user
                    context.TransitionTo(new UserLoggedInState());
                    context.HandleUserSeenBefore();
                }
                else
                {
                    context.TransitionTo(new UserIdentifiedState());
                    context.HandleUserFirstTimeVisit();
                }
            }
        }
    }
}