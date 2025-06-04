using System;
using System.Collections.Generic;
using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Shared;

namespace Core.ApplicationServices.Model.Contracts.Write
{
    public class ItContractGeneralDataModificationParameters
    {
        public OptionalValueChange<string> ContractId { get; set; } = OptionalValueChange<string>.None;
        public OptionalValueChange<Guid?> ContractTypeUuid { get; set; } = OptionalValueChange<Guid?>.None;
        public OptionalValueChange<Guid?> ContractTemplateUuid { get; set; } = OptionalValueChange<Guid?>.None;
        public OptionalValueChange<IEnumerable<Guid>> AgreementElementUuids { get; set; } = OptionalValueChange<IEnumerable<Guid>>.None;
        public OptionalValueChange<string> Notes { get; set; } = OptionalValueChange<string>.None;
        public OptionalValueChange<Maybe<bool>> EnforceValid { get; set; } = OptionalValueChange<Maybe<bool>>.None;
        public OptionalValueChange<Maybe<DateTime>> ValidFrom { get; set; } = OptionalValueChange<Maybe<DateTime>>.None;
        public OptionalValueChange<Maybe<DateTime>> ValidTo { get; set; } = OptionalValueChange<Maybe<DateTime>>.None;
        public OptionalValueChange<Maybe<bool>> RequireValidParent { get; set; } = OptionalValueChange<Maybe<bool>>.None;
        public OptionalValueChange<Guid?> CriticalityUuid { get; set; } = OptionalValueChange<Guid?>.None;
    }
}
