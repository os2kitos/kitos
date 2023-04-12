using Core.ApplicationServices.Authorization;
using System.Collections.Generic;

namespace Core.ApplicationServices.Model.Interface
{
    public class ItInterfacePermissions
    {
        public ResourcePermissionsResult BasePermissions { get; }
        public IEnumerable<ItInterfaceDeletionConflict> DeletionConflicts { get; }

        public ItInterfacePermissions(ResourcePermissionsResult basePermissions, IEnumerable<ItInterfaceDeletionConflict> deletionConflicts)
        {
            BasePermissions = basePermissions;
            DeletionConflicts = deletionConflicts;
        }
    }
}
