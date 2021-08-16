using System;
using System.Collections.Generic;
using System.Linq;
using Core.DomainModel.Organization;
using Infrastructure.Services.Types;

namespace Core.DomainServices
{
    public interface IOrgUnitService
    {
        OrganizationUnit GetRoot(OrganizationUnit unit);

        ICollection<OrganizationUnit> GetSubTree(int orgUnitId);
        ICollection<OrganizationUnit> GetSubTree(OrganizationUnit unit);

        bool IsAncestorOf(OrganizationUnit unitA, OrganizationUnit unitB);
        bool IsAncestorOf(int unitIdA, int unitIdB);
        void Delete(int id);
        IQueryable<OrganizationUnit> GetOrganizationUnits(Organization organization);
        Maybe<OrganizationUnit> GetOrganizationUnit(Guid uuid);
    }
}
