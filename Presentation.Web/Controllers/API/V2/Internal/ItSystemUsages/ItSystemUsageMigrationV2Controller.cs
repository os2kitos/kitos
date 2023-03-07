using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Web.Http;
using Core.ApplicationServices.SystemUsage.Migration;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices.Queries;
using Core.DomainServices.Queries.SystemUsage;
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
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ItSystemUsageMigrationPermissionsResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult GetPermissions([Required][NonEmptyGuid] Guid usageUuid)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            return _adapter.GetCommandPermission(usageUuid)
                .Select(_responseMapper.MapCommandPermissions)
                .Match(Ok, FromOperationError);
        }

        [HttpGet]
        [Route("unused")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<IdentityNamePairWithDeactivatedStatusDTO>))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult GetUnusedItSystemsBySearchAndOrganization([Required][NonEmptyGuid] Guid organizationUuid,
            [Required] int numberOfItSystems,
            [Required] bool getPublicFromOtherOrganizations,
            string nameContent = null)
        {
            if(!ModelState.IsValid)
                return BadRequest();

            var conditions = new List<IDomainQuery<ItSystem>>();

            if (nameContent != null)
                conditions.Add(new QueryByPartOfName<ItSystem>(nameContent));

            return _adapter.GetUnusedItSystemsByOrganization(organizationUuid, numberOfItSystems, getPublicFromOtherOrganizations, conditions.ToArray())
                .Select(x=> x.Select(system => system.MapIdentityNamePairWithDeactivatedStatusDTO()))
                .Match(Ok, FromOperationError);
        }
    }
}