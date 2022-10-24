using System.Collections.Generic;
using Core.DomainModel.ItContract;

namespace Core.ApplicationServices.Model.Organizations
{
    public class PaymentRegistrationDetails
    {
        public PaymentRegistrationDetails(int unitId, ItContract itContract)
        {
            ItContract = itContract;
            InternalPayments = itContract.GetInternalPaymentsForUnit(unitId);
            ExternalPayments = itContract.GetExternalPaymentsForUnit(unitId);
        }

        public ItContract ItContract { get; set; }
        public IEnumerable<EconomyStream> InternalPayments { get; set; }
        public IEnumerable<EconomyStream> ExternalPayments { get; set; }
    }
}
