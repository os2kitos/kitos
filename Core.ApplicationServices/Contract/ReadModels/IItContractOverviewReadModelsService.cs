using System.Linq;
using Core.Abstractions.Types;
using Core.DomainModel.ItContract.Read;

namespace Core.ApplicationServices.Contract.ReadModels
{
    public interface IItContractOverviewReadModelsService
    {
        Result<IQueryable<ItContractOverviewReadModel>, OperationError> GetByOrganizationId(int organizationId);
        Result<IQueryable<ItContractOverviewReadModel>, OperationError> GetByOrganizationAndResponsibleOrganizationUnitId(int organizationId, int responsibleOrganizationUnit);
    }
}
