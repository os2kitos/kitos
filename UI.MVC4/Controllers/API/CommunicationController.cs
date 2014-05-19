using System.Net.Http;
using System.Web.Http;
using Core.DomainModel.ItProject;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class CommunicationController : GenericApiController<Communication, int, CommunicationDTO>
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