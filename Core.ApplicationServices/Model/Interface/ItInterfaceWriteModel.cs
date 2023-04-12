using System;
using System.Collections.Generic;
using Core.ApplicationServices.Model.Shared;
using Core.DomainModel;

namespace Core.ApplicationServices.Model.Interface
{
    public class ItInterfaceWriteModel : ItInterfaceWriteModelParametersBase
    {
        public OptionalValueChange<bool> Deactivated { get; set; } = OptionalValueChange<bool>.None;
        public OptionalValueChange<AccessModifier> Scope { get; set; } = OptionalValueChange<AccessModifier>.None;
        public OptionalValueChange<Guid?> InterfaceTypeUuid { get; set; } = OptionalValueChange<Guid?>.None;
        public OptionalValueChange<string> Note { get; set; } = OptionalValueChange<string>.None;
        public OptionalValueChange<IReadOnlyList<ItInterfaceDataWriteModel>> Data { get; set; } = OptionalValueChange<IReadOnlyList<ItInterfaceDataWriteModel>>.None;
    }
}
