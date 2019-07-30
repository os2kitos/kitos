using System.Net.Http;
using System.Web.Http;
using Core.DomainModel.ItContract;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    [InternalApi]
    public class PaymentMilestoneController : GenericContextAwareApiController<PaymentMilestone, PaymentMilestoneDTO>
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
