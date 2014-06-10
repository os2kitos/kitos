using System.Linq;
using System.Net.Http;
using Core.DomainModel;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class TaskRefController : GenericApiController<TaskRef, TaskRefDTO>
    {
        public TaskRefController(IGenericRepository<TaskRef> repository) 
            : base(repository)
        {
        }

        public HttpResponseMessage GetByOrgUnit(int orgUnitId, int skip = 0, int take = 100)
        {
            var items = Repository.AsQueryable().Where(x => x.OwnedByOrganizationUnitId == orgUnitId || x.IsPublic).OrderBy(x => x.Id).Skip(skip).Take(take);
            return Ok(Map(items));
        }

        public HttpResponseMessage GetByOrg(int orgId, int skip = 0, int take = 100)
        {
            var items = Repository.AsQueryable().Where(x => x.OwnedByOrganizationUnit.OrganizationId == orgId || x.IsPublic).OrderBy(x => x.Id).Skip(skip).Take(take);
            return Ok(Map(items));
        }
    }
}