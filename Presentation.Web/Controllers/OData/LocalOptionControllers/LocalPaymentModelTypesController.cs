using Core.ApplicationServices;
using Core.DomainModel.ItContract;
using Core.DomainModel.LocalOptions;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData.LocalOptionControllers
{
    public class LocalPaymentModelTypesController : LocalOptionBaseController<LocalPaymentModelType, ItContract, PaymentModelType>
    {
        public LocalPaymentModelTypesController(IGenericRepository<LocalPaymentModelType> repository, IAuthenticationService authService, IGenericRepository<PaymentModelType> optionsRepository)
            : base(repository, authService, optionsRepository)
        {
        }
    }
}
