using System;
using Core.ApplicationServices.SSO.Factories;
using Core.DomainModel;
using Core.DomainServices.Repositories.SSO;
using Core.DomainServices.SSO;

namespace Core.ApplicationServices.SSO.State
{
    public class UserIdentifiedState : AbstractState
    {
        private readonly User _user;
        private readonly StsBrugerInfo _externalUser;
        private readonly ISsoUserIdentityRepository _ssoUserIdentityRepository;
        private readonly ISsoOrganizationIdentityRepository _ssoOrganizationIdentityRepository;
        private readonly ISsoStateFactory _ssoStateFactory;

        public UserIdentifiedState(
            User user,
            StsBrugerInfo externalUser,
            ISsoUserIdentityRepository ssoUserIdentityRepository,
            ISsoOrganizationIdentityRepository ssoOrganizationIdentityRepository, 
            ISsoStateFactory ssoStateFactory)
        {
            _user = user;
            _externalUser = externalUser;
            _ssoUserIdentityRepository = ssoUserIdentityRepository;
            _ssoOrganizationIdentityRepository = ssoOrganizationIdentityRepository;
            _ssoStateFactory = ssoStateFactory;
        }

        public override void Handle(FlowEvent @event, FlowContext context)
        {
            if (@event.Equals(FlowEvent.UserFirstTimeVisit))
            {
                var result = _ssoUserIdentityRepository.AddNew(_user, _externalUser.Uuid);
                if (result.Failed)
                {
                    //TODO Handle the error
                }
                //TODO: Handle error result

                var organizationResult = _ssoOrganizationIdentityRepository.GetByExternalUuid(_externalUser.BelongsToOrganizationUuid);
                if (organizationResult.HasValue)
                {
                    context.TransitionTo(_ssoStateFactory.CreateAuthorizingUserState(_user,organizationResult.Value.Organization));
                    context.HandleOrganizationFound();
                }
                else
                {
                    context.TransitionTo(_ssoStateFactory.CreateAuthorizingUserFromUnknownOrgState(_user,_externalUser));
                    context.HandleOrganizationNotFound();
                }
            }
        }
    }
}