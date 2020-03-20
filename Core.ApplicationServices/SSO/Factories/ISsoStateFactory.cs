using System;
using Core.ApplicationServices.SSO.State;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Core.DomainServices.SSO;

namespace Core.ApplicationServices.SSO.Factories
{
    public interface ISsoStateFactory
    {
        AbstractState CreateInitialState();
        AbstractState CreatePrivilegeVerifiedState(Guid userExternalUuid);
        AbstractState CreateUserLoggedIn(User valueUser);
        AbstractState CreateUserIdentifiedState(User user, StsBrugerInfo stsBrugerInfo);
        AbstractState CreateAuthorizingUserState(User user, Organization organization);
        AbstractState CreateAuthorizingUserFromUnknownOrgState(User user);
    }
}
