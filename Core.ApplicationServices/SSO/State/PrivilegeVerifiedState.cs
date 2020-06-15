using System;
using Core.ApplicationServices.SSO.Factories;
using Core.DomainModel;
using Core.DomainModel.Result;
using Core.DomainServices;
using Core.DomainServices.Repositories.SSO;
using Core.DomainServices.SSO;

namespace Core.ApplicationServices.SSO.State
{
    public class PrivilegeVerifiedState : AbstractState
    {
        private readonly Guid _userUuid;
        private readonly string _cvrNumber;
        private readonly IStsBrugerInfoService _stsBrugerInfoService;
        private readonly ISsoUserIdentityRepository _ssoUserIdentityRepository;
        private readonly ISsoStateFactory _ssoStateFactory;
        private readonly IUserRepository _userRepository;

        public PrivilegeVerifiedState(
            Guid userUuid,
            string cvrNumber,
            IUserRepository userRepository,
            IStsBrugerInfoService stsBrugerInfoService,
            ISsoUserIdentityRepository ssoUserIdentityRepository,
            ISsoStateFactory ssoStateFactory)
        {
            _userUuid = userUuid;
            _cvrNumber = cvrNumber;
            _stsBrugerInfoService = stsBrugerInfoService;
            _ssoUserIdentityRepository = ssoUserIdentityRepository;
            _userRepository = userRepository;
            _ssoStateFactory = ssoStateFactory;
        }

        public override void Handle(FlowEvent @event, FlowContext context)
        {
            if (@event.Equals(FlowEvent.UserPrivilegeVerified))
            {
                var userResult = _ssoUserIdentityRepository.GetByExternalUuid(_userUuid);
                if (userResult.HasValue) // User has used the same SSO identity before and exists
                {
                    var user = userResult.Value.User;
                    if (user.CanAuthenticate())
                    {
                        context.TransitionTo(_ssoStateFactory.CreateUserLoggedIn(user), 
                            _ => _.HandleUserSeenBefore());
                    }
                    else
                    {
                        var stsBrugerInfo = _stsBrugerInfoService.GetStsBrugerInfo(_userUuid, _cvrNumber);
                        if (!stsBrugerInfo.HasValue)
                        {
                            context.TransitionTo(_ssoStateFactory.CreateErrorState(), _ => _.HandleUnableToResolveUserInStsOrganisation());
                        }
                        else
                        {
                            context.TransitionTo(_ssoStateFactory.CreateUserIdentifiedState(user, stsBrugerInfo.Value), 
                                _ => _.HandleExistingSsoUserWithoutRoles());
                        }
                    }
                }
                else // Try to find the user by email
                {
                    var stsBrugerInfo = _stsBrugerInfoService.GetStsBrugerInfo(_userUuid, _cvrNumber);
                    if (!stsBrugerInfo.HasValue)
                    {
                        context.TransitionTo(_ssoStateFactory.CreateErrorState(), _ => _.HandleUnableToResolveUserInStsOrganisation());
                    }
                    else
                    {
                        var userByKitosEmail = FindUserByEmail(stsBrugerInfo);
                        if (userByKitosEmail.HasValue)
                        {
                            context.TransitionTo(_ssoStateFactory.CreateUserIdentifiedState(userByKitosEmail.Value, stsBrugerInfo.Value), 
                                _ => _.HandleUserFirstTimeSsoVisit());
                        }
                        else
                        {
                            context.TransitionTo(_ssoStateFactory.CreateFirstTimeUserNotFoundState(stsBrugerInfo.Value), 
                                _ => _.HandleUnableToLocateUser());
                        }
                    }

                }
            }
        }

        private Maybe<User> FindUserByEmail(Maybe<StsBrugerInfo> stsBrugerInfo)
        {
            foreach (var email in stsBrugerInfo.Value.Emails)
            {
                var user = _userRepository.GetByEmail(email);
                if (user != null)
                {
                    return user;
                }
            }
            return Maybe<User>.None;
        }
    }
}