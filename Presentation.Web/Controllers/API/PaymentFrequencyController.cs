using Core.DomainModel.ItContract;
using Core.DomainServices;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    public class PaymentFrequencyController : GenericOptionApiController<PaymentFreqency, ItContract, OptionDTO>
    {
        public PaymentFrequencyController(IGenericRepository<PaymentFreqency> repository) 
            : base(repository)
        {
        }
    }
}