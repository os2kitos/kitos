using Core.ApplicationServices.Model.Shared;

namespace Core.ApplicationServices.Model.Organizations.Write.MasterDataRoles
{
    public class OrganizationMasterDataRoleUpdateParameters
    {
        public OptionalValueChange<string> Name { get; set; }
        public OptionalValueChange<string> Email { get; set; }
    }
}
