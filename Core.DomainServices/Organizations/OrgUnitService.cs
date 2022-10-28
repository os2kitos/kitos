using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.DomainModel.Extensions;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Organization;
using Core.DomainServices.Extensions;
using Infrastructure.Services.DataAccess;


namespace Core.DomainServices.Organizations
{
    public class OrgUnitService : IOrgUnitService
    {
        private readonly IGenericRepository<OrganizationUnit> _orgUnitRepository;
        private readonly IGenericRepository<Organization> _organizationRepository;
        private readonly IGenericRepository<ItSystemUsageOrgUnitUsage> _itSystemUsageOrgUnitUsageRepository;
        private readonly ITransactionManager _transactionManager;

        public OrgUnitService(IGenericRepository<OrganizationUnit> orgUnitRepository, 
            IGenericRepository<ItSystemUsageOrgUnitUsage> itSystemUsageOrgUnitUsageRepository, 
            ITransactionManager transactionManager, 
            IGenericRepository<Organization> organizationRepository)
        {
            _orgUnitRepository = orgUnitRepository;
            _itSystemUsageOrgUnitUsageRepository = itSystemUsageOrgUnitUsageRepository;
            _transactionManager = transactionManager;
            _organizationRepository = organizationRepository;
        }

        public OrganizationUnit GetRoot(OrganizationUnit unit)
        {
            var whereWeStarted = unit;

            while (unit.Parent != null)
            {
                unit = unit.Parent;

                //did we get a loop?
                if (unit.Id == whereWeStarted.Id) throw new Exception("Loop in Organization Units");
            }

            return unit;
        }

        public ICollection<OrganizationUnit> GetSubTree(int orgUnitId)
        {
            var orgUnit = _orgUnitRepository.GetByKey(orgUnitId);

            return orgUnit.FlattenHierarchy().ToList();
        }

        public bool DescendsFrom(int descendantUnitId, int ancestorUnitId)
        {
            var unit = _orgUnitRepository.GetByKey(descendantUnitId);
            if (unit == null)
            {
                throw new ArgumentException($"Invalid org unit id:{descendantUnitId}");
            }

            return unit.SearchAncestry(ancestor => ancestor.Id == ancestorUnitId).HasValue;
        }

        public void Delete(int id, int organizationId)
        {
            using var transaction = _transactionManager.Begin();

            var organization = _organizationRepository.GetByKey(organizationId);
            var result = organization.RemoveOrganizationUnit(id);
            if (result.Failed)
                throw new ArgumentException(result.Error.Message.GetValueOrDefault());

            var orgUnit = result.Value;

            // Remove OrgUnit from ItSystemUsages
            var itSystemUsageOrgUnitUsages = _itSystemUsageOrgUnitUsageRepository.Get(x => x.OrganizationUnitId == id);
            foreach (var itSystemUsage in itSystemUsageOrgUnitUsages)
            {
                if (itSystemUsage.ResponsibleItSystemUsage != null)
                {
                    throw new ArgumentException($"OrganizationUnit is ResponsibleOrgUnit for ItSystemUsage: {itSystemUsage.ItSystemUsageId}");
                }

                _itSystemUsageOrgUnitUsageRepository.Delete(itSystemUsage);

            }

            // attach children to parent of this instance to avoid orphans
            // parent id will never be null because users aren't allowed to delete the root node
            foreach (var child in orgUnit.Children)
            {
                child.ParentId = orgUnit.ParentId;
            }

            _orgUnitRepository.DeleteWithReferencePreload(orgUnit);
            _itSystemUsageOrgUnitUsageRepository.Save();
            _orgUnitRepository.Save();

            transaction.Commit();
        }

        public IQueryable<OrganizationUnit> GetOrganizationUnits(Organization organization)
        {
            return _orgUnitRepository.AsQueryable().ByOrganizationId(organization.Id);
        }

        public Maybe<OrganizationUnit> GetOrganizationUnit(Guid uuid)
        {
            return _orgUnitRepository.AsQueryable().ByUuid(uuid).FromNullable();
        }
    }
}
