using Core.ApplicationServices.Authorization;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Core.DomainModel.Result;
using Core.DomainServices.Extensions;
using Core.DomainServices.Repositories.Organization;
using Infrastructure.Services.Types;
using System.Linq;

namespace Core.ApplicationServices.RightsHolders
{
    public abstract class BaseRightsHolderService
    {

        private readonly IOrganizationalUserContext _userContext;
        private readonly IOrganizationRepository _organizationRepository;

        protected BaseRightsHolderService(IOrganizationalUserContext userContext, IOrganizationRepository organizationRepository)
        {
            _userContext = userContext;
            _organizationRepository = organizationRepository;
        }

        protected Maybe<OperationError> WithAnyRightsHoldersAccess()
        {
            return _userContext.HasRoleInAnyOrganization(OrganizationRole.RightsHolderAccess)
                ? Maybe<OperationError>.None
                : new OperationError("User does not have 'rightsholders access' in any organization", OperationFailure.Forbidden);
        }

        protected Result<T, OperationError> WithRightsHolderAccessTo<T>(T subject) where T : IHasRightsHolder
        {
            //User may have read access in a different context (own systems but not with rightsholder set to a rightsholding organization) but in this case we insist that rightsholder access must be issued
            var hasAssignedRightsHolderAccess = subject
                .GetRightsHolderOrganizationId()
                .Select(organizationId => _userContext.HasRole(organizationId, OrganizationRole.RightsHolderAccess))
                .GetValueOrFallback(false);

            if (hasAssignedRightsHolderAccess)
                return subject;

            return new OperationError("Not rightsholder for the requested system", OperationFailure.Forbidden);
        }

        public IQueryable<Organization> ResolveOrganizationsWhereAuthenticatedUserHasRightsHolderAccess()
        {
            var organizationIds = _userContext.GetOrganizationIdsWhereHasRole(OrganizationRole.RightsHolderAccess).ToList();
            return _organizationRepository.GetAll().ByIds(organizationIds);
        }
    }
}
