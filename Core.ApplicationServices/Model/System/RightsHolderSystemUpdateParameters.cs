using System;
using System.Collections.Generic;
using Core.ApplicationServices.Model.Shared;

namespace Core.ApplicationServices.Model.System
{
    public class RightsHolderSystemUpdateParameters
    {
        public OptionalValueChange<string> Name { get; set; } = OptionalValueChange<string>.None;
        public OptionalValueChange<Guid?> ParentSystemUuid { get; set; } = OptionalValueChange<Guid?>.None;
        public OptionalValueChange<string> FormerName { get; set; } = OptionalValueChange<string>.None;
        public OptionalValueChange<string> Description { get; set; } = OptionalValueChange<string>.None;
        public OptionalValueChange<string> UrlReference { get; set; } = OptionalValueChange<string>.None; //TODO: To proper model
        public OptionalValueChange<Guid?> BusinessTypeUuid { get; set; } = OptionalValueChange<Guid?>.None;
        public OptionalValueChange<IEnumerable<Guid>> TaskRefUuids { get; set; } = OptionalValueChange<IEnumerable<Guid>>.None;
    }
}
