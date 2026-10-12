using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Shared;

namespace Core.ApplicationServices.Model.Organizations.Write
{
    public class OrganizationCvrUpdateParameter
    {
        public required OptionalValueChange<Maybe<string>> Cvr { get; set; }
    }
}
