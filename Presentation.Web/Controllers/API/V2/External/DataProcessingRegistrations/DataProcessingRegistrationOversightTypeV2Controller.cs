using System;
using System.Collections.Generic;
using System.Net;
using Core.ApplicationServices.OptionTypes;
using Core.DomainModel.GDPR;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.Request.Generic.Queries;
using Presentation.Web.Models.API.V2.Response.Options;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Web.Controllers.API.V2.External.DataProcessingRegistrations
{
    [Route("api/v2/data-processing-registration-oversight-types")]
    public class DataProcessingRegistrationOversightTypeV2Controller : BaseRegularOptionTypeV2Controller<DataProcessingRegistration, DataProcessingOversightOption>
    {
        public DataProcessingRegistrationOversightTypeV2Controller(IOptionsApplicationService<DataProcessingRegistration, DataProcessingOversightOption> optionService)
            : base(optionService)
        {
        }

        /// <summary>
        /// Returns Data Processing Registration oversight options which are available for new registrations within the organization
        /// </summary>
        /// <param name="organizationUuid">organization context for the oversight availability</param>
        /// <param name="pagination">Optional pagination query</param>
        /// <returns>A list of available Data Processing Registration oversight</returns>
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
        /// Returns requested Data Processing Registration oversight
        /// </summary>
        /// <param name="oversightUuid">oversight identifier</param>
        /// <param name="organizationUuid">organization context for the oversight availability</param>
        /// <returns>A uuid and name pair with boolean to mark if the oversight is available in the organization</returns>
        [HttpGet]
        [Route("{oversightUuid}")]
        [ApiResponse(typeof(RegularOptionExtendedResponseDTO), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        [ApiResponse(HttpStatusCode.NotFound)]
        public IActionResult Get([NonEmptyGuid] Guid oversightUuid, [NonEmptyGuid] Guid organizationUuid)
        {
            return GetSingle(oversightUuid, organizationUuid);
        }
    }
}

