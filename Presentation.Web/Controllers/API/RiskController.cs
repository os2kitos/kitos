using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Core.DomainModel.ItProject;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.API
{
    [PublicApi]
    public class RiskController : GenericContextAwareApiController<Risk, RiskDTO>
    {
        public RiskController(IGenericRepository<Risk> repository) : base(repository)
        {
        }

        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<IEnumerable<RiskDTO>>))]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.InternalServerError)]
        public HttpResponseMessage GetByProject(bool? getByProject, int projectId)
        {
            try
            {
                var risks = Repository.Get(r => r.ItProjectId == projectId);

                return Ok(Map(risks));
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }
    }
}
