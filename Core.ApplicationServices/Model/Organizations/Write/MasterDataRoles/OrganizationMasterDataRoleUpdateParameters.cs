using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Shared;

namespace Core.ApplicationServices.Model.Organizations.Write.MasterDataRoles
{
    public class OrganizationMasterDataRoleUpdateParameters
    {
        public OptionalValueChange<Maybe<string>> Name { get; set; }
        public OptionalValueChange<Maybe<string>> Email { get; set; }
    }
}
