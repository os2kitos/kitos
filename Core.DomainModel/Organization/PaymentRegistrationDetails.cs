using System.Collections.Generic;
using Core.DomainModel.ItContract;

namespace Core.DomainModel.Organization
{
    public class PaymentRegistrationDetails
    {
        public PaymentRegistrationDetails(ItContract.ItContract itContract, IEnumerable<EconomyStream> internalPayments, IEnumerable<EconomyStream> externalPayments)
        {
            ItContract = itContract;
            InternalPayments = internalPayments;
            ExternalPayments = externalPayments;
        }

        public ItContract.ItContract ItContract { get; }
        public IEnumerable<EconomyStream> InternalPayments { get; }
        public IEnumerable<EconomyStream> ExternalPayments { get; }
    }
}
