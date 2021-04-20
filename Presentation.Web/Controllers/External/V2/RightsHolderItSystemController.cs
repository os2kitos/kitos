using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.External;
using Swashbuckle.Examples;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.External.V2
{
    [PublicApi]
    [RoutePrefix("api/v2/rightsholder")]
    public partial class RightsHolderController: ExternalBaseController
    {
        [HttpPost]
        [Route("it-systems")]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(HttpStatusCode.Created)]
        [SwaggerResponseHeader(HttpStatusCode.Created, "Location", "string", "Location of the newly created it-system")]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.Conflict)]
        public HttpResponseMessage PostItSystem([FromBody] ExternalItSystemDTO itSystemDTO)
        {
            return Created(new Uri(Request.RequestUri + "/" + itSystemDTO.Uuid));
        }

        [HttpGet]
        [Route("it-systems")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<ExternalItSystemDTO>))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public HttpResponseMessage GetItSystems(int? page = 0, int? pageSize = 100)
        {
            return Ok(new List<ExternalItSystemDTO>());
        }

        [HttpGet]
        [Route("it-systems/{uuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ExternalItSystemDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage GetItSystem(Guid uuid)
        {
            return Ok(new ExternalItSystemDTO());
        }

        [HttpPatch]
        [Route("it-systems/{uuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ExternalItSystemDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage PutItSystem(Guid uuid)
        {
            return Ok(new ExternalItSystemDTO());
        }

        [HttpDelete]
        [Route("it-systems/{uuid}")]
        [SwaggerResponse(HttpStatusCode.NoContent)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage PutItSystem(Guid uuid, [FromBody] ExternalDeactivationReasonDTO deactivationReasonDTO)
        {
            return NoContent();
        }

    }
}