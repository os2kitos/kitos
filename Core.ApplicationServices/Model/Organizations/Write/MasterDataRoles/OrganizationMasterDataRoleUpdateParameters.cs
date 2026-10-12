using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Shared;

namespace Core.ApplicationServices.Model.Organizations.Write.MasterDataRoles
{
    public class OrganizationMasterDataRoleUpdateParameters
    {
        public required OptionalValueChange<Maybe<string>> Name { get; set; }
        public required OptionalValueChange<Maybe<string>> Email { get; set; }
    }
}
