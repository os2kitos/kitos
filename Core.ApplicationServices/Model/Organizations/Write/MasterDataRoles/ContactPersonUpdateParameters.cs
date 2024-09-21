using Core.ApplicationServices.Model.Shared;

namespace Core.ApplicationServices.Model.Organizations.Write.MasterDataRoles;

public class ContactPersonUpdateParameters: OrganizationMasterDataRoleUpdateParameters
{
    public OptionalValueChange<string> LastName { get; set; }
    public OptionalValueChange<string> PhoneNumber { get; set; }
}