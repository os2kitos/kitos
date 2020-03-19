using System;
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
        private readonly ISsoOrganizationIdentityRepository _ssoOrganizationIdentityRepository;
        private readonly IUserRepository _userRepository;

        public PrivilegeVerifiedState(Guid userUuid,
            IUserRepository userRepository,
            IStsBrugerInfoService stsBrugerInfoService,
            ISsoUserIdentityRepository ssoUserIdentityRepository,
            ISsoOrganizationIdentityRepository ssoOrganizationIdentityRepository)
        {
            _userUuid = userUuid;
            _stsBrugerInfoService = stsBrugerInfoService;
            _ssoUserIdentityRepository = ssoUserIdentityRepository;
            _userRepository = userRepository;
            _ssoOrganizationIdentityRepository = ssoOrganizationIdentityRepository;
        }

        public override void Handle(FlowEvent @event, FlowContext context)
        {
            if (@event.Equals(FlowEvent.UserPrivilegeVerified))
            {
                var stsBrugerInfo = _stsBrugerInfoService.GetStsBrugerInfo(_userUuid);
                if (!stsBrugerInfo.HasValue)
                {
                    throw new ApplicationException("PrivilegeVerifiedState: Unable to extract user info from STS");
                }

                if (_ssoUserIdentityRepository.GetByExternalUuid(stsBrugerInfo.Value.Uuid).HasValue)
                {
                    context.TransitionTo(new UserLoggedInState());
                    context.HandleUserSeenBefore();
                }
                else
                {
                    foreach (var email in stsBrugerInfo.Value.Emails)
                    {
                        var user = _userRepository.GetByEmail(email);
                        if (user != null)
                        {
                            context.TransitionTo(new UserIdentifiedState(user, _userUuid, stsBrugerInfo.Value.BelongsToOrganizationUuid, _ssoUserIdentityRepository, _ssoOrganizationIdentityRepository));
                            context.HandleUserFirstTimeVisit();
                        }
                    }
                    // TODO: ?? Further handling in KITOSUDV-627: User creation flow
                }
            }
        }
    }
}