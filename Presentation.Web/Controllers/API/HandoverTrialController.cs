using System.Net.Http;
using Core.DomainModel.ItContract;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    [InternalApi]
    public class HandoverTrialController : GenericContextAwareApiController<HandoverTrial, HandoverTrialDTO>
    {
        public HandoverTrialController(IGenericRepository<HandoverTrial> repository)
            : base(repository)
        {
        }

        public HttpResponseMessage GetByContractid(int id, bool? byContract)
        {
            var query = Repository.Get(x => x.ItContractId == id);
            var dtos = Map(query);
            return Ok(dtos);
        }
    }
}
