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
    [Route("api/v2/data-processing-registration-data-responsible-types")]
    public class DataProcessingRegistrationDataResponsibleTypeV2Controller : BaseRegularOptionTypeV2Controller<DataProcessingRegistration, DataProcessingDataResponsibleOption>
    {
        public DataProcessingRegistrationDataResponsibleTypeV2Controller(IOptionsApplicationService<DataProcessingRegistration, DataProcessingDataResponsibleOption> optionService)
            : base(optionService)
        {
        }

        /// <summary>
        /// Returns Data Processing Registration data responsible options which are available for new registrations within the organization
        /// </summary>
        /// <param name="organizationUuid">organization context for the data responsible availability</param>
        /// <param name="pagination">Optional pagination query</param>
        /// <returns>A list of available Data Processing Registration data responsible</returns>
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
        /// Returns requested Data Processing Registration data responsible
        /// </summary>
        /// <param name="dataResponsibleUuid">data responsible identifier</param>
        /// <param name="organizationUuid">organization context for the data responsible availability</param>
        /// <returns>A uuid and name pair with boolean to mark if the data responsible is available in the organization</returns>
        [HttpGet]
        [Route("{dataResponsibleUuid}")]
        [ApiResponse(typeof(RegularOptionExtendedResponseDTO), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        [ApiResponse(HttpStatusCode.NotFound)]
        public IActionResult Get([NonEmptyGuid] Guid dataResponsibleUuid, [NonEmptyGuid] Guid organizationUuid)
        {
            return GetSingle(dataResponsibleUuid, organizationUuid);
        }
    }
}

