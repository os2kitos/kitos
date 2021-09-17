using System;
using Core.ApplicationServices.Model.Shared;

namespace Core.ApplicationServices.Model.Contracts.Write
{
    public class ItContractAgreementPeriodModificationParameters
    {
        public OptionalValueChange<int?> DurationYears { get; set; } = OptionalValueChange<int?>.None;
        public OptionalValueChange<int?> DurationMonths { get; set; } = OptionalValueChange<int?>.None;
        public OptionalValueChange<bool> IsContinuous { get; set; } = OptionalValueChange<bool>.None;
        public OptionalValueChange<Guid?> ExtensionOptionsUuid { get; set; } = OptionalValueChange<Guid?>.None;
        public OptionalValueChange<int> ExtensionOptionsUsed { get; set; } = OptionalValueChange<int>.None;
        public OptionalValueChange<DateTime?> IrrevocableUntil { get; set; } = OptionalValueChange<DateTime?>.None;
    }
}
