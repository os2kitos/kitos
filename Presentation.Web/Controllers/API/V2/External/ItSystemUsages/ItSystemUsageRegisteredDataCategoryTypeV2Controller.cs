using System;
using System.Collections.Generic;
using System.Net;
using Core.ApplicationServices.OptionTypes;
using Core.DomainModel.ItSystemUsage;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.Request.Generic.Queries;
using Presentation.Web.Models.API.V2.Response.Options;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Web.Controllers.API.V2.External.ItSystemUsages
{
    [Route("api/v2/it-system-usage-registered-data-category-types")]
    public class ItSystemUsageRegisteredDataCategoryTypeV2Controller : BaseRegularOptionTypeV2Controller<ItSystemUsage, RegisterType>
    {
        public ItSystemUsageRegisteredDataCategoryTypeV2Controller(IOptionsApplicationService<ItSystemUsage, RegisterType> optionService)
            : base(optionService)
        {
        }

        /// <summary>
        /// Returns It-System Usage registered data category types which are available for new registrations within the organization
        /// </summary>
        /// <param name="organizationUuid">organization context for the type availability</param>
        /// <param name="pagination">Optional pagination query</param>
        /// <returns>A list of available types</returns>
        [HttpGet]
        [Route("")]
        [ApiResponse(typeof(IEnumerable<RegularOptionResponseDTO>), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.NotFound)]
        public IActionResult Get([NonEmptyGuid] Guid organizationUuid, [FromQuery] UnboundedPaginationQuery? pagination = null)
        {
            return GetAll(organizationUuid, pagination);
        }

        /// <summary>
        /// Returns requested It-System Usage registered data category type
        /// </summary>
        /// <param name="registeredDataCatagoryTypeUuid">register type identifier</param>
        /// <param name="organizationUuid">organization context for the type availability</param>
        /// <returns>A uuid and name pair with boolean to mark if the type is available in the organization</returns>
        [HttpGet]
        [Route("{registeredDataCatagoryTypeUuid}")]
        [ApiResponse(typeof(RegularOptionExtendedResponseDTO), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        [ApiResponse(HttpStatusCode.NotFound)]
        public IActionResult Get([NonEmptyGuid] Guid registeredDataCatagoryTypeUuid, [NonEmptyGuid] Guid organizationUuid)
        {
            return GetSingle(registeredDataCatagoryTypeUuid, organizationUuid);
        }
    }
}

