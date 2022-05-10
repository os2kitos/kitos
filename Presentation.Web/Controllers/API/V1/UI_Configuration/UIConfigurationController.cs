using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V1;
using Presentation.Web.Models.API.V1.UI_Configuration;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.API.V1.UI_Configuration
{
    [InternalApi]
    [RoutePrefix("api/v1/organizations/{organizationId}/ui-config/modules/{module}")]
    public class UIConfigurationController : BaseApiController
    {
        [HttpGet]
        [Route]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<CustomizedUINodeDTO>))]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage Get(int organizationId, string module)
        {
            return Ok();
        }

        [HttpPut]
        [Route]
        [SwaggerResponse(HttpStatusCode.NoContent)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage Put(int organizationId, string module, [FromBody] List<CustomizedUINodeDTO> dtos)
        {
            return NoContent();
        }
    }
}