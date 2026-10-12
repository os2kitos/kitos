using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Shared;

namespace Core.ApplicationServices.Model.Organizations.Write.MasterDataRoles;

public class ContactPersonUpdateParameters: OrganizationMasterDataRoleUpdateParameters
{
    public required OptionalValueChange<Maybe<string>> LastName { get; set; }
    public required OptionalValueChange<Maybe<string>> PhoneNumber { get; set; }
}