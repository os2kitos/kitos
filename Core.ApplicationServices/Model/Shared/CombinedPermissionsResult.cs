using Core.ApplicationServices.Authorization;

namespace Core.ApplicationServices.Model.Shared
{
    public class CombinedPermissionsResult
    {
        public ResourcePermissionsResult BasePermissions { get; }
        public ModuleFieldsPermissionsResult FieldPermissions { get; set; }

        public CombinedPermissionsResult(ResourcePermissionsResult basePermissions, ModuleFieldsPermissionsResult fieldPermissions)
        {
            BasePermissions = basePermissions;
            FieldPermissions = fieldPermissions;
        }
    }
}
