using Core.DomainModel.ItContract;
using Core.DomainServices;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    public class PaymentModelController : GenericOptionApiController<PaymentModelType, ItContract, OptionDTO>
    {
        public PaymentModelController(IGenericRepository<PaymentModelType> repository)
            : base(repository)
        {
        }
    }
}
