﻿using System;
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
                var userResult = _ssoUserIdentityRepository.GetByExternalUuid(_userUuid);
                if (userResult.HasValue)
                {
                    context.TransitionTo(_ssoStateFactory.CreateUserLoggedIn(userResult.Value.User));
                    context.HandleUserSeenBefore();
                }
                else
                {
                    var stsBrugerInfo = _stsBrugerInfoService.GetStsBrugerInfo(_userUuid);
                    if (!stsBrugerInfo.HasValue)
                    {
                        context.TransitionTo(new ErrorState());
                        context.HandleUnableToResolveUserInStsOrganisation();
                    }
                    else
                    {
                        var userByEmail = FindUserByEmail(context, stsBrugerInfo);
                        if (userByEmail.HasValue)
                        {
                            context.TransitionTo(_ssoStateFactory.CreateUserIdentifiedState(userByEmail.Value, stsBrugerInfo.Value));
                            context.HandleUserFirstTimeVisit();
                        }
                        else
                        {
                            // TODO: In KITOSUDV-627 switch to UserNotFoundState
                            context.TransitionTo(new ErrorState());
                            context.HandleUnsupportedFlow();
                        }
                    }

                }
            }
        }

        private Maybe<User> FindUserByEmail(FlowContext context, Maybe<StsBrugerInfo> stsBrugerInfo)
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