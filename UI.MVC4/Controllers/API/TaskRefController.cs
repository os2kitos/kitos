using System.Net.Http;
using Core.DomainModel;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class TaskRefController : GenericApiController<TaskRef, int, TaskRefDTO>
    {
        public TaskRefController(IGenericRepository<TaskRef> repository) 
            : base(repository)
        {
        }

        public HttpResponseMessage GetByOrgUnit(int orgUnitId)
        {
            var items = Repository.Get(x => x.OwnedByOrganizationUnitId == orgUnitId || x.IsPublic);
            return Ok(Map(items));
        }
    }
}