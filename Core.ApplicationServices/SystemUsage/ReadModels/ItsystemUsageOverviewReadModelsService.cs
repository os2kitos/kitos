using System.Linq;
using Core.ApplicationServices.Authorization;
using Core.DomainModel.ItSystemUsage.Read;
using Core.DomainModel.Result;
using Core.DomainServices.Authorization;
using Core.DomainServices.Repositories.Organization;
using Core.DomainServices.Repositories.SystemUsage;

namespace Core.ApplicationServices.SystemUsage.ReadModels
{
    public class ItsystemUsageOverviewReadModelsService : IItsystemUsageOverviewReadModelsService
    {
        private readonly IItSystemUsageOverviewReadModelRepository _repository;
        private readonly IAuthorizationContext _authorizationContext;
        private readonly IOrganizationUnitRepository _organizationUnitRepository;

        public ItsystemUsageOverviewReadModelsService(IItSystemUsageOverviewReadModelRepository repository, IAuthorizationContext authorizationContext, IOrganizationUnitRepository organizationUnitRepository)
        {
            _repository = repository;
            _authorizationContext = authorizationContext;
            _organizationUnitRepository = organizationUnitRepository;
        }

        public Result<IQueryable<ItSystemUsageOverviewReadModel>, OperationError> GetByOrganizationId(int organizationId)
        {
            if (_authorizationContext.GetOrganizationReadAccessLevel(organizationId) != OrganizationDataReadAccessLevel.All)
                return new OperationError(OperationFailure.Forbidden);

            return Result<IQueryable<ItSystemUsageOverviewReadModel>, OperationError>.Success(_repository.GetByOrganizationId(organizationId));
        }

        public Result<IQueryable<ItSystemUsageOverviewReadModel>, OperationError> GetByOrganizationAndResponsibleOrganizationUnitId(int organizationId, int responsibleOrganizationUnit)
        {
            if (_authorizationContext.GetOrganizationReadAccessLevel(organizationId) != OrganizationDataReadAccessLevel.All)
                return new OperationError(OperationFailure.Forbidden);

            var orgUnitTreeIds = _organizationUnitRepository.GetIdsOfSubTree(organizationId, responsibleOrganizationUnit).ToList();
            var readModels = _repository
                .GetByOrganizationId(organizationId)
                .Where(model=>model.ResponsibleOrganizationUnitId != null && orgUnitTreeIds.Contains(model.ResponsibleOrganizationUnitId.Value));

            return Result<IQueryable<ItSystemUsageOverviewReadModel>, OperationError>.Success(readModels);
        }
    }
}
