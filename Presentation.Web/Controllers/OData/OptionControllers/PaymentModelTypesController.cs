using Core.ApplicationServices;
using Core.DomainModel.ItContract;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    public class PaymentModelTypesController : BaseOptionController<PaymentModelType, ItContract>
    {
        public PaymentModelTypesController(IGenericRepository<PaymentModelType> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
        }
    }
}