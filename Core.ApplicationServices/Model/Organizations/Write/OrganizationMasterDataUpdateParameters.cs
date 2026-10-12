using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Shared;

namespace Core.ApplicationServices.Model.Organizations.Write
{
    public class OrganizationMasterDataUpdateParameters: OrganizationCvrUpdateParameter
    {
        public required OptionalValueChange<Maybe<string>> Phone { get; set; }
        public required OptionalValueChange<Maybe<string>> Address { get; set; }
        public required OptionalValueChange<Maybe<string>> Email { get; set; }
    }
}
