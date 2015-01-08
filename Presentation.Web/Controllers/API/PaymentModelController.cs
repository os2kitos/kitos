using Core.DomainModel.ItContract;
using Core.DomainServices;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    public class PaymentModelController : GenericOptionApiController<PaymentModel, ItContract, OptionDTO>
    {
        public PaymentModelController(IGenericRepository<PaymentModel> repository) 
            : base(repository)
        {
        }
    }
}
