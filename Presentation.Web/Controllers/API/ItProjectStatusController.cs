using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Core.DomainModel.ItProject;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.API
{
    [PublicApi]
    [ControllerEvaluationCompleted]
    public class ItProjectStatusController : GenericContextAwareApiController<ItProjectStatus, ItProjectStatusDTO>
    {
        public ItProjectStatusController(IGenericRepository<ItProjectStatus> repository) 
            : base(repository)
        {
        }

        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<IEnumerable<ItProjectStatusDTO>>))]
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
