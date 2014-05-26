using Core.DomainModel.ItContract;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class PaymentMilestoneController : GenericApiController<PaymentMilestone, int, PaymentMilestoneDTO>
    {
        public PaymentMilestoneController(IGenericRepository<PaymentMilestone> repository) 
            : base(repository)
        {
        }
    }
}