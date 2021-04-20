using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.External;
using Swashbuckle.Examples;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.External.V2
{
    public partial class RightsHolderController
    {
        [HttpPost]
        [Route("it-interfaces")]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(HttpStatusCode.Created)]
        [SwaggerResponseHeader(HttpStatusCode.Created, "Location", "string", "Location of the newly created it-interface")]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.Conflict)]
        public HttpResponseMessage PostItInterface([FromBody] ExternalItInterfaceDTO itInterfaceDTO)
        {
            return Created(new Uri(Request.RequestUri + "/" + itInterfaceDTO.Uuid));
        }

        [HttpGet]
        [Route("it-interfaces")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<ExternalItInterfaceDTO>))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public HttpResponseMessage GetItInterface(int? page = 0, int? pageSize = 100)
        {
            return Ok(new List<ExternalItInterfaceDTO>());
        }

        [HttpGet]
        [Route("it-interfaces/{uuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ExternalItInterfaceDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage GetItInterface(Guid uuid)
        {
            return Ok(new ExternalItInterfaceDTO());
        }

        [HttpPatch]
        [Route("it-interfaces/{uuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ExternalItInterfaceDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage PutItInterface(Guid uuid)
        {
            return Ok(new ExternalItInterfaceDTO());
        }

        [HttpDelete]
        [Route("it-interfaces/{uuid}")]
        [SwaggerResponse(HttpStatusCode.NoContent)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage PutItInterface(Guid uuid, [FromBody] ExternalDeactivationReasonDTO deactivationReasonDTO)
        {
            return NoContent();
        }
    }
}