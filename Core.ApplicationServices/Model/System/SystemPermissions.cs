using System.Collections.Generic;
using Core.ApplicationServices.Authorization;

namespace Core.ApplicationServices.Model.System
{
    public class SystemPermissions
    {
        public ResourcePermissionsResult BasePermissions { get; }
        public IEnumerable<SystemDeletionConflict> DeletionConflicts { get; }

        public SystemPermissions(ResourcePermissionsResult basePermissions, IEnumerable<SystemDeletionConflict> deletionConflicts)
        {
            BasePermissions = basePermissions;
            DeletionConflicts = deletionConflicts;
        }
    }
}
