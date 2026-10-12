using System;
using System.Collections.Generic;
using Core.ApplicationServices.OptionTypes;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.Request.Generic.Queries;
using Presentation.Web.Models.API.V2.Response.Options;
using Microsoft.AspNetCore.Mvc;

using System.Net;
namespace Presentation.Web.Controllers.API.V2.External.ItSystemUsages
{
    [Route("api/v2/it-system-usage-technical-system-types")]
    public class ItSystemUsageTechnicalSystemTypeV2Controller : BaseRegularOptionTypeV2Controller<ItSystemUsage, TechnicalSystemType>
    {
        public ItSystemUsageTechnicalSystemTypeV2Controller(IOptionsApplicationService<ItSystemUsage, TechnicalSystemType> optionService)
            : base(optionService)
        {
        }

        /// <summary>
        /// Returns It-System Usage technical system type options which are available for new registrations within the organization
        /// </summary>
        /// <param name="organizationUuid">organization context for the technical system type availability</param>
        /// <param name="pagination">pagination parameters for the query</param>
        /// <returns>A list of available It-System Usage technical system types</returns>
        [HttpGet]
        [Route("")]
        [ApiResponse(typeof(IEnumerable<RegularOptionResponseDTO>), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        [ApiResponse(HttpStatusCode.NotFound)]
        public IActionResult Get([NonEmptyGuid] Guid organizationUuid, [FromQuery] UnboundedPaginationQuery? pagination = null)
        {
            return GetAll(organizationUuid, pagination);
        }

        /// <summary>
        /// Returns requested It-System Usage technical system type
        /// </summary>
        /// <param name="technicalSystemTypeUuid">Technical system type identifier</param>
        /// <param name="organizationUuid">organization context for the technical system type availability</param>
        /// <returns>A uuid and name pair with boolean to mark if the technical system type is available in the organization</returns>
        [HttpGet]
        [Route("{technicalSystemTypeUuid}")]
        [ApiResponse(typeof(IEnumerable<RegularOptionResponseDTO>), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        [ApiResponse(HttpStatusCode.NotFound)]
        public IActionResult Get([NonEmptyGuid] Guid technicalSystemTypeUuid, [NonEmptyGuid] Guid organizationUuid)
        {
            return GetSingle(technicalSystemTypeUuid, organizationUuid);
        }
    }
}
