using System.Collections.Generic;
using Core.ApplicationServices.Authorization;

namespace Core.ApplicationServices.Model.System
{
    public class SystemPermissions
    {
        public ResourcePermissionsResult BasePermissions { get; }
        public IEnumerable<SystemDeletionConflict> DeletionConflicts { get; }
        public bool ModifyVisibility { get; }

        public SystemPermissions(ResourcePermissionsResult basePermissions, IEnumerable<SystemDeletionConflict> deletionConflicts, bool modifyVisibility)
        {
            BasePermissions = basePermissions;
            DeletionConflicts = deletionConflicts;
            ModifyVisibility = modifyVisibility;
        }
    }
}
