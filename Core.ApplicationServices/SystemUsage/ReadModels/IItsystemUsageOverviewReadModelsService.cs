using System.Linq;
using Core.DomainModel.ItSystemUsage.Read;
using Core.DomainModel.Result;

namespace Core.ApplicationServices.SystemUsage.ReadModels
{
    public interface IItsystemUsageOverviewReadModelsService
    {
        Result<IQueryable<ItSystemUsageOverviewReadModel>, OperationError> GetByOrganizationId(int organizationId);
        Result<IQueryable<ItSystemUsageOverviewReadModel>, OperationError> GetByOrganizationAndResponsibleOrganizationUnitId(int organizationId, int responsibleOrganizationUnit);

    }
}
