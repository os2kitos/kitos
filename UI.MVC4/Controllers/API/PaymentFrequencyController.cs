using Core.DomainModel.ItContract;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class PaymentFrequencyController : GenericOptionApiController<PaymentFreqency, ItContract, OptionDTO>
    {
        public PaymentFrequencyController(IGenericRepository<PaymentFreqency> repository) 
            : base(repository)
        {
        }
    }
}