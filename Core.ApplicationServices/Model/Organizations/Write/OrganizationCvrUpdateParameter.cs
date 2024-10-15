using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Shared;

namespace Core.ApplicationServices.Model.Organizations.Write
{
    public class OrganizationCvrUpdateParameter
    {
        public OptionalValueChange<Maybe<string>> Cvr { get; set; }
    }
}
