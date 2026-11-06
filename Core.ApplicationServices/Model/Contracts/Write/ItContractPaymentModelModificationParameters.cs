using System;
using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Shared;
using System.Collections.Generic;
using Core.DomainModel.ItContract;

namespace Core.ApplicationServices.Model.Contracts.Write
{
    public class ItContractPaymentModelModificationParameters
    {
        public OptionalValueChange<InvoiceFrequencyOptions?> InvoiceFrequency { get; set; } = OptionalValueChange<InvoiceFrequencyOptions?>.None;
        public OptionalValueChange<IEnumerable<ItContractInvoiceDate>> InvoiceDates { get; set; } = OptionalValueChange<IEnumerable<ItContractInvoiceDate>>.None;
        public OptionalValueChange<Maybe<DateTime>> OperationsRemunerationStartedAt { get; set; } = OptionalValueChange<Maybe<DateTime>>.None;
        public OptionalValueChange<Guid?> PaymentFrequencyUuid { get; set; } = OptionalValueChange<Guid?>.None;
        public OptionalValueChange<Guid?> PaymentModelUuid { get; set; } = OptionalValueChange<Guid?>.None;
        public OptionalValueChange<Guid?> PriceRegulationUuid { get; set; } = OptionalValueChange<Guid?>.None;
    }
}
