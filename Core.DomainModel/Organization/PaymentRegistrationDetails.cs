using System.Collections.Generic;
using Core.DomainModel.ItContract;

namespace Core.DomainModel.Organization
{
    public class PaymentRegistrationDetails
    {
        public PaymentRegistrationDetails(int unitId, ItContract.ItContract itContract)
        {
            ItContract = itContract;
            InternalPayments = itContract.GetInternalPaymentsForUnit(unitId);
            ExternalPayments = itContract.GetExternalPaymentsForUnit(unitId);
        }

        public ItContract.ItContract ItContract { get; }
        public IEnumerable<EconomyStream> InternalPayments { get; }
        public IEnumerable<EconomyStream> ExternalPayments { get; }
    }
}
