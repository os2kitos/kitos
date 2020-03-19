using System;
using Core.ApplicationServices.SSO.Factories;
using Core.DomainServices;
using Core.DomainServices.Repositories.SSO;
using Core.DomainServices.SSO;

namespace Core.ApplicationServices.SSO.State
{
    public class PrivilegeVerifiedState : AbstractState
    {
        private readonly Guid _userUuid;
        private readonly IStsBrugerInfoService _stsBrugerInfoService;
        private readonly ISsoUserIdentityRepository _ssoUserIdentityRepository;
        private readonly ISsoStateFactory _ssoStateFactory;
        private readonly IUserRepository _userRepository;

        public PrivilegeVerifiedState(
            Guid userUuid,
            IUserRepository userRepository,
            IStsBrugerInfoService stsBrugerInfoService,
            ISsoUserIdentityRepository ssoUserIdentityRepository,
            ISsoStateFactory ssoStateFactory)
        {
            _userUuid = userUuid;
            _stsBrugerInfoService = stsBrugerInfoService;
            _ssoUserIdentityRepository = ssoUserIdentityRepository;
            _userRepository = userRepository;
            _ssoStateFactory = ssoStateFactory;
        }

        public override void Handle(FlowEvent @event, FlowContext context)
        {
            if (@event.Equals(FlowEvent.UserPrivilegeVerified))
            {
                var stsBrugerInfo = _stsBrugerInfoService.GetStsBrugerInfo(_userUuid);
                if (!stsBrugerInfo.HasValue)
                {
                    //TODO: Not exception just bail out
                    throw new ApplicationException("PrivilegeVerifiedState: Unable to extract user info from STS");
                }

                var userResult = _ssoUserIdentityRepository.GetByExternalUuid(stsBrugerInfo.Value.Uuid);
                if (userResult.HasValue)
                {
                    context.TransitionTo(_ssoStateFactory.CreateUserLoggedIn(userResult.Value.User));
                    context.HandleUserSeenBefore();
                }
                else
                {
                    foreach (var email in stsBrugerInfo.Value.Emails)
                    {
                        var user = _userRepository.GetByEmail(email);
                        if (user != null)
                        {
                            context.TransitionTo(_ssoStateFactory.CreateUserIdentifiedState(user, stsBrugerInfo.Value));
                            context.HandleUserFirstTimeVisit();
                            return;
                        }
                    }
                    // TODO: ?? Further handling in KITOSUDV-627: User creation flow
                }
            }
        }
    }
}