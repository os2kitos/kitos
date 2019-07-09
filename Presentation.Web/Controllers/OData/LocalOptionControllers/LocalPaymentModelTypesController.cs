using System.Web.Http.Description;
using Core.ApplicationServices;
using Core.DomainModel.ItContract;
using Core.DomainModel.LocalOptions;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData.LocalOptionControllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class LocalPaymentModelTypesController : LocalOptionBaseController<LocalPaymentModelType, ItContract, PaymentModelType>
    {
        public LocalPaymentModelTypesController(IGenericRepository<LocalPaymentModelType> repository, IAuthenticationService authService, IGenericRepository<PaymentModelType> optionsRepository)
            : base(repository, authService, optionsRepository)
        {
        }
    }
}
