using System.Web.Http.Description;
using Core.ApplicationServices;
using Core.DomainModel.ItContract;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class PaymentModelTypesController : BaseOptionController<PaymentModelType, ItContract>
    {
        public PaymentModelTypesController(IGenericRepository<PaymentModelType> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
        }
    }
}