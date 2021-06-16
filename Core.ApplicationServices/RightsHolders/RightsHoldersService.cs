
using System;
using System.Linq;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.System;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Organization;
using Core.DomainModel.Result;
using Core.DomainServices;
using Core.DomainServices.Extensions;
using Core.DomainServices.Queries.ItSystem;
using Infrastructure.Services.Types;

namespace Core.ApplicationServices.RightsHolders
{
    public class RightsHoldersService : IRightsHoldersService
    {
        private readonly IOrganizationalUserContext _userContext;
        private readonly IGenericRepository<Organization> _organizationRepository;
        private readonly IItSystemService _systemService;

        public RightsHoldersService(IOrganizationalUserContext userContext, IGenericRepository<Organization> organizationRepository, IItSystemService systemService)
        {
            _userContext = userContext;
            _organizationRepository = organizationRepository;
            _systemService = systemService;
        }

        public IQueryable<Organization> ResolveOrganizationsWhereAuthenticatedUserHasRightsHolderAccess()
        {
            var organizationIds = _userContext.GetOrganizationIdsWhereHasRole(OrganizationRole.RightsHolderAccess).ToList();
            return _organizationRepository.AsQueryable().ByIds(organizationIds);
        }

        public Result<IQueryable<ItSystem>, OperationError> GetSystemsWhereAuthenticatedUserHasRightsHolderAccess()
        {
            return WithAnyRightsHoldersAccess()
                .Match(
                    error => error,
                    () =>
                    {
                        var organizationIdsWhereUserHasRightsHoldersAccess = _userContext.GetOrganizationIdsWhereHasRole(OrganizationRole.RightsHolderAccess);
                        var query = new QueryByRightsHolderIdOrOwnOrganizationIds(organizationIdsWhereUserHasRightsHoldersAccess);
                        return Result<IQueryable<ItSystem>, OperationError>.Success(_systemService.GetAvailableSystems(query));
                    }
                );
        }

        public Result<ItSystem, OperationError> GetSystemAsRightsHolder(Guid systemUuid)
        {
            return WithAnyRightsHoldersAccess()
                .Match(
                    error => error,
                    () => _systemService.GetSystem(systemUuid).Bind(WithRightsHolderAccessTo)
                );
        }

        private Maybe<OperationError> WithAnyRightsHoldersAccess()
        {
            return _userContext.HasRoleInAnyOrganization(OrganizationRole.RightsHolderAccess)
                ? Maybe<OperationError>.None
                : new OperationError("User does not have 'rightsholders access' in any organization", OperationFailure.Forbidden);
        }

        private Result<ItSystem, OperationError> WithRightsHolderAccessTo(ItSystem system)
        {
            //User may have read access in a different context (own systems but not with rightsholder set to a rightsholding organization) but in this case we insist that rightsholder access must be issued
            var hasAssignedRightsHolderAccess = system
                .GetRightsHolderOrganizationId()
                .Select(organizationId => _userContext.HasRole(organizationId, OrganizationRole.RightsHolderAccess))
                .GetValueOrFallback(false);

            if (hasAssignedRightsHolderAccess)
                return system;

            return new OperationError("Not rightsholder for the requested system", OperationFailure.Forbidden);
        }
    }
}
