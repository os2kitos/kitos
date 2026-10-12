using System;
using System.Collections.Generic;
using System.Net;
using Core.ApplicationServices.OptionTypes;
using Core.DomainModel.ItSystem;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.Request.Generic.Queries;
using Presentation.Web.Models.API.V2.Response.Options;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Web.Controllers.API.V2.External.ItSystemUsages
{
    [Route("api/v2/it-system-usage-sensitive-personal-data-types")]
    public class ItSystemUsageSensitivePersonalDataTypeV2Controller : BaseRegularOptionTypeV2Controller<ItSystem, SensitivePersonalDataType>
    {
        public ItSystemUsageSensitivePersonalDataTypeV2Controller(IOptionsApplicationService<ItSystem, SensitivePersonalDataType> optionService)
            : base(optionService)
        {
        }

        /// <summary>
        /// Returns It-System usage sensitive personal data types which are available for new registrations within the organization
        /// </summary>
        /// <param name="organizationUuid">organization context for the sensitive personal data types availability</param>
        /// <param name="pagination">pagination parameters for the query</param>
        /// <returns>A list of available It-System usage sensitive personal data types</returns>
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
        /// Returns requested It-System usage sensitive personal data type
        /// </summary>
        /// <param name="sensitivePersonalDataTypeUuid">sensitive personal data type identifier</param>
        /// <param name="organizationUuid">organization context for the sensitive personal data type availability</param>
        /// <returns>A uuid and name pair with boolean to mark if the sensitive personal data type is available in the organization</returns>
        [HttpGet]
        [Route("{sensitivePersonalDataTypeUuid}")]
        [ApiResponse(typeof(RegularOptionExtendedResponseDTO), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        [ApiResponse(HttpStatusCode.NotFound)]
        public IActionResult Get([NonEmptyGuid] Guid sensitivePersonalDataTypeUuid, [NonEmptyGuid] Guid organizationUuid)
        {
            return GetSingle(sensitivePersonalDataTypeUuid, organizationUuid);
        }
    }
}


