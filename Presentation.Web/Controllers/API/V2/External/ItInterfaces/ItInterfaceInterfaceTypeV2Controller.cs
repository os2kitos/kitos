using System;
using System.Collections.Generic;
using System.Net;
using Core.ApplicationServices.OptionTypes;
using Core.DomainModel.ItSystem;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.Request.Generic.Queries;
using Presentation.Web.Models.API.V2.Response.Options;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Web.Controllers.API.V2.External.ItInterfaces
{
    [Route("api/v2/it-interface-interface-types")]
    public class ItInterfaceInterfaceTypeV2Controller : BaseRegularOptionTypeV2Controller<ItInterface, InterfaceType>
    {
        public ItInterfaceInterfaceTypeV2Controller(IOptionsApplicationService<ItInterface, InterfaceType> optionService)
            : base(optionService)
        {
        }

        /// <summary>
        /// Returns IT-Interface 'interface-type' options which are available for new registrations within the organization
        /// </summary>
        /// <param name="organizationUuid">organization context for the archive locations availability</param>
        /// <param name="pagination">Optional pagination query</param>
        /// <returns>IT-Interface 'interface-type'</returns>
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
        /// Returns requested IT-Interface 'interface-type'
        /// </summary>
        /// <param name="itInterfaceTypeUuid">archive location identifier</param>
        /// <param name="organizationUuid">organization context for the archive location availability</param>
        /// <returns>A uuid and name pair with boolean to mark if option is available in the organization</returns>
        [HttpGet]
        [Route("{itInterfaceTypeUuid}")]
        [ApiResponse(typeof(RegularOptionExtendedResponseDTO), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        [ApiResponse(HttpStatusCode.NotFound)]
        public IActionResult Get([NonEmptyGuid] Guid itInterfaceTypeUuid, [NonEmptyGuid] Guid organizationUuid)
        {
            return GetSingle(itInterfaceTypeUuid, organizationUuid);
        }
    }
}

