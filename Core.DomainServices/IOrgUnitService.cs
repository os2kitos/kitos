using System.Collections.Generic;
using System.Linq;
using Core.DomainModel;

namespace Core.DomainServices
{
    public interface IOrgUnitService
    {
        ICollection<OrganizationUnit> GetByUser(User user);
        OrganizationUnit GetRoot(OrganizationUnit unit);

        ICollection<OrganizationUnit> GetSubTree(int orgUnitId);
        ICollection<OrganizationUnit> GetSubTree(OrganizationUnit unit);

        bool IsAncestorOf(OrganizationUnit unitA, OrganizationUnit unitB);
        bool IsAncestorOf(int unitIdA, int unitIdB);

        bool HasWriteAccess(User user, int orgUnitId);
        bool HasWriteAccess(User user, OrganizationUnit unit);
    }
}