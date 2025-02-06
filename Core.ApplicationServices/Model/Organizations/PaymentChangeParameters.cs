using System.Collections.Generic;

namespace Core.ApplicationServices.Model.Organizations
{
    public class PaymentChangeParameters
    {
        public PaymentChangeParameters(int contractId, IEnumerable<int> internalPaymentIds, IEnumerable<int> externalPaymentIds)
        {
            ItContractId = contractId;
            InternalPayments = internalPaymentIds;
            ExternalPayments = externalPaymentIds;
        }

        public int ItContractId { get; }
        public IEnumerable<int> InternalPayments { get; }
        public IEnumerable<int> ExternalPayments { get; }
    }
}
