using System.Linq;
using Core.Abstractions.Types;
using Core.DomainModel.ItContract.Read;

namespace Core.ApplicationServices.Contract.ReadModels
{
    public interface IItContractSupplierOverviewReadModelsService
    {
        Result<IQueryable<ItContractSupplierOverviewReadModel>, OperationError> GetByOrganizationId(int organizationId);
    }
}
