using System;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Core.DomainModel.ItProject;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    [PublicApi]
    public class ItProjectStatusController : GenericContextAwareApiController<ItProjectStatus, ItProjectStatusDTO>
    {
        public ItProjectStatusController(IGenericRepository<ItProjectStatus> repository) 
            : base(repository)
        {
        }

        public HttpResponseMessage GetByProject(int id, [FromUri] bool? project, [FromUri] PagingModel<ItProjectStatus> paging)
        {
            try
            {
                var query = Repository.AsQueryable().Where(x => x.AssociatedItProjectId == id);
                var pagedQuery = Page(query, paging);

                if (!pagedQuery.Any()) return NotFound();

                return Ok(Map(pagedQuery));
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }
    }
}
