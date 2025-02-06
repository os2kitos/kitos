using Core.ApplicationServices.Authorization;

namespace Core.ApplicationServices.Model.GDPR
{
    public class DataProcessingRegistrationPermissions
    {
        public ResourcePermissionsResult BasePermissions { get; }
        public DataProcessingRegistrationPermissions(ResourcePermissionsResult basePermissions)
        {
            BasePermissions = basePermissions;
        }

    }
}
