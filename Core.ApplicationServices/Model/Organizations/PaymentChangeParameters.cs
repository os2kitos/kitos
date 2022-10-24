using Core.DomainModel.ItContract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public int ItContractId { get; set; }
        public IEnumerable<int> InternalPayments { get; set; }
        public IEnumerable<int> ExternalPayments { get; set; }
    }
}
