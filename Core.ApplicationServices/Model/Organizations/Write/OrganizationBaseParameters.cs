using System;
using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Shared;

namespace Core.ApplicationServices.Model.Organizations.Write
{
    public class OrganizationBaseParameters : OrganizationCvrUpdateParameter
    {
        public OptionalValueChange<Maybe<string>> Name { get; set; }
        public OptionalValueChange<int> TypeId { get; set; }
        public OptionalValueChange<Guid?> ForeignCountryCodeUuid { get; set; }
    }
}
