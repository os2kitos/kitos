using Core.ApplicationServices.SSO.Factories;
using Core.DomainModel;
using Core.DomainModel.Organization;
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
            if (@event.Equals(FlowEvent.UserFirstTimeVisit))
            {
                var result = _ssoUserIdentityRepository.AddNew(_user, _externalUser.Uuid);
                if (result.Failed)
                {
                    context.TransitionTo(new ErrorState());
                    context.HandleUnknownError();
                    return;
                }

                var organizationByExternalIdResult = _ssoOrganizationIdentityRepository.GetByExternalUuid(_externalUser.BelongsToOrganizationUuid);
                if (organizationByExternalIdResult.HasValue)
                {
                    TransitionToAuthorizingState(context, organizationByExternalIdResult.Value.Organization);
                }
                else
                {
                    //Try to find organization by cvr and associate it
                    var organizationByCvrResult = _organizationRepository.GetByCvr(_externalUser.MunicipalityCvr);
                    if (organizationByCvrResult.HasValue)
                    {
                        var organization = organizationByCvrResult.Value;
                        var addOrganizationIdentityResult = _ssoOrganizationIdentityRepository.AddNew(organization, _externalUser.BelongsToOrganizationUuid);
                        if (addOrganizationIdentityResult.Failed)
                        {
                            //NOTE: This is not a blocker! - concurrency might be to blame but we log the error.. authentication is still allowed to proceed - it just failed to save the relation between org uuid and external uuid.
                            _logger.Warning("Failed to save SSO relation between org uuid {orgUuid} and organization {orgId} for while authenticating user with id {userId}", _externalUser.BelongsToOrganizationUuid, organization.Id, _user.Id);
                        }
                        TransitionToAuthorizingState(context, organization);
                    }
                    else
                    {
                        context.TransitionTo(_ssoStateFactory.CreateAuthorizingUserFromUnknownOrgState(_user, _externalUser));
                        context.HandleOrganizationNotFound();
                    }
                }
            }
        }

        private void TransitionToAuthorizingState(FlowContext context, Organization organization)
        {
            context.TransitionTo(_ssoStateFactory.CreateAuthorizingUserState(_user, organization));
            context.HandleOrganizationFound();
        }
    }
}