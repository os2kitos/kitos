using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Web.Http;
using Core.ApplicationServices.SystemUsage.Migration;
using Presentation.Web.Controllers.API.V2.Common.Mapping;
using Presentation.Web.Controllers.API.V2.Internal.ItSystemUsages.Mapping;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.Internal.Response.ItSystemUsage;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;
using Presentation.Web.Models.API.V2.Response.Shared;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.API.V2.Internal.ItSystemUsages
{
    [RoutePrefix("api/v2/internal/it-system-usages")]
    public class ItSystemUsageMigrationV2Controller : InternalApiV2Controller
    {
        private readonly IItSystemUsageMigrationResponseMapper _responseMapper;
        private readonly IItSystemUsageMigrationServiceAdapter _adapter;

        public ItSystemUsageMigrationV2Controller(IItSystemUsageMigrationResponseMapper responseMapper,
            IItSystemUsageMigrationServiceAdapter adapter)
        {
            _responseMapper = responseMapper;
            _adapter = adapter;
        }

        [HttpGet]
        [Route("{usageUuid}/migration")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ItSystemUsageMigrationV2ResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult Get([Required][NonEmptyGuid] Guid usageUuid, [Required][NonEmptyGuid] Guid toSystemUuid)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            return _adapter.GetMigration(usageUuid, toSystemUuid)
                .Select(_responseMapper.MapMigration)
                .Match(Ok, FromOperationError);
        }

        [HttpPost]
        [Route("{usageUuid}/migration")]
        [SwaggerResponse(HttpStatusCode.NoContent)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult ExecuteMigration([Required][NonEmptyGuid] Guid usageUuid, [Required][NonEmptyGuid] Guid toSystemUuid)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            return _adapter.ExecuteMigration(usageUuid, toSystemUuid)
                .Match(NoContent, FromOperationError);
        }

        [HttpGet]
        [Route("{usageUuid}/migration/permissions")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ResourcePermissionsResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult GetPermissions([Required][NonEmptyGuid] Guid usageUuid)
        {
            return Ok();
        }

        [HttpGet]
        [Route("unused")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<IdentityNamePairWithDeactivatedStatusDTO>))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult GetUnusedItSystemsBySearchAndOrganization([Required][NonEmptyGuid] Guid organizationUuid,
            string nameContent,
            int numberOfItSystems,
            bool getPublicFromOtherOrganizations)
        {
            if(!ModelState.IsValid)
                return BadRequest();

            return _adapter.GetUnusedItSystemsByOrganization(organizationUuid, nameContent, numberOfItSystems, getPublicFromOtherOrganizations)
                .Select(x=> x.Select(system => system.MapIdentityNamePairWithDeactivatedStatusDTO()))
                .Match(Ok, FromOperationError);
        }
    }
}