using Core.ApplicationServices;
using Core.DomainModel.ItContract;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    public class PaymentFrequencyTypesController : BaseEntityController<PaymentFreqencyType>
    {
        public PaymentFrequencyTypesController(IGenericRepository<PaymentFreqencyType> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
        }
    }
}