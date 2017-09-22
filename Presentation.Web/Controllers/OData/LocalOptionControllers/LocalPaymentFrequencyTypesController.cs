using Core.ApplicationServices;
using Core.DomainModel.ItContract;
using Core.DomainModel.LocalOptions;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData.LocalOptionControllers
{
    public class LocalPaymentFrequencyTypesController : LocalOptionBaseController<LocalPaymentFreqencyType, ItContract, PaymentFreqencyType>
    {
        public LocalPaymentFrequencyTypesController(IGenericRepository<LocalPaymentFreqencyType> repository, IAuthenticationService authService, IGenericRepository<PaymentFreqencyType> optionsRepository)
            : base(repository, authService, optionsRepository)
        {
        }
    }
}
