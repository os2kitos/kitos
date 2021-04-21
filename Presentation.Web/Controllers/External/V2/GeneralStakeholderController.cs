using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Presentation.Web.Models.External.V2;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.External.V2
{
    public partial class GeneralController
    {
        [HttpGet]
        [Route("it-systems")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<ItSystemStakeholderResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public HttpResponseMessage GetItSystems(Guid? rightsholderUuid, Guid? businessTypeUuid, string? kleNumber, Guid? kleUuid, int? numberOfUsers, int? page = 0, int? pageSize = 100)
        {
            return Ok(new List<ItSystemStakeholderResponseDTO>());
        }

        [HttpGet]
        [Route("it-systems2/{uuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ItSystemStakeholderResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage GetItSystem(Guid uuid)
        {
            return Ok(new ItSystemStakeholderResponseDTO());
        }

        [HttpGet]
        [Route("it-interfaces")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<ItInterfaceResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public HttpResponseMessage GetItInterface(Guid? exposedBySystemUuid, int? page = 0, int? pageSize = 100)
        {
            return Ok(new List<ItInterfaceResponseDTO>());
        }

        [HttpGet]
        [Route("it-interfaces/{uuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ItInterfaceResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage GetItInterface(Guid uuid)
        {
            return Ok(new ItInterfaceResponseDTO());
        }
    }
}