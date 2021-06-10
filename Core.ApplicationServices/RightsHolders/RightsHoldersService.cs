
using System.Linq;
using Core.ApplicationServices.Authorization;
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

        public RightsHoldersService(IOrganizationalUserContext userContext, IGenericRepository<Organization> organizationRepository)
        {
            _userContext = userContext;
            _organizationRepository = organizationRepository;
        }

        public Result<IQueryable<Organization>, OperationError> ResolveOrganizationsWhereAuthenticatedUserHasRightsHolderAccess()
        {
            var organizationIds = _userContext.GetOrganizationIdsWhereHasRole(OrganizationRole.RightsHolderAccess).ToList();

            var organizations = _organizationRepository.AsQueryable().ByIds(organizationIds);

            return Result<IQueryable<Organization>, OperationError>.Success(organizations);
        }
    }
}
