
using System;
using System.Linq;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.System;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Organization;
using Core.DomainModel.Result;
using Core.DomainServices;
using Core.DomainServices.Extensions;

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

        public Result<IQueryable<ItSystem>, OperationError> GetAvailableSystems()
        {
            return _userContext.HasRoleInAnyOrganization(OrganizationRole.RightsHolderAccess)
                ? Result<IQueryable<ItSystem>, OperationError>.Success(_systemService.GetAvailableSystems())
                : new OperationError("User does not have 'rightsholders access' in any organization", OperationFailure.Forbidden);
        }

        public Result<ItSystem, OperationError> GetSystem(Guid systemUuid)
        {
            return _userContext.HasRoleInAnyOrganization(OrganizationRole.RightsHolderAccess)
                ? _systemService.GetSystem(systemUuid)
                : new OperationError("User does not have 'rightsholders access' in any organization", OperationFailure.Forbidden);
        }
    }
}
