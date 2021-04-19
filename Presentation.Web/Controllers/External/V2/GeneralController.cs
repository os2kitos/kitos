using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models;
using Presentation.Web.Models.External;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.External.V2
{
    [PublicApi]
    [RoutePrefix("api/v2")]
    public class GeneralController: ExternalBaseController
    {
        [HttpGet]
        [Route("business-types")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<ApiReturnDTO<ExternalIdentityNamePairDTO>>))]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        public HttpResponseMessage GetBusinessTypes()
        {
            return Ok();
        }

        [HttpGet]
        [Route("business-type/{uuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<ExternalIdentityNamePairDTO>))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage GetBusinessType(Guid uuid)
        {
            return Ok();
        }
    }
}