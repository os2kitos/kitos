using Core.ApplicationServices.Model.Shared;

namespace Core.ApplicationServices.Model.Organizations.Write.MasterDataRoles;

public class DataResponsibleUpdateParameters: OrganizationMasterDataRoleUpdateParameters
{
    public OptionalValueChange<string> Cvr { get; set; }
    public OptionalValueChange<string> Phone { get; set; }
    public OptionalValueChange<string> Address { get; set; }
}