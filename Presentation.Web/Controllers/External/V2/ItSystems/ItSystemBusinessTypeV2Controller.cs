using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using Core.ApplicationServices.OptionTypes;
using Core.DomainModel.ItSystem;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.External.V2.Request;
using Presentation.Web.Models.External.V2.Response;
using Presentation.Web.Models.External.V2.Response.Options;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.External.V2.ItSystems
{
    [RoutePrefix("api/v2/business-types")]
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
        /// <returns>A list of available IT-System business type specifics formatted as uuid and name pairs</returns>
        [HttpGet]
        [Route("")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<IdentityNamePairResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult GetBusinessTypes([NonEmptyGuid] Guid organizationUuid, [FromUri] UnboundedPaginationQuery pagination = null)
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
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(RegularOptionExtendedResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult GetBusinessType([NonEmptyGuid] Guid businessTypeUuid, [NonEmptyGuid] Guid organizationUuid)
        {
            return GetSingle(businessTypeUuid, organizationUuid);
        }
    }
}