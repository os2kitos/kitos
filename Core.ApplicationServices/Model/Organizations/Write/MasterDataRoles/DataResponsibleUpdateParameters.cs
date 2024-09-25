using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Shared;

namespace Core.ApplicationServices.Model.Organizations.Write.MasterDataRoles;

public class DataResponsibleUpdateParameters: OrganizationMasterDataRoleUpdateParameters
{
    public OptionalValueChange<Maybe<string>> Cvr { get; set; }
    public OptionalValueChange<Maybe<string>> Phone { get; set; }
    public OptionalValueChange<Maybe<string>> Address { get; set; }
}