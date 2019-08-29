using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Core.DomainModel.ItContract;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.API
{
    [PublicApi]
    public class PaymentMilestoneController : GenericContextAwareApiController<PaymentMilestone, PaymentMilestoneDTO>
    {
        public PaymentMilestoneController(IGenericRepository<PaymentMilestone> repository) 
            : base(repository)
        {
        }

        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<IEnumerable<PaymentMilestoneDTO>>))]
        public HttpResponseMessage GetByContractId(int id, [FromUri] bool? contract)
        {
            var items = Repository.Get(x => x.ItContractId == id);

            return Ok(Map(items));
        }
    }
}
