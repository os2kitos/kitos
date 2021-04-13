using System.Linq;
using Core.ApplicationServices.Authorization;
using Core.DomainModel.ItSystemUsage.Read;
using Core.DomainModel.Result;
using Core.DomainServices.Authorization;
using Core.DomainServices.Repositories.SystemUsage;

namespace Core.ApplicationServices.SystemUsage
{
    public class ItsystemUsageOverviewReadModelsService : IItsystemUsageOverviewReadModelsService
    {
        private readonly IItSystemUsageOverviewReadModelRepository _repository;
        private readonly IAuthorizationContext _authorizationContext;

        public ItsystemUsageOverviewReadModelsService(IItSystemUsageOverviewReadModelRepository repository, IAuthorizationContext authorizationContext)
        {
            _repository = repository;
            _authorizationContext = authorizationContext;
        }

        public Result<IQueryable<ItSystemUsageOverviewReadModel>, OperationError> GetByOrganizationId(int organizationId)
        {
            if (_authorizationContext.GetOrganizationReadAccessLevel(organizationId) != OrganizationDataReadAccessLevel.All)
                return new OperationError(OperationFailure.Forbidden);

            return Result<IQueryable<ItSystemUsageOverviewReadModel>, OperationError>.Success(_repository.GetByOrganizationId(organizationId));
        }
    }
}
