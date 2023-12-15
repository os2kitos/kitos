using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Shared.Write;
using Core.ApplicationServices.Model.Shared;
using System.Collections.Generic;
using System;

namespace Core.ApplicationServices.Model.System
{
    public abstract class SharedSystemUpdateParameters
    {
        public OptionalValueChange<string> Name { get; set; } = OptionalValueChange<string>.None;
        public OptionalValueChange<Guid?> ExternalUuid { get; set; } = OptionalValueChange<Guid?>.None;
        public OptionalValueChange<Guid?> ParentSystemUuid { get; set; } = OptionalValueChange<Guid?>.None;
        public OptionalValueChange<string> FormerName { get; set; } = OptionalValueChange<string>.None;
        public OptionalValueChange<string> Description { get; set; } = OptionalValueChange<string>.None;
        public Maybe<IEnumerable<UpdatedExternalReferenceProperties>> ExternalReferences { get; set; } = Maybe<IEnumerable<UpdatedExternalReferenceProperties>>.None;
        public OptionalValueChange<Guid?> BusinessTypeUuid { get; set; } = OptionalValueChange<Guid?>.None;
        public OptionalValueChange<IEnumerable<Guid>> TaskRefUuids { get; set; } = OptionalValueChange<IEnumerable<Guid>>.None;
    }
}
