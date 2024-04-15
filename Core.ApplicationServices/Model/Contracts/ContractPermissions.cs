using Core.ApplicationServices.Authorization;

namespace Core.ApplicationServices.Model.Contracts
{
    public class ContractPermissions
    {
        public ResourcePermissionsResult BasePermissions { get; }

        public ContractPermissions(ResourcePermissionsResult basePermissions)
        {
            BasePermissions = basePermissions;
        }
    }
}
