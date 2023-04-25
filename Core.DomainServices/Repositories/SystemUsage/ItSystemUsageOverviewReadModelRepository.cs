using System;
using System.Linq;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.DomainModel.ItSystemUsage.Read;
using Core.DomainServices.Extensions;
using Core.DomainServices.Queries.SystemUsage;

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

        private void Delete(ItSystemUsageOverviewReadModel readModel)
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
                .Where(x => x.ResponsibleOrganizationUnitId == organizationUnitId || x.RelevantOrganizationUnits.Any(ru=>ru.OrganizationUnitId == organizationUnitId));
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
                .Where(x => x.AssociatedContracts.Any(ac=>ac.ItContractId == contractId));
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
                .Where(x => x.DependsOnInterfaces.Select(y => y.InterfaceId).Contains(interfaceId));
        }

        public IQueryable<ItSystemUsageOverviewReadModel> GetReadModelsMustUpdateToChangeActiveState()
        {
            var now = DateTime.Now;
            var expiringReadModelIds = _repository
                .AsQueryable()
                .Transform(new QueryReadModelsWhichShouldExpire(now).Apply)
                .Select(x => x.Id);

            var activatingReadModelIds = _repository
                .AsQueryable()
                .Transform(new QueryReadModelsWhichShouldBecomeActive(now).Apply)
                .Select(x => x.Id);

            var idsOfReadModelsWhichMustUpdate = expiringReadModelIds.Concat(activatingReadModelIds).Distinct().ToList();

            return _repository.AsQueryable().ByIds(idsOfReadModelsWhichMustUpdate);
        }
    }
}
