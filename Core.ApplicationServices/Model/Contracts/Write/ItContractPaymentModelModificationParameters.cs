using System;
using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Shared;

namespace Core.ApplicationServices.Model.Contracts.Write
{
    public class ItContractPaymentModelModificationParameters
    {
        public OptionalValueChange<Maybe<DateTime>> OperationsRemunerationStartedAt { get; set; } = OptionalValueChange<Maybe<DateTime>>.None;
        public OptionalValueChange<Guid?> PaymentFrequencyUuid { get; set; } = OptionalValueChange<Guid?>.None;
        public OptionalValueChange<Guid?> PaymentModelUuid { get; set; } = OptionalValueChange<Guid?>.None;
        public OptionalValueChange<Guid?> PriceRegulationUuid { get; set; } = OptionalValueChange<Guid?>.None;
    }
}