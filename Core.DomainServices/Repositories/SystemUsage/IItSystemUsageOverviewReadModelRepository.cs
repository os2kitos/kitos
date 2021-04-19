using System.Collections.Generic;
using System.Linq;
using Core.DomainModel.ItSystemUsage.Read;
using Infrastructure.Services.Types;

namespace Core.DomainServices.Repositories.SystemUsage
{
    public interface IItSystemUsageOverviewReadModelRepository
    {
        IQueryable<ItSystemUsageOverviewReadModel> GetByOrganizationId(int organizationId);
        ItSystemUsageOverviewReadModel Add(ItSystemUsageOverviewReadModel newModel);
        void Delete(ItSystemUsageOverviewReadModel readModel);
        void DeleteBySourceId(int sourceId);
        Maybe<ItSystemUsageOverviewReadModel> GetBySourceId(int sourceId);
        void Update(ItSystemUsageOverviewReadModel readModel);
        IQueryable<ItSystemUsageOverviewReadModel> GetByUserId(int userId);
        IQueryable<ItSystemUsageOverviewReadModel> GetByOrganizationUnitId(int organizationUnitId);
        IQueryable<ItSystemUsageOverviewReadModel> GetByRightsHolderId(int organizationId);
        IQueryable<ItSystemUsageOverviewReadModel> GetByBusinessTypeId(int businessTypeId);
        IQueryable<ItSystemUsageOverviewReadModel> GetByObjectOwnerId(int objectOwnerId);
        IQueryable<ItSystemUsageOverviewReadModel> GetByLastChangeById(int lastChangeById);
    }
}
