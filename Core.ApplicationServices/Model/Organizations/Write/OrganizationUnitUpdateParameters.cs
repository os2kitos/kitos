using Core.DomainModel.Organization;
using System;
using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Shared;

namespace Core.ApplicationServices.Model.Organizations.Write
{
    public class OrganizationUnitUpdateParameters
    {
        public required OptionalValueChange<string> Name { get; set; }
        public required OptionalValueChange<OrganizationUnitOrigin> Origin { get; set; }
        public required OptionalValueChange<Maybe<Guid>> ParentUuid { get; set; }
        public required OptionalValueChange<Maybe<long>> Ean { get; set; }
        public required OptionalValueChange<Maybe<string>> LocalId { get; set; }
    }
}
