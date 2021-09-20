using System.Linq;
using Core.Abstractions.Types;
using Core.DomainModel.ItSystemUsage.Read;

namespace Core.ApplicationServices.SystemUsage.ReadModels
{
    public interface IItsystemUsageOverviewReadModelsService
    {
        Result<IQueryable<ItSystemUsageOverviewReadModel>, OperationError> GetByOrganizationId(int organizationId);
        Result<IQueryable<ItSystemUsageOverviewReadModel>, OperationError> GetByOrganizationAndResponsibleOrganizationUnitId(int organizationId, int responsibleOrganizationUnit);

    }
}
