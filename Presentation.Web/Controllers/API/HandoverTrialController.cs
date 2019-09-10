using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Core.DomainModel.ItContract;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.API
{
    [PublicApi]
    public class HandoverTrialController : GenericContextAwareApiController<HandoverTrial, HandoverTrialDTO>
    {
        public HandoverTrialController(IGenericRepository<HandoverTrial> repository)
            : base(repository)
        {
        }

        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<IEnumerable<HandoverTrialDTO>>))]
        public HttpResponseMessage GetByContractid(int id, bool? byContract)
        {
            var query = Repository.Get(x => x.ItContractId == id);
            var dtos = Map(query);
            return Ok(dtos);
        }
    }
}
