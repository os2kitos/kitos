using System.Linq;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.DomainModel.ItContract.Read;
using Core.DomainServices.Authorization;
using Core.DomainServices.Repositories.Contract;

namespace Core.ApplicationServices.Contract.ReadModels
{
    public class ItContractOverviewReadModelsService : IItContractOverviewReadModelsService
    {
        private readonly IItContractOverviewReadModelRepository _repository;
        private readonly IAuthorizationContext _authorizationContext;

        public ItContractOverviewReadModelsService(IItContractOverviewReadModelRepository repository, IAuthorizationContext authorizationContext)
        {
            _repository = repository;
            _authorizationContext = authorizationContext;
        }

        public Result<IQueryable<ItContractOverviewReadModel>, OperationError> GetByOrganizationId(int organizationId)
        {
            if (_authorizationContext.GetOrganizationReadAccessLevel(organizationId) != OrganizationDataReadAccessLevel.All)
            {
                return new OperationError(OperationFailure.Forbidden);
            }

            return Result<IQueryable<ItContractOverviewReadModel>, OperationError>.Success(_repository.GetByOrganizationId(organizationId));
        }

        public Result<IQueryable<ItContractOverviewReadModel>, OperationError> GetByOrganizationIdOrUnitIdInSubTree(int organizationId, int organizationUnitId)
        {
            if (_authorizationContext.GetOrganizationReadAccessLevel(organizationId) != OrganizationDataReadAccessLevel.All)
            {
                return new OperationError(OperationFailure.Forbidden);
            }

            return Result<IQueryable<ItContractOverviewReadModel>, OperationError>.Success(_repository.GetByOrganizationAndResponsibleOrganizationUnitIncludingSubTree(organizationId, organizationUnitId));
        }
    }
}
