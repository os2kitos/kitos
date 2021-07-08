using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using Core.ApplicationServices.OptionTypes;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.External.V2.Request;
using Presentation.Web.Models.External.V2.Response;
using Presentation.Web.Models.External.V2.Response.Options;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.External.V2.ItSystemUsages
{
    [RoutePrefix("api/v2/it-system-usage-registered-data-category-types")]
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
        /// <returns>A list of available types</returns>
        [HttpGet]
        [Route("")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<IdentityNamePairResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult Get([NonEmptyGuid] Guid organizationUuid, [FromUri] UnboundedPaginationQuery pagination = null)
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
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(RegularOptionExtendedResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult Get([NonEmptyGuid] Guid registeredDataCatagoryTypeUuid, [NonEmptyGuid] Guid organizationUuid)
        {
            return GetSingle(registeredDataCatagoryTypeUuid, organizationUuid);
        }
    }
}