using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Shared;

namespace Core.ApplicationServices.Model.Organizations.Write
{
    public class OrganizationUpdateParameters
    {
        public OptionalValueChange<Maybe<string>> Cvr { get; set; }
        public OptionalValueChange<Maybe<string>> Name { get; set; }
    }
}
