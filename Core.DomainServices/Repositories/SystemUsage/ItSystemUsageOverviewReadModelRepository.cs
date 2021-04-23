using System;
using System.Linq;
using Core.DomainModel.ItSystemUsage.Read;
using Core.DomainServices.Extensions;
using Infrastructure.Services.Types;

namespace Core.DomainServices.Repositories.SystemUsage
{
    public class ItSystemUsageOverviewReadModelRepository : IItSystemUsageOverviewReadModelRepository
    {
        private readonly IGenericRepository<ItSystemUsageOverviewReadModel> _repository;

        public ItSystemUsageOverviewReadModelRepository(IGenericRepository<ItSystemUsageOverviewReadModel> repository)
        {
            _repository = repository;
        }

        public IQueryable<ItSystemUsageOverviewReadModel> GetByOrganizationId(int organizationId)
        {
            return _repository.AsQueryable().ByOrganizationId(organizationId);
        }

        public ItSystemUsageOverviewReadModel Add(ItSystemUsageOverviewReadModel newModel)
        {
            var existing = GetBySourceId(newModel.SourceEntityId);

            if (existing.HasValue)
                throw new InvalidOperationException("Only one read model per entity is allowed");

            var inserted = _repository.Insert(newModel);
            _repository.Save();
            return inserted;
        }

        public void DeleteBySourceId(int sourceId)
        {
            var readModel = GetBySourceId(sourceId);
            if (readModel.HasValue)
            {
                Delete(readModel.Value);
            }
        }

        public void Delete(ItSystemUsageOverviewReadModel readModel)
        {
            if (readModel == null) throw new ArgumentNullException(nameof(readModel));

            _repository.DeleteWithReferencePreload(readModel);
            _repository.Save();
        }

        public Maybe<ItSystemUsageOverviewReadModel> GetBySourceId(int sourceId)
        {
            return _repository.AsQueryable().FirstOrDefault(x => x.SourceEntityId == sourceId);
        }

        public void Update(ItSystemUsageOverviewReadModel readModel)
        {
            _repository.Save();
        }

        public IQueryable<ItSystemUsageOverviewReadModel> GetByUserId(int userId)
        {
            //Gets all read models that have dependencies on the user
            return _repository
                .AsQueryable()
                .Where(x => x.RoleAssignments.Any(assignment => assignment.UserId == userId) || x.ObjectOwnerId == userId || x.LastChangedById == userId)
                .Distinct();
        }

        public IQueryable<ItSystemUsageOverviewReadModel> GetByOrganizationUnitId(int organizationUnitId)
        {
            //Gets all read models that have dependencies on the organization unit
            return _repository
                .AsQueryable()
                .Where(x => x.ResponsibleOrganizationUnitId == organizationUnitId);
        }

        public IQueryable<ItSystemUsageOverviewReadModel> GetByDependentOrganizationId(int organizationId)
        {
            //Gets all read models that have dependencies on the ItSystems RightsHolder organization or on the MainContracts Supplier
            return _repository
                .AsQueryable()
                .Where(x => x.ItSystemRightsHolderId == organizationId || x.MainContractSupplierId == organizationId);
        }

        public IQueryable<ItSystemUsageOverviewReadModel> GetByBusinessTypeId(int businessTypeId)
        {
            return _repository
                .AsQueryable()
                .Where(x => x.ItSystemBusinessTypeId == businessTypeId);
        }

        public IQueryable<ItSystemUsageOverviewReadModel> GetByContractId(int contractId)
        {
            return _repository
                .AsQueryable()
                .Where(x => x.MainContractId == contractId);
        }

        public IQueryable<ItSystemUsageOverviewReadModel> GetByProjectId(int projectId)
        {
            return _repository
                .AsQueryable()
                .Where(x => x.ItProjects.Select(y => y.ItProjectId).Contains(projectId));
        }

        public IQueryable<ItSystemUsageOverviewReadModel> GetByDataProcessingRegistrationId(int dataProcessingRegistrationId)
        {
            return _repository
                .AsQueryable()
                .Where(x => x.DataProcessingRegistrations.Select(y => y.DataProcessingRegistrationId).Contains(dataProcessingRegistrationId));
        }

        public IQueryable<ItSystemUsageOverviewReadModel> GetByItInterfaceId(int interfaceId)
        {
            return _repository
                .AsQueryable()
                .Where(x => x.AppliedInterfaces.Select(y => y.InterfaceId).Contains(interfaceId));
        }
    }
}
