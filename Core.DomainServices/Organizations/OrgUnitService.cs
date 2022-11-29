using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.DomainModel.Extensions;
using Core.DomainModel.Organization;
using Core.DomainServices.Extensions;


namespace Core.DomainServices.Organizations
{
    public class OrgUnitService : IOrgUnitService
    {
        private readonly IGenericRepository<OrganizationUnit> _orgUnitRepository;

        public OrgUnitService(IGenericRepository<OrganizationUnit> orgUnitRepository)
        {
            _orgUnitRepository = orgUnitRepository;
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
