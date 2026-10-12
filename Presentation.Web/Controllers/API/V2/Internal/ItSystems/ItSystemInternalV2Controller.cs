using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Core.Abstractions.Extensions;
using Core.ApplicationServices.System;
using Core.DomainModel.ItSystem;
using Presentation.Web.Controllers.API.V2.Common.Helpers;
using Presentation.Web.Controllers.API.V2.Common.Mapping;
using Presentation.Web.Controllers.API.V2.External.Generic;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.Internal.Response.ItSystem;
using Presentation.Web.Models.API.V2.Request.Generic.Queries;
using Presentation.Web.Models.API.V2.Response.System;
using Presentation.Web.Models.API.V2.Types.Shared;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Web.Controllers.API.V2.Internal.ItSystems
{
    /// <summary>
    /// Internal API for the system product master data (not local organization usage registrations) stored in KITOS.
    /// </summary>
    [Route("api/v2/internal")]
    public class ItSystemInternalV2Controller : InternalApiV2Controller
    {
        private readonly IItSystemService _itSystemService;

        public ItSystemInternalV2Controller(IItSystemService itSystemService)
        {
            _itSystemService = itSystemService;
        }

        /// <summary>
        /// Shallow search endpoint returning all IT-Systems available to the current user
        /// </summary>
        /// <param name="rightsHolderUuid">Rightsholder UUID filter</param>
        /// <param name="businessTypeUuid">Business type UUID filter</param>
        /// <param name="kleNumber">KLE number filter ("NN.NN.NN" format)</param>
        /// <param name="kleUuid">KLE UUID number filter</param>
        /// <param name="numberOfUsers">Greater than or equal to number of users filter</param>
        /// <param name="includeDeactivated">If set to true, the response will also include deactivated it-systems</param>
        /// <param name="changedSinceGtEq">Include only changes which were LastModified (UTC) is equal to or greater than the provided value</param>
        /// <param name="nameEquals">Include only systems with a name equal to the parameter</param>
        /// <param name="nameContains">Include only systems with a name that contains the content in the parameter</param>
        /// <param name="orderByProperty">Ordering property</param>
        /// <param name="paginationQuery">Pagination query parameters</param>
        /// <param name="excludeUuid">UUID of the system to exclude from the results</param>
        /// <param name="excludeChildrenOfUuid">UUID of the parent system whose children should be excluded from the results</param>
        /// <returns></returns>
        [HttpGet]
        [Route("it-systems/search")]
        [ApiResponse(typeof(IEnumerable<ItSystemSearchResponseDTO>), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        public IActionResult GetItSystems(
            [NonEmptyGuid] Guid? rightsHolderUuid = null,
            [NonEmptyGuid] Guid? businessTypeUuid = null,
            string? kleNumber = null,
            [NonEmptyGuid] Guid? kleUuid = null,
            int? numberOfUsers = null,
            bool? includeDeactivated = null,
            DateTime? changedSinceGtEq = null,
            string? nameEquals = null,
            string? nameContains = null,
            CommonOrderByProperty? orderByProperty = null,
            [FromQuery] BoundedPaginationQuery? paginationQuery = null,
            [NonEmptyGuid] Guid? excludeUuid = null,
            [NonEmptyGuid] Guid? excludeChildrenOfUuid = null)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return _itSystemService
                .ExecuteItSystemsQuery(rightsHolderUuid, businessTypeUuid, kleNumber, kleUuid, numberOfUsers, includeDeactivated, changedSinceGtEq, nameEquals: nameEquals, nameContains: nameContains, orderByProperty: orderByProperty, paginationQuery: paginationQuery, excludeUuid: excludeUuid, excludeChildrenOfUuid: excludeChildrenOfUuid)
                .Select(Map)
                .Transform(Ok);
        }

        [HttpGet]
        [Route("organization/{organizationUuid}/it-systems/{systemUuid}/hierarchy")]
        [ApiResponse(typeof(IEnumerable<ItSystemHierarchyNodeResponseDTO>), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        [ApiResponse(HttpStatusCode.NotFound)]
        public IActionResult GetHierarchy([NonEmptyGuid] Guid systemUuid, [NonEmptyGuid] Guid organizationUuid)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return _itSystemService.GetCompleteHierarchyByUuid(systemUuid)
                .Select(systems => RegistrationHierarchyNodeMapper.MapSystemHierarchyToDtos(systems, organizationUuid))
                .Match(Ok, FromOperationError);
        }

        private static ItSystemSearchResponseDTO Map(ItSystem arg)
        {
            return arg
                .MapIdentityNamePairDTO()
                .Transform(x => new ItSystemSearchResponseDTO(x.Uuid, x.Name, arg.Disabled));
        }
    }
}

