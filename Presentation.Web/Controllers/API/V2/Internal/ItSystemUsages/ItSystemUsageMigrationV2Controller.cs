using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using Core.ApplicationServices.SystemUsage.Migration;
using Core.DomainModel.ItSystem;
using Core.DomainServices.Queries;
using Presentation.Web.Controllers.API.V2.Common.Mapping;
using Presentation.Web.Controllers.API.V2.Internal.ItSystemUsages.Mapping;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.Internal.Response.ItSystemUsage;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Web.Controllers.API.V2.Internal.ItSystemUsages
{
    [Route("api/v2/internal/it-system-usages")]
    public class ItSystemUsageMigrationV2Controller : InternalApiV2Controller
    {
        private readonly IItSystemUsageMigrationResponseMapper _responseMapper;
        private readonly IEntityWithDeactivatedStatusMapper _entityWithDeactivatedStatusMapper;
        private readonly IItSystemUsageMigrationServiceAdapter _adapter;

        public ItSystemUsageMigrationV2Controller(IItSystemUsageMigrationResponseMapper responseMapper,
            IEntityWithDeactivatedStatusMapper entityWithDeactivatedStatusMapper,
            IItSystemUsageMigrationServiceAdapter adapter)
        {
            _responseMapper = responseMapper;
            _entityWithDeactivatedStatusMapper = entityWithDeactivatedStatusMapper;
            _adapter = adapter;
        }

        /// <summary>
        /// Gets the migration description if a system usage is migrated to another master it-system resource
        /// </summary>
        /// <param name="usageUuid">uuid of system usage being migrated</param>
        /// <param name="toSystemUuid">uuid of the master it-system to migrate to</param>
        /// <returns>A description of the consequences of the migration</returns>
        [HttpGet]
        [Route("{usageUuid}/migration")]
        [ApiResponse(typeof(ItSystemUsageMigrationV2ResponseDTO), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.NotFound)]
        public IActionResult Get([Required][NonEmptyGuid] Guid usageUuid, [Required][NonEmptyGuid] Guid toSystemUuid)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return _adapter.GetMigration(usageUuid, toSystemUuid)
                .Select(_responseMapper.MapMigration)
                .Match(Ok, FromOperationError);
        }

        /// <summary>
        /// Perform a migration of the it-system-usage to from the current it-system master data to a new it-system master data
        /// </summary>
        /// <param name="usageUuid"></param>
        /// <param name="toSystemUuid"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("{usageUuid}/migration")]
        [ApiResponse(HttpStatusCode.NoContent)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.NotFound)]
        public IActionResult ExecuteMigration([Required][NonEmptyGuid] Guid usageUuid, [Required][NonEmptyGuid] Guid toSystemUuid)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return _adapter.ExecuteMigration(usageUuid, toSystemUuid)
                .Match(NoContent, FromOperationError);
        }

        /// <summary>
        /// Gets the migration permissions of the authenticated user
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("migration/permissions")]
        [ApiResponse(typeof(ItSystemUsageMigrationPermissionsResponseDTO), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.NotFound)]
        public IActionResult GetPermissions()
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var permissions = _adapter.GetCommandPermissions();
            return Ok(_responseMapper.MapCommandPermissions(permissions));
        }

        /// <summary>
        /// Search for systems which are not in use and which are valid migration targets
        /// </summary>
        /// <param name="organizationUuid"></param>
        /// <param name="numberOfItSystems"></param>
        /// <param name="nameContent"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("migration/unused-it-systems")]
        [ApiResponse(typeof(IEnumerable<IdentityNamePairWithDeactivatedStatusDTO>), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.NotFound)]
        public IActionResult GetUnusedItSystemsBySearchAndOrganization(
            [Required][NonEmptyGuid] Guid organizationUuid,
            [Required] int numberOfItSystems,
            string? nameContent = null)
        {
            if(!ModelState.IsValid)
                return BadRequest(ModelState);

            var conditions = new List<IDomainQuery<ItSystem>>();

            if (nameContent != null)
                conditions.Add(new QueryByPartOfName<ItSystem>(nameContent));

            return _adapter.GetUnusedItSystemsByOrganization(organizationUuid, numberOfItSystems, conditions.ToArray())
                .Select(_entityWithDeactivatedStatusMapper.Map)
                .Match(Ok, FromOperationError);
        }
    }
}


