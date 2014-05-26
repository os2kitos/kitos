using Core.DomainModel.ItContract;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class PaymentFreqencyController : GenericOptionApiController<PaymentFreqency, ItContract, OptionDTO>
    {
        public PaymentFreqencyController(IGenericRepository<PaymentFreqency> repository) 
            : base(repository)
        {
        }
    }
}