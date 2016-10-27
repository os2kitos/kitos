using Core.ApplicationServices;
using Core.DomainModel.ItContract;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    public class PaymentFrequencyTypesController : BaseRoleController<PaymentFreqencyType, ItContract>
    {
        public PaymentFrequencyTypesController(IGenericRepository<PaymentFreqencyType> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
        }
    }
}