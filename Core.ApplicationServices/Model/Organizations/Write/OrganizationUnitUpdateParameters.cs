using Core.DomainModel.Organization;
using System;
using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Shared;

namespace Core.ApplicationServices.Model.Organizations.Write
{
    public class OrganizationUnitUpdateParameters
    {
        public OptionalValueChange<string> Name { get; set; }
        public OptionalValueChange<OrganizationUnitOrigin> Origin { get; set; }
        public OptionalValueChange<Maybe<Guid>> ParentUuid { get; set; }
        public OptionalValueChange<Maybe<long>> Ean { get; set; }
        public OptionalValueChange<Maybe<string>> LocalId { get; set; }
    }
}
