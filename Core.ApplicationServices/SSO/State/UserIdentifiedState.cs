using Core.ApplicationServices.SSO.Factories;
using Core.DomainModel;
using Core.DomainServices.Repositories.Organization;
using Core.DomainServices.Repositories.SSO;
using Core.DomainServices.SSO;
using Serilog;

namespace Core.ApplicationServices.SSO.State
{
    public class UserIdentifiedState : AbstractState
    {
        private readonly User _user;
        private readonly StsBrugerInfo _externalUser;
        private readonly ISsoUserIdentityRepository _ssoUserIdentityRepository;
        private readonly ISsoOrganizationIdentityRepository _ssoOrganizationIdentityRepository;
        private readonly IOrganizationRepository _organizationRepository;
        private readonly ISsoStateFactory _ssoStateFactory;
        private readonly ILogger _logger;

        public UserIdentifiedState(
            User user,
            StsBrugerInfo externalUser,
            ISsoUserIdentityRepository ssoUserIdentityRepository,
            ISsoOrganizationIdentityRepository ssoOrganizationIdentityRepository,
            IOrganizationRepository organizationRepository,
            ISsoStateFactory ssoStateFactory,
            ILogger logger)
        {
            _user = user;
            _externalUser = externalUser;
            _ssoUserIdentityRepository = ssoUserIdentityRepository;
            _ssoOrganizationIdentityRepository = ssoOrganizationIdentityRepository;
            _organizationRepository = organizationRepository;
            _ssoStateFactory = ssoStateFactory;
            _logger = logger;
        }

        public override void Handle(FlowEvent @event, FlowContext context)
        {
            if (@event.Equals(FlowEvent.UserFirstTimeSsoVisit))
            {
                AssociateUserWithExternalIdentity();
                HandleUserWithSsoIdentity(context);
            }
            else if (@event.Equals(FlowEvent.ExistingSsoUserWithoutRoles))
            {
                HandleUserWithSsoIdentity(context);
            }
        }

        private void AssociateUserWithExternalIdentity()
        {
            //On first time visit we add the binding for the user
            var result = _ssoUserIdentityRepository.AddNew(_user, _externalUser.Uuid);
            if (result.Failed)
            {
                //NOTE: This is not a blocker! - but we log the error.. authentication is still allowed to proceed - it just failed to save the relation between user id and external uuid.
                _logger.Error(
                    "Failed to save SSO relation between external user uuid {userUuid} and user with {userId} which logged in using external org uuid {orgUuid}",
                    _externalUser.Uuid, _user.Id, _externalUser.BelongsToOrganizationUuid);
            }
        }

        private void HandleUserWithSsoIdentity(FlowContext context)
        {
            //Transition to authorizing state if organization sso binding already exists
            var organizationByExternalIdResult = _ssoOrganizationIdentityRepository.GetByExternalUuid(_externalUser.BelongsToOrganizationUuid);
            if (organizationByExternalIdResult.HasValue)
            {
                context.TransitionTo(_ssoStateFactory.CreateAuthorizingUserState(_user, organizationByExternalIdResult.Value.Organization),
                    _ => _.HandleOrganizationFound());
            }
            else
            {
                //If no sso binding exists for the organization, try to create one by finding the org by cvr and adding the sso relation
                var organizationByCvrResult = _organizationRepository.GetByCvr(_externalUser.MunicipalityCvr);
                if (organizationByCvrResult.HasValue)
                {
                    var organization = organizationByCvrResult.Value;
                    var addOrganizationIdentityResult = _ssoOrganizationIdentityRepository.AddNew(organization, _externalUser.BelongsToOrganizationUuid);
                    if (addOrganizationIdentityResult.Failed)
                    {
                        //NOTE: This is not a blocker! - concurrency might be to blame but we log the error.. authentication is still allowed to proceed - it just failed to save the relation between org id and external uuid.
                        _logger.Error(
                            "Failed to save SSO relation between org uuid {orgUuid} and organization {orgId} for while authenticating user with id {userId}",
                            _externalUser.BelongsToOrganizationUuid, organization.Id, _user.Id);
                    }

                    context.TransitionTo(_ssoStateFactory.CreateAuthorizingUserState(_user, organization),
                        _ => _.HandleOrganizationFound());
                }
                else
                {
                    context.TransitionTo(_ssoStateFactory.CreateAuthorizingUserFromUnknownOrgState(_user), 
                        _ => _.HandleOrganizationNotFound());
                }
            }
        }
    }
}