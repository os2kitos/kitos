using System;
using System.Collections.Generic;
using System.Net;
using Core.ApplicationServices.OptionTypes;
using Core.DomainModel.ItContract;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.Request.Generic.Queries;
using Presentation.Web.Models.API.V2.Response.Options;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Web.Controllers.API.V2.External.ItContracts
{
    [Route("api/v2/it-contract-criticality-types")]
    public class ItContractCriticalityTypeV2Controller : BaseRegularOptionTypeV2Controller<ItContract, CriticalityType>
    {
        public ItContractCriticalityTypeV2Controller(IOptionsApplicationService<ItContract, CriticalityType> optionService)
            : base(optionService)
        {
        }

        /// <summary>
        /// Returns It-Contract criticality type options 
        /// </summary>
        /// <param name="organizationUuid">organization context for the criticality types availability</param>
        /// <param name="pagination">Optional pagination query</param>
        /// <returns>A list of available It-Contract criticality types</returns>
        [HttpGet]
        [Route("")]
        [ApiResponse(typeof(IEnumerable<RegularOptionResponseDTO>), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.NotFound)]
        public IActionResult Get([NonEmptyGuid] Guid organizationUuid, [FromQuery] UnboundedPaginationQuery? pagination = null)
        {
            return GetAll(organizationUuid, pagination);
        }

        /// <summary>
        /// Returns requested It-Contract criticality type
        /// </summary>
        /// <param name="criticalityTypeUuid">criticality type identifier</param>
        /// <param name="organizationUuid">organization context for the criticality type availability</param>
        /// <returns>A uuid and name pair with boolean to mark if the criticality type is available in the organization</returns>
        [HttpGet]
        [Route("{criticalityTypeUuid}")]
        [ApiResponse(typeof(RegularOptionExtendedResponseDTO), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        [ApiResponse(HttpStatusCode.NotFound)]
        public IActionResult Get([NonEmptyGuid] Guid criticalityTypeUuid, [NonEmptyGuid] Guid organizationUuid)
        {
            return GetSingle(criticalityTypeUuid, organizationUuid);
        }
    }
}

