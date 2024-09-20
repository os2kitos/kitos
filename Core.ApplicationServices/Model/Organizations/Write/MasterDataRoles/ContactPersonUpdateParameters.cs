using Core.ApplicationServices.Model.Shared;

namespace Core.ApplicationServices.Model.Organizations.Write.MasterDataRoles;

public class ContactPersonUpdateParameters
{
    public OptionalValueChange<string> Name { get; set; }
    public OptionalValueChange<string> LastName { get; set; }
    public OptionalValueChange<string> PhoneNumber { get; set; }
    public OptionalValueChange<string> Email { get; set; }
}