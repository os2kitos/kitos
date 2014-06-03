using System.Net.Http;
using System.Web.Http;
using Core.DomainModel.ItContract;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class PaymentMilestoneController : GenericApiController<PaymentMilestone, PaymentMilestoneDTO>
    {
        public PaymentMilestoneController(IGenericRepository<PaymentMilestone> repository) 
            : base(repository)
        {
        }

        public HttpResponseMessage GetByContractId(int id, [FromUri] bool? contract)
        {
            var items = Repository.Get(x => x.ItContractId == id);

            return Ok(Map(items));
        }
    }
}