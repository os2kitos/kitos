using System;
using Core.DomainModel;
using Core.DomainModel.Result;
using Core.DomainModel.SSO;
using Core.DomainServices.Repositories.SSO;

namespace Core.ApplicationServices.SSO.State
{
    public class UserIdentifiedState : AbstractState
    {
        private readonly User _user;
        private readonly Guid _externalUuid;
        private readonly Guid _belongsToOrganizationUuid;
        private readonly ISsoUserIdentityRepository _ssoUserIdentityRepository;
        private readonly ISsoOrganizationIdentityRepository _ssoOrganizationIdentityRepository;

        public UserIdentifiedState(User user, Guid externalUuid, Guid belongsToOrganizationUuid,
            ISsoUserIdentityRepository ssoUserIdentityRepository,
            ISsoOrganizationIdentityRepository ssoOrganizationIdentityRepository)
        {
            _user = user;
            _externalUuid = externalUuid;
            _belongsToOrganizationUuid = belongsToOrganizationUuid;
            _ssoUserIdentityRepository = ssoUserIdentityRepository;
            _ssoOrganizationIdentityRepository = ssoOrganizationIdentityRepository;
        }

        public override void Handle(FlowEvent @event, FlowContext context)
        {
            if (@event.Equals(FlowEvent.UserFirstTimeVisit))
            {
                _ssoUserIdentityRepository.AddNew(_user, _externalUuid);

                // TODO: Lookup organization in Kitos db
                Maybe<SsoOrganizationIdentity> byExternalUuid = _ssoOrganizationIdentityRepository.GetByExternalUuid(_belongsToOrganizationUuid);
                if (byExternalUuid.HasValue)
                {
                    context.TransitionTo(new AuthorizingUserState());
                }
                else
                {
                    context.TransitionTo(new AuthorizingUserFromUnknownOrgState());
                }
            }
        }
    }
}