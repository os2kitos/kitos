using Core.DomainModel.ItContract;
using Core.DomainServices;

namespace UI.MVC4.Controllers
{
    public class PaymentModelController : GenericApiController<PaymentModel, int>
    {
        public PaymentModelController(IGenericRepository<PaymentModel> repository) 
            : base(repository)
        {
        }
    }
}