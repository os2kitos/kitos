using System;
using System.Collections.Generic;
using System.Net;
using Core.ApplicationServices.OptionTypes;
using Core.DomainModel.ItSystem;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.Request.Generic.Queries;
using Presentation.Web.Models.API.V2.Response.Options;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Web.Controllers.API.V2.External.ItSystems
{
    [Route("api/v2/business-types")]
    [AllowRightsHoldersAccess]
    public class ItSystemBusinessTypeV2Controller: BaseRegularOptionTypeV2Controller<ItSystem,BusinessType>
    {
        public ItSystemBusinessTypeV2Controller(IOptionsApplicationService<ItSystem, BusinessType> optionService)
            : base(optionService)
        {
        }

        /// <summary>
        /// Returns IT-System business types which are available for new registrations within the organization
        /// </summary>
        /// <param name="organizationUuid">organization context for the business type availability</param>
        /// <param name="pagination">Optional pagination query</param>
        /// <returns>A list of available IT-System business type specifics formatted as uuid and name pairs</returns>
        [HttpGet]
        [Route("")]
        [ApiResponse(typeof(IEnumerable<RegularOptionResponseDTO>), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.NotFound)]
        public IActionResult GetBusinessTypes([NonEmptyGuid] Guid organizationUuid, [FromQuery] UnboundedPaginationQuery? pagination = null)
        {
            return GetAll(organizationUuid, pagination);
        }

        /// <summary>
        /// Returns requested IT-System business type
        /// </summary>
        /// <param name="businessTypeUuid">business type identifier</param>
        /// <param name="organizationUuid">organization context for the business type availability</param>
        /// <returns>A uuid and name pair with boolean to mark if the business type is available in the organization</returns>
        [HttpGet]
        [Route("{businessTypeUuid}")]
        [ApiResponse(typeof(RegularOptionExtendedResponseDTO), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        [ApiResponse(HttpStatusCode.NotFound)]
        public IActionResult GetBusinessType([NonEmptyGuid] Guid businessTypeUuid, [NonEmptyGuid] Guid organizationUuid)
        {
            return GetSingle(businessTypeUuid, organizationUuid);
        }
    }
}

