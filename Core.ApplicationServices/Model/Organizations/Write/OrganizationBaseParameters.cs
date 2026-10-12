using System;
using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Shared;

namespace Core.ApplicationServices.Model.Organizations.Write
{
    public class OrganizationBaseParameters : OrganizationCvrUpdateParameter
    {
        public required OptionalValueChange<Maybe<string>> Name { get; set; }
        public required OptionalValueChange<int> TypeId { get; set; }
        public required OptionalValueChange<Guid?> ForeignCountryCodeUuid { get; set; }
        public required OptionalValueChange<bool> IsSupplier { get; set; }
    }
}
