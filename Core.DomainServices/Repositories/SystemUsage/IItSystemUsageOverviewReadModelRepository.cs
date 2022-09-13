using System.Linq;
using Core.Abstractions.Types;
using Core.DomainModel.ItSystemUsage.Read;


namespace Core.DomainServices.Repositories.SystemUsage
{
    public interface IItSystemUsageOverviewReadModelRepository
    {
        IQueryable<ItSystemUsageOverviewReadModel> GetByOrganizationId(int organizationId);
        ItSystemUsageOverviewReadModel Add(ItSystemUsageOverviewReadModel newModel);
        void DeleteBySourceId(int sourceId);
        Maybe<ItSystemUsageOverviewReadModel> GetBySourceId(int sourceId);
        void Update(ItSystemUsageOverviewReadModel readModel);
        IQueryable<ItSystemUsageOverviewReadModel> GetByUserId(int userId);
        IQueryable<ItSystemUsageOverviewReadModel> GetByOrganizationUnitId(int organizationUnitId);
        IQueryable<ItSystemUsageOverviewReadModel> GetByDependentOrganizationId(int organizationId);
        IQueryable<ItSystemUsageOverviewReadModel> GetByBusinessTypeId(int businessTypeId);
        IQueryable<ItSystemUsageOverviewReadModel> GetByContractId(int contractId);
        IQueryable<ItSystemUsageOverviewReadModel> GetByDataProcessingRegistrationId(int dataProcessingRegistrationId);
        IQueryable<ItSystemUsageOverviewReadModel> GetByItInterfaceId(int interfaceId);
        IQueryable<ItSystemUsageOverviewReadModel> GetReadModelsMustUpdateToChangeActiveState();
    }
}
