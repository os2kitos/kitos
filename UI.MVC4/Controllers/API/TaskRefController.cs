using System.Linq;
using System.Net.Http;
using Core.DomainModel;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class TaskRefController : GenericApiController<TaskRef, TaskRefDTO, TaskRefDTO>
    {
        public TaskRefController(IGenericRepository<TaskRef> repository) 
            : base(repository)
        {
        }

        public HttpResponseMessage GetByOrgUnit(int orgUnitId)
        {
            var items =  GetAllQuery().Where(x => x.OwnedByOrganizationUnitId == orgUnitId || x.IsPublic);
            return Ok(Map(items));
        }

        public HttpResponseMessage GetByOrg(int orgId)
        {
            var items = GetAllQuery().Where(x => x.OwnedByOrganizationUnit.OrganizationId == orgId || x.IsPublic);
            return Ok(Map(items));
        }
    }
}