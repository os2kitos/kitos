using System.Linq;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.DomainModel.ItContract.Read;
using Core.DomainServices.Authorization;
using Core.DomainServices.Repositories.Contract;

namespace Core.ApplicationServices.Contract.ReadModels
{
    public class ItContractSupplierOverviewReadModelsService(
        IItContractSupplierOverviewReadModelRepository repository,
        IAuthorizationContext authorizationContext)
        : IItContractSupplierOverviewReadModelsService
    {
        public Result<IQueryable<ItContractSupplierOverviewReadModel>, OperationError> GetByOrganizationId(int organizationId)
        {
            if (authorizationContext.GetOrganizationReadAccessLevel(organizationId) != OrganizationDataReadAccessLevel.All)
            {
                return new OperationError(OperationFailure.Forbidden);
            }

            return Result<IQueryable<ItContractSupplierOverviewReadModel>, OperationError>.Success(repository.GetByOrganizationId(organizationId));
        }
    }
}
