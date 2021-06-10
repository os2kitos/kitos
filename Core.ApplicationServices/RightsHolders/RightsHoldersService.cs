
using System.Linq;
using Core.ApplicationServices.Authorization;
using Core.DomainModel.Organization;
using Core.DomainServices;
using Core.DomainServices.Extensions;

namespace Core.ApplicationServices.RightsHolders
{
    public class RightsHoldersService : IRightsHoldersService
    {
        private readonly IOrganizationalUserContext _userContext;
        private readonly IGenericRepository<Organization> _organizationRepository;

        public RightsHoldersService(IOrganizationalUserContext userContext, IGenericRepository<Organization> organizationRepository)
        {
            _userContext = userContext;
            _organizationRepository = organizationRepository;
        }

        public IQueryable<Organization> ResolveOrganizationsWhereAuthenticatedUserHasRightsHolderAccess()
        {
            var organizationIds = _userContext.GetOrganizationIdsWhereHasRole(OrganizationRole.RightsHolderAccess).ToList();
            return _organizationRepository.AsQueryable().ByIds(organizationIds);
        }
    }
}
