using Core.ApplicationServices.Model.Shared;

namespace Core.ApplicationServices.Model.Organizations.Write.MasterDataRoles;

public class DataResponsibleUpdateParameters
{
    public OptionalValueChange<string> Name { get; set; }
    public OptionalValueChange<string> Cvr { get; set; }
    public OptionalValueChange<string> Phone { get; set; }
    public OptionalValueChange<string> Address { get; set; }
    public OptionalValueChange<string> Email { get; set; }
    public OptionalValueChange<int> Id { get; set; }
}