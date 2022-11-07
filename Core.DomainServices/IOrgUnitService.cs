using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;
using Core.DomainModel.Organization;


namespace Core.DomainServices
{
    public interface IOrgUnitService
    {
        OrganizationUnit GetRoot(OrganizationUnit unit);

        ICollection<OrganizationUnit> GetSubTree(int orgUnitId);

        bool DescendsFrom(int descendantUnitId, int ancestorUnitId);
        Maybe<OperationError> Delete(Guid organizationUuid, Guid unitUuid);
        IQueryable<OrganizationUnit> GetOrganizationUnits(Organization organization);
        Maybe<OrganizationUnit> GetOrganizationUnit(Guid uuid);
    }
}
