using System.Collections.Generic;
using Core.ApplicationServices.Model.Shared;

namespace Core.ApplicationServices.Model.Contracts.Write
{
    public class ItContractPaymentDataModificationParameters
    {
        public OptionalValueChange<IEnumerable<ItContractPayment>> ExternalPayments { get; set; } = OptionalValueChange<IEnumerable<ItContractPayment>>.None;
        public OptionalValueChange<IEnumerable<ItContractPayment>> InternalPayments { get; set; } = OptionalValueChange<IEnumerable<ItContractPayment>>.None;
    }
}
