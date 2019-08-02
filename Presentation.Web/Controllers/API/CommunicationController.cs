using System.Net.Http;
using System.Web.Http;
using Core.DomainModel.ItProject;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    [PublicApi]
    public class CommunicationController : GenericContextAwareApiController<Communication, CommunicationDTO>
    {
        public CommunicationController(IGenericRepository<Communication> repository) 
            : base(repository)
        {
        }

        public HttpResponseMessage GetSingle(int id, [FromUri] bool project)
        {
            var item = Repository.Get(x => x.ItProjectId == id);
            
            if (item == null)
                return NotFound();

            return Ok(Map(item));
        }
    }
}
