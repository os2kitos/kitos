using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;
using Core.DomainModel.Organization;


namespace Core.DomainServices
{
    public interface IOrgUnitService
    {
        ICollection<OrganizationUnit> GetSubTree(int orgUnitId);

        bool DescendsFrom(int descendantUnitId, int ancestorUnitId);
        IQueryable<OrganizationUnit> GetOrganizationUnits(Organization organization);
        Maybe<OrganizationUnit> GetOrganizationUnit(Guid uuid);
    }
}
